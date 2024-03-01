using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Fuzz;

[TestClass]
public sealed class Bug_220
{
    private sealed class MyModuleResolver : IModuleResolver
    {
        private readonly Module _module;

        public MyModuleResolver(Module module)
        {
            _module = module;
        }

        public bool TryGetModule(ModuleRequest moduleRequest, out Module result)
        {
            if (moduleRequest.AbsolutePath == _module.FilePath)
            {
                result = _module;
                return true;
            }

            result = null;
            return false;
        }
    }

    [TestMethod]
    public void ContextOfExportedFunction()
    {
        var module0 = new Module("/a.js", @"
export function a() {
  const b = [1,2,3];
  const c = b.length;
  return { b, c };
}
");

        var module1 = new Module("/main.js", @"
import { a } from './a';
var result = a().c;
");

        module1.ModuleResolversChain.Add(new MyModuleResolver(module0));

        module1.Run();

        var result = module1.Context.GetVariable("result");

        Assert.AreEqual(JSValueType.Integer, result.ValueType);
        Assert.AreEqual(3, result.As<int>());
    }
}
