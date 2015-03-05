using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Служит для передачи в среду выполнения скрипта информации о количестве ожидаемых параметров метода.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class ArgumentsLengthAttribute : Attribute
    {
        public int Count { get; private set; }

        public ArgumentsLengthAttribute(int count)
        {
            Count = count;
        }
    }
}
