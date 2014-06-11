using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class WithContext : Context
    {
        private JSObject @object;

        public WithContext(JSObject obj, Context prototype, Statement owner)
            : base(prototype, owner)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj.valueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new ReferenceError("Varible not defined.")));
            if (obj.valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
            if (obj.valueType >= JSObjectType.Object && obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\".")));
            @object = obj.Clone() as JSObject;
        }

        public override JSObject DefineVarible(string name)
        {
            return prototype.DefineVarible(name);
        }

        public override JSObject GetVarible(string name)
        {
            thisBind = prototype.thisBind;
            var res = @object.GetMember(name);
            if (res.valueType < JSObjectType.Undefined)
                return prototype.GetVarible(name);
            else
            {
                objectSource = @object;
                return res;
            }
        }
    }
}
