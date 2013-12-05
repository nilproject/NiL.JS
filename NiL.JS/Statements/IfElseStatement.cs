using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class IfElseStatement : Statement, IOptimizable
    {
        private Statement condition;
        private Statement body;
        private Statement elseBody;

        private IfElseStatement()
        {
        }

        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            if (!Parser.Validate(code, "if (", ref i) && !Parser.Validate(code, "if(", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            Statement condition = OperatorStatement.Parse(code, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            Statement body = Parser.Parse(code, ref i, 0);
            Statement elseBody = null;
            while (char.IsWhiteSpace(code[i])) i++;
            if (Parser.Validate(code, "else", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                elseBody = Parser.Parse(code, ref i, 0);
            }
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new IfElseStatement()
                {
                    body = body,
                    condition = condition,
                    elseBody = elseBody
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            if (condition.Invoke(context))
                return body.Invoke(context);
            else if (elseBody != null)
                return elseBody.Invoke(context);
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            depth++;
            Parser.Optimize(ref body, depth, varibles);
            Parser.Optimize(ref condition, depth, varibles);
            Parser.Optimize(ref elseBody, depth, varibles);
            return false;
        }
    }
}
