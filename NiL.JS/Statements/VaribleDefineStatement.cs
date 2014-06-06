using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements.Operators;

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
                {
                    if (Parser.ValidateName(code, ref i, true, false, true, state.strict.Peek()))
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError('\"' + Tools.Unescape(code.Substring(s, i - s)) + "\" is a reserved word at " + Tools.PositionToTextcord(code, s))));
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid varible definition at " + Tools.PositionToTextcord(code, s))));
                }
                string name = Tools.Unescape(code.Substring(s, i - s));
                names.Add(name);
                isDef = true;
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if (i < code.Length && (code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Tools.isLineTerminator(code[i])))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + Tools.PositionToTextcord(code, i))));
                if (i >= code.Length)
                {
                    initializator.Add(new GetVaribleStatement(name) { Position = s, Length = name.Length });
                    break;
                }
                if (Tools.isLineTerminator(code[i]))
                {
                    s = i;
                    do i++; while (i < code.Length && char.IsWhiteSpace(code[i]));
                    if (i >= code.Length)
                        break;
                    if (code[i] != '=')
                        i = s;
                }
                if (code[i] == '=')
                {
                    do i++; while (i < code.Length && char.IsWhiteSpace(code[i]));
                    if (i == code.Length)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected end of line in varible defenition.")));
                    initializator.Add(
                        new Assign(new GetVaribleStatement(name) { Position = s, Length = name.Length }, OperatorStatement.Parse(state, ref i, false).Statement)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                    initializator.Add(new GetVaribleStatement(name) { Position = s, Length = name.Length });
                if (i >= code.Length)
                    break;
                s = i;
                if ((code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Tools.isLineTerminator(code[i])))
                    throw new ArgumentException("code (" + i + ")");
                while (s < code.Length && char.IsWhiteSpace(code[s])) s++;
                if (s >= code.Length)
                    break;
                if (code[s] == ',')
                {
                    i = s;
                    do i++; while (char.IsWhiteSpace(code[i]));
                }
                else
                    while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            }
            if (!isDef)
                throw new ArgumentException("code (" + i + ")");
            var inits = initializator.ToArray();
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new VaribleDefineStatement(names.ToArray(), inits)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject res = null;
            for (int i = 0; i < initializators.Length; i++)
                res = initializators[i].Invoke(context);
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>();
            res.AddRange(initializators);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            for (int i = 0; i < initializators.Length; i++)
                Parser.Optimize(ref initializators[i], 1, varibles);
            for (var i = 0; i < names.Length; i++)
                varibles[names[i]].Defined = true;
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