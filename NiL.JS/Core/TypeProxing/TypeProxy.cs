using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> staticProxies = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> dynamicProxies = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        [NonSerialized]
        internal Dictionary<string, IList<MemberInfo>> members;
        private JSObject _prototypeInstance;
        internal JSObject prototypeInstance
        {
            get
            {
                if (_prototypeInstance == null && (bindFlags & BindingFlags.Instance) != 0 && !hostedType.IsAbstract)
                {
                    try
                    {
                        var ictor = hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
                        if (ictor != null)
                        {
                            _prototypeInstance = new JSObject()
                                                    {
                                                        oValue = ictor.Invoke(null),
                                                        __proto__ = this,
                                                        valueType = JSObjectType.Object,
                                                        attributes = attributes | JSObjectAttributesInternal.ProxyPrototype,
                                                        fields = fields
                                                    };
                            if (_prototypeInstance.oValue is JSObject)
                            {
                                _prototypeInstance.valueType = (JSObjectType)System.Math.Max((int)_prototypeInstance.valueType, (int)(_prototypeInstance.oValue as JSObject).valueType);
                                (_prototypeInstance.oValue as JSObject).fields = fields;
                                (_prototypeInstance.oValue as JSObject).attributes |= JSObjectAttributesInternal.ProxyPrototype;
                            }
                        }
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
                return JSObject.undefined;
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
            else if (value is Delegate)
                return new MethodProxy(((Delegate)value).Method, ((Delegate)value).Target);
            else
            {
                var type = value.GetType();
                var res = new JSObject() { oValue = value, valueType = JSObjectType.Object, __proto__ = GetPrototype(type) };
                res.attributes |= res.__proto__.attributes & JSObjectAttributesInternal.Immutable;
                return res;
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            TypeProxy prot = null;
            if (!dynamicProxies.TryGetValue(type, out prot))
            {
                lock (dynamicProxies)
                {
                    new TypeProxy(type);
                    prot = dynamicProxies[type];
                }
            }
            return prot;
        }

        public static JSObject GetConstructor(Type type)
        {
            JSObject constructor = null;
            if (!staticProxies.TryGetValue(type, out constructor))
            {
                lock (staticProxies)
                {
                    new TypeProxy(type);
                    constructor = staticProxies[type];
                }
            }
            return constructor;
        }

        internal static void Clear()
        {
            BaseTypes.Boolean.True.__proto__ = null;
            BaseTypes.Boolean.False.__proto__ = null;
            JSObject.nullString.__proto__ = null;
            Number.NaN.__proto__ = null;
            Number.POSITIVE_INFINITY.__proto__ = null;
            Number.NEGATIVE_INFINITY.__proto__ = null;
            Number.MIN_VALUE.__proto__ = null;
            Number.MAX_VALUE.__proto__ = null;
            staticProxies.Clear();
            dynamicProxies.Clear();
        }

        private TypeProxy()
            : base(true)
        {
            valueType = JSObjectType.Object;
            oValue = this;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        private TypeProxy(Type type)
            : base(true)
        {
            if (dynamicProxies.ContainsKey(type))
                throw new InvalidOperationException("Type \"" + type + "\" already proxied.");
            else
            {
                hostedType = type;
                dynamicProxies[type] = this;
                if (hostedType == typeof(JSObject))
                {
                    _prototypeInstance = new JSObject()
                    {
                        valueType = JSObjectType.Object,
                        oValue = this, // Не убирать!
                        attributes = JSObjectAttributesInternal.ProxyPrototype | JSObjectAttributesInternal.ReadOnly,
                        __proto__ = this
                    };
                }
                else
                {
                    if (typeof(JSObject).IsAssignableFrom(hostedType))
                    {
                        _prototypeInstance = prototypeInstance;
                    }
                }

                valueType = _prototypeInstance is JSObject ? (JSObjectType)System.Math.Max((int)(_prototypeInstance as JSObject).valueType, (int)JSObjectType.Object) : JSObjectType.Object;
                oValue = this;
                attributes |= JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
                if (hostedType.IsDefined(typeof(ImmutableAttribute), false))
                    attributes |= JSObjectAttributesInternal.Immutable;
                var staticProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                bindFlags |= BindingFlags.Instance;
                if (hostedType.IsAbstract)
                {
                    staticProxies[type] = staticProxy;
                }
                else
                {
                    var prot = staticProxy.fields["prototype"] = this;
                    Function ctor = null;
                    if (type == typeof(JSObject))
                        ctor = new ObjectConstructor(staticProxy);
                    else
                        ctor = new ProxyConstructor(staticProxy);
                    ctor.attributes = attributes;
                    attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly;
                    staticProxies[type] = ctor;
                    fields["constructor"] = ctor;
                }
                var pa = type.GetCustomAttributes(typeof(PrototypeAttribute), false);
                if (pa.Length != 0)
                    __proto__ = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType).CloneImpl();
            }
        }

        private void fillMembers()
        {
            lock (this)
            {
                lock (fields)
                {
                    if (members != null)
                        return;
                    var tempmemb = new Dictionary<string, IList<MemberInfo>>();
                    var mmbrs = hostedType.GetMembers(bindFlags);
                    string prewName = null;
                    IList<MemberInfo> temp = null;
                    for (int i = 0; i < mmbrs.Length; i++)
                    {
                        mmbrs[i].ToString();
                        if (mmbrs[i].IsDefined(typeof(HiddenAttribute), false))
                            continue;
                        if (mmbrs[i].MemberType == MemberTypes.Method
                            && ((mmbrs[i] as MethodBase).DeclaringType == typeof(object)))
                            continue;
                        if (mmbrs[i].MemberType == MemberTypes.Constructor)
                            continue;
                        var membername = mmbrs[i].Name;
                        membername = membername[0] == '.' ? membername : membername.Contains(".") ? membername.Substring(membername.LastIndexOf('.') + 1) : membername;
                        if (prewName != membername)
                        {
                            if (temp != null && temp.Count > 1)
                            {
                                var type = temp[0].DeclaringType;
                                for (var j = 1; j < temp.Count; j++)
                                {
                                    if (type != temp[j].DeclaringType && type.IsAssignableFrom(temp[j].DeclaringType))
                                    {
                                        type = temp[j].DeclaringType;
                                        j = 0;
                                        continue;
                                    }
                                    if (!type.IsAssignableFrom(temp[j].DeclaringType))
                                        temp.RemoveAt(j--);
                                }
                            }
                            if (!tempmemb.TryGetValue(membername, out temp))
                                tempmemb[membername] = temp = new List<MemberInfo>();
                            prewName = membername;
                        }
                        if (temp.Count == 1)
                            tempmemb.Add(membername + "$0", new[] { temp[0] });
                        temp.Add(mmbrs[i]);
                        if (temp.Count != 1)
                            tempmemb.Add(membername + "$" + (temp.Count - 1), new[] { mmbrs[i] });
                    }
                    members = tempmemb;
                }
            }
        }

        public override void Assign(NiL.JS.Core.JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                throw new JSException("Can not assign to __proto__ of immutable or special objects.");
        }

        internal protected override JSObject GetMember(JSObject nameObj, bool create, bool own)
        {
            string name = nameObj.ToString();
            JSObject r = null;
            if (fields.TryGetValue(name, out r))
            {
                if (r.valueType < JSObjectType.Undefined)
                {
                    if (!create)
                    {
                        var t = DefaultFieldGetter(nameObj, false, own);
                        if (t.isExist)
                            r.Assign(t);
                    }
                }
                if (create
                    && (r.attributes & JSObjectAttributesInternal.SystemObject) != 0
                    && (r.attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                    fields[name] = r = r.CloneImpl();
                return r;
            }
            IList<MemberInfo> m = null;
            if (members == null)
                fillMembers();
            members.TryGetValue(name, out m);
            if (m == null || m.Count == 0)
            {
                r = DefaultFieldGetter(nameObj, create, own);
                return r;
            }
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodBase))
                        throw new JSException(Proxy(new TypeError("Incompatible fields type.")));
                var cache = new MethodProxy[m.Count];
                for (int i = 0; i < m.Count; i++)
                    cache[i] = new MethodProxy(m[i] as MethodBase);
                r = new ExternalFunction((thisBind, args) =>
                {
                    int l = args == null ? 0 : args.length;
                    for (int i = 0; i < m.Count; i++)
                    {
                        if (cache[i].Parameters.Length == l
                        || (cache[i].Parameters.Length == 1
                            && (cache[i].Parameters[0].ParameterType == typeof(Arguments)
                            //|| cache[i].Parameters[0].ParameterType == typeof(JSObject[])
                                || cache[i].Parameters[0].ParameterType == typeof(object[]))))
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
                            return TypeProxy.Proxy(cache[i].InvokeImpl(thisBind, cargs, args));
                        }
                    }
                    throw new JSException(new TypeError("Invalid parameters for function " + m[0].Name));
                });
            }
            else
            {
                switch (m[0].MemberType)
                {
                    case MemberTypes.Constructor:
                        throw new InvalidOperationException("Constructor can not be called directly");
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(method);
                            r.attributes = JSObjectAttributesInternal.SystemObject;
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
                                    valueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                    {
                                        m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ? 
                                            new ExternalFunction((thisBind, a)=>
                                            {
                                                field.SetValue(field.IsStatic ? null : thisBind.Value, cva.To(a[0].Value)); 
                                                return null; 
                                            }) : null,
                                        new ExternalFunction((thisBind, a)=>
                                        { 
                                            return Proxy(cva.From(field.GetValue(field.IsStatic ? null : thisBind.Value)));
                                        })
                                    }
                                };
                                r.attributes = JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.Field;
                                if ((r.oValue as Function[])[0] == null)
                                    r.attributes |= JSObjectAttributesInternal.ReadOnly;
                            }
                            else
                            {
                                if ((field.Attributes & (FieldAttributes.Literal | FieldAttributes.InitOnly)) != 0
                                    && (field.Attributes & FieldAttributes.Static) != 0)
                                {
                                    r = Proxy(field.GetValue(null));
                                    r.attributes |= JSObjectAttributesInternal.ReadOnly;
                                }
                                else
                                {
                                    r = new JSObject()
                                    {
                                        valueType = JSObjectType.Property,
                                        oValue = new Function[] 
                                        {
                                            !m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ? new ExternalFunction((thisBind, a)=>
                                            {
                                                field.SetValue(field.IsStatic ? null : thisBind.Value, a[0].Value); 
                                                return null; 
                                            }) : null,
                                            new ExternalFunction((thisBind, a)=>
                                            { 
                                                return Proxy(field.GetValue(field.IsStatic ? null : thisBind.Value));
                                            })
                                        }
                                    };
                                    r.attributes = JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.Field;
                                    if ((r.oValue as Function[])[0] == null)
                                        r.attributes |= JSObjectAttributesInternal.ReadOnly;
                                }
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
                                        valueType = JSObjectType.Property,
                                        oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false), cva, new[]{ cva }) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false), cva, null) : null 
                                        }
                                    };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    valueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null 
                                        }
                                };
                            }
                            r.attributes = JSObjectAttributesInternal.Immutable;
                            if ((r.oValue as Function[])[0] == null)
                                r.attributes |= JSObjectAttributesInternal.ReadOnly;
                            if (pinfo.IsDefined(typeof(FieldAttribute), false))
                                r.attributes |= JSObjectAttributesInternal.Field;
                            break;
                        }
                    case MemberTypes.Event:
                        {
                            var pinfo = (EventInfo)m[0];
                            r = new JSObject()
                            {
                                valueType = JSObjectType.Property,
                                oValue = new Function[] { 
                                    new MethodProxy(pinfo.GetAddMethod()),
                                    null
                                }
                            };
                            break;
                        }
                    case MemberTypes.NestedType:
                        {
                            r = GetConstructor(m[0] as Type);
                            break;
                        }
                    default:
                        throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
            }
            r.attributes |= JSObjectAttributesInternal.DoNotEnum;
            lock (fields)
                fields[name] = create && r.GetType() != typeof(JSObject) ? (r = r.CloneImpl()) : r;
            if (m[0].IsDefined(typeof(ReadOnlyAttribute), false))
                r.attributes |= JSObjectAttributesInternal.ReadOnly;
            if (m[0].IsDefined(typeof(NotConfigurable), false))
                r.attributes |= JSObjectAttributesInternal.NotConfigurable;
            if (m[0].IsDefined(typeof(DoNotDeleteAttribute), false))
                r.attributes |= JSObjectAttributesInternal.DoNotDelete;
            for (var i = m.Count; i-- > 0; )
            {
                if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                {
                    r.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    break;
                }
            }
            return r;
        }

        public override JSObject propertyIsEnumerable(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var name = args[0].ToString();
            JSObject temp;
            if (fields != null && fields.TryGetValue(name, out temp))
                return temp.isExist && (temp.attributes & JSObjectAttributesInternal.DoNotEnum) == 0;
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

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            if (members == null)
                fillMembers();
            foreach (var f in fields)
            {
                if (f.Value.isExist && (!pdef || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return f.Key;
            }
            foreach (var m in members)
            {
                if (fields.ContainsKey(m.Key))
                    continue;
                for (var i = m.Value.Count; i-- > 0; )
                {
                    if (!pdef || !m.Value[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                    {
                        yield return m.Key;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            if (hostedType.IsAbstract)
                return "[object " + hostedType.Name + "]";
            return ((bindFlags & BindingFlags.Static) != 0 ? "Proxy:Static (" : "Proxy:Dynamic (") + hostedType + ")";
        }
    }
}