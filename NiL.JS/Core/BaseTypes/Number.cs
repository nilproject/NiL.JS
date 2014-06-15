using System;
using NiL.JS.Core.Modules;
using System.Collections.Generic;
using System.Globalization;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Immutable]
    public sealed class Number : EmbeddedType
    {
        [DoNotDelete]
        [DoNotEnumerate]
        [ReadOnly]
        public static JSObject NaN = double.NaN;
        [DoNotDelete]
        [DoNotEnumerate]
        [ReadOnly]
        public static JSObject POSITIVE_INFINITY = double.PositiveInfinity;
        [DoNotDelete]
        [DoNotEnumerate]
        [ReadOnly]
        public static JSObject NEGATIVE_INFINITY = double.NegativeInfinity;
        [DoNotDelete]
        [DoNotEnumerate]
        [ReadOnly]
        public static JSObject MAX_VALUE = double.MaxValue;
        [DoNotDelete]
        [DoNotEnumerate]
        [ReadOnly]
        public static JSObject MIN_VALUE = double.Epsilon;

        [Modules.DoNotEnumerate]
        static Number()
        {
            POSITIVE_INFINITY.assignCallback = null;
            POSITIVE_INFINITY.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
            NEGATIVE_INFINITY.assignCallback = null;
            NEGATIVE_INFINITY.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
            MAX_VALUE.assignCallback = null;
            MAX_VALUE.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
            MIN_VALUE.assignCallback = null;
            MIN_VALUE.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
            NaN.assignCallback = null;
            NaN.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
        }

        [Modules.DoNotEnumerate]
        public Number()
        {
            valueType = JSObjectType.Int;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(int value)
        {
            valueType = JSObjectType.Int;
            iValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(double value)
        {
            valueType = JSObjectType.Double;
            dValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [Modules.DoNotEnumerate]
        public Number(string value)
        {
            value = value.Trim();
            valueType = JSObjectType.Int;
            assignCallback = JSObject.ErrorAssignCallback;
            dValue = value.Length != 0 ? double.NaN : 0;
            valueType = JSObjectType.Double;
            double d = 0;
            int i = 0;
            if (value.Length != 0 && Tools.ParseNumber(value, ref i, out d, 0, !Context.CurrentContext.strict) && i == value.Length)
                dValue = d;
        }

        [Modules.DoNotEnumerate]
        public Number(JSObject obj)
        {
            valueType = JSObjectType.Double;
            dValue = Tools.JSObjectToDouble(obj.GetMember("0"));
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public JSObject toPrecision(JSObject digits)
        {
            double res = 0;
            switch (valueType)
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
            switch ((digits ?? JSObject.undefined).valueType)
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
                        if (Tools.ParseNumber(digits.oValue.ToString(), i, out d))
                            dgts = (int)d;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetMember("0").ToPrimitiveValue_Value_String();
                        if (digits.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    return Tools.DoubleToString(res);
            }
            string integerPart = ((int)res).ToString(CultureInfo.InvariantCulture);
            if (integerPart.Length <= dgts)
                return Tools.DoubleToString(System.Math.Round(res, dgts - integerPart.Length));
            var sres = ((int)res).ToString("e" + (dgts - 1), System.Globalization.CultureInfo.InvariantCulture);
            return sres;
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public JSObject toExponential(JSObject digits)
        {
            double res = 0;
            switch (valueType)
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
            switch ((digits ?? JSObject.undefined).valueType)
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
                        if (Tools.ParseNumber(digits.oValue.ToString(), i, out d))
                            dgts = (int)d;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetMember("0").ToPrimitiveValue_Value_String();
                        if (digits.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    return res.ToString("e", System.Globalization.CultureInfo.InvariantCulture);
            }
            return res.ToString("e" + dgts, System.Globalization.CultureInfo.InvariantCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public JSObject toFixed(JSObject digits)
        {
            double res = 0;
            switch (valueType)
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
            switch ((digits ?? JSObject.undefined).valueType)
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
                        if (Tools.ParseNumber(digits.oValue.ToString(), i, out d))
                            dgts = (int)d;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        digits = digits.GetMember("0").ToPrimitiveValue_Value_String();
                        if (digits.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (digits.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (digits.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    return ((int)res).ToString(CultureInfo.InvariantCulture);
            }
            if (System.Math.Abs(dValue) >= 1e+21)
                return dValue.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            if (dgts < 0 || dgts > 20)
                throw new JSException(TypeProxy.Proxy(new RangeError("toFixed() digits argument must be between 0 and 20")));
            if (dgts > 0)
                dgts++;
            return System.Math.Round(res, dgts).ToString("0.00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public override JSObject toLocaleString()
        {
            return valueType == JSObjectType.Int ? iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public override JSObject toString(JSObject radix)
        {
            if (this.valueType != JSObjectType.Int && this.valueType != JSObjectType.Double)
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Number.toString on not Number object")));
            int r = 10;
            if (radix != null && radix.GetMember("length").iValue > 0)
            {
                var ar = radix.GetMember("0");
                if (ar.valueType == JSObjectType.Object && ar.oValue == null)
                    throw new JSException(TypeProxy.Proxy(new Error("Radix can't be null.")));
                switch (ar.valueType)
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
                if (valueType == JSObjectType.Double)
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

        [AllowUnsafeCall(typeof(JSObject))]
        [Modules.DoNotEnumerate]
        public override JSObject valueOf()
        {
            if (!typeof(Number).IsAssignableFrom(GetType()))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Number.valueOf on not number object.")));
            if (valueType == JSObjectType.Object) // prototype instance
                return 0;
            return base.valueOf();
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            return EmptyEnumerator;
        }

        [Hidden]
        public override string ToString()
        {
            return valueType == JSObjectType.Int ? iValue >= 0 && iValue < 16 ? Tools.NumString[iValue] : iValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : Tools.DoubleToString(dValue);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return valueType == JSObjectType.Int ? iValue.GetHashCode() : dValue.GetHashCode();
        }

        [Hidden]
        public static implicit operator Number(int value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator double(Number value)
        {
            return value == null ? 0 : value.valueType == JSObjectType.Int ? value.iValue : value.dValue;
        }

        [Hidden]
        public static explicit operator int(Number value)
        {
            return value == null ? 0 : value.valueType == JSObjectType.Int ? value.iValue : (int)value.dValue;
        }
    }
}