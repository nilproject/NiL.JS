using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ArrayDefinition : Expression
    {
        private static JSValue writableNotExists = null;
        private Expression[] elements;

        public Expression[] Elements { get { return elements; } }

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

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (state.Code[index] != '[')
                throw new ArgumentException("Syntax error. Expected '['");
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));
            var elms = new List<Expression>();
            while (state.Code[i] != ']')
            {
                var start = i;
                var spread = Parser.Validate(state.Code, "...", ref i);
                if (state.Code[i] == ',')
                {
                    if (spread)
                        ExceptionHelper.ThrowSyntaxError("Expected expression", state.Code, i);
                    elms.Add(null);
                }
                else
                    elms.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                if (spread)
                    elms[elms.Count - 1] = new Spread(elms[elms.Count - 1]) { Position = start, Length = i - start };
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] == ',')
                {
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));
                }
                else if (state.Code[i] != ']')
                    ExceptionHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
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

        public override JSValue Evaluate(Context context)
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
                        if (e._valueType == JSValueType.SpreadOperatorResult)
                        {
                            var spreadArray = e._oValue as IList<JSValue>;
                            for (var i = 0; i < spreadArray.Count; i++, targetIndex++)
                            {
                                res._data[targetIndex] = spreadArray[i].CloneImpl(false);
                            }
                            targetIndex--;
                        }
                        else
                        {
                            e = e.CloneImpl(true);
                            e._attributes = 0;
                            res._data[targetIndex] = e;
                        }
                    }
                    else
                    {
                        if (writableNotExists == null)
                            writableNotExists = new JSValue() { _valueType = JSValueType.NotExistsInObject, _attributes = JSValueAttributesInternal.SystemObject };
                        res._data[targetIndex] = writableNotExists;
                    }
                }
            }
            return res;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return elements;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            for (int i = 0; i < elements.Length; i++)
                Parser.Build(ref elements[i], 2,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            for (var i = elements.Length; i-- > 0; )
            {
                var cn = elements[i] as CodeNode;
                if (cn != null)
                {
                    cn.Optimize(ref cn, owner, message, opts, stats);
                    elements[i] = cn as Expression;
                }
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
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
                if (!(elements[i] is ExtractStoredValue))
                {
                    result.Add(new StoreValue(elements[i], false));
                    elements[i] = new ExtractStoredValue(elements[i]);
                }
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < elements.Length; i++)
                elements[i]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
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