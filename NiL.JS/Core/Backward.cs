using System;
using System.Reflection;

namespace NiL.JS.Backward
{
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
#if NET35 || NET40 || __MonoCS__
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
#endif
}
