using System;

namespace NiL.JS.Core.Interop;

[Prototype(typeof(JSObject), true)]
internal sealed class PrototypeProxy : Proxy
{
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
}
