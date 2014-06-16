using System;

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
                (oVal is JSObject && (((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue == null)
                                        || (oVal as JSObject).valueType <= JSObjectType.Undefined)))
                return CreateObject();
            else if ((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue != null)
                return oVal as JSObject;

            res = CreateObject();

            res.valueType = JSObjectType.Object;
            res.oValue = (oVal is JSObject && ((oVal as JSObject).attributes & JSObjectAttributes.SystemConstant) != 0) ? (oVal as JSObject).Clone() : oVal;
            if (oVal is JSObject)
                res.prototype = (oVal as JSObject).GetMember("__proto__", false, true).Clone() as JSObject;
            return res;
        }

        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
