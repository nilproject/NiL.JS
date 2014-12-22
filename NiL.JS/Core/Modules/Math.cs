using System;

namespace NiL.JS.Core.Modules
{
    public static class Math
    {
        [Hidden]
        internal readonly static Random randomInstance = new Random((int)DateTime.Now.Ticks);

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public const double E = System.Math.E;
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public const double PI = System.Math.PI;
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double LN2 = System.Math.Log(2);
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double LN10 = System.Math.Log(10);
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double LOG2E = 1.0 / LN2;
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double LOG10E = 1.0 / LN10;
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double SQRT1_2 = System.Math.Sqrt(0.5);
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public static readonly double SQRT2 = System.Math.Sqrt(2);

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject abs(Arguments args)
        {
            return System.Math.Abs(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject acos(Arguments args)
        {
            return System.Math.Acos(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject asin(Arguments args)
        {
            return System.Math.Asin(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject atan(Arguments args)
        {
            return System.Math.Atan(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ParametersCount(2)]
        public static JSObject atan2(Arguments args)
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
        public static JSObject ceil(Arguments args)
        {
            return System.Math.Ceiling(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject cos(Arguments args)
        {
            return System.Math.Cos(Tools.JSObjectToDouble(args.length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject exp(Arguments args)
        {
            return System.Math.Exp(Tools.JSObjectToDouble(args.length > 0 ? args[0] : null));
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
        public static JSObject floor(JSObject arg)
        {
            if (arg.valueType == JSObjectType.Int)
                return arg;
            var a = Tools.JSObjectToDouble(arg);
            if (a == 0.0)
            {
                if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                {
                    arg.valueType = JSObjectType.Int;
                    arg.iValue = 0;
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
                if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        arg.valueType = JSObjectType.Int;
                        arg.iValue = (int)r;
                    }
                    else
                    {
                        arg.valueType = JSObjectType.Double;
                        arg.dValue = r;
                    }
                    return arg;
                }
                return r;
            }
            else
                return double.IsNaN(a) ? BaseTypes.Number.NaN : a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject log(Arguments args)
        {
            return System.Math.Log(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [ParametersCount(2)]
        public static JSObject max(Arguments args)
        {
            JSObject reso = null;
            double res = double.NegativeInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                if (reso == null && (args[i].attributes & JSObjectAttributesInternal.Cloned) != 0)
                    reso = args[i];
                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return BaseTypes.Number.NaN;
                res = System.Math.Max(res, t);
            }
            if (reso != null)
            {
                reso.valueType = JSObjectType.Double;
                reso.dValue = res;
                return reso;
            }
            return res;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ParametersCount(2)]
        public static JSObject min(Arguments args)
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
        [ParametersCount(2)]
        public static JSObject pow(Arguments args)
        {
            JSObject result = 0;
            if (args.Length < 2)
                result.dValue = double.NaN;
            else
            {
                var @base = Tools.JSObjectToDouble(args[0]);
                var degree = Tools.JSObjectToDouble(args[1]);
                if (@base == 1 && double.IsInfinity(degree))
                    result.dValue = double.NaN;
                else if (double.IsNaN(@base) && degree == 0.0)
                    result.dValue = 1;
                else
                    result.dValue = System.Math.Pow(@base, degree);
            }
            result.valueType = JSObjectType.Double;
            return result;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject random()
        {
            return randomInstance.NextDouble();
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject round(JSObject arg)
        {
            if (arg.valueType == JSObjectType.Int)
                return arg;
            var a = Tools.JSObjectToDouble(arg);
            if (a == 0.0)
            {
                if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                {
                    arg.valueType = JSObjectType.Int;
                    arg.iValue = 0;
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
                if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                {
                    if (r <= int.MaxValue)
                    {
                        arg.valueType = JSObjectType.Int;
                        arg.iValue = (int)r;
                    }
                    else
                    {
                        arg.valueType = JSObjectType.Double;
                        arg.dValue = r;
                    }
                    return arg;
                }
                return r;
            }
            else
                return double.IsNaN(a) ? BaseTypes.Number.NaN : a;
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject sin(Arguments args)
        {
            return System.Math.Sin(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject sqrt(Arguments args)
        {
            return System.Math.Sqrt(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject tan(Arguments args)
        {
            return System.Math.Tan(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        #region Exclusives

        [DoNotEnumerate]
        [DoNotDelete]
        [ParametersCount(2)]
        public static JSObject IEEERemainder(Arguments args)
        {
            if (args.Length < 2)
                return double.NaN;
            return System.Math.IEEERemainder(Tools.JSObjectToDouble(args[0]), Tools.JSObjectToDouble(args[1]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject sign(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sign(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject sinh(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sinh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject tanh(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Tanh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject trunc(Arguments args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Truncate(Tools.JSObjectToDouble(args[0]));
        }

        #endregion
    }
}
