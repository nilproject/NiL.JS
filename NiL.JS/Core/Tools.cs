using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public static class Tools
    {
        public static double jsobjectToDouble(JSObject arg)
        {
            if (arg == null)
                return double.NaN;
            var r = arg;
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
            return x;
        }
    }
}
