using System;
using System.Reflection;
using System.Runtime.Serialization;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.TypeProxing
{
    [Serializable]
    public sealed class MethodProxy : Function
    {
#if !NET35
        private delegate void SetValueDelegate(FieldInfo field, Object obj, Object value, Type fieldType, FieldAttributes fieldAttr, Type declaringType, ref bool domainInitialized);
        private static readonly SetValueDelegate SetFieldValue = Activator.CreateInstance(typeof(SetValueDelegate), null, typeof(RuntimeFieldHandle).GetMethod("SetValue", BindingFlags.Static | BindingFlags.NonPublic).MethodHandle.GetFunctionPointer()) as SetValueDelegate;
        private static readonly FieldInfo _targetInfo = typeof(Action).GetMember("_target", BindingFlags.NonPublic | BindingFlags.Instance)[0] as FieldInfo;
#endif
        private enum CallMode
        {
            Default = 0,
            FuncDynamicOne,
            FuncDynamicOneArray,
            FuncDynamicOneRaw,
            FuncDynamicZero,
            FuncStaticOne,
            FuncStaticOneArray,
            FuncStaticOneRaw,
            FuncStaticZero,

            ActionDynamicOne,
            ActionDynamicOneArray,
            ActionDynamicOneRaw,
            ActionDynamicZero,
            ActionStaticOne,
            ActionStaticOneArray,
            ActionStaticOneRaw,
            ActionStaticZero,
        }

        private object hardTarget = null;
        private MethodBase info;
        private CallMode mode;
        private AllowUnsafeCallAttribute[] alternedTypes;
        private Modules.ConvertValueAttribute converter;
        private Modules.ConvertValueAttribute[] paramsConverters;
        private bool constructorMode;
        private bool callOverload;

        private Func<object> delegateF0 = null;
        private Func<object, object> delegateF1 = null;
        private Func<object, object, object> delegateF2 = null;
        private Action delegateA0 = null;
        private Action<object> delegateA1 = null;
        private Action<object, object> delegateA2 = null;

        internal readonly ParameterInfo[] parameters;

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return FunctionType.Function;
            }
        }
        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return info.Name;
            }
        }
        [Hidden]
        public MethodBase Method
        {
            [Hidden]
            get { return info; }
        }
        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get { return parameters; }
        }

        public MethodProxy(MethodBase methodinfo, Modules.ConvertValueAttribute converter, Modules.ConvertValueAttribute[] paramsConverters)
            : this(methodinfo, null)
        {
            this.converter = converter;
            this.paramsConverters = paramsConverters;
        }

        public MethodProxy(MethodBase methodinfo)
            : this(methodinfo, null)
        {
        }

        public MethodProxy(MethodBase methodinfo, object hardTarget)
        {
            if (!(methodinfo is MethodInfo)
               && !(methodinfo is ConstructorInfo))
                throw new ArgumentException("methodinfo");
            _prototype = undefined;
            this.hardTarget = hardTarget;
            info = methodinfo;
            parameters = info.GetParameters();
            Type retType = null;
            constructorMode = methodinfo is ConstructorInfo;
            alternedTypes = (AllowUnsafeCallAttribute[])methodinfo.GetCustomAttributes(typeof(AllowUnsafeCallAttribute), false);

            var methodInfo = info as MethodInfo;
            retType = constructorMode ? typeof(void) : methodInfo.ReturnType;
            converter = constructorMode ? null : methodInfo.ReturnParameter.GetCustomAttribute(typeof(Modules.ConvertValueAttribute), false) as Modules.ConvertValueAttribute;
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
            if (!retType.IsValueType && !info.ReflectedType.IsValueType)
            {
                switch (parameters.Length)
                {
                    case 0:
                        {
                            if (methodinfo.IsStatic)
                            {
                                this.delegateF0 = Activator.CreateInstance(typeof(Func<object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object>;
                                mode = CallMode.FuncStaticZero;
                            }
                            else
                            {
                                this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                this.delegateF0 = Activator.CreateInstance(typeof(Func<object>), this, info.MethodHandle.GetFunctionPointer()) as Func<object>;
                                mode = CallMode.FuncDynamicZero;
                            }
                            break;
                        }
                    case 1:
                        {
                            if (methodinfo.IsStatic)
                            {
                                if (parameters[0].ParameterType == typeof(Arguments))
                                {
                                    this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                    mode = CallMode.FuncStaticOneRaw;
                                }
                                else if (!parameters[0].ParameterType.IsValueType)
                                {
                                    this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                    mode = CallMode.FuncStaticOne;
                                }
                            }
                            else
                            {
                                if (parameters[0].ParameterType == typeof(Arguments))
                                {
                                    this.delegateF2 = Activator.CreateInstance(typeof(Func<object, object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object, object>;
                                    this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), this, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                    mode = CallMode.FuncDynamicOneRaw;
                                }
                                else if (!parameters[0].ParameterType.IsValueType)
                                {
                                    this.delegateF2 = Activator.CreateInstance(typeof(Func<object, object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object, object>;
                                    this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), this, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                    mode = CallMode.FuncDynamicOne;
                                }
                            }
                            break;
                        }
                }
            }
            else if (retType == typeof(void) && !info.ReflectedType.IsValueType)
            {
                switch (parameters.Length)
                {
                    case 0:
                        {
                            if (methodinfo.IsStatic)
                            {
                                this.delegateA0 = Activator.CreateInstance(typeof(Action), null, info.MethodHandle.GetFunctionPointer()) as Action;
                                mode = CallMode.ActionStaticZero;
                            }
                            else
                            {
                                this.delegateA1 = Activator.CreateInstance(typeof(Action<object>), null, info.MethodHandle.GetFunctionPointer()) as Action<object>;
                                this.delegateA0 = Activator.CreateInstance(typeof(Action), this, info.MethodHandle.GetFunctionPointer()) as Action;
                                mode = CallMode.ActionDynamicZero;
                            }
                            break;
                        }
                    case 1:
                        {
                            if (methodinfo.IsStatic)
                            {
                                if (parameters[0].ParameterType == typeof(Arguments))
                                {
                                    this.delegateA1 = Activator.CreateInstance(typeof(Action<object>), null, info.MethodHandle.GetFunctionPointer()) as Action<object>;
                                    mode = CallMode.ActionStaticOneRaw;
                                }
                                else if (!parameters[0].ParameterType.IsValueType)
                                {
                                    this.delegateA1 = Activator.CreateInstance(typeof(Action<object>), null, info.MethodHandle.GetFunctionPointer()) as Action<object>;
                                    mode = CallMode.ActionStaticOne;
                                }
                            }
                            else
                            {
                                if (parameters[0].ParameterType == typeof(Arguments))
                                {
                                    this.delegateA2 = Activator.CreateInstance(typeof(Action<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Action<object, object>;
                                    this.delegateA1 = Activator.CreateInstance(typeof(Action<object>), this, info.MethodHandle.GetFunctionPointer()) as Action<object>;
                                    mode = CallMode.ActionDynamicOneRaw;
                                }
                                else if (!parameters[0].ParameterType.IsValueType)
                                {
                                    this.delegateA2 = Activator.CreateInstance(typeof(Action<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Action<object, object>;
                                    this.delegateA1 = Activator.CreateInstance(typeof(Action<object>), this, info.MethodHandle.GetFunctionPointer()) as Action<object>;
                                    mode = CallMode.ActionDynamicOne;
                                }
                            }
                            break;
                        }
                }
            }

            callOverload = info.IsDefined(typeof(CallOverloaded), true);
            if (_length == null)
                _length = new Number(0) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
            var pc = info.GetCustomAttributes(typeof(Modules.ParametersCountAttribute), false);
            if (pc.Length != 0)
                _length.iValue = (pc[0] as Modules.ParametersCountAttribute).Count;
            else
                _length.iValue = parameters.Length;
        }

        private static object[] convertArray(NiL.JS.Core.BaseTypes.Array array)
        {
            var arg = new object[array.data.Count];
            for (var j = arg.Length; j-- > 0; )
            {
                var temp = (array.data[j] ?? undefined).Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
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
                //if (ptype == typeof(IEnumerable<JSObject>)
                //    || ptype == typeof(IEnumerable<object>)
                //    || ptype == typeof(ICollection)
                //    || ptype == typeof(IEnumerable)
                //    || ptype == typeof(List<JSObject>)
                //    || ptype == typeof(JSObject[])
                //    || ptype == typeof(List<object>)
                //    || ptype == typeof(object[]))
                //    return new[] { argumentsToArray<JSObject>(source) };
            }
            int targetCount = parameters.Length;
            object[] res = new object[targetCount];
            for (int i = len; i-- > 0; )
            {
                var obj = source[i];
                if (obj.isExist)
                {
                    res[i] = marshal(obj, parameters[i].ParameterType);
                    if (paramsConverters != null && paramsConverters[i] != null)
                        res[i] = paramsConverters[i].To(res[i]);
                }
            }
            return res;
        }

        private static object marshal(JSObject obj, Type targetType)
        {
            var v = Tools.convertJStoObj(obj, targetType);
            if (v != null)
                return v;
            v = obj.Value;
            if (v is Core.BaseTypes.Array)
                return convertArray(v as Core.BaseTypes.Array);
            else if (v is TypeProxy)
            {
                var tp = v as TypeProxy;
                return (tp.bindFlags & BindingFlags.Static) != 0 ? tp.hostedType : tp.prototypeInstance.oValue;
            }
            else if (v is ProxyConstructor)
                return (v as ProxyConstructor).proxy.hostedType;
            else if (v is Function && targetType.IsSubclassOf(typeof(Delegate)))
                return (v as Function).MakeDelegate(targetType);
            else if (targetType.IsArray)
            {
                var eltype = targetType.GetElementType();
                if (eltype.IsPrimitive)
                {
                    if (eltype == typeof(byte) && v is ArrayBuffer)
                        return (v as ArrayBuffer).Data;
                    var ta = v as TypedArray;
                    if (ta != null && ta.ElementType == eltype)
                        return ta.ToNativeArray();
                }
            }
            return v;
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
            if (_this == null)
                return null;
            _this = _this.oValue as JSObject ?? _this; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(_this, targetType);
            if (res != null)
                return res;
            for (var i = alternedTypes.Length; i-- > 0; )
            {
                var at = alternedTypes[i];
                res = Tools.convertJStoObj(_this, at.baseType);
                if (res != null)
                {
                    res = at.Convert(res);
                    return res;
                }
            }
            return null;
        }

        [Hidden]
        internal object InvokeImpl(JSObject thisBind, object[] args, Arguments argsSource)
        {
            object res = null;
            object target = null;
            if (!constructorMode && !info.IsStatic)
            {
                target = hardTarget ?? getTargetObject(thisBind ?? undefined, info.DeclaringType);
                if (target == null)
                    throw new JSException(new TypeError("Can not call function \"" + this.name + "\" for object of another type."));
            }
            try
            {
                switch (mode)
                {
                    case CallMode.FuncDynamicOneRaw:
                        {
                            if (!callOverload && target != null && info.IsVirtual && target.GetType() != info.ReflectedType) // your bunny wrote
#if !NET35
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(argsSource);
                            }
                            else
#else
                                goto default;
#endif
                                res = delegateF2(target, argsSource);
                            break;
                        }
                    case CallMode.FuncDynamicZero:
                        {
                            if (!callOverload && target != null && info.IsVirtual && target.GetType() != info.ReflectedType) // your bunny wrote
#if !NET35
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF0, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF0();
                            }
                            else
#else
                                goto default;
#endif
                                res = delegateF1(target);
                            break;
                        }
                    case CallMode.FuncDynamicOneArray:
                        {
                            if (!callOverload && target != null && info.IsVirtual && target.GetType() != info.ReflectedType) // your bunny wrote
#if !NET35
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(args != null ? args[0] : argumentsToArray(argsSource));
                            }
                            else
#else
                                goto default;
#endif
                                res = delegateF2(target, args ?? argumentsToArray(argsSource));
                            break;
                        }
                    case CallMode.FuncDynamicOne:
                        {
                            if (!callOverload && target != null && info.IsVirtual && target.GetType() != info.ReflectedType) // your bunny wrote
#if !NET35
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(args == null ? marshal(argsSource[0], parameters[0].ParameterType) : args[0]);
                            }
                            else
#else
                                goto default;
#endif
                                res = delegateF2(target, args == null ? marshal(argsSource[0], parameters[0].ParameterType) : args[0]);
                            break;
                        }
                    case CallMode.FuncStaticZero:
                        {
                            res = delegateF0();
                            break;
                        }
                    case CallMode.FuncStaticOneArray:
                        {
                            res = delegateF1(args != null ? args[0] : argumentsToArray(argsSource));
                            break;
                        }
                    case CallMode.FuncStaticOneRaw:
                        {
                            res = delegateF1(argsSource);
                            break;
                        }
                    case CallMode.FuncStaticOne:
                        {
                            res = delegateF1(args == null ? marshal(argsSource[0], parameters[0].ParameterType) : args[0]);
                            break;
                        }
                    case CallMode.ActionDynamicOneRaw:
                        {
                            if (constructorMode)
                            {
                                res = FormatterServices.GetUninitializedObject(info.ReflectedType);
                                delegateA2(res, argsSource);
                            }
                            else
                            {
                                delegateA2(target, argsSource);
                                res = null;
                            }
                            break;
                        }
                    default:
                        {
                            if (args == null)
                                args = ConvertArgs(argsSource);
                            if (constructorMode)
                                res = (info as ConstructorInfo).Invoke(args);
                            else
                            {
                                if (!callOverload && target != null && info.IsVirtual && target.GetType() != info.ReflectedType) // your bunny wrote
                                {
                                    var minfo = info as MethodInfo;
                                    if (minfo.ReturnType != typeof(void) && minfo.ReturnType.IsValueType)
                                        throw new JSException((new NiL.JS.Core.BaseTypes.TypeError("Invalid return type of method " + minfo)));
                                    if (parameters.Length > 16)
                                        throw new JSException((new NiL.JS.Core.BaseTypes.TypeError("Invalid parameters count of method " + minfo)));
                                    for (int i = 0; i < parameters.Length; i++)
                                        if (parameters[i].ParameterType.IsValueType)
                                            throw new JSException((new NiL.JS.Core.BaseTypes.TypeError("Invalid parameter (" + parameters[i].Name + ") type of method " + minfo)));
                                    var cargs = args;
                                    Delegate del = null;
                                    switch (parameters.Length)
                                    {
                                        case 0:
                                            del = (Activator.CreateInstance(typeof(Func<object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 1:
                                            del = (Activator.CreateInstance(typeof(Func<object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 2:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 3:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 4:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 5:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 6:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 7:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 8:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 9:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 10:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 11:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 12:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 13:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 14:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 15:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                        case 16:
                                            del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate);
                                            break;
                                    }
                                    res = del.DynamicInvoke(cargs);
                                }
                                else
                                {
                                    res = info.Invoke(
                                        target,
                                        BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy,
                                        null,
                                        args,
                                        System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }
                            break;
                        }
                }
                if (converter != null)
                    res = converter.From(res);
                return res;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                    e = e.InnerException;
                if (e is JSException)
                    throw e;
#if DEBUG
                if (e is AccessViolationException || e is NullReferenceException)
                    System.Diagnostics.Debugger.Break();
#endif
                throw new JSException(new TypeError(e.Message), e);
            }
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, Arguments args)
        {
            return TypeProxy.Proxy(InvokeImpl(thisBind, null, args));
        }

        public override string ToString()
        {
            var res = "function " + info.Name + "(";
            var prms = parameters;
            for (int i = 0; i < prms.Length; i++)
            {
                if (i > 0)
                    res += ", ";
                res += prms[i].Name + "/*:" + prms[i].ParameterType.Name + "*/";
            }
            res += "){ [native code] }";
            return res;
        }
    }
}
