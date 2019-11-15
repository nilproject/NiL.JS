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

console.log(`PI equals ${Consts.Pi}`);
console.log(`E equals ${Consts.E}`);
console.log(`Gravitational acceleration on Earth approximately ${Consts.g} m/s^2`);
");

            mainModule.ModuleResolversChain.Add(new MyTestModuleResolver());

            mainModule.Run();
        }

        public sealed class MyTestModuleResolver : CachedModuleResolverBase
        {
            public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                if (moduleRequest.AbsolutePath == "/math consts.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
export const Pi = Math.PI, E = Math.E;
");
                    return true;
                }
                else if (moduleRequest.AbsolutePath == "/fakedir/somelib/consts.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
export * from ""/math consts.js"";
export const g = 9.8;
");
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
