using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace Examples._4_Namespaces_and_external_types
{
    public sealed class Using_Generic_Types : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            var temp = new
            {
                GList = JSValue.GetGenericTypeSelector(new[]
                {
                    typeof(List<>),
                    typeof(ArrayList)
                })
            };

            context.DefineVariable("test").Assign(JSValue.Marshal(temp));
            context.Eval(@"
console.log('ArrayList');
var list = test.GList()(); 
list.Add(1); 
list.Add('2');
console.log(list.get_Item(0));
console.log(list.get_Item(1));

console.log('List<Number>');
var list = test.GList(Number)(); 
list.Add(1); 
list.Add('2');
console.log(list.get_Item(0));
console.log(list.get_Item(1));");

        }
    }
}
