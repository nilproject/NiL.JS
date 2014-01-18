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

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "if (", ref i) && !Parser.Validate(code, "if(", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            Statement condition = OperatorStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            Statement body = Parser.Parse(state, ref i, 0);
            Statement elseBody = null;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!(body is CodeBlock) && (code[i] == ';'))
            {
                i++;
                while (char.IsWhiteSpace(code[i])) i++;
            }
            if (Parser.Validate(code, "else", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                elseBody = Parser.Parse(state, ref i, 0);
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

        public override JSObject Invoke(Context context)
        {
            if ((bool)condition.Invoke(context))
                return body.Invoke(context);
            else if (elseBody != null)
                return elseBody.Invoke(context);
            return null;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            depth++;
            Parser.Optimize(ref body, 1, varibles);
            Parser.Optimize(ref condition, 1, varibles);
            Parser.Optimize(ref elseBody, 1, varibles);
            return false;
        }
    }
}