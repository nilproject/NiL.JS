using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Functions
{
    internal sealed class MethodProxy : Function
    {
        private Func<object, object[], Arguments, object> implementation;
        private bool raw;
        private bool forceInstance;

        private object hardTarget;
        internal ParameterInfo[] parameters;
        private MethodBase method;
        private ConvertValueAttribute returnConverter;
        private ConvertValueAttribute[] paramsConverters;
        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get
            {
                return parameters;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override string name
        {
            [Hidden]
            get
            {
                return method.Name;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return null;
            }
            [Hidden]
            set
            {

            }
        }

        private MethodProxy()
        {
            parameters = new ParameterInfo[0];
            implementation = delegate { return null; };
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        public MethodProxy(MethodBase methodBase, object hardTarget)
        {
            this.method = methodBase;
            this.hardTarget = hardTarget;
            this.parameters = methodBase.GetParameters();

            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject };
            var pc = methodBase.GetCustomAttributes(typeof(ArgumentsLengthAttribute), false).ToArray();
            if (pc.Length != 0)
                _length.iValue = (pc[0] as ArgumentsLengthAttribute).Count;
            else
                _length.iValue = parameters.Length;

            for (int i = 0; i < parameters.Length; i++)
            {
                var t = parameters[i].GetCustomAttribute(typeof(ConvertValueAttribute)) as ConvertValueAttribute;
                if (t != null)
                {
                    if (paramsConverters == null)
                        paramsConverters = new ConvertValueAttribute[parameters.Length];
                    paramsConverters[i] = t;
                }
            }

            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
            {
                returnConverter = methodInfo.ReturnParameter.GetCustomAttribute(typeof(ConvertValueAttribute), false) as ConvertValueAttribute;

                forceInstance = methodBase.IsDefined(typeof(InstanceMemberAttribute), false);

                if (forceInstance)
                {
                    if (!methodInfo.IsStatic
                        || (parameters.Length == 0)
                        || (parameters.Length > 2)
                        || (parameters[0].ParameterType != typeof(JSValue))
                        || (parameters.Length > 1 && parameters[1].ParameterType != typeof(Arguments)))
                        throw new ArgumentException("Force-instance method \"" + methodBase + "\" have invalid signature");
                    raw = true;
                }
#if PORTABLE
                makeMethodOverExpression(methodInfo);
#else
                makeMethodOverEmit(methodInfo);
#endif
                RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
            }
            else if (methodBase is ConstructorInfo)
                makeConstructorOverExpression(methodBase as ConstructorInfo);
            else
                throw new NotImplementedException();
        }
#if !PORTABLE
        private void makeMethodOverEmit(MethodInfo methodInfo)
        {
            var impl = new DynamicMethod(
                "<nil.js@wrapper>" + methodInfo.Name,
                typeof(object),
                new[]
                {
                    typeof(object), // target
                    typeof(object[]), // argsArray
                    typeof(Arguments) // argsSource
                },
                typeof(MethodProxy),
                true);
            var generator = impl.GetILGenerator();

            if (!methodInfo.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldc_I4, IntPtr.Size);
                    generator.Emit(OpCodes.Add);
                }
            }

            if (forceInstance)
            {
                for (;;)
                {
                    if (methodInfo.IsStatic && parameters[0].ParameterType == typeof(JSValue))
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        if (parameters.Length == 1)
                        {
                            break;
                        }
                        else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Arguments))
                        {
                            generator.Emit(OpCodes.Ldarg_2);
                            break;
                        }
                    }
                    throw new ArgumentException("Invalid method signature");
                }
                raw = true;
            }
            else if (parameters.Length == 0)
            {
                raw = true;
            }
            else
            {
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    generator.Emit(OpCodes.Ldarg_2);
                }
                else
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        switch (i)
                        {
                            case 0:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_0);
                                    break;
                                }
                            case 1:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_1);
                                    break;
                                }
                            case 2:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_2);
                                    break;
                                }
                            case 3:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_3);
                                    break;
                                }
                            case 4:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_4);
                                    break;
                                }
                            case 5:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_5);
                                    break;
                                }
                            case 6:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_6);
                                    break;
                                }
                            case 7:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_7);
                                    break;
                                }
                            case 8:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_8);
                                    break;
                                }
                            default:
                                {
                                    generator.Emit(OpCodes.Ldc_I4, i);
                                    break;
                                }
                        }
                        generator.Emit(OpCodes.Ldelem_Ref);
                        if (parameters[i].ParameterType.IsValueType)
                            generator.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
                    }
                }
            }
            if (methodInfo.IsStatic || method.DeclaringType.IsValueType)
                generator.Emit(OpCodes.Call, methodInfo);
            else
                generator.Emit(OpCodes.Callvirt, methodInfo);
            if (methodInfo.ReturnType == typeof(void))
                generator.Emit(OpCodes.Ldnull);
            else if (methodInfo.ReturnType.IsValueType)
                generator.Emit(OpCodes.Box, methodInfo.ReturnType);
            generator.Emit(OpCodes.Ret);
            implementation = (Func<object, object[], Arguments, object>)impl.CreateDelegate(typeof(Func<object, object[], Arguments, object>));
        }
#else
        private void makeMethodOverExpression(MethodInfo methodInfo)
        {
            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (forceInstance)
            {
                for (; ; )
                {
                    if (methodInfo.IsStatic && parameters[0].ParameterType == typeof(JSValue))
                    {
                        if (parameters.Length == 1)
                        {
                            tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)));
                            break;
                        }
                        else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Arguments))
                        {
                            tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)), argsSource);
                            break;
                        }
                    }
                    throw new ArgumentException("Invalid method signature");
                }
            }
            else if (parameters.Length == 0)
            {
                raw = true;
                tree = methodInfo.IsStatic ?
                    Expression.Call(methodInfo)
                    :
                    Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo);
            }
            else
            {
                prms = new Expression[parameters.Length];
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    tree = methodInfo.IsStatic ?
                        Expression.Call(methodInfo, argsSource)
                        :
                        Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, argsSource);
                }
                else
                {
                    for (var i = 0; i < prms.Length; i++)
                        prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
                    tree = methodInfo.IsStatic ?
                        Expression.Call(methodInfo, prms)
                        :
                        Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);
                }
            }
            if (methodInfo.ReturnType == typeof(void))
                tree = Expression.Block(tree, Expression.Constant(null));
            try
            {
                implementation = Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
        }
#endif
        private void makeConstructorOverExpression(ConstructorInfo constructorInfo)
        {
            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (parameters.Length == 0)
            {
                raw = true;
                tree = Expression.New(constructorInfo.DeclaringType);
            }
            else
            {
                prms = new Expression[parameters.Length];
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    tree = Expression.New(constructorInfo, argsSource);
                }
                else
                {
                    for (var i = 0; i < prms.Length; i++)
#if NET35
                        prms[i] = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#else
                        prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#endif
                    tree = Expression.New(constructorInfo, prms);
                }
            }
            try
            {
                implementation = Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expressions.Expression[] arguments, Context initiator, Function newTarget, bool withSpread, bool withNew)
        {
            if (withNew)
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly)
                {
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));
                }
            }
            else
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithNewOnly)
                {
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name));
                }
            }
            if (parameters.Length == 0 || (forceInstance && parameters.Length == 1))
                return Invoke(withNew, correctTargetObject(targetObject, creator.body._strict), null, newTarget);

            if (raw || withSpread)
            {
                return base.InternalInvoke(targetObject, arguments, initiator, newTarget, true, withNew);
            }
            else
            {
                // copied from ConvertArgs
                object[] args = null;
                int targetCount = parameters.Length;
                args = new object[targetCount];
                for (int i = targetCount; i-- > 0;)
                {
                    var obj = arguments.Length > i ? Tools.PrepareArg(initiator, arguments[i]) : notExists;
                    if (obj.Exists)
                    {
                        args[i] = marshal(obj, parameters[i].ParameterType);
                        if (paramsConverters != null && paramsConverters[i] != null)
                            args[i] = paramsConverters[i].To(args[i]);
                    }
                    if (args[i] == null)
                    {
                        args[i] = parameters[i].DefaultValue;

#if PORTABLE
                        if (args[i] != null && args[i].GetType().FullName == "System.DBNull")
                        {
                            if (parameters[i].ParameterType.GetTypeInfo().IsValueType)
#else
                        if (args[i] is DBNull)
                        {
                            if (parameters[i].ParameterType.IsValueType)
#endif
                                args[i] = Activator.CreateInstance(parameters[i].ParameterType);
                            else
                                args[i] = null;
                        }
                    }
                }
                return Interop.TypeProxy.Proxy(InvokeImpl(targetObject, args, null));
            }
        }

        [Hidden]
        internal object InvokeImpl(JSValue thisBind, object[] args, Arguments argsSource)
        {
            object target = null;
            if (forceInstance)
            {
                if (thisBind != null && thisBind.valueType >= JSValueType.Object)
                {
                    // Объект нужно развернуть до основного значения. Даже если это обёртка над примитивным значением
                    target = thisBind.Value;
                    if (target is TypeProxy)
                        target = (target as TypeProxy).prototypeInstance ?? thisBind.Value;
                    // ForceInstance работает только если первый аргумент типа JSValue
                    if (!(target is JSValue))
                        target = thisBind;
                }
                else
                    target = thisBind ?? undefined;
            }
            else if (!method.IsStatic && !method.IsConstructor)
            {
                target = hardTarget ?? getTargetObject(thisBind ?? undefined, method.DeclaringType);
                if (target == null)
                {
                    // Исключительная ситуация. Я не знаю почему Function.length обобщённое свойство, а не константа. Array.length работает по-другому.
                    if (method.Name == "get_length" && typeof(Function).IsAssignableFrom(method.DeclaringType))
                        return 0;

                    ExceptionsHelper.Throw(new TypeError("Can not call function \"" + this.name + "\" for object of another type."));
                }
            }

            try
            {
                object res = implementation(
                    target,
                    raw ? null : (args ?? ConvertArgs(argsSource, true)),
                    argsSource);

                if (returnConverter != null)
                    res = returnConverter.From(res);
                return res;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                    e = e.InnerException;
                if (e is JSException)
                    throw e;
                ExceptionsHelper.Throw(new TypeError(e.Message), e);
            }
            return null;
        }

        private object getDummy()
        {
            if (typeof(JSValue).IsAssignableFrom(method.DeclaringType))
                if (typeof(Function).IsAssignableFrom(method.DeclaringType))
                    return this;
                else if (typeof(TypedArray).IsAssignableFrom(method.DeclaringType))
                    return new Int8Array();
                else
                    return new JSValue();
            if (typeof(Error).IsAssignableFrom(method.DeclaringType))
                return new Error();
            return null;
        }

        public MethodProxy(MethodBase methodBase)
            : this(methodBase, null)
        {
        }

        private static object getTargetObject(JSValue _this, Type targetType)
        {
            if (_this == null)
                return null;
            _this = _this.oValue as JSValue ?? _this; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(_this, targetType);
            return res;
        }

        internal object[] ConvertArgs(Arguments source, bool dummyValueTypes)
        {
            if (parameters.Length == 0)
                return null;
            int targetCount = parameters.Length;
            object[] res = new object[targetCount];
            if (source != null) // it is possible
            {
                for (int i = targetCount; i-- > 0;)
                {
                    var obj = source[i];
                    if (obj.Exists)
                    {
                        res[i] = marshal(obj, parameters[i].ParameterType);
                        if (paramsConverters != null && paramsConverters[i] != null)
                            res[i] = paramsConverters[i].To(res[i]);
                    }
                    if (dummyValueTypes)
                    {
#if PORTABLE
                        if (res[i] == null && parameters[i].ParameterType.GetTypeInfo().IsValueType)
                            res[i] = Activator.CreateInstance(parameters[i].ParameterType);
#else
                        if (res[i] == null && parameters[i].ParameterType.IsValueType)
                            res[i] = Activator.CreateInstance(parameters[i].ParameterType);
#endif
                    }
                }
            }
            return res;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            return TypeProxy.Proxy(InvokeImpl(targetObject, null, arguments));
        }

        private static object[] convertArray(BaseLibrary.Array array)
        {
            var arg = new object[array.data.Length];
            for (var j = arg.Length; j-- > 0;)
            {
                var temp = (array.data[j] ?? undefined).Value;
                arg[j] = temp is NiL.JS.BaseLibrary.Array ? convertArray(temp as NiL.JS.BaseLibrary.Array) : temp;
            }
            return arg;
        }

        internal static object[] argumentsToArray(Arguments source)
        {
            var len = source.length;
            var res = new object[len];
            for (int i = 0; i < len; i++)
                res[i] = source[i] as object;
            return res;
        }

        private static object marshal(JSValue obj, Type targetType)
        {
            if (obj == null)
                return null;
            var v = Tools.convertJStoObj(obj, targetType);
            if (v != null)
                return v;
            v = obj.Value;
            if (v is BaseLibrary.Array)
                return convertArray(v as BaseLibrary.Array);
            else if (v is ProxyConstructor)
                return (v as ProxyConstructor).proxy.hostedType;
            else if (v is Function && targetType.IsSubclassOf(typeof(Delegate)))
                return (v as Function).MakeDelegate(targetType);
            else if (targetType.IsArray)
            {
                var eltype = targetType.GetElementType();
#if PORTABLE
                if (eltype.GetTypeInfo().IsPrimitive)
                {
#else
                if (eltype.IsPrimitive)
                {
#endif
                    if (eltype == typeof(byte) && v is ArrayBuffer)
                        return (v as ArrayBuffer).GetData();
                    var ta = v as TypedArray;
                    if (ta != null && ta.ElementType == eltype)
                        return ta.ToNativeArray();
                }
            }
            return v;
        }

#if DEVELOPBRANCH || VERSION21
        public sealed override JSValue bind(Arguments args)
        {
            if (hardTarget != null || args.Length == 0)
                return this;

            return new MethodProxy()
            {
                hardTarget = getTargetObject(args[0], method.DeclaringType),
                method = method,
                parameters = parameters,
                implementation = implementation
            };
        }

        public override Delegate MakeDelegate(Type delegateType)
        {
            try
            {
                var methodInfo = method as MethodInfo;
                return methodInfo.CreateDelegate(delegateType, hardTarget);
            }
            catch
            {
            }

            return base.MakeDelegate(delegateType);
        }
#endif
    }
}
