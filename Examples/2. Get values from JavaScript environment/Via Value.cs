using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples._2_Get_values_from_JavaScript_environment
{
    public sealed class Via_Value : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.DefineVariable("x").Assign(123);
            context.Eval("var result = x * 2");

            object result = context.GetVariable("result").Value; // using NiL.JS.Extensions;

            Console.WriteLine("result: " + result); // Console: result: 246

            Console.WriteLine("Type of result: " + result.GetType()); // Console: Type of result: System.Int32
        }
    }
}
