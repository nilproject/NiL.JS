using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.BaseLibrary
{
    internal static class console
    {
        public static JSValue log(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
            {
                if (i > 0)
                    System.Console.Write(' ');
                var r = args[i].ToString();
                System.Console.Write(r);
            }
            System.Console.WriteLine();
            return JSValue.undefined;
        }

        public static JSValue assert(Arguments args)
        {
            if (!(bool)args[0])
            {
                for (var i = 1; i < args.length; i++)
                {
                    if (i > 1)
                        System.Console.Error.Write(" ");
                    var r = args[i].ToString();
                    System.Console.Error.Write(r);
                }
                System.Console.Error.WriteLine();
            }
            return null;
        }

        public static JSValue error(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
            {
                if (i > 0)
                    System.Console.Error.Write(" ");
                var r = args[i].ToString();
                System.Console.Error.Write(r);
            }
            System.Console.Error.WriteLine();
            return null;
        }
    }
}
