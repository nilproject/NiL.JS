using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Backward;
using System.Runtime.InteropServices;

namespace NiL.JS.Core.Interop
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal abstract class Proxy : JSObject
    {
        internal Type _hostedType;
#if !(PORTABLE || NETCORE)
        [NonSerialized]
#endif
        internal StringMap<IList<MemberInfo>> _members;
        internal GlobalContext _context;

        private PropertyPair _indexerProperty;
        private bool _indexersSupported;
        private ConstructorInfo _instanceCtor;
        private JSObject _prototypeInstance;

        internal virtual JSObject PrototypeInstance
        {
            get
            {
#if (PORTABLE || NETCORE)
                if (_prototypeInstance == null && IsInstancePrototype && !_hostedType.GetTypeInfo().IsAbstract)
                {
#else
                if (_prototypeInstance == null && IsInstancePrototype && !_hostedType.IsAbstract)
                {
                    try
                    {
#endif
                        if (_instanceCtor != null)
                        {
                            if (_hostedType == typeof(JSObject))
                            {
                                _prototypeInstance = CreateObject();
                                _prototypeInstance._objectPrototype = @null;
                                _prototypeInstance._fields = _fields;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                            }
                            else if (typeof(JSObject).IsAssignableFrom(_hostedType))
                            {
                                _prototypeInstance = _instanceCtor.Invoke(null) as JSObject;
                                _prototypeInstance._objectPrototype = __proto__;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                                _prototypeInstance._fields = _fields;
                                //_prototypeInstance.valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance.valueType);
                                _valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance._valueType);
                            }
                            else
                            {
                                var instance = _instanceCtor.Invoke(null);
                                _prototypeInstance = new ObjectWrapper(instance, this)
                                {
                                    _attributes = _attributes | JSValueAttributesInternal.ProxyPrototype,
                                    _fields = _fields,
                                    _objectPrototype = _context.GlobalContext._globalPrototype
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

        internal abstract bool IsInstancePrototype { get; }

        internal Proxy(GlobalContext context, Type type, bool indexersSupport)
        {
            _indexersSupported = indexersSupport;
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.DoNotEnumerate;
            _fields = getFieldsContainer();

            _context = context;
            _hostedType = type;

#if (PORTABLE || NETCORE)
            _instanceCtor = _hostedType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0 && !x.IsStatic);
#else
            _instanceCtor = _hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
#endif
        }

        private void fillMembers()
        {
            lock (this)
            {
                if (_members != null)
                    return;

                var tempMembers = new StringMap<IList<MemberInfo>>();
                string prewName = null;
                IList<MemberInfo> temp = null;
                bool instanceAttribute = false;
#if (PORTABLE || NETCORE)
                    var members = _hostedType.GetTypeInfo().DeclaredMembers
                         .Union(_hostedType.GetRuntimeMethods())
                         .Union(_hostedType.GetRuntimeProperties())
                         .Union(_hostedType.GetRuntimeFields())
                         .Union(_hostedType.GetRuntimeEvents()).ToArray();
#else
                var members = _hostedType.GetMembers();
#endif
                for (int i = 0; i < members.Length; i++)
                {
                    var member = members[i];
                    if (member.IsDefined(typeof(HiddenAttribute), false))
                        continue;

                    instanceAttribute = member.IsDefined(typeof(InstanceMemberAttribute), false);

                    if (!IsInstancePrototype && instanceAttribute)
                        continue;

                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        if ((property.GetSetMethod(true) ?? property.GetGetMethod(true)).IsStatic != !(IsInstancePrototype ^ instanceAttribute))
                            continue;
                        if ((property.GetSetMethod(true) == null || !property.GetSetMethod(true).IsPublic)
                            && (property.GetGetMethod(true) == null || !property.GetGetMethod(true).IsPublic))
                            continue;

                        var parentProperty = property;
                        while (parentProperty != null
                            && parentProperty.DeclaringType != typeof(object)
                            && ((property.GetGetMethod() ?? property.GetSetMethod()).Attributes & MethodAttributes.NewSlot) == 0)
                        {
                            property = parentProperty;
#if (PORTABLE || NETCORE)
                            parentProperty = property.DeclaringType.GetTypeInfo().BaseType?.GetRuntimeProperty(property.Name);
#else
                            parentProperty = property.DeclaringType.GetTypeInfo().BaseType?.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
#endif
                        }

                        member = property;
                    }

                    if (member is EventInfo
                        && (!(member as EventInfo).GetAddMethod(true).IsPublic || (member as EventInfo).GetAddMethod(true).IsStatic != !IsInstancePrototype))
                        continue;

                    if (member is FieldInfo && (!(member as FieldInfo).IsPublic || (member as FieldInfo).IsStatic != !IsInstancePrototype))
                        continue;
#if (PORTABLE || NETCORE)
                    if ((members[i] is TypeInfo) && !(members[i] as TypeInfo).IsPublic)
                        continue;
#else
                    if (member is Type && !(member as Type).IsPublic && !(member as Type).IsNestedPublic)
                        continue;
#endif
                    var method = member as MethodBase;
                    if (method != null)
                    {
                        if (method.IsStatic != !(IsInstancePrototype ^ instanceAttribute))
                            continue;
                        if (!method.IsPublic)
                            continue;
                        if (method.DeclaringType == typeof(object) && member.Name == "GetType")
                            continue;
                        if (method is ConstructorInfo)
                            continue;

                        if (method.IsVirtual && (method.Attributes & MethodAttributes.NewSlot) == 0)
                        {
                            var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
                            var parentMethod = method;
                            while (parentMethod != null && parentMethod.DeclaringType != typeof(object) && (method.Attributes & MethodAttributes.NewSlot) == 0)
                            {
                                method = parentMethod;
#if (PORTABLE || NETCORE)
                                parentMethod = method.DeclaringType.GetTypeInfo().BaseType?.GetMethod(method.Name, parameterTypes);
#else
                                parentMethod = method.DeclaringType.BaseType?.GetMethod(method.Name, BindingFlags.Public | BindingFlags.Instance, null, parameterTypes, null);
#endif
                            }
                        }

                        member = method;
                    }

                    var membername = member.Name;
                    if (member.IsDefined(typeof(JavaScriptNameAttribute), false))
                    {
                        var nameOverrideAttribute = member.GetCustomAttributes(typeof(JavaScriptNameAttribute), false).ToArray();
                        membername = (nameOverrideAttribute[0] as JavaScriptNameAttribute).Name;
                    }
                    else
                    {
                        membername = membername[0] == '.' ? membername : membername.Contains(".") ? membername.Substring(membername.LastIndexOf('.') + 1) : membername;

#if (PORTABLE || NETCORE)
                        if (members[i] is TypeInfo && membername.Contains("`"))
#else
                        if (member is Type && membername.Contains('`'))
#endif
                        {
                            membername = membername.Substring(0, membername.IndexOf('`'));
                        }
                    }

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

                    if (membername.StartsWith("@@"))
                    {
                        if (_symbols == null)
                            _symbols = new Dictionary<Symbol, JSValue>();
                        _symbols.Add(Symbol.@for(membername.Substring(2)), proxyMember(false, new[] { member }));
                    }
                    else
                    {
                        if (temp.Count == 1)
                            tempMembers.Add(membername + "$0", new[] { temp[0] });

                        temp.Add(member);

                        if (temp.Count != 1)
                            tempMembers.Add(membername + "$" + (temp.Count - 1), new[] { member });
                    }
                }

                _members = tempMembers;

                if (IsInstancePrototype)
                {
                    if (typeof(IIterable).IsAssignableFrom(_hostedType))
                    {
                        IList<MemberInfo> iterator = null;
                        if (_members.TryGetValue("iterator", out iterator))
                        {
                            if (_symbols == null)
                                _symbols = new Dictionary<Symbol, JSValue>();
                            _symbols.Add(Symbol.iterator, proxyMember(false, iterator));
                            _members.Remove("iterator");
                        }
                    }
#if NET40
                    var toStringTag = _hostedType.GetCustomAttribute<ToStringTagAttribute>();
#else
                    var toStringTag = _hostedType.GetTypeInfo().GetCustomAttribute<ToStringTagAttribute>();
#endif
                    if (toStringTag != null)
                    {
                        if (_symbols == null)
                            _symbols = new Dictionary<Symbol, JSValue>();
                        _symbols.Add(Symbol.toStringTag, toStringTag.Tag);
                    }
                }

                if (_indexersSupported)
                {
                    IList<MemberInfo> getter = null;
                    IList<MemberInfo> setter = null;
                    _members.TryGetValue("get_Item", out getter);
                    _members.TryGetValue("set_Item", out setter);

                    if (getter != null || setter != null)
                    {
                        _indexerProperty = new PropertyPair();

                        if (getter != null)
                        {
                            _indexerProperty.getter = (Function)proxyMember(false, getter);
                            _fields["get_Item"] = _indexerProperty.getter;
                        }

                        if (setter != null)
                        {
                            _indexerProperty.setter = (Function)proxyMember(false, setter);
                            _fields["set_Item"] = _indexerProperty.setter;
                        }
                    }
                    else
                    {
                        _indexersSupported = false;
                    }
                }
            }
        }

        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (_members == null)
                fillMembers();

            if (memberScope == PropertyScope.Super || key._valueType == JSValueType.Symbol)
                return base.GetProperty(key, forWrite, memberScope);

            forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;

            string name = key.ToString();
            JSValue r = null;
            if (_fields.TryGetValue(name, out r))
            {
                if (!r.Exists && !forWrite)
                {
                    var t = base.GetProperty(key, false, memberScope);
                    if (t.Exists)
                    {
                        r.Assign(t);
                        r._valueType = t._valueType;
                    }
                }

                if (forWrite && r.NeedClone)
                    _fields[name] = r = r.CloneImpl(false);

                return r;
            }

            IList<MemberInfo> m = null;
            _members.TryGetValue(name, out m);

            if (m == null || m.Count == 0)
            {
                JSValue property;
                var protoInstanceAsJs = PrototypeInstance as JSValue;
                if (protoInstanceAsJs != null)
                    property = protoInstanceAsJs.GetProperty(key, forWrite && !_indexersSupported, memberScope);
                else
                    property = base.GetProperty(key, forWrite && !_indexersSupported, memberScope);

                if (!_indexersSupported)
                    return property;

                if (property.Exists)
                {
                    if (forWrite)
                    {
                        if ((property._attributes & (JSValueAttributesInternal.SystemObject & JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                        {
                            if (protoInstanceAsJs != null)
                                property = protoInstanceAsJs.GetProperty(key, true, memberScope);
                            else
                                property = base.GetProperty(key, true, memberScope);
                        }
                    }

                    return property;
                }

                if (forWrite)
                {
                    return new JSValue
                    {
                        _valueType = JSValueType.Property,
                        _oValue = new PropertyPair(null, _indexerProperty.setter.bind(new Arguments { null, key }))
                    };
                }
                else
                {
                    return new JSValue
                    {
                        _valueType = JSValueType.Property,
                        _oValue = new PropertyPair(_indexerProperty.getter.bind(new Arguments { null, key }), null)
                    };
                }
            }

            var result = proxyMember(forWrite, m);
            _fields[name] = result;

            return result;
        }

        internal JSValue proxyMember(bool forWrite, IList<MemberInfo> m)
        {
            JSValue r = null;
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodBase))
                        ExceptionHelper.Throw(_context.ProxyValue(new TypeError("Incompatible fields types.")));

                var cache = new MethodProxy[m.Count];
                for (int i = 0; i < m.Count; i++)
                    cache[i] = new MethodProxy(_context, m[i] as MethodBase);
                r = new MethodGroup(cache);
            }
            else
            {
#if PORTABLE || NETCORE
                switch (m[0].GetMemberType())
#else
                switch (m[0].MemberType)
#endif
                {
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(_context, method);
                            r._attributes &= ~(JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotEnumerate);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            if ((field.Attributes & (FieldAttributes.Literal | FieldAttributes.InitOnly)) != 0
                                && (field.Attributes & FieldAttributes.Static) != 0)
                            {
                                r = _context.ProxyValue(field.GetValue(null));
                                r._attributes |= JSValueAttributesInternal.ReadOnly;
                            }
                            else
                            {
                                r = new JSValue()
                                {
                                    _valueType = JSValueType.Property,
                                    _oValue = new PropertyPair
                                    (
                                        new ExternalFunction((thisBind, a) => _context.ProxyValue(field.GetValue(field.IsStatic ? null : thisBind.Value))),
                                        !m[0].IsDefined(typeof(ReadOnlyAttribute), false) ? new ExternalFunction((thisBind, a) =>
                                        {
                                            field.SetValue(field.IsStatic ? null : thisBind.Value, a[0].Value);
                                            return null;
                                        }) : null
                                    )
                                };

                                r._attributes = JSValueAttributesInternal.Immutable | JSValueAttributesInternal.Field;
                                if ((r._oValue as PropertyPair).setter == null)
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
                                _oValue = new PropertyPair
                                    (
#if (PORTABLE || NETCORE)
                                        pinfo.CanRead && pinfo.GetMethod != null ? new MethodProxy(_context, pinfo.GetMethod) : null,
                                        pinfo.CanWrite && pinfo.SetMethod != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(_context, pinfo.SetMethod) : null
#else
                                        pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(_context, pinfo.GetGetMethod(false)) : null,
                                        pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(_context, pinfo.GetSetMethod(false)) : null
#endif
)
                            };

                            r._attributes = JSValueAttributesInternal.Immutable;

                            if ((r._oValue as PropertyPair).setter == null)
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
                                _oValue = new PropertyPair
                                (
                                    null,
#if (PORTABLE || NETCORE)
 new MethodProxy(_context, pinfo.AddMethod)
#else
 new MethodProxy(_context, pinfo.GetAddMethod())
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
                            r = _context.GetConstructor((Type)m[0]);
                            break;
                        }
                    default:
                        throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
#endif
                }
            }

            if (m[0].IsDefined(typeof(DoNotEnumerateAttribute), false))
                r._attributes |= JSValueAttributesInternal.DoNotEnumerate;

            if (forWrite && r.NeedClone)
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
            if (_members == null)
                fillMembers();

            string stringName = null;
            JSValue field = null;
            if (_fields != null
                && _fields.TryGetValue(stringName = name.ToString(), out field)
                && (!field.Exists || (field._attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field._attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field._valueType = JSValueType.NotExistsInObject;
                return _fields.Remove(stringName) | _members.Remove(stringName); // it's not mistake
            }
            else
            {
                IList<MemberInfo> m = null;
                if (_members.TryGetValue(stringName.ToString(), out m))
                {
                    for (var i = m.Count; i-- > 0;)
                    {
                        if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                            return false;
                    }
                }

                if (!_members.Remove(stringName) && PrototypeInstance != null)
                    return _prototypeInstance.DeleteProperty(stringName);
            }

            return true;
        }

        public override JSValue propertyIsEnumerable(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var name = args[0].ToString();
            JSValue temp;
            if (_fields != null && _fields.TryGetValue(name, out temp))
                return temp.Exists && (temp._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0;
            IList<MemberInfo> m = null;
            if (_members.TryGetValue(name, out m))
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
            if (_members == null)
                fillMembers();

            if (PrototypeInstance != null)
            {
                var @enum = PrototypeInstance.GetEnumerator(hideNonEnumerable, enumerationMode);
                while (@enum.MoveNext())
                    yield return @enum.Current;
            }
            else
            {
                foreach (var f in _fields)
                {
                    if (!hideNonEnumerable || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0)
                        yield return f;
                }
            }

            foreach (var item in _members)
            {
                if (_fields.ContainsKey(item.Key))
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
                                        _fields[item.Key] = proxyMember(enumerationMode == EnumerationMode.RequireValuesForWrite, item.Value));
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
