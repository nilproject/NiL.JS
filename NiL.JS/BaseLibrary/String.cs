using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class String : JSObject
    {
        [DoNotEnumerate]
        public static JSValue fromCharCode(Arguments args)
        {
            if (args == null || args.Length == 0)
                return new String();

            int chc = 0;
            string res = "";
            for (int i = 0; i < args.Length; i++)
            {
                chc = Tools.JSObjectToInt32(args[i]);
                res += ((char)chc).ToString();
            }
            return res;
        }

        [DoNotEnumerate]
        public static JSValue fromCodePoint(Arguments args)
        {
            if (args == null || args.Length == 0)
                return new String();

            JSValue n;
            int ucs = 0;
            string res = "";
            for (int i = 0; i < args.Length; i++)
            {
                n = Tools.JSObjectToNumber(args[i]);
                if (n._valueType == JSValueType.Integer)
                {
                    if (n._iValue < 0 || n._iValue > 0x10FFFF)
                        ExceptionHelper.Throw(new RangeError("Invalid code point " + Tools.Int32ToString(n._iValue)));
                    ucs = n._iValue;
                }
                else if (n._valueType == JSValueType.Double)
                {
                    if (n._dValue < 0 || n._dValue > 0x10FFFF || double.IsInfinity(n._dValue) || double.IsNaN(n._dValue) || n._dValue % 1.0 != 0.0)
                        ExceptionHelper.Throw(new RangeError("Invalid code point " + Tools.DoubleToString(n._dValue)));
                    ucs = (int)n._dValue;
                }

                res += Tools.CodePointToString(ucs);
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
        public static String charAt(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.charAt called on null or undefined"));

            var selfStr = self.BaseToString();

            int p = Tools.JSObjectToInt32(args[0], true);
            if (p < 0 || p >= selfStr.Length)
                return "";

            return selfStr[p].ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue charCodeAt(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.charCodeAt called on null or undefined"));

            var selfStr = self.BaseToString();

            int p = Tools.JSObjectToInt32(args[0], true);
            if (p < 0 || p >= selfStr.Length)
                return Number.NaN;

            return new Number((int)selfStr[p]);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue codePointAt(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.codePointAt called on null or undefined"));

            var selfStr = self.BaseToString();

            int p = Tools.JSObjectToInt32(args[0], true);
            if (p < 0 || p >= selfStr.Length)
                return JSValue.undefined;

            return Tools.NextCodePoint(selfStr, ref p);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue concat(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.concat called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return selfStr;

            if (args.Length == 1)
                return string.Concat(selfStr, args[0].ToString());
            if (args.Length == 2)
                return string.Concat(selfStr, args[0].ToString(), args[1].ToString());
            if (args.Length == 3)
                return string.Concat(selfStr, args[0].ToString(), args[1].ToString(), args[2].ToString());
            if (args.Length == 4)
                return string.Concat(selfStr, args[0].ToString(), args[1].ToString(), args[2].ToString(), args[3].ToString());

            var res = new StringBuilder().Append(selfStr);
            for (var i = 0; i < args.Length; i++)
                res.Append(args[i]);
            return res.ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue endsWith(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.endsWith called on null or undefined"));

            var selfStr = self.BaseToString();

            var value = (args?[0] ?? undefinedString).ToString();

            return selfStr.EndsWith(value, StringComparison.Ordinal) ? Boolean.True : Boolean.False;
        }


        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue includes(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.includes called on null or undefined"));

            var selfStr = self.BaseToString();

            var value = (args?[0] ?? undefinedString).ToString();

            return selfStr.IndexOf(value, StringComparison.Ordinal) != -1 ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue indexOf(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.indexOf called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return -1;

            string fstr = args[0].ToString();

            var pos = 0;
            if (args.Length > 1)
            {
                pos = Tools.JSObjectToInt32(args[1], 0, 0, true);

                if (pos < 0)
                    pos = 0;

                if (pos > selfStr.Length)
                    pos = selfStr.Length - 1;
            }

            return selfStr.IndexOf(fstr, pos, StringComparison.Ordinal);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue lastIndexOf(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.lastIndexOf called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return -1;

            string fstr = args[0].ToString();

            var pos = selfStr.Length;
            if (args.Length > 1)
            {
                var posArg = args[1];
                if (posArg.ValueType >= JSValueType.Object)
                    posArg = posArg.ToPrimitiveValue_Value_String();

                pos = Tools.JSObjectToInt32(posArg, pos, pos, true);

                if (pos < 0)
                    pos = 0;

                pos += fstr.Length;

                if (pos > selfStr.Length)
                    pos = selfStr.Length;
            }

            return selfStr.LastIndexOf(fstr, pos, StringComparison.Ordinal);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue localeCompare(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.localeCompare called on null or undefined"));

            var selfStr = self.BaseToString();

            string str1 = args[0].ToString();
            return string.CompareOrdinal(selfStr, str1);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue match(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));

            var a0 = args[0];
            var regex = a0.Value as RegExp;

            if (regex == null)
                regex = new RegExp((a0._valueType > JSValueType.Undefined ? (object)a0 : "").ToString(), ""); // cached

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

// Not implemented in .Net Standard 1.3. Will be in .Net Standard 2.0
#if !NETCORE && !PORTABLE
        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue normalize(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.normalize called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return selfStr.Normalize(NormalizationForm.FormC);

            string form = "NFC";
            var a0 = args[0];
            if (a0 != null && a0._valueType > JSValueType.Undefined)
                form = a0.ToString();

            var nf = NormalizationForm.FormC;
            if (form == "NFC")
                nf = NormalizationForm.FormC;
            else if (form == "NFD")
                nf = NormalizationForm.FormD;
            else if (form == "NFKC")
                nf = NormalizationForm.FormKC;
            else if (form == "NFKD")
                nf = NormalizationForm.FormKD;
            else
                ExceptionHelper.Throw(new RangeError("The normalization form should be one of NFC, NFD, NFKC, NFKD"));

            return selfStr.Normalize(nf);
        }
#endif

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue padEnd(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.codePointAt called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return selfStr;

            int p = Tools.JSObjectToInt32(args[0], true);
            if (p <= selfStr.Length)
                return selfStr;

            if (args.Length >= 2 && args[1] != null && args[1].Defined)
            {
                string filler = args[1].ToString();

                if (string.IsNullOrEmpty(filler))
                    return selfStr;
                if (filler.Length == 1)
                    return selfStr.PadRight(p, filler[0]);

                var s = new StringBuilder(p);

                s.Append(selfStr);
                for (int i = p / filler.Length - 1; i >= 0; i--)
                    s.Append(filler);
                s.Append(filler.Substring(0, p % filler.Length));

                return s.ToString();
            }

            return selfStr.PadRight(p, ' ');
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue padStart(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.codePointAt called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return selfStr;

            int p = Tools.JSObjectToInt32(args[0], true);
            if (p <= selfStr.Length)
                return selfStr;

            if (args.Length >= 2 && args[1] != null && args[1].Defined)
            {
                string filler = args[1].ToString();

                if (string.IsNullOrEmpty(filler))
                    return selfStr;
                if (filler.Length == 1)
                    return selfStr.PadLeft(p, filler[0]);

                var s = new StringBuilder(p);

                for (int i = p / filler.Length - 1; i >= 0; i--)
                    s.Append(filler);
                s.Append(filler.Substring(0, p % filler.Length));
                s.Append(selfStr);

                return s.ToString();
            }

            return selfStr.PadLeft(p, ' ');
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue repeat(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.repeat called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return "";
            if (selfStr.Length == 0)
                return "";

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
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.replace called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return selfStr;

            var a0 = args[0];
            var regex = a0?.Value as RegExp;

            var a1 = args[1];
            var f = a1?.Value as Function;

            if (regex != null)
            {
                var re = regex._regex;
                if (f != null)
                {
                    var str = new String(selfStr);
                    var match = new String();
                    var fArgs = new Arguments(Context.CurrentContext);
                    return re.Replace(
                        selfStr,
                        (m) =>
                        {
                            str._oValue = selfStr;
                            str._valueType = JSValueType.String;
                            match._oValue = m.Value;
                            match._valueType = JSValueType.String;

                            fArgs.length = m.Groups.Count + 2;
                            fArgs[0] = match;

                            for (int i = 1; i < m.Groups.Count; i++)
                                fArgs[i] = m.Groups[i].Value;

                            fArgs[fArgs.length - 2] = m.Index;
                            fArgs[fArgs.length - 1] = str;

                            return f.Call(fArgs).ToString();
                        },
                        regex._global ? int.MaxValue : 1);
                }
                else
                {
                    string replacement = args.Length > 1 ? (a1 ?? "").ToString() : "undefined";
                    return re.Replace(selfStr, replacement, regex._global ? int.MaxValue : 1);
                }
            }
            else
            {
                string pattern = (a0 ?? "").ToString();
                if (f != null)
                {
                    int index = selfStr.IndexOf(pattern, StringComparison.Ordinal);
                    if (index == -1)
                        return selfStr;

                    var fArgs = new Arguments(Context.CurrentContext) { length = 3 };
                    fArgs[0] = pattern;
                    fArgs[1] = index;
                    fArgs[2] = self;

                    return selfStr.Substring(0, index) + f.Call(fArgs).ToString() + selfStr.Substring(index + pattern.Length);
                }
                else
                {
                    string replacement = args.Length > 1 ? (a1 ?? "").ToString() : "undefined";
                    if (pattern.Length == 0)
                        return replacement + selfStr;

                    var index = selfStr.IndexOf(pattern, StringComparison.Ordinal);
                    if (index == -1)
                        return selfStr;

                    return selfStr.Substring(0, index) + replacement + selfStr.Substring(index + pattern.Length);
                }
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue search(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.search called on null or undefined"));

            string selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return 0;

            var a0 = args[0];
            var regex = a0.Value as RegExp;

            if (regex == null)
                return selfStr.IndexOf(a0.ToString(), StringComparison.Ordinal);

            var match = regex._regex.Match(selfStr);
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
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.slice called on null or undefined"));

            string selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return self.BaseToString();

            int pos0 = Tools.JSObjectToInt32(args[0], 0, 0, 0, true);
            int pos1 = Tools.JSObjectToInt32(args[1], 0, selfStr.Length, 0, true);

            if (pos0 < 0)
                pos0 += selfStr.Length;
            if (pos1 < 0)
                pos1 += selfStr.Length;
            pos0 = System.Math.Min(System.Math.Max(0, pos0), selfStr.Length);
            pos1 = System.Math.Min(System.Math.Max(0, pos1), selfStr.Length);

            if (pos0 >= pos1)
                return new String();

            return selfStr.Substring(pos0, pos1 - pos0);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue split(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.split called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0 || !args[0].Defined)
                return new Array { selfStr };

            var a0 = args[0];
            var regex = a0.Value as RegExp;

            var limit = (uint)Tools.JSObjectToInt64(args[1], long.MaxValue, true);

            if (limit == 0)
                return new Array();

            if (regex != null)
            {
                var re = regex._regex;

                var m = re.Match(selfStr, 0);
                if (!m.Success)
                    return new Array(new JSValue[] { selfStr });

                Array res = new Array();

                int index = 0;
                while (res._data.Length < limit)
                {
                    if (index > 0)
                        m = m.NextMatch();

                    if (!m.Success)
                    {
                        res._data.Add(selfStr.Substring(index, selfStr.Length - index));
                        break;
                    }

                    if (m.Index >= selfStr.Length)
                        break;

                    int nindex = m.Index + (m.Length == 0 ? 1 : 0);
                    var item = selfStr.Substring(index, nindex - index);
                    res._data.Add(item);

                    if (nindex < selfStr.Length)
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
                string fstr = a0.ToString();

                if (string.IsNullOrEmpty(fstr))
                {
                    int len = System.Math.Min(selfStr.Length, (int)System.Math.Min(int.MaxValue, limit));
                    var arr = new JSValue[len];
                    for (var i = 0; i < len; i++)
                        arr[i] = new String(selfStr[i].ToString());
                    return new Array(arr);
                }
                else
                {
                    Array res = new Array();
                    int index = 0;
                    while (res._data.Length < limit)
                    {
                        int nindex = selfStr.IndexOf(fstr, index, StringComparison.Ordinal);
                        if (nindex == -1)
                        {
                            res._data.Add(selfStr.Substring(index, selfStr.Length - index));
                            break;
                        }
                        else
                        {
                            res._data.Add(selfStr.Substring(index, nindex - index));
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
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.startsWith called on null or undefined"));

            var selfStr = self.BaseToString();

            var value = (args?[0] ?? undefinedString).ToString();

            return selfStr.StartsWith(value, StringComparison.Ordinal) ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue substring(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.substring called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args == null || args.Length == 0)
                return self.BaseToString();

            int pos0 = Tools.JSObjectToInt32(args[0], 0, 0, 0, true);
            int pos1 = Tools.JSObjectToInt32(args[1], 0, selfStr.Length, 0, true);

            if (pos0 > pos1)
            {
                pos0 ^= pos1;
                pos1 ^= pos0;
                pos0 ^= pos1;
            }

            pos0 = System.Math.Max(0, System.Math.Min(pos0, selfStr.Length));
            pos1 = System.Math.Max(0, System.Math.Min(pos1, selfStr.Length));

            return selfStr.Substring(pos0, System.Math.Min(selfStr.Length, System.Math.Max(0, pos1 - pos0)));
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue substr(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.substr called on null or undefined"));

            var selfStr = self.BaseToString();

            if (args.Length == 0)
                return self;

            int start = Tools.JSObjectToInt32(args[0], 0, 0, 0, true);
            int length = Tools.JSObjectToInt32(args[1], 0, selfStr.Length, 0, true);

            if (start < 0)
                start += selfStr.Length;
            if (start < 0)
                start = 0;
            if (start >= selfStr.Length || length <= 0)
                return "";

            if (selfStr.Length < start + length)
                length = selfStr.Length - start;

            return selfStr.Substring(start, length);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLocaleLowerCase(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.toLocaleLowerCase called on null or undefined"));

            return self.ToString().ToLower();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLocaleUpperCase(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.toLocaleUpperCase called on null or undefined"));

            return self.ToString().ToUpper();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toLowerCase(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.toLowerCase called on null or undefined"));

            return self.ToString().ToLowerInvariant();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue toUpperCase(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.toUpperCase called on null or undefined"));

            return self.ToString().ToUpperInvariant();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue trim(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.trim called on null or undefined"));

            var selfStr = self.BaseToString();

            if (selfStr == "")
                return selfStr;

            return selfStr.Trim(Tools.TrimChars);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        [CLSCompliant(false)]
        public static JSValue toString(JSValue self)
        {
            if ((self as object) is String && self._valueType == JSValueType.Object) // prototype instance
                return self.BaseToString();
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
                return self.BaseToString();
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

        #region HTML Wrapping
        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue anchor(JSValue self, Arguments arg)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.anchor called on null or undefined"));

            return "<a name=\"" + arg[0].Value + "\">" + self.ToString() + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue big(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.big called on null or undefined"));

            return "<big>" + self.ToString() + "</big>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue blink(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.blink called on null or undefined"));

            return "<blink>" + self.ToString() + "</blink>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue bold(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.bold called on null or undefined"));

            return "<bold>" + self.ToString() + "</bold>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue @fixed(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.fixed called on null or undefined"));

            return "<tt>" + self.ToString() + "</tt>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue fontcolor(JSValue self, Arguments arg)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.fontcolor called on null or undefined"));

            return "<font color=\"" + arg[0].Value + "\">" + self.ToString() + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue fontsize(JSValue self, Arguments arg)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.fontsize called on null or undefined"));

            return "<font size=\"" + arg.Value + "\">" + self.ToString() + "</font>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue italics(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.italics called on null or undefined"));

            return "<i>" + self.ToString() + "</i>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue link(JSValue self, Arguments arg)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.link called on null or undefined"));

            return "<a href=\"" + arg[0].Value + "\">" + self.ToString() + "</a>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue small(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.small called on null or undefined"));

            return "<small>" + self.ToString() + "</small>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue strike(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.strike called on null or undefined"));

            return "<strike>" + self.ToString() + "</strike>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue sub(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.sub called on null or undefined"));

            return "<sub>" + self.ToString() + "</sub>";
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static JSValue sup(JSValue self)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.sup called on null or undefined"));

            return "<sup>" + self.ToString() + "</sup>";
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
