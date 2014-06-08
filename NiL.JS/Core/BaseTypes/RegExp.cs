using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class RegExp : EmbeddedType
    {
        private JSObject lIndex = 0;
        internal System.Text.RegularExpressions.Regex regEx;

        public RegExp()
        {
            global = false;
            regEx = new System.Text.RegularExpressions.Regex("");
        }

        public RegExp(JSObject x)
        {
            var ptrn = x.GetField("0", true, false);
            if (ptrn.valueType == JSObjectType.Object && ptrn.oValue is RegExp)
            {
                if (x.GetField("length", true, false).iValue > 1 && x.GetField("1", true, false).valueType > JSObjectType.Undefined)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Cannot supply flags when constructing one RegExp from another")));
                oValue = ptrn.oValue;
                return;
            }
            global = false;
            var pattern = ptrn.valueType > JSObjectType.Undefined ? ptrn.ToString().Replace("\\P", "P") : "";
            var flags = x.GetField("length", false, true).iValue > 1 && x.GetField("1", true, false).valueType > JSObjectType.Undefined ? x.GetField("1", true, false).ToString() : "";
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
                        c = Tools.Unescape(flags.Substring(i, len + 1))[0];
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

        public Boolean ignoreCase
        {
            get
            {
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0;
            }
        }

        public Boolean multiline
        {
            get
            {
                return (regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0;
            }
        }

        public bool global { get; private set; }

        public String source
        {
            get
            {
                return regEx.ToString();
            }
        }

        public JSObject lastIndex
        {
            get
            {
                return lIndex;
            }
            set
            {
                lIndex = value.GetField("0", true, false);
            }
        }

        public JSObject exec(JSObject args)
        {
            if (this.GetType() != typeof(RegExp))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call RegExp.exec on not RegExp object.")));
            string input = args.GetField("0", true, false).ToString();
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
            res.GetField("index", false, true).Assign(m.Index);
            res.GetField("input", false, true).Assign(input);
            return res;
        }

        public bool test(JSObject args)
        {
            string input = args.GetField("0", true, false).ToString();
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

        public override string ToString()
        {
            return "/" + regEx.ToString() + "/"
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? "i" : "")
                + ((regEx.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? "m" : "")
                + (global ? "g" : "");
        }
    }
}
