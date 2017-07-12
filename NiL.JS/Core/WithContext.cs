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
            : base(prototype, false, prototype._owner)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj._valueType == JSValueType.NotExists)
                ExceptionHelper.Throw((new ReferenceError("Variable is not defined.")));
            if (obj._valueType <= JSValueType.Undefined)
                ExceptionHelper.Throw(new TypeError("Can't access to property value of \"undefined\"."));
            if (obj._valueType >= JSValueType.Object && obj._oValue == null)
                ExceptionHelper.Throw(new TypeError("Can't access to property value of \"null\"."));
            @object = obj._oValue as JSValue ?? obj;
        }

        public override JSValue DefineVariable(string name, bool deletable)
        {
            return _parent.DefineVariable(name);
        }

        internal protected override JSValue GetVariable(string name, bool create)
        {
            _thisBind = _parent._thisBind;
            var res = @object.GetProperty(name, create, PropertyScope.Common);
            if (res._valueType < JSValueType.Undefined)
            {
                res = _parent.GetVariable(name, create);
                _objectSource = _parent._objectSource;
            }
            else
                _objectSource = @object;
            return res;
        }
    }
}
