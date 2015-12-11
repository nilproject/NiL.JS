using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Modulo : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var ft = first.ResultType;
                var st = second.ResultType;
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
            var f = first.Evaluate(context);
            if (f.valueType == JSValueType.Integer)
            {
                var ileft = f.iValue;
                f = second.Evaluate(context);
                if (ileft >= 0 && f.valueType == JSValueType.Integer && f.iValue != 0)
                {
                    tempContainer.valueType = JSValueType.Integer;
                    tempContainer.iValue = ileft % f.iValue;
                }
                else
                {
                    tempContainer.valueType = JSValueType.Double;
                    tempContainer.dValue = ileft % Tools.JSObjectToDouble(f);
                }
            }
            else
            {
                double left = Tools.JSObjectToDouble(f);
                tempContainer.dValue = left % Tools.JSObjectToDouble(second.Evaluate(context));
                tempContainer.valueType = JSValueType.Double;
            }
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " % " + second + ")";
        }
    }
}