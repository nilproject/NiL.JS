using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    public enum JSObjectType : int
    {
        NotExist = 0,
        NotExistInObject = 1,
        Undefined = 2,
        Bool = 6,
        Int = 10,
        Double = 18,
        String = 34,
        Object = 66,
        Function = 130,
        Date = 258,
        Property = 514
    }

    [Serializable]
    [Flags]
    public enum JSObjectAttributes : int
    {
        None = 0,
        DoNotEnum = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        Argument = 1 << 16,
        /// <summary>
        /// Объект является системным.
        /// </summary>
        SystemObject = 1 << 17,
    }

    public delegate void AssignCallback(JSObject sender);

    [Serializable]
    /// <summary>
    /// Базовый объект для всех объектов, участвующих в выполнении скрипта.
    /// Для создания пользовательских объектов, в качестве базового типа, рекомендуется использовать тип NiL.JS.Core.EmbeddedType
    /// </summary>
    public class JSObject : IEnumerable<string>, IEnumerable, ICloneable
    {
        [Hidden]
        internal static readonly AssignCallback ErrorAssignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Hidden]
        internal static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Hidden]
        internal static readonly JSObject undefined = new JSObject() { valueType = JSObjectType.Undefined, attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum | JSObjectAttributes.ReadOnly };
        [Hidden]
        internal static readonly JSObject notExist = new JSObject() { valueType = JSObjectType.NotExist, attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum | JSObjectAttributes.ReadOnly | JSObjectAttributes.SystemObject };
        [Hidden]
        internal static readonly JSObject Null = new JSObject() { valueType = JSObjectType.Object, oValue = null, assignCallback = ErrorAssignCallback, attributes = JSObjectAttributes.DoNotEnum | JSObjectAttributes.SystemObject };
        [Hidden]
        internal static readonly JSObject nullString = new JSObject() { valueType = JSObjectType.String, oValue = "null", assignCallback = ErrorAssignCallback, attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum | JSObjectAttributes.SystemObject };
        [Hidden]
        internal static JSObject GlobalPrototype;

        static JSObject()
        {
            undefined.attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
        }

        [NonSerialized]
        [Hidden]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal AssignCallback assignCallback;
        [Hidden]
        internal JSObject __proto__;
        [Hidden]
        internal IDictionary<string, JSObject> fields;

        [Hidden]
        internal string lastRequestedName;
        [Hidden]
        internal JSObjectType valueType;
        [Hidden]
        internal int iValue;
        [Hidden]
        internal double dValue;
        [Hidden]
        internal object oValue;
        [Hidden]
        internal JSObjectAttributes attributes;

        /// <summary>
        /// Возвращает член объекта или пытается присвоить полученному члену значение.
        /// </summary>
        /// <param name="name">Имя члена.</param>
        /// <returns>Член объекта с указанным именем.</returns>
        [Hidden]
        public JSObject this[string name]
        {
            [Hidden]
            get
            {
                return this.GetMember(name);
            }
            [Hidden]
            set
            {
                this.GetMember(name, true, true).Assign(value);
            }
        }

        [Hidden]
        public object Value
        {
            [Hidden]
            get
            {
                switch (valueType)
                {
                    case JSObjectType.Bool:
                        return iValue != 0;
                    case JSObjectType.Int:
                        return iValue;
                    case JSObjectType.Double:
                        return dValue;
                    case JSObjectType.String:
                    case JSObjectType.Object:
                    case JSObjectType.Function:
                    case JSObjectType.Property:
                    case JSObjectType.Date:
                        return oValue;
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                    default:
                        return null;
                }
            }
        }

        [Hidden]
        public JSObjectType ValueType
        {
            [Hidden]
            get
            {
                return valueType;
            }
        }

        [Hidden]
        public JSObjectAttributes Attributes
        {
            [Hidden]
            get
            {
                return attributes;
            }
        }

        [Hidden]
        public JSObject()
        {
            valueType = JSObjectType.Undefined;
        }

        [Hidden]
        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        [DoNotEnumerate]
        public static JSObject create(JSObject args)
        {
            var proto = args["0"];
            if (proto.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Prototype may be only Object or null.")));
            var res = CreateObject();
            var members = args["1"];
            if (members.valueType > JSObjectType.Undefined)
            {
                if (members.valueType < JSObjectType.Object)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Properties descriptor may be only Object.")));
                foreach (var member in members)
                {
                    var desc = members[member];
                    var value = desc["value"];
                    var configurable = desc["configurable"];
                    var enumerable = desc["enumerable"];
                    var writable = desc["writable"];
                    var get = desc["get"];
                    var set = desc["set"];
                    if (value.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and default value.")));
                    if (writable.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and writable attribute.")));
                    string name = args.GetMember("1").ToString();
                    JSObject obj = new JSObject();
                    res.fields[name] = obj;
                    obj.attributes |= JSObjectAttributes.DoNotEnum;
                    obj.attributes |= JSObjectAttributes.Immutable | JSObjectAttributes.DoNotDelete;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSObjectAttributes.DoNotEnum;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSObjectAttributes.Immutable | JSObjectAttributes.DoNotDelete);
                    if (value.valueType > JSObjectType.Undefined)
                    {
                        obj.Assign(value);
                        if (!(bool)writable)
                            obj.attributes |= JSObjectAttributes.ReadOnly;
                    }
                    else if (get.valueType != JSObjectType.NotExistInObject
                          || set.valueType != JSObjectType.NotExistInObject)
                    {
                        if (get.valueType > JSObjectType.Undefined
                            && get.valueType != JSObjectType.Function)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Getter mast be a function.")));
                        if (set.valueType > JSObjectType.Undefined
                            && set.valueType != JSObjectType.Function)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Setter mast be a function.")));
                        obj.valueType = JSObjectType.Property;
                        obj.oValue = new Function[]
                        {
                            set.valueType > JSObjectType.Undefined ? set.oValue as Function : null,
                            get.valueType > JSObjectType.Undefined ? get.oValue as Function : null
                        };
                    }
                    else if (!(bool)writable) // На тот случай, когда в дескрипторе не указано ни значение, ни геттер/сеттер
                        obj.attributes |= JSObjectAttributes.ReadOnly;
                }
            }
            return res;
        }

        [Hidden]
        public static JSObject CreateObject()
        {
            var t = new JSObject(true)
            {
                valueType = JSObjectType.Object
            };
            t.oValue = t;
            return t;
        }

        [DoNotEnumerate]
        public static JSObject defineProperty(JSObject args)
        {
            var obj = args.GetMember("0");
            if (obj.valueType <= JSObjectType.Undefined)
                return undefined;
            var desc = args["2"];
            var value = desc["value"];
            var configurable = desc["configurable"];
            var enumerable = desc["enumerable"];
            var writable = desc["writable"];
            var get = desc["get"];
            var set = desc["set"];
            if (value.valueType != JSObjectType.NotExistInObject
                && (get.valueType != JSObjectType.NotExistInObject
                || set.valueType != JSObjectType.NotExistInObject))
                throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and default value.")));
            if (writable.valueType != JSObjectType.NotExistInObject
                && (get.valueType != JSObjectType.NotExistInObject
                || set.valueType != JSObjectType.NotExistInObject))
                throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and writable attribute.")));
            string name = args.GetMember("1").ToString();
            obj = obj.GetMember(name, true, true);
            if (obj.valueType < JSObjectType.Undefined)
            {
                obj.attributes |= JSObjectAttributes.DoNotEnum;
                obj.attributes |= JSObjectAttributes.Immutable | JSObjectAttributes.DoNotDelete;
            }
            if ((bool)enumerable)
                obj.attributes &= ~JSObjectAttributes.DoNotEnum;
            if ((bool)configurable)
                obj.attributes &= ~(JSObjectAttributes.Immutable | JSObjectAttributes.DoNotDelete);
            if (value.valueType > JSObjectType.Undefined)
            {
                obj.Assign(value);
                if (!(bool)writable)
                    obj.attributes |= JSObjectAttributes.ReadOnly;
            }
            else if (get.valueType != JSObjectType.NotExistInObject
                  || set.valueType != JSObjectType.NotExistInObject)
            {
                if (get.valueType > JSObjectType.Undefined
                    && get.valueType != JSObjectType.Function)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Getter mast be a function.")));
                if (set.valueType > JSObjectType.Undefined
                    && set.valueType != JSObjectType.Function)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Setter mast be a function.")));
                obj.valueType = JSObjectType.Property;
                obj.oValue = new Function[]
                {
                    set.valueType > JSObjectType.Undefined ? set.oValue as Function : null,
                    get.valueType > JSObjectType.Undefined ? get.oValue as Function : null
                };
            }
            else if (!(bool)writable) // На тот случай, когда в дескрипторе не указано ни значение, ни геттер/сеттер
            {
                obj.attributes |= JSObjectAttributes.ReadOnly;
                obj.valueType = JSObjectType.Undefined;
            }
            return obj;
        }

        /// <summary>
        /// Возвращает член объекта с указанным именем.
        /// </summary>
        /// <param name="name">Имя члена.</param>
        /// <returns>Объект, представляющий запрошенный член.</returns>
        [Hidden]
        public JSObject GetMember(string name)
        {
            return GetMember(name, false, false);
        }

        /// <summary>
        /// Возвращает член объекта с указанным именем.
        /// </summary>
        /// <param name="name">Имя члена.</param>
        /// <param name="own">Указывает, следует ли пропускать прототипы объекта при поиске члена</param>
        /// <returns>Объект, представляющий запрошенный член.</returns>
        [Hidden]
        public JSObject GetMember(string name, bool own)
        {
            return GetMember(name, false, own);
        }

        /// <summary>
        /// Возвращает член объекта с указанным именем.
        /// </summary>
        /// <param name="name">Имя члена.</param>
        /// <returns>Объект, представляющий запрошенный член.</returns>
        [Hidden]
        public JSObject DefineMember(string name)
        {
            return GetMember(name, true, true);
        }

        [Hidden]
        internal protected virtual JSObject GetMember(string name, bool createMember, bool own)
        {
            switch (valueType)
            {
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new ReferenceError("Variable not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"undefined\".")));
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.Number));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.Number)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.String:
                    {
                        int index = 0;
                        double dindex = 0.0;
                        if (Tools.ParseNumber(name, index, out dindex, Tools.ParseNumberOptions.Default))
                        {
                            if (dindex > 0.0 && ((index = (int)dindex) == dindex) && oValue.ToString().Length > index)
                                return oValue.ToString()[index];
                            return undefined;
                        }
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.String));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.String)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.Bool:
                    {
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.Boolean));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.Boolean)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        if (oValue == this)
                            break;
                        if (oValue != this && (oValue is JSObject))
                            return (oValue as JSObject).GetMember(name, createMember, own);
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        break;
                    }
                case JSObjectType.Function:
                    {
#if DEBUG
                        if (oValue == this)
                            System.Diagnostics.Debugger.Break();
#endif
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        if (oValue == this)
                            break;
                        return (oValue as JSObject).GetMember(name, createMember, own);
                    }
                case JSObjectType.Property:
                    throw new InvalidOperationException("Try to get member of property");
                default:
                    throw new NotImplementedException();
            }
            return DefaultFieldGetter(name, createMember, own);
        }

        [Hidden]
        protected JSObject DefaultFieldGetter(string name, bool forWrite, bool own)
        {
            switch (name)
            {
                case "__proto__":
                    {
                        forWrite &= (attributes & JSObjectAttributes.Immutable) == 0;
                        if (this == GlobalPrototype)
                        {
                            if (forWrite)
                            {
                                if (__proto__ == null || (__proto__.attributes & JSObjectAttributes.SystemObject) != 0)
                                    return __proto__ = new JSObject();
                                else
                                    return __proto__ ?? Null;
                            }
                            else
                                return __proto__ ?? Null;
                        }
                        else
                        {
                            if (forWrite)
                            {
                                if (__proto__ == null || (__proto__.attributes & JSObjectAttributes.SystemObject) != 0)
                                    return __proto__ = new JSObject();
                                else
                                    return __proto__ ?? GlobalPrototype ?? Null;
                            }
                            else
                                return __proto__ ?? GlobalPrototype ?? Null;
                        }
                    }
                default:
                    {
                        JSObject res = null;
                        var proto = __proto__ ?? GlobalPrototype ?? Null;
                        bool fromProto =
                            (fields == null || !fields.TryGetValue(name, out res) || res.valueType < JSObjectType.Undefined)
                            && (proto != null)
                            && (proto != this)
                            && (!own || (proto is TypeProxy && proto != GlobalPrototype.oValue));
                        if (fromProto)
                        {
                            res = proto.GetMember(name, false, own);
                            if (own && res.valueType != JSObjectType.Property)
                                res = null;
                            else if (res.valueType < JSObjectType.Undefined)
                                res = null;
                        }
                        if (res == null)
                        {
                            if (!forWrite || (attributes & JSObjectAttributes.Immutable) != 0)
                            {
                                notExist.valueType = JSObjectType.NotExistInObject;
                                return notExist;
                            }
                            res = new JSObject()
                            {
                                lastRequestedName = name,
                                valueType = JSObjectType.NotExistInObject
                            };
                            if (fields == null)
                                fields = new Dictionary<string, JSObject>();
                            fields[name] = res;
                        }
                        else if (fromProto && forWrite)
                        {
                            if ((res.attributes & JSObjectAttributes.ReadOnly) == 0 && res.valueType != JSObjectType.Property)
                            {
                                var t = res.Clone() as JSObject;
                                t.lastRequestedName = name;
                                if (fields == null)
                                    fields = new Dictionary<string, JSObject>();
                                fields[name] = t;
                                res = t;
                            }
                        }
                        else
                            res.lastRequestedName = name;
                        if (res.valueType == JSObjectType.NotExist)
                            res.valueType = JSObjectType.NotExistInObject;
                        return res;
                    }
            }
        }

        [Hidden]
        internal JSObject ToPrimitiveValue_Value_String()
        {
            if (valueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetMember("valueOf");
                JSObject res = null;
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetMember("toString");
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

        [Hidden]
        internal JSObject ToPrimitiveValue_String_Value()
        {
            if (valueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetMember("toString");
                JSObject res = null;
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetMember("valueOf");
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public virtual void Assign(JSObject value)
        {
            if (this.assignCallback != null)
                this.assignCallback(this);
            if ((attributes & JSObjectAttributes.ReadOnly) != 0)
                return;
            if (value == this)
                return;
            if (value != null)
            {
                this.valueType = (value.valueType & ~(JSObjectType.NotExistInObject | JSObjectType.NotExist)) | JSObjectType.Undefined;
                this.iValue = value.iValue;
                this.oValue = value.oValue;
                this.dValue = value.dValue;
                this.__proto__ = value.__proto__;
                this.fields = value.fields;
                return;
            }
            this.fields = null;
            this.__proto__ = null;
            this.valueType = JSObjectType.Undefined;
            this.oValue = null;
            this.__proto__ = null;
        }

        [Hidden]
        public virtual object Clone()
        {
            var res = new JSObject();
            res.Assign(this);
            res.attributes = this.attributes & ~JSObjectAttributes.SystemObject;
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType <= JSObjectType.Undefined)
                return "undefined";
            var res = ToPrimitiveValue_String_Value();
            switch (res.valueType)
            {
                case JSObjectType.Bool:
                    return res.iValue != 0 ? "true" : "false";
                case JSObjectType.Int:
                    return res.iValue >= 0 && res.iValue < 16 ? Tools.NumString[res.iValue] : res.iValue.ToString(CultureInfo.InvariantCulture);
                case JSObjectType.Double:
                    return Tools.DoubleToString(res.dValue);
                case JSObjectType.String:
                    return res.oValue as string;
                default:
                    return (res.oValue ?? "null").ToString();
            }
        }

        [Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Hidden]
        public IEnumerator<string> GetEnumerator()
        {
            if (this is JSObject && valueType >= JSObjectType.Object)
            {
                if (oValue != this && oValue is JSObject)
                    return (oValue as JSObject).GetEnumeratorImpl(true);
            }
            return GetEnumeratorImpl(true);
        }

        protected internal virtual IEnumerator<string> GetEnumeratorImpl(bool doNotEnumProcess)
        {
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.valueType >= JSObjectType.Undefined && (!doNotEnumProcess || (f.Value.attributes & JSObjectAttributes.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [Modules.ParametersCount(0)]
        public virtual JSObject toString(JSObject args)
        {
            switch (this.valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return "[object Number]";
                    }
                case JSObjectType.Undefined:
                    {
                        return "[object Undefined]";
                    }
                case JSObjectType.String:
                    {
                        return "[object String]";
                    }
                case JSObjectType.Bool:
                    {
                        return "[object Boolean]";
                    }
                case JSObjectType.Function:
                    {
                        return "[object Function]";
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        if (this.oValue is ThisBind)
                            return this.oValue.ToString();
                        if (this.oValue is TypeProxy)
                        {
                            var ht = (this.oValue as TypeProxy).hostedType;
                            if (ht == typeof(RegExp))
                                return "[object Object]";
                            return "[object " + (ht == typeof(JSObject) ? typeof(System.Object) : ht).Name + "]";
                        }
                        if (this.oValue != null)
                            return "[object " + (this.oValue.GetType() == typeof(JSObject) ? typeof(System.Object) : this.oValue.GetType()).Name + "]";
                        else
                            return "[object Null]";
                    }
                default: throw new NotImplementedException();
            }
        }

        [DoNotEnumerate]
        public virtual JSObject toLocaleString()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on undefined value.")));
            return toString(null);
        }

        [DoNotEnumerate]
        public virtual JSObject valueOf()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("valueOf calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("valueOf calling on undefined value.")));
            if (valueType >= JSObjectType.Object && oValue is JSObject && oValue != this)
                return (oValue as JSObject).valueOf();
            else
                return this;
        }

        [DoNotEnumerate]
        public virtual JSObject isPrototypeOf(JSObject args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("isPrototypeOf calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("isPrototypeOf calling on undefined value.")));
            if (args.GetMember("length").iValue == 0)
                return false;
            var a = args.GetMember("0");
            if (this.valueType >= JSObjectType.Object && this.oValue != null)
            {
                while (a.valueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == this.oValue)
                        return true;
                    if ((a.oValue is TypeProxy) && (a.oValue as TypeProxy).prototypeInstance == this)
                        return true;
                    a = a.GetMember("__proto__");
                }
            }
            return false;
        }

        [DoNotEnumerate]
        public virtual JSObject hasOwnProperty(JSObject args)
        {
            JSObject name = args.GetMember("0");
            string n = "";
            switch (name.valueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        n = "undefined";
                        break;
                    }
                case JSObjectType.Int:
                    {
                        n = name.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    }
                case JSObjectType.Double:
                    {
                        n = Tools.DoubleToString(name.dValue);
                        break;
                    }
                case JSObjectType.String:
                    {
                        n = name.oValue as string;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        args = name.ToPrimitiveValue_Value_String();
                        if (args.valueType == JSObjectType.String)
                            n = name.oValue as string;
                        if (args.valueType == JSObjectType.Int)
                            n = name.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (args.valueType == JSObjectType.Double)
                            n = Tools.DoubleToString(name.dValue);
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = GetMember(n, true);
            res = (res.valueType >= JSObjectType.Undefined);
            return res;
        }

        [DoNotEnumerate]
        public static JSObject preventExtensions(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Prevent the expansion can only for objects")));
            obj.attributes |= JSObjectAttributes.Immutable;
            var res = (obj.oValue as JSObject);
            if (res != null)
                res.attributes |= JSObjectAttributes.Immutable;
            return res;
        }

        [DoNotEnumerate]
        public virtual JSObject propertyIsEnumerable(JSObject args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on undefined value.")));
            JSObject name = args.GetMember("0");
            string n = name.ToString();
            var res = GetMember(n);
            res = (res.valueType >= JSObjectType.Undefined) && ((res.attributes & JSObjectAttributes.DoNotEnum) == 0);
            return res;
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(JSObject args)
        {
            if (args.GetMember("0").valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Parameter isn't an Object.")));
            return args.GetMember("0")["__proto__"];
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyDescriptor(JSObject args)
        {
            var obj = args.GetMember("0");
            if (obj.valueType <= JSObjectType.Undefined)
                return undefined;
            obj = obj.GetMember(args.GetMember("1").ToString(), true);
            var res = CreateObject();
            if (obj.valueType != JSObjectType.Property)
            {
                res["value"] = obj;
                res["writable"] = obj.valueType < JSObjectType.Undefined || (obj.attributes & JSObjectAttributes.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj.oValue as Function[])[0];
                res["get"] = (obj.oValue as Function[])[1];
            }
            res["configurable"] = ((obj.attributes & JSObjectAttributes.Immutable) == 0 && (obj.valueType >= JSObjectType.Object)) || (obj.attributes & JSObjectAttributes.DoNotDelete) == 0;
            res["enumerable"] = (obj.attributes & JSObjectAttributes.DoNotEnum) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(JSObject args)
        {
            var obj = args.GetMember("0");
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            return new BaseTypes.Array((obj.oValue as JSObject).GetEnumeratorImpl(false));
        }

        [Hidden]
        public static implicit operator JSObject(char value)
        {
            return new BaseTypes.String(value.ToString());
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator JSObject(bool value)
        {
            return (BaseTypes.Boolean)value;
        }

        [Hidden]
        public static implicit operator JSObject(int value)
        {
            return (BaseTypes.Number)value;
        }

        [Hidden]
        public static implicit operator JSObject(long value)
        {
            return (BaseTypes.Number)(double)value;
        }

        [Hidden]
        public static implicit operator JSObject(double value)
        {
            return (BaseTypes.Number)value;
        }

        [Hidden]
        public static implicit operator JSObject(string value)
        {
            if (string.IsNullOrEmpty(value))
                return BaseTypes.String.EmptyString;
            return new BaseTypes.String(value);
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static explicit operator bool(JSObject obj)
        {
            var vt = obj.valueType;
            switch (vt)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    return obj.iValue != 0;
                case JSObjectType.Double:
                    return obj.dValue != 0.0 && !double.IsNaN(obj.dValue);
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                    return obj.oValue != null;
                case JSObjectType.String:
                    return !string.IsNullOrEmpty(obj.oValue as string);
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return false;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
