using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS
{
    public delegate void ResolveModuleHandler(ResolveModuleEventArgs e);

    public class ResolveModuleEventArgs : EventArgs
    {
        public string ModuleName { get; protected set; }
        public Module Module { get; set; }
        public bool AddToCache { get; set; }

        public ResolveModuleEventArgs(string moduleName)
        {
            ModuleName = moduleName;
            AddToCache = true;
        }
    }
}
