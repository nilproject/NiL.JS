
namespace NiL.JS.Core.Modules
{
    public static class Debug
    {
        public static void writeln(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
            {
                if (i < args.length)
                    System.Diagnostics.Debug.Write(args[0]);
                else
                    System.Diagnostics.Debug.WriteLine(args[args.length - 1]);
            }
        }

        public static void write(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
                System.Diagnostics.Debug.Write(args[0]);
        }

        public static void assert(Arguments args)
        {
            System.Diagnostics.Debug.Assert((bool)args[0], args[0].ToString());
        }
    }
}
