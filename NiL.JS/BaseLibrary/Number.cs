using System;
using System.Globalization;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Number : JSObject
    {
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSValue NaN = double.NaN;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSValue POSITIVE_INFINITY = double.PositiveInfinity;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSValue NEGATIVE_INFINITY = double.NegativeInfinity;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSValue MAX_VALUE = double.MaxValue;
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public readonly static JSValue MIN_VALUE = double.Epsilon;

        [DoNotEnumerate]
        static Number()
        {
            POSITIVE_INFINITY.attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            NEGATIVE_INFINITY.attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            MAX_VALUE.attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            MIN_VALUE.attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            NaN.attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Number()
        {
            valueType = JSValueType.Int;
            iValue = 0;
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(int value)
        {
            valueType = JSValueType.Int;
            iValue = value;
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [Hidden]
        public Number(long value)
        {
            if ((long)(int)(value) == value)
            {
                valueType = JSValueType.Int;
                iValue = (int)value;
            }
            else
            {
                valueType = JSValueType.Double;
                dValue = value;
            }
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(double value)
        {
            valueType = JSValueType.Double;
            dValue = value;
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(string value)
        {
            value = value.Trim(Tools.TrimChars);
            valueType = JSValueType.Int;
            dValue = value.Length != 0 ? double.NaN : 0;
            valueType = JSValueType.Double;
            double d = 0;
            int i = 0;
            if (value.Length != 0 && Tools.ParseNumber(value, ref i, out d, 0, Tools.ParseNumberOptions.Default | (Context.CurrentContext.strict ? Tools.ParseNumberOptions.RaiseIfOctal : 0)) && i == value.Length)
                dValue = d;
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(Arguments obj)
        {
            valueType = JSValueType.Double;
            dValue = Tools.JSObjectToDouble(obj[0]);
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue toPrecision(JSValue self, Arguments digits)
        {
            double res = Tools.JSObjectToDouble(self);
            return res.ToString("G" + Tools.JSObjectToInt32(digits[0]), CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue toExponential(JSValue self, Arguments digits)
        {
            double res = 0;
            switch (self.valueType)
            {
                case JSValueType.Int:
                    {
                        res = self.iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        res = self.dValue;
                        break;
                    }
                case JSValueType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            ExceptionsHelper.Throw((new TypeError("Try to call Number.toExponential on not number object.")));
                        res = self.iValue == 0 ? self.dValue : self.iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSValue.undefined).valueType)
            {
                case JSValueType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case JSValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits.oValue.ToString(), i, out d, Tools.ParseNumberOptions.Default))
                            dgts = (int)d;
                        break;
                    }
                case JSValueType.Object:
                    {
                        var d = digits[0].ToPrimitiveValue_Value_String();
                        if (d.valueType == JSValueType.String)
                            goto case JSValueType.String;
                        if (d.valueType == JSValueType.Int)
                            goto case JSValueType.Int;
                        if (d.valueType == JSValueType.Double)
                            goto case JSValueType.Double;
                        break;
                    }
                default:
                    return res.ToString("e", System.Globalization.CultureInfo.InvariantCulture);
            }
            return res.ToString("e" + dgts, System.Globalization.CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue toFixed(JSValue self, Arguments digits)
        {
            double res = 0;
            switch (self.valueType)
            {
                case JSValueType.Int:
                    {
                        res = self.iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        res = self.dValue;
                        break;
                    }
                case JSValueType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            ExceptionsHelper.Throw((new TypeError("Try to call Number.toFixed on not number object.")));
                        res = self.iValue == 0 ? self.dValue : self.iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = Tools.JSObjectToInt32(digits[0], true);
            if (dgts < 0 || dgts > 20)
                ExceptionsHelper.Throw((new RangeError("toFixed() digits argument must be between 0 and 20")));
            if (System.Math.Abs(self.dValue) >= 1e+21)
                return self.dValue.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            if (dgts > 0)
                dgts++;
            return System.Math.Round(res, dgts).ToString("0.00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSValue toLocaleString(JSValue self)
        {
            return self.valueType == JSValueType.Int ? self.iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : self.dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        [InstanceMember]
        [DoNotEnumerate]
        [CLSCompliant(false)]
        public static JSValue toString(JSValue self, Arguments radix)
        {
            var ovt = self.valueType;
            if (self.valueType > JSValueType.Double && self is Number)
                self.valueType = self.dValue == 0.0 ? JSValueType.Int : JSValueType.Double;
            try
            {
                if (self.valueType != JSValueType.Int && self.valueType != JSValueType.Double)
                    ExceptionsHelper.Throw((new TypeError("Try to call Number.toString on not Number object")));
                int r = 10;
                if (radix != null && radix.GetMember("length").iValue > 0)
                {
                    var ar = radix[0];
                    if (ar.valueType == JSValueType.Object && ar.oValue == null)
                        ExceptionsHelper.Throw((new Error("Radix can't be null.")));
                    switch (ar.valueType)
                    {
                        case JSValueType.Int:
                        case JSValueType.Bool:
                            {
                                r = ar.iValue;
                                break;
                            }
                        case JSValueType.Double:
                            {
                                r = (int)ar.dValue;
                                break;
                            }
                        case JSValueType.NotExistsInObject:
                        case JSValueType.Undefined:
                            {
                                r = 10;
                                break;
                            }
                        default:
                            {
                                r = Tools.JSObjectToInt32(ar);
                                break;
                            }
                    }
                }
                if (r < 2 || r > 36)
                    ExceptionsHelper.Throw((new TypeError("Radix must be between 2 and 36.")));
                if (r == 10)
                    return self.ToString();
                else
                {
                    long res = self.iValue;
                    var sres = new StringBuilder();
                    bool neg;
                    if (self.valueType == JSValueType.Double)
                    {
                        if (double.IsNaN(self.dValue))
                            return "NaN";
                        if (double.IsPositiveInfinity(self.dValue))
                            return "Infinity";
                        if (double.IsNegativeInfinity(self.dValue))
                            return "-Infinity";
                        res = (long)self.dValue;
                        if (res != self.dValue) // your bunny wrote
                        {
                            double dtemp = self.dValue;
                            neg = dtemp < 0;
                            if (neg)
                                dtemp = -dtemp;
                            sres.Append(Tools.NumChars[(int)(dtemp % r)]);
                            res /= r;
                            while (dtemp >= 1.0)
                            {
                                sres.Append(Tools.NumChars[(int)(dtemp % r)]);
                                dtemp /= r;
                            }
                            if (neg)
                                sres.Append('-');
                            for (int i = sres.Length - 1, j = 0; i > j; j++, i--)
                            {
                                sres[i] ^= sres[j];
                                sres[j] ^= sres[i];
                                sres[i] ^= sres[j];
                                sres[i] += (char)((sres[i] / 'A') * ('a' - 'A'));
                                sres[j] += (char)((sres[j] / 'A') * ('a' - 'A'));
                            }
                            return sres.ToString();
                        }
                    }
                    neg = res < 0;
                    if (neg)
                        res = -res;
                    if (res < 0)
                        ExceptionsHelper.Throw(new Error("Internal error"));
                    sres.Append(Tools.NumChars[res % r]);
                    res /= r;
                    while (res != 0)
                    {
                        sres.Append(Tools.NumChars[res % r]);
                        res /= r;
                    }
                    if (neg)
                        sres.Append('-');
                    for (int i = sres.Length - 1, j = 0; i > j; j++, i--)
                    {
                        sres[i] ^= sres[j];
                        sres[j] ^= sres[i];
                        sres[i] ^= sres[j];
                        sres[i] += (char)((sres[i] / 'A') * ('a' - 'A'));
                        sres[j] += (char)((sres[j] / 'A') * ('a' - 'A'));
                    }
                    return sres.ToString();
                }
            }
            finally
            {
                self.valueType = ovt;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue valueOf(JSValue self)
        {
            if (self is Number)
                return self.iValue == 0 ? self.dValue : self.iValue;
            if (self.valueType != JSValueType.Int && self.valueType != JSValueType.Double)
                ExceptionsHelper.Throw((new TypeError("Try to call Number.valueOf on not number object.")));
            return self;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType == JSValueType.Int)
                return Tools.Int32ToString(iValue);
            if (valueType == JSValueType.Double)
                return Tools.DoubleToString(dValue);
            if (iValue != 0)
                return Tools.Int32ToString(iValue);
            return Tools.DoubleToString(dValue);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return valueType == JSValueType.Int ? iValue.GetHashCode() : dValue.GetHashCode();
        }
#if !WRC
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
            return value == null ? 0 : value.valueType == JSValueType.Int ? value.iValue : value.dValue;
        }

        [Hidden]
        public static explicit operator int(Number value)
        {
            return value == null ? 0 : value.valueType == JSValueType.Int ? value.iValue : (int)value.dValue;
        }
#endif
        [DoNotEnumerate]
        public static JSValue isNaN(Arguments a)
        {
            switch (a[0].valueType)
            {
                case JSValueType.Double:
                    return double.IsNaN(a[0].dValue);
                default:
                    return Boolean.False;
            }
        }
    }
}