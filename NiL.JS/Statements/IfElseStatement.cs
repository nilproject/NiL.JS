using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class IfElseStatement : Statement
    {
        private Statement condition;
        private Statement body;
        private Statement elseBody;

        public Statement Body { get { return body; } }
        public Statement ElseBody { get { return elseBody; } }
        public Statement Condition { get { return condition; } }

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
            if (body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
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
                if (elseBody is FunctionStatement && state.strict.Peek())
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            }
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new IfElseStatement()
                {
                    body = body,
                    condition = condition,
                    elseBody = elseBody,
                    Position = pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            if ((bool)condition.Invoke(context))
                return body.Invoke(context);
            else if (elseBody != null)
                return elseBody.Invoke(context);
            return null;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, depth, varibles);
            Parser.Optimize(ref condition, 2, varibles);
            Parser.Optimize(ref elseBody, depth, varibles);
            return false;
        }

        public override string ToString()
        {
            string rp = Environment.NewLine;
            string rs = Environment.NewLine + "  ";
            var sbody = body.ToString();
            var sebody = elseBody == null ? "" : elseBody.ToString();
            return "if (" + condition + ")" + (body is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs)) +
                (elseBody != null ?
                Environment.NewLine + "else" + Environment.NewLine +
                (elseBody is CodeBlock ? sebody.Replace(rp, rs) : "  " + sebody) : "");
        }
    }
}