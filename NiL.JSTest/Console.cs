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
            System.Console.WriteLine(args[0]);
        }
    }
}
