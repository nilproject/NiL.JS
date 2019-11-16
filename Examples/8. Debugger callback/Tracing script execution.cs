using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;
using NiL.JS;

namespace Examples._8.Debugger_callback
{
    [Level(8)]
    class Tracing_script_execution : Example
    {
        private readonly string _code = @"
var a = 1;
var b = 2;
var c = a + b;
console.log(c);
";

        public override void Run()
        {
            var module = new Module(_code);

            module.Context.DebuggerCallback += Context_DebuggerCallback;
            module.Context.Debugging = true;

            module.Run();
        }

        private void Context_DebuggerCallback(NiL.JS.Core.Context sender, NiL.JS.Core.DebuggerCallbackEventArgs e)
        {
            Console.Clear();
            for (var i = 0; i < _code.Length; i++)
            {
                if (i >= e.Statement.Position && i <= e.Statement.EndPosition)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                Console.Write(_code[i]);
            }

            Console.Write($"a = {sender.GetVariable("a")}; b = {sender.GetVariable("b")}; c = {sender.GetVariable("c")}");

            while (Console.ReadKey().Key != ConsoleKey.Spacebar) ;
        }
    }
}
