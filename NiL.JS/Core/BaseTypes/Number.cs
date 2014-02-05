using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    internal class Number : EmbeddedType
    {
        [Modules.Protected]
        public static JSObject NaN = double.NaN;
        [Modules.Protected]
        public static JSObject POSITIVE_INFINITY = double.PositiveInfinity;
        [Modules.Protected]
        public static JSObject NEGATIVE_INFINITY = double.NegativeInfinity;
        [Modules.Protected]
        public static JSObject MAX_VALUE = double.MaxValue;
        [Modules.Protected]
        public static JSObject MIN_VALUE = double.Epsilon;

        static Number()
        {
            POSITIVE_INFINITY.assignCallback = null;
            POSITIVE_INFINITY.Protect();
            NEGATIVE_INFINITY.assignCallback = null;
            NEGATIVE_INFINITY.Protect();
            MAX_VALUE.assignCallback = null;
            MAX_VALUE.Protect();
            MIN_VALUE.assignCallback = null;
            MIN_VALUE.Protect();
            NaN.assignCallback = null;
            NaN.Protect();
        }
        
        public Number()
        {
            ValueType = JSObjectType.Int;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(int value)
        {
            ValueType = JSObjectType.Int;
            iValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(double value)
        {
            ValueType = JSObjectType.Double;
            dValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(string value)
        {
            ValueType = JSObjectType.Int;
            assignCallback = JSObject.ErrorAssignCallback;
            double d = 0;
            int i = 0;
            if (Tools.ParseNumber(value, ref i, false, out d))
            {
                dValue = d;
                ValueType = JSObjectType.Double;
            }
        }

        public Number(JSObject obj)
        {
            ValueType = JSObjectType.Double;
            dValue = Tools.JSObjectToDouble(obj.GetField("0", true, false));
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public JSObject toPrecision(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case JSObjectType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case JSObjectType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetField("0", true, false).ToPrimitiveValue_Value_String();
                        if (digits.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.ValueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return Tools.DoubleToString(res);
            }
            string integerPart = ((int)res).ToString();
            if (integerPart.Length <= dgts)
                return Tools.DoubleToString(System.Math.Round(res, dgts - integerPart.Length));
            var sres = ((int)res).ToString("e" + (dgts - 1), System.Globalization.CultureInfo.InvariantCulture);
            return sres;
        }

        public JSObject toExponential(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case JSObjectType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case JSObjectType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetField("0", true, false).ToPrimitiveValue_Value_String();
                        if (digits.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.ValueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return res.ToString("e", System.Globalization.CultureInfo.InvariantCulture);
            }
            return res.ToString("e" + dgts, System.Globalization.CultureInfo.InvariantCulture);
        }

        public JSObject toFixed(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case JSObjectType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case JSObjectType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetField("0", true, false).ToPrimitiveValue_Value_String();
                        if (digits.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.ValueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return ((int)res).ToString();
            }
            if (dgts < 0 || dgts > 20)
                throw new ArgumentException("toFixed() digits argument must be between 0 and 20");
            return System.Math.Round(res, dgts).ToString(".00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        public override JSObject toLocaleString()
        {
            return ValueType == JSObjectType.Int ? iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        public override string ToString()
        {
            return ValueType == JSObjectType.Int ? iValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : Tools.DoubleToString(dValue);
        }

        public override int GetHashCode()
        {
            return ValueType == JSObjectType.Int ? iValue.GetHashCode() : dValue.GetHashCode();
        }

        public static implicit operator Number(int value)
        {
            return new Number(value);
        }

        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        public static implicit operator double(Number value)
        {
            return value == null ? 0 : value.ValueType == JSObjectType.Int ? value.iValue : value.dValue;
        }

        public static explicit operator int(Number value)
        {
            return value == null ? 0 : value.ValueType == JSObjectType.Int ? value.iValue : (int)value.dValue;
        }
    }
}