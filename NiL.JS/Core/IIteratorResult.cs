namespace NiL.JS.Core
{
    public interface IIteratorResult
    {
#pragma warning disable IDE1006

        JSValue value { get; }
        bool done { get; }

#pragma warning restore IDE1006
    }
}
