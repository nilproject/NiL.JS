using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Interop
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSValue> staticProxies = new Dictionary<Type, JSValue>();
        private static readonly Dictionary<Type, TypeProxy> dynamicProxies = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
#if !(PORTABLE || NETCORE)
        [NonSerialized]
#endif
        internal Dictionary<string, IList<MemberInfo>> members;

        private ConstructorInfo ictor;
        private JSObject _prototypeInstance;
        internal JSObject prototypeInstance
        {
            get
            {
#if (PORTABLE || NETCORE)
                if (_prototypeInstance == null && InstanceMode && !hostedType.GetTypeInfo().IsAbstract)
                {
#else
                if (_prototypeInstance == null && InstanceMode && !hostedType.IsAbstract)
                {
                    try
                    {
#endif
                        if (ictor != null)
                        {
                            if (hostedType == typeof(JSObject))
                            {
                                _prototypeInstance = CreateObject();
                                _prototypeInstance.__prototype = @null;
                                _prototypeInstance.fields = fields;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                            }
                            else if (typeof(JSObject).IsAssignableFrom(hostedType))
                            {
                                _prototypeInstance = ictor.Invoke(null) as JSObject;
                                _prototypeInstance.__prototype = __proto__;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                                _prototypeInstance.fields = fields;
                                //_prototypeInstance.valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance.valueType);
                                _valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance._valueType);
                            }
                            else
                            {
                                _prototypeInstance = new ObjectWrapper(ictor.Invoke(null))
                                {
                                    _attributes = _attributes | JSValueAttributesInternal.ProxyPrototype,
                                    fields = fields,
                                    __prototype = JSObject.GlobalPrototype
                                };
                            }
                        }
#if !(PORTABLE || NETCORE)
                    }
                    catch (COMException)
                    {

                    }
#endif
                }

                return _prototypeInstance;
            }
        }

        internal bool InstanceMode = false;

        private TypeProxy()
        {
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.DoNotEnumerate;
            fields = getFieldsContainer();
        }

        private TypeProxy(Type type, JSObject prototype)
            : this()
        {
            if (dynamicProxies.ContainsKey(type))
                throw new InvalidOperationException("Type \"" + type + "\" already proxied.");
            else
            {
                hostedType = type;
                dynamicProxies[type] = this;
                __prototype = prototype;
                try
                {
#if (PORTABLE || NETCORE)
                    ictor = hostedType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0 && !x.IsStatic);
#else
                    ictor = hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
#endif

                    if (hostedType.GetTypeInfo().IsDefined(typeof(ImmutableAttribute), false))
                        _attributes |= JSValueAttributesInternal.Immutable;
                    var staticProxy = new TypeProxy()
                    {
                        hostedType = type,
                        InstanceMode = false
                    };
                    InstanceMode = true;

                    if (typeof(JSValue).IsAssignableFrom(hostedType))
                        _prototypeInstance = prototypeInstance;

#if (PORTABLE || NETCORE)
                    if (hostedType.GetTypeInfo().IsAbstract)
#else
                    if (hostedType.IsAbstract)
#endif
                    {
                        staticProxies[type] = staticProxy;
                    }
                    else
                    {
                        Function ctor = null;
                        if (type == typeof(JSObject))
                            ctor = new ObjectConstructor(staticProxy);
                        else
                            ctor = new ProxyConstructor(staticProxy);
                        ctor._attributes = _attributes;
                        _attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.ReadOnly;
                        staticProxies[type] = ctor;
                        if (hostedType != typeof(ProxyConstructor))
                            fields["constructor"] = ctor;
                    }
                }
                catch
                {
                    dynamicProxies.Remove(type);
                    throw;
                }
            }
        }

        public static JSValue Proxy(object value)
        {
            JSValue res;
            if (value == null)
                return JSValue.NotExists;
            else
            {
                res = value as JSValue;
                if (res != null)
                    return res;
            }
#if PORTABLE
            switch (value.GetType().GetTypeCode())
#else
            switch (Type.GetTypeCode(value.GetType()))
#endif
            {
                case TypeCode.Boolean:
                    {
                        return new JSValue
                        {
                            _iValue = (bool)value ? 1 : 0,
                            _valueType = JSValueType.Boolean
                        };
                    }
                case TypeCode.Byte:
                    {
                        return new JSValue
                        {
                            _iValue = (byte)value,
                            _valueType = JSValueType.Integer
                        };
                    }
                case TypeCode.Char:
                    {
                        return new JSValue
                        {
                            _oValue = ((char)value).ToString(),
                            _valueType = JSValueType.String
                        };
                    }
                case TypeCode.DateTime:
                    {
                        var dateTime = (DateTime)value;
                        var timeZoneOffset = dateTime.Kind == DateTimeKind.Local ? dateTime.ToLocalTime().Ticks - dateTime.ToUniversalTime().Ticks : 0;
                        return new ObjectWrapper(new Date(dateTime.ToUniversalTime().Ticks + timeZoneOffset, timeZoneOffset));
                    }
                case TypeCode.Decimal:
                    {
                        return new JSValue
                        {
                            _dValue = (double)(decimal)value,
                            _valueType = JSValueType.Double
                        };
                    }
                case TypeCode.Double:
                    {
                        return new JSValue
                        {
                            _dValue = (double)value,
                            _valueType = JSValueType.Double
                        };
                    }
                case TypeCode.Int16:
                    {
                        return new JSValue
                        {
                            _iValue = (short)value,
                            _valueType = JSValueType.Integer
                        };
                    }
                case TypeCode.Int32:
                    {
                        return new JSValue
                        {
                            _iValue = (int)value,
                            _valueType = JSValueType.Integer
                        };
                    }
                case TypeCode.Int64:
                    {
                        return new JSValue
                        {
                            _dValue = (long)value,
                            _valueType = JSValueType.Double
                        };
                    }
                case TypeCode.SByte:
                    {
                        return new JSValue
                        {
                            _iValue = (sbyte)value,
                            _valueType = JSValueType.Integer
                        };
                    }
                case TypeCode.Single:
                    {
                        return new JSValue
                        {
                            _dValue = (float)value,
                            _valueType = JSValueType.Double
                        };
                    }
                case TypeCode.String:
                    {
                        return new JSValue
                        {
                            _oValue = value,
                            _valueType = JSValueType.String
                        };
                    }
                case TypeCode.UInt16:
                    {
                        return new JSValue
                        {
                            _iValue = (ushort)value,
                            _valueType = JSValueType.Integer
                        };
                    }
                case TypeCode.UInt32:
                    {
                        var v = (uint)value;
                        if (v > int.MaxValue)
                        {
                            return new JSValue
                            {
                                _dValue = v,
                                _valueType = JSValueType.Double
                            };
                        }
                        else
                        {
                            return new JSValue
                            {
                                _iValue = (int)v,
                                _valueType = JSValueType.Integer
                            };
                        }
                    }
                case TypeCode.UInt64:
                    {
                        var v = (long)value;
                        if (v > int.MaxValue)
                        {
                            return new JSValue
                            {
                                _dValue = v,
                                _valueType = JSValueType.Double
                            };
                        }
                        else
                        {
                            return new JSValue
                            {
                                _iValue = (int)v,
                                _valueType = JSValueType.Integer
                            };
                        }
                    }
                default:
                    {
                        if (value is Delegate)
                        {
                            return new JSValue
                            {
#if (PORTABLE || NETCORE)
                                _oValue = new MethodProxy(((Delegate)value).GetMethodInfo(), ((Delegate)value).Target),
#else
                                _oValue = new MethodProxy(((Delegate)value).Method, ((Delegate)value).Target),
#endif
                                _valueType = JSValueType.Function
                            };
                        }
                        else if (value is IList)
                        {
                            return new JSValue
                            {
                                _oValue = new NativeList(value as IList),
                                _valueType = JSValueType.Object
                            };
                        }
                        else
                        {
                            return new ObjectWrapper(value);
                        }
                    }
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            return GetPrototype(type, true);
        }

        internal static TypeProxy GetPrototype(Type type, bool create)
        {
            TypeProxy prot = null;
            lock (dynamicProxies)
            {
                if (!dynamicProxies.TryGetValue(type, out prot))
                {
                    if (!create)
                        return null;
                    JSObject prototype = null;
                    var pa = type.GetTypeInfo().GetCustomAttributes(typeof(PrototypeAttribute), false).ToArray();
                    if (pa.Length != 0 && (pa[0] as PrototypeAttribute).PrototypeType != type)
                    {
                        if ((pa[0] as PrototypeAttribute).Replace && (pa[0] as PrototypeAttribute).PrototypeType.IsAssignableFrom(type))
                            return GetPrototype((pa[0] as PrototypeAttribute).PrototypeType, create);

                        prototype = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType);
                    }

                    prot = new TypeProxy(type, prototype);
                }
            }

            return prot;
        }
        
        public new static JSValue GetConstructor(Type type)
        {
            JSValue constructor = null;
            if (!staticProxies.TryGetValue(type, out constructor))
            {
                lock (staticProxies)
                {
#if (PORTABLE || NETCORE)
                    if (type.GetTypeInfo().ContainsGenericParameters)
#else
                    if (type.ContainsGenericParameters)
#endif
                        return staticProxies[type] = GetGenericTypeSelector(new[] { type });

                    JSObject prototype = null;
                    var pa = type.GetTypeInfo().GetCustomAttributes(typeof(PrototypeAttribute), false).ToArray();
                    if (pa.Length != 0 && (pa[0] as PrototypeAttribute).PrototypeType != type)
                    {
                        if ((pa[0] as PrototypeAttribute).Replace && (pa[0] as PrototypeAttribute).PrototypeType.IsAssignableFrom(type))
                            return GetConstructor((pa[0] as PrototypeAttribute).PrototypeType);

                        prototype = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType);
                    }

                    new TypeProxy(type, prototype); // It's ok. This instance will be registered and saved
                    constructor = staticProxies[type];
                }
            }

            return constructor;
        }

        internal static void Clear()
        {
            NiL.JS.BaseLibrary.Boolean.True.__prototype = null;
            NiL.JS.BaseLibrary.Boolean.False.__prototype = null;
            staticProxies.Clear();
            dynamicProxies.Clear();
        }

        internal override JSObject GetDefaultPrototype()
        {
#if (PORTABLE || NETCORE)
            if (Context.currentContextStack == null)
                throw new Exception();
#else
            if (Context.RunningContexts.Length == 0) // always false, but it protects from uninitialized global context
                throw new Exception();
#endif
            return GlobalPrototype;
        }

        private void fillMembers()
        {
            lock (this)
            {
                if (members != null)
                    return;
                var tempMembers = new Dictionary<string, IList<MemberInfo>>();
                string prewName = null;
                IList<MemberInfo> temp = null;
                bool instanceAttribute = false;
#if (PORTABLE || NETCORE)
                    var mmbrs = hostedType.GetTypeInfo().DeclaredMembers
                        .Union(hostedType.GetRuntimeMethods())
                        .Union(hostedType.GetRuntimeProperties())
                        .Union(hostedType.GetRuntimeFields())
                        .Union(hostedType.GetRuntimeEvents()).ToArray(); // ïðèõîäèòñÿ äåëàòü âîò òàê íåîïòèìàëüíî, äðóãîãî ñïîñîáà íåò
#else
                var mmbrs = hostedType.GetMembers();
#endif
                for (int i = 0; i < mmbrs.Length; i++)
                {
                    if (mmbrs[i].IsDefined(typeof(HiddenAttribute), false))
                        continue;

                    instanceAttribute = mmbrs[i].IsDefined(typeof(InstanceMemberAttribute), false);

                    if (!InstanceMode && instanceAttribute)
                        continue;

                    if (mmbrs[i] is PropertyInfo)
                    {
                        if (((mmbrs[i] as PropertyInfo).GetSetMethod(true) ?? (mmbrs[i] as PropertyInfo).GetGetMethod(true)).IsStatic != !(InstanceMode ^ instanceAttribute))
                            continue;
                        if (((mmbrs[i] as PropertyInfo).GetSetMethod(true) == null || !(mmbrs[i] as PropertyInfo).GetSetMethod(true).IsPublic)
                            && ((mmbrs[i] as PropertyInfo).GetGetMethod(true) == null || !(mmbrs[i] as PropertyInfo).GetGetMethod(true).IsPublic))
                            continue;
                    }
                    if ((mmbrs[i] is EventInfo)
                        && (!(mmbrs[i] as EventInfo).GetAddMethod(true).IsPublic || (mmbrs[i] as EventInfo).GetAddMethod(true).IsStatic != !InstanceMode))
                        continue;

                    if ((mmbrs[i] is FieldInfo) && (!(mmbrs[i] as FieldInfo).IsPublic || (mmbrs[i] as FieldInfo).IsStatic != !InstanceMode))
                        continue;
#if (PORTABLE || NETCORE)
                    if ((mmbrs[i] is TypeInfo) && !(mmbrs[i] as TypeInfo).IsPublic)
                        continue;
#else
                    if ((mmbrs[i] is Type) && !(mmbrs[i] as Type).IsPublic && !(mmbrs[i] as Type).IsNestedPublic)
                        continue;
#endif
                    if (mmbrs[i] is MethodBase)
                    {
                        if ((mmbrs[i] as MethodBase).IsStatic != !(InstanceMode ^ instanceAttribute))
                            continue;
                        if (!(mmbrs[i] as MethodBase).IsPublic)
                            continue;
                        if ((mmbrs[i] as MethodBase).DeclaringType == typeof(object) && mmbrs[i].Name == "GetType")
                            continue;
                        if (mmbrs[i] is ConstructorInfo)
                            continue;
                    }

                    var membername = mmbrs[i].Name;
                    membername = membername[0] == '.' ? membername : membername.Contains(".") ? membername.Substring(membername.LastIndexOf('.') + 1) : membername;
#if (PORTABLE || NETCORE)
                    if (mmbrs[i] is TypeInfo && membername.Contains("`"))
#else
                    if (mmbrs[i] is Type && membername.Contains('`'))
#endif
                        membername = membername.Substring(0, membername.IndexOf('`'));

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
                                    tempMembers.Remove(prewName + "$" + (++offset + j));
                                }
                            }
                            if (temp.Count == 1)
                                tempMembers.Remove(prewName + "$0");
                        }
                        if (!tempMembers.TryGetValue(membername, out temp))
                            tempMembers[membername] = temp = new List<MemberInfo>();
                        prewName = membername;
                    }
                    if (temp.Count == 1)
                        tempMembers.Add(membername + "$0", new[] { temp[0] });
                    temp.Add(mmbrs[i]);
                    if (temp.Count != 1)
                        tempMembers.Add(membername + "$" + (temp.Count - 1), new[] { mmbrs[i] });
                }
                members = tempMembers;

                if (InstanceMode && typeof(IIterable).IsAssignableFrom(hostedType))
                {
                    IList<MemberInfo> iterator = null;
                    if (members.TryGetValue("iterator", out iterator))
                    {
                        this.SetProperty(Symbol.iterator, proxyMember(false, iterator), false);
                        members.Remove("iterator");
                    }
                }
            }
        }

        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope == PropertyScope.Super || key._valueType == JSValueType.Symbol)
                return base.GetProperty(key, forWrite, memberScope);

            forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;

            string name = key.ToString();
            JSValue r = null;
            if (fields.TryGetValue(name, out r))
            {
                if (!r.Exists)
                {
                    if (!forWrite)
                    {
                        var t = base.GetProperty(key, false, memberScope);
                        if (t.Exists)
                        {
                            r.Assign(t);
                            r._valueType = t._valueType;
                        }
                    }
                }
                if (forWrite
                    && (r._attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                    fields[name] = r = r.CloneImpl(false);
                return r;
            }

            if (members == null)
                fillMembers();

            IList<MemberInfo> m = null;
            members.TryGetValue(name, out m);

            if (m == null || m.Count == 0)
            {
                var pi = prototypeInstance as JSValue;
                if (pi != null)
                    return pi.GetProperty(key, forWrite, memberScope);
                else
                    return base.GetProperty(key, forWrite, memberScope);
            }

            var result = proxyMember(forWrite, m);
            fields[name] = result;

            return result;
        }

        internal JSValue proxyMember(bool forWrite, IList<MemberInfo> m)
        {
            JSValue r = null;
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodBase))
                        ExceptionsHelper.Throw(Proxy(new TypeError("Incompatible fields types.")));
                var cache = new MethodProxy[m.Count];
                for (int i = 0; i < m.Count; i++)
                    cache[i] = new MethodProxy(m[i] as MethodBase);
                r = new MethodGroup(cache);
            }
            else
            {
#if PORTABLE
                switch (m[0].GetMemberType())
#else
                switch (m[0].MemberType)
#endif
                {
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(method);
                            r._attributes &= ~(JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotEnumerate);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            if ((field.Attributes & (FieldAttributes.Literal | FieldAttributes.InitOnly)) != 0
                                && (field.Attributes & FieldAttributes.Static) != 0)
                            {
                                r = Proxy(field.GetValue(null));
                                r._attributes |= JSValueAttributesInternal.ReadOnly;
                            }
                            else
                            {
                                r = new JSValue()
                                {
                                    _valueType = JSValueType.Property,
                                    _oValue = new GsPropertyPair
                                    (
                                        new ExternalFunction((thisBind, a) => Proxy(field.GetValue(field.IsStatic ? null : thisBind.Value))),
                                        !m[0].IsDefined(typeof(Interop.ReadOnlyAttribute), false) ? new ExternalFunction((thisBind, a) =>
                                        {
                                            field.SetValue(field.IsStatic ? null : thisBind.Value, a[0].Value);
                                            return null;
                                        }) : null
                                    )
                                };
                                r._attributes = JSValueAttributesInternal.Immutable | JSValueAttributesInternal.Field;
                                if ((r._oValue as GsPropertyPair).set == null)
                                    r._attributes |= JSValueAttributesInternal.ReadOnly;

                            }
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var pinfo = (PropertyInfo)m[0];
                            r = new JSValue()
                            {
                                _valueType = JSValueType.Property,
                                _oValue = new GsPropertyPair
                                    (
#if (PORTABLE || NETCORE)
pinfo.CanRead && pinfo.GetMethod != null ? new MethodProxy(pinfo.GetMethod) : null,
                                            pinfo.CanWrite && pinfo.SetMethod != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.SetMethod) : null
#else
pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null,
                                        pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false)) : null
#endif
)
                            };

                            r._attributes = JSValueAttributesInternal.Immutable;
                            if ((r._oValue as GsPropertyPair).set == null)
                                r._attributes |= JSValueAttributesInternal.ReadOnly;
                            if (pinfo.IsDefined(typeof(FieldAttribute), false))
                                r._attributes |= JSValueAttributesInternal.Field;
                            break;
                        }
                    case MemberTypes.Event:
                        {
                            var pinfo = (EventInfo)m[0];
                            r = new JSValue()
                            {
                                _valueType = JSValueType.Property,
                                _oValue = new GsPropertyPair
                                (
                                    null,
#if (PORTABLE || NETCORE)
 new MethodProxy(pinfo.AddMethod)
#else
 new MethodProxy(pinfo.GetAddMethod())
#endif
)
                            };
                            break;
                        }
                    case MemberTypes.TypeInfo:
#if (PORTABLE || NETCORE)
                        {
                            r = GetConstructor((m[0] as TypeInfo).AsType());
                            break;
                        }
#else
                    case MemberTypes.NestedType:
                        {
                            r = GetConstructor((Type)m[0]);
                            break;
                        }
                    default:
                        throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
#endif
                }
            }
            if (m[0].IsDefined(typeof(DoNotEnumerateAttribute), false))
                r._attributes |= JSValueAttributesInternal.DoNotEnumerate;
            if (forWrite && (r._attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.SystemObject)) == JSValueAttributesInternal.SystemObject)
                r = r.CloneImpl(false);

            for (var i = m.Count; i-- > 0;)
            {
                if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                    r._attributes &= ~JSValueAttributesInternal.DoNotEnumerate;
                if (m[i].IsDefined(typeof(ReadOnlyAttribute), false))
                    r._attributes |= JSValueAttributesInternal.ReadOnly;
                if (m[i].IsDefined(typeof(NotConfigurable), false))
                    r._attributes |= JSValueAttributesInternal.NonConfigurable;
                if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                    r._attributes |= JSValueAttributesInternal.DoNotDelete;
            }
            return r;
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            if (members == null)
                fillMembers();
            string tname = null;
            JSValue field = null;
            if (fields != null
                && fields.TryGetValue(tname = name.ToString(), out field)
                && (!field.Exists || (field._attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field._attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field._valueType = JSValueType.NotExistsInObject;
                return fields.Remove(tname) | members.Remove(tname); // it's not mistake
            }
            else
            {
                IList<MemberInfo> m = null;
                if (members.TryGetValue(tname.ToString(), out m))
                {
                    for (var i = m.Count; i-- > 0;)
                    {
                        if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                            return false;
                    }
                }
                if (!members.Remove(tname) && prototypeInstance != null)
                    return _prototypeInstance.DeleteProperty(tname);
            }
            return true;
        }

        public override JSValue propertyIsEnumerable(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var name = args[0].ToString();
            JSValue temp;
            if (fields != null && fields.TryGetValue(name, out temp))
                return temp.Exists && (temp._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0;
            IList<MemberInfo> m = null;
            if (members.TryGetValue(name, out m))
            {
                for (var i = m.Count; i-- > 0;)
                    if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                        return true;
                return false;
            }
            return false;
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            if (members == null)
                fillMembers();
            if (prototypeInstance != null)
            {
                var @enum = prototypeInstance.GetEnumerator(hideNonEnumerable, enumerationMode);
                while (@enum.MoveNext())
                    yield return @enum.Current;
            }
            else
            {
                foreach (var f in fields)
                {
                    if (!hideNonEnumerable || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0)
                        yield return f;
                }
            }
            foreach (var item in members)
            {
                if (fields.ContainsKey(item.Key))
                    continue;
                for (var i = item.Value.Count; i-- > 0;)
                {
                    if (item.Value[i].IsDefined(typeof(HiddenAttribute), false))
                        continue;

                    if (!hideNonEnumerable || !item.Value[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                    {
                        switch (enumerationMode)
                        {
                            case EnumerationMode.KeysOnly:
                                {
                                    yield return new KeyValuePair<string, JSValue>(item.Key, null);
                                    break;
                                }
                            case EnumerationMode.RequireValues:
                            case EnumerationMode.RequireValuesForWrite:
                                {
                                    yield return new KeyValuePair<string, JSValue>(
                                        item.Key,
                                        fields[item.Key] = proxyMember(enumerationMode == EnumerationMode.RequireValuesForWrite, item.Value));
                                    break;
                                }
                        }
                        break;
                    }
                }
            }
        }
    }
}
