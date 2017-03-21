using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Continue : CodeNode
    {
        private JSValue label;

        public JSValue Label { get { return label; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "continue", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;
            if (!state.AllowContinue.Peek())
                ExceptionHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Invalid use of continue statement")));
            while (Tools.IsWhiteSpace(state.Code[i]) && !Tools.IsLineTerminator(state.Code[i])) i++;
            int sl = i;
            JSValue label = null;
            if (Parser.ValidateName(state.Code, ref i, state.strict))
            {
                label = Tools.Unescape(state.Code.Substring(sl, i - sl), state.strict);
                if (!state.Labels.Contains(label._oValue.ToString()))
                    ExceptionHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Try to continue to undefined label.")));
            }
            int pos = index;
            index = i;
            state.continiesCount++;
            return new Continue()
            {
                label = label,
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            context._executionMode = ExecutionMode.Continue;
            context._executionInfo = label;
            return null;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return null;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref CodeNode self)
        {

        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {

        }

        public override string ToString()
        {
            return "continue" + (label != null ? " " + label : "");
        }
    }
}