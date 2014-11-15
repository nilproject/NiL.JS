using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ReturnStatement : CodeNode
    {
#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            if (state.TryFinally <= 0)
                return Expression.Goto(state.ReturnTarget, body != null ? body.CompileToIL(state) : JITHelpers.NotExistsConstant);
            else
                return Expression.Goto(state.ReturnTarget, Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abortInfo"), body != null ? body.CompileToIL(state) : JITHelpers.NotExistsConstant));
        }

#endif

        private CodeNode body;

        public CodeNode Body { get { return body; } }

        internal ReturnStatement()
        {

        }

        internal ReturnStatement(CodeNode body)
        {
            this.body = body;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
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
                    body = body,
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
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (body != null)
                return new[] { body };
            return new CodeNode[0];
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Build(ref body, 2, variables, strict);
            if (body is NiL.JS.Expressions.Ternary)
            {
                var bat = body as NiL.JS.Expressions.Ternary;
                var bts = bat.Threads;
                _this = new IfElseStatement(bat.FirstOperand, new ReturnStatement(bts[0]), new ReturnStatement(bts[1]));
                return true;
            }
            if (body is NiL.JS.Expressions.Call)
            {
                (body as NiL.JS.Expressions.Call).allowTCO = true;
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner)
        {
            body.Optimize(ref body, owner);
        }

        public override string ToString()
        {
            return "return" + (body != null ? " " + body : "");
        }
    }
}