using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ReturnStatement : Statement
    {
        private Statement body;

        public ReturnStatement()
        {
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "return", ref i))
                throw new ArgumentException("code (" + i + ")");
            if ((code[i] != ' ') && (code[i] != ';') && (code[i] != '(') && (code[i] != '}') && (!Parser.isLineTerminator(code[i])))
                return new ParseResult() { IsParsed = false };
            while (char.IsWhiteSpace(code[i])) i++;
            if ((code[i] == ';') || (code[i] == '}') || (Parser.isLineTerminator(code[i])))
            {
                index = i;
                return new ParseResult()
                {
                    IsParsed = true,
                    Message = "",
                    Statement = new ReturnStatement()
                    {
                        body = new ImmidateValueStatement(BaseObject.undefined)
                    }
                };
            }
            var body = Parser.Parse(state, ref i, 1);
            var vrs = new System.Collections.Generic.HashSet<string>();
            Parser.Optimize(ref body, vrs);
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ReturnStatement()
                {
                    body = body,
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Return;
            context.abortInfo = body.Invoke(context);
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
