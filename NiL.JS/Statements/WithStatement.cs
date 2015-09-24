using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class WithStatement : CodeNode
    {
        private CodeNode obj;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Scope { get { return obj; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "with (", ref i) && !Parser.Validate(state.Code, "with(", ref i))
                return new ParseResult();
            if (state.strict)
                throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("WithStatement is not allowed in strict mode.")));
            if (state.message != null)
                state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, index, 4), "Do not use \"with\".");
            var obj = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Invalid syntax WithStatement.")));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionNotation)
            {
                if (state.strict)
                    throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }, state.strict); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new WithStatement()
                {
                    obj = obj,
                    body = body,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSValue Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(obj);
#endif
            var intcontext = new WithContext(obj.Evaluate(context), context);
#if DEV
            if (context.debugging && !(body is CodeBlock))
                context.raiseDebugger(body);
#endif
            try
            {
                intcontext.Activate();
                context.lastResult = body.Evaluate(intcontext) ?? intcontext.lastResult ?? context.lastResult;
                context.abort = intcontext.abort;
                context.abortInfo = intcontext.abortInfo;
            }
            finally
            {
                intcontext.Deactivate();
            }
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                obj
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (statistic != null)
                statistic.ContainsWith = true;
            Parser.Build(ref obj, depth + 1, variables, state | _BuildState.InExpression, message, statistic, opts);
            Parser.Build(ref body, depth, variables, state | _BuildState.InWith, message, statistic, opts);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (obj != null)
                obj.Optimize(ref obj, owner, message, opts, statistic);
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override string ToString()
        {
            return "with (" + obj + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
