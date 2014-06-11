using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class RegExp : EmbeddedType
    {
        private JSObject lIndex = 0;
        internal Regex regEx;

        [DoNotEnumerate]
        public RegExp()
        {
            global = false;
            regEx = new System.Text.RegularExpressions.Regex("");
        }

        private void makeRegex(JSObject args)
        {
            var ptrn = args.GetMember("0");
            if (ptrn.valueType == JSObjectType.Object && ptrn.oValue is RegExp)
            {
                if (args.GetMember("length").iValue > 1 && args.GetMember("1").valueType > JSObjectType.Undefined)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Cannot supply flags when constructing one RegExp from another")));
                oValue = ptrn.oValue;
                return;
            }
            global = false;
            var pattern = ptrn.valueType > JSObjectType.Undefined ? ptrn.ToString().Replace("\\P", "P") : "";
            var flags = args.GetMember("length").iValue > 1 && args.GetMember("1").valueType > JSObjectType.Undefined ? args.GetMember("1").ToString() : "";
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
                                if (global)
                                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                global = true;
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
        public RegExp(JSObject args)
        {
            makeRegex(args);
        }

        [DoNotEnumerate]
        public RegExp(string pattern, string flags)
        {
            global = false;
            try
            {
                System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.ECMAScript;
                for (int i = 0; i < flags.Length; i++)
                {
                    switch (flags[i])
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
                                if (global)
                                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to double use RegExp flag \"" + flags[i] + '"')));
                                global = true;
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
        public Boolean ignoreCase
        {
            get
            {
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0;
            }
        }

        [DoNotEnumerate]
        public Boolean multiline
        {
            get
            {
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0;
            }
        }

        [DoNotEnumerate]
        public bool global { get; private set; }

        [DoNotEnumerate]
        public String source
        {
            get
            {
                return regEx.ToString();
            }
        }

        [DoNotEnumerate]
        public JSObject lastIndex
        {
            get
            {
                return lIndex;
            }
            set
            {
                lIndex = value.GetMember("0");
            }
        }
        
        [DoNotEnumerate]
        public JSObject compile(JSObject args)
        {
            makeRegex(args);
            return this;
        }

        [DoNotEnumerate]
        public JSObject exec(JSObject args)
        {
            if (this.GetType() != typeof(RegExp))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call RegExp.exec on not RegExp object.")));
            string input = args.GetMember("0").ToString();
            lIndex = Tools.JSObjectToNumber(lIndex);
            if (lIndex.valueType == JSObjectType.Double)
            {
                lIndex.valueType = JSObjectType.Int;
                lIndex.iValue = (int)lIndex.dValue;
            }
            if ((lIndex.iValue >= input.Length || lIndex.iValue < 0) && (input.Length > 0))
            {
                lIndex.iValue = 0;
                return JSObject.Null;
            }
            var m = regEx.Match(input, lIndex.iValue);
            if (!m.Success)
            {
                lIndex.iValue = 0;
                return JSObject.Null;
            }
            var res = new Array(m.Groups.Count);
            for (int i = 0; i < m.Groups.Count; i++)
                res[i] = m.Groups[i].Success ? (JSObject)m.Groups[i].Value : null;
            if (global)
                lastIndex.iValue = m.Index + m.Length;
            res.GetMember("index", true, true).Assign(m.Index);
            res.GetMember("input", true, true).Assign(input);
            return res;
        }

        [DoNotEnumerate]
        public bool test(JSObject args)
        {
            string input = args.GetMember("0").ToString();
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
            if (global)
                lastIndex.iValue = m.Index + m.Length;
            return m.Success;
        }

        [Hidden]
        public override string ToString()
        {
            return "/" + regEx.ToString() + "/"
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? "m" : "")
                + (global ? "g" : "");
        }
    }
}
