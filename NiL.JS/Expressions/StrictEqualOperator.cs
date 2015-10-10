using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public class StrictEqualOperator : Expression
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

        public StrictEqualOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal static bool Check(JSValue first, JSValue second)
        {
            switch (first.valueType)
            {
                case JSValueType.NotExistsInObject:
                case JSValueType.Undefined:
                    {
                        return second.valueType <= JSValueType.Undefined;
                    }
                case JSValueType.Bool:
                    {
                        if (first.valueType != second.valueType)
                            return false;
                        return first.iValue == second.iValue;
                    }
                case JSValueType.Int:
                    {
                        if (second.valueType == JSValueType.Double)
                            return first.iValue == second.dValue;
                        else if (second.valueType != JSValueType.Int)
                            return false;
                        else
                            return first.iValue == second.iValue;
                    }
                case JSValueType.Double:
                    {
                        if (second.valueType == JSValueType.Int)
                            return first.dValue == second.iValue;
                        else if (second.valueType != JSValueType.Double)
                            return false;
                        else
                            return first.dValue == second.dValue;
                    }
                case JSValueType.String:
                    {
                        if (second.valueType != JSValueType.String)
                            return false;
                        return string.CompareOrdinal(first.oValue.ToString(), second.oValue.ToString()) == 0;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Symbol:
                case JSValueType.Object:
                    {
                        if (first.valueType != second.valueType)
                            return false;
                        else if (first.oValue == null)
                            return second.oValue == null || object.ReferenceEquals(second.oValue, first.oValue);
                        else
                            return object.ReferenceEquals(second.oValue, first.oValue);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public override JSValue Evaluate(Context context)
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