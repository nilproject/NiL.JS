using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements.Operators;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class VariableDefineStatement : Statement
    {
        internal readonly Statement[] initializators;
        internal readonly string[] names;

        public Statement[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        public VariableDefineStatement(string name, Statement init)
        {
            names = new[] { name };
            initializators = new[] { init };
        }

        private VariableDefineStatement(string[] names, Statement[] initializators)
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
            while ((code[i] != ';') && (code[i] != '}') && !Tools.isLineTerminator(code[i]))
            {
                int s = i;
                if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                {
                    if (Parser.ValidateName(code, ref i, false, true, state.strict.Peek()))
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError('\"' + Tools.Unescape(code.Substring(s, i - s), state.strict.Peek()) + "\" is a reserved word at " + Tools.PositionToTextcord(code, s))));
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid variable definition at " + Tools.PositionToTextcord(code, s))));
                }
                string name = Tools.Unescape(code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (name == "arguments" || name == "eval")
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, s))));
                }
                names.Add(name);
                isDef = true;
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if (i < code.Length && (code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Tools.isLineTerminator(code[i])))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + Tools.PositionToTextcord(code, i))));
                if (i >= code.Length)
                {
                    initializator.Add(new GetVariableStatement(name) { Position = s, Length = name.Length });
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
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected end of line in variable defenition.")));
                    initializator.Add(
                        new Assign(new GetVariableStatement(name) { Position = s, Length = name.Length }, OperatorStatement.Parse(state, ref i, false).Statement)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                    initializator.Add(new GetVariableStatement(name) { Position = s, Length = name.Length });
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
                Statement = new VariableDefineStatement(names.ToArray(), inits)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            for (int i = 0; i < initializators.Length; i++)
                initializators[i].Invoke(context);
            return JSObject.undefined;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>();
            res.AddRange(initializators);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            for (int i = 0; i < initializators.Length; i++)
                Parser.Optimize(ref initializators[i], 1, variables, strict);
            for (var i = 0; i < names.Length; i++)
                variables[names[i]].Defined = true;
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