using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Служит для передачи в среду выполнения сценария информации о количестве ожидаемых параметров метода.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public sealed class ParametersCountAttribute : Attribute
    {
        public int Value { get; private set; }

        public ParametersCountAttribute(int count)
        {
            Value = count;
        }
    }
}
