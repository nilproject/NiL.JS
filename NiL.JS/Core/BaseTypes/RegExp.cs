using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    internal class RegExp
    {
        private System.Text.RegularExpressions.Regex regEx;

        public RegExp()
        {

        }

        public RegExp(JSObject x)
        {
            var pattern = x.GetField("0", true, false).ToString();
            var flags = x.GetField("length", false, true).iValue > 1 ? x.GetField("1", true, false).Value.ToString() : "";
            try
            {
                regEx = new System.Text.RegularExpressions.Regex(pattern,
                    System.Text.RegularExpressions.RegexOptions.ECMAScript
                    | (flags.IndexOf('i') != -1 ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : 0)
                    | (flags.IndexOf('m') != -1 ? System.Text.RegularExpressions.RegexOptions.Multiline : 0)
                    );
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

        public String source
        {
            get
            {
                return regEx.ToString();
            }
        }

        public JSObject exec(JSObject args)
        {
            if (args.GetField("length", false, true).iValue == 0)
                return new JSObject() { ValueType = JSObjectType.Object };
            var m = regEx.Match(args.GetField("0", true, false).Value.ToString());
            var res = new Array(m.Groups.Count);
            for (int i = 0; i < m.Groups.Count; i++)
                res[i] = m.Groups[i].Value;
            res.GetField("index", false, true).Assign(m.Groups[1].Index);
            res.GetField("input", false, true).Assign(m.Groups[0].Value);
            return res;
        }

        public bool test(JSObject args)
        {
            if (args.GetField("length", false, true).iValue == 0)
                return regEx.ToString().Length == 0;
            var m = regEx.Match(args.GetField("0", true, false).Value.ToString());
            return m.Success;
        }
    }
}
