using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Modules
{
    internal sealed class Math
    {
        [Protected]
        public const double E = System.Math.E;

        public static JSObject pow(IContextStatement[] args)
        {
            if (args.Length < 2)
                return double.NaN;
            var r = args[0].Invoke();
            double x = double.NaN;
            if (r.ValueType == ObjectValueType.Int)
                x = r.iValue;
            else if (r.ValueType == ObjectValueType.Double)
                x = r.dValue;
            else if (r.ValueType == ObjectValueType.Bool)
                x = r.iValue;
            else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                return double.NaN;
            else if ((r.ValueType == ObjectValueType.Object) && (r.oValue is string))
            {
                int ix = 0;
                string s = r.oValue as string;
                Parser.ParseNumber(s, ref ix, false, out x);
            }

            r = args[1].Invoke();
            double y = double.NaN;
            if (r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Bool)
                y = r.iValue;
            else if (r.ValueType == ObjectValueType.Double)
                y = r.dValue;
            else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                return double.NaN;
            else if ((r.ValueType == ObjectValueType.Object) && (r.oValue is string))
            {
                int ix = 0;
                string s = r.oValue as string;
                Parser.ParseNumber(s, ref ix, false, out y);
            }

            return System.Math.Pow(x, y);
        }

        public static JSObject floor(IContextStatement[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            var r = args[0].Invoke();
            double x = double.NaN;
            if (r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Bool)
                x = r.iValue;
            else if (r.ValueType == ObjectValueType.Double)
                x = r.dValue;
            else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                return double.NaN;
            else if ((r.ValueType == ObjectValueType.String))
            {
                int ix = 0;
                string s = r.oValue as string;
                Parser.ParseNumber(s, ref ix, false, out x);
            }

            return System.Math.Floor(x);
        }

        public static JSObject round(IContextStatement[] args)
        {
            if (args.Length < 1)
                return double.NaN;
            var r = args[0].Invoke();
            double x = double.NaN;
            if (r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Bool)
                x = r.iValue;
            else if (r.ValueType == ObjectValueType.Double)
                x = r.dValue;
            else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                return double.NaN;
            else if ((r.ValueType == ObjectValueType.String))
            {
                int ix = 0;
                string s = r.oValue as string;
                Parser.ParseNumber(s, ref ix, false, out x);
            }

            return (int)System.Math.Round(x);
        }

        private static class _Random
        {
            public static Random random = new Random((int)DateTime.Now.Ticks);
        }

        public static JSObject random()
        {
            return _Random.random.NextDouble();
        }
    }
}
