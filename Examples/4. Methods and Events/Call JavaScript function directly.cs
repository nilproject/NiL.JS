using System;
using ExamplesFramework;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples.Methods_and_Events
{
    [Level(4)]
    public sealed class Call_JavaScript_function_directly : Example
    {
        public override void Run()
        {
            var context = new Context();

            context.Eval("var concat = (a, b) => a + ', ' + b");

            var concatFunction = context.GetVariable("concat").As<Function>();

            Console.WriteLine(concatFunction.Call(new Arguments { "Hello", "World!" })); // Console: 'Hello, World!'
        }
    }
}
