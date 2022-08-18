using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NiL.JS.Backward
{
    internal static class EmpryArrayHelper
    {
        private static class EmptyArrayContainer<T>
        {
            public static readonly T[] EmptyArray = new T[0];
        }

        public static T[] Empty<T>()
        {
            return EmptyArrayContainer<T>.EmptyArray;
        }
    }

#if NETSTANDARD1_3
    internal enum MemberTypes
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
#endif

    internal static class Backward
    {
#if NETSTANDARD1_3
        internal static ConstructorInfo[] GetConstructors<T>(this Type self)
        {
            return self.GetTypeInfo().DeclaredConstructors.ToArray();
        }
#endif

        internal static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> self)
        {
            return new ReadOnlyCollection<T>(self);
        }

        internal static ReadOnlyCollection<T> AsReadOnly<T>(this List<T> self)
        {
            return new ReadOnlyCollection<T>(self);
        }

        internal static bool IsSubclassOf(this Type self, Type sourceType)
        {
            return self != sourceType && self.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo());
        }

        internal static object[] GetCustomAttributes(this Type self, Type attributeType, bool inherit)
        {
            return self.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        internal static bool IsDefined(this Type self, Type attributeType, bool inherit)
        {
            return self.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        internal static MemberTypes GetMemberType(this MemberInfo self)
        {
            if (self is ConstructorInfo)
                return MemberTypes.Constructor;
            if (self is EventInfo)
                return MemberTypes.Event;
            if (self is FieldInfo)
                return MemberTypes.Field;
            if (self is MethodInfo)
                return MemberTypes.Method;
#if !NET40
            if (self is TypeInfo)
                return MemberTypes.TypeInfo;
#else
            if (self is Type)
                return MemberTypes.TypeInfo;
#endif
            if (self is PropertyInfo)
                return MemberTypes.Property;
            return MemberTypes.Custom; // чёт своё, пускай сами разбираются
        }

        private static readonly Type[] _Types =
            {
                null,
                typeof(object),
                Type.GetType("System.DBNull"),
                typeof(bool),
                typeof(char),
                typeof(sbyte),
                typeof(byte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                null,
                typeof(string)
            };

        internal static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;

            if (type.GetTypeInfo().IsClass)
            {
                if (type == _Types[2])
                    return (TypeCode)2; // Database null value

                if (type == typeof(string))
                    return TypeCode.String;

                return TypeCode.Object;
            }

            for (var i = 3; i < _Types.Length; i++)
            {
                if (_Types[i] == type)
                    return (TypeCode)i;
            }

            return TypeCode.Object;
        }

#if !NET40
        internal static Type GetInterface(this Type type, string name)
        {
            foreach (var i in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (i.FullName.Contains(name))
                    return i;
            }

            return null;
        }
#endif
    }
}

#if NET40_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) { ReturnValue = returnValue; }
        public bool ReturnValue { get; }
    }
}
#endif
