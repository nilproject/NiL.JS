using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class ThrowStatement : Statement
    {
        private Statement body;

        public Statement Body { get { return body; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "throw", ref i) || (!char.IsWhiteSpace(code[i]) && (code[i] != '(')))
                return new ParseResult();
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Can't throw result of EmptyStatement " + Tools.PositionToTextcord(code, i - 1))));
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ThrowStatement()
                {
                    body = b,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            throw new JSException(body.Invoke(context));
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref body, 2, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}