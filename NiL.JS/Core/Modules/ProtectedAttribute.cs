using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Наличие этого аттрибута указывает обработчику JavaScript, что необходимо запретить изменение значения этого поля внутри скрипта.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class ProtectedAttribute : Attribute
    {

    }
}
