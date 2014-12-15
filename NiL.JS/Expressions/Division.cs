using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Division : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Double;
            }
        }

        public Division(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.dValue = Tools.JSObjectToDouble(first.Evaluate(context)) / Tools.JSObjectToDouble(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Double;
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}