using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class WhileStatement : Statement
    {
        private Statement condition;
        private Statement body;

        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            if (!Parser.Validate(code, "while", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '(')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = Parser.Parse(code, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var body = Parser.Parse(code, ref i, 0);
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new WhileStatement()
                {
                    body = body,
                    condition = condition
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            while (condition.Invoke(context))
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Continue)
                        context.abort = AbortType.None;
                    else
                        return null;
                }
            }
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
