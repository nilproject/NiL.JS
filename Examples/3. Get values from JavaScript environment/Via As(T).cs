using System;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples.Get_values_from_JavaScript_environment
{
    public sealed class Via_As_T_ : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.DefineVariable("x").Assign(123);
            context.Eval("var result = x * 2");

            int result = context.GetVariable("result").As<int>(); // using NiL.JS.Extensions;
            
            Console.WriteLine("result: " + result); // Console: result: 246

            Console.WriteLine("Type of result: " + result.GetType()); // Console: Type of result: System.Int32
        }
    }
}
