using System;
using System.Text;
using System.Text.RegularExpressions;
using NiL.JS.Core;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
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
                return new JSObject(false) { valueType = JSObjectType.String, oValue = (oValue.ToString())[pos].ToString(), attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotEnumerate | JSObjectAttributesInternal.DoNotDelete };
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static String charAt(JSObject self, Arguments pos)
        {
            var strValue = self.ToString();
            int p = Tools.JSObjectToInt32(pos[0], true);
            if ((p < 0) || (p >= strValue.Length))
                return "";
            return strValue[p].ToString();//Tools.charStrings[strValue[p]];
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject charCodeAt(JSObject self, Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos.a0 ?? notExists, true);
            var selfStr = self.ToString();
            if ((p < 0) || (p >= selfStr.Length))
                return Number.NaN;
            var res = new JSObject()
            {
                valueType = JSObjectType.Int,
                iValue = selfStr[p],
            };
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject concat(JSObject self, Arguments args)
        {
            if (args.length == 0)
                return self.ToString();
            if (args.length == 1)
                return string.Concat(self.ToString(), args[0].ToString());
            if (args.length == 2)
                return string.Concat(self.ToString(), args[0].ToString(), args[1].ToString());
            if (args.length == 3)
                return string.Concat(self.ToString(), args[0].ToString(), args[1].ToString(), args[2].ToString());
            if (args.length == 4)
                return string.Concat(self.ToString(), args[0].ToString(), args[1].ToString(), args[2].ToString(), args[3].ToString());
            var res = new StringBuilder().Append(self.ToString());
            for (var i = 0; i < args.Length; i++)
                res.Append(args[i].ToString());
            return res.ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject indexOf(JSObject self, Arguments args)
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
            var strValue = self.ToString();
            return strValue.IndexOf(fstr, System.Math.Max(0, System.Math.Min(pos, strValue.Length)), StringComparison.CurrentCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject lastIndexOf(JSObject self, Arguments args)
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
            var strValue = self.ToString();
            return strValue.LastIndexOf(fstr, StringComparison.CurrentCulture) == 0
                    ? 0
                    : strValue.LastIndexOf(fstr, System.Math.Max(0, System.Math.Min(pos, strValue.Length - 1)), StringComparison.CurrentCulture);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject localeCompare(JSObject self, Arguments args)
        {
            string str0 = self.ToString();
            string str1 = args[0].ToString();
            return string.CompareOrdinal(str0, str1);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject match(JSObject self, Arguments args)
        {
            if (self.valueType <= JSObjectType.Undefined || (self.valueType >= JSObjectType.Object && self.Value == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            var a0 = args[0];
            if (a0.valueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    regex.lastIndex.valueType = JSObjectType.Int;
                    regex.lastIndex.iValue = 0;
                    return regex.exec(self);
                }
                else
                {
                    var match = regex.regEx.Match(self.ToString());
                    int index = 0;
                    var res = new Array();
                    while (match.Success)
                    {
                        res.data[index++] = match.Value;
                        match = match.NextMatch();
                    }
                    return res;
                }
            }
            else
            {
                var match = new Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), RegexOptions.ECMAScript).Match(self.ToString());
                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res.data[i] = match.Groups[i].Value;
                res.GetMember("index", true, true).Assign(match.Index);
                res.GetMember("input", true, true).Assign(self);
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSObject search(JSObject self, Arguments args)
        {
            if (self.valueType <= JSObjectType.Undefined || (self.valueType >= JSObjectType.Object && self.Value == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            if (args.length == 0)
                return 0;
            var a0 = args[0];
            if (a0.valueType == JSObjectType.Object
                && a0.oValue != null
                && a0.oValue.GetType() == typeof(RegExp))
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    var res = regex.exec(self);
                    if ((res ?? Null) != Null)
                        return res["index"];
                    return -1;
                }
                else
                {
                    return regex.regEx.Match(self.ToString()).Index;
                }
            }
            else
            {
                return self.ToString().IndexOf(a0.ToString());
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(2)]
        [AllowNullArguments]
        public static JSObject replace(JSObject self, Arguments args)
        {
            if (args == null || args.length == 0)
                return self;
            if ((args[0] ?? Null).valueType == JSObjectType.Object
                && (args[0] ?? Null).oValue != null
                && args[0].oValue.GetType() == typeof(RegExp))
            {
                if (args.length > 1 && args[1].oValue is Function)
                {
                    string temp = self.oValue.ToString();
                    var f = args[1].oValue as Function;
                    var match = new String();
                    var margs = new Arguments();
                    margs.length = 1;
                    margs[0] = match;
                    match.oValue = (args[0].oValue as RegExp).regEx.Replace(self.ToString(), new System.Text.RegularExpressions.MatchEvaluator(
                        (m) =>
                        {
                            self.oValue = temp;
                            self.valueType = JSObjectType.String;
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
                            margs[margs.length - 1] = self;
                            return f.Invoke(margs).ToString();
                        }), (args[0].Value as RegExp)._global ? int.MaxValue : 1);
                    self.oValue = temp;
                    self.valueType = JSObjectType.String;
                    return match;
                }
                else
                {
                    return (args[0].oValue as RegExp).regEx.Replace(self.ToString(), args.Length > 1 ? args[1].ToString() : "undefined", (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
                }
            }
            else
            {
                string pattern = args.Length > 0 ? args[0].ToString() : "";
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    string othis = self.oValue.ToString();
                    var f = args[1].oValue as Function;
                    var margs = new Arguments();
                    margs.length = 3;
                    margs[0] = pattern;
                    margs[2] = self;
                    int index = self.ToString().IndexOf(pattern);
                    if (index == -1)
                        return self;
                    margs[1] = index;
                    var res = othis.Substring(0, index) + f.Invoke(margs).ToString() + othis.Substring(index + pattern.Length);
                    self.oValue = othis;
                    self.valueType = JSObjectType.String;
                    return res;
                }
                else
                {
                    string replace = args.Length > 1 ? args[1].ToString() : "undefined";
                    if (string.IsNullOrEmpty(pattern))
                        return replace + self;
                    var str = self.ToString();
                    var index = str.IndexOf(pattern);
                    if (index == -1)
                        return self;
                    return str.Substring(0, index) + replace + str.Substring(index + pattern.Length);
                }
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(2)]
        public static JSObject slice(JSObject self, Arguments args)
        {
            string selfString = self.ToPrimitiveValue_Value_String().ToString();
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
        [InstanceMember]
        [ArgumentsLength(2)]
        public static JSObject split(JSObject self, Arguments args)
        {
            if (args.Length == 0 || !args[0].IsDefined)
                return new Array(new object[] { self.ToString() });
            uint limit = uint.MaxValue;
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
                            limit = (uint)limO.iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            limit = (uint)limO.dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(limO.ToString(), 0, out d, Tools.ParseNumberOptions.Default);
                            limit = (uint)d;
                            break;
                        }
                }
            }
            if (args[0].valueType == JSObjectType.Object && args[0].oValue is RegExp)
            {
                string selfString = self.ToPrimitiveValue_Value_String().ToString();
                var match = (args[0].oValue as RegExp).regEx.Match(selfString);
                if ((args[0].oValue as RegExp).regEx.ToString().Length == 0)
                {
                    match = match.NextMatch();
                    if (limit == uint.MaxValue)
                        limit = (uint)selfString.Length;
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
                string selfString = self.ToPrimitiveValue_Value_String().ToString();
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
        [InstanceMember]
        [ArgumentsLength(2)]
        public static JSObject substring(JSObject self, Arguments args)
        {
            string selfString = self.ToPrimitiveValue_Value_String().ToString();
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
        [InstanceMember]
        [ArgumentsLength(2)]
        public static JSObject substr(JSObject self, Arguments args)
        {
            if (args.Length == 0)
                return self;
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
            var selfs = self.ToString();
            int len = selfs.Length - pos0;
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
            if (pos0 < 0)
                pos0 += selfs.Length;
            if (pos0 < 0)
                pos0 = 0;
            if (pos0 >= selfs.Length || len <= 0)
                return "";
            if (selfs.Length < pos0 + len)
                len = selfs.Length - pos0;
            return selfs.Substring(pos0, len);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject toLocaleLowerCase(JSObject self)
        {
            var sstr = self.ToString();
            var res = sstr.ToLower();
            if (self.valueType == JSObjectType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject toLocaleUpperCase(JSObject self)
        {
            var sstr = self.ToString();
            var res = sstr.ToUpper();
            if (self.valueType == JSObjectType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject toLowerCase(JSObject self)
        {
            var sstr = self.ToString();
            var res = sstr.ToLowerInvariant();
            if (self.valueType == JSObjectType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject toUpperCase(JSObject self)
        {
            var sstr = self.ToString();
            var res = sstr.ToUpperInvariant();
            if (self.valueType == JSObjectType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject trim(JSObject self)
        {
            switch (self.valueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExists:
                case JSObjectType.NotExistsInObject:
                    throw new JSException(new TypeError("string can't be undefined"));
                case JSObjectType.Function:
                case JSObjectType.String:
                case JSObjectType.Object:
                    {
                        if (self.oValue == null)
                            throw new JSException(new TypeError("string can't be null"));
                        break;
                    }
            }
            try
            {
                var sb = new StringBuilder(self.ToString());
                int initialLength = sb.Length;
                int index = 0;
                for (; index < sb.Length && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index++)
                    ;
                if (index > 0)
                    sb.Remove(0, index);
                index = sb.Length - 1;
                for (; index >= 0 && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index--)
                    ;
                index++;
                if (index < sb.Length)
                    sb.Remove(index, sb.Length - index);
                if (sb.Length != initialLength)
                {
                    index = 0;
                    for (; ; )
                    {
                        for (; index < sb.Length && sb[index] != '\n' && sb[index] != '\r'; index++)
                            ;
                        if (index >= sb.Length)
                            break;
                        var startindex = index;
                        for (; index < sb.Length && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1; index++)
                            ;
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

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        [CLSCompliant(false)]
        public static JSObject toString(JSObject self)
        {
            if ((self as object) is String && self.valueType == JSObjectType.Object) // prototype instance
                return self.ToString();
            if (self.valueType == JSObjectType.String)
                return self;
            else
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject valueOf(JSObject self)
        {
            if ((self as object) is String && self.valueType == JSObjectType.Object) // prototype instance
                return self.ToString();
            if (self.valueType == JSObjectType.String)
                return self;
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
            [Hidden]
            get
            {
                var len = oValue.ToString().Length;
                if (this.GetType() == typeof(String))
                {
                    if (_length == null)
                        _length = new Number(len) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnumerate | JSObjectAttributesInternal.NotConfigurable };
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
            if (this.valueType != JSObjectType.String)
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
            return oValue.ToString();
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
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
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
            var namestr = name.ToString();
            if (namestr == "length")
                return length;
            return DefaultFieldGetter(name, forWrite, own); // обращение идёт к Объекту String, а не к значению string, поэтому члены создавать можно
        }

        #region HTML Wraping
        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject anchor(JSObject self, Arguments arg)
        {
            return "<a name=\"" + arg[0].Value + "\">" + self + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject big(JSObject self)
        {
            return "<big>" + self + "</big>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject blink(JSObject self)
        {
            return "<blink>" + self + "</blink>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject bold(JSObject self)
        {
            return "<bold>" + self + "</bold>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject @fixed(JSObject self)
        {
            return "<tt>" + self + "</tt>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject fontcolor(JSObject self, Arguments arg)
        {
            return "<font color=\"" + arg[0].Value + "\">" + self + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject fontsize(JSObject self, Arguments arg)
        {
            return "<font size=\"" + arg[0].Value + "\">" + self + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject italics(JSObject self)
        {
            return "<i>" + self + "</i>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject link(JSObject self, Arguments arg)
        {
            return "<a href=\"" + arg[0].Value + "\">" + self + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject small(JSObject self)
        {
            return "<small>" + self + "</small>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject strike(JSObject self)
        {
            return "<strike>" + self + "</strike>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject sub(JSObject self)
        {
            return "<sub>" + self + "</sub>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSObject sup(JSObject self)
        {
            return "<sup>" + self + "</sup>";
        }
        #endregion

        [Hidden]
        public static implicit operator String(string val)
        {
            return new String(val);
        }
    }
}