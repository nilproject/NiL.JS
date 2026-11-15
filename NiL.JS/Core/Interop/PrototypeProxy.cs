using System;

namespace NiL.JS.Core.Interop;

[Prototype(typeof(JSObject), true)]
internal sealed class PrototypeProxy : Proxy
{
    internal JSValue _constructor;

    internal override bool IsInstancePrototype
    {
        get
        {
            return true;
        }
    }

    public PrototypeProxy(GlobalContext context, Type type, bool indexersSupport)
        : base(context, type, indexersSupport)
    {
    }

    protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
    {
        if (key._valueType is JSValueType.String && key._oValue is "constructor")
        {
            if (forWrite && _constructor.NeedClone)
            {
                _constructor = _constructor.CloneImpl(true);
            }

            return _constructor;
        }

        return base.GetProperty(key, forWrite, memberScope);
    }

    protected internal override bool DeleteProperty(JSValue key)
    {
        if (key._valueType is JSValueType.String && key._oValue is "constructor")
        {
            if (_constructor.Exists && (_constructor._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                _constructor = NotExists;
                return true;
            }
        }

        return base.DeleteProperty(key);
    }
}
