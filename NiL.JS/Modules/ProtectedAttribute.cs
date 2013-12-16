using System;

namespace NiL.JS.Modules
{    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class ProtectedAttribute : Attribute
    {

    }
}
