using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public class StrictEqual : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public StrictEqual(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal static bool Check(JSObject first, JSObject second)
        {
            switch (first.valueType)
            {
                case JSObjectType.NotExistsInObject:
                case JSObjectType.Undefined:
                    {
                        return second.valueType <= JSObjectType.Undefined;
                    }
                case JSObjectType.Bool:
                    {
                        if (first.valueType != second.valueType)
                            return false;
                        return first.iValue == second.iValue;
                    }
                case JSObjectType.Int:
                    {
                        if (second.valueType == JSObjectType.Double)
                            return first.iValue == second.dValue;
                        else if (second.valueType != JSObjectType.Int)
                            return false;
                        else
                            return first.iValue == second.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (second.valueType == JSObjectType.Int)
                            return first.dValue == second.iValue;
                        else if (second.valueType != JSObjectType.Double)
                            return false;
                        else
                            return first.dValue == second.dValue;
                    }
                case JSObjectType.String:
                    {
                        if (second.valueType != JSObjectType.String)
                            return false;
                        return string.CompareOrdinal(first.oValue.ToString(), second.oValue.ToString()) == 0;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (first.valueType != second.valueType)
                            return false;
                        else if (first.oValue == null)
                            return second.oValue == null || object.ReferenceEquals(second.oValue, first.oValue);
                        else
                            return object.ReferenceEquals(second.oValue, first.oValue);
                    }
                default:
                    return false;
            }
        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.Assign(first.Evaluate(context));
            if (Check(tempContainer, second.Evaluate(context)))
                return BaseLibrary.Boolean.True;
            return BaseLibrary.Boolean.False;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " === " + second + ")";
        }
    }
}