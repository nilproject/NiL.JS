using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    public class MethodProxy : Function
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
        public override bool IsRecursive
        {
            get
            {
                return false;
            }
        }
        [Hidden]
        public override FunctionType Type
        {
            get
            {
                return FunctionType.Function;
            }
        }
        [Hidden]
        public override string Name
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
                if (!retType.IsValueType)
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
                                    else if (typeof(object).IsAssignableFrom(parameters[0].ParameterType))
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
                                    else if (typeof(object).IsAssignableFrom(parameters[0].ParameterType))
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
            for (var j = 0; j < arg.Length; j++)
            {
                var temp = array[j].Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
            }
            return arg;
        }

        private static object[] argumentsToArray(JSObject source)
        {
            var len = source.GetMember("length").iValue;
            var res = new object[len];
            for (int i = 0; i < len; i++)
                res[i] = source.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
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
                    return argumentsToArray(source);
            }
            int targetCount = parameters.Length;
            object[] res = new object[targetCount];
            for (int i = len; i-- > 0; )
            {
                var obj = source.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
                if (obj != notExist)
                {
                    res[i] = Tools.convertJStoObj(obj, parameters[i].ParameterType);
                    if (res[i] == null)
                    {
                        var v = obj.Value;
                        if (v is Core.BaseTypes.Array)
                            res[i] = convertArray(v as Core.BaseTypes.Array);
                        else if (v is TypeProxy)
                        {
                            var tp = v as TypeProxy;
                            res[i] = (tp.bindFlags & BindingFlags.Static) != 0 ? tp.hostedType : tp.prototypeInstance;
                        }
                        else if (v is TypeProxyConstructor)
                            res[i] = (v as TypeProxyConstructor).proxy.hostedType;
                        else if (v is Function && parameters[i].ParameterType.IsSubclassOf(typeof(Delegate)))
                            res[i] = (v as Function).MakeDelegate(parameters[i].ParameterType);
                        else if (v is ArrayBuffer && parameters[i].ParameterType.IsAssignableFrom(typeof(byte[])))
                            res[i] = (v as ArrayBuffer).Data;
                        else
                            res[i] = v;
                    }
                    if (paramsConverters != null && paramsConverters[i] != null)
                        res[i] = paramsConverters[i].To(res[i]);
                }
            }
            return res;
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
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
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum };
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
                    throw new JSException(new TypeError("Can not call function \"" + this.Name + "\" for object of another type."));
            }
            try
            {
                switch (mode)
                {
                    case CallMode.FuncDynamicZero:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.DeclaringType) // your bunny wrote
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
                            if (target != null && info.IsVirtual && target.GetType() != info.DeclaringType) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1(args ?? argumentsToArray(argsSource));
                            }
                            else
#endif
                                res = delegateF2(target, args ?? argumentsToArray(argsSource));
                            break;
                        }
                    case CallMode.FuncDynamicOneRaw:
                        {
#if !NET35
                            if (target != null && info.IsVirtual && target.GetType() != info.DeclaringType) // your bunny wrote
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
                            if (target != null && info.IsVirtual && target.GetType() != info.DeclaringType) // your bunny wrote
                            {
                                bool di = true;
                                SetFieldValue(_targetInfo, delegateF1, target, typeof(object), _targetInfo.Attributes, typeof(Action), ref di);
                                res = delegateF1((args ?? ConvertArgs(argsSource))[0]);
                            }
                            else
#endif
                                res = delegateF2(target, (args ?? ConvertArgs(argsSource))[0]);
                            break;
                        }
                    case CallMode.FuncStaticZero:
                        {
                            res = delegateF0();
                            break;
                        }
                    case CallMode.FuncStaticOneArray:
                        {
                            res = delegateF1(args ?? argumentsToArray(argsSource));
                            break;
                        }
                    case CallMode.FuncStaticOneRaw:
                        {
                            res = delegateF1(argsSource);
                            break;
                        }
                    case CallMode.FuncStaticOne:
                        {
                            res = delegateF1((args ?? ConvertArgs(argsSource))[0]);
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
                                if (target != null && target.GetType() != info.DeclaringType) // your bunny wrote
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
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(e.Message)), e);
            }
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, JSObject args)
        {
            return TypeProxy.Proxy(InvokeImpl(thisBind, null, args));
        }

        [Hidden]
        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, create, own);
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
