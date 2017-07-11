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
        private struct RegExpCacheItem
        {
            public string key;
            public Regex re;
            public RegExpCacheItem(string key, Regex re)
            {
                this.key = key;
                this.re = re;
            }
        }
        private static RegExpCacheItem[] _cache = new RegExpCacheItem[_cacheSize];
        private static int _cacheIndex = -1;
        private const int _cacheSize = 16;


        private string _source;
        private JSValue _lastIndex;
        internal Regex _regex;

        [DoNotEnumerate]
        public RegExp()
        {
            _source = "";
            _global = false;
            _sticky = false;
            _unicode = false;
            _regex = new Regex("");
        }

        private void makeRegex(Arguments args)
        {
            var ptrn = args[0];
            if (ptrn._valueType == JSValueType.Object && ptrn.Value is RegExp)
            {
                if (args.GetProperty("length")._iValue > 1 && args[1]._valueType > JSValueType.Undefined)
                    ExceptionHelper.Throw(new TypeError("Cannot supply flags when constructing one RegExp from another"));
                _oValue = ptrn._oValue;
                _regex = (ptrn.Value as RegExp)._regex;
                _global = (ptrn.Value as RegExp)._global;
                _sticky = (ptrn.Value as RegExp)._sticky;
                _unicode = (ptrn.Value as RegExp)._unicode;
                _source = (ptrn.Value as RegExp)._source;
                return;
            }
            var pattern = ptrn._valueType > JSValueType.Undefined ? ptrn.ToString() : "";
            var flags = args.GetProperty("length")._iValue > 1 && args[1]._valueType > JSValueType.Undefined ? args[1].ToString() : "";
            makeRegex(pattern, flags, false);
        }

        private void makeRegex(string pattern, string flags, bool unescapeFlags)
        {
            pattern = pattern ?? "null";
            flags = flags ?? "null";
            _global = false;
            _sticky = false;
            _unicode = false;
            try
            {
                var options = RegexOptions.ECMAScript | RegexOptions.CultureInvariant;

                if (unescapeFlags)
                    flags = Tools.Unescape(flags, false, true, false, true);
                for (int i = 0; i < flags.Length; i++)
                {
                    switch (flags[i])
                    {
                        case 'i':
                            {
                                if ((options & RegexOptions.IgnoreCase) != 0)
                                    ExceptionHelper.Throw(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"'));
                                options |= RegexOptions.IgnoreCase;
                                break;
                            }
                        case 'm':
                            {
                                if ((options & RegexOptions.Multiline) != 0)
                                    ExceptionHelper.Throw(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"'));
                                options |= RegexOptions.Multiline;
                                break;
                            }
                        case 'g':
                            {
                                if (_global)
                                    ExceptionHelper.Throw(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"'));
                                _global = true;
                                break;
                            }
                        case 'u':
                            {
                                if (_unicode)
                                    ExceptionHelper.Throw(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"'));
                                _unicode = true;
                                break;
                            }
                        case 'y':
                            {
                                if (_sticky)
                                    ExceptionHelper.Throw(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"'));
                                _sticky = true;
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

                string label = _source + "/"
                + ((options & RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((options & RegexOptions.Multiline) != 0 ? "m" : "")
                + (_unicode ? "u" : "");
                if (_cacheIndex >= 0)
                {
                    int _cacheSizeMinusOne = _cacheSize - 1;
                    for (var i = _cacheSize + _cacheIndex; i > _cacheIndex; i--)
                    {
                        if (_cache[i & _cacheSizeMinusOne].key == label)
                        {
                            _regex = _cache[i & _cacheSizeMinusOne].re;
                            return;
                        }
                    }
                }

                // We will need a transpiler to support ES6 Unicode regular expressions.
                // For now, I settled with Unicode escapes like \u{41} being supported.
                _regex = new Regex(Tools.Unescape(pattern, false, false, true, _unicode), options);

                _cacheIndex = (_cacheIndex + 1) % _cacheSize;
                _cache[_cacheIndex].key = label;
                _cache[_cacheIndex].re = _regex;
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
            : this(pattern, flags, false)
        {
        }

        [DoNotEnumerate]
        [Hidden]
        public RegExp(string pattern, string flags, bool unescapeFlags)
        {
            makeRegex(pattern, flags, unescapeFlags);
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
        public Boolean ignoreCase
        {
            get
            {
                return (_regex.Options & RegexOptions.IgnoreCase) != 0;
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
                return (_regex.Options & RegexOptions.Multiline) != 0;
            }
        }

        internal bool _sticky;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public Boolean sticky
        {
            [Hidden]
            get
            {
                return _sticky;
            }
        }

        internal bool _unicode;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public Boolean unicode
        {
            [Hidden]
            get
            {
                return _unicode;
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
                return _lastIndex ?? (_lastIndex = 0);
            }
            set
            {
                _lastIndex = (value ?? JSValue.undefined).CloneImpl(false);
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

            if (!_global && !_sticky)
            {
                // non-global and/or non-sticky matching doesn't use lastIndex

                var m = _regex.Match(input);
                if (!m.Success)
                    return JSValue.@null;

                var res = new Array(m.Groups.Count);
                for (int i = 0; i < m.Groups.Count; i++)
                    res._data[i] = m.Groups[i].Success ? (JSValue)m.Groups[i].Value : null;

                res.DefineProperty("index").Assign(m.Index);
                res.DefineProperty("input").Assign(input);

                return res;
            }
            else
            {
                _lastIndex = Tools.JSObjectToNumber(_lastIndex);
                if ((_lastIndex._attributes & JSValueAttributesInternal.SystemObject) != 0)
                    _lastIndex = _lastIndex.CloneImpl(false);
                if (_lastIndex._valueType == JSValueType.Double)
                {
                    _lastIndex._valueType = JSValueType.Integer;
                    _lastIndex._iValue = (int)_lastIndex._dValue;
                }

                int li = (_lastIndex._iValue < 0) ? 0 : _lastIndex._iValue;
                _lastIndex._iValue = 0;

                if (li >= input.Length && input.Length > 0)
                    return JSValue.@null;

                var m = _regex.Match(input, li);
                if (!m.Success || (_sticky && m.Index != li))
                    return JSValue.@null;

                var res = new Array(m.Groups.Count);
                for (int i = 0; i < m.Groups.Count; i++)
                    res._data[i] = m.Groups[i].Success ? (JSValue)m.Groups[i].Value : null;

                _lastIndex._iValue = m.Index + m.Length;

                res.DefineProperty("index").Assign(m.Index);
                res.DefineProperty("input").Assign(input);

                return res;
            }
        }

        [DoNotEnumerate]
        public JSValue test(JSValue arg)
        {
            // definition: exec(arg) != null

            string input = (arg ?? "null").ToString();

            if (!_global && !_sticky)
                return _regex.IsMatch(input);


            _lastIndex = Tools.JSObjectToNumber(_lastIndex);
            if ((_lastIndex._attributes & JSValueAttributesInternal.SystemObject) != 0)
                _lastIndex = _lastIndex.CloneImpl(false);
            if (_lastIndex._valueType == JSValueType.Double)
            {
                _lastIndex._valueType = JSValueType.Integer;
                _lastIndex._iValue = (int)_lastIndex._dValue;
            }

            int li = (_lastIndex._iValue < 0) ? 0 : _lastIndex._iValue;
            _lastIndex._iValue = 0;

            if (li >= input.Length && input.Length > 0)
                return false;

            var m = _regex.Match(input, li);
            if (!m.Success || (_sticky && m.Index != li))
                return false;

            _lastIndex._iValue = m.Index + m.Length;

            return true;
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
                + (_global ? "g" : "")
                + ((_regex.Options & RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((_regex.Options & RegexOptions.Multiline) != 0 ? "m" : "")
                + (_unicode ? "u" : "")
                + (_sticky ? "y" : "");
        }
    }
}
