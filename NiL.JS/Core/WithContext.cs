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
            if (obj.valueType == JSObjectType.NotExists)
                throw new JSException((new ReferenceError("Variable not defined.")));
            if (obj.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("Can't access to property value of \"undefined\"."));
            if (obj.valueType >= JSObjectType.Object && obj.oValue == null)
                throw new JSException(new TypeError("Can't access to property value of \"null\"."));
            @object = obj.oValue as JSObject ?? obj;
        }

        public override JSObject DefineVariable(string name)
        {
            return parent.DefineVariable(name);
        }

        internal protected override JSObject GetVariable(string name, bool create)
        {
            thisBind = parent.thisBind;
            var res = @object.GetMember(name, create, false);
            if (res.valueType < JSObjectType.Undefined)
            {
                res = parent.GetVariable(name, create);
                objectSource = parent.objectSource;
            }
            else
                objectSource = @object;
            return res;
        }
    }
}
