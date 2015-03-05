using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    namespace Reflection
    {
        public enum MemberTypes
        {
            Constructor = 1,
            Event = 2,
            Field = 4,
            Method = 8,
            Property = 16,
            TypeInfo = 32,
            Custom = 64,
            NestedType = 128,
            All = 191,
        }
    }

    /// <summary>
    /// Portable version only!
    /// </summary>
    public static class PortableBackward
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> self)
        {
            return new ReadOnlyCollection<T>(self);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this List<T> self)
        {
            return new ReadOnlyCollection<T>(self);
        }

        public static bool IsAssignableFrom(this Type self, Type sourceType)
        {
            return self.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo());
        }

        public static bool IsSubclassOf(this Type self, Type sourceType)
        {
            return self != sourceType && self.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo());
        }

        public static Attribute[] GetCustomAttributes(this Type self, Type attributeType, bool inherit)
        {
            return self.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static bool IsDefined(this Type self, Type attributeType, bool inherit)
        {
            return self.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        /// <summary>
        /// Реализация не полностью совместима с реализацией в полном .NET.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static MemberTypes get_MemberType(this MemberInfo self)
        {
            if (self is ConstructorInfo)
                return MemberTypes.Constructor;
            if (self is EventInfo)
                return MemberTypes.Event;
            if (self is FieldInfo)
                return MemberTypes.Field;
            if (self is MethodInfo)
                return MemberTypes.Method;
            if (self is TypeInfo)
                return MemberTypes.TypeInfo;
            if (self is PropertyInfo)
                return MemberTypes.Property;
            return MemberTypes.Custom; // чёт своё, пускай сами разбираются
        }

        public static MethodInfo GetGetMethod(this PropertyInfo self)
        {
            return self.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo self)
        {
            return self.SetMethod;
        }

        public static MethodInfo GetAddMethod(this EventInfo self)
        {
            return self.AddMethod;
        }
    }
}
