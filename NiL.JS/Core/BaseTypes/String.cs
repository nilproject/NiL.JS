using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    internal class String : EmbeddedType
    {
        public String()
            : this("")
        {
        }

        public String(string s)
        {
            oValue = s;
            ValueType = JSObjectType.String;
            assignCallback = JSObject.ErrorAssignCallback;
            attributes |= ObjectAttributes.Immutable;
        }

        public String(JSObject[] s)
            : this(s.Length > 0 ? s[0].ToString() : "")
        {
        }

        public JSObject this[int pos]
        {
            get
            {
                if ((pos < 0) || (pos >= (oValue as string).Length))
                    return JSObject.undefined;
                return new JSObject(false) { ValueType = JSObjectType.String, oValue = (oValue as string)[pos].ToString(), attributes = ObjectAttributes.ReadOnly };
            }
        }

        public string charAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt(pos.GetField("0", true, false));
            if ((p < 0) || (p >= (oValue as string).Length))
                return "";
            return (oValue as string)[p].ToString();
        }

        public double charCodeAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt(pos.GetField("0", true, false));
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
            if (code == null || code.Length == 0)
                return new String();
            string res = "";
            for (int i = 0; i < code.Length; i++)
            {
                chc = Tools.JSObjectToInt(code[i]);
                res += ((char)chc).ToString();
            }
            return res;
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
            return (oValue as string).IndexOf(fstr, pos, StringComparison.Ordinal);
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
            return (oValue as string).LastIndexOf(fstr, pos, StringComparison.Ordinal);
        }

        public JSObject localeCompare(JSObject[] args)
        {
            string str0 = oValue.ToString();
            string str1 = args.Length > 0 ? args[0].ToString() : "";
            return string.Compare(str0, str1, StringComparison.Ordinal);
        }

        public JSObject replace(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            string pattern = args.Length > 0 ? args[0].ToString() : "";
            string replace = args.Length > 1 ? args[1].ToString() : "undefined";
            if (string.IsNullOrEmpty(pattern))
                return replace + oValue;
            return oValue.ToString().Replace(pattern, replace);
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
                pos1 = (oValue as string).Length;
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
            string[] res = null;
            if (string.IsNullOrEmpty(fstr))
                return new Array(System.Text.UTF8Encoding.UTF8.GetChars(System.Text.UTF8Encoding.UTF8.GetBytes(oValue as string)));
            else
                res = (oValue as string).Split(new string[] { fstr }, limit, StringSplitOptions.None);
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

        [ParametersCount(0)]
        public override JSObject toString(JSObject args)
        {
            if (typeof(String).IsAssignableFrom(this.GetType()))
            {
                if (ValueType == JSObjectType.Object)
                    return "";
                return this;
            }
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.toString for not string object.")));
        }

        public override JSObject valueOf()
        {
            if (typeof(String).IsAssignableFrom(this.GetType()))
                return this;
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.valueOf for not string object.")));
        }

        private Number _length = null;// new Number(0) { attributes = ObjectAttributes.ReadOnly | ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };

        public JSObject length
        {
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = ObjectAttributes.ReadOnly | ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
                _length.iValue = (oValue as string).Length;
                return _length;
            }
        }

        public override string ToString()
        {
            if (typeof(String).IsAssignableFrom(this.GetType()))
                return oValue as string;
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.toString for not string object.")));
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
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(typeof(String));
            int index = 0;
            double dindex = 0.0;
            if (Tools.ParseNumber(name, ref index, false, out dindex) && ((index = (int)dindex) == dindex))
                return this[index];
            else
                return DefaultFieldGetter(name, fast, false);
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

        public static implicit operator String (string val)
        {
            return new String(val);
        }
    }
}