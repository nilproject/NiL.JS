using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples._3_Methods_and_Events
{
    class Create_delegate_for_JavaScript_function : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.Eval("var sum = (a, b) => a + b");
            var sum = context.GetVariable("sum").As<Func<int, int, int>>();
            Console.WriteLine(sum(1, 2));
        }
    }
}
