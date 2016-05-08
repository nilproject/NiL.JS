using System;
using NiL.JS.Core;

namespace Examples.Methods_and_Events
{
    public sealed class Pass_delegate_into_JavaScript_environment : ExamplesFramework.Example
    {
        public override void Run()
        {
            var @delegate = new Action<string>(text => System.Windows.Forms.MessageBox.Show(text));
            var context = new Context();

            context.DefineVariable("alert").Assign(JSValue.Marshal(@delegate));
            context.Eval("alert('Hello, World!')"); // Message box: Hello, World!
        }
    }
}
