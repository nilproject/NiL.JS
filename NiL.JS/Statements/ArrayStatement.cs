using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ArrayStatement : CodeNode
    {
        private CodeNode[] elements;

        public ICollection<CodeNode> Elements { get { return elements; } }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       JITHelpers.methodof(impl),
                       JITHelpers.ContextParameter,
                       System.Linq.Expressions.Expression.Constant(elements)
                       );
        }

#endif

        private ArrayStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (state.Code[index] != '[')
                throw new ArgumentException("Syntax error. Expected '['");
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            var elms = new List<CodeNode>();
            while (state.Code[i] != ']')
            {
                if (state.Code[i] == ',')
                    elms.Add(null);
                else
                    elms.Add(ExpressionStatement.Parse(state, ref i, false).Statement);
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] == ',')
                {
                    do
                        i++;
                    while (char.IsWhiteSpace(state.Code[i]));
                }
                else if (state.Code[i] != ']')
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

#if INLINE
        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static JSObject impl(Context context, CodeNode[] elements)
        {
            var res = new NiL.JS.Core.BaseTypes.Array((long)elements.Length);
            if (elements.Length > 0)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] != null)
                    {
                        var e = elements[i].Evaluate(context).CloneImpl();
                        e.attributes = 0;
                        res.data[i] = e;
                    }
                }
                res.data[elements.Length - 1] = res.data[elements.Length - 1];
            }
            return res;
        }

        internal override JSObject Evaluate(Context context)
        {
            return impl(context, elements);
        }

        protected override CodeNode[] getChildsImpl()
        {
            return elements;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (int i = 0; i < elements.Length; i++)
                Parser.Build(ref elements[i], 2, vars, strict);
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