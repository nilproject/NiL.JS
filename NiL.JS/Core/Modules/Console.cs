using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core.Modules
{
    internal sealed class console
    {
        public static JSObject log(JSObject args)
        {
            var r = args.GetField("0", true).Value;
            if (r is double)
                System.Console.WriteLine((r as double?).Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else
                System.Console.WriteLine(r);
            return JSObject.undefined;
        }
    }
}
