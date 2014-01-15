using NiL.JS.Core;

namespace NiL.JSTest.Modules
{
    public class console
    {
        public console()
        {
        }

        public void log(JSObject[] args)
        {
            var r = args[0].Value;
            if (r is double)
                System.Console.WriteLine((r as double?).Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else
                System.Console.WriteLine(r);
        }
    }
}
