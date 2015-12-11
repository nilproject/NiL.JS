using System;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Examples._0_Run_script
{
    public sealed class Canonical_way : ExamplesFramework.Example
    {
        private static readonly string _code = "console.log('Hello, World!');";

        public override void Run()
        {
            Module module = null;

            try
            {
                module = new Module(_code);
            }
            catch(JSException e)
            {
                var syntaxError = e.Error.Value as SyntaxError;
                if (syntaxError != null)
                {
                    Console.WriteLine(syntaxError.ToString());
                }
                else
                {
                    Console.WriteLine("Unknown error: " + e);
                }

                return;
            }

            module.Run(); // Console: Hello, World!
        }
    }
}
