using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class WithStatement : CodeNode
    {
        private CodeNode obj;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Scope { get { return obj; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            state.containsWith.Push(state.containsWith.Pop() || true);
            int i = index;
            if (!Parser.Validate(state.Code, "with (", ref i) && !Parser.Validate(state.Code, "with(", ref i))
                return new ParseResult();
            if (state.strict.Peek())
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("WithStatement is not allowed in strict mode.")));
            if (state.message != null)
                state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, index, 4), "Do not use \"with\".");
            var obj = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("Invalid syntax WithStatement.")));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionExpression)
            {
                if (state.strict.Peek())
                    throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
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
#if !NET35
        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var intContext = System.Linq.Expressions.Expression.Parameter(typeof(WithContext));
            var tempContainer = System.Linq.Expressions.Expression.Parameter(typeof(Context));
            return System.Linq.Expressions.Expression.Block(new[] { intContext, tempContainer }
                , System.Linq.Expressions.Expression.Assign(intContext, System.Linq.Expressions.Expression.Call(JITHelpers.methodof(initContext), JITHelpers.ContextParameter, obj.CompileToIL(state)))
                , System.Linq.Expressions.Expression.TryFinally(
                    System.Linq.Expressions.Expression.Block(
                        System.Linq.Expressions.Expression.Assign(tempContainer, JITHelpers.ContextParameter)
                        , System.Linq.Expressions.Expression.Assign(JITHelpers.ContextParameter, intContext)
                        , System.Linq.Expressions.Expression.Call(intContext, typeof(WithContext).GetMethod("Activate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , body.CompileToIL(state)
                    )
                    , System.Linq.Expressions.Expression.Block(
                        System.Linq.Expressions.Expression.Assign(JITHelpers.ContextParameter, tempContainer)
                        , System.Linq.Expressions.Expression.Call(intContext, typeof(WithContext).GetMethod("Deactivate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(JITHelpers.ContextParameter, "abort"), System.Linq.Expressions.Expression.Field(intContext, "abort"))
                        , System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(JITHelpers.ContextParameter, "abortInfo"), System.Linq.Expressions.Expression.Field(intContext, "abortInfo"))
                    )
                ));
        }
#endif
        private static WithContext initContext(Context parent, JSObject obj)
        {
            return new WithContext(obj, parent);
        }

        internal override JSObject Evaluate(Context context)
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref obj, depth + 1, variables, strict, message);
            Parser.Build(ref body, depth, variables, strict, message);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            if (obj != null)
                obj.Optimize(ref obj, owner, message);
            if (body != null)
                body.Optimize(ref body, owner, message);
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
