using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NiL.JS.Core
{
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> constructors = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> prototypes = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        internal object prototypeInstance;
        internal BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic;

        public static JSObject Proxy(object value)
        {
            if (value == null)
                return JSObject.Null;
            else if (value is JSObject)
                return value as JSObject;
            else if (value is sbyte)
                return (int)(sbyte)value;
            else if (value is byte)
                return (int)(byte)value;
            else if (value is short)
                return (int)(short)value;
            else if (value is ushort)
                return (int)(ushort)value;
            else if (value is int)
                return (int)value;
            else if (value is uint)
                return (double)(uint)value;
            else if (value is long)
                return (double)(long)value;
            else if (value is ulong)
                return (double)(ulong)value;
            else if (value is float)
                return (double)(float)value;
            else if (value is double)
                return (double)value;
            else if (value is string)
                return value.ToString();
            else if (value is char)
                return value.ToString();
            else if (value is bool)
                return (bool)value;
            else
            {
                var type = value.GetType();
                var res = new JSObject() { oValue = value, ValueType = JSObjectType.Object, prototype = GetPrototype(type) };
                res.attributes |= res.prototype.attributes & ObjectAttributes.Immutable;
                return res;
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            TypeProxy prot = null;
            if (!prototypes.TryGetValue(type, out prot))
            {
                lock (prototypes)
                {
                    new TypeProxy(type);
                    prot = prototypes[type];
                }
            }
            return prot;
        }

        public static JSObject GetConstructor(Type type)
        {
            JSObject constructor = null;
            if (!constructors.TryGetValue(type, out constructor))
            {
                lock (prototypes)
                {
                    new TypeProxy(type);
                    constructor = constructors[type];
                }
            }
            return constructor;
        }

        public static void Clear()
        {
            constructors.Clear();
            prototypes.Clear();
        }

        private TypeProxy()
            : base(true)
        {
            ValueType = JSObjectType.Object;
            oValue = this;
        }

        private TypeProxy(Type type)
            : base(true)
        {
            if (prototypes.ContainsKey(type))
                throw new InvalidOperationException("Type \"" + type + "\" already proxied.");
            else
            {
                hostedType = type;
                prototypes[type] = this;
                if (type.IsValueType)
                    prototypeInstance = Activator.CreateInstance(type);
                else
                {
                    if (hostedType == typeof(JSObject))
                    {
                        prototypeInstance = new JSObject()
                        {
                            ValueType = JSObjectType.Object,
                            oValue = this // Не убирать!
                        };
                    }
                    else
                    {
                        var ictor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                        if (ictor != null)
                        {
                            prototypeInstance = ictor.Invoke(null);
                            if (prototypeInstance is JSObject)
                            {
                                (prototypeInstance as JSObject).fields = this.fields;
                                if ((prototypeInstance as JSObject).ValueType < JSObjectType.Object)
                                    (prototypeInstance as JSObject).ValueType = JSObjectType.Object;
                            }
                        }
                    }
                }

                ValueType = prototypeInstance is JSObject ? (JSObjectType)System.Math.Max((int)(prototypeInstance as JSObject).ValueType, (int)JSObjectType.Object) : JSObjectType.Object;
                oValue = this;
                attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                if (hostedType.GetCustomAttributes(typeof(ImmutableAttribute), false).Length != 0)
                    attributes |= ObjectAttributes.Immutable;
                var ctorProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                if (prototypeInstance == null) // Static
                {
                    constructors[type] = ctorProxy;
                }
                else
                {
                    var prot = ctorProxy.DefaultFieldGetter("prototype", false, false);
                    prot.Assign(this);
                    prot.attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                    var ctor = new TypeProxyConstructor(ctorProxy);
                    ctorProxy.DefaultFieldGetter("__proto__", false, false).Assign(GetPrototype(typeof(TypeProxyConstructor)));
                    ctor.attributes = attributes;
                    constructors[type] = ctor;
                    fields["constructor"] = ctor;
                }
                bindFlags |= BindingFlags.Instance;
                var pa = type.GetCustomAttributes(typeof(PrototypeAttribute), false);
                if (pa.Length != 0)
                    prototype = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType).Clone() as JSObject;
                else
                    prototype = JSObject.GlobalPrototype ?? (typeof(JSObject) != type ? GetPrototype(typeof(JSObject)) : null);
            }
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            JSObject r = null;
            if (fields.TryGetValue(name, out r))
            {
                if (r.ValueType < JSObjectType.Undefined)
                    r.Assign(DefaultFieldGetter(name, fast, own));
                return r;
            }
            var m = hostedType.GetMember(name, bindFlags);
            if (m.Length > 1)
            {
                for (int i = 0; i < m.Length; i++)
                    if (!(m[i] is MethodInfo))
                        throw new JSException(Proxy(new TypeError("Incompatible fields type.")));
                var cache = new Function[m.Length];
                r = new CallableField((context, args) =>
                {
                    context.ValidateThreadID();
                    int l = args.GetField("length", true, false).iValue;
                    for (int i = 0; i < m.Length; i++)
                    {
                        var mi = m[i] as MethodInfo;
                        if (mi.GetParameters().Length == l && mi.GetCustomAttributes(typeof(HiddenAttribute), false).Length == 0)
                            return (cache[i] ?? (cache[i] = new MethodProxy(m[i] as MethodInfo))).Invoke(context, args);
                    }
                    return null;
                });
            }
            else
            {
                if (m.Length == 0 || name == "GetType" || m[0].GetCustomAttributes(typeof(HiddenAttribute), false).Length != 0)
                {
                    switch (name)
                    {
                        case "toString":
                            {
                                return GetField("ToString", true, true);
                            }
                        default:
                            {
                                r = DefaultFieldGetter(name, fast, own);
                                return r;
                            }
                    }
                }
                switch (m[0].MemberType)
                {
                    case MemberTypes.Constructor:
                        {
                            var method = (ConstructorInfo)m[0];
                            r = new MethodProxy(method);
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(method);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            if (field.IsStatic)
                            {
                                var val = field.GetValue(null);
                                if (val == null)
                                    r = JSObject.Null;
                                else if (val is JSObject)
                                    r = val as JSObject;
                                else
                                {
                                    r = Proxy(val);
                                    r.assignCallback = null;
                                }
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    ValueType = JSObjectType.Property,
                                    oValue = new Function[] {
                                    null, // Запись не поддерживается. Временно, надеюсь
                                    new ExternalFunction((c,a)=>{ return Proxy(field.GetValue(c.thisBind.oValue));})
                                }
                                };
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
                                    pinfo.CanWrite && pinfo.GetSetMethod(true).IsPublic ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                    pinfo.CanRead && pinfo.GetGetMethod(true).IsPublic ? new MethodProxy(pinfo.GetGetMethod(false)) : null 
                                }
                            };
                            break;
                        }
                    case MemberTypes.Event:
                        {
                            var pinfo = (EventInfo)m[0];
                            r = new JSObject()
                            {
                                ValueType = JSObjectType.Property,
                                oValue = new Function[] { 
                                    new MethodProxy(pinfo.GetAddMethod()),
                                    null
                                }
                            };
                            break;
                        }
                    default: throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
                if (m[0].GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0)
                    r.Protect();
                if (m[0].GetCustomAttributes(typeof(DoNotDeleteAttribute), false).Length != 0)
                    r.attributes |= ObjectAttributes.DontDelete;
            }
            r.attributes |= ObjectAttributes.DontEnum;
            fields[name] = r;
            return r;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            if (fields == null)
                return JSObject.EmptyEnumerator;
            return fields.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return ((bindFlags & BindingFlags.Static) != 0 ? "Proxy:Static (" : "Proxy:Dynamic (") + hostedType + ")";
        }
    }
}