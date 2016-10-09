using System;
using System.Text.RegularExpressions;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class RegExp : CustomType
    {
        private string _source;
        private JSValue lIndex;
        internal Regex _Regex;

        [DoNotEnumerate]
        public RegExp()
        {
            _source = "";
            _global = false;
            _Regex = new Regex("");
        }

        private void makeRegex(Arguments args)
        {
            var ptrn = args[0];
            if (ptrn._valueType == JSValueType.Object && ptrn.Value is RegExp)
            {
                if (args.GetProperty("length")._iValue > 1 && args[1]._valueType > JSValueType.Undefined)
                    ExceptionHelper.Throw(new TypeError("Cannot supply flags when constructing one RegExp from another"));
                _oValue = ptrn._oValue;
                _Regex = (ptrn.Value as RegExp)._Regex;
                _global = (ptrn.Value as RegExp).global;
                _source = (ptrn.Value as RegExp)._source;
                return;
            }
            var pattern = ptrn._valueType > JSValueType.Undefined ? ptrn.ToString() : "";
            var flags = args.GetProperty("length")._iValue > 1 && args[1]._valueType > JSValueType.Undefined ? args[1].ToString() : "";
            makeRegex(pattern, flags);
        }

        private void makeRegex(string pattern, string flags)
        {
            pattern = pattern ?? "null";
            flags = flags ?? "~";
            _global = false;
            try
            {
                var options = RegexOptions.ECMAScript | RegexOptions.CultureInvariant;
                for (int i = 0; i < flags.Length; i++)
                {
                    char c = flags[i];
                    if (c == '\\')
                    {
                        int len = 1;
                        if (flags[i + 1] == 'u')
                            len = 5;
                        else if (flags[i + 1] == 'x')
                            len = 3;
                        c = Tools.Unescape(flags.Substring(i, len + 1), false)[0];
                        i += len;
                    }
                    switch (c)
                    {
                        case 'i':
                            {
                                if ((options & RegexOptions.IgnoreCase) != 0)
                                    ExceptionHelper.Throw((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= RegexOptions.IgnoreCase;
                                break;
                            }
                        case 'm':
                            {
                                if ((options & RegexOptions.Multiline) != 0)
                                    ExceptionHelper.Throw((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= RegexOptions.Multiline;
                                break;
                            }
                        case 'g':
                            {
                                if (_global)
                                    ExceptionHelper.Throw((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                _global = true;
                                break;
                            }
                        default:
                            {
                                ExceptionHelper.Throw((new SyntaxError("Invalid RegExp flag \"" + flags[i] + '"')));
                                break;
                            }
                    }
                }
                _source = pattern;
                _Regex = new Regex(Tools.Unescape(pattern, false, false, true), options);
            }
            catch (ArgumentException e)
            {
                ExceptionHelper.Throw((new SyntaxError(e.Message)));
            }
        }

        [DoNotEnumerate]
        public RegExp(Arguments args)
        {
            makeRegex(args);
        }

        [DoNotEnumerate]
        public RegExp(string pattern, string flags)
        {
            makeRegex(pattern, flags);
        }

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public Boolean ignoreCase
        {
            get
            {
                return (_Regex.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0;
            }
        }

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public Boolean multiline
        {
            get
            {
                return (_Regex.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0;
            }
        }

        internal bool _global;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public Boolean global
        {
            [Hidden]
            get
            {
                return _global;
            }
        }

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public String source
        {
            get
            {
                return new String(_source);
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSValue lastIndex
        {
            get
            {
                return lIndex ?? (lIndex = 0);
            }
            set
            {
                lIndex = (value ?? JSValue.undefined).CloneImpl(false);
            }
        }

        [DoNotEnumerate]
        public RegExp compile(Arguments args)
        {
            makeRegex(args);
            return this;
        }

        [DoNotEnumerate]
        public JSValue exec(JSValue arg)
        {
            string input = (arg ?? "null").ToString();
            lIndex = Tools.JSObjectToNumber(lastIndex);
            if ((lIndex._attributes & JSValueAttributesInternal.SystemObject) != 0)
                lIndex = lIndex.CloneImpl(false);
            if (lIndex._valueType == JSValueType.Double)
            {
                lIndex._valueType = JSValueType.Integer;
                lIndex._iValue = (int)lIndex._dValue;
            }
            if (lIndex._iValue < 0)
                lIndex._iValue = 0;
            if (lIndex._iValue >= input.Length && input.Length > 0)
            {
                lIndex._iValue = 0;
                return JSValue.@null;
            }
            var m = _Regex.Match(input, lIndex._iValue);
            if (!m.Success)
            {
                lIndex._iValue = 0;
                return JSValue.@null;
            }
            var res = new Array(m.Groups.Count);
            for (int i = 0; i < m.Groups.Count; i++)
                res._data[i] = m.Groups[i].Success ? (JSValue)m.Groups[i].Value : null;
            if (_global)
                lIndex._iValue = m.Index + m.Length;
            res.DefineProperty("index").Assign(m.Index);
            res.DefineProperty("input").Assign(input);
            return res;
        }

        [DoNotEnumerate]
        public JSValue test(JSValue arg)
        {
            string input = (arg ?? "null").ToString();
            lIndex = Tools.JSObjectToNumber(lIndex);
            if (lIndex._valueType == JSValueType.Double)
            {
                lIndex._valueType = JSValueType.Integer;
                lIndex._iValue = (int)lIndex._dValue;
            }
            if (lIndex._iValue >= input.Length || lIndex._iValue < 0)
            {
                lIndex._iValue = 0;
                return false;
            }
            var m = _Regex.Match(input, lIndex._iValue);
            if (!m.Success)
            {
                lIndex._iValue = 0;
                return false;
            }
            if (_global)
                lastIndex._iValue = m.Index + m.Length;
            return m.Success;
        }

#if !WRC
        [CLSCompliant(false)]
        [DoNotEnumerate]
        public JSValue toString()
        {
            return ToString();
        }
#endif

        [Hidden]
        public override string ToString()
        {
            return "/" + _source + "/"
                + ((_Regex.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((_Regex.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? "m" : "")
                + (_global ? "g" : "");
        }
    }
}
