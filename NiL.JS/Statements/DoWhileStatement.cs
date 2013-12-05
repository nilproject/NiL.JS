using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class DoWhileStatement : Statement, IOptimizable
    {
        private Statement condition;
        private Statement body;

        private DoWhileStatement()
        {
        }

        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "do", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            var body = Parser.Parse(code, ref i, 0);
            do i++; while (char.IsWhiteSpace(code[i]));
            if (!Parser.Validate(code, "while", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '(')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = Parser.Parse(code, ref i, 0);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            i++;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new DoWhileStatement()
                {
                    body = body,
                    condition = condition,
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            do
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
            while (condition.Invoke(context));
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref body, varibles);
            Parser.Optimize(ref condition, varibles);
            return false;
        }
    }
}
