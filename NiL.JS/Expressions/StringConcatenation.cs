using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class StringConcatenation : Expression
    {
        internal IList<Expression> _parts;

        protected internal override bool ContextIndependent
        {
            get
            {
                for (var i = 0; i < _parts.Count; i++)
                {
                    if (!_parts[i].ContextIndependent)
                        return false;
                }
                return true;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                for (var i = 0; i < _parts.Count; i++)
                {
                    if (_parts[i].NeedDecompose)
                        return true;
                }
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public StringConcatenation(IList<Expression> sources)
            : base(null, null, true)
        {
            if (sources.Count < 2)
                throw new ArgumentException("sources too short");
            this._parts = sources;
        }

        private static object prep(JSValue x)
        {
            if (x._valueType == JSValueType.String)
            {
                return x._oValue;
            }
            if (x._valueType == JSValueType.Date)
                x = x.ToPrimitiveValue_String_Value();
            else
                x = x.ToPrimitiveValue_Value_String();
            if (x._valueType == JSValueType.String)
            {
                return x._oValue;
            }
            return x.ToString();
        }

        public override JSValue Evaluate(Context context)
        {
            var result = prep(_parts[0].Evaluate(context));
            for (var i = 1; i < _parts.Count; i++)
                result = new RopeString(result, prep(_parts[i].Evaluate(context)));

            _tempContainer._valueType = JSValueType.String;
            _tempContainer._oValue = result;
            return _tempContainer;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
                _right = _parts[_parts.Count - 1];
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            for (var i = 0; i < _parts.Count; i++)
            {
                Expression s = _parts[i];
                _parts[i].Decompose(ref s, result);
                _parts[i] = s;
                if (_parts[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(_parts[i] is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_parts[i], false));
                    _parts[i] = new ExtractStoredValue(_parts[i]);
                }
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < _parts.Count; i++)
                _parts[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("(", _parts.Count * 10).Append(_parts[0]);
            for (var i = 1; i < _parts.Count; i++)
                res.Append(" + ").Append(_parts[i]);
            return res.Append(")").ToString();
        }
    }
}