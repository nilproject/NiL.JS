using System;

namespace NiL.JS.Core.Interop
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(
          AttributeTargets.Field
        | AttributeTargets.Property
        | AttributeTargets.ReturnValue
        | AttributeTargets.Parameter)]
    /// <summary>
    /// Указывает на необходимость преобразования значения для доступа из сценария.
    /// </summary>
    public abstract class ConvertValueAttribute : Attribute
    {
        /// <summary>
        /// Вызывается для преобразования из типа значения в тип, доступный из сценария.
        /// </summary>
        /// <param name="source">Исходное значение</param>
        /// <returns>Преобразованное значение.</returns>
        public abstract object From(object source);
        /// <summary>
        /// Вызывается для преобразования из значения, доступного из сценария в исходный тип значения.
        /// </summary>
        /// <param name="source">Значение, доступное из сценария.</param>
        /// <returns>Преобразованное значение.</returns>
        public abstract object To(JSValue source);
    }
}
