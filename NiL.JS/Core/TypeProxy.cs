using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NiL.JS.Core
{
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, TypeProxy> constructors = new Dictionary<Type, TypeProxy>();
        private static readonly Dictionary<Type, TypeProxy> prototypes = new Dictionary<Type, TypeProxy>();
        private static readonly NiL.JS.Core.BaseTypes.String @string = new BaseTypes.String();
        private static readonly NiL.JS.Core.BaseTypes.Number number = new Number();
        private static readonly NiL.JS.Core.BaseTypes.Boolean boolean = new BaseTypes.Boolean();

        private static JSObject embeddedTypeConvert(JSObject source, Type targetType)
        {
            if (source.GetType() == targetType)
                return source;
            if (targetType == typeof(Number))
            {
                if (source.ValueType != JSObjectType.Int && source.ValueType != JSObjectType.Double)
                    return null;
                number.iValue = source.iValue;
                number.dValue = source.dValue;
                number.ValueType = source.ValueType;
                return number;
            }
            else if (targetType == typeof(NiL.JS.Core.BaseTypes.String))
            {
                if (source.ValueType != JSObjectType.String)
                    return null;
                @string.oValue = source.oValue;
                return @string;
            }
            else if (targetType == typeof(NiL.JS.Core.BaseTypes.Boolean))
            {
                if (source.ValueType != JSObjectType.Bool)
                    return null;
                boolean.iValue = source.iValue;
                return boolean;
            }
            return null;
        }

        private static object[] convertArgs(JSObject source, ParameterInfo[] targetTypes)
        {
            if (targetTypes.Length == 0)
                return null;
            object[] res;
            if (targetTypes.Length == 1)
            {
                if (targetTypes[0].ParameterType == typeof(JSObject))
                    return new object[] { source };
                if (targetTypes[0].ParameterType == typeof(JSObject[]))
                {
                    var len = source.GetField("length", true, false).iValue;
                    res = new JSObject[len];
                    for (int i = 0; i < len; i++)
                        res[i] = source.GetField(i.ToString(), true, true);
                    return new object[] { res };
                }
            }
            int targetCount = targetTypes.Length;
            if (source != null)
                targetCount = System.Math.Min(targetCount, source.GetField("length", true, false).iValue);
            res = new object[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                if (source == null || source.GetField(i.ToString(), true, true) == null)
                    continue;
                var obj = source.GetField(i.ToString(), true, true);
                if (targetTypes[i].ParameterType == typeof(JSObject))
                    res[i] = obj;
                else
                {
                    var v = obj.Value;
                    if (v is Core.BaseTypes.Array)
                    {
                        var arr = v as Core.BaseTypes.Array;
                        var arg = new object[arr.length];
                        for (var j = 0; j < arg.Length; j++)
                            arg[j] = arr[j].Value;
                        res[i] = arg;
                    }
                    else
                    {
                        if (targetTypes[i].ParameterType.IsAssignableFrom(v.GetType()))
                            res[i] = v;
                    }
                }
            }
            return res;
        }

        public static JSObject Proxy(object obj)
        {
            if (obj == null)
                return JSObject.Null;
            else if (obj is JSObject)
                return obj as JSObject;
            else if (obj is int)
                return (int)obj;
            else if (obj is double)
                return (double)obj;
            else if (obj is string)
                return (string)obj;
            else if (obj is bool)
                return (bool)obj;
            else
            {
                var type = obj.GetType();
                var res = new JSObject(false) { oValue = obj, ValueType = JSObjectType.Object };
                res.GetField("constructor", false, true).Assign(GetConstructor(type));
                res.GetField("__proto__", false, true).Assign(GetPrototype(type));
                return res;
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            TypeProxy prot = null;
            if (!prototypes.TryGetValue(type, out prot))
            {
                new TypeProxy(type);
                prot = prototypes[type];
            }
            return prot;
        }

        public static TypeProxy GetConstructor(Type type)
        {
            TypeProxy constructor = null;
            if (!constructors.TryGetValue(type, out constructor))
                constructor = new TypeProxy(type);
            return constructor;
        }

        private Type hostedType;

        internal TypeProxy(Type type, bool fictive)
            : base(true)
        {
            hostedType = type;
            oValue = type;
            ValueType = JSObjectType.Proxy;
            assignCallback = JSObject.ProtectAssignCallback;
            fields["constructor"] = GetConstructor(hostedType);
            prototype = BaseObject.Prototype;
            attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
        }

        public TypeProxy(Type type)
        {
            ValueType = JSObjectType.Function;
            hostedType = type;
            JSObject proto = null;
            TypeProxy exconst = null;
            assignCallback = JSObject.ProtectAssignCallback;
            prototype = BaseObject.Prototype;
            attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
            if (constructors.TryGetValue(type, out exconst))
            {
                oValue = exconst.oValue;
                proto = DefaultFieldGetter("prototype", false, false);
                proto.Assign(prototypes[type]);
                proto.attributes |= ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
            }
            else
            {
                oValue = new ExternalFunction((x, y) =>
                {
                    object[] args = null;
                    ConstructorInfo constructor = null;
                    var len = y.GetField("length", false, false).iValue;
                    if (y == null || len == 0)
                        constructor = hostedType.GetConstructor(Type.EmptyTypes);
                    else
                    {
                        Type[] argtypes = null;
                        argtypes = new[] { typeof(JSObject) };
                        if (len == 1)
                        {
                            constructor = hostedType.GetConstructor(argtypes);
                            if (constructor != null)
                                args = new object[] { y };
                        }

                        argtypes[0] = typeof(object[]);
                        constructor = hostedType.GetConstructor(argtypes);
                        if (constructor != null)
                        {
                            args = new object[len];
                            for (int i = 0; i < len; i++)
                                args[i] = y.GetField(i.ToString(), true, false);
                            args = new[] { args };
                        }
                        else
                        {
                            argtypes[0] = typeof(JSObject[]);
                            constructor = hostedType.GetConstructor(argtypes);
                            if (constructor != null)
                            {
                                args = new JSObject[len];
                                for (int i = 0; i < len; i++)
                                    args[i] = y.GetField(i.ToString(), true, false);
                                args = new[] { args };
                            }
                        }
                        if (constructor == null)
                        {
                            argtypes = new Type[len];
                            for (int i = 0; i < len; i++)
                                argtypes[i] = typeof(JSObject);
                            constructor = hostedType.GetConstructor(argtypes);
                            if (constructor != null)
                            {
                                args = new object[len];
                                for (int i = 0; i < len; i++)
                                    args[i] = y.GetField(i.ToString(), true, false);
                            }
                            else
                            {
                                for (int i = 0; i < len; i++)
                                    argtypes[i] = y.GetField(i.ToString(), true, false).Value.GetType();
                                constructor = hostedType.GetConstructor(argtypes);
                                if (constructor == null)
                                {
                                    for (int i = 0; i < len; i++)
                                        argtypes[i] = typeof(object);
                                    constructor = hostedType.GetConstructor(argtypes);
                                }
                                if (constructor != null)
                                {
                                    args = new object[len];
                                    for (int i = 0; i < len; i++)
                                        args[i] = y.GetField(i.ToString(), true, false).Value;
                                }
                                else
                                    constructor = hostedType.GetConstructor(Type.EmptyTypes);
                            }
                        }
                    }
                    var _this = x.thisBind;
                    JSObject thproto = null;
                    bool bynew = false;
                    if (_this != null)
                    {
                        thproto = _this.prototype;
                        bynew = thproto != null && thproto.ValueType == JSObjectType.Proxy && thproto.oValue == type as object;
                    }
                    var obj = constructor.Invoke(args);
                    JSObject res = null;
                    if (bynew)
                    {
                        _this.oValue = obj;
                        res = _this;
                    }
                    else
                    {
                        res = obj is JSObject ? obj as JSObject : new JSObject(false)
                        {
                            oValue = obj,
                            ValueType = JSObjectType.Object,
                            prototype = proto
                        };
                    }
                    return res;
                });
                constructors[type] = this;
                proto = new TypeProxy(type, true);
                (proto as TypeProxy).DefaultFieldGetter("toString", false, true).Assign(toStringObj);
                prototypes[type] = proto as TypeProxy;
                proto = DefaultFieldGetter("prototype", false, false);
                proto.Assign(prototypes[type]);
                proto.attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
            }
        }

        private JSObject ProxyMethod(MethodInfo method)
        {
            JSObject result = null;
            if (method.IsStatic)
            {
                if (method.ReturnType == typeof(JSObject))
                {
                    if (method.GetParameters().Length == 1)
                    {
                        if (method.GetParameters()[0].ParameterType == typeof(JSObject))
                        {
                            var dinv = (Func<JSObject, JSObject>)Delegate.CreateDelegate(typeof(Func<JSObject, JSObject>), null, method);
                            result = new CallableField((th, args) =>
                            {
                                return dinv(args);
                            });
                            return result;
                        }
                        if (method.GetParameters()[0].ParameterType == typeof(JSObject[]))
                        {
                            var dinv = (Func<JSObject[], JSObject>)Delegate.CreateDelegate(typeof(Func<JSObject[], JSObject>), null, method);
                            JSObject[] cargs = null;
                            result = new CallableField((th, args) =>
                            {
                                var len = args.GetField("length", true, false).iValue;
                                if (cargs == null || cargs.Length != len)
                                    cargs = new JSObject[len];
                                for (int i = 0; i < len; i++)
                                    cargs[i] = args.GetField(i.ToString(), true, true);
                                return dinv(cargs);
                            });
                            return result;
                        }
                    }
                    else if (method.GetParameters().Length == 0)
                    {
                        var dinv = (Func<JSObject>)Delegate.CreateDelegate(typeof(Func<JSObject>), null, method);
                        result = new CallableField((th, args) =>
                        {                            
                            return dinv();
                        });
                        return result;
                    }
                }
            }
            var cb = new CallableField((context, args) =>
            {
                try
                {
                    var res = method.Invoke(getTargetObject(context, method.DeclaringType), convertArgs(args, method.GetParameters()));
                    if (method.ReturnType == typeof(void))
                        return JSObject.undefined;
                    return Proxy(res);
                }
                catch (Exception e)
                {
                    throw e.InnerException ?? e;
                }
            });
            result = cb;
            return result;
        }

        private object getTargetObject(Context context, Type targetType)
        {
            if (ValueType == JSObjectType.Function)
                return null;
            object obj = context.thisBind;
            if (obj is EmbeddedType)
                return obj;
            obj = embeddedTypeConvert(obj as JSObject, targetType) ?? context.thisBind.oValue;
            return obj;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            JSObject r = null;
            if (fields.TryGetValue(name, out r) && r.ValueType > JSObjectType.NotExistInObject)
                return r;
            var m = hostedType.GetMember(name, BindingFlags.Public | (ValueType == JSObjectType.Function ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (m.Length > 1)
                throw new InvalidOperationException("Too many fields with name " + name);
            if (m.Length == 0 || m[0].GetCustomAttributes(typeof(HiddenAttribute), true).Length != 0)
            {
                r = DefaultFieldGetter(name, fast, own);
                return r;
            }
            switch (m[0].MemberType)
            {
                case MemberTypes.Constructor:
                    {
                        var method = (ConstructorInfo)m[0];
                        r = new CallableField((th, args) =>
                        {
                            var res = method.Invoke(convertArgs(args, method.GetParameters()));
                            if (res is JSObject)
                                return res as JSObject;
                            else if (res is int)
                                return (int)res;
                            else if (res is double || res is long)
                                return (double)res;
                            else if (res is string)
                                return (string)res;
                            else if (res is bool)
                                return (bool)res;
                            else
                                return TypeProxy.Proxy(res);
                        });
                        break;
                    }
                case MemberTypes.Method:
                    {
                        var method = (MethodInfo)m[0];
                        r = ProxyMethod(method);
                        break;
                    }
                case MemberTypes.Field:
                    {
                        var field = (m[0] as FieldInfo);
                        if (!field.IsStatic)
                            throw new NotSupportedException("Fields for instances not supported. Use properties.");
                        object res = field.GetValue(null);
                        if (res is JSObject)
                            r = res as JSObject;
                        else
                        {
                            if (res is int)
                                r = (int)res;
                            else if (res is double || res is long)
                                r = (double)res;
                            else if (res is string)
                                r = (string)res;
                            else if (res is bool)
                                r = (bool)res;
                            else
                                r = TypeProxy.Proxy(res);
                            r.assignCallback = null;
                        }
                        break;
                    }
                case MemberTypes.Property:
                    {
                        var pinfo = (PropertyInfo)m[0];
                        r = new JSObject()
                        {
                            ValueType = JSObjectType.Property,
                            oValue = new Function[] { 
                                    pinfo.CanWrite ? ProxyMethod(pinfo.GetSetMethod(true)).oValue as Function : null,
                                    pinfo.CanRead ? ProxyMethod(pinfo.GetGetMethod(true)).oValue as Function : null 
                                }
                        };
                        break;
                    }
                default: throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
            }
            if (m[0].GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0)
                r.Protect();
            r.attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
            fields[name] = r;
            return r;
        }

        private static JSObject toStringObj = new CallableField(toString);

        private static JSObject toString(Context context, JSObject args)
        {
            return context.thisBind.oValue.ToString();
        }
    }
}