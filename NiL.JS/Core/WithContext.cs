using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    internal class WithContext : Context
    {
        private JSObject obj;

        public WithContext(JSObject obj, Context prototype)
            : base(prototype)
        {
            if (obj.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new ReferenceError("Varible not defined.")));
            if (obj.ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
            if (obj.ValueType >= JSObjectType.Object && obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\".")));
            this.obj = obj;
            this.fields = obj.fields;
        }

        internal override JSObject Define(string name)
        {
            return prototype.Define(name);
        }
    }
}
