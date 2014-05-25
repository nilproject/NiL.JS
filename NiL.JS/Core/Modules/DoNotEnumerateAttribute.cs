using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Указывает, что помеченный член следует пропустить при перечислении в конструкции for-in
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public sealed class DoNotEnumerateAttribute : Attribute
    {
        public DoNotEnumerateAttribute()
        { }
    }
}
