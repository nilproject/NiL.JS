using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    internal class ObjectConstructor : TypeProxyConstructor
    {
        public ObjectConstructor(TypeProxy proxy)
            : base(proxy)
        {

        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject argsObj)
        {
            var res = JSObject.Object(context, argsObj);
            if (res.prototype == null)
                res.prototype = (this.protorypeField ?? JSObject.GlobalPrototype).Clone() as JSObject;
            return res;
        }

        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
