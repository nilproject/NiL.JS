using System;
using NiL.JS.Core;

namespace NiL.JS.Extensions
{
    public static class ContextExtensions
    {
        [Obsolete("Use Add(this Context context, string key, JSValue value)")]
        public static void Add(this Context context, string key, object value)
        {
            context.DefineVariable(key).Assign(value);
        }

        public static void Add(this Context context, string key, JSValue value)
        {
            context.DefineVariable(key).Assign(value);
        }
    }
}
