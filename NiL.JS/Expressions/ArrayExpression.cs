using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ArrayExpression : Expression
    {
        private static JSObject writableNotExist = null;
        private Expression[] elements;

        public ICollection<Expression> Elements { get { return elements; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private ArrayExpression()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (state.Code[index] != '[')
                throw new ArgumentException("Syntax error. Expected '['");
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            var elms = new List<Expression>();
            while (state.Code[i] != ']')
            {
                if (state.Code[i] == ',')
                    elms.Add(null);
                else
                    elms.Add((Expression)ExpressionTree.Parse(state, ref i, false).Statement);
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
                Statement = new ArrayExpression()
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
            var res = new NiL.JS.BaseLibrary.Array((long)elements.Length);
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
                    else
                        res.data[i] = (writableNotExist ?? (writableNotExist = new JSObject() { valueType = JSObjectType.NotExistsInObject, attributes = JSObjectAttributesInternal.SystemObject }));
                }
                //res.data[elements.Length - 1] = res.data[elements.Length - 1];
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            for (int i = 0; i < elements.Length; i++)
                Parser.Build(ref elements[i], 2,variables, state, message, statistic, opts);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            for (var i = elements.Length; i-- > 0; )
            {
                var cn = elements[i] as CodeNode;
                if (cn != null)
                {
                    cn.Optimize(ref cn, owner, message);
                    elements[i] = cn as Expression;
                }
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
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