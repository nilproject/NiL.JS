using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS
{
    public delegate void ResolveModuleHandler(Module sender, ResolveModuleEventArgs e);

    public class ResolveModuleEventArgs : EventArgs
    {
        public string ModulePath { get; private set; }
        public Module Module { get; set; }
        public bool AddToCache { get; set; }

        public ResolveModuleEventArgs(string moduleName)
        {
            ModulePath = moduleName;
            AddToCache = true;
        }
    }
}
