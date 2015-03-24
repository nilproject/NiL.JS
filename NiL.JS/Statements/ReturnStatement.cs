using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ReturnStatement : CodeNode
    {
        private Expressions.Expression body;

        public Expressions.Expression Body { get { return body; } }

        internal ReturnStatement()
        {

        }

        internal ReturnStatement(Expressions.Expression body)
        {
            this.body = body;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "return", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            if (state.AllowReturn == 0)
                throw new JSException((new SyntaxError("Invalid use of return statement.")));
            var body = Parser.Parse(state, ref i, 1, true);
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ReturnStatement()
                {
                    body = (Expressions.Expression)body,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            context.abortInfo = body != null ? body.Evaluate(context) : JSObject.undefined;
            if (context.abort < AbortType.Return)
                context.abort = AbortType.Return;
            //if (body is VariableReference)
            //    context.abortInfo = context.abortInfo.CloneImpl();
            return JSObject.notExists;
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (body != null)
                return new[] { body };
            return new CodeNode[0];
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref body, 2, variables, state, message, statistic, opts);
            // Улучшает работу оптимизатора хвостовой рекурсии
            if (message == null && body is NiL.JS.Expressions.Ternary)
            {
                var bat = body as NiL.JS.Expressions.Ternary;
                var bts = bat.Threads;
                _this = new IfElseStatement(bat.FirstOperand, new ReturnStatement(bts[0]), new ReturnStatement(bts[1])) { Position = bat.Position, Length = bat.Length };
                return false;
            }
            else if (body is NiL.JS.Expressions.Call)
                (body as NiL.JS.Expressions.Call).allowTCO = true;
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (body != null)
            {
                var t = body as CodeNode;
                body.Optimize(ref t, owner, message, opts, statistic);
                body = (Expressions.Expression)t;
            }
        }
#if !PORTABLE && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var b = body.TryCompile(false, false, null, dynamicValues);
            if (b != null)
                body = new CompiledNode(body, b, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "return" + (body != null ? " " + body : "");
        }
    }
}