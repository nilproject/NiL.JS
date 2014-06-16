using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class WithContext : Context
    {
        private JSObject @object;

        public WithContext(JSObject obj, Context prototype)
            : base(prototype, false, prototype.caller)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj.valueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new ReferenceError("Variable not defined.")));
            if (obj.valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
            if (obj.valueType >= JSObjectType.Object && obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\".")));
            @object = obj.Clone() as JSObject;
        }

        public override JSObject DefineVariable(string name)
        {
            return prototype.DefineVariable(name);
        }

        internal protected override JSObject GetVariable(string name, bool create)
        {
            thisBind = prototype.thisBind;
            var res = @object.GetMember(name);
            if (res.valueType == JSObjectType.NotExistInObject)
                return prototype.GetVariable(name, create);
            else
            {
                objectSource = @object;
                return res;
            }
        }
    }
}
