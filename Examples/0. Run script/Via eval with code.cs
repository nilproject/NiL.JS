using System;
using ExamplesFramework;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Examples.Run_script
{
    [Level(0)]
    public sealed class Via_eval_with_code_and_inplace : ExamplesFramework.Example
    {
        public override void Run()
        {
            // Example 1
            try
            {
                Context context = new Context();
                context.Eval("var message = 'Hello, World!';", true);
                context.Eval("console.log(message);", true); // Console: Hello, World!
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

            // Example 2
            try
            {
                Context context = new Context();
                context.Eval("let message = 'Hello, World!';", true);
                context.Eval("console.log(message);", true); // Console: Hello, World!
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

            // But
            // Example 3
            try
            {
                Context context = new Context();
                context.Eval("let message = 'Hello, World!';", false);
                context.Eval("console.log(message);", false); // Error: ReferenceError
            }
            catch (JSException e)
            {
                var referenceError = e.Error.Value as ReferenceError;
                if (referenceError != null)
                {
                    Console.WriteLine(referenceError.ToString());
                }
                else
                {
                    Console.WriteLine("Unknown error: " + e);
                }
            }
        }
    }
}
