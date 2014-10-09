using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS;

namespace NiL.JSTest
{
    public static class StringExt
    {
        public static JSObject JSEval(this string code)
        {
            return new Script("").Context.Eval(code);
        }
    }
}
