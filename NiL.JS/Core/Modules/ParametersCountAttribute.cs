using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Служит для передачи в среду выполнения скрипта информации о количестве ожидаемых параметров метода.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class ParametersCountAttribute : Attribute
    {
        public int Count { get; private set; }

        public ParametersCountAttribute(int count)
        {
            Count = count;
        }
    }
}
