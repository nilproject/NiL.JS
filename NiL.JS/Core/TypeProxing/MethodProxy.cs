using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace NiL.JS.Core
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
        }

        private object hardTarget = null;
        private MethodBase info;
        private CallMode mode;
        private AllowUnsafeCallAttribute[] alternedTypes;
        private Func<object> delegateF0 = null;
        private Func<object, object> delegateF1 = null;
        private Func<object, object, object> delegateF2 = null;
        private Modules.ConvertValueAttribute converter;
        private Modules.ConvertValueAttribute[] paramsConverters;
        private ParameterInfo[] parameters;

        [Hidden]
        public override FunctionType Type
        {
            get
            {
                return FunctionType.Function;
            }
        }
        [Hidden]
        public override string name
        {
            get
            {
                return info.Name;
            }
        }
        [Hidden]
        public MethodBase Method { get { return info; } }
        [Hidden]
        public ParameterInfo[] Parameters { get { return parameters; } }

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
            _prototype = undefined;
            this.hardTarget = hardTarget;
            info = methodinfo;
            parameters = info.GetParameters();
            Type retType = null;
            alternedTypes = (AllowUnsafeCallAttribute[])methodinfo.GetCustomAttributes(typeof(AllowUnsafeCallAttribute), false);
            if (info is MethodInfo)
            {
                var mi = info as MethodInfo;
                retType = mi.ReturnType;
                converter = mi.ReturnParameter.GetCustomAttribute(typeof(Modules.ConvertValueAttribute), false) as Modules.ConvertValueAttribute;
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
                                    if (parameters[0].ParameterType == typeof(JSObject[]))
                                    {
                                        this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                        mode = CallMode.FuncStaticOneArray;
                                    }
                                    else if (parameters[0].ParameterType == typeof(JSObject))
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
                                    if (parameters[0].ParameterType == typeof(JSObject[]))
                                    {
                                        this.delegateF2 = Activator.CreateInstance(typeof(Func<object, object, object>), null, info.MethodHandle.GetFunctionPointer()) as Func<object, object, object>;
                                        this.delegateF1 = Activator.CreateInstance(typeof(Func<object, object>), this, info.MethodHandle.GetFunctionPointer()) as Func<object, object>;
                                        mode = CallMode.FuncDynamicOneArray;
                                    }
                                    else if (parameters[0].ParameterType == typeof(JSObject))
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
            }
            else if (info is ConstructorInfo)
            {
                // TODO.
            }
            else
                throw new ArgumentException("methodinfo");
        }

        private static object[] convertArray(NiL.JS.Core.BaseTypes.Array array)
        {
            var arg = new object[array.data.Count];
            for (var j = 0U; j < arg.Length; j++)
            {
                var temp = (array.data[j] ?? undefined).Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
            }
            return arg;
        }

        internal static T[] argumentsToArray<T>(JSObject source)
        {
            var len = source.GetMember("length").iValue;
            var res = new T[len];
            for (int i = 0; i < len; i++)
                res[i] = (T)(source.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture)) as object);
            return res;
        }

        internal object[] ConvertArgs(JSObject source)
        {
            if (parameters.Length == 0)
                return null;
            int len = 0;
            if (source != null)
            {
                var length = source.GetMember("length");
                len = length.valueType == JSObjectType.Property ? (length.oValue as Function[])[1].Invoke(source, null).iValue : length.iValue;
            }
            if (parameters.Length == 1)
            {
                var ptype = parameters[0].ParameterType;
                if (ptype == typeof(JSObject))
                    return new object[] { source };
                if (ptype == typeof(IEnumerable<JSObject>)
                    || ptype == typeof(IEnumerable<object>)
                    || ptype == typeof(ICollection)
                    || ptype == typeof(IEnumerable)
                    || ptype == typeof(List<JSObject>)
                    || ptype == typeof(JSObject[])
                    || ptype == typeof(List<object>)
                    || ptype == typeof(object[]))
                    return new[] { argumentsToArray<JSObject>(source) };
            }
            int targetCount = parameters.Length;
            object[] res = new object[targetCount];
            for (int i = len; i-- > 0; )
            {
                var obj = source.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
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
            else if (v is ArrayBuffer && targetType.IsAssignableFrom(typeof(byte[])))
                return (v as ArrayBuffer).Data;
            return v;
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
            _this = _this.oValue as JSObject ?? _this; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(_this, targetType);
            if (res != null)
                return res;
            for (var i = alternedTypes.Length; i-- > 0; )
            {
                res = Tools.convertJStoObj(_this, alternedTypes[i].baseType);
                if (res != null)
                {
                    res = alternedTypes[i].Convert(res);
                    return res;
                }
            }
            return null;
        }
        
        [Modules.DoNotEnumerate]
        [Modules.DoNotDelete]
        public override JSObject length
        {
            [Hidden]
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
                var pc = info.GetCustomAttributes(typeof(Modules.ParametersCountAttribute), false);
                if (pc.Length != 0)
                    _length.iValue = (pc[0] as Modules.ParametersCountAttribute).Count;
                else
                    _length.iValue = parameters.Length;
                return _length;
            }
        }

        [Hidden]
        internal object InvokeImpl(JSObject thisBind, object[] args, JSObject argsSource)
        {
            object res = null;
            object target = null;
            if (!(info is ConstructorInfo))
            {
                target = info.IsStatic ? null : hardTarget ?? getTargetObject(thisBind ?? JSObject.Null, info.DeclaringType);
                if (target == null && !info.IsStatic)
                    throw new JSException(new TypeError("Can not call function \"" + this.name + "\" for object of another type."));
            }
            try
            {
                switch (mode)
                {
                    case CallMode.FuncDynamicZero:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.ReflectedType && !info.ReflectedType.IsAssignableFrom(target.GetType())) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF0, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF0();
                            }
                            else
#endif
                                res = delegateF1(target);
                            break;
                        }
                    case CallMode.FuncDynamicOneArray:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.ReflectedType && !info.ReflectedType.IsAssignableFrom(target.GetType())) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(args != null ? args[0] : argumentsToArray<object>(argsSource));
                            }
                            else
#endif
                                res = delegateF2(target, args ?? argumentsToArray<object>(argsSource));
                            break;
                        }
                    case CallMode.FuncDynamicOneRaw:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.ReflectedType && !info.ReflectedType.IsAssignableFrom(target.GetType())) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(argsSource);
                            }
                            else
#endif
                                res = delegateF2(target, argsSource);
                            break;
                        }
                    case CallMode.FuncDynamicOne:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.ReflectedType && !info.ReflectedType.IsAssignableFrom(target.GetType())) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(args == null ? marshal(argsSource["0"], parameters[0].ParameterType) : args[0]);
                            }
                            else
#endif
                                res = delegateF2(target, args == null ? marshal(argsSource["0"], parameters[0].ParameterType) : args[0]);
                            break;
                        }
                    case CallMode.FuncStaticZero:
                        {
                            res = delegateF0();
                            break;
                        }
                    case CallMode.FuncStaticOneArray:
                        {
                            res = delegateF1(args != null ? args[0] : argumentsToArray<object>(argsSource));
                            break;
                        }
                    case CallMode.FuncStaticOneRaw:
                        {
                            res = delegateF1(argsSource);
                            break;
                        }
                    case CallMode.FuncStaticOne:
                        {
                            res = delegateF1(args == null ? marshal(argsSource["0"], parameters[0].ParameterType) : args[0]);
                            break;
                        }
                    default:
                        {
                            if (args == null)
                                args = ConvertArgs(argsSource);
                            if (info is ConstructorInfo)
                                res = (info as ConstructorInfo).Invoke(args);
                            else
                            {
                                if (target != null && info.IsVirtual && target.GetType() != info.ReflectedType && !info.ReflectedType.IsAssignableFrom(target.GetType())) // your bunny wrote
                                {
                                    var minfo = info as MethodInfo;
                                    if (minfo.ReturnType != typeof(void) && minfo.ReturnType.IsValueType)
                                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid return type of method " + minfo)));
                                    if (parameters.Length > 16)
                                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid parameters count of method " + minfo)));
                                    for (int i = 0; i < parameters.Length; i++)
                                        if (parameters[i].ParameterType.IsValueType)
                                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid parameter (" + parameters[i].Name + ") type of method " + minfo)));
                                    var cargs = args;
                                    Delegate del = null;
                                    switch (parameters.Length)
                                    {
                                        case 0: del = (Activator.CreateInstance(typeof(Func<object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 1: del = (Activator.CreateInstance(typeof(Func<object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 2: del = (Activator.CreateInstance(typeof(Func<object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 3: del = (Activator.CreateInstance(typeof(Func<object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 4: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 5: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 6: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 7: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 8: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 9: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 10: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 11: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 12: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 13: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 14: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 15: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
                                        case 16: del = (Activator.CreateInstance(typeof(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>), target, minfo.MethodHandle.GetFunctionPointer()) as Delegate); break;
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
                if (e is AccessViolationException
                    || e is NullReferenceException)
                    System.Diagnostics.Debugger.Break();
#endif
                throw new JSException(new TypeError(e.Message), e);
            }
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, JSObject args)
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
