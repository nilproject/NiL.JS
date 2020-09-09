using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;
using NiL.JS;
using NiL.JS.Core;

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

function add(a,b,c){
    var d=a+b;
    return d;
}
var d=add(a,b);
console.log(c);
";

        public override void Run()
        {
            var module = new Module(_code);

            module.Context.DebuggerCallback += Context_DebuggerCallback;
            module.Context.Debugging = true;

            module.Run();
        }

        private void Context_DebuggerCallback(Context sender, DebuggerCallbackEventArgs e)
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

            Console.WriteLine();

            var debugger = new ContextDebuggerProxy(sender);
            if (debugger.CurrFunction.Name!="") {
                Console.WriteLine("Current Function:" + debugger.CurrFunction.Name);
                var ps = debugger.CurrFunction.ParamNames;
                var args = debugger.CurrFunction.RunArguments;

                Console.WriteLine("Current Function ParamNames:");
                var index = 0;
                foreach (var item in args) {
                    var name = "";
                    if (ps.Count> index) {
                        name = ps[index];
                    }
                    Console.WriteLine($"[{index}] {name}: {item.Value.Value} ({item.Value.ValueType})");
                    index++;
                }
                while (index<ps.Count) {
                   var name = ps[index];
                    Console.WriteLine($"[{index}] {name}:  (Undefined) ");
                    index++;
                }
                Console.WriteLine("Variables:");
                var variables = debugger.LocalVariables;
                foreach (var item in variables) {
                    Console.WriteLine($"{item.Name}: {item.Value}");
                }
            } else {
                Console.WriteLine("Variables:");
                Console.WriteLine(string.Join(Environment.NewLine, new ContextDebuggerProxy(sender).Variables.Select(x => x.Key + ": " + x.Value)));

            }





            Console.WriteLine();
            Console.WriteLine("Output:");

            while (Console.ReadKey().Key != ConsoleKey.Spacebar) ;
        }
    }
}
