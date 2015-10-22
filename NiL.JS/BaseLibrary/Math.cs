using NiL.JS.BaseLibrary;
using System;
using NiL.JS.Core.Modules;
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
        public static JSObject abs(Arguments args)
        {
            var arg = args[0];
            switch (arg.valueType)
            {
                case JSObjectType.Int:
                    {
                        if ((arg.iValue & int.MinValue) == 0)
                            return arg;
                        if (arg.iValue == int.MinValue)
                            goto default;
                        if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                        {
                            arg.valueType = JSObjectType.Int;
                            arg.iValue = -arg.iValue;
                            return arg;
                        }
                        return -arg.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (arg.dValue >= 0)
                            return arg;
                        if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                        {
                            arg.valueType = JSObjectType.Double;
                            arg.dValue = -arg.dValue;
                            return arg;
                        }
                        return -arg.dValue;
                    }
                default:
                    {
                        var value = System.Math.Abs(Tools.JSObjectToDouble(arg));
                        if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
                        {
                            arg.valueType = JSObjectType.Double;
                            arg.dValue = value;
                            return arg;
                        }
                        return value;
                    }
            }
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject acos(Arguments args)
        {
            return System.Math.Acos(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
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

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsLength(2)]
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

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject ceil(Arguments args)
        {
            return System.Math.Ceiling(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject cos(Arguments args)
        {
            return System.Math.Cos(Tools.JSObjectToDouble(args.length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject exp(Arguments args)
        {
            var arg = args[0];
            var res = System.Math.Exp(Tools.JSObjectToDouble(arg));
            if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
            {
                arg.valueType = JSObjectType.Double;
                arg.dValue = res;
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
        public static JSObject floor(Arguments args)
        {
            var arg = args.a0 ?? JSObject.notExists;
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
                return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject log(Arguments args)
        {
            return System.Math.Log(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsLength(2)]
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
                    return Number.NaN;
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
        [ArgumentsLength(2)]
        public static JSObject min(Arguments args)
        {
            double res = double.PositiveInfinity;
            for (int i = 0; i < args.Length; i++)
            {
                var t = Tools.JSObjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return Number.NaN;
                res = System.Math.Min(res, t);
            }
            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsLength(2)]
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

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject random()
        {
            return randomInstance.NextDouble();
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject round(Arguments args)
        {
            var arg = args[0];
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
                return double.IsNaN(a) ? Number.NaN : a;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject sin(Arguments args)
        {
            return System.Math.Sin(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject sqrt(Arguments args)
        {
            var arg = args[0];
            var res = System.Math.Sqrt(Tools.JSObjectToDouble(arg));
            if ((arg.attributes & JSObjectAttributesInternal.Cloned) != 0)
            {
                arg.valueType = JSObjectType.Double;
                arg.dValue = res;
                return arg;
            }
            return res;
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject tan(Arguments args)
        {
            return System.Math.Tan(Tools.JSObjectToDouble(args.Length > 0 ? args[0] : null));
        }

        #region Exclusives

        [DoNotDelete]
        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSObject IEEERemainder(Arguments args)
        {
            if (args.Length < 2)
                return Number.NaN;
            return System.Math.IEEERemainder(Tools.JSObjectToDouble(args[0]), Tools.JSObjectToDouble(args[1]));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject sign(Arguments args)
        {
            if (args.Length < 1)
                return Number.NaN;
            return System.Math.Sign(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject sinh(Arguments args)
        {
            if (args.Length < 1)
                return Number.NaN;
            return System.Math.Sinh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject tanh(Arguments args)
        {
            if (args.Length < 1)
                return Number.NaN;
            return System.Math.Tanh(Tools.JSObjectToDouble(args[0]));
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public static JSObject trunc(Arguments args)
        {
            if (args.Length < 1)
                return Number.NaN;
            return System.Math.Truncate(Tools.JSObjectToDouble(args[0]));
        }

        #endregion
    }
}
