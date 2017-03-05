using System;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Linq;

#if NET40 || NETCORE
using NiL.JS.Backward;
#endif

namespace NiL.JS.Extensions
{
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
                case TypeCode.Boolean:
                    return (T)(object)(bool)self; // оптимизатор разруливает такой каскад преобразований
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
                        return (T)(object)(decimal)Tools.JSObjectToDouble(self);
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
                        if (self.Value is Function && typeof(Delegate).IsAssignableFrom(typeof(T)))
                            return ((Function)self.Value).MakeDelegate<T>();

                        if (typeof(T).IsAssignableFrom(self.GetType()))
                            return (T)(object)self;

                        try
                        {
                            return (T)(Tools.convertJStoObj(self, typeof(T), true) ?? self.Value);
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
            return Tools.convertJStoObj(value, targetType, true);
        }

        public static void Assign(this JSValue target, object value)
        {
            target.Assign(JSValue.Marshal(value));
        }

#if DEBUG && !(PORTABLE || NETCORE) // TODO
        //private static WeakReference<AssemblyBuilder> dynamicAssembly = new WeakReference<AssemblyBuilder>(null);

        public static T AsImplementationOf<T>(this JSValue self)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException(typeof(T).FullName + " is not an interface type");

            if (self._oValue is T)
                return (T)self._oValue;
            if (typeof(T) == typeof(IIterable))
                return (T)self.AsIterable();
            if (typeof(T) == typeof(IIterator))
                return (T)(object)new IteratorAdapter(self);

            AssemblyBuilder assemblyBuilder = null;
            //if (!dynamicAssembly.TryGetTarget(out assemblyBuilder))
            //{
            //    assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("<nil.js>DynamicAssembly"), AssemblyBuilderAccess.RunAndCollect);
            //    dynamicAssembly.SetTarget(assemblyBuilder);
            //}

            var module = assemblyBuilder.GetDynamicModule("InterfaceImplementations") ?? assemblyBuilder.DefineDynamicModule("InterfaceImplementations");
            var typename = "<jswrapper>" + typeof(T).FullName;
            var type = (TypeBuilder)module.GetType(typename);
            if (type == null)
            {
                type = module.DefineType(typename, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object), new[] { typeof(T) });
                var methods = typeof(T).GetMethods();

                var jsobjectField = type.DefineField("_jsvalue", typeof(JSValue), FieldAttributes.Private);
                var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(JSValue) });
                {
                    var ilgen = ctor.GetILGenerator();
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldarg_1);
                    ilgen.Emit(OpCodes.Stfld, jsobjectField);
                }

                var thisParameter = Expression.Parameter(type, "self");

                var getPropertyMethod = typeof(JSValue).GetMethod(nameof(JSValue.GetProperty), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(JSValue), typeof(bool), typeof(PropertyScope) }, null);

                for (var i = 0; i < methods.Length; i++)
                {
                    var method = type.DefineMethod(
                        methods[i].Name,
                        MethodAttributes.Public,
                        methods[i].ReturnParameter.ParameterType,
                        methods[i].GetParameters().Select(x => x.ParameterType).ToArray());

                    //var tree = Tools.BuildJsCallTree(method.Name,
                    //    Expression.Convert(Expression.Field(Expression.Call(Expression.Field(thisParameter, jsobjectField), getPropertyMethod,
                    //        Expression.Constant((JSValue)method.Name), Expression.Constant(false), Expression.Constant(PropertyScope.Сommon)), "oValue"), typeof(Function)),
                    //    thisParameter,
                    //    methods[i],
                    //    null);
                    //tree.CompileToMethod(method);
                }
            }

            return default(T);
        }
#endif
    }
}
