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
                throw new ArgumentException("code (" + i + ")");
            bool isDef = false;
            while (char.IsWhiteSpace(code[i])) i++;
            var initializator = new List<Statement>();
            var names = new List<string>();
            while ((code[i] != '\n') && (code[i] != '\r') && (code[i] != ';') && (code[i] != '}'))
            {
                int s = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("invalid char " + code[i]);
                string name = code.Substring(s, i - s);
                names.Add(name);
                while (char.IsWhiteSpace(code[i]) && !Parser.isLineTerminator(code[i])) i++;
                if ((code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Parser.isLineTerminator(code[i])))
                    throw new ArgumentException("code (" + i + ")");
                initializator.Add(OperatorStatement.Parse(state, ref s, false).Statement);
                i = s;
                if ((code[i] != ',') && (code[i] != ';') && (code[i] != '=') && (code[i] != '}') && (!Parser.isLineTerminator(code[i])))
                    throw new ArgumentException("code (" + i + ")");
                while (char.IsWhiteSpace(code[s])) s++;
                if (code[s] == ',')
                {
                    i = s;
                    do i++; while (char.IsWhiteSpace(code[i]));
                }
                else
                    while (char.IsWhiteSpace(code[i]) && !Parser.isLineTerminator(code[i])) i++;
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

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            /*context.localVaribleCount = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                context.GetField(names[i]).Define();
                initializators[i].Invoke(context);
            }
            return null;*/
            throw new InvalidOperationException("VaribleDefineStatement.Invoke");
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            if (initializators.Length > 1)
                _this = new CodeBlock(initializators);
            else
                _this = initializators[0];
            for (var i = 0; i < names.Length; i++)
                varibles.Add(names[i]);
            return _this is IOptimizable;
        }
    }
}