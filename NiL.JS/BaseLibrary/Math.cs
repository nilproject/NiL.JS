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
        public static JSValue asin(JSValue value)
        {
            return System.Math.Asin(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue atan(JSValue value)
        {
            return System.Math.Atan(Tools.JSObjectToDouble(value));
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
        public static JSValue ceil(JSValue value)
        {
            return System.Math.Ceiling(Tools.JSObjectToDouble(value));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue cos(JSValue value)
        {
            return System.Math.Cos(Tools.JSObjectToDouble(value));
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

            if (e > 0) // есть что округлить
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

        #region Exclusives

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsCount(2)]
        public static JSValue IEEERemainder(JSValue a, JSValue b)
        {
            return System.Math.IEEERemainder(Tools.JSObjectToDouble(a), Tools.JSObjectToDouble(b));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sign(JSValue value)
        {
            var res = System.Math.Sign(Tools.JSObjectToDouble(value));
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

        #endregion
    }
}
