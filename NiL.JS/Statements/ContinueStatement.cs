using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    class ContinueStatement : Statement
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "continue", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.AllowContinue <= 0)
                throw new ArgumentException();
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ContinueStatement()
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = null;
            return JSObject.undefined;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}