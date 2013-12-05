using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ForInStatement : Statement, IOptimizable
    {
        private Statement init;
        private string name;
        private Statement body;

        private ForInStatement()
        {
        }

        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "for(", ref i) && (!Parser.Validate(code, "for (", ref i)))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            return new ParseResult() { Message = "not implemented" };
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ForInStatement()
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            throw new NotImplementedException();
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref init, depth + 1, varibles);
            Parser.Optimize(ref body, depth + 1, varibles);
            return false;
        }
    }
}
