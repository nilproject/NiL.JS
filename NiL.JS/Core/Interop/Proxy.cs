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
    internal abstract class Proxy : JSObject
    {
        internal Type _hostedType;
#if !(PORTABLE || NETCORE)
        [NonSerialized]
#endif
        internal Dictionary<string, IList<MemberInfo>> members;
        internal GlobalContext _context;

        private ConstructorInfo instanceCtor;
        private JSObject _prototypeInstance;
        internal virtual JSObject prototypeInstance
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
                        if (instanceCtor != null)
                        {
                            if (_hostedType == typeof(JSObject))
                            {
                                _prototypeInstance = CreateObject();
                                _prototypeInstance.__prototype = @null;
                                _prototypeInstance._fields = _fields;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                            }
                            else if (typeof(JSObject).IsAssignableFrom(_hostedType))
                            {
                                _prototypeInstance = instanceCtor.Invoke(null) as JSObject;
                                _prototypeInstance.__prototype = __proto__;
                                _prototypeInstance._attributes |= JSValueAttributesInternal.ProxyPrototype;
                                _prototypeInstance._fields = _fields;
                                //_prototypeInstance.valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance.valueType);
                                _valueType = (JSValueType)System.Math.Max((int)JSValueType.Object, (int)_prototypeInstance._valueType);
                            }
                            else
                            {
                                var instance = instanceCtor.Invoke(null);
                                _prototypeInstance = new ObjectWrapper(instance, this)
                                {
                                    _attributes = _attributes | JSValueAttributesInternal.ProxyPrototype,
                                    _fields = _fields,
                                    __prototype = _context.GlobalContext._GlobalPrototype
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

        internal Proxy(GlobalContext context, Type type)
        {
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.DoNotEnumerate;
            _fields = getFieldsContainer();

            _context = context;
            _hostedType = type;
            
#if (PORTABLE || NETCORE)
            instanceCtor = _hostedType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0 && !x.IsStatic);
#else
            instanceCtor = _hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
#endif
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
            return _context.GlobalContext._GlobalPrototype;
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
                    var mmbrs = _hostedType.GetTypeInfo().DeclaredMembers
                         .Union(_hostedType.GetRuntimeMethods())
                         .Union(_hostedType.GetRuntimeProperties())
                         .Union(_hostedType.GetRuntimeFields())
                         .Union(_hostedType.GetRuntimeEvents()).ToArray();
#else
                var mmbrs = _hostedType.GetMembers();
#endif
                for (int i = 0; i < mmbrs.Length; i++)
                {
                    if (mmbrs[i].IsDefined(typeof(HiddenAttribute), false))
                        continue;

                    instanceAttribute = mmbrs[i].IsDefined(typeof(InstanceMemberAttribute), false);

                    if (!IsInstancePrototype && instanceAttribute)
                        continue;

                    if (mmbrs[i] is PropertyInfo)
                    {
                        if (((mmbrs[i] as PropertyInfo).GetSetMethod(true) ?? (mmbrs[i] as PropertyInfo).GetGetMethod(true)).IsStatic != !(IsInstancePrototype ^ instanceAttribute))
                            continue;
                        if (((mmbrs[i] as PropertyInfo).GetSetMethod(true) == null || !(mmbrs[i] as PropertyInfo).GetSetMethod(true).IsPublic)
                            && ((mmbrs[i] as PropertyInfo).GetGetMethod(true) == null || !(mmbrs[i] as PropertyInfo).GetGetMethod(true).IsPublic))
                            continue;
                    }
                    if ((mmbrs[i] is EventInfo)
                        && (!(mmbrs[i] as EventInfo).GetAddMethod(true).IsPublic || (mmbrs[i] as EventInfo).GetAddMethod(true).IsStatic != !IsInstancePrototype))
                        continue;

                    if ((mmbrs[i] is FieldInfo) && (!(mmbrs[i] as FieldInfo).IsPublic || (mmbrs[i] as FieldInfo).IsStatic != !IsInstancePrototype))
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
                        if ((mmbrs[i] as MethodBase).IsStatic != !(IsInstancePrototype ^ instanceAttribute))
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

                if (IsInstancePrototype && typeof(IIterable).IsAssignableFrom(_hostedType))
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

            if (members == null)
                fillMembers();

            IList<MemberInfo> m = null;
            members.TryGetValue(name, out m);

            if (m == null || m.Count == 0)
            {
                var protoInstanceAsJs = prototypeInstance as JSValue;
                if (protoInstanceAsJs != null)
                    return protoInstanceAsJs.GetProperty(key, forWrite, memberScope);
                else
                    return base.GetProperty(key, forWrite, memberScope);
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
#if PORTABLE
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
                                    _oValue = new GsPropertyPair
                                    (
                                        new ExternalFunction((thisBind, a) => _context.ProxyValue(field.GetValue(field.IsStatic ? null : thisBind.Value))),
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
                                        pinfo.CanRead && pinfo.GetMethod != null ? new MethodProxy(_context, pinfo.GetMethod) : null,
                                        pinfo.CanWrite && pinfo.SetMethod != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(_context, pinfo.SetMethod) : null
#else
                                        pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(_context, pinfo.GetGetMethod(false)) : null,
                                        pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(_context, pinfo.GetSetMethod(false)) : null
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
            if (members == null)
                fillMembers();
            string stringName = null;
            JSValue field = null;
            if (_fields != null
                && _fields.TryGetValue(stringName = name.ToString(), out field)
                && (!field.Exists || (field._attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field._attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field._valueType = JSValueType.NotExistsInObject;
                return _fields.Remove(stringName) | members.Remove(stringName); // it's not mistake
            }
            else
            {
                IList<MemberInfo> m = null;
                if (members.TryGetValue(stringName.ToString(), out m))
                {
                    for (var i = m.Count; i-- > 0;)
                    {
                        if (m[i].IsDefined(typeof(DoNotDeleteAttribute), false))
                            return false;
                    }
                }

                if (!members.Remove(stringName) && prototypeInstance != null)
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
                foreach (var f in _fields)
                {
                    if (!hideNonEnumerable || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0)
                        yield return f;
                }
            }

            foreach (var item in members)
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
