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
        public static JSValue abs(Arguments args)
        {
            var arg = args[0];
            switch (arg._valueType)
            {
                case JSValueType.Integer:
                    {
                        if ((arg._iValue & int.MinValue) == 0)
                            return arg;
                        if (arg._iValue == int.MinValue)
                            goto default;
                        if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                        {
                            arg._valueType = JSValueType.Integer;
                            arg._iValue = -arg._iValue;
                            return arg;
                        }
                        return -arg._iValue;
                    }
                case JSValueType.Double:
                    {
                        if (arg._dValue >= 0)
                            return arg;
                        if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                        {
                            arg._valueType = JSValueType.Double;
                            arg._dValue = -arg._dValue;
                            return arg;
                        }
                        return -arg._dValue;
                    }
                default:
                    {
                        var value = System.Math.Abs(Tools.JSObjectToDouble(arg));
                        if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                        {
                            arg._valueType = JSValueType.Double;
                            arg._dValue = value;
                            return arg;
                        }
                        return value;
                    }
            }
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue acos(Arguments args)
        {
            return System.Math.Acos(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue asin(Arguments args)
        {
            return System.Math.Asin(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue atan(Arguments args)
        {
            return System.Math.Atan(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsLength(2)]
        public static JSValue atan2(Arguments args)
        {
            if (args.Length < 2)
                return double.NaN;
            var a = Tools.JSObjectToDouble(args[0]);
            var b = Tools.JSObjectToDouble(args[1]);
            if (double.IsInfinity(a)
                && double.IsInfinity(b))
                return System.Math.Atan2(System.Math.Sign(a), System.Math.Sign(b));
            return System.Math.Atan2(a, b);
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue ceil(Arguments args)
        {
            return System.Math.Ceiling(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue cos(Arguments args)
        {
            return System.Math.Cos(Tools.JSObjectToDouble(args.length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue exp(Arguments args)
        {
            var arg = args[0];
            var res = System.Math.Exp(Tools.JSObjectToDouble(arg));
            if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                arg._valueType = JSValueType.Double;
                arg._dValue = res;
                return arg;
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
        public static JSValue floor(Arguments args)
        {
            var arg = args[0];
            if (arg._valueType == JSValueType.Integer)
                return arg;
            var a = Tools.JSObjectToDouble(arg);
            if (a == 0.0)
            {
                if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    arg._valueType = JSValueType.Integer;
                    arg._iValue = 0;
                    return arg;
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
                if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        arg._valueType = JSValueType.Integer;
                        arg._iValue = (int)r;
                    }
                    else
                    {
                        arg._valueType = JSValueType.Double;
                        arg._dValue = r;
                    }

                    return arg;
                }

                return r;
            }

            return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue log(Arguments args)
        {
            return System.Math.Log(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsLength(2)]
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
        [ArgumentsLength(2)]
        public static JSValue min(Arguments args)
        {
            double res = double.PositiveInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return double.NaN;
                res = System.Math.Min(res, t);
            }
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsLength(2)]
        public static JSValue pow(Arguments args)
        {
            JSValue result = 0;
            if (args.Length < 2)
                result._dValue = double.NaN;
            else
            {
                var @base = Tools.JSObjectToDouble(args[0]);
                var degree = Tools.JSObjectToDouble(args[1]);
                if (@base == 1 && double.IsInfinity(degree))
                    result._dValue = double.NaN;
                else if (double.IsNaN(@base) && degree == 0.0)
                    result._dValue = 1;
                else
                    result._dValue = System.Math.Pow(@base, degree);
            }
            result._valueType = JSValueType.Double;
            return result;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue random()
        {
            return randomInstance.NextDouble();
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSValue round(Arguments args)
        {
            var arg = args[0];
            if (arg._valueType == JSValueType.Integer)
                return arg;

            var a = Tools.JSObjectToDouble(arg);
            if (a == 0.0)
            {
                if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    arg._valueType = JSValueType.Integer;
                    arg._iValue = 0;
                    return arg;
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
                if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        arg._valueType = JSValueType.Integer;
                        arg._iValue = (int)r;
                    }
                    else
                    {
                        arg._valueType = JSValueType.Double;
                        arg._dValue = r;
                    }

                    return arg;
                }

                return r;
            }

            return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sin(Arguments args)
        {
            return System.Math.Sin(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sqrt(Arguments args)
        {
            var arg = args[0];
            var res = System.Math.Sqrt(Tools.JSObjectToDouble(arg));
            if ((arg._attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                arg._valueType = JSValueType.Double;
                arg._dValue = res;
                return arg;
            }
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue tan(Arguments args)
        {
            return System.Math.Tan(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        #region Exclusives

        [DoNotEnumerate]
        [DoNotDelete]
        [ArgumentsLength(2)]
        public static JSValue IEEERemainder(Arguments args)
        {
            if (args.Length < 2)
                return double.NaN;
            return System.Math.IEEERemainder(Tools.JSObjectToDouble(args[0]), Tools.JSObjectToDouble(args[1]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sign(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sign(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue sinh(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sinh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue tanh(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Tanh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSValue trunc(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Truncate(Tools.JSObjectToDouble(args[0]));
        }

        #endregion
    }
}
