using NiL.JS.Backward;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NiL.JS.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines if the given type matches Task<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTaskOf(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo == null)
            {
                return false;
            }

            if (typeInfo.IsGenericType
                && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return true;
            }

            return IsTaskOf(typeInfo.BaseType);
        }
    }
}
