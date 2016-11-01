using System;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Служит для передачи в среду выполнения скрипта информации о количестве ожидаемых параметров метода.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class ArgumentsCountAttribute : Attribute
    {
        public int Count { get; private set; }

        public ArgumentsCountAttribute(int count)
        {
            Count = count;
        }
    }
}
