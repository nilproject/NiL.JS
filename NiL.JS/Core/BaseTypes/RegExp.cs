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
            regEx = new System.Text.RegularExpressions.Regex(pattern,
                System.Text.RegularExpressions.RegexOptions.ECMAScript
                | (flags.IndexOf('i') != -1 ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : 0)
                | (flags.IndexOf('m') != -1 ? System.Text.RegularExpressions.RegexOptions.Multiline : 0)
                );
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
            var mres = new JSObject();
            mres.ValueType = JSObjectType.Object;
            if (m.Groups.Count != 1)
            {
                mres.oValue = new string[] { m.Groups[1].Value };
                mres.GetField("index", false, true).Assign(m.Groups[1].Index);
                mres.GetField("input", false, true).Assign(m.Groups[0].Value);
            }
            return mres;
        }
    }
}
