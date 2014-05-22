using System;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace NiL.JS.Core
{
    [Serializable]
    public class MethodProxy : Function
    {
        private NiL.JS.Core.BaseTypes.String @string;// = new BaseTypes.String();
        private NiL.JS.Core.BaseTypes.Number number;// = new Number();
        private NiL.JS.Core.BaseTypes.Boolean boolean;// = new BaseTypes.Boolean();

        private MethodBase info;
        private Func<JSObject[], object> @delegate = null;
        private Modules.ConvertValueAttribute converter;
        private Modules.ConvertValueAttribute[] paramsConverters;
        private ParameterInfo[] parameters;

        [Modules.Hidden]
        public MethodBase Method { get { return info; } }
        [Modules.Hidden]
        public ParameterInfo[] Parameters { get { return parameters; } }

        public MethodProxy(MethodBase methodinfo, Modules.ConvertValueAttribute converter, Modules.ConvertValueAttribute[] paramsConverters)
            : this(methodinfo)
        {
            this.converter = converter;
            this.paramsConverters = paramsConverters;
        }

        public MethodProxy(MethodBase methodinfo)
        {
            info = methodinfo;
            parameters = info.GetParameters();
            if (info is MethodInfo)
            {
                var mi = info as MethodInfo;
                converter = mi.ReturnParameter.GetCustomAttribute(typeof(Modules.ConvertValueAttribute), false) as Modules.ConvertValueAttribute;
                var prmtrs = parameters;
                for (int i = 0; i < prmtrs.Length; i++)
                {
                    var t = prmtrs[i].GetCustomAttribute(typeof(Modules.ConvertValueAttribute)) as Modules.ConvertValueAttribute;
                    if (t != null)
                    {
                        if (paramsConverters == null)
                            paramsConverters = new Modules.ConvertValueAttribute[prmtrs.Length];
                        paramsConverters[i] = t;
                    }
                }
                if (methodinfo.IsStatic)
                {
                    if (mi.ReturnType.IsSubclassOf(typeof(object))
                        && prmtrs.Length == 1
                        && prmtrs[0].ParameterType == typeof(JSObject[]))
                        this.@delegate = (Func<JSObject[], object>)Delegate.CreateDelegate(typeof(Func<JSObject[], object>), mi);
                }
                else
                {

                }
            }
        }

        private static object[] convertArray(NiL.JS.Core.BaseTypes.Array array)
        {
            var arg = new object[array.length];
            for (var j = 0; j < arg.Length; j++)
            {
                var temp = array[j].Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
            }
            return arg;
        }

        private static JSObject[] argumentsToArray(JSObject source)
        {
            var len = source.GetField("length", true, false).iValue;
            var res = new JSObject[len];
            for (int i = 0; i < len; i++)
                res[i] = source.GetField(i < 10 ? Tools.NumString[i] : i.ToString(), true, true);
            return res;
        }

        internal object[] ConvertArgs(JSObject source)
        {
            if (parameters.Length == 0)
                return null;
            object[] res;
            int len = 0;
            if (source != null)
            {
                var length = source.GetField("length", true, false);
                len = length.ValueType == JSObjectType.Property ? (length.oValue as Function[])[1].Invoke(source, null).iValue : length.iValue;
            }
            if (parameters.Length == 1)
            {
                if (parameters[0].ParameterType == typeof(JSObject))
                    return new object[] { source };
                if (parameters[0].ParameterType == typeof(JSObject[]))
                {
                    res = new JSObject[len];
                    for (int i = 0; i < len; i++)
                        res[i] = source.GetField(i.ToString(), true, true);
                    return new object[] { res };
                }
            }
            int targetCount = parameters.Length;
            targetCount = System.Math.Min(targetCount, len);
            res = targetCount != 0 ? new object[targetCount] : null;
            for (int i = 0; i < targetCount; i++)
            {
                var obj = source.GetField(i < 10 ? Tools.NumString[i] : i.ToString(), true, true);
                if (obj != null)
                {
                    res[i] = embeddedTypeConvert(obj, parameters[i].ParameterType);
                    if (res[i] == null)
                    {
                        if (parameters[i].ParameterType == typeof(JSObject))
                            res[i] = obj;
                        else
                        {
                            var v = obj.ValueType == JSObjectType.Object && obj.oValue != null && obj.oValue.GetType() == typeof(object) ? obj : obj.Value;
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
                            {
                                res[i] = (v as Function).MakeDelegate(parameters[i].ParameterType);
                            }
                            else
                                res[i] = v;
                        }
                        if (paramsConverters != null && paramsConverters[i] != null)
                            res[i] = paramsConverters[i].To(res[i]);
                    }
                }
            }
            return res;
        }

        private JSObject embeddedTypeConvert(JSObject source, Type targetType)
        {
            if (source.GetType() == targetType)
                return source;
            if (source.GetType().IsSubclassOf(targetType))
                return source;

            switch (source.ValueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        if (typeof(BaseTypes.Number) != targetType && !typeof(BaseTypes.Number).IsSubclassOf(targetType))
                            return null;
                        if (number == null)
                            number = new Number();
                        number.iValue = source.iValue;
                        number.dValue = source.dValue;
                        number.ValueType = source.ValueType;
                        return number;
                    }
                case JSObjectType.String:
                    {
                        if (typeof(BaseTypes.String) != targetType && !typeof(BaseTypes.String).IsSubclassOf(targetType))
                            return null;
                        if (@string == null)
                            @string = new BaseTypes.String();
                        @string.oValue = source.oValue;
                        return @string;
                    }
                case JSObjectType.Bool:
                    {
                        if (typeof(BaseTypes.Boolean) != targetType && !typeof(BaseTypes.Boolean).IsSubclassOf(targetType))
                            return null;
                        if (boolean == null)
                            boolean = new BaseTypes.Boolean();
                        boolean.iValue = source.iValue;
                        return boolean;
                    }
            }
            return null;
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
            if (info.IsStatic)
                return null;
            if (_this == null)
                return JSObject.undefined;
            if (_this is EmbeddedType)
                return _this;
            object res = null;
            if (_this.ValueType >= JSObjectType.Object && _this.oValue is JSObject)
                _this = _this.oValue as JSObject;
            res = embeddedTypeConvert(_this, targetType) ?? _this.oValue;
            if (res is TypeProxy)
                res = (res as TypeProxy).prototypeInstance;
            return res;
        }

        [Modules.DoNotDelete]
        public override JSObject length
        {
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum };
                var pc = info.GetCustomAttributes(typeof(Modules.ParametersCountAttribute), false);
                if (pc.Length != 0)
                    _length.iValue = (pc[0] as Modules.ParametersCountAttribute).Count;
                else
                    _length.iValue = parameters.Length;
                return _length;
            }
        }

        [Modules.Hidden]
        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            try
            {
                context.ValidateThreadID();
                object res = null;
                if (@delegate != null)
                    res = @delegate(argumentsToArray(args));
                else
                {
                    var target = getTargetObject(thisOverride ?? context.thisBind, info.DeclaringType);
                    if (target != null && target.GetType() != info.ReflectedType) // you bunny wrote
                    {
                        var minfo = info as MethodInfo;
                        if (minfo.ReturnType.IsValueType)
                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid return type of method " + minfo)));
                        var prms = parameters;
                        if (prms.Length > 16)
                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid parameters count of method " + minfo)));
                        for (int i = 0; i < prms.Length; i++)
                            if (prms[i].ParameterType.IsValueType)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Invalid parameter (" + prms[i].Name + ") type of method " + minfo)));
                        var cargs = ConvertArgs(args);
                        Delegate del = null;
                        switch (prms.Length)
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
                            ConvertArgs(args),
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                if (converter != null)
                    res = converter.From(res);
                return TypeProxy.Proxy(res);
            }
            catch (Exception e)
            {
                var st = e.StackTrace;
                while (e.InnerException != null)
                    e = e.InnerException;
                if (e is JSException)
                    throw e;
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(e.Message)));
            }
        }

        [Modules.Hidden]
        public override JSObject Invoke(Context contextOverride, JSObject args)
        {
            var oldContext = context;
            context = contextOverride;
            try
            {
                return Invoke(null as JSObject, args);
            }
            finally
            {
                context = oldContext;
                context.ValidateThreadID();
            }
        }

        [Modules.Hidden]
        public override JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            var oldContext = context;
            if (contextOverride == null || oldContext == contextOverride)
                return Invoke(thisOverride, args);
            context = contextOverride;
            try
            {
                return Invoke(thisOverride, args);
            }
            finally
            {
                context = oldContext;
                context.ValidateThreadID();
            }
        }

        [Modules.Hidden]
        public override JSObject GetField(string name, bool fast, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, own);
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

        public override JSObject call(JSObject args)
        {
            var newThis = args.GetField("0", true, false);
            var prmlen = --args.GetField("length", true, false).iValue;
            for (int i = 0; i < prmlen; i++)
                args.fields[i.ToString()] = args.GetField((i + 1).ToString(), true, false);
            args.fields.Remove(prmlen.ToString());
            if (newThis.ValueType < JSObjectType.Object || newThis.oValue != null || (info.DeclaringType == typeof(JSObject)))
                return Invoke(newThis, args);
            else
                return Invoke(Context.thisBind ?? Context.GetField("this"), args);
        }
    }
}
