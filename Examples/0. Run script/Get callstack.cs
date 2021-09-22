using ExamplesFramework;
using NiL.JS;
using NiL.JS.Core;
using System;

namespace Examples.Run_script
{
    [Level(0)]
    public sealed class Get_stack : Example
    {
        private static readonly string _code = @"
try{
var text = `
test callstack
MissingThing${missingThing}
`;
}
catch(e){
console.log(e.toString());
}

try{
ClrTest();
}
catch(e){
console.log(e.toString());
}


try{
import Test from 'test';

new Test().test();
}
catch(e){
console.log(e.toString());
}
";
        public override void Run()
        {
            var mainModule = new Module("main.js", _code);
            mainModule.ModuleResolversChain.Add(new MyTestModuleResolver());
            mainModule.Context.DefineVariable("ClrTest").Assign(JSValue.Marshal(new Action(() => { throw new Exception("clr error"); })));
            mainModule.Run();
        }

        public sealed class MyTestModuleResolver : CachedModuleResolverBase
        {
            public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                if (moduleRequest.AbsolutePath == "/test.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
class Test{
    test() {
        this.test2();
    }

    test1() {
        this.test3();
    }
    test2() {
        var x = new MissingThing(1, 2, 3);
    }
}
export default Test;
");
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
