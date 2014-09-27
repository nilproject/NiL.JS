using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class DebuggerOperator : CodeNode
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "debugger", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            int pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DebuggerOperator()
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
#if DEV
            if (!context.debugging)
                // Без этого условия обработчик остановки вызывается дважды с одним выражением.
                // Первый вызов происходит из цикла CodeBlock, второй из строки ниже.
                context.raiseDebugger(this);
#else
            context.raiseDebugger(this);
#endif
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "debugger";
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}