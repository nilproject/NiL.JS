using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.TypeProxing
{
    [Serializable]
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> staticProxies = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> dynamicProxies = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        [NonSerialized]
        internal Dictionary<string, IList<MemberInfo>> members;
        private ConstructorInfo ictor;
        private JSObject _prototypeInstance;
        internal JSObject prototypeInstance
        {
            get
            {
                if (_prototypeInstance == null && (bindFlags & BindingFlags.Instance) != 0 && !hostedType.IsAbstract)
                {
                    try
                    {
                        if (ictor != null)
                        {
                            if (hostedType == typeof(JSObject))
                            {
                                _prototypeInstance = CreateObject();
                                (_prototypeInstance as JSObject).__prototype = Null;
                                (_prototypeInstance as JSObject).fields = fields;
                                (_prototypeInstance as JSObject).attributes |= JSObjectAttributesInternal.ProxyPrototype;
                            }
                            else if (typeof(JSObject).IsAssignableFrom(hostedType))
                            {
                                _prototypeInstance = ictor.Invoke(null) as JSObject;
                                _prototypeInstance.__prototype = __proto__;
                                _prototypeInstance.attributes |= JSObjectAttributesInternal.ProxyPrototype;
                                _prototypeInstance.fields = fields;
                                _prototypeInstance.valueType = (JSObjectType)System.Math.Max((int)JSObjectType.Object, (int)_prototypeInstance.valueType);
                                valueType = (JSObjectType)System.Math.Max((int)JSObjectType.Object, (int)_prototypeInstance.valueType);
                            }
                            else
                            {
                                _prototypeInstance = new JSObject()
                                {
                                    oValue = ictor.Invoke(null),
                                    valueType = JSObjectType.Object,
                                    attributes = attributes | JSObjectAttributesInternal.ProxyPrototype,
                                    fields = fields
                                };
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
                return new Number((long)(uint)value);
            else if (value is long)
                return new Number((long)value);
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
            return GetPrototype(type, true);
        }

        internal static TypeProxy GetPrototype(Type type, bool create)
        {
            TypeProxy prot = null;
            if (!dynamicProxies.TryGetValue(type, out prot))
            {
                if (!create)
                    return null;
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
            BaseTypes.Boolean.True.__prototype = null;
            BaseTypes.Boolean.False.__prototype = null;
            JSObject.nullString.__prototype = null;
            Number.NaN.__prototype = null;
            Number.POSITIVE_INFINITY.__prototype = null;
            Number.NEGATIVE_INFINITY.__prototype = null;
            Number.MIN_VALUE.__prototype = null;
            Number.MAX_VALUE.__prototype = null;
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
                valueType = JSObjectType.Object;
                oValue = this;
                var pa = type.GetCustomAttributes(typeof(PrototypeAttribute), false);
                if (pa.Length != 0 && (pa[0] as PrototypeAttribute).PrototypeType != hostedType)
                    __prototype = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType);
                ictor = hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);

                attributes |= JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
                if (hostedType.IsDefined(typeof(ImmutablePrototypeAttribute), false))
                    attributes |= JSObjectAttributesInternal.Immutable;
                var staticProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                bindFlags |= BindingFlags.Instance;

                if (typeof(JSObject).IsAssignableFrom(hostedType))
                    _prototypeInstance = prototypeInstance;

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
                    if (hostedType != typeof(ProxyConstructor))
                        fields["constructor"] = ctor;
                }
            }
        }

        protected override JSObject getDefaultPrototype()
        {
            return GlobalPrototype;
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
                                        type = temp[j].DeclaringType;
                                }
                                int offset = 0;
                                for (var j = 1; j < temp.Count; j++)
                                {
                                    if (!type.IsAssignableFrom(temp[j].DeclaringType))
                                    {
                                        temp.RemoveAt(j--);
                                        tempmemb.Remove(prewName + "$" + (++offset + j));
                                    }
                                }
                                if (temp.Count == 1)
                                    tempmemb.Remove(prewName + "$0");
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
                        if (t.IsExist)
                            r.Assign(t);
                    }
                }
                if (create
                    && ((attributes & JSObjectAttributesInternal.Immutable) == 0)
                    && (r.attributes & JSObjectAttributesInternal.SystemObject) != 0
                    && (r.attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                    fields[name] = r = r.CloneImpl();
                return r;
            }
            if (members == null)
                fillMembers();
            IList<MemberInfo> m = null;
            members.TryGetValue(name, out m);
            if (m == null || m.Count == 0)
            {
                var pi = prototypeInstance as JSObject;
                if (pi != null)
                    return pi.GetMember(nameObj, create, own);
                else
                    return DefaultFieldGetter(nameObj, create, own);
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
                                    oValue = new PropertyPair
                                    {
                                        set = m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ?
                                            new ExternalFunction((thisBind, a) =>
                                            {
                                                field.SetValue(field.IsStatic ? null : thisBind.Value, cva.To(a[0].Value));
                                                return null;
                                            }) : null,
                                        get = new ExternalFunction((thisBind, a) =>
                                        {
                                            return Proxy(cva.From(field.GetValue(field.IsStatic ? null : thisBind.Value)));
                                        })
                                    }
                                };
                                r.attributes = JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.Field;
                                if ((r.oValue as PropertyPair).set == null)
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
                                        oValue = new PropertyPair
                                        {
                                            set = !m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ? new ExternalFunction((thisBind, a) =>
                                            {
                                                field.SetValue(field.IsStatic ? null : thisBind.Value, a[0].Value);
                                                return null;
                                            }) : null,
                                            get = new ExternalFunction((thisBind, a) =>
                                            {
                                                return Proxy(field.GetValue(field.IsStatic ? null : thisBind.Value));
                                            })
                                        }
                                    };
                                    r.attributes = JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.Field;
                                    if ((r.oValue as PropertyPair).set == null)
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
                                    oValue = new PropertyPair
                                        {
                                            set = pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false), cva, new[] { cva }) : null,
                                            get = pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false), cva, null) : null
                                        }
                                };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    valueType = JSObjectType.Property,
                                    oValue = new PropertyPair
                                        {
                                            set = pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                            get = pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null
                                        }
                                };
                            }
                            r.attributes = JSObjectAttributesInternal.Immutable;
                            if ((r.oValue as PropertyPair).set == null)
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
                                oValue = new PropertyPair
                                {
                                    set = new MethodProxy(pinfo.GetAddMethod())
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
            if (m[0].IsDefined(typeof(DoNotEnumerateAttribute), false))
                r.attributes |= JSObjectAttributesInternal.DoNotEnum;
            lock (fields)
                fields[name] = create && (r.attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) == JSObjectAttributesInternal.SystemObject ? (r = r.CloneImpl()) : r;

            for (var i = m.Count; i-- > 0; )
            {
                if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                {
                    r.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    break;
                }
                if (m[i].IsDefined(typeof(ReadOnlyAttribute), false))
                    r.attributes |= JSObjectAttributesInternal.ReadOnly;
                if (m[i].IsDefined(typeof(NotConfigurable), false))
                    r.attributes |= JSObjectAttributesInternal.NotConfigurable;
                if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                    r.attributes |= JSObjectAttributesInternal.DoNotDelete;
            }
            return r;
        }

        internal override bool DeleteMember(JSObject name)
        {
            if (members == null)
                fillMembers();
            string tname = null;
            JSObject field = null;
            if (fields != null
                && fields.TryGetValue(tname = name.ToString(), out field)
                && (!field.IsExist || (field.attributes & JSObjectAttributesInternal.DoNotDelete) == 0))
            {
                if ((field.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    field.valueType = JSObjectType.NotExistsInObject;
                return fields.Remove(tname) | members.Remove(tname); // it's not mistake
            }
            else
            {
                IList<MemberInfo> m = null;
                if (members.TryGetValue(tname.ToString(), out m))
                {
                    for (var i = m.Count; i-- > 0; )
                    {
                        if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                            return false;
                    }
                }
                if (!members.Remove(tname) && prototypeInstance != null)
                    return _prototypeInstance.DeleteMember(tname);
            }
            return true;
        }

        public override JSObject propertyIsEnumerable(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var name = args[0].ToString();
            JSObject temp;
            if (fields != null && fields.TryGetValue(name, out temp))
                return temp.IsExist && (temp.attributes & JSObjectAttributesInternal.DoNotEnum) == 0;
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
            if (prototypeInstance != null)
            {
                var @enum = prototypeInstance.GetEnumeratorImpl(pdef);
                while (@enum.MoveNext())
                    yield return @enum.Current;
            }
            else
            {
                foreach (var f in fields)
                {
                    if (!pdef || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0)
                        yield return f.Key;
                }
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
    }
}