using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Указывает на необходимость учитывать член при создании представителя в среде выполнения сценария 
    /// вне зависимости от модификатора доступа
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.All
#if PORTABLE
        & ~AttributeTargets.Constructor
#endif
, AllowMultiple = false, Inherited = true)]
#if !WRC
    public
#endif
 sealed class ForceUse : Attribute
    {
    }
}
