using System;

namespace NiL.JS.Core.TypeProxing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InstanceMemberAttribute : Attribute
    {
        public InstanceMemberAttribute()
        {

        }
    }
}
