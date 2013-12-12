using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class ArrayStatement : Statement
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
                    while (char.IsWhiteSpace(code[i])) ;
                }
                else if (code[i] != ']')
                    throw new ArgumentException();
            }
            do i++; while (char.IsWhiteSpace(code[i]));
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

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            var res = new NiL.JS.Core.BaseTypes.JSArray();
            for (int i = 0; i < elements.Length; i++)
                res.GetField(i.ToString()).Assign(elements[i].Invoke(context));
            res.GetField("length").Assign(elements.Length);
            return res;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
