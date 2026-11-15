using System;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core;

#if !NETCORE
[Serializable]
#endif
public sealed class PropertyPair
{
    internal Function _getter;
    internal Function _setter;

    public Function Getter { get { return _getter; } }
    public Function Setter { get { return _setter; } }

    internal PropertyPair() { }

    public PropertyPair(Function getter, Function setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public override string ToString()
    {
        var tempStr = "[";
        if (_getter != null)
            tempStr += "Getter";
        if (_setter != null)
            tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
        if (tempStr.Length == 1)
            return "[Invalid Property]";
        tempStr += "]";
        return tempStr;
    }
}
