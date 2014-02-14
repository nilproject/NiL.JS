using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class VaribleDefineStatement : Statement, IOptimizable
    {
        public readonly Statement[] initializators;
        public readonly string[] names;

        public VaribleDefineStatement(string name, Statement init)
        {
            names = new[] { name };
            initializators = new[] { init };
        }

        private VaribleDefineStatement(Statement[] initializators, string[] names)
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
                Statement = new VaribleDefineStatement(inits, names.ToArray())
            };
        }

        public override JSObject Invoke(Context context)
        {
            throw new InvalidOperationException("VaribleDefineStatement.Invoke");
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            if (initializators.Length > 1)
                _this = new CodeBlock(initializators, false);
            else
                _this = initializators[0];
            for (var i = 0; i < names.Length; i++)
            {
                if (!varibles.ContainsKey(names[i]))
                    varibles.Add(names[i], null);
            }
            return _this is IOptimizable;
        }
    }
}