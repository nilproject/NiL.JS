using System;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class WithContext : Context
    {
        private JSValue @object;

        public WithContext(JSValue obj, Context prototype)
            : base(prototype, false, prototype.owner)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj._valueType == JSValueType.NotExists)
                ExceptionsHelper.Throw((new ReferenceError("Variable is not defined.")));
            if (obj._valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("Can't access to property value of \"undefined\"."));
            if (obj._valueType >= JSValueType.Object && obj._oValue == null)
                ExceptionsHelper.Throw(new TypeError("Can't access to property value of \"null\"."));
            @object = obj._oValue as JSValue ?? obj;
        }

        public override JSValue DefineVariable(string name, bool deletable)
        {
            return parent.DefineVariable(name);
        }

        internal protected override JSValue GetVariable(string name, bool create)
        {
            thisBind = parent.thisBind;
            var res = @object.GetProperty(name, create, PropertyScope.Сommon);
            if (res._valueType < JSValueType.Undefined)
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
