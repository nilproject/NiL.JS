using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.Functions
{
    public sealed class MethodProxy : Function
    {
        private enum _Mode
        {
            Regular = 0,
            A1,
            A2,
            F1,
            F2
        }

        public static bool PartiallyTrusted { get; private set; }
        private static FieldInfo handleInfo;
        private Func<object, object[], Arguments, object> implementation;
        private bool raw;
        private bool forceInstance;

        #region Только для небезопасных вызовов
        private AllowUnsafeCallAttribute[] alternedTypes;
        private Action<object> action1;
        private Action<object, object> action2;
        private Func<object, object> func1;
        private Func<object, object, object> func2;
        private _Mode mode;
        #endregion

        private object hardTarget;
        internal ParameterInfo[] parameters;
        private MethodBase methodBase;
        private ConvertValueAttribute returnConverter;
        private ConvertValueAttribute[] paramsConverters;
        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get { return parameters; }
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
                return methodBase.Name;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSObject prototype
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

        static MethodProxy()
        {
            try
            {
                Func<IntPtr, IntPtr> donor = x => x;
#if PORTABLE
                var members = donor.GetType().GetRuntimeFields().GetEnumerator();
                members.MoveNext(); // 0
                members.MoveNext(); // 1
                members.MoveNext(); // 2
                members.MoveNext(); // 3
                handleInfo = members.Current as FieldInfo;
#else
                var members = donor.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                handleInfo = members[3] as FieldInfo;
#endif

                var handle = handleInfo.GetValue(donor);
                var forceConverterHandle = (IntPtr)handle;

                var forceConverterType = typeof(Func<,>).MakeGenericType(typeof(JSObject), typeof(NiL.JS.BaseLibrary.String));
#if PORTABLE
                var forceConverterConstructor = forceConverterType.GetTypeInfo().DeclaredConstructors.First();
#else
                var forceConverterConstructor = forceConverterType.GetConstructors().First();
#endif
                var forceConverter = forceConverterConstructor.Invoke(new object[] { null, (IntPtr)forceConverterHandle }) as Func<JSObject, NiL.JS.BaseLibrary.String>;

                var test = forceConverter(new JSObject() { oValue = "hello", valueType = JSObjectType.String });
                if (test == null || test.GetType() != typeof(JSObject))
                    PartiallyTrusted = true;
            }
            catch
            {
                PartiallyTrusted = true;
            }

        }

        public MethodProxy()
        {
            implementation = (a, b, c) => null;
        }

        public MethodProxy(MethodBase methodBase, object hardTarget)
        {
            this.methodBase = methodBase;
            this.hardTarget = hardTarget;

            parameters = methodBase.GetParameters();

            if (_length == null)
                _length = new Number(0) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
            var pc = methodBase.GetCustomAttributes(typeof(Modules.ArgumentsLengthAttribute), false).ToArray();
            if (pc.Length != 0)
                _length.iValue = (pc[0] as Modules.ArgumentsLengthAttribute).Count;
            else
                _length.iValue = parameters.Length;

            for (int i = 0; i < parameters.Length; i++)
            {
                var t = parameters[i].GetCustomAttribute(typeof(Modules.ConvertValueAttribute)) as Modules.ConvertValueAttribute;
                if (t != null)
                {
                    if (paramsConverters == null)
                        paramsConverters = new Modules.ConvertValueAttribute[parameters.Length];
                    paramsConverters[i] = t;
                }
            }

            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (methodBase is MethodInfo)
            {
                var methodInfo = methodBase as MethodInfo;
                returnConverter = methodInfo.ReturnParameter.GetCustomAttribute(typeof(Modules.ConvertValueAttribute), false) as Modules.ConvertValueAttribute;

                forceInstance = methodBase.IsDefined(typeof(InstanceMemberAttribute), false);

                if (forceInstance)
                {
                    if (!methodInfo.IsStatic
                        || (parameters.Length == 0)
                        || (parameters.Length > 2)
                        || (parameters[0].ParameterType != typeof(JSObject))
                        || (parameters.Length > 1 && parameters[1].ParameterType != typeof(Arguments)))
                        throw new ArgumentException("Force-instance method \"" + methodBase + "\" have invalid signature");
                    raw = true;
                }

                if (!PartiallyTrusted
                    && !methodInfo.IsStatic
                    && (parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments)))
#if PORTABLE
                    && !methodInfo.ReturnType.GetTypeInfo().IsValueType)
#else
 && !methodInfo.ReturnType.IsValueType)
#endif
                {
                    var t = methodBase.GetCustomAttributes(typeof(AllowUnsafeCallAttribute), false).ToArray();
                    alternedTypes = new AllowUnsafeCallAttribute[t.Length];
                    for (var i = 0; i < t.Length; i++)
                        alternedTypes[i] = (AllowUnsafeCallAttribute)t[i];
                }

                #region Magic
                if (alternedTypes != null)
                {
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        if (parameters.Length == 0)
                        {
#if PORTABLE
                            var methodDelegate = methodInfo.CreateDelegate(typeof(Action<>).MakeGenericType(methodBase.DeclaringType));
                            var handle = handleInfo.GetValue(methodDelegate);
                            var forceConverterConstructor = typeof(Action<object>).GetTypeInfo().DeclaredConstructors.First();
#else
                            var forceConverterConstructor = typeof(Action<object>).GetConstructors().First();
                            var handle = methodInfo.MethodHandle.GetFunctionPointer();
#endif
                            action1 = (Action<object>)forceConverterConstructor.Invoke(new object[] { null, (IntPtr)handle });
                            mode = _Mode.A1;
                        }
                        else // 1
                        {
#if PORTABLE
                            var methodDelegate = methodInfo.CreateDelegate(typeof(Action<,>).MakeGenericType(methodBase.DeclaringType, typeof(Arguments)));
                            var handle = handleInfo.GetValue(methodDelegate);
                            var forceConverterConstructor = typeof(Action<object, object>).GetTypeInfo().DeclaredConstructors.First();
#else
                            var forceConverterConstructor = typeof(Action<object, object>).GetConstructors().First();
                            var handle = methodInfo.MethodHandle.GetFunctionPointer();
#endif
                            action2 = (Action<object, object>)forceConverterConstructor.Invoke(new object[] { null, (IntPtr)handle });
                            mode = _Mode.A2;
                        }
                    }
                    else
                    {
                        if (parameters.Length == 0)
                        {
#if PORTABLE
                            var methodDelegate = methodInfo.CreateDelegate(typeof(Func<,>).MakeGenericType(methodBase.DeclaringType, methodInfo.ReturnType));
                            var handle = handleInfo.GetValue(methodDelegate);
                            var forceConverterConstructor = typeof(Func<object, object>).GetTypeInfo().DeclaredConstructors.First();
#else
                            var forceConverterConstructor = typeof(Func<object, object>).GetConstructors().First();
                            var handle = methodInfo.MethodHandle.GetFunctionPointer();
#endif
                            func1 = (Func<object, object>)forceConverterConstructor.Invoke(new object[] { getDummy(), (IntPtr)handle });
                            mode = _Mode.F1;

                        }
                        else // 1
                        {
#if PORTABLE
                            var methodDelegate = methodInfo.CreateDelegate(typeof(Func<,,>).MakeGenericType(methodBase.DeclaringType, typeof(Arguments), methodInfo.ReturnType));
                            var handle = handleInfo.GetValue(methodDelegate);
                            var forceConverterConstructor = typeof(Func<object, object, object>).GetTypeInfo().DeclaredConstructors.First();
#else
                            var forceConverterConstructor = typeof(Func<object, object, object>).GetConstructors().First();
                            var handle = methodInfo.MethodHandle.GetFunctionPointer();
#endif
                            func2 = (Func<object, object, object>)forceConverterConstructor.Invoke(new object[] { getDummy(), (IntPtr)handle });
                            mode = _Mode.F2;
                        }
                    }
                    return; // больше ничего не требуется, будет вызывать через этот путь
                }
                #endregion

                if (forceInstance)
                {
                    if (parameters.Length == 1)
                    {
                        tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSObject)));
                    }
                    else // 2
                    {
                        System.Diagnostics.Debug.Assert(parameters.Length == 2);
                        tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSObject)), argsSource);
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
#if NET35
                            prms[i] = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#else
                            prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#endif
                        tree = methodInfo.IsStatic ?
                            Expression.Call(methodInfo, prms)
                            :
                            Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);
                    }
                }
                if (methodInfo.ReturnType == typeof(void))
#if NET35
                {
#error Expression.Block do not supported in .NET 3.5
                }
#else
                    tree = Expression.Block(tree, Expression.Constant(null));
#endif
            }
            else if (methodBase is ConstructorInfo)
            {
                var constructorInfo = methodBase as ConstructorInfo;

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
            }
            else
                throw new NotImplementedException();
            try
            {
                implementation = Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
        }

        [Hidden]
        internal object InvokeImpl(JSObject thisBind, object[] args, Arguments argsSource)
        {
            object target = null;
            if (forceInstance)
            {
                if (thisBind != null && thisBind.valueType >= JSObjectType.Object)
                {
                    target = thisBind.Value;
                    if (target is TypeProxy)
                        target = (target as TypeProxy).prototypeInstance ?? thisBind.Value;
                    if (target == null || !typeof(JSObject).IsAssignableFrom(target.GetType()))
                        target = thisBind;
                }
                else
                    target = thisBind ?? undefined;
            }
            else if (!methodBase.IsStatic && !methodBase.IsConstructor)
            {
                target = hardTarget ?? getTargetObject(thisBind ?? undefined, methodBase.DeclaringType);
                if (target == null)
                    throw new JSException(new TypeError("Can not call function \"" + this.name + "\" for object of another type."));
            }
            try
            {
                object res = null;
                switch (mode)
                {
                    case _Mode.A1:
                        action1(target);
                        break;
                    case _Mode.A2:
                        action2(target, argsSource);
                        break;
                    case _Mode.F1:
                        res = func1(target);
                        break;
                    case _Mode.F2:
                        res = func2(target, argsSource);
                        break;
                    default:
                        res = implementation(
                            target,
                            raw ? null : args ?? ConvertArgs(argsSource),
                            argsSource);
                        break;
                }
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
                throw new JSException(new TypeError(e.Message), e);
            }
        }

        private object getDummy()
        {
            if (typeof(JSObject).IsAssignableFrom(methodBase.DeclaringType))
                if (typeof(Function).IsAssignableFrom(methodBase.DeclaringType))
                    return this;
                else if (typeof(TypedArray).IsAssignableFrom(methodBase.DeclaringType))
                    return new Int8Array();
                else
                    return new JSObject();
            if (typeof(Error).IsAssignableFrom(methodBase.DeclaringType))
                return new Error();
            return null;
        }

        public MethodProxy(MethodBase methodBase)
            : this(methodBase, null)
        {
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
            if (_this == null)
                return null;
            _this = _this.oValue as JSObject ?? _this; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(_this, targetType);
            if (res != null)
                return res;
            if (alternedTypes != null)
                for (var i = alternedTypes.Length; i-- > 0; )
                {
                    var at = alternedTypes[i];
                    res = Tools.convertJStoObj(_this, at.baseType);
                    if (res != null)
                        return res;
                }

            return null;
        }

        [Hidden]
        internal object[] ConvertArgs(Arguments source)
        {
            if (parameters.Length == 0)
                return null;
            int len = source.length;
            if (parameters.Length == 1)
            {
                var ptype = parameters[0].ParameterType;
                if (ptype == typeof(Arguments))
                    return new object[] { source };
            }
            int targetCount = parameters.Length;
            object[] res = new object[targetCount];
            for (int i = len; i-- > 0; )
            {
                var obj = source[i];
                if (obj.IsExist)
                {
                    res[i] = marshal(obj, parameters[i].ParameterType);
                    if (paramsConverters != null && paramsConverters[i] != null)
                        res[i] = paramsConverters[i].To(res[i]);
                }
            }
            return res;
        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.Arguments args)
        {
            return TypeProxing.TypeProxy.Proxy(InvokeImpl(thisBind, null, args));
        }

        private static object[] convertArray(NiL.JS.BaseLibrary.Array array)
        {
            var arg = new object[array.data.Count];
            for (var j = arg.Length; j-- > 0; )
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

        private static object marshal(JSObject obj, Type targetType)
        {
            if (obj == null)
                return null;
            var v = Tools.convertJStoObj(obj, targetType);
            if (v != null)
                return v;
            v = obj.Value;
            if (v is NiL.JS.BaseLibrary.Array)
                return convertArray(v as NiL.JS.BaseLibrary.Array);
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
                        return (v as ArrayBuffer).Data;
                    var ta = v as TypedArray;
                    if (ta != null && ta.ElementType == eltype)
                        return ta.ToNativeArray();
                }
            }
            return v;
        }
    }
}
