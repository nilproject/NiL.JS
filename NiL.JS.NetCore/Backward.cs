using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NiL.JS.Backward
{
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

    internal static class PortableBackward
    {
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

        internal static Attribute[] GetCustomAttributes(this Type self, Type attributeType, bool inherit)
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
            if (self is TypeInfo)
                return MemberTypes.TypeInfo;
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

        internal static Type GetInterface(this Type type, string name)
        {
            return type.GetTypeInfo().ImplementedInterfaces.First(x => x.Name == name);
        }
    }
}
