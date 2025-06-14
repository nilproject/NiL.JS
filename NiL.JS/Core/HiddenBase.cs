using NiL.JS.Core.Interop;

namespace NiL.JS.Core;

internal abstract class HiddenBase
{
    [Hidden]
    public override bool Equals(object obj) => base.Equals(obj);

    [Hidden]
    public override int GetHashCode() => base.GetHashCode();

    [Hidden]
    public override string ToString() => base.ToString();
}
