using System;
using NiL.JS.Core.Interop;
using NiL.JS.Core;

namespace NiL.JS.BaseLibrary
{
    public static class Math
    {
        [Hidden]
        internal readonly static Random randomInstance = new Random((int)DateTime.Now.Ticks);

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number E = new Number(System.Math.E);
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number PI = new Number(System.Math.PI);
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number LN2 = new Number(System.Math.Log(2));
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number LN10 = new Number(System.Math.Log(10));
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number LOG2E = new Number(1.0 / System.Math.Log(2));
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number LOG10E = new Number(1.0 / System.Math.Log(10));
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number SQRT1_2 = new Number(System.Math.Sqrt(0.5));
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly Number SQRT2 = new Number(System.Math.Sqrt(2));

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue abs(JSValue value)
        {
            switch (value._valueType)
            {
                case JSValueType.Integer:
                    {
                        if (value._iValue >= 0)
                            return value;

                        value = value.CloneImpl(false);

                        if (value._iValue == int.MinValue)
                        {
                            value._valueType = JSValueType.Double;
                            value._dValue = -(double)value._iValue;
                        }
                        else
                        {
                            value._iValue = -value._iValue;
                        }

                        return value;
                    }
                case JSValueType.Double:
                    {
                        if (value._dValue > 0
                            || (value._dValue == 0 && double.IsPositiveInfinity(1.0 / value._dValue)))
                            return value;

                        value = value.CloneImpl(false);

                        value._dValue = -value._dValue;

                        return value;
                    }
                default:
                    {
                        return System.Math.Abs(Tools.JSObjectToDouble(value));
                    }
            }
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue acos(JSValue value)
        {
            return System.Math.Acos(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue acosh(JSValue value)
        {
            var res = Tools.JSObjectToDouble(value);
            if (res < 1.0)
                res = double.NaN;
            else
                res = System.Math.Log(res + System.Math.Sqrt(res * res - 1.0));
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue asin(JSValue value)
        {
            return System.Math.Asin(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue asinh(JSValue value)
        {
            var res = Tools.JSObjectToDouble(value);
            return System.Math.Log(res + System.Math.Sqrt(res * res + 1.0));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue atan(JSValue value)
        {
            return System.Math.Atan(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue atanh(JSValue value)
        {
            var res = Tools.JSObjectToDouble(value);
            if (System.Math.Abs(res) >= 1.0)
                res = double.NaN;
            else
                res = 0.5 * System.Math.Log((1.0 + res) / (1.0 - res));
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue atan2(JSValue x, JSValue y)
        {
            if (!x.Defined || !y.Defined)
                return double.NaN;

            var a = Tools.JSObjectToDouble(x);
            var b = Tools.JSObjectToDouble(y);
            if (double.IsInfinity(a)
                && double.IsInfinity(b))
                return System.Math.Atan2(System.Math.Sign(a), System.Math.Sign(b));

            return System.Math.Atan2(a, b);
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue cbrt(JSValue value)
        {
            double x = Tools.JSObjectToDouble(value);
            if (double.IsNaN(x))
                return Number.NaN;
            var res = System.Math.Pow(System.Math.Abs(x), 1.0 / 3.0);
            if (x < 0.0)
                res = -res;
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue ceil(JSValue value)
        {
            return System.Math.Ceiling(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue clz32(JSValue value)
        {
            var x = (uint)Tools.JSObjectToInt32(value, 0, 0, false);

            if (x < 0)
                return 0;
            if (x == 0)
                return 32;

            var res = 0;
            var shift = 16;
            while (x > 1)
            {
                var aspt = x >> shift;
                if (aspt != 0)
                {
                    x = aspt;
                    res += shift;
                }

                shift >>= 1;
            }

            return 31 - res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue cos(JSValue value)
        {
            return System.Math.Cos(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue cosh(JSValue value)
        {
            return System.Math.Cosh(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue exp(JSValue value)
        {
            var res = System.Math.Exp(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue expm1(JSValue value)
        {
            var x = Tools.JSObjectToDouble(value);
            var res = 0.0;
            if (x >= -1.0 && x <= 1.0) // for better accuracy
            {
                double f = 1.0;
                double p = x;
                double s = x;
                int i = 2;
                while (res != s)
                {
                    res = s;
                    p *= x;
                    f *= i++;
                    s += p / f;
                }
            }
            else
                res = System.Math.Exp(x) - 1.0;

            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        private static ulong shl(ulong x, int y)
        {
            while (y > 0)
            {
                x >>= System.Math.Min(y, 62);
                y -= 62;
            }
            return x;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue floor(JSValue value)
        {
            if (value._valueType == JSValueType.Integer)
                return value;
            var a = Tools.JSObjectToDouble(value);
            if (a == 0.0)
            {
                if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    value._valueType = JSValueType.Integer;
                    value._iValue = 0;
                    return value;
                }
                return a;
            }
            var b = BitConverter.DoubleToInt64Bits(a);
            ulong m = ((ulong)b & ((1UL << 52) - 1)) | (1UL << 52);
            int e = 0;
            long s = (b >> 63) | 1L;
            b &= long.MaxValue;
            e |= (int)(b >> 52);
            e = 52 - e + 1023;

            if (e > 0)
            {
                if (s < 0)
                {
                    if ((e > 64) || (m & ((1UL << e) - 1UL)) != 0)
                        return -(long)shl(m, e) - 1;
                    return -(long)shl(m, e);
                }

                var r = (long)shl(m, e) * s;
                if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        value._valueType = JSValueType.Integer;
                        value._iValue = (int)r;
                    }
                    else
                    {
                        value._valueType = JSValueType.Double;
                        value._dValue = r;
                    }

                    return value;
                }

                return r;
            }

            return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue fround(JSValue value)
        {
            var res = (double)((float)Tools.JSObjectToDouble(value));
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue hypot(Arguments args)
        {
            JSValue reso = null;
            double res = 0.0;
            for (int i = 0; i < args.Length; i++)
            {
                if (reso == null && (args[i]._attributes & JSValueAttributesInternal.Cloned) != 0)
                    reso = args[i];

                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsInfinity(t))
                {
                    res = double.PositiveInfinity;
                    break;
                }
                res += t * t;
            }
            res = System.Math.Sqrt(res);

            if (reso != null)
            {
                reso._valueType = JSValueType.Double;
                reso._dValue = res;
                return reso;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue imul(JSValue x, JSValue y)
        {
            int a = Tools.JSObjectToInt32(x, 0, 0, false);
            int b = Tools.JSObjectToInt32(y, 0, 0, false);
            var res = unchecked(a * b);
            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue log(JSValue value)
        {
            var res = System.Math.Log(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue log1p(JSValue value)
        {
            double x = Tools.JSObjectToDouble(value);
            double res = 0.0;
            if (x >= -0.25 && x <= 0.25) // for better accuracy
            {
                double p = x;
                double s = x;
                int i = 1;
                while (res != s)
                {
                    res = s;
                    s -= (p *= x) / ++i;
                    s += (p *= x) / ++i;
                }
                return res;
            }
            else
                res = System.Math.Log(x + 1.0);

            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue log10(JSValue value)
        {
            var res = System.Math.Log10(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue log2(JSValue value)
        {
            // 1.442... = 1 / ln(2)
            var res = System.Math.Log(Tools.JSObjectToDouble(value)) * 1.4426950408889634073599246810019d;
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsCount(2)]
        public static JSValue max(Arguments args)
        {
            JSValue reso = null;
            double res = double.NegativeInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                if (reso == null && (args[i]._attributes & JSValueAttributesInternal.Cloned) != 0)
                    reso = args[i];

                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return Number.NaN;

                res = System.Math.Max(res, t);
            }

            if (reso != null)
            {
                reso._valueType = JSValueType.Double;
                reso._dValue = res;
                return reso;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue min(Arguments args)
        {
            JSValue reso = null;
            double res = double.PositiveInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                if (reso == null && (args[i]._attributes & JSValueAttributesInternal.Cloned) != 0)
                    reso = args[i];

                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return Number.NaN;

                res = System.Math.Min(res, t);
            }

            if (reso != null)
            {
                reso._valueType = JSValueType.Double;
                reso._dValue = res;
                return reso;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue pow(JSValue a, JSValue b)
        {
            if (!a.Defined || !b.Defined)
            {
                return Number.NaN;
            }
            else
            {
                var @base = Tools.JSObjectToDouble(a);
                var degree = Tools.JSObjectToDouble(b);
                if (@base == 1 && double.IsInfinity(degree))
                    return Number.NaN;
                else if (double.IsNaN(@base) && degree == 0.0)
                    return 1;
                else
                    return System.Math.Pow(@base, degree);
            }
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue random()
        {
            return randomInstance.NextDouble();
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue round(JSValue value)
        {
            if (value._valueType == JSValueType.Integer)
                return value;

            var a = Tools.JSObjectToDouble(value);
            if (a == 0.0)
            {
                if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    value._valueType = JSValueType.Integer;
                    value._iValue = 0;
                    return value;
                }

                return a;
            }

            var b = BitConverter.DoubleToInt64Bits(a);
            ulong m = ((ulong)b & ((1UL << 52) - 1)) | (1UL << 52);
            int e = 0;
            long s = (b >> 63) | 1L;
            b &= long.MaxValue;
            e = (int)(b >> 52);
            e = 52 - e + 1023;

            if (e > 0)
            {
                if (s < 0)
                {
                    if ((shl(m, (e - 1)) & 1) == 1)
                    {
                        if ((m & ((1UL << (e - 1)) - 1UL)) != 0)
                            return -(long)shl(m, e) - 1;
                        return -(long)shl(m, e);
                    }
                }

                var r = ((long)shl(m, e) + ((long)shl(m, (e - 1)) & 1) * s) * s;
                if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        value._valueType = JSValueType.Integer;
                        value._iValue = (int)r;
                    }
                    else
                    {
                        value._valueType = JSValueType.Double;
                        value._dValue = r;
                    }

                    return value;
                }

                return r;
            }

            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = a;
                return value;
            }

            return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sign(JSValue value)
        {
            var res = Tools.JSObjectToDouble(value);
            if (!double.IsNaN(res))
                res = System.Math.Sign(res);

            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sin(JSValue value)
        {
            var res = System.Math.Sin(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sinh(JSValue value)
        {
            var res = System.Math.Sinh(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sqrt(JSValue value)
        {
            var res = System.Math.Sqrt(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue tan(JSValue value)
        {
            var res = System.Math.Tan(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue tanh(JSValue value)
        {
            var res = System.Math.Tanh(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue trunc(JSValue value)
        {
            var res = System.Math.Truncate(Tools.JSObjectToDouble(value));
            if ((value._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                value._valueType = JSValueType.Double;
                value._dValue = res;
                return value;
            }

            return res;
        }

        #region Exclusives

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue IEEERemainder(JSValue a, JSValue b)
        {
            return System.Math.IEEERemainder(Tools.JSObjectToDouble(a), Tools.JSObjectToDouble(b));
        }

        #endregion
    }
}
