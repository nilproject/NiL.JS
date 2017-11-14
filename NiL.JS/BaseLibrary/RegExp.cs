using System;
using System.Collections.Generic;
using System.Text;
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
            makeRegex(pattern, flags);
        }

        private void makeRegex(string pattern, string flags)
        {
            pattern = pattern ?? "null";
            flags = flags ?? "null";
            _global = false;
            _sticky = false;
            _unicode = false;
            try
            {
                var options = RegexOptions.ECMAScript | RegexOptions.CultureInvariant;

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
                                ExceptionHelper.Throw(new SyntaxError("Invalid RegExp flag \"" + flags[i] + '"'));
                                break;
                            }
                    }
                }
                _source = pattern;

                string label = _source + "/"
                + ((options & RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((options & RegexOptions.Multiline) != 0 ? "m" : "")
                + (_unicode ? "u" : "");

                lock (_cache)
                {
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

                    pattern = Tools.Unescape(pattern, false, false, true, _unicode);
                    if (_unicode)
                        pattern = translateToUnicodePattern(pattern);

                    _regex = new Regex(pattern, options);

                    _cacheIndex = (_cacheIndex + 1) % _cacheSize;
                    _cache[_cacheIndex].key = label;
                    _cache[_cacheIndex].re = _regex;
                }
            }
            catch (ArgumentException e)
            {
                ExceptionHelper.Throw(new SyntaxError(e.Message));
            }
        }

        private static string translateToUnicodePattern(string pattern)
        {
            if (pattern == null || pattern == "")
                return "";

            var s = new StringBuilder(pattern.Length);

            /**
             * This is what we need to change:
             * 1.   The dot. It has to be adjusted so that is now also matches all surrogate pairs.
             * 2.   Character sets have to be adjusted:
             *      a)  [ (surrogate pair) ] should match the code point and not the high or low part of the surrogate pair.
             *      b)  [^ set ] should include all Unicode characters (also > \uFFFF) except set.
             *      c)  [(surrogate pair or UTF-16 char)-(surrogate pair)] should not throw an error.
             * 3.   Character classes (\D, \S and \W) outside of sets have to be adjusted.
             * 4.   Surrogate pairs should be surrounded by a non-capturing group to act as one character
             */

            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];

                if (c == '.')
                {
                    s.Append("(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|.)");
                }
                else if (c == '[' && i + 1 < pattern.Length)
                {
                    int stop = i + 1;
                    while (i < pattern.Length && pattern[stop] != ']')
                    {
                        if (pattern[stop] == '\\')
                            stop++;
                        stop++;
                    }

                    if (stop >= pattern.Length)
                    {
                        s.Append('[');
                        continue;
                    }

                    bool inv = pattern[i + 1] == '^';
                    if (inv)
                        i++;

                    s.Append(translateCharSet(pattern.Substring(i + 1, stop - i - 1), inv));
                    i = stop;
                }
                else if (c == '\\' && i + 1 < pattern.Length)
                {
                    c = pattern[++i];
                    if (c == 'D' || c == 'S' || c == 'W')
                        s.Append("(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|\\" + c + ")");
                    else
                        s.Append('\\').Append(c);
                }
                else if (Tools.IsSurrogatePair(pattern, i))
                {
                    s.Append("(?:").Append(c).Append(pattern[++i]).Append(')');
                }
                else
                    s.Append(c);
            }

            return s.ToString();
        }

        private struct CharRange
        {
            public int start;
            public int stop; // inclusive

            public CharRange(int start, int stop)
            {
                this.start = start;
                this.stop = stop;
            }

            public static int MaxValue = 0x10FFFF;
            public static int MinValue = 0;
        }
        private static string translateCharSet(string set, bool inverted)
        {
            CharRange[] crs = analyzeCharSet(set); // character ranges
            if (inverted)
                crs = invertCharSet(crs);

            if (crs.Length == 0)
                return @"[]";

            if (crs.Length == 1 && crs[0].start == 0 && crs[0].stop == CharRange.MaxValue)
                return @"(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|[\s\S])";

            var sC = new List<CharRange>(crs); // single char ranges
            for (var i = sC.Count - 1; i >= 0; i--)
            {
                if (sC[i].start > 0xFFFF)
                {
                    sC.RemoveAt(i);
                    continue;
                }
                else if (sC[i].stop > 0xFFFF)
                    sC[i] = new CharRange(sC[i].start, 0xFFFF);
                break;
            }

            var mC = new List<CharRange>(crs.Length - sC.Count + 1); // multi char ranges
            for (int i = 0; i < crs.Length; i++)
            {
                if (crs[i].start > 0xFFFF)
                    mC.Add(crs[i]);
                else if (crs[i].stop > 0xFFFF)
                    mC.Add(new CharRange(0x10000, crs[i].stop));
            }


            var s = new StringBuilder("(?:");

            if (mC.Count > 0)
            {
                s.Append("(?:");

                for (int i = 0; i < mC.Count; i++)
                {
                    if (i > 0)
                        s.Append('|');

                    var c = mC[i];

                    if (c.start == c.stop)
                        s.Append(Tools.CodePointToString(c.start));
                    else
                    {
                        var start = Tools.CodePointToString(c.start);
                        var stop = Tools.CodePointToString(c.stop);

                        // It will print each character range independent from each other.
                        // (This might not be the most efficient way)

                        // This assumes c.start <= c.stop
                        // (which will be true if CharRange is used correctly)

                        if (start[0] == stop[0])
                            s.Append(start[0]).Append('[').Append(start[1]).Append('-').Append(stop[1]).Append(']');
                        else
                        {
                            int s1 = (start[1] > '\uDC00') ? 1 : 0;
                            int s2 = (stop[1] < '\uDFFF') ? 1 : 0;

                            if (s1 != 0)
                                s.Append(start[0]).Append('[').Append(start[1]).Append("-\uDFFF]|");
                            if (stop[0] - start[0] >= s1 + s2)
                            {
                                s.Append('[');
                                s.Append((char)(start[0] + s1));
                                s.Append('-');
                                s.Append((char)(stop[0] - s2));
                                s.Append(']');
                                s.Append("[\uDC00-\uDFFF]|");
                            }
                            if (s2 != 0)
                                s.Append(stop[0]).Append("[\uDC00-").Append(stop[1]).Append(']');
                        }
                    }
                }

                s.Append(")");
            }

            if (sC.Count > 0)
            {
                if (mC.Count > 0)
                    s.Append('|');

                s.Append("[");
                for (int i = 0; i < sC.Count; i++)
                {
                    var c = sC[i];
                    if (c.start == c.stop)
                        s.Append("\\u").Append(c.start.ToString("X4"));
                    else
                    {
                        s.Append("\\u").Append(c.start.ToString("X4"));
                        if (c.stop > c.start + 1)
                            s.Append('-');
                        s.Append("\\u").Append(c.stop.ToString("X4"));
                    }
                }
                s.Append("]");
            }


            s.Append(")");
            return s.ToString();
        }
        private static CharRange[] analyzeCharSet(string set)
        {
            var r = new List<CharRange>();

            char c;
            int cI, dI;
            for (int i = 0; i < set.Length; i++)
            {
                c = set[i];
                cI = Tools.NextCodePoint(set, ref i);

                if (c == '\\' && i + 1 < set.Length)
                {
                    c = set[++i];

                    if (c == 'd')
                    {
                        r.Add(new CharRange(48, 57));
                        continue;
                    }
                    if (c == 'D')
                    {
                        r.Add(new CharRange(0, 47));
                        r.Add(new CharRange(58, CharRange.MaxValue));
                        continue;
                    }
                    else if (c == 's')
                    {
                        r.Add(new CharRange(9, 10));
                        r.Add(new CharRange(13, 13));
                        r.Add(new CharRange(32, 32));
                        continue;
                    }
                    else if (c == 'S')
                    {
                        r.Add(new CharRange(0, 8));
                        r.Add(new CharRange(11, 12));
                        r.Add(new CharRange(14, 31));
                        r.Add(new CharRange(33, CharRange.MaxValue));
                        continue;
                    }
                    else if (c == 'w')
                    {
                        r.Add(new CharRange(48, 57));
                        r.Add(new CharRange(65, 90));
                        r.Add(new CharRange(95, 95));
                        r.Add(new CharRange(97, 122));
                        continue;
                    }
                    else if (c == 'W')
                    {
                        r.Add(new CharRange(0, 47));
                        r.Add(new CharRange(58, 64));
                        r.Add(new CharRange(91, 94));
                        r.Add(new CharRange(96, 96));
                        r.Add(new CharRange(123, CharRange.MaxValue));
                        continue;
                    }

                    i--;
                }

                if (i + 2 < set.Length && set[i + 1] == '-') // -[char]
                {
                    i += 2;
                    dI = Tools.NextCodePoint(set, ref i, true);

                    if (dI < cI)
                        ExceptionHelper.Throw(new SyntaxError("Range out of order in character class"));

                    r.Add(new CharRange(cI, dI));
                    continue;
                }

                r.Add(new CharRange(cI, cI));
            }


            if (r.Count <= 1)
                return r.ToArray();

            // sort

            r.Sort(new Comparison<CharRange>(new Func<CharRange, CharRange, int>((x, y) => x.start - y.start)));

            // optimize

            var rNew = new List<CharRange>();

            CharRange cr = r[0];
            for (int i = 1; i < r.Count; i++)
            {
                if (r[i].stop <= cr.stop)
                    continue;
                if (cr.stop >= r[i].start - 1)
                {
                    cr.stop = r[i].stop;
                    continue;
                }

                rNew.Add(cr);
                cr = r[i];
            }
            rNew.Add(cr);

            return rNew.ToArray();
        }
        private static CharRange[] invertCharSet(CharRange[] set)
        {
            if (set.Length == 0)
                return new CharRange[] { new CharRange(0, CharRange.MaxValue) };

            var r = new List<CharRange>();

            if (set[0].start > 0)
                r.Add(new CharRange(0, set[0].start - 1));

            for (int i = 1; i < set.Length; i++)
                r.Add(new CharRange(set[i - 1].stop + 1, set[i].start - 1));

            if (set[set.Length - 1].stop < CharRange.MaxValue)
                r.Add(new CharRange(set[set.Length - 1].stop + 1, CharRange.MaxValue));

            return r.ToArray();
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
