using NiL.JS.Core;

namespace NiL.JS.BaseLibrary
{
    internal static class console
    {
        public static JSObject log(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
            {
                if (i > 0)
                    System.Console.Write(" ");
                var r = args[i].ToString();
                System.Console.Write(r);
            }
            System.Console.WriteLine();
            return JSObject.undefined;
        }
    }
}
