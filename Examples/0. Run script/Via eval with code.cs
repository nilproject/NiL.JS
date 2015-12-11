using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Examples._0_Run_script
{
    public sealed class Via_eval_with_code : ExamplesFramework.Example
    {
        private static readonly string _code = "console.log('Hello, World!');";

        public override void Run()
        {
            var context = new Context();

            try
            {
                context.Eval(_code); // Console: Hello, World!
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
