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
                x >>= y % 64;
                y -= 64;
            }
            return x;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject floor(Arguments args)
        {
            var a = args.Length > 0 ? Tools.JSObjectToDouble(args[0]) : double.NaN;
            if (a == 0.0)
                return a;
            var b = BitConverter.DoubleToInt64Bits(a);
            ulong m = ((ulong)b & ((1UL << 52) - 1)) | (1UL << 52);
            int e = 0;
            long s = (b >> 63) | 1;
            unchecked { b &= ((1L << 63) - 1L); }
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
                return (long)shl(m, e) * s;
            }
            else
                return a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject log(Arguments args)
        {
            return System.Math.Log(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotEnumerate]
        [DoNotDelete]
        [ParametersCount(2)]
        public static JSObject max(Arguments args)
        {
            double res = double.NegativeInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return double.NaN;
                res = System.Math.Max(res, t);
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

        [DoNotEnumerate]
        [DoNotDelete]
        public static JSObject round(Arguments args)
        {
            var a = args.Length > 0 ? Tools.JSObjectToDouble(args[0]) : double.NaN;
            var b = BitConverter.DoubleToInt64Bits(a);
            ulong m = ((ulong)b & ((1UL << 52) - 1)) | (1UL << 52);
            int e = 0;
            long s = (b >> 63) | 1;
            unchecked { b &= ((1L << 63) - 1L); }
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
                return ((long)shl(m, e) + ((long)shl(m, (e - 1)) & 1) * s) * s;
            }
            else
                return a;
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
