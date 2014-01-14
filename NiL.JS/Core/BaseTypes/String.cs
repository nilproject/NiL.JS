using System;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    internal class String : EmbeddedType
    {
        public String()
        {
            oValue = "";
            ValueType = ObjectValueType.String;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public String(string s)
        {
            oValue = s;
            ValueType = ObjectValueType.String;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public String(JSObject[] s)
        {
            if (s.Length > 0)
                oValue = s[0].Value.ToString();
            else
                oValue = "";
            ValueType = ObjectValueType.String;
            assignCallback = JSObject.ErrorAssignCallback;
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
                if ((p < 0) || (p >= (oValue as string).Length))
                    return JSObject.undefined;
                return (oValue as string)[p].ToString();
            }
        }

        public static JSObject fromCharCode(JSObject[] code)
        {
            int chc = 0;
            if (code.Length == 0)
                return new String();
            object charCode = code[0].Value;
            if (charCode is String)
                charCode = (charCode as String).oValue;
            if (charCode is int)
                chc = (int)charCode;
            else if (charCode is double)
                chc = (int)(double)charCode;
            else if (charCode is string)
            {
                double d = 0;
                if (Parser.ParseNumber((string)charCode, ref chc, false, out d))
                    chc = (int)d;
            }
            return new String() { oValue = ((char)chc).ToString() };
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
            if ((p < 0) || (p >= (oValue as string).Length))
                return double.NaN;
            return (int)(oValue as string)[p];
        }

        public override JSObject toString()
        {
            return this;
        }

        [Hidden]
        private static JSObject _length = 0;

        public JSObject length
        {
            get
            {
                _length.iValue = (oValue as string).Length;
                return _length;
            }
        }

        public override string ToString()
        {
            return oValue as string;
        }

        public override bool Equals(object obj)
        {
            if (obj is String)
                return oValue.Equals((obj as String).oValue);
            return false;
        }

        public override int GetHashCode()
        {
            return oValue.GetHashCode();
        }
    }
}