using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core.Modules
{
    internal static class console
    {
        public static JSObject log(JSObject args)
        {
            var r = args.GetField("0", true, false).ToString();
            System.Console.WriteLine(r);
            return JSObject.undefined;
        }
    }
}
