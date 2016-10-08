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
        [ArgumentsLength(1)]
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
        [ArgumentsLength(1)]
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
        [ArgumentsLength(1)]
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
        [ArgumentsLength(1)]
        public static JSValue endsWith(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.EndsWith(value) ? Boolean.True : Boolean.False;
        }


        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue includes(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.IndexOf(value) != -1 ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
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
        [ArgumentsLength(1)]
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
        [ArgumentsLength(1)]
        public static JSValue localeCompare(JSValue self, Arguments args)
        {
            string str0 = self.ToString();
            string str1 = args[0].ToString();
            return string.CompareOrdinal(str0, str1);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue match(JSValue self, Arguments args)
        {
            if (self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));

            var a0 = args[0];
            var regex = a0.Value as RegExp;
            if (regex != null)
            {
                if (!regex._global)
                {
                    regex.lastIndex._valueType = JSValueType.Integer;
                    regex.lastIndex._iValue = 0;
                    return regex.exec(self);
                }
                else
                {
                    var match = regex._Regex.Match(self.ToString());
                    int index = 0;

                    // Result should be w/o 'index' and 'input'
                    var res = new Array();
                    while (match.Success)
                    {
                        res._data[index++] = match.Value;
                        match = match.NextMatch();
                    }

                    return res;
                }
            }
            else
            {
                var match = new Regex((a0._valueType > JSValueType.Undefined ? (object)a0 : "").ToString(), RegexOptions.ECMAScript).Match(self.ToString());

                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res._data[i] = match.Groups[i].Value;

                res.SetProperty("index", match.Index, false);
                res.SetProperty("input", self, true);
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(2)]
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
        [ArgumentsLength(1)]
        public static JSValue search(JSValue self, Arguments args)
        {
            if (self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("String.prototype.match called on null or undefined"));
            if (args.length == 0)
                return 0;
            var a0 = args[0];
            if (a0._valueType == JSValueType.Object
                && a0._oValue is RegExp)
            {
                var regex = a0._oValue as RegExp;
                if (!regex._global)
                {
                    var res = regex.exec(self);
                    if ((res ?? @null) != @null)
                        return res["index"];
                    return -1;
                }
                else
                {
                    return regex._Regex.Match(self.ToString()).Index;
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
        [ArgumentsLength(2)]
        public static JSValue split(JSValue self, Arguments args)
        {
            if (args.Length == 0 || !args[0].Defined)
                return new Array(new object[] { self.ToString() });

            var selfString = self.ToString();
            var limit = (uint)Tools.JSObjectToInt64(args[1], long.MaxValue, true);

            if (args[0]._valueType == JSValueType.Object && args[0]._oValue is RegExp)
            {
                var match = (args[0]._oValue as RegExp)._Regex.Match(selfString);
                if ((args[0]._oValue as RegExp)._Regex.ToString().Length == 0)
                {
                    match = match.NextMatch();
                    if (limit == uint.MaxValue)
                        limit = (uint)selfString.Length;
                }

                Array res = new Array();
                int index = 0;
                while (res._data.Length < limit)
                {
                    if (!match.Success)
                    {
                        res._data.Add(selfString.Substring(index, selfString.Length - index));
                        break;
                    }

                    int nindex = match.Index;
                    if (nindex == -1)
                    {
                        res._data.Add(selfString.Substring(index, selfString.Length - index));
                        break;
                    }
                    else
                    {
                        var item = selfString.Substring(index, nindex - index);
                        res._data.Add(item);
                        index = nindex + match.Length;
                    }

                    match = match.NextMatch();
                }

                return res;
            }
            else
            {
                string fstr = args[0].ToString();
                Array res = new Array();
                if (string.IsNullOrEmpty(fstr))
                {
                    for (var i = 0; i < System.Math.Min(selfString.Length, limit); i++)
                        res._data.Add(selfString[i]);
                }
                else
                {
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
                            var item = selfString.Substring(index, nindex - index);
                            res._data.Add(item);
                            index = nindex + fstr.Length;
                        }
                    }
                }

                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue startsWith(JSValue self, Arguments args)
        {
            var selfAsString = (self ?? undefinedString).ToString();
            var value = (args?[0] ?? undefinedString).ToString();

            return selfAsString.StartsWith(value) ? Boolean.True : Boolean.False;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(2)]
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
        [ArgumentsLength(2)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
        [ArgumentsLength(0)]
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
                yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumeratorMode > 0 ? str[i].ToString() : null);
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
    }
}