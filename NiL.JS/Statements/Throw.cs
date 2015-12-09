using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Throw : CodeNode
    {
        private Expression body;

        public Throw(Exception e)
        {
            body = new Constant(TypeProxy.Marshal(e));
        }

        internal Throw(Expression statement)
        {
            body = statement;
        }

        public CodeNode Body { get { return body; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "throw", ref i) || (!Parser.IsIdentificatorTerminator(state.Code[i])))
                return null;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]) && !Tools.IsLineTerminator(state.Code[i]))
                i++;
            var b = state.Code[i] == ';' || Tools.IsLineTerminator(state.Code[i]) ? null : (Expression)Parser.Parse(state, ref i, CodeFragmentType.Expression);
            if (b is Empty)
                ExceptionsHelper.Throw((new SyntaxError("Can't throw result of EmptyStatement " + CodeCoordinates.FromTextPosition(state.Code, i - 1, 0))));
            var pos = index;
            index = i;
            return new Throw(b)
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var value = body == null ? JSValue.undefined : body.Evaluate(context);
            if (context.abortReason == AbortReason.Suspend)
                return null;

            ExceptionsHelper.Throw(value);
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref body, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (body != null)
                body.Optimize(ref body, owner, message, opts, stats);
        }

        public override void Decompose(ref CodeNode self)
        {
            if (body != null)
                body.Decompose(ref body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            body?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}