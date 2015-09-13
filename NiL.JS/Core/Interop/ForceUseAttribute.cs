using System;

namespace NiL.JS.Core.Interop
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
 sealed class ForceUseAttribute : Attribute
    {
    }
}
