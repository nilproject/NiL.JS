using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    internal class WithContext : Context
    {
        private JSObject @object;

        public WithContext(JSObject obj, Context prototype)
            : base(prototype)
        {
            if (obj.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new ReferenceError("Varible not defined.")));
            if (obj.ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
            if (obj.ValueType >= JSObjectType.Object && obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\".")));
            @object = obj.Clone() as JSObject;
        }

        public override JSObject GetOwnField(string name)
        {
            return prototype.GetOwnField(name);
        }

        public override JSObject GetField(string name)
        {
            thisBind = prototype.thisBind;
            var res = @object.GetField(name, true, false);
            if (res.ValueType < JSObjectType.Undefined || res == JSObject.undefined)
                return prototype.GetField(name);
            else
            {
                thisBind = @object;
                return res;
            }
        }
    }
}
