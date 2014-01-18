using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    internal class String : EmbeddedType
    {
        [Hidden]
        private static readonly String result = new String();

        public String()
            : this("")
        {
        }

        public String(string s)
        {
            oValue = s;
            ValueType = JSObjectType.String;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public String(JSObject[] s)
            : this(s.Length > 0 ? s[0].ToString() : "")
        {
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
                return new JSObject(false) { ValueType = JSObjectType.String, oValue = (oValue as string)[p].ToString(), assignCallback = () => { return false; } };
            }
        }

        public string charAt(object pos)
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
                return "";
            return (oValue as string)[p].ToString();
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

        public JSObject concat(JSObject[] args)
        {
            string res = oValue.ToString();
            for (var i = 0; i < args.Length; i++)
                res += args[i].ToString();
            return res;
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
                if (Tools.ParseNumber((string)charCode, ref chc, false, out d))
                    chc = (int)d;
            }
            result.oValue = ((char)chc).ToString();
            return result;
        }

        public JSObject indexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch(args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref pos, false, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).IndexOf(fstr, pos);
        }

        public JSObject lastIndexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].Value.ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref pos, false, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).LastIndexOf(fstr, pos);
        }

        public JSObject localeCompare(JSObject[] args)
        {
            string str0 = oValue.ToString();
            string str1 = args.Length > 0 ? args[0].ToString() : "";
            return str0.CompareTo(str1);
        }

        public JSObject slice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            switch (args[0].ValueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        pos0 = args[0].iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        pos0 = (int)args[0].dValue;
                        break;
                    }
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.String:
                    {
                        double d;
                        Tools.ParseNumber(args[0].ToString(), ref pos0, false, out d);
                        pos0 = (int)d;
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos1 = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos1 = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref pos1, false, out d);
                            pos1 = (int)d;
                            break;
                        }
                }
            }
            else
                pos1 = int.MaxValue;
            return (oValue as string).Substring(pos0, pos1 - pos0);
        }
        
        public JSObject split(JSObject[] args)
        {
            if (args.Length == 0)
                return new Array(new object[] { this });
            string fstr = args[0].ToString();
            int limit = int.MaxValue;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            limit = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            limit = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref limit, false, out d);
                            limit = (int)d;
                            break;
                        }
                }
            }
            var res = (oValue as string).Split(new string[] { fstr }, limit, StringSplitOptions.None);
            return new Array(res);
        }

        public JSObject substring(JSObject[] args)
        {
            return slice(args);
        }

        public JSObject substr(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos0 = args[0].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos0 = (int)args[0].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[0].ToString(), ref pos0, false, out d);
                            pos0 = (int)d;
                            break;
                        }
                }
            }
            int len = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            len = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            len = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref len, false, out d);
                            len = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).Substring(pos0, len);
        }

        public JSObject toLocaleLowerCase()
        {
            return (oValue as string).ToLower(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        public JSObject toLocaleUpperCase()
        {
            return (oValue as string).ToUpper(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        public JSObject toLowerCase()
        {
            return (oValue as string).ToLowerInvariant();
        }

        public JSObject toUpperCase()
        {
            return (oValue as string).ToUpperInvariant();
        }

        public JSObject Trim()
        {
            return (oValue as string).Trim();
        }

        public override JSObject toString()
        {
            return this;
        }

        private static readonly Number _length = new Number(0) { assignCallback = JSObject.ProtectAssignCallback };

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

        public override JSObject GetField(string name, bool fast, bool own)
        {
            int index = 0;
            double dindex = 0.0;
            if (Tools.ParseNumber(name, ref index, false, out dindex) && ((index = (int)dindex) == dindex))
                return this[index];
            else
                return base.GetField(name, fast, own);
        }

        #region HTML Wraping
        public JSObject anchor(JSObject arg)
        {
            return "<a name=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        public JSObject big()
        {
            return "<big>" + oValue + "</big>";
        }

        public JSObject blink()
        {
            return "<blink>" + oValue + "</blink>";
        }

        public JSObject bold()
        {
            return "<bold>" + oValue + "</bold>";
        }

        public JSObject @fixed()
        {
            return "<tt>" + oValue + "</tt>";
        }
        
        public JSObject fontcolor(JSObject arg)
        {
            return "<font color=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        public JSObject fontsize(JSObject arg)
        {
            return "<font size=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        public JSObject italics()
        {
            return "<i>" + oValue + "</i>";
        }
        public JSObject link(JSObject arg)
        {
            return "<a href=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        public JSObject small()
        {
            return "<small>" + oValue + "</small>";
        }
        public JSObject strike()
        {
            return "<strike>" + oValue + "</strike>";
        }

        public JSObject sub()
        {
            return "<sub>" + oValue + "</sub>";
        }

        public JSObject sup()
        {
            return "<sup>" + oValue + "</sup>";
        }
        #endregion
    }
}