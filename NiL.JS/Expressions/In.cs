using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class In : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public In(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            bool res;
            if (_tempContainer == null)
                _tempContainer = new JSValue { _attributes = JSValueAttributesInternal.Temporary };
            _tempContainer.Assign(_left.Evaluate(context));
            var temp = _tempContainer;
            _tempContainer = null;
            var source = _right.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Right-hand value of operator in is not object."));
            if (temp._valueType == JSValueType.Integer)
            {
                var array = source._oValue as BaseLibrary.Array;
                if (array != null)
                {
                    res = temp._iValue >= 0 && temp._iValue < array._data.Length && (array._data[temp._iValue] ?? JSValue.notExists).Exists;
                    _tempContainer = temp;
                    return res;
                }
            }
            var t = source.GetProperty(temp, false, PropertyScope.Common);
            _tempContainer = temp;
            return t.Exists;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " in " + _right + ")";
        }
    }
}