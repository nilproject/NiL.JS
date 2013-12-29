using System;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal class String
    {
        [Hidden]
        private string value;
        [Hidden]
        private JSObject native;

        public String()
        {
            value = "";
        }

        public String(string s)
        {
            value = s;
        }

        public JSObject this[object pos]
        {
            get
            {
                int p = 0;
                if (pos is int)
                    p = (int)pos;
                else if (pos is double)
                    p = (int)(double)pos;
                else if (pos is string)
                {
                    double d = 0;
                    if (double.TryParse((string)pos, out d))
                        p = (int)d;
                    else
                        return JSObject.undefined;
                }
                if ((p < 0) || (p >= value.Length))
                    return JSObject.undefined;
                return value[p].ToString();
            }
        }

        public static String fromCharCode(object charCode)
        {
            int chc = 0;
            if (charCode is int)
                chc = (int)charCode;
            else if (charCode is double)
                chc = (int)(double)charCode;
            else if (charCode is string)
            {
                double d = 0;
                if (double.TryParse((string)charCode, out d))
                    chc = (int)d;
            }
            return new String(((char)chc).ToString());
        }

        public double charCodeAt(object pos)
        {
            int p = 0;
            if (pos is int)
                p = (int)pos;
            else if (pos is double)
                p = (int)(double)pos;
            else if (pos is string)
            {
                double d = 0;
                if (double.TryParse((string)pos, out d))
                    p = (int)d;
            }
            if ((p < 0) || (p >= value.Length))
                return double.NaN;
            return (int)value[p];
        }

        public JSObject toString()
        {
            (native ?? (native = new JSObject()
            {
                oValue = value,
                ValueType = ObjectValueType.String,
                assignCallback = JSObject.ErrorAssignCallback,
                fieldGetter = (x, y) => { throw new InvalidOperationException(); }
            })).oValue = value;
            return native;
        }

        [Hidden]
        private static JSObject _length = 0;

        public JSObject length
        {
            get
            {
                _length.iValue = value.Length;
                return _length;
            }
        }

        public override string ToString()
        {
            return value;
        }

        public override bool Equals(object obj)
        {
            if (obj is String)
                return value == (obj as String).value;
            return false;
        }
    }
}