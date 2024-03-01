using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Extensions;

public static class JSValueExtensions
{
    public static bool Is(this JSValue self, JSValueType type)
    {
        return self != null && self._valueType == type;
    }

    public static bool Is<T>(this JSValue self)
    {
        if (self == null)
            return false;
#if PORTABLE || NETCORE
        switch (typeof(T).GetTypeCode())
#else
        switch (Type.GetTypeCode(typeof(T)))
#endif
        {
            case TypeCode.Boolean:
            {
                return self.Is(JSValueType.Boolean);
            }
            case TypeCode.Byte:
            {
                return self.Is(JSValueType.Integer) && (self._iValue & ~byte.MaxValue) == 0;
            }
            case TypeCode.Char:
            {
                return (self != null
                    && self._valueType == JSValueType.Object
                    && self._oValue is char);
            }
            case TypeCode.Decimal:
            {
                return false;
            }
            case TypeCode.Double:
            {
                return self.Is(JSValueType.Double);
            }
            case TypeCode.Int16:
            {
                return self.Is(JSValueType.Integer) && (self._iValue & ~ushort.MaxValue) == 0;
            }
            case TypeCode.Int32:
            {
                return self.Is(JSValueType.Integer);
            }
            case TypeCode.Int64:
            {
                return self.Is(JSValueType.Integer) || (self.Is(JSValueType.Double) && self._dValue == (long)self._dValue);
            }
            case TypeCode.Object:
            {
                return self.Value is T;
            }
            case TypeCode.SByte:
            {
                return self.Is(JSValueType.Integer) && (self._iValue & ~byte.MaxValue) == 0;
            }
            case TypeCode.Single:
            {
                return self.Is(JSValueType.Double) && (float)self._dValue == self._dValue;
            }
            case TypeCode.String:
            {
                return self.Is(JSValueType.String);
            }
            case TypeCode.UInt16:
            {
                return self.Is(JSValueType.Integer) && (self._iValue & ~ushort.MaxValue) == 0;
            }
            case TypeCode.UInt32:
            {
                return self.Is(JSValueType.Integer);
            }
            case TypeCode.UInt64:
            {
                return (self.Is(JSValueType.Integer) && self._iValue >= 0) || (self.Is(JSValueType.Double) && self._dValue == (ulong)self._dValue);
            }
            default:
            {
                return false;
            }
        }
    }

    public static T As<T>(this JSValue self)
    {
#if PORTABLE || NETCORE
        switch (typeof(T).GetTypeCode())
#else
        switch (Type.GetTypeCode(typeof(T)))
#endif
        {
            case TypeCode.Double:
                return GetDefinedOr<T>(self, (T)(object)double.NaN);
        }

        return GetDefinedOr<T>(self, default(T));
    }

    public static T GetDefinedOr<T>(this JSValue self, T defaultValue)
    {
        if (!self.Defined)
            return defaultValue;

#if PORTABLE || NETCORE
        switch (typeof(T).GetTypeCode())
#else
        switch (Type.GetTypeCode(typeof(T)))
#endif
        {
            case TypeCode.Boolean:
            {
                return (T)(object)(bool)self; // оптимизатор разруливает такой каскад преобразований
            }

            case TypeCode.Byte:
            {
                return (T)(object)(byte)Tools.JSObjectToInt32(self);
            }

            case TypeCode.Char:
            {
                if (self._valueType == JSValueType.Object
                    && self._oValue is char)
                    return (T)self._oValue;
                break;
            }

            case TypeCode.Decimal:
            {
                var value = Tools.JSObjectToDouble(self);
                if (double.IsNaN(value))
                    value = 0;

                return (T)(object)(decimal)value;
            }

            case TypeCode.Double:
            {
                return (T)(object)Tools.JSObjectToDouble(self);
            }

            case TypeCode.Int16:
            {
                return (T)(object)(Int16)Tools.JSObjectToInt32(self);
            }

            case TypeCode.Int32:
            {
                return (T)(object)Tools.JSObjectToInt32(self);
            }

            case TypeCode.Int64:
            {
                return (T)(object)Tools.JSObjectToInt64(self);
            }

            case TypeCode.Object:
            {
                var value = self.Value;
                if (self is null || value is null)
                    return default(T);

                if (value is Function && typeof(Delegate).IsAssignableFrom(typeof(T)))
                    return ((Function)value).MakeDelegate<T>();

                if (self._oValue is not null && typeof(T).IsAssignableFrom(self._oValue.GetType()))
                    return (T)self._oValue;

                if (typeof(T).IsAssignableFrom(value.GetType()))
                    return (T)value;

                try
                {
                    return (T)(Tools.ConvertJStoObj(self, typeof(T), true) ?? value);
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }

            case TypeCode.SByte:
            {
                return (T)(object)(sbyte)Tools.JSObjectToInt32(self);
            }

            case TypeCode.Single:
            {
                return (T)(object)(float)Tools.JSObjectToDouble(self);
            }

            case TypeCode.String:
            {
                return (T)(object)self.ToString();
            }

            case TypeCode.UInt16:
            {
                return (T)(object)(ushort)Tools.JSObjectToInt32(self);
            }

            case TypeCode.UInt32:
            {
                return (T)(object)(uint)Tools.JSObjectToInt32(self);
            }

            case TypeCode.UInt64:
            {
                return (T)(object)(ulong)Tools.JSObjectToInt64(self);
            }
        }

        throw new InvalidCastException();
    }

    public static bool IsNaN(this JSValue self)
    {
        return self != null && self._valueType == JSValueType.Double && double.IsNaN(self._dValue);
    }

    public static bool IsUndefined(this JSValue self)
    {
        return self != null && self._valueType <= JSValueType.Undefined;
    }

    public static bool IsNumber(this JSValue self)
    {
        return self._valueType == JSValueType.Integer || self._valueType == JSValueType.Double;
    }

    public static object ConvertToType(this JSValue value, Type targetType)
    {
        return Tools.ConvertJStoObj(value, targetType, true);
    }
    public static void Assign(this JSValue target, object value, Context context)
    {
        target.Assign(context.GlobalContext.ProxyValue(value));
    }

    [Obsolete("Use Assign(this JSValue, JSValue) or Use Assign(this JSValue, object, Context)")]
    public static void Assign(this JSValue target, object value)
    {
        target.Assign(Context.CurrentGlobalContext.ProxyValue(value));
    }

    private static WeakReference<AssemblyBuilder> dynamicAssembly = new WeakReference<AssemblyBuilder>(null);
    private static WeakReference<ModuleBuilder> dynamicModule = new WeakReference<ModuleBuilder>(null);

    public static T AsImplementationOf<T>(this JSValue self, Context context) where T : class
    {
        if (self._oValue is T)
            return (T)self._oValue;
        if (typeof(T) == typeof(IIterable))
            return (T)self.AsIterable();
        if (typeof(T) == typeof(IIterator))
            return (T)(object)new IteratorAdapter(self);

        TypeBuilder type = null;
        AssemblyBuilder assemblyBuilder = null;
        lock (dynamicAssembly)
        {
            if (!dynamicAssembly.TryGetTarget(out assemblyBuilder))
            {
                assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("<nil.js>DynamicAssembly"), AssemblyBuilderAccess.RunAndCollect);
                dynamicAssembly.SetTarget(assemblyBuilder);

                var m = assemblyBuilder.GetDynamicModule("InterfaceImplementations") ?? assemblyBuilder.DefineDynamicModule("InterfaceImplementations");
                dynamicModule.SetTarget(m);
            }
        }

        dynamicModule.TryGetTarget(out var module);
        var typename = "<jswrapper>" + typeof(T).FullName;
        type = (TypeBuilder)module.GetType(typename);
        if (type == null)
        {
            var baseType = typeof(T).IsClass ? typeof(T) : typeof(object);
            var interfaces = typeof(T).IsInterface ? [typeof(T)] : Type.EmptyTypes;
            type = module.DefineType(typename, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, baseType, interfaces);
            var methods = typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsVirtual && !x.IsFinal && x.Name is not "Finalize")
                .ToArray();

            var jsobjectField = type.DefineField("_jsvalue", typeof(JSValue), FieldAttributes.Private);
            var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, [typeof(JSValue)]);
            {
                var ilgen = ctor.GetILGenerator();
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Stfld, jsobjectField);
                ilgen.Emit(OpCodes.Ret);
            }

            var delegatesField = type.DefineField("_delegates", typeof(Delegate[]), FieldAttributes.Private | FieldAttributes.Static);
            var delegates = new Delegate[methods.Length];

            var jsValParameter = Expression.Parameter(typeof(JSValue), "jsval");

            var getPropertyMethod = typeof(JSValue).GetMethod(nameof(JSValue.GetProperty), BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(JSValue), typeof(bool), typeof(PropertyScope)], null);

            for (var i = 0; i < methods.Length; i++)
            {
                var paramters = methods[i].GetParameters();
                var method = type.DefineMethod(
                    methods[i].Name,
                    methods[i].Attributes & ~(MethodAttributes.Abstract | MethodAttributes.NewSlot),
                    methods[i].ReturnParameter.ParameterType,
                    paramters.Select(x => x.ParameterType).ToArray());

                var functionTree = Expression.Convert(
                    Expression.Property(
                        Expression.Call(
                            jsValParameter,
                            getPropertyMethod,
                            Expression.Constant((JSValue)(methods[i].GetCustomAttribute<JavaScriptNameAttribute>()?.Name ?? method.Name)),
                            Expression.Constant(false),
                            Expression.Constant(PropertyScope.Common)),
                        nameof(JSValue.Value)),
                    typeof(Function));

                var tree = Tools.BuildJsCallTree(
                    context,
                    method.Name,
                    functionTree,
                    jsValParameter,
                    methods[i],
                    null);

                var @delegate = tree.Compile();
                delegates[i] = @delegate;

                var ilGenerator = method.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, delegatesField);

                ilGenerator.Emit(OpCodes.Ldc_I4, i);

                ilGenerator.Emit(OpCodes.Ldelem_Ref);

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, jsobjectField);

                for (var pi = 0; pi < paramters.Length; pi++)
                    ilGenerator.Emit(OpCodes.Ldarg, pi + 1);

                var invokeMethod = @delegate.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
                ilGenerator.EmitCall(OpCodes.Call, invokeMethod, null);

                ilGenerator.Emit(OpCodes.Ret);
            }

            type.CreateType().GetField(delegatesField.Name, BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, delegates);
        }

        var resultType = type.CreateType();

        return (T)Activator.CreateInstance(resultType, self);
    }
}
