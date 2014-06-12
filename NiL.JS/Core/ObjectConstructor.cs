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

        public override NiL.JS.Core.JSObject Invoke(JSObject thisBind, NiL.JS.Core.JSObject args)
        {
            object oVal = null;
            if (args != null && args.GetMember("length").iValue > 0)
                oVal = args.GetMember("0");
            JSObject res = null;
            if ((oVal == null) ||
                (oVal is JSObject && (((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue == null) || (oVal as JSObject).valueType <= JSObjectType.Undefined)))
                return CreateObject();
            else if ((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue != null)
                return oVal as JSObject;

            if (thisBind != null
                && thisBind.valueType == JSObjectType.Object
                && thisBind.prototype != null
                && thisBind.prototype.oValue == GlobalPrototype)
                res = thisBind;
            else
                res = CreateObject();

            res.valueType = JSObjectType.Object;
            res.oValue = oVal ?? res;
            if (oVal is JSObject)
                res.prototype = (oVal as JSObject).GetMember("__proto__", false, true).Clone() as JSObject;
            else
                res.prototype = null;
            return res;
        }

        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
