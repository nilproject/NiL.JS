using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> constructors = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> prototypes = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        [NonSerialized]
        internal Dictionary<string, IList<MemberInfo>> members;
        private object _prototypeInstance;
        internal object prototypeInstance
        {
            get
            {
                if (_prototypeInstance == null)
                {
                    try
                    {
                        var ictor = hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
                        if (ictor != null)
                            _prototypeInstance = ictor.Invoke(null);
                    }
                    catch (COMException)
                    {

                    }
                }
                return _prototypeInstance;
            }
        }
        internal BindingFlags bindFlags = BindingFlags.Public;

        /// <summary>
        /// Создаёт объект-прослойку указанного объекта для доступа к этому объекту из скрипта. 
        /// </summary>
        /// <param name="value">Объект, который необходимо представить.</param>
        /// <returns>Объект-прослойка, представляющий переданный объект.</returns>
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
                res.attributes |= res.prototype.attributes & JSObjectAttributes.Immutable;
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

        internal static void Clear()
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
                if (!type.IsVisible)
                    bindFlags |= BindingFlags.NonPublic;
                hostedType = type;
                prototypes[type] = this;
                if (type.IsValueType)
                    _prototypeInstance = Activator.CreateInstance(type);
                else
                {
                    if (hostedType == typeof(JSObject))
                    {
                        _prototypeInstance = new JSObject()
                        {
                            ValueType = JSObjectType.Object,
                            oValue = this // Не убирать!
                        };
                    }
                    else
                    {
                        if (typeof(JSObject).IsAssignableFrom(hostedType))
                        {
                            var ictor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
                            if (ictor != null)
                            {
                                _prototypeInstance = ictor.Invoke(null);
                                (_prototypeInstance as JSObject).fields = this.fields;
                                if ((_prototypeInstance as JSObject).ValueType < JSObjectType.Object)
                                    (_prototypeInstance as JSObject).ValueType = JSObjectType.Object;
                            }
                        }
                    }
                }

                ValueType = prototypeInstance is JSObject ? (JSObjectType)System.Math.Max((int)(prototypeInstance as JSObject).ValueType, (int)JSObjectType.Object) : JSObjectType.Object;
                oValue = this;
                attributes |= JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum | JSObjectAttributes.ReadOnly;
                if (hostedType.IsDefined(typeof(ImmutableAttribute), false))
                    attributes |= JSObjectAttributes.Immutable;
                var ctorProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                if (hostedType.IsAbstract)
                {
                    constructors[type] = ctorProxy;
                }
                else
                {
                    var prot = ctorProxy.DefaultFieldGetter("prototype", false, false);
                    prot.Assign(this);
                    prot.attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum | JSObjectAttributes.ReadOnly;
                    var ctor = type == typeof(JSObject) ? new ObjectConstructor(ctorProxy) : new TypeProxyConstructor(ctorProxy);
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

        private void fillMembers()
        {
            lock (this)
            {
                lock (fields)
                {
                    members = new Dictionary<string, IList<MemberInfo>>();
                    var mmbrs = hostedType.GetMembers(bindFlags);
                    string prewName = null;
                    IList<MemberInfo> temp = null;
                    for (int i = 0; i < mmbrs.Length; i++)
                    {
                        if (mmbrs[i].IsDefined(typeof(HiddenAttribute), false))
                            continue;
                        var membername = mmbrs[i].Name;
                        if (membername.EndsWith("GetType"))
                            continue;
                        membername = membername[0] == '.' ? membername : membername.Contains(".") ? membername.Substring(membername.LastIndexOf('.') + 1) : membername;
                        if (prewName != membername && !members.TryGetValue(membername, out temp))
                        {
                            members[membername] = temp = new List<MemberInfo>() { mmbrs[i] };
                            prewName = membername;
                        }
                        else
                        {
                            if (temp.Count == 1)
                                members.Add(membername + "$0", new[] { temp[0] });
                            temp.Add(mmbrs[i]);
                            if (temp.Count != 1)
                                members.Add(membername + "$" + (temp.Count - 1), new[] { mmbrs[i] });
                        }
                    }
                }
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
            IList<MemberInfo> m = null;
            if (members == null)
                fillMembers();
            members.TryGetValue(name, out m);
            if (m == null || m.Count == 0)
            {
                switch (name)
                {
                    default:
                        {
                            r = DefaultFieldGetter(name, fast, own);
                            return r;
                        }
                }
            }
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodBase))
                        throw new JSException(Proxy(new TypeError("Incompatible fields type.")));
                var cache = new MethodProxy[m.Count];
                for (int i = 0; i < m.Count; i++)
                    cache[i] = new MethodProxy(m[i] as MethodBase);
                r = new ExternalFunction((context, args) =>
                {
                    int l = args.GetField("length", true, false).iValue;
                    for (int i = 0; i < m.Count; i++)
                    {
                        if (cache[i].Parameters.Length == l
                        || (cache[i].Parameters.Length == 1
                            && (cache[i].Parameters[0].ParameterType == typeof(JSObject)
                                || cache[i].Parameters[0].ParameterType == typeof(JSObject[]))))
                        {
                            object[] cargs = null;
                            if (l != 0)
                            {
                                cargs = cache[i].ConvertArgs(args);
                                for (var j = cargs.Length; j-- > 0; )
                                {
                                    if (cargs[j] == null ? cache[i].Parameters[j].ParameterType.IsValueType : !cache[i].Parameters[j].ParameterType.IsAssignableFrom(cargs[j].GetType()))
                                    {
                                        j = 0;
                                        cargs = null;
                                    }
                                }
                                if (cargs == null)
                                    continue;
                            }
                            if (cargs.Length == 1 && cargs[0] is JSObject && (cargs[0] as JSObject).oValue == Arguments.Instance)
                                (cargs[0] as JSObject).fields["callee"] = cache[i];
                            return TypeProxy.Proxy(cache[i].InvokeRaw(context, null, cargs));
                        }
                    }
                    return null;
                });
            }
            else
            {
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
                            var cva = field.GetCustomAttribute(typeof(ConvertValueAttribute)) as ConvertValueAttribute;
                            if (cva != null)
                            {
                                r = new JSObject()
                                {
                                    ValueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                    {
                                        m[0].IsDefined(typeof(Modules.ProtectedAttribute), false) ? 
                                            new ExternalFunction((c,a)=>{ field.SetValue(field.IsStatic ? null : (c.thisBind ?? c.GetField("this")).oValue, cva.To(a.GetField("0", true, false).Value)); return null; }) : null,
                                        new ExternalFunction((c,a)=>{ return Proxy(cva.From(field.GetValue(field.IsStatic ? null : c.thisBind.oValue)));})
                                    }
                                };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    ValueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                    {
                                        !m[0].IsDefined(typeof(Modules.ProtectedAttribute), false) ? new ExternalFunction((c,a)=>{ field.SetValue(field.IsStatic ? null : (c.thisBind ?? c.GetField("this")).oValue, a.GetField("0", true, false).Value); return null; }) : null,
                                        new ExternalFunction((c,a)=>{ return Proxy(field.GetValue(field.IsStatic ? null : c.thisBind.oValue));})
                                    }
                                };
                            }
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var pinfo = (PropertyInfo)m[0];
                            var cva = pinfo.GetCustomAttribute(typeof(ConvertValueAttribute)) as ConvertValueAttribute;
                            if (cva != null)
                            {
                                r = new JSObject()
                                    {
                                        ValueType = JSObjectType.Property,
                                        oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null ? new MethodProxy(pinfo.GetSetMethod(false), cva, new[]{ cva }) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false), cva, null) : null 
                                        }
                                    };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    ValueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null 
                                        }
                                };
                            }
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
                if (m[0].IsDefined(typeof(ProtectedAttribute), false))
                    r.Protect();
                if (m[0].IsDefined(typeof(DoNotDeleteAttribute), false))
                    r.attributes |= JSObjectAttributes.DontDelete;
            }
            r.attributes |= JSObjectAttributes.DontEnum;
            lock (fields)
                fields[name] = r;
            for (var i = m.Count; i-- > 0; )
            {
                if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute)))
                {
                    r.attributes &= ~JSObjectAttributes.DontEnum;
                    break;
                }
            }
            return r;
        }

        public override JSObject propertyIsEnumerable(JSObject args)
        {
            var name = args.GetField("0", true, false).ToString();
            JSObject temp;
            if (fields != null && fields.TryGetValue(name, out temp))
                return temp.ValueType >= JSObjectType.Undefined && (temp.attributes & JSObjectAttributes.DontEnum) == 0;
            IList<MemberInfo> m = null;
            if (members.TryGetValue(name, out m))
            {
                for (var i = m.Count; i-- > 0; )
                    if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                        return false;
                return true;
            }
            return false;
        }

        [Hidden]
        public override IEnumerator<string> GetEnumerator()
        {
            if (members == null)
                fillMembers();
            foreach (var m in members)
            {
                for (var i = m.Value.Count; i-- > 0; )
                {
                    if (!m.Value[i].IsDefined(typeof(DoNotEnumerateAttribute)))
                    {
                        yield return m.Key;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return ((bindFlags & BindingFlags.Static) != 0 ? "Proxy:Static (" : "Proxy:Dynamic (") + hostedType + ")";
        }
    }
}