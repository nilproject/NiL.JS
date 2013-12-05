using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Modules
{
    internal sealed class console
    {
        public console()
        {
        }

        public JSObject log(IContextStatement[] args)
        {
            var r = args[0].Invoke().Value;
            if (r is double)
                System.Console.WriteLine((r as double?).Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else
                System.Console.WriteLine(r);
            return JSObject.undefined;
        }
    }
}
