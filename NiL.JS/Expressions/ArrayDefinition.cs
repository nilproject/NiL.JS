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
    public sealed class ArrayDefinition : Expression
    {
        private static JSValue writableNotExists = null;
        private Expression[] elements;

        public ICollection<Expression> Elements { get { return elements; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                for (var i = 0; i < elements.Length; i++)
                {
                    if (elements[i] != null && elements[i].NeedDecompose)
                        return true;
                }
                return false;
            }
        }

        private ArrayDefinition()
        {

        }

        internal static CodeNode Parse(ParsingState state, ref int index)
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
                var start = i;
                var spread = Parser.Validate(state.Code, "...", ref i);
                if (state.Code[i] == ',')
                {
                    if (spread)
                        ExceptionsHelper.ThrowSyntaxError("Expected expression", state.Code, i);
                    elms.Add(null);
                }
                else
                    elms.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                if (spread)
                    elms[elms.Count - 1] = new SpreadOperator(elms[elms.Count - 1]) { Position = start, Length = i - start };
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] == ',')
                {
                    do
                        i++;
                    while (char.IsWhiteSpace(state.Code[i]));
                }
                else if (state.Code[i] != ']')
                    ExceptionsHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
            }
            i++;
            var pos = index;
            index = i;
            return new ArrayDefinition()
                {
                    elements = elms.ToArray(),
                    Position = pos,
                    Length = index - pos
                };
        }

#if INLINE
        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static JSValue impl(Context context, Expression[] elements)
        {
            var length = elements.Length;
            var res = new BaseLibrary.Array(length);
            if (length > 0)
            {
                for (int sourceIndex = 0, targetIndex = 0; sourceIndex < length; sourceIndex++, targetIndex++)
                {
                    if (elements[sourceIndex] != null)
                    {
                        var e = elements[sourceIndex].Evaluate(context);
                        if (e.valueType == JSValueType.SpreadOperatorResult)
                        {
                            var spreadArray = e.oValue as IList<JSValue>;
                            for (var i = 0; i < spreadArray.Count; i++, targetIndex++)
                            {
                                res.data[targetIndex] = spreadArray[i].CloneImpl(false);
                            }
                            targetIndex--;
                        }
                        else
                        {
                            e = e.CloneImpl(true);
                            e.attributes = 0;
                            res.data[targetIndex] = e;
                        }
                    }
                    else
                    {
                        if (writableNotExists == null)
                            writableNotExists = new JSValue() { valueType = JSValueType.NotExistsInObject, attributes = JSValueAttributesInternal.SystemObject };
                        res.data[targetIndex] = writableNotExists;
                    }
                }
            }
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            return impl(context, elements);
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return elements;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            _codeContext = codeContext;

            for (int i = 0; i < elements.Length; i++)
                Parser.Build(ref elements[i], 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            for (var i = elements.Length; i-- > 0; )
            {
                var cn = elements[i] as CodeNode;
                if (cn != null)
                {
                    cn.Optimize(ref cn, owner, message, opts, statistic);
                    elements[i] = cn as Expression;
                }
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i].Decompose(ref elements[i], result);
                if (elements[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(elements[i] is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(elements[i], false));
                    elements[i] = new ExtractStoredValueExpression(elements[i]);
                }
            }
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