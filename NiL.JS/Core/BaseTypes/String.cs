using System;
using System.Globalization;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class String : JSObject
    {
        [DoNotEnumerate]
        public static JSObject fromCharCode(Arguments code)
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
            attributes |= JSObjectAttributesInternal.SystemObject;
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
                if ((pos < 0) || (pos >= oValue.ToString().Length))
                    return JSObject.notExists;
                return new JSObject(false) { valueType = JSObjectType.String, oValue = (oValue.ToString())[pos].ToString(), attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete };
            }
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public String charAt(Arguments pos)
        {
            var strValue = this.ToString();
            int p = Tools.JSObjectToInt32(pos[0]);
            if ((p < 0) || (p >= strValue.Length))
                return "";
            return strValue[p].ToString();//Tools.charStrings[strValue[p]];
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject charCodeAt(Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos.a0 ?? notExists);
            var selfStr = this.ToString();
            if ((p < 0) || (p >= selfStr.Length))
                return Number.NaN;
            var res = new JSObject()
            {
                iValue = selfStr[p],
                valueType = JSObjectType.Int
            };
            return res;
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject concat(Arguments args)
        {
            if (args.length == 0)
                return this.toString(null);
            if (args.length == 1)
                return string.Concat(this.ToString(), args[0].ToString());
            if (args.length == 2)
                return string.Concat(this.ToString(), args[0].ToString(), args[1].ToString());
            if (args.length == 3)
                return string.Concat(this.ToString(), args[0].ToString(), args[1].ToString(), args[2].ToString());
            if (args.length == 4)
                return string.Concat(this.ToString(), args[0].ToString(), args[1].ToString(), args[2].ToString(), args[3].ToString());
            var res = new StringBuilder().Append(this.ToString());
            for (var i = 0; i < args.Length; i++)
                res.Append(args[i].ToString());
            return res.ToString();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject indexOf(Arguments args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = 0;
            while (args.Length > 1)
            {
                JSObject value = null;
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
                        {
                            value = args[1].ToPrimitiveValue_Value_String();
                            if (value.valueType < JSObjectType.String)
                            {
                                args[1] = value;
                                continue;
                            }
                            goto case JSObjectType.String;
                        }
                    case JSObjectType.String:
                        {
                            double d = 0;
                            Tools.ParseNumber((value ?? args[1]).ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
                break;
            }
            var strValue = this.ToString();
            return strValue.IndexOf(fstr, System.Math.Max(0, System.Math.Min(pos, strValue.Length)), StringComparison.CurrentCulture);
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject lastIndexOf(Arguments args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = int.MaxValue >> 1;
            while (args.Length > 1)
            {
                JSObject value = null;
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
                            if (!double.IsNaN(args[1].dValue))
                                pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                        {
                            value = args[1].ToPrimitiveValue_Value_String();
                            if (value.valueType < JSObjectType.String)
                            {
                                args[1] = value;
                                continue;
                            }
                            goto case JSObjectType.String;
                        }
                    case JSObjectType.String:
                        {
                            double d = 0;
                            Tools.ParseNumber((value ?? args[1]).ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
                break;
            }
            var strValue = this.ToString();
            pos += strValue.Length;
            return strValue.LastIndexOf(fstr, System.Math.Max(0, System.Math.Min(pos, strValue.Length)), StringComparison.CurrentCulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject localeCompare(Arguments args)
        {
            string str0 = oValue.ToString();
            string str1 = args[0].ToString();
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
                    var match = regex.regEx.Match(this.ToString());
                    int index = 0;
                    var res = new Array();
                    if (match.Success) do
                        {
                            res.data[index++] = match.Value;
                            match = match.NextMatch();
                        }
                        while (match.Success);
                    return res;
                }
            }
            else
            {
                var match = new System.Text.RegularExpressions.Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(this.ToString());
                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res.data[i] = match.Groups[i].Value;
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
                    return regex.regEx.Match(oValue.ToString() ?? this.ToString()).Index;
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
                    string temp = this.oValue.ToString();
                    var f = args[1].oValue as Function;
                    var match = new String();
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
                                margs[i] = t;
                            }
                            t = m.Index;
                            margs[margs.length - 2] = t;
                            margs[margs.length - 1] = this;
                            return f.Invoke(margs).ToString();
                        }), (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
                    this.oValue = temp;
                    this.valueType = JSObjectType.String;
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
                    string othis = this.oValue.ToString();
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
                        if (double.IsNaN(args[0].dValue) || double.IsNegativeInfinity(args[0].dValue))
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
                            if (double.IsNaN(args[1].dValue) || double.IsNegativeInfinity(args[0].dValue))
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
            return selfString.Substring(pos0, System.Math.Min(selfString.Length, System.Math.Max(0, pos1 - pos0)));
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject split(Arguments args)
        {
            if (args.Length == 0 || !args[0].isDefinded)
                return new Array(new object[] { this.ToString() });
            int limit = int.MaxValue;
            if (args.Length > 1)
            {
                var limO = args[1];
                if (limO.valueType >= JSObjectType.Object)
                    limO = limO.ToPrimitiveValue_Value_String();
                switch (limO.valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            limit = limO.iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            limit = (int)limO.dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(limO.ToString(), 0, out d, Tools.ParseNumberOptions.Default);
                            limit = (int)d;
                            break;
                        }
                }
            }
            if (args[0].valueType == JSObjectType.Object && args[0].oValue is RegExp)
            {
                string selfString = this.ToPrimitiveValue_Value_String().ToString();
                var match = (args[0].oValue as RegExp).regEx.Match(selfString);
                if ((args[0].oValue as RegExp).regEx.ToString().Length == 0)
                {
                    match = match.NextMatch();
                    limit = selfString.Length;
                }
                Array res = new Array();
                int index = 0;
                while (res.data.Length < limit)
                {
                    if (!match.Success)
                    {
                        res.data.Add(selfString.Substring(index, selfString.Length - index));
                        break;
                    }
                    int nindex = match.Index;
                    if (nindex == -1)
                    {
                        res.data.Add(selfString.Substring(index, selfString.Length - index));
                        break;
                    }
                    else
                    {
                        var item = selfString.Substring(index, nindex - index);
                        res.data.Add(item);
                        index = nindex + match.Length;
                    }
                    match = match.NextMatch();
                }
                return res;
            }
            else
            {
                string fstr = args[0].ToString();
                string selfString = this.ToPrimitiveValue_Value_String().ToString();
                Array res = new Array();
                if (string.IsNullOrEmpty(fstr))
                {
                    for (var i = 0; i < System.Math.Min(selfString.Length, limit); i++)
                        res.data.Add(selfString[i]);
                }
                else
                {
                    int index = 0;
                    while (res.data.Length < limit)
                    {
                        int nindex = selfString.IndexOf(fstr, index);
                        if (nindex == -1)
                        {
                            res.data.Add(selfString.Substring(index, selfString.Length - index));
                            break;
                        }
                        else
                        {
                            var item = selfString.Substring(index, nindex - index);
                            res.data.Add(item);
                            index = nindex + fstr.Length;
                        }
                    }
                }
                return res;
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject substring(Arguments args)
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
                        if (double.IsNaN(args[0].dValue) || double.IsNegativeInfinity(args[0].dValue))
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
                            if (double.IsNaN(args[1].dValue) || double.IsNegativeInfinity(args[0].dValue))
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
            if (pos0 > pos1)
            {
                pos0 ^= pos1;
                pos1 ^= pos0;
                pos0 ^= pos1;
            }
            pos0 = System.Math.Max(0, System.Math.Min(pos0, selfString.Length));
            pos1 = System.Math.Max(0, System.Math.Min(pos1, selfString.Length));
            return selfString.Substring(pos0, System.Math.Min(selfString.Length, System.Math.Max(0, pos1 - pos0)));
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject substr(Arguments args)
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
            int len = (oValue.ToString()).Length - pos0;
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
            return (this as JSObject).ToString().Substring(pos0, len);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLocaleLowerCase()
        {
            return (this as JSObject).ToString().ToLower(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLocaleUpperCase()
        {
            return (this as JSObject).ToString().ToUpper(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toLowerCase()
        {
            return (this as JSObject).ToString().ToLowerInvariant();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject toUpperCase()
        {
            return (this as JSObject).ToString().ToUpperInvariant();
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public JSObject trim()
        {
            switch (this.valueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExists:
                case JSObjectType.NotExistsInObject:
                    throw new JSException(new TypeError("string can't be undefined"));
                case JSObjectType.Function:
                case JSObjectType.String:
                case JSObjectType.Object:
                    {
                        if (this.oValue == null)
                            throw new JSException(new TypeError("string can't be null"));
                        break;
                    }
            }
            try
            {
                var sb = new StringBuilder(this.ToString());
                int initialLength = sb.Length;
                int index = 0;
                for (; index < sb.Length && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index++) ;
                if (index > 0)
                    sb.Remove(0, index);
                index = sb.Length - 1;
                for (; index >= 0 && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index--) ;
                index++;
                if (index < sb.Length)
                    sb.Remove(index, sb.Length - index);
                if (sb.Length != initialLength)
                {
                    index = 0;
                    for (; ; )
                    {
                        for (; index < sb.Length && sb[index] != '\n' && sb[index] != '\r'; index++) ;
                        if (index >= sb.Length)
                            break;
                        var startindex = index;
                        for (; index < sb.Length && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index++) ;
                        sb.Remove(startindex, index - startindex);
                    }
                }
                return sb.ToString();
            }
            catch
            {
                throw;
            }
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
                var len = oValue.ToString().Length;
                if (this.GetType() == typeof(String))
                {
                    if (_length == null)
                        _length = new Number(len) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.NotConfigurable };
                    else
                        _length.iValue = len;
                    return _length;
                }
                else
                    return len;
            }
        }

        [Hidden]
        public override string ToString()
        {
            if ((this as object) is String)
                return oValue.ToString();
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
            int index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsInfinity(dindex)
                && !double.IsNaN(dindex)
                && ((index = (int)dindex) == dindex)
                && ((index = (int)dindex) == dindex)
                && index < (oValue.ToString()).Length
                && index >= 0)
            {
                return this[index];
            }
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            var namestr = name.ToString();
            if (namestr == "length")
                return length;
            if (namestr == "__proto__")
            {
                if (create
                    && ((__proto__.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                    && ((__proto__.attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                    __proto__ = __proto__.CloneImpl();
                return __proto__;
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

        [Hidden]
        public static implicit operator String(string val)
        {
            return new String(val);
        }
    }
}