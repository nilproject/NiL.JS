using System;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core.Interop;

public sealed class NativeError : Error
{
    public NativeError(string message) : base(message)
    {
    }

    public Exception exception { get; set; }
}
