using System;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Указывает, какой тип необходимо представить в цепочке прототипов объекта-прослойки для помеченного типа.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class PrototypeAttribute : Attribute
    {
        public Type PrototypeType { get; private set; }
        public bool Replace { get; private set; }

        public PrototypeAttribute(Type type)
            : this(type, false)
        {
        }

        internal PrototypeAttribute(Type type, bool doNotChainButReplace)
        {
            Replace = doNotChainButReplace;
            PrototypeType = type;
        }
    }
}
