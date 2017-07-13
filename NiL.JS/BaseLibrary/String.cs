using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class String : JSObject
    {
        [DoNotEnumerate]
        public static JSValue fromCharCode(Arguments code)
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
        public static JSValue fromCodePoint(Arguments code)
        {
            if (code == null || code.Length == 0)
                return new String();

            JSValue n;
            uint ucs = 0;
            string res = "";
            for (int i = 0; i < code.Length; i++)
            {
                n = Tools.JSObjectToNumber(code[i]);
                if (n._valueType == JSValueType.Integer)
                {
                    if (n._iValue < 0 || n._iValue > 0x10FFFF)
                        ExceptionHelper.Throw((new RangeError("Invalid code point " + Tools.Int32ToString(n._iValue))));
                    ucs = (uint)n._iValue;
                }
                else if (n._valueType == JSValueType.Double)
                {
                    if (n._dValue < 0 || n._dValue > 0x10FFFF || double.IsInfinity(n._dValue) || double.IsNaN(n._dValue) || n._dValue % 1.0 != 0.0)
                        ExceptionHelper.Throw((new RangeError("Invalid code point " + Tools.DoubleToString(n._dValue))));
                    ucs = (uint)n._dValue;
                }

                if (ucs >= 0 && ucs <= 0xFFFF)
                    res += ((char)ucs).ToString();
                else if (ucs > 0xFFFF && ucs <= 0x10FFFF)
                {
                    ucs -= 0x10000;
                    char h = (char)((ucs >> 10) + 0xD800);
                    char l = (char)((ucs % 0x400) + 0xDC00);
                    res += h.ToString() + l.ToString();
                }
                else
                    ExceptionHelper.Throw((new RangeError("Invalid code point " + ucs.ToString())));
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
        [StrictConversion]
        public String(string s)
        {
            _oValue = s ?? "null";
            _valueType = JSValueType.String;
            _attributes |= JSValueAttributesInternal.SystemObject;
        }

        [Hidden]
        public JSValue this[int pos]
        {
            [Hidden]
            get
            {
                if ((pos < 0) || (pos >= _oValue.ToString().Length))
                    return JSValue.notExists;
                return new JSValue() { _valueType = JSValueType.String, _oValue = (_oValue.ToString())[pos].ToString(), _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.DoNotDelete };
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static String charAt(JSValue self, Arguments pos)
        {
            var strValue = self.ToString();
            int p = Tools.JSObjectToInt32(pos[0], true);
            if ((p < 0) || (p >= strValue.Length))
                return "";
            return strValue[p].ToString();//Tools.charStrings[strValue[p]];
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue charCodeAt(JSValue self, Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos[0], true);
            var selfStr = self.ToString();
            if ((p < 0) || (p >= selfStr.Length))
                return Number.NaN;
            var res = new JSValue()
            {
                _valueType = JSValueType.Integer,
                _iValue = selfStr[p],
            };
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue codePointAt(JSValue self, Arguments pos)
        {
            int p = Tools.JSObjectToInt32(pos[0], true);
            var selfStr = self.ToString();
            if ((p < 0) || (p >= selfStr.Length))
                return JSValue.undefined;

            // look here in section 3.7 Surrogates for more information.
            // http://unicode.org/versions/Unicode3.0.0/ch03.pdf

            int code = selfStr[p];
            if (p + 1 < selfStr.Length && code >= 0xD800 && code <= 0xDBFF)
            {
                // code is the high part
                int low = selfStr[p + 1];
                if (low >= 0xDC00 && low <= 0xDFFF)
                    code = (code - 0xD800) * 0x400 + (low - 0xDC00) + 0x10000;
            }

            var res = new JSValue()
            {
                _valueType = JSValueType.Integer,
                _iValue = code,
            };
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue concat(JSValue self, Arguments args)
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
            var res = new StringBuilder().Append(self);
            for (var i = 0; i < args.Length; i++)
                res.Append(args[i]);
            return res.ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue endsWith(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.EndsWith(value) ? Boolean.True : Boolean.False;
        }


        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue includes(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.IndexOf(value) != -1 ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue indexOf(JSValue self, Arguments args)
        {
            if (args.Length == 0)
                return -1;

            var strValue = self.ToString();
            string fstr = args[0].ToString();

            var pos = 0;
            if (args.length > 1)
            {
                pos = Tools.JSObjectToInt32(args[1], 0, 0, true);

                if (pos < 0)
                    pos = 0;

                if (pos > strValue.Length)
                    pos = strValue.Length - 1;
            }

            return strValue.IndexOf(fstr, pos, StringComparison.Ordinal);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue lastIndexOf(JSValue self, Arguments args)
        {
            if (args.Length == 0)
                return -1;

            var strValue = self.ToString();
            string fstr = args[0].ToString();

            var pos = strValue.Length;
            if (args.length > 1)
            {
                pos = Tools.JSObjectToInt32(args[1], pos, pos, true);

                if (pos < 0)
                    pos = 0;

                pos += fstr.Length;

                if (pos > strValue.Length)
                    pos = strValue.Length;
            }

            return strValue.LastIndexOf(fstr, pos, StringComparison.Ordinal);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue localeCompare(JSValue self, Arguments args)
        {
            string str0 = self.ToString();
            string str1 = args[0].ToString();
            return string.CompareOrdinal(str0, str1);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue match(JSValue self, Arguments args)
        {
            if (self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));

            var a0 = args[0];
            var regex = a0.Value as RegExp;

            if (regex == null)
                regex = new RegExp((a0._valueType > JSValueType.Undefined ? (object)a0 : "").ToString(), "", false); // cached

            if (!regex._global)
            {
                return regex.exec(self);
            }
            else
            {
                regex.lastIndex = 0;

                if (regex.sticky)
                {
                    var match = regex._regex.Match(self.ToString());
                    if (!match.Success || match.Index != 0)
                        return null;

                    var res = new Array();
                    res._data[0] = match.Value;

                    int li = 0;
                    while (true)
                    {
                        match = match.NextMatch();
                        if (!match.Success || match.Index != ++li)
                            break;
                        res._data[li] = match.Value;
                    }
                    return res;
                }
                else
                {
                    var matches = regex._regex.Matches(self.ToString());
                    if (matches.Count == 0)
                        return null;

                    var res = new JSValue[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                        res[i] = matches[i].Value;

                    return new Array(res);
                }
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue normalize(JSValue self, Arguments args)
        {
            var selfStr = (self ?? undefinedString).ToString();

            var form = "NFC";
            if (args != null && args.Length > 0)
            {
                var a0 = args[0];
                if (a0 != null && a0._valueType > JSValueType.Undefined)
                    form = a0.ToString();
            }

            switch (form)
            {
                case "NFC":
                    {
                        selfStr = selfStr.Normalize(NormalizationForm.FormC);
                        break;
                    }
                case "NFD":
                    {
                        selfStr = selfStr.Normalize(NormalizationForm.FormD);
                        break;
                    }
                case "NFKC":
                    {
                        selfStr = selfStr.Normalize(NormalizationForm.FormKC);
                        break;
                    }
                case "NFKD":
                    {
                        selfStr = selfStr.Normalize(NormalizationForm.FormKD);
                        break;
                    }
                default:
                    ExceptionHelper.Throw(new RangeError("The normalization form should be one of NFC, NFD, NFKC, NFKD"));
                    break;
            }
            return selfStr;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue repeat(JSValue self, Arguments args)
        {
            var selfStr = (self ?? undefinedString).ToString();

            double count = 0;
            if (args.Length > 0)
                count = Tools.JSObjectToDouble(args[0]);
            if (double.IsNaN(count))
                count = 0;
            count = System.Math.Truncate(count);

            if (count < 0 || double.IsInfinity(count))
                ExceptionHelper.Throw(new RangeError("Invalid count value"));

            int c = (int)count;

            if (c == 0)
                return "";
            if (c == 1)
                return selfStr;
            if (selfStr.Length == 0)
                return "";

            var s = new StringBuilder(selfStr.Length * c);
            for (int i = 0; i < c; i++)
                s.Append(selfStr);

            return s.ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        [AllowNullArguments]
        public static JSValue replace(JSValue self, Arguments args)
        {
            if (args == null || args.length == 0)
                return self;
            if ((args[0] ?? @null)._valueType == JSValueType.Object
                && (args[0] ?? @null).Value != null
                && args[0].Value.GetType() == typeof(RegExp))
            {
                var f = args[1]._oValue as Function;
                if (args.length > 1 && f != null)
                {
                    string temp = self._oValue.ToString();
                    var match = new String();
                    var margs = new Arguments();
                    match._oValue = (args[0]._oValue as RegExp)._Regex.Replace(self.ToString(),
                        (m) =>
                        {
                            self._oValue = temp;
                            self._valueType = JSValueType.String;
                            margs.length = m.Groups.Count + 2;

                            JSValue t;
                            for (int i = 1; i < m.Groups.Count; i++)
                            {
                                t = m.Groups[i].Value;
                                margs[i] = t;
                            }

                            t = m.Index;
                            match._oValue = m.Value;
                            margs[0] = match;
                            margs[margs.length - 2] = t;
                            margs[margs.length - 1] = self;

                            return f.Call(margs).ToString();
                        }, (args[0].Value as RegExp)._global ? int.MaxValue : 1);
                    self._oValue = temp;
                    self._valueType = JSValueType.String;
                    return match;
                }
                else
                {
                    return (args[0].Value as RegExp)._Regex.Replace(self.ToString(), args.Length > 1 ? args[1].ToString() : "undefined", (args[0].Value as RegExp)._global ? int.MaxValue : 1);
                }
            }
            else
            {
                string pattern = args.Length > 0 ? args[0].ToString() : "";
                var f = args[1]._oValue as Function;
                if (args.Length > 1 && f != null)
                {
                    string othis = self._oValue.ToString();
                    var margs = new Arguments();
                    margs.length = 3;
                    margs[0] = pattern;
                    margs[2] = self;
                    int index = self.ToString().IndexOf(pattern);
                    if (index == -1)
                        return self;
                    margs[1] = index;
                    var res = othis.Substring(0, index) + f.Call(margs).ToString() + othis.Substring(index + pattern.Length);
                    self._oValue = othis;
                    self._valueType = JSValueType.String;
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
        [ArgumentsCount(1)]
        public static JSValue search(JSValue self, Arguments args)
        {
            if (self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));

            if (args.length == 0)
                return 0;

            var a0 = args[0];
            var regex = a0.Value as RegExp;

            if (regex == null)
                return self.ToString().IndexOf(a0.ToString());

            var match = regex._regex.Match(self.ToString());
            if (!match.Success)
                return -1;

            if (regex.sticky && match.Index != 0)
                return -1;

            return match.Index;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue slice(JSValue self, Arguments args)
        {
            string selfString = self.ToPrimitiveValue_Value_String().ToString();
            if (args.Length == 0)
                return selfString;
            int pos0 = 0;
            switch (args[0]._valueType)
            {
                case JSValueType.Integer:
                case JSValueType.Boolean:
                    {
                        pos0 = args[0]._iValue;
                        break;
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(args[0]._dValue) || double.IsNegativeInfinity(args[0]._dValue))
                            pos0 = 0;
                        else if (double.IsPositiveInfinity(args[0]._dValue))
                            pos0 = selfString.Length;
                        else
                            pos0 = (int)args[0]._dValue;
                        break;
                    }
                case JSValueType.Object:
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.String:
                    {
                        pos0 = Tools.JSObjectToInt32(args[0], 0, true);
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1]._valueType)
                {
                    case JSValueType.Integer:
                    case JSValueType.Boolean:
                        {
                            pos1 = args[1]._iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            if (double.IsNaN(args[1]._dValue) || double.IsNegativeInfinity(args[0]._dValue))
                                pos1 = 0;
                            else if (double.IsPositiveInfinity(args[1]._dValue))
                                pos1 = selfString.Length;
                            else
                                pos1 = (int)args[1]._dValue;
                            break;
                        }
                    case JSValueType.Object:
                    case JSValueType.Date:
                    case JSValueType.Function:
                    case JSValueType.String:
                        {
                            //double d;
                            //Tools.ParseNumber(args[1].ToPrimitiveValue_Value_String().ToString(), pos1, out d, Tools.ParseNumberOptions.Default);
                            //pos1 = (int)d;
                            pos1 = Tools.JSObjectToInt32(args[1], 0, true);
                            break;
                        }
                    case JSValueType.NotExists:
                    case JSValueType.NotExistsInObject:
                    case JSValueType.Undefined:
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
        [ArgumentsCount(2)]
        public static JSValue split(JSValue self, Arguments args)
        {
            if (self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));

            if (args == null || args.Length == 0 || !args[0].Defined)
                return new Array(new JSValue[] { self.ToString() });

            var a0 = args[0];
            var regex = a0?.Value as RegExp;

            var selfString = self.ToString();
            var limit = (uint)Tools.JSObjectToInt64(args[1], long.MaxValue, true);

            if (limit == 0)
                return new Array();

            if (regex != null)
            {
                var re = regex._regex;

                var m = re.Match(selfString, 0);
                if (!m.Success)
                    return new Array(new JSValue[] { selfString });

                Array res = new Array();

                int index = 0;
                while (res._data.Length < limit)
                {
                    if (index > 0)
                        m = m.NextMatch();

                    if (!m.Success)
                    {
                        res._data.Add(selfString.Substring(index, selfString.Length - index));
                        break;
                    }

                    if (m.Index >= selfString.Length)
                        break;

                    int nindex = m.Index + (m.Length == 0 ? 1 : 0);
                    var item = selfString.Substring(index, nindex - index);
                    res._data.Add(item);

                    if (nindex < selfString.Length)
                    {
                        for (int i = 1; i < m.Groups.Count; i++)
                        {
                            if (res._data.Length >= limit)
                                break;
                            res._data.Add(m.Groups[i].Success ? m.Groups[i].Value : undefined);
                        }
                    }

                    index = nindex + m.Length;
                }

                return res;
            }
            else
            {
                string fstr = a0?.ToString();

                if (string.IsNullOrEmpty(fstr))
                {
                    int len = System.Math.Min(selfString.Length, (int)System.Math.Min(int.MaxValue, limit));
                    var arr = new JSValue[len];
                    for (var i = 0; i < len; i++)
                        arr[i] = new String(selfString[i].ToString());
                    return new Array(arr);
                }
                else
                {
                    Array res = new Array();
                    int index = 0;
                    while (res._data.Length < limit)
                    {
                        int nindex = selfString.IndexOf(fstr, index, StringComparison.Ordinal);
                        if (nindex == -1)
                        {
                            res._data.Add(selfString.Substring(index, selfString.Length - index));
                            break;
                        }
                        else
                        {
                            res._data.Add(selfString.Substring(index, nindex - index));
                            index = nindex + fstr.Length;
                        }
                    }
                    return res;
                }
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue startsWith(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.StartsWith(value) ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue substring(JSValue self, Arguments args)
        {
            string selfString = self.ToString();
            if (args.Length == 0)
                return selfString;

            int pos0 = Tools.JSObjectToInt32(args[0], 0, 0, 0, true);
            int pos1 = Tools.JSObjectToInt32(args[1], 0, selfString.Length, 0, true);

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
        [ArgumentsCount(2)]
        public static JSValue substr(JSValue self, Arguments args)
        {
            if (args.Length == 0)
                return self;

            var selfString = self.ToString();

            int start = Tools.JSObjectToInt32(args[0], 0, 0, 0, true);
            int length = Tools.JSObjectToInt32(args[1], 0, selfString.Length, 0, true);

            if (start < 0)
                start += selfString.Length;
            if (start < 0)
                start = 0;
            if (start >= selfString.Length || length <= 0)
                return "";

            if (selfString.Length < start + length)
                length = selfString.Length - start;

            return selfString.Substring(start, length);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLocaleLowerCase(JSValue self)
        {
            var sstr = self.ToString();
            var res = sstr.ToLower();
            if (self._valueType == JSValueType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLocaleUpperCase(JSValue self)
        {
            var sstr = self.ToString();
            var res = sstr.ToUpper();
            if (self._valueType == JSValueType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLowerCase(JSValue self)
        {
            var sstr = self.ToString();
            var res = sstr.ToLowerInvariant();
            if (self._valueType == JSValueType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toUpperCase(JSValue self)
        {
            var sstr = self.ToString();
            var res = sstr.ToUpperInvariant();
            if (self._valueType == JSValueType.String && string.CompareOrdinal(sstr, res) == 0)
                return self;
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue trim(JSValue self)
        {
            switch (self._valueType)
            {
                case JSValueType.Undefined:
                case JSValueType.NotExists:
                case JSValueType.NotExistsInObject:
                    {
                        ExceptionHelper.Throw(new TypeError("string can't be undefined"));
                        break;
                    }
                case JSValueType.Function:
                case JSValueType.String:
                case JSValueType.Object:
                    {
                        if (self._oValue == null)
                            ExceptionHelper.Throw(new TypeError("string can't be null"));
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
                while (index >= 0 && System.Array.IndexOf(Tools.TrimChars, sb[index]) != -1)
                    index--;
                index++;
                if (index < sb.Length)
                    sb.Remove(index, sb.Length - index);
                if (sb.Length != initialLength)
                {
                    index = 0;
                    for (;;)
                    {
                        while (index < sb.Length && sb[index] != '\n' && sb[index] != '\r')
                            index++;
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
        [ArgumentsCount(0)]
        [CLSCompliant(false)]
        public static JSValue toString(JSValue self)
        {
            if ((self as object) is String && self._valueType == JSValueType.Object) // prototype instance
                return self.ToString();
            if (self._valueType != JSValueType.String)
                ExceptionHelper.Throw(new TypeError("Try to call String.toString for not string object."));
            return self;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue valueOf(JSValue self)
        {
            if ((self as object) is String && self._valueType == JSValueType.Object) // prototype instance
                return self.ToString();
            if (self._valueType != JSValueType.String)
                ExceptionHelper.Throw(new TypeError("Try to call String.valueOf for not string object."));
            return self;
        }

        private Number _length = null;

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSValue length
        {
            [Hidden]
            get
            {
                var len = _oValue.ToString().Length;
                if (_length == null)
                    _length = new Number(len) { _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.NonConfigurable };
                else
                    _length._iValue = len;
                return _length;
            }
        }

        [Hidden]
        public override string ToString()
        {
            if (this._valueType != JSValueType.String)
                ExceptionHelper.Throw(new TypeError("Try to call String.toString for not string object."));
            return _oValue.ToString();
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            if (obj is String)
                return _oValue.Equals((obj as String)._oValue);
            return false;
        }

        [Hidden]
        public override int GetHashCode()
        {
            return _oValue.GetHashCode();
        }

        [Hidden]
        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                int index = 0;
                double dindex = Tools.JSObjectToDouble(key);
                if (!double.IsInfinity(dindex)
                    && !double.IsNaN(dindex)
                    && ((index = (int)dindex) == dindex)
                    && ((index = (int)dindex) == dindex)
                    && index < (_oValue.ToString()).Length
                    && index >= 0)
                {
                    return this[index];
                }
                var namestr = key.ToString();
                if (namestr == "length")
                    return length;
            }
            return base.GetProperty(key, forWrite, memberScope); // обращение идёт к Объекту String, а не к значению string, поэтому члены создавать можно
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            var str = _oValue.ToString();
            var len = str.Length;
            for (var i = 0; i < len; i++)
            {
                if (str[i] >= '\uD800' && str[i] <= '\uDBFF' && i + 1 < len && str[i + 1] >= '\uDC00' && str[i + 1] <= '\uDFFF') // Unicode surrogates
                {
                    yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumeratorMode > 0 ? str.Substring(i, 2) : null);
                    i++;
                }
                else
                    yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumeratorMode > 0 ? str[i].ToString() : null);
            }

            if (!hideNonEnum)
                yield return new KeyValuePair<string, JSValue>("length", length);
            if (_fields != null)
            {
                foreach (var f in _fields)
                {
                    if (f.Value.Exists && (!hideNonEnum || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                        yield return f;
                }
            }
        }

        public static JSValue raw(Arguments args)
        {
            var result = new StringBuilder();
            var strings = args[0].Value as Array ?? Tools.arraylikeToArray(args[0], true, false, false, -1);

            for (var i = 0; i < strings._data.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(args[i]);
                }

                result.Append(strings._data[i]);
            }

            return result.ToString();
        }

        #region HTML Wraping
        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue anchor(JSValue self, Arguments arg)
        {
            return "<a name=\"" + arg[0].Value + "\">" + self + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue big(JSValue self)
        {
            return "<big>" + self + "</big>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue blink(JSValue self)
        {
            return "<blink>" + self + "</blink>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue bold(JSValue self)
        {
            return "<bold>" + self + "</bold>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue @fixed(JSValue self)
        {
            return "<tt>" + self + "</tt>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue fontcolor(JSValue self, Arguments arg)
        {
            return "<font color=\"" + arg[0].Value + "\">" + self + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue fontsize(JSValue self, Arguments arg)
        {
            return "<font size=\"" + arg.Value + "\">" + self + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue italics(JSValue self)
        {
            return "<i>" + self + "</i>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue link(JSValue self, Arguments arg)
        {
            return "<a href=\"" + arg[0].Value + "\">" + self + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue small(JSValue self)
        {
            return "<small>" + self + "</small>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue strike(JSValue self)
        {
            return "<strike>" + self + "</strike>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue sub(JSValue self)
        {
            return "<sub>" + self + "</sub>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue sup(JSValue self)
        {
            return "<sup>" + self + "</sup>";
        }
        #endregion
        
        [Hidden]
        public static implicit operator String(string val)
        {
            return new String(val);
        }

        [Hidden]
        public static implicit operator string(String val)
        {
            return val._oValue.ToString();
        }
    }
}
