using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JSTest
{
    class SubspaceProvider : NiL.JS.Core.EmbeddedType
    {
        public string Namespace { get; private set; }

        public SubspaceProvider(string @namespace)
        {
            Namespace = @namespace;
        }

        public override JS.Core.JSObject GetField(string name, bool fast, bool own)
        {
            string reqname = Namespace + "." + name;
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            bool createSubNode = false;
            for (var i = 0; i < assms.Length; i++)
            {
                var types = assms[i].GetTypes();
                for (var j = 0; j < types.Length; j++)
                {
                    if (types[j].FullName == reqname)
                        return NiL.JS.Core.TypeProxy.GetConstructor(types[j]);
                    if (!createSubNode && types[j].Namespace != null && types[j].Namespace.Length >= reqname.Length && types[j].Namespace.IndexOf(reqname) == 0)
                        createSubNode = true;
                }
            }
            if (createSubNode)
                return NiL.JS.Core.TypeProxy.Proxy(new SubspaceProvider(reqname));
            return NiL.JS.Core.JSObject.undefined;
        }
    }
}
