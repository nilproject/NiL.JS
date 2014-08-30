using System;
using System.Linq.Expressions;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ContinueStatement : CodeNode
    {
        private JSObject label;

        public JSObject Label { get { return label; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "continue", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            if (state.AllowContinue <= 0)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Invalid use continue statement")));
            while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
            {
                label = Tools.Unescape(state.Code.Substring(sl, i - sl), state.strict.Peek());
                if (!state.Labels.Contains(label.oValue as string))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Try to continue to undefined label.")));
            }
            int pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ContinueStatement()
                {
                    label = label,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override System.Linq.Expressions.Expression BuildTree(Core.JIT.TreeBuildingState state)
        {
            if (label != null)
                return Expression.Continue(state.NamedContinueLabels[label.ToString()]);
            return Expression.Continue(state.ContinueLabels.Peek());
        }

        internal override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = label;
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return "continue" + (label != null ? " " + label : "");
        }
    }
}