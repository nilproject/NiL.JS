using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Указывает, что при вызове помеченного метода всегда будет вызываться перегруженная версия, соответствующая экземпляру объекта, 
    /// а не источнику получения указателя на функцию
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class CallOverloaded : Attribute
    {
    }
}
