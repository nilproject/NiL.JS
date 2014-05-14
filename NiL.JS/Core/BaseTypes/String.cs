using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Immutable]
    internal class String : EmbeddedType
    {
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

        public JSObject indexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
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
            return string.CompareOrdinal(str0, str1);
        }

        public JSObject match(JSObject args)
        {
            if (ValueType <= JSObjectType.Undefined || (ValueType >= JSObjectType.Object && oValue == null))
                throw new JSException(TypeProxy.Proxy(new TypeError("String.prototype.match called on null or undefined")));
            var a0 = args.GetField("0", true, false);
            if (a0.ValueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex.global)
                {
                    args.GetField("0", false, true).Assign(this);
                    return regex.exec(args);
                }
                else
                {
                    var groups = regex.regEx.Match(oValue as string ?? this.ToString()).Groups;
                    var res = new Array(groups.Count);
                    for (int i = 0; i < groups.Count; i++)
                        res.data[i] = groups[i].Value;
                    return res;
                }
            }
            else
            {
                var groups = new System.Text.RegularExpressions.Regex((a0.ValueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(oValue as string ?? this.ToString()).Groups;
                var res = new Array(groups.Count);
                for (int i = 0; i < groups.Count; i++)
                    res.data[i] = groups[i].Value;
                return res;
            }
        }

        public JSObject replace(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            if (args[0].ValueType == JSObjectType.Object && args[0].oValue is RegExp)
            {
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string temp = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var match = new String();
                    match.assignCallback = null;
                    var margs = new JSObject(true) { ValueType = JSObjectType.Object, oValue = Arguments.Instance, prototype = JSObject.GlobalPrototype };
                    JSObject len = 1;
                    len.assignCallback = null;
                    len.attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                    margs.fields["length"] = len;
                    margs.fields["0"] = match;
                    match.oValue = (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), new System.Text.RegularExpressions.MatchEvaluator(
                        (m) =>
                        {
                            this.oValue = temp;
                            this.ValueType = JSObjectType.String;
                            len.iValue = 1 + m.Groups.Count + 1 + 1;
                            match.oValue = m.Value;
                            JSObject t = m.Index;
                            for (int i = 0; i < m.Groups.Count; i++)
                            {
                                t = m.Groups[i].Value;
                                t.assignCallback = null;
                                margs.fields[(i + 1).ToString()] = t;
                            }
                            t = m.Index;
                            t.assignCallback = null;
                            margs.fields[(len.iValue - 2).ToString()] = t;
                            margs.fields[(len.iValue - 1).ToString()] = this;
                            return f.Invoke(margs).ToString();
                        }), (args[0].oValue as RegExp).global ? int.MaxValue : 1);
                    this.oValue = temp;
                    this.ValueType = JSObjectType.String;
                    assignCallback = oac;
                    return match;
                }
                else
                {
                    return (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), args.Length > 1 ? args[1].ToString() : "undefined", (args[0].oValue as RegExp).global ? int.MaxValue : 1);
                }
            }
            else
            {
                string pattern = args.Length > 0 ? args[0].ToString() : "";
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string othis = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var match = new String();
                    var margs = new JSObject(true) { ValueType = JSObjectType.Object, oValue = Arguments.Instance, prototype = JSObject.GlobalPrototype };
                    JSObject alen = 3;
                    alen.assignCallback = null;
                    alen.attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                    margs.fields["length"] = alen;
                    margs.fields["0"] = pattern;
                    margs.fields["2"] = this;
                    int index = oValue.ToString().IndexOf(pattern);
                    if (index == -1)
                        return this;
                    margs.fields["1"] = index;
                    var res = othis.Substring(0, index) + f.Invoke(margs).ToString() + othis.Substring(index + pattern.Length);
                    oValue = othis;
                    ValueType = JSObjectType.String;
                    assignCallback = oac;
                    return res;
                }
                else
                {
                    string replace = args.Length > 1 ? args[1].ToString() : "undefined";
                    if (string.IsNullOrEmpty(pattern))
                        return replace + oValue;
                    return oValue.ToString().Replace(pattern, replace);
                }
            }
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
            if (args.Length > 0)
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
            int len = (oValue as string).Length - pos0;
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

        public static implicit operator String(string val)
        {
            return new String(val);
        }
    }
}