using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NiL.JS
{
    public interface IModuleResolver
    {
        bool TryGetModule(ModuleRequest moduleRequest, out Module result);
    }
}
