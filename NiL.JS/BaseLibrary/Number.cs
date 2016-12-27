using System;
using System.Globalization;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
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
            POSITIVE_INFINITY._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            NEGATIVE_INFINITY._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            MAX_VALUE._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            MIN_VALUE._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
            NaN._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Number()
        {
            _valueType = JSValueType.Integer;
            _iValue = 0;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        [StrictConversion]
        public Number(int value)
        {
            _valueType = JSValueType.Integer;
            _iValue = value;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [Hidden]
        public Number(long value)
        {
            if ((long)(int)(value) == value)
            {
                _valueType = JSValueType.Integer;
                _iValue = (int)value;
            }
            else
            {
                _valueType = JSValueType.Double;
                _dValue = value;
            }
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        [StrictConversion]
        public Number(double value)
        {
            _valueType = JSValueType.Double;
            _dValue = value;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        [StrictConversion]
        public Number(string value)
        {
            value = (value ?? "0").Trim(Tools.TrimChars);
            _valueType = JSValueType.Integer;
            _dValue = value.Length != 0 ? double.NaN : 0;
            _valueType = JSValueType.Double;
            double d = 0;
            int i = 0;
            if (value.Length != 0 && Tools.ParseNumber(value, ref i, out d, 0, ParseNumberOptions.Default | (Context.CurrentContext._strict ? ParseNumberOptions.RaiseIfOctal : 0)) && i == value.Length)
                _dValue = d;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Number(Arguments obj)
        {
            _valueType = JSValueType.Double;
            _dValue = Tools.JSObjectToDouble(obj[0]);
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
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
            switch (self._valueType)
            {
                case JSValueType.Integer:
                    {
                        res = self._iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        res = self._dValue;
                        break;
                    }
                case JSValueType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            ExceptionHelper.Throw((new TypeError("Try to call Number.toExponential on not number object.")));
                        res = self._iValue == 0 ? self._dValue : self._iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSValue.undefined)._valueType)
            {
                case JSValueType.Integer:
                    {
                        dgts = digits._iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        dgts = (int)digits._dValue;
                        break;
                    }
                case JSValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Tools.ParseNumber(digits._oValue.ToString(), i, out d, ParseNumberOptions.Default))
                            dgts = (int)d;
                        break;
                    }
                case JSValueType.Object:
                    {
                        var d = digits[0].ToPrimitiveValue_Value_String();
                        if (d._valueType == JSValueType.String)
                            goto case JSValueType.String;
                        if (d._valueType == JSValueType.Integer)
                            goto case JSValueType.Integer;
                        if (d._valueType == JSValueType.Double)
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
        [ArgumentsCount(1)]
        public static JSValue toFixed(JSValue self, Arguments digits)
        {
            double res = 0;
            switch (self._valueType)
            {
                case JSValueType.Integer:
                    {
                        res = self._iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        res = self._dValue;
                        break;
                    }
                case JSValueType.Object:
                    {
                        if (typeof(Number) != self.GetType())
                            ExceptionHelper.Throw((new TypeError("Try to call Number.toFixed on not number object.")));
                        res = self._iValue == 0 ? self._dValue : self._iValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = Tools.JSObjectToInt32(digits[0], true);
            if (dgts < 0 || dgts > 20)
                ExceptionHelper.Throw((new RangeError("toFixed() digits argument must be between 0 and 20")));
            if (System.Math.Abs(self._dValue) >= 1e+21)
                return self._dValue.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            if (dgts > 0)
                dgts++;
            return System.Math.Round(res, dgts).ToString("0.00000000000000000000".Substring(0, dgts + 1), System.Globalization.CultureInfo.InvariantCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLocaleString(JSValue self)
        {
            return self._valueType == JSValueType.Integer ? self._iValue.ToString(System.Globalization.CultureInfo.CurrentCulture) : self._dValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        [InstanceMember]
        [DoNotEnumerate]
        [CLSCompliant(false)]
        public static JSValue toString(JSValue self, Arguments radix)
        {
            var ovt = self._valueType;
            if (self._valueType > JSValueType.Double && self is Number)
                self._valueType = self._dValue == 0.0 ? JSValueType.Integer : JSValueType.Double;
            try
            {
                if (self._valueType != JSValueType.Integer && self._valueType != JSValueType.Double)
                    ExceptionHelper.Throw((new TypeError("Try to call Number.toString on not Number object")));
                int r = 10;
                if (radix != null && radix.GetProperty("length")._iValue > 0)
                {
                    var ar = radix[0];
                    if (ar._valueType == JSValueType.Object && ar._oValue == null)
                        ExceptionHelper.Throw((new Error("Radix can't be null.")));
                    switch (ar._valueType)
                    {
                        case JSValueType.Integer:
                        case JSValueType.Boolean:
                            {
                                r = ar._iValue;
                                break;
                            }
                        case JSValueType.Double:
                            {
                                r = (int)ar._dValue;
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
                    ExceptionHelper.Throw((new TypeError("Radix must be between 2 and 36.")));
                if (r == 10)
                    return self.ToString();
                else
                {
                    long res = self._iValue;
                    var sres = new StringBuilder();
                    bool neg;
                    if (self._valueType == JSValueType.Double)
                    {
                        if (double.IsNaN(self._dValue))
                            return "NaN";
                        if (double.IsPositiveInfinity(self._dValue))
                            return "Infinity";
                        if (double.IsNegativeInfinity(self._dValue))
                            return "-Infinity";
                        res = (long)self._dValue;
                        if (res != self._dValue) // your bunny wrote
                        {
                            double dtemp = self._dValue;
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
                        ExceptionHelper.Throw(new Error("Internal error"));
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
                self._valueType = ovt;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue valueOf(JSValue self)
        {
            if (self is Number)
                return self._iValue == 0 ? self._dValue : self._iValue;
            if (self._valueType != JSValueType.Integer && self._valueType != JSValueType.Double)
                ExceptionHelper.Throw((new TypeError("Try to call Number.valueOf on not number object.")));
            return self;
        }

        [Hidden]
        public override string ToString()
        {
            if (_valueType == JSValueType.Integer)
                return Tools.Int32ToString(_iValue);
            if (_valueType == JSValueType.Double)
                return Tools.DoubleToString(_dValue);
            if (_iValue != 0)
                return Tools.Int32ToString(_iValue);
            return Tools.DoubleToString(_dValue);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return _valueType == JSValueType.Integer ? _iValue.GetHashCode() : _dValue.GetHashCode();
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
            return value == null ? 0 : value._valueType == JSValueType.Integer ? value._iValue : value._dValue;
        }

        [Hidden]
        public static explicit operator int(Number value)
        {
            return value == null ? 0 : value._valueType == JSValueType.Integer ? value._iValue : (int)value._dValue;
        }
#endif
        [DoNotEnumerate]
        public static JSValue isNaN(JSValue x)
        {
            switch (x._valueType)
            {
                case JSValueType.Double:
                    return double.IsNaN(x._dValue);
                default:
                    return Boolean.False;
            }
        }
    }
}