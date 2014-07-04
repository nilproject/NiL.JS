
namespace NiL.JS.Core.Modules
{
    internal static class console
    {
        public static JSObject log(Arguments args)
        {
            var r = args[0].ToString();
            System.Console.WriteLine(r);
            return JSObject.undefined;
        }
    }
}
