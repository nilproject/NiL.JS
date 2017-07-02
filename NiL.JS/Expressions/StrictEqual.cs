using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
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

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public StrictEqual(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal static bool Check(JSValue first, JSValue second)
        {
            switch (first._valueType)
            {
                case JSValueType.NotExistsInObject:
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                    {
                        return second._valueType <= JSValueType.Undefined;
                    }
                case JSValueType.Boolean:
                    {
                        if (first._valueType != second._valueType)
                            return false;
                        return first._iValue == second._iValue;
                    }
                case JSValueType.Integer:
                    {
                        if (second._valueType == JSValueType.Double)
                            return first._iValue == second._dValue;
                        else if (second._valueType != JSValueType.Integer)
                            return false;
                        else
                            return first._iValue == second._iValue;
                    }
                case JSValueType.Double:
                    {
                        if (second._valueType == JSValueType.Integer)
                            return first._dValue == second._iValue;
                        else if (second._valueType != JSValueType.Double)
                            return false;
                        else
                            return first._dValue == second._dValue;
                    }
                case JSValueType.String:
                    {
                        if (second._valueType != JSValueType.String)
                            return false;
                        return string.CompareOrdinal(first._oValue.ToString(), second._oValue.ToString()) == 0;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Symbol:
                case JSValueType.Object:
                    {
                        if (first._valueType != second._valueType)
                            return false;
                        else if (first._oValue == null)
                            return second._oValue == null;
                        else
                            return object.ReferenceEquals(second._oValue, first._oValue);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public override JSValue Evaluate(Context context)
        {
            _tempContainer.Assign(_left.Evaluate(context));
            if (Check(_tempContainer, _right.Evaluate(context)))
                return BaseLibrary.Boolean.True;
            return BaseLibrary.Boolean.False;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " === " + _right + ")";
        }
    }
}