using System;
using ExamplesFramework;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Examples.Run_script
{
    [Level(0)]
    public sealed class Canonical_way : Example
    {
        private static readonly string _code = "console.log('Hello, World!');";

        public override void Run()
        {
            Module module = null;

            try
            {
                module = new Module(_code);
                module.Run(); // Console: Hello, World!
            }
            catch (JSException e)
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
            }
        }
    }
}
