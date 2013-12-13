using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Modules
{
    internal sealed class Math
    {
        [Protected]
        public const double E = System.Math.E;

        public JSObject pow(IContextStatement[] args)
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
                if (!double.TryParse(s, out x) && (s.Length > 2) && (s[0] == '0') && (s[1] == 'x'))
                    if (int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ix))
                        x = ix;
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
                if (!double.TryParse(s, out x) && (s.Length > 2) && (s[0] == '0') && (s[1] == 'x'))
                    if (int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ix))
                        y = ix;
            }

            return System.Math.Pow(x, y);
        }

        public JSObject floor(IContextStatement[] args)
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
            else if ((r.ValueType == ObjectValueType.Object) && (r.oValue is string))
            {
                int ix = 0;
                string s = r.oValue as string;
                if (!double.TryParse(s, out x) && (s.Length > 2) && (s[0] == '0') && (s[1] == 'x'))
                    if (int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ix))
                        x = ix;
            }

            return System.Math.Floor(x);
        }
    }
}
