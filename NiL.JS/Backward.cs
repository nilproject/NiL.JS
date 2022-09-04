using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    internal static class Backward
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

#if NET461
namespace System
{
    public struct ValueTuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }
}
#endif

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

namespace NiL.JS.Backward
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> keyValuePair, out T1 value1, out T2 value2)
        {
            value1 = keyValuePair.Key;
            value2 = keyValuePair.Value;
        }
    }
}


namespace Microsoft.CSharp.RuntimeBinder
{
    /*[EditorBrowsable(EditorBrowsableState.Never)]
    [Flags]
    public enum CSharpArgumentInfoFlags
    {
        None = 0,
        UseCompileTimeType = 1,
        Constant = 2,
        NamedArgument = 4,
        IsRef = 8,
        IsOut = 16,
        IsStaticType = 32
    }

    /*[EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CSharpArgumentInfo
    {
        private CSharpArgumentInfoFlags _flags;
        private string _name;

        public CSharpArgumentInfo(CSharpArgumentInfoFlags flags, string name)
        {
            _flags = flags;
            _name = name;
        }

        public static CSharpArgumentInfo Create(CSharpArgumentInfoFlags flags, string name)
            => new CSharpArgumentInfo(flags, name);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Flags]
    public enum CSharpBinderFlags
    {
        None = 0,
        CheckedContext = 1,
        InvokeSimpleName = 2,
        InvokeSpecialName = 4,
        BinaryOperationLogical = 8,
        ConvertExplicit = 16,
        ConvertArrayIndex = 32,
        ResultIndexed = 64,
        ValueFromCompoundAssignment = 128,
        ResultDiscarded = 256
    }

    /*[EditorBrowsable(EditorBrowsableState.Never)]
    public static class Binder
    {
        public static CallSiteBinder BinaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
            => new DynamicMetaObjectBinder(flags, operation, context, argumentInfo);
        public static CallSiteBinder Convert(CSharpBinderFlags flags, Type type, Type context);
        public static CallSiteBinder GetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder GetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder Invoke(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder InvokeConstructor(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder InvokeMember(CSharpBinderFlags flags, string name, IEnumerable<Type> typeArguments, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder IsEvent(CSharpBinderFlags flags, string name, Type context);
        public static CallSiteBinder SetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder SetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
        public static CallSiteBinder UnaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo);
    }*/
}
#endif
