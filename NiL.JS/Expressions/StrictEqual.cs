using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public class StrictEqual : Expression
    {
        public StrictEqual(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal static bool Check(JSObject first, JSObject second, Context context)
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
                            return second.oValue == null || second.oValue.Equals(first.oValue);
                        else
                            return first.oValue.Equals(second.oValue);
                    }
                case JSObjectType.Property:
                    return false;
            }
            throw new NotImplementedException();
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.Assign(first.Evaluate(context));
                return Check(tempContainer, second.Evaluate(context), context);
            }
        }

        public override string ToString()
        {
            return "(" + first + " === " + second + ")";
        }
    }
}