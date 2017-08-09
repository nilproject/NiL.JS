using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Modulo : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var ft = _left.ResultType;
                var st = _right.ResultType;
                if (ft == st)
                    return st;
                return PredictedType.Number;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Modulo(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var f = _left.Evaluate(context);
            if (f._valueType == JSValueType.Integer)
            {
                var ileft = f._iValue;
                f = _right.Evaluate(context);
                if (ileft >= 0 && f._valueType == JSValueType.Integer && f._iValue != 0)
                {
                    _tempContainer._valueType = JSValueType.Integer;
                    _tempContainer._iValue = ileft % f._iValue;
                }
                else
                {
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = ileft % Tools.JSObjectToDouble(f);
                }
            }
            else
            {
                double left = Tools.JSObjectToDouble(f);
                _tempContainer._dValue = left % Tools.JSObjectToDouble(_right.Evaluate(context));
                _tempContainer._valueType = JSValueType.Double;
            }
            return _tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " % " + _right + ")";
        }
    }
}