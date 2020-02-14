using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;
using NiL.JS.Core;

namespace Examples._7.Few_words_about_Global_Context
{
    [Level(7)]
    public sealed class What_it_is : Example
    {
        /*      Global Context
         *      ┌───────────
         *      │  <Object, Number, Math, Function etc.>
         *      │  <Global functions like parseInt(...) and isNaN(...)>
         *      │  <Global constants like undefined and NaN>
         *      │
         *      │  new Context(), new Module().Context
         *      │  ┌───────────
         *      │  │ <Global variables of the script. Smthg like "window.my_variable"> 
         *      │  │
         *      │  │ Contexts of nested functions
         *      │  │ ┌───────────
         *      │  │ │ <Parameters and local variables of a function>
         *      │  │ │
         *      │  │ │ Contexts of functions inside other functions
         *      │  │ │ ┌───────────
         *      │  │ │ │ ...
         *      │  │ │ └───────────
         *      │  │ └───────────
         *      │  └───────────
         *      └───────────
         *      
         *      Before version 2.4 was the one Global Context in all cases.
         *      Since version 2.4 you can create addition global contexts for isolation scripts from each other.
         *      
         *      New created Global Context can be activated in one or more thread, after that all new instances
         *      of Context will be linked to this Global Context. 
         *      Also you can directly link Context to new Global Context without activation. 
         *      Just specify it when creating by pass it into constructor new Context(my_global_context);
         */

        public override void Run()
        {
            var firstGlobalContext = new GlobalContext("First global context");
            var secondGlobalContext = new GlobalContext("Second global context");

            firstGlobalContext.ActivateInCurrentThread();
            var firstContext = new Context();
            firstGlobalContext.Deactivate();

            var secondContext = new Context(secondGlobalContext);

            firstContext.Eval(@"
Object.my_secret_field = 'my secret value';
console.log(Object.my_secret_field); // Output: my secret value
");

            secondContext.Eval(@"
console.log(Object.my_secret_field); // Output: undefined
");
        }
    }
}
