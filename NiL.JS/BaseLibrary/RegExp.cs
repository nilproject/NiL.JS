using System;
using System.Text.RegularExpressions;
using NiL.JS.Core;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class RegExp : CustomType
    {
        private string _source;
        private JSValue lIndex;
        internal Regex regEx;

        [DoNotEnumerate]
        public RegExp()
        {
            _source = "";
            _global = false;
            regEx = new System.Text.RegularExpressions.Regex("");
        }

        private void makeRegex(Arguments args)
        {
            var ptrn = args[0];
            if (ptrn.valueType == JSValueType.Object && ptrn.Value is RegExp)
            {
                if (args.GetMember("length").iValue > 1 && args[1].valueType > JSValueType.Undefined)
                    throw new JSException(new TypeError("Cannot supply flags when constructing one RegExp from another"));
                oValue = ptrn.oValue;
                regEx = (ptrn.Value as RegExp).regEx;
                _global = (ptrn.Value as RegExp).global;
                _source = (ptrn.Value as RegExp)._source;
                return;
            }
            var pattern = ptrn.valueType > JSValueType.Undefined ? ptrn.ToString() : "";
            var flags = args.GetMember("length").iValue > 1 && args[1].valueType > JSValueType.Undefined ? args[1].ToString() : "";
            makeRegex(pattern, flags);
        }

        private void makeRegex(string pattern, string flags)
        {
            _global = false;
            try
            {
                System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.ECMAScript;
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
                                if ((options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0)
                                    throw new JSException((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                                break;
                            }
                        case 'm':
                            {
                                if ((options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0)
                                    throw new JSException((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= System.Text.RegularExpressions.RegexOptions.Multiline;
                                break;
                            }
                        case 'g':
                            {
                                if (_global)
                                    throw new JSException((new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                _global = true;
                                break;
                            }
                        default:
                            {
                                throw new JSException((new SyntaxError("Invalid RegExp flag \"" + flags[i] + '"')));
                            }
                    }
                }
                _source = pattern;
                regEx = new System.Text.RegularExpressions.Regex(Tools.Unescape(pattern, false, false, true), options);
            }
            catch (ArgumentException e)
            {
                throw new JSException((new SyntaxError(e.Message)));
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
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0;
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
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0;
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
            get { return _global; }
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
                lIndex = (value ?? JSValue.undefined).CloneImpl();
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
            if (this.GetType() != typeof(RegExp))
                throw new JSException(new TypeError("Try to call RegExp.exec for not RegExp object."));
            string input = (arg ?? "undefined").ToString();
            lIndex = Tools.JSObjectToNumber(lastIndex);
            if ((lIndex.attributes & JSValueAttributesInternal.SystemObject) != 0)
                lIndex = lIndex.CloneImpl();
            if (lIndex.valueType == JSValueType.Double)
            {
                lIndex.valueType = JSValueType.Int;
                lIndex.iValue = (int)lIndex.dValue;
            }
            if (lIndex.iValue < 0)
                lIndex.iValue = 0;
            if (lIndex.iValue >= input.Length && input.Length > 0)
            {
                lIndex.iValue = 0;
                return JSValue.Null;
            }
            var m = regEx.Match(input, lIndex.iValue);
            if (!m.Success)
            {
                lIndex.iValue = 0;
                return JSValue.Null;
            }
            var res = new Array(m.Groups.Count);
            for (int i = 0; i < m.Groups.Count; i++)
                res.data[i] = m.Groups[i].Success ? (JSValue)m.Groups[i].Value : null;
            if (_global)
                lIndex.iValue = m.Index + m.Length;
            res.DefineMember("index").Assign(m.Index);
            res.DefineMember("input").Assign(input);
            return res;
        }

        [DoNotEnumerate]
        public JSValue test(JSValue arg)
        {
            string input = (arg ?? "undefined").ToString();
            lIndex = Tools.JSObjectToNumber(lIndex);
            if (lIndex.valueType == JSValueType.Double)
            {
                lIndex.valueType = JSValueType.Int;
                lIndex.iValue = (int)lIndex.dValue;
            }
            if (lIndex.iValue >= input.Length || lIndex.iValue < 0)
            {
                lIndex.iValue = 0;
                return false;
            }
            var m = regEx.Match(input, lIndex.iValue);
            if (!m.Success)
            {
                lIndex.iValue = 0;
                return false;
            }
            if (_global)
                lastIndex.iValue = m.Index + m.Length;
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
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? "m" : "")
                + (_global ? "g" : "");
        }
    }
}
