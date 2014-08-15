using System;
using System.Globalization;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class String : JSObject
    {
        [DoNotEnumerate]
        public static JSObject fromCharCode(JSObject[] code)
        {
            int chc = 0;
            if (code == null || code.Length == 0)
                return new String();
            string res = "";
            for (int i = 0; i < code.Length; i++)
            {
                chc = Tools.JSObjectToInt32(code[i]);
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
        public String(Arguments args)
            : this(args.Length == 0 ? "" : args[0].ToPrimitiveValue_String_Value().ToString())
        {
        }

        [DoNotEnumerate]
        public String(string s)
        {
            oValue = s;
            valueType = JSObjectType.String;
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to String");
            }
        }

        [Hidden]
        public JSObject this[int pos]
        {
            [Hidden]
            get
            {
                if ((pos < 0) || (pos >= (oValue as string).Length))
                    return JSObject.notExists;
                return new JSObject(false) { valueType = JSObjectType.String, oValue = (oValue as string)[pos].ToString(), attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete };
            }
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public String charAt(Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos[0]);
            if ((p < 0) || (p >= (oValue as string).Length))
                return "";
            return (oValue as string)[p].ToString();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public Number charCodeAt(Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos[0]);
            if ((p < 0) || (p >= (oValue as string).Length))
                return double.NaN;
            return (int)(oValue as string)[p];
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject concat(JSObject[] args)
        {
            string res = oValue.ToString();
            for (var i = 0; i < args.Length; i++)
                res += args[i].ToString();
            return res;
        }

        [AllowUnsafeCall(typeof(JSObject))]
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
                            Tools.ParseNumber(args[1].ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).IndexOf(fstr, pos, StringComparison.CurrentCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject lastIndexOf(JSObject[] args)
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
                            Tools.ParseNumber(args[1].ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).LastIndexOf(fstr, (oValue as string).Length, StringComparison.CurrentCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject localeCompare(JSObject[] args)
        {
            string str0 = oValue.ToString();
            string str1 = args.Length > 0 ? args[0].ToString() : "";
            return string.CompareOrdinal(str0, str1);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject match(Arguments args)
        {
            if (valueType <= JSObjectType.Undefined || (valueType >= JSObjectType.Object && oValue == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            var a0 = args[0];
            if (a0.valueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    regex.lastIndex.valueType = JSObjectType.Int;
                    regex.lastIndex.iValue = 0;
                    args[0] = this;
                    return regex.exec(args);
                }
                else
                {
                    var match = regex.regEx.Match(oValue as string ?? this.ToString());
                    int index = 0;
                    var res = new Array();

                    while (match.Success)
                    {
                        res.data[index++] = match.Value;
                        match = match.NextMatch();
                    }
                    res._length = index;
                    return res;
                }
            }
            else
            {
                var match = new System.Text.RegularExpressions.Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(oValue as string ?? this.ToString());
                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res.data[(uint)i] = match.Groups[i].Value;
                res.GetMember("index", true, true).Assign(match.Index);
                res.GetMember("input", true, true).Assign(this);
                return res;
            }
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject search(Arguments args)
        {
            if (valueType <= JSObjectType.Undefined || (valueType >= JSObjectType.Object && oValue == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            if (args.length == 0)
                return 0;
            var a0 = args[0];
            if ((args[0] ?? Null).valueType == JSObjectType.Object
                && (args[0] ?? Null).oValue != null
                && args[0].oValue.GetType() == typeof(RegExp))
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    args[0] = this;
                    var res = regex.exec(args);
                    if ((res ?? Null) != Null)
                        return res["index"];
                    return -1;
                }
                else
                {
                    return regex.regEx.Match(oValue as string ?? this.ToString()).Index;
                }
            }
            else
            {
                return (valueType == JSObjectType.String ? oValue : this).ToString().IndexOf(a0.ToString());
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject replace(Arguments args)
        {
            if (args.length == 0)
                return this;
            if ((args[0] ?? Null).valueType == JSObjectType.Object
                && (args[0] ?? Null).oValue != null
                && args[0].oValue.GetType() == typeof(RegExp))
            {
                if (args.length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string temp = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var match = new String();
                    match.assignCallback = null;
                    var margs = new Arguments();
                    margs.length = 1;
                    margs[0] = match;
                    match.oValue = (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), new System.Text.RegularExpressions.MatchEvaluator(
                        (m) =>
                        {
                            this.oValue = temp;
                            this.valueType = JSObjectType.String;
                            margs.length = 1 + m.Groups.Count - 1 + 1 + 1;
                            match.oValue = m.Value;
                            JSObject t;
                            for (int i = 1; i < m.Groups.Count; i++)
                            {
                                t = m.Groups[i].Value;
                                t.assignCallback = null;
                                margs[i] = t;
                            }
                            t = m.Index;
                            t.assignCallback = null;
                            margs[margs.length - 2] = t;
                            margs[margs.length - 1] = this;
                            return f.Invoke(margs).ToString();
                        }), (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
                    this.oValue = temp;
                    this.valueType = JSObjectType.String;
                    assignCallback = oac;
                    return match;
                }
                else
                {
                    return (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), args.Length > 1 ? args[1].ToString() : "undefined", (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
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
                    var margs = new Arguments();
                    margs.length = 3;
                    margs[0] = pattern;
                    margs[2] = this;
                    int index = oValue.ToString().IndexOf(pattern);
                    if (index == -1)
                        return this;
                    margs[1] = index;
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
                    var str = (valueType == JSObjectType.String ? oValue : ToPrimitiveValue_String_Value()).ToString();
                    var index = str.IndexOf(pattern);
                    if (index == -1)
                        return this;
                    return str.Substring(0, index) + replace + str.Substring(index + pattern.Length);
                }
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject slice(Arguments args)
        {
            string selfString = this.ToPrimitiveValue_Value_String().ToString();
            if (args.Length == 0)
                return selfString;
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
                        if (double.IsNaN(args[0].dValue))
                            pos0 = 0;
                        else if (double.IsPositiveInfinity(args[0].dValue))
                            pos0 = selfString.Length;
                        else
                            pos0 = (int)args[0].dValue;
                        break;
                    }
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.String:
                    {
                        //double d;
                        //Tools.ParseNumber(args[0].ToPrimitiveValue_Value_String().ToString(), pos0, out d, Tools.ParseNumberOptions.Default);
                        //pos0 = (int)d;
                        pos0 = Tools.JSObjectToInt32(args[0], 0, true);
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
                            if (double.IsNaN(args[1].dValue))
                                pos1 = 0;
                            else if (double.IsPositiveInfinity(args[1].dValue))
                                pos1 = selfString.Length;
                            else
                                pos1 = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            //double d;
                            //Tools.ParseNumber(args[1].ToPrimitiveValue_Value_String().ToString(), pos1, out d, Tools.ParseNumberOptions.Default);
                            //pos1 = (int)d;
                            pos1 = Tools.JSObjectToInt32(args[1], 0, true);
                            break;
                        }
                    case JSObjectType.NotExists:
                    case JSObjectType.NotExistsInObject:
                    case JSObjectType.Undefined:
                        {
                            pos1 = selfString.Length;
                            break;
                        }
                }
            }
            else
                pos1 = selfString.Length;
            while (pos0 < 0)
                pos0 += selfString.Length;
            while (pos1 < 0)
                pos1 += selfString.Length;
            pos0 = System.Math.Min(pos0, selfString.Length);
            return selfString.Substring(pos0, System.Math.Max(0, pos1 - pos0));
        }

        [AllowUnsafeCall(typeof(JSObject))]
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
                            Tools.ParseNumber(args[1].ToString(), limit, out d, Tools.ParseNumberOptions.Default);
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

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject substring(Arguments args)
        {
            return slice(args);
        }

        [AllowUnsafeCall(typeof(JSObject))]
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
                            Tools.ParseNumber(args[0].ToString(), pos0, out d, Tools.ParseNumberOptions.Default);
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
                            Tools.ParseNumber(args[1].ToString(), len, out d, Tools.ParseNumberOptions.Default);
                            len = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).Substring(pos0, len);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLocaleLowerCase()
        {
            return (oValue as string).ToLower(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLocaleUpperCase()
        {
            return (oValue as string).ToUpper(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLowerCase()
        {
            return (oValue as string).ToLowerInvariant();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toUpperCase()
        {
            return (oValue as string).ToUpperInvariant();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject trim()
        {
            return (oValue as string).Trim();
        }

        [CLSCompliant(false)]
        [AllowUnsafeCall(typeof(JSObject))]
        [ParametersCount(0)]
        [DoNotEnumerate]
        public new JSObject toString(Arguments args)
        {
            if ((this as object) is String && valueType == JSObjectType.Object) // prototype instance
                return "";
            if (this.valueType == JSObjectType.String)
                return this;
            else
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public override JSObject valueOf()
        {
            if (typeof(String) == this.GetType() && valueType == JSObjectType.Object) // prototype instance
                return "";
            if (this.valueType == JSObjectType.String)
                return this;
            else
                throw new JSException(new TypeError("Try to call String.valueOf for not string object."));
        }

        private Number _length = null;

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSObject length
        {
            [AllowUnsafeCall(typeof(JSObject))]
            [Hidden]
            get
            {
                if (this.GetType() == typeof(String))
                {
                    if (_length == null)
                        _length = new Number((oValue as string).Length) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum };
                    else
                        _length.iValue = (oValue as string).Length;
                    return _length;
                }
                else
                    return (oValue as string).Length;
            }
        }

        [Hidden]
        public override string ToString()
        {
            if ((this as object) is String)//(this.GetType() == typeof(String))
                return oValue as string;
            else
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
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
        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            create &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(typeof(String));
            int index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsInfinity(dindex)
                && !double.IsNaN(dindex)
                && ((index = (int)dindex) == dindex)
                && ((index = (int)dindex) == dindex)
                && index < (oValue as string).Length
                && index >= 0)
            {
                return this[index];
            }
            return DefaultFieldGetter(name, create, own); // обращение идёт к Объекту String, а не к значению string, поэтому члены создавать можно
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

        protected internal override System.Collections.Generic.IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (!hideNonEnum)
            {
                var len = (oValue as string).Length;
                for (var i = 0; i < len; i++)
                    yield return i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture);
                yield return "length";
            }
            for (var e = base.GetEnumeratorImpl(hideNonEnum); e.MoveNext(); )
                yield return e.Current;
        }

        [Hidden]
        public static implicit operator String(string val)
        {
            return new String(val);
        }
    }
}