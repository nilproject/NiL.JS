using System;
using NiL.JS.Core.Modules;
using System.Globalization;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Immutable]
    public sealed class String : EmbeddedType
    {
        internal static readonly String EmptyString = new String("");

        [DoNotEnumerate]
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

        [DoNotEnumerate]
        public String()
            : this("")
        {
        }

        [DoNotEnumerate]
        public String(JSObject args)
            : this(Tools.JSObjectToInt(args.GetMember("length")) == 0 ? "" : args.GetMember("0").ToString())
        {
        }

        [DoNotEnumerate]
        public String(string s)
        {
            oValue = s;
            valueType = JSObjectType.String;
            assignCallback = JSObject.ErrorAssignCallback;
            attributes |= JSObjectAttributes.Immutable;
        }

        [DoNotEnumerate]
        public JSObject this[int pos]
        {
            [Hidden]
            get
            {
                if ((pos < 0) || (pos >= (oValue as string).Length))
                    return JSObject.undefined;
                return new JSObject(false) { valueType = JSObjectType.String, oValue = (oValue as string)[pos].ToString(), attributes = JSObjectAttributes.ReadOnly };
            }
        }

        [DoNotEnumerate]
        public string charAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt(pos.GetMember("0"));
            if ((p < 0) || (p >= (oValue as string).Length))
                return "";
            return (oValue as string)[p].ToString();
        }

        [DoNotEnumerate]
        public double charCodeAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt(pos.GetMember("0"));
            if ((p < 0) || (p >= (oValue as string).Length))
                return double.NaN;
            return (int)(oValue as string)[p];
        }

        [DoNotEnumerate]
        public JSObject concat(JSObject[] args)
        {
            string res = oValue.ToString();
            for (var i = 0; i < args.Length; i++)
                res += args[i].ToString();
            return res;
        }

        [DoNotEnumerate]
        public JSObject indexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
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
                            Tools.ParseNumber(args[1].ToString(), pos, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).IndexOf(fstr, pos, StringComparison.CurrentCulture);
        }

        [DoNotEnumerate]
        public JSObject lastIndexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].Value.ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
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
                            Tools.ParseNumber(args[1].ToString(), pos, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).LastIndexOf(fstr, pos, StringComparison.CurrentCulture);
        }

        [DoNotEnumerate]
        public JSObject localeCompare(JSObject[] args)
        {
            string str0 = oValue.ToString();
            string str1 = args.Length > 0 ? args[0].ToString() : "";
            return string.CompareOrdinal(str0, str1);
        }

        [DoNotEnumerate]
        public JSObject match(JSObject args)
        {
            if (valueType <= JSObjectType.Undefined || (valueType >= JSObjectType.Object && oValue == null))
                throw new JSException(TypeProxy.Proxy(new TypeError("String.prototype.match called on null or undefined")));
            var a0 = args.GetMember("0");
            if (a0.valueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex.global)
                {
                    args.GetMember("0", true, true).Assign(this);
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
                var match = new System.Text.RegularExpressions.Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(oValue as string ?? this.ToString());
                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res.data[i] = match.Groups[i].Value;
                res.GetMember("index", true, true).Assign(match.Index);
                res.GetMember("input", true, true).Assign(this);
                return res;
            }
        }

        [DoNotEnumerate]
        public JSObject replace(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            if (args[0].valueType == JSObjectType.Object && args[0].oValue is RegExp)
            {
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string temp = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var match = new String();
                    match.assignCallback = null;
                    var margs = CreateObject();
                    JSObject len = 1;
                    len.assignCallback = null;
                    len.attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum | JSObjectAttributes.ReadOnly;
                    margs.fields["length"] = len;
                    margs.fields["0"] = match;
                    match.oValue = (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), new System.Text.RegularExpressions.MatchEvaluator(
                        (m) =>
                        {
                            this.oValue = temp;
                            this.valueType = JSObjectType.String;
                            len.iValue = 1 + m.Groups.Count + 1 + 1;
                            match.oValue = m.Value;
                            JSObject t = m.Index;
                            for (int i = 0; i < m.Groups.Count; i++)
                            {
                                t = m.Groups[i].Value;
                                t.assignCallback = null;
                                margs.fields[(i + 1).ToString(CultureInfo.InvariantCulture)] = t;
                            }
                            t = m.Index;
                            t.assignCallback = null;
                            margs.fields[(len.iValue - 2).ToString()] = t;
                            margs.fields[(len.iValue - 1).ToString()] = this;
                            return f.Invoke(margs).ToString();
                        }), (args[0].oValue as RegExp).global ? int.MaxValue : 1);
                    this.oValue = temp;
                    this.valueType = JSObjectType.String;
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
                    var margs = CreateObject();
                    margs.oValue = Arguments.Instance;
                    JSObject alen = 3;
                    alen.assignCallback = null;
                    alen.attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum | JSObjectAttributes.ReadOnly;
                    margs.fields["length"] = alen;
                    margs.fields["0"] = pattern;
                    margs.fields["2"] = this;
                    int index = oValue.ToString().IndexOf(pattern);
                    if (index == -1)
                        return this;
                    margs.fields["1"] = index;
                    var res = othis.Substring(0, index) + f.Invoke(margs).ToString() + othis.Substring(index + pattern.Length);
                    oValue = othis;
                    valueType = JSObjectType.String;
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

        [DoNotEnumerate]
        public JSObject slice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            switch (args[0].valueType)
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
                        Tools.ParseNumber(args[0].ToString(), pos0, out d);
                        pos0 = (int)d;
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
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
                            Tools.ParseNumber(args[1].ToString(), pos1, out d);
                            pos1 = (int)d;
                            break;
                        }
                }
            }
            else
                pos1 = (oValue as string).Length;
            return (oValue as string).Substring(pos0, pos1 - pos0);
        }

        [DoNotEnumerate]
        public JSObject split(JSObject[] args)
        {
            if (args.Length == 0)
                return new Array(new object[] { this });
            string fstr = args[0].ToString();
            int limit = int.MaxValue;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
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
                            Tools.ParseNumber(args[1].ToString(), limit, out d);
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

        [DoNotEnumerate]
        public JSObject substring(JSObject[] args)
        {
            return slice(args);
        }

        [DoNotEnumerate]
        public JSObject substr(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            if (args.Length > 0)
            {
                switch (args[0].valueType)
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
                            Tools.ParseNumber(args[0].ToString(), pos0, out d);
                            pos0 = (int)d;
                            break;
                        }
                }
            }
            int len = (oValue as string).Length - pos0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
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
                            Tools.ParseNumber(args[1].ToString(), len, out d);
                            len = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).Substring(pos0, len);
        }

        [DoNotEnumerate]
        public JSObject toLocaleLowerCase()
        {
            return (oValue as string).ToLower(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [DoNotEnumerate]
        public JSObject toLocaleUpperCase()
        {
            return (oValue as string).ToUpper(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [DoNotEnumerate]
        public JSObject toLowerCase()
        {
            return (oValue as string).ToLowerInvariant();
        }

        [DoNotEnumerate]
        public JSObject toUpperCase()
        {
            return (oValue as string).ToUpperInvariant();
        }

        [DoNotEnumerate]
        public JSObject trim()
        {
            return (oValue as string).Trim();
        }

        [ParametersCount(0)]
        public override JSObject toString(JSObject args)
        {
            if (typeof(String).IsAssignableFrom(this.GetType()))
            {
                if (valueType == JSObjectType.Object)
                    return "";
                return this;
            }
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.toString for not string object.")));
        }

        [DoNotEnumerate]
        public override JSObject valueOf()
        {
            if (typeof(String).IsAssignableFrom(this.GetType()))
                return this;
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.valueOf for not string object.")));
        }

        private Number _length = null;

        [DoNotEnumerate]
        public JSObject length
        {
            [Hidden]
            get
            {
                if (_length == null)
                    _length = new Number((oValue as string).Length) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum };
                else
                    _length.iValue = (oValue as string).Length;
                return _length;
            }
        }

        [Hidden]
        public override string ToString()
        {
            if (this is String)
                return oValue as string;
            else
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call String.toString for not string object.")));
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            if (obj is String)
                return oValue.Equals((obj as String).oValue);
            return false;
        }

        [Hidden]
        public override int GetHashCode()
        {
            return oValue.GetHashCode();
        }

        [Hidden]
        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(typeof(String));
            int index = 0;
            double dindex = 0.0;
            if (Tools.ParseNumber(name, index, out dindex) && ((index = (int)dindex) == dindex))
                return this[index];
            else
                return DefaultFieldGetter(name, create, false);
        }

        #region HTML Wraping
        [DoNotEnumerate]
        public JSObject anchor(JSObject arg)
        {
            return "<a name=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        [DoNotEnumerate]
        public JSObject big()
        {
            return "<big>" + oValue + "</big>";
        }

        [DoNotEnumerate]
        public JSObject blink()
        {
            return "<blink>" + oValue + "</blink>";
        }

        [DoNotEnumerate]
        public JSObject bold()
        {
            return "<bold>" + oValue + "</bold>";
        }

        [DoNotEnumerate]
        public JSObject @fixed()
        {
            return "<tt>" + oValue + "</tt>";
        }

        [DoNotEnumerate]
        public JSObject fontcolor(JSObject arg)
        {
            return "<font color=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        [DoNotEnumerate]
        public JSObject fontsize(JSObject arg)
        {
            return "<font size=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        [DoNotEnumerate]
        public JSObject italics()
        {
            return "<i>" + oValue + "</i>";
        }

        [DoNotEnumerate]
        public JSObject link(JSObject arg)
        {
            return "<a href=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        [DoNotEnumerate]
        public JSObject small()
        {
            return "<small>" + oValue + "</small>";
        }

        [DoNotEnumerate]
        public JSObject strike()
        {
            return "<strike>" + oValue + "</strike>";
        }

        [DoNotEnumerate]
        public JSObject sub()
        {
            return "<sub>" + oValue + "</sub>";
        }

        [DoNotEnumerate]
        public JSObject sup()
        {
            return "<sup>" + oValue + "</sup>";
        }
        #endregion

        [Hidden]
        public static implicit operator String(string val)
        {
            if (string.IsNullOrEmpty(val))
                return EmptyString;
            return new String(val);
        }
    }
}