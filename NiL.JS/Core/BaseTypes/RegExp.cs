using System;
using System.Text.RegularExpressions;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class RegExp : CustomType
    {
        private JSObject lIndex;
        internal Regex regEx;

        [DoNotEnumerate]
        public RegExp()
        {
            _global = false;
            regEx = new System.Text.RegularExpressions.Regex("");
            attributes |= JSObjectAttributesInternal.ReadOnly;
        }

        private void makeRegex(Arguments args)
        {
            var ptrn = args[0];
            if (ptrn.valueType == JSObjectType.Object && ptrn.oValue is RegExp)
            {
                if (args.GetMember("length").iValue > 1 && args[1].valueType > JSObjectType.Undefined)
                    throw new JSException(new TypeError("Cannot supply flags when constructing one RegExp from another"));
                oValue = ptrn.oValue;
                regEx = (oValue as RegExp).regEx;
                _global = (oValue as RegExp).global;
                return;
            }
            var pattern = ptrn.valueType > JSObjectType.Undefined ? ptrn.ToString().Replace("\\P", "P") : "";
            var flags = args.GetMember("length").iValue > 1 && args[1].valueType > JSObjectType.Undefined ? args[1].ToString() : "";
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
                                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                                break;
                            }
                        case 'm':
                            {
                                if ((options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0)
                                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                options |= System.Text.RegularExpressions.RegexOptions.Multiline;
                                break;
                            }
                        case 'g':
                            {
                                if (_global)
                                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                _global = true;
                                break;
                            }
                        default:
                            {
                                throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid RegExp flag \"" + flags[i] + '"')));
                            }
                    }
                }
                regEx = new System.Text.RegularExpressions.Regex(pattern, options);
            }
            catch (ArgumentException e)
            {
                throw new JSException(TypeProxy.Proxy(new SyntaxError(e.Message)));
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
                return regEx.ToString();
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSObject lastIndex
        {
            get
            {
                return lIndex ?? (lIndex = 0);
            }
            set
            {
                lIndex = (value ?? undefined).CloneImpl();
            }
        }

        [DoNotEnumerate]
        public JSObject compile(Arguments args)
        {
            makeRegex(args);
            return this;
        }

        [DoNotEnumerate]
        public JSObject exec(Arguments args)
        {
            if (this.GetType() != typeof(RegExp))
                throw new JSException(new TypeError("Try to call RegExp.exec on not RegExp object."));
            string input = args[0].ToString();
            lIndex = Tools.JSObjectToNumber(lastIndex);
            if ((lIndex.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                lIndex = lIndex.CloneImpl();
            if (lIndex.valueType == JSObjectType.Double)
            {
                lIndex.valueType = JSObjectType.Int;
                lIndex.iValue = (int)lIndex.dValue;
            }
            if (lIndex.iValue < 0)
                lIndex.iValue = 0;
            if (lIndex.iValue >= input.Length && input.Length > 0)
            {
                lIndex.iValue = 0;
                return Null;
            }
            var m = regEx.Match(input, lIndex.iValue);
            if (!m.Success)
            {
                lIndex.iValue = 0;
                return Null;
            }
            var res = new Array(m.Groups.Count);
            for (int i = 0; i < m.Groups.Count; i++)
                res.data[(uint)i] = m.Groups[i].Success ? (JSObject)m.Groups[i].Value : null;
            if (_global)
                lIndex.iValue = m.Index + m.Length;
            res.DefineMember("index").Assign(m.Index);
            res.DefineMember("input").Assign(input);
            return res;
        }

        [DoNotEnumerate]
        public bool test(Arguments args)
        {
            string input = args[0].ToString();
            lIndex = Tools.JSObjectToNumber(lIndex);
            if (lIndex.valueType == JSObjectType.Double)
            {
                lIndex.valueType = JSObjectType.Int;
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

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public JSObject toString()
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            return "/" + regEx.ToString() + "/"
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? "m" : "")
                + (_global ? "g" : "");
        }
    }
}
