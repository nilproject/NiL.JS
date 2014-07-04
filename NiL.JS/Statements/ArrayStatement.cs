using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ArrayStatement : CodeNode
    {
        private CodeNode[] elements;

        public ICollection<CodeNode> Elements { get { return elements; } }

        private ArrayStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (code[index] != '[')
                throw new ArgumentException("Syntax error. Expected '['");
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            var elms = new List<CodeNode>();
            while (code[i] != ']')
            {
                if (code[i] == ',')
                    elms.Add(null);
                else
                    elms.Add(ExpressionStatement.Parse(state, ref i, false).Statement);
                while (char.IsWhiteSpace(code[i]))
                    i++;
                if (code[i] == ',')
                {
                    do
                        i++;
                    while (char.IsWhiteSpace(code[i]));
                }
                else if (code[i] != ']')
                    throw new ArgumentException("Syntax error. Expected ']'");
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
            for (uint i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    var e = elements[i].Invoke(context).CloneImpl();
                    e.attributes = 0;
                    res.data[i] = e;
                }
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return elements;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (int i = 0; i < elements.Length; i++)
                Parser.Optimize(ref elements[i], 2, fdepth, vars, strict);
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