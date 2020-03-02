using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;

namespace FunctionalTests
{
    [TestClass]
    public sealed class ModuleTests
    {
        [TestMethod]
        public void Run()
        {
            var mainModule = new Module("superscript.js", @"
import { random } from 'utils';

if(!random || !random())
    throw new Error('Should not fail!');
");

            mainModule.ModuleResolversChain.Add(new MyTestModuleResolver());

            mainModule.Run();
        }

        public sealed class MyTestModuleResolver : CachedModuleResolverBase
        {
            public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                if (moduleRequest.AbsolutePath == "/utils.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
export * from './utils/index.js';
");
                    return true;
                }
                else if (moduleRequest.AbsolutePath == "/utils/index.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
export { random } from './random';
");
                    return true;
                }
                else if (moduleRequest.AbsolutePath == "/utils/random.js")
                {
                    result = new Module(moduleRequest.AbsolutePath, @"
export function random() {
    return Math.random();
}
");
                    return true;
                }

                result = null;
                return false;
            }
        }

    }
}
