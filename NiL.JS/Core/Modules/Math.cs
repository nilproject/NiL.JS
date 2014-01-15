using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core.Modules
{
    internal static class Math
    {
        private static class _Random
        {
            public readonly static Random random = new Random((int)DateTime.Now.Ticks);
        }

        [Protected]
        public static readonly double E = System.Math.E;
        [Protected]
        public static readonly double LN2 = System.Math.Log(2);
        [Protected]
        public static readonly double LN10 = System.Math.Log(10);
        [Protected]
        public static readonly double LOG2E = 1.0 / System.Math.Log(2);
        [Protected]
        public static readonly double LOG10E = 1.0 / System.Math.Log(10);
        [Protected]
        public static readonly double PI = System.Math.PI;
        [Protected]
        public static readonly double SQRT1_2 = System.Math.Sqrt(0.5);
        [Protected]
        public static readonly double SQRT2 = System.Math.Sqrt(2);

        public static JSObject abs(JSObject[] args)
        {
            return System.Math.Abs(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject acos(JSObject[] args)
        {
            return System.Math.Acos(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject asin(JSObject[] args)
        {
            return System.Math.Asin(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject atan(JSObject[] args)
        {
            return System.Math.Atan(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject atan2(JSObject[] args)
        {
            if (args.Length < 2)
                return double.NaN;
            return System.Math.Atan2(Tools.jsobjectToDouble(args[0]), Tools.jsobjectToDouble(args[1]));
        }

        public static JSObject ceil(JSObject[] args)
        {
            return System.Math.Ceiling(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject cos(JSObject[] args)
        {
            return System.Math.Cos(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject exp(JSObject[] args)
        {
            return System.Math.Exp(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject floor(JSObject[] args)
        {
            return System.Math.Floor(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject log(JSObject[] args)
        {
            return System.Math.Log(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject max(JSObject[] args)
        {
            if (args.Length == 0)
                return double.NaN;
            double res = double.MinValue;
            for (int i = 0; i < args.Length; i++)
            {
                var t = Tools.jsobjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return double.NaN;
                res = System.Math.Max(res, t);
            }
            return res;
        }

        public static JSObject min(JSObject[] args)
        {
            if (args.Length == 0)
                return double.NaN;
            double res = double.MinValue;
            for (int i = 0; i < args.Length; i++)
            {
                var t = Tools.jsobjectToDouble(args[i]);
                if (double.IsNaN(t))
                    return double.NaN;
                res = System.Math.Min(res, t);
            }
            return res;
        }

        public static JSObject pow(JSObject[] args)
        {
            if (args.Length < 2)
                return double.NaN;
            return System.Math.Pow(Tools.jsobjectToDouble(args[0]), Tools.jsobjectToDouble(args[1]));
        }

        public static JSObject random()
        {
            return _Random.random.NextDouble();
        }

        public static JSObject round(JSObject[] args)
        {
            return (int)System.Math.Round(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject sin(JSObject[] args)
        {
            return System.Math.Sin(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject sqrt(JSObject[] args)
        {
            return System.Math.Sqrt(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        public static JSObject tan(JSObject[] args)
        {
            return System.Math.Tan(Tools.jsobjectToDouble(args.Length > 0 ? args[0] : null));
        }

        #region Exclusives

        public static JSObject IEEERemainder(JSObject[] args)
        {
            if (args.Length < 2)
                return double.NaN;
            return System.Math.IEEERemainder(Tools.jsobjectToDouble(args[0]), Tools.jsobjectToDouble(args[1]));
        }

        public static JSObject sign(JSObject[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sign(Tools.jsobjectToDouble(args[0]));
        }

        public static JSObject sinh(JSObject[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Sinh(Tools.jsobjectToDouble(args[0]));
        }

        public static JSObject tanh(JSObject[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Tanh(Tools.jsobjectToDouble(args[0]));
        }

        public static JSObject trunc(JSObject[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            return System.Math.Truncate(Tools.jsobjectToDouble(args[0]));
        }

        #endregion
    }
}
