using System;

namespace NiL.JS.Core.Interop;

[AttributeUsage(
    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate, 
    AllowMultiple = false, 
    Inherited = false)]
public sealed class StrictConversionAttribute : Attribute
{
}
