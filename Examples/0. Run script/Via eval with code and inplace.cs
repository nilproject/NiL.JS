using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Examples._0_Run_script
{
    public sealed class Via_eval_with_code_and_inplace : ExamplesFramework.Example
    {
        public override void Run()
        {
            /*
                Parameter inplace of Context.Eval(...) switches mode of variable definition.
                Default value of this parameter is False. But if Eval(...) called with inplce equals True,
                all variables declared inside passed code will be defined 
                in the context for which the method is called (except Strict Mode).
                Also, all defined variables will be undeletable.
            */

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
