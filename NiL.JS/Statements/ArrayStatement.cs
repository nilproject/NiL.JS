using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class ArrayStatement : Statement, IOptimizable
    {
        private Statement[] elements;

        public ArrayStatement()
        {

        }

        public static ParseResult Parse(ParsingState state, ref int index)
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
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ArrayStatement()
                {
                    elements = elms.ToArray()
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            var res = new NiL.JS.Core.BaseTypes.Array(elements.Length);
            for (int i = 0; i < elements.Length; i++)
                res.data[i] = elements[i].Invoke(context);
            return res;
        }

        public override JSObject Invoke(Context context, JSObject args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            for (int i = 0; i < elements.Length; i++)
                Parser.Optimize(ref elements[i], 2, vars);
            return false;
        }
    }
}