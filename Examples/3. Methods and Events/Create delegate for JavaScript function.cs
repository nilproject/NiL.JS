using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples._3_Methods_and_Events
{
    class Create_delegate_for_JavaScript_function : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.Eval("var concat = (a, b) => a + ', ' + b");
            
            var concatFunction = context.GetVariable("concat").As<Function>();
            var concat = (Func<string, string, string>)concatFunction.MakeDelegate(typeof(Func<string, string, string>));
            
            Console.WriteLine(concat("Hello", "World!")); // Console: 'Hello, World!'
        }
    }
}
