using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

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

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "return", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            if (state.AllowReturn == 0)
                ExceptionsHelper.Throw(new SyntaxError("Invalid use of return statement."));
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i]))
                i++;
            var body = state.Code[i] == ';' || Tools.isLineTerminator(state.Code[i]) ? null : Parser.Parse(state, ref i, CodeFragmentType.Expression);
            var pos = index;
            index = i;
            return new ReturnStatement()
                {
                    body = (Expressions.Expression)body,
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            context.abortInfo = body != null ? body.Evaluate(context) : null;
            if (context.abort < AbortType.Return)
                context.abort = AbortType.Return;
            return JSValue.notExists;
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (body != null)
                return new CodeNode[] { body };
            return new CodeNode[0];
        }

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref body, depth + 1, variables, state | BuildState.InExpression, message, statistic, opts);
            // Улучшает работу оптимизатора хвостовой рекурсии
            if (message == null && body is NiL.JS.Expressions.ConditionalOperator)
            {
                var bat = body as NiL.JS.Expressions.ConditionalOperator;
                var bts = bat.Threads;
                _this = new IfElseStatement(bat.FirstOperand, new ReturnStatement(bts[0]), new ReturnStatement(bts[1])) { Position = bat.Position, Length = bat.Length } as T;
                return true;
            }
            else if (body is NiL.JS.Expressions.CallOperator)
                (body as NiL.JS.Expressions.CallOperator).allowTCO = true;

            statistic.Returns.Add(body ?? EmptyExpression.Instance);

            return false;
        }

        internal protected override void Optimize<T>(ref T _this, Expressions.FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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