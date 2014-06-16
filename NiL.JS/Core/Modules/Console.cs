
namespace NiL.JS.Core.Modules
{
    internal static class console
    {
        public static JSObject log(JSObject args)
        {
            var r = args.GetMember("0").ToString();
            System.Console.WriteLine(r);
            return JSObject.undefined;
        }
    }
}
