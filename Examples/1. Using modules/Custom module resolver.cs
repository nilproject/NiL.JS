using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;
using NiL.JS;

namespace Examples.Using_modules
{
    [Level(1)]
    public sealed class Custom_module_resolver : Example
    {
        public override void Run()
        {
            var mainModule = new Module("fakedir/superscript.js", @"
import * as Consts from ""somelib/consts""

console.log(`PI equal ${Consts.Pi}`);
console.log(`E equal ${Consts.E}`);
console.log(`Gravitational acceleration on Earth approximately ${Consts.g} m/s^2`);
");

            Module.ResolveModule += MyModuleResolver;

            mainModule.Run();
        }

        private void MyModuleResolver(Module sender, ResolveModuleEventArgs e)
        {
            if (e.ModulePath == "/math consts.js")
            {
                e.Module = new Module(e.ModulePath, @"
export const Pi = Math.PI, E = Math.E;
");
            }
            else if (e.ModulePath == "/fakedir/somelib/consts.js")
            {
                e.Module = new Module(e.ModulePath, @"
export * from ""/math consts.js"";
export const g = 9.8;
");
            }

            e.Module.Run(); // It's necessary
        }
    }
}
