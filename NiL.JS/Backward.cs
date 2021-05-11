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

#if NET35
    internal delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11, T12 prm12);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11, T12 prm12, T13 prm13);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11, T12 prm12, T13 prm13, T14 prm14);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11, T12 prm12, T13 prm13, T14 prm14, T15 prm15);
    internal delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(T1 prm1, T2 prm2, T3 prm3, T4 prm4, T5 prm5, T6 prm6, T7 prm7, T8 prm8, T9 prm9, T10 prm10, T11 prm11, T12 prm12, T13 prm13, T14 prm14, T15 prm15, T16 prm16);
#endif

#if NET40 || __MonoCS__
    internal static class ParameterInfoExtension
    {
        public static Object GetCustomAttribute(this ParameterInfo _this, Type attributeType)
        {
            var t = _this.GetCustomAttributes(attributeType, true);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }

        public static Object GetCustomAttribute(this ParameterInfo _this, Type attributeType, bool inherit)
        {
            var t = _this.GetCustomAttributes(attributeType, inherit);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }
    }

    internal static class PropertyInfoExtension
    {
        public static Object GetCustomAttribute(this PropertyInfo _this, Type attributeType)
        {
            var t = _this.GetCustomAttributes(attributeType, true);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }

        public static Object GetCustomAttribute(this PropertyInfo _this, Type attributeType, bool inherit)
        {
            var t = _this.GetCustomAttributes(attributeType, inherit);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }
    }

    internal static class FieldInfoExtension
    {
        public static object GetCustomAttribute(this FieldInfo _this, Type attributeType)
        {
            var t = _this.GetCustomAttributes(attributeType, true);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }

        public static object GetCustomAttribute(this FieldInfo _this, Type attributeType, bool inherit)
        {
            var t = _this.GetCustomAttributes(attributeType, inherit);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }

        public static TAttributeType GetCustomAttribute<TAttributeType>(this FieldInfo _this) where TAttributeType : Attribute
        {
            var t = _this.GetCustomAttributes(typeof(TAttributeType), true);
            if (t == null || t.Length == 0)
                return null;

            return t[0] as TAttributeType;
        }
    }

    internal static class TypeExtensions
    {
        public static MethodInfo GetRuntimeMethod(this Type type, string name, Type[] types)
        {
            return type.GetMethod(name, types);
        }

        public static MethodInfo[] GetRuntimeMethods(this Type type)
        {
            return type.GetMethods();
        }

        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static TAttributeType GetCustomAttribute<TAttributeType>(this Type type) where TAttributeType : Attribute
        {
            var t = type.GetCustomAttributes(typeof(TAttributeType), true);
            if (t == null || t.Length == 0)
                return null;

            return t[0] as TAttributeType;
        }
    }

    internal static class DelegateExtensions
    {
        public static MethodInfo GetMethodInfo(this Delegate @delegate)
        {
            return @delegate.Method;
        }
    }

    internal static class MethodInfoExtensions
    {
        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, methodInfo, true);
        }
    }

    internal static class MethodBaseExtensions
    {
        public static object GetCustomAttribute(this MethodBase _this, Type attributeType)
        {
            var t = _this.GetCustomAttributes(attributeType, true);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }

        public static object GetCustomAttribute(this MethodBase _this, Type attributeType, bool inherit)
        {
            var t = _this.GetCustomAttributes(attributeType, inherit);
            if (t == null || t.Length == 0)
                return null;
            return t[0];
        }
    }
#endif
}
