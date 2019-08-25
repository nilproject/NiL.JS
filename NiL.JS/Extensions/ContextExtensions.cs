using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Extensions
{
    public static class ContextExtensions
    {
        public static void Add(this Context context, string key, object value)
        {
            context.DefineVariable(key).Assign(value);
        }
    }
}
