using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class VaribleDefineStatement : Statement
    {
        internal readonly Statement[] initializators;
        internal readonly string[] names;

        public Statement[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        public VaribleDefineStatement(string name, Statement init)
        {
            names = new[] { name };
            initializators = new[] { init };
        }

        private VaribleDefineStatement(string[] names, Statement[] initializators)
        {
            this.initializators = initializators;
            this.names = names;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "var ", ref i))
                return new ParseResult();
            bool isDef = false;
            while (char.IsWhiteSpace(code[i])) i++;
            var initializator = new List<Statement>();
            var names = new List<string>();
            while ((code[i] != '\n') && (code[i] != '\r') && (code[i] != ';') && (code[i] != '}'))
            {
                int s = i;
                if (!Parser.ValidateName(code, ref i, true, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid varible definition at " + Tools.PositionToTextcord(code, s))));
                string name = Tools.Unescape(code.Substring(s, i - s));
                names.Add(name);
                while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if ((code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Tools.isLineTerminator(code[i])))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + Tools.PositionToTextcord(code, i))));
                initializator.Add(OperatorStatement.Parse(state, ref s, false).Statement);
                i = s;
                if ((code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Tools.isLineTerminator(code[i])))
                    throw new ArgumentException("code (" + i + ")");
                while (char.IsWhiteSpace(code[s])) s++;
                if (code[s] == ',')
                {
                    i = s;
                    do i++; while (char.IsWhiteSpace(code[i]));
                }
                else
                    while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                isDef = true;
            }
            if (!isDef)
                throw new ArgumentException("code (" + i + ")");
            var inits = initializator.ToArray();
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new VaribleDefineStatement(names.ToArray(), inits)
            };
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject res = null;
            for (int i = 0; i < initializators.Length; i++)
                res = initializators[i].Invoke(context);
            return res;
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            for (int i = 0; i < initializators.Length; i++)
                Parser.Optimize(ref initializators[i], 1, varibles);
            for (var i = 0; i < names.Length; i++)
            {
                if (!varibles.ContainsKey(names[i]))
                    varibles.Add(names[i], null);
            }
            return false;
        }

        public override string ToString()
        {
            var res = "var ";
            for (var i = 0; i < initializators.Length; i++)
            {
                var t = initializators[i].ToString();
                if (string.IsNullOrEmpty(t))
                    continue;
                if (t[0] == '(')
                    t = t.Substring(1, t.Length - 2);
                if (i > 0)
                    res += ", ";
                res += t;
            }
            return res;
        }
    }
}