using System;
using NiL.JS.Core.Modules;
using System.Collections.Generic;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Immutable]
    public class Number : EmbeddedType
    {
        [Modules.DoNotEnumerate]
        [Modules.Protected]
        public static JSObject NaN = double.NaN;
        [Modules.DoNotEnumerate]
        [Modules.Protected]
        public static JSObject POSITIVE_INFINITY = double.PositiveInfinity;
        [Modules.DoNotEnumerate]
        [Modules.Protected]
        public static JSObject NEGATIVE_INFINITY = double.NegativeInfinity;
        [Modules.DoNotEnumerate]
        [Modules.Protected]
        public static JSObject MAX_VALUE = double.MaxValue;
        [Modules.DoNotEnumerate]
        [Modules.Protected]
        public static JSObject MIN_VALUE = double.Epsilon;

        [Modules.DoNotEnumerate]
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

        [Modules.DoNotEnumerate]
        public Number()
        {
            ValueType = JSObjectType.Int;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(int value)
        {
            ValueType = JSObjectType.Int;
            iValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(double value)
        {
            ValueType = JSObjectType.Double;
            dValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(string value)
        {
            ValueType = JSObjectType.Int;
            assignCallback = JSObject.ErrorAssignCallback;
            dValue = double.NaN;
            ValueType = JSObjectType.Double;
            double d = 0;
            int i = 0;
            value = value.Trim();
            if (Tools.ParseNumber(value, ref i, true, out d) && i == value.Length)
                dValue = d;
        }

        [Modules.DoNotEnumerate]
        public Number(JSObject obj)
        {
            ValueType = JSObjectType.Double;
            dValue = Tools.JSObjectToDouble(obj.GetField("0", true, false));
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
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
                            dgts = (int)d;
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

        [Modules.DoNotEnumerate]
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
                            dgts = (int)d;
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

        [Modules.DoNotEnumerate]
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
                case JSObjectType.Object:
                    {
                        if (!typeof(Number).IsAssignableFrom(GetType()))
                            throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Number.toFixed on not number object.")));
                        res = 0;
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
                        if (double.IsNaN(digits.dValue))
                            dgts = 0;
                        else if (double.IsInfinity(digits.dValue))
                            throw new JSException(TypeProxy.Proxy(new RangeError("toFixed() digits argument must be between 0 and 20")));
                        else
                            dgts = (int)digits.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                            dgts = (int)d;
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
            if (System.Math.Abs(dValue) >= 1e+21)
                return dValue.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            if (dgts < 0 || dgts > 20)
                throw new JSException(TypeProxy.Proxy(new RangeError("toFixed() digits argument must be between 0 and 20")));
            if (dgts > 0)
                dgts++;
            return System.Math.Round(res, dgts).ToString("0.00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        [Modules.DoNotEnumerate]
        public override JSObject toLocaleString()
        {
            return ValueType == JSObjectType.Int ? iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        [Modules.DoNotEnumerate]
        public override JSObject toString(JSObject radix)
        {
            if (!typeof(Number).IsAssignableFrom(this.GetType()))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Number.toString on not Number object")));
            int r = 10;
            if (radix != null && radix.GetField("length", true, false).iValue > 0)
            {
                var ar = radix.GetField("0", true, false);
                if (ar.ValueType == JSObjectType.Object && ar.oValue == null)
                    throw new JSException(TypeProxy.Proxy(new Error("Radix can't be null.")));
                switch (ar.ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            r = ar.iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            r = (int)ar.dValue;
                            break;
                        }
                    case JSObjectType.NotExistInObject:
                    case JSObjectType.Undefined:
                        {
                            r = 10;
                            break;
                        }
                    default:
                        {
                            r = Tools.JSObjectToInt(ar);
                            break;
                        }
                }
            }
            if (r < 2 || r > 36)
                throw new JSException(TypeProxy.Proxy(new TypeError("Radix must be between 2 and 36.")));
            if (r == 10)
                return ToString();
            else
            {
                long res = iValue;
                if (ValueType == JSObjectType.Double)
                {
                    if (double.IsNaN(dValue))
                        return "NaN";
                    if (double.IsPositiveInfinity(dValue))
                        return "Infinity";
                    if (double.IsNegativeInfinity(dValue))
                        return "-Infinity";
                    res = (int)dValue;
                }
                bool neg = res < 0;
                if (neg)
                    res = -res;
                string sres = Tools.NumChars[res % r].ToString();
                res /= r;
                while (res != 0)
                {
                    sres = Tools.NumChars[res % r] + sres;
                    res /= r;
                }
                return neg ? "-" + sres : sres;
            }
        }

        [Modules.DoNotEnumerate]
        public override JSObject valueOf()
        {
            if (!typeof(Number).IsAssignableFrom(GetType()))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Number.valueOf on not number object.")));
            if (ValueType == JSObjectType.Object) // prototype instance
                return 0;
            return base.valueOf();
        }

        [Modules.DoNotEnumerate]
        public override IEnumerator<string> GetEnumerator()
        {
            return EmptyEnumerator;
        }

        [Modules.Hidden]
        public override string ToString()
        {
            return ValueType == JSObjectType.Int ? iValue >= 0 && iValue < 16 ? Tools.NumString[iValue] : iValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : Tools.DoubleToString(dValue);
        }

        [Modules.Hidden]
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