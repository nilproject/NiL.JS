using System;

namespace NiL.JS.Core.Interop;

[Prototype(typeof(JSObject), true)]
internal sealed class StaticProxy : Proxy
{
    internal override JSObject PrototypeInstance
    {
        get
        {
            return null;
        }
    }

    internal override bool IsInstancePrototype
    {
        get
        {
            return false;
        }
    }

    [Hidden]
    public StaticProxy(GlobalContext context, Type type, bool indexersSupport)
        : base(context, type, indexersSupport)
    {

    }
}
