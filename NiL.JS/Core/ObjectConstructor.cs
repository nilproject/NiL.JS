using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    [Serializable]
    internal class ObjectConstructor : TypeProxyConstructor
    {
        public ObjectConstructor(TypeProxy proxy)
            : base(proxy)
        {

        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject argsObj)
        {
            var res = JSObject.Object(context, argsObj);
            return res;
        }

        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
