using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ArrayStatement : Statement
    {
        private Statement[] elements;

        public Statement[] Elements { get { return elements; } }

        private ArrayStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (code[index] != '[')
                throw new ArgumentException();
            do i++; while (char.IsWhiteSpace(code[i]));
            var elms = new List<Statement>();
            while (code[i] != ']')
            {
                if (code[i] == ',')
                    elms.Add(new ImmidateValueStatement(JSObject.undefined));
                else
                    elms.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] == ',')
                {
                    do i++;
                    while (char.IsWhiteSpace(code[i]));
                }
                else if (code[i] != ']')
                    throw new ArgumentException();
            }
            i++;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ArrayStatement()
                {
                    elements = elms.ToArray(),
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            var res = new NiL.JS.Core.BaseTypes.Array(elements.Length);
            for (int i = 0; i < elements.Length; i++)
                res[i] = elements[i].Invoke(context).Clone() as JSObject;
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            return elements;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            for (int i = 0; i < elements.Length; i++)
                Parser.Optimize(ref elements[i], 2, vars);
            return false;
        }

        public override string ToString()
        {
            string res = "[";
            for (int i = 0; i < elements.Length; i++)
            {
                res += elements[i];
                if (i + 1 < elements.Length)
                    res += ", ";
            }
            return res + ']';
        }
    }
}