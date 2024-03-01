using System;

namespace NiL.JS.Core.Interop;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
[AttributeUsage(
      AttributeTargets.Field
    | AttributeTargets.ReturnValue
    | AttributeTargets.Parameter)]
/// <summary>
/// Indicates that a value needs to be converted for access from script
/// </summary>
public abstract class ConvertValueAttribute : Attribute
{
    /// <summary>
    /// Called to convert from a value type to a script-accessible type.
    /// </summary>
    /// <param name="source">Исходное значение</param>
    /// <returns>Преобразованное значение.</returns>
    public abstract object From(object source);
    /// <summary>
    /// Called to convert from a value accessible from the script to the original value type.
    /// </summary>
    /// <param name="source">Значение, доступное из сценария.</param>
    /// <returns>Преобразованное значение.</returns>
    public abstract object To(JSValue source);
}
