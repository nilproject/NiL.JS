using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Mod : Expression
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

        public Mod(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var f = first.Evaluate(context);
                if (f.valueType == JSObjectType.Int)
                {
                    var ileft = f.iValue;
                    f = second.Evaluate(context);
                    if (ileft >= 0 && f.valueType == JSObjectType.Int && f.iValue != 0)
                    {
                        tempContainer.valueType = JSObjectType.Int;
                        tempContainer.iValue = ileft % f.iValue;
                    }
                    else
                    {
                        tempContainer.valueType = JSObjectType.Double;
                        tempContainer.dValue = ileft % Tools.JSObjectToDouble(f);
                    }
                }
                else
                {
                    double left = Tools.JSObjectToDouble(f);
                    tempContainer.dValue = left % Tools.JSObjectToDouble(second.Evaluate(context));
                    tempContainer.valueType = JSObjectType.Double;
                }
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " % " + second + ")";
        }
    }
}