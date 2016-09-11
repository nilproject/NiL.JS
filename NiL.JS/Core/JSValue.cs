using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

#if !(PORTABLE || NETCORE)
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum PropertyScope
    {
        Сommon = 0,
        Own = 1,
        Super = 2,
        SuperProto = 3
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum JSValueType
    {
        NotExists = 0,
        NotExistsInObject = 1,
        Undefined = 3,                          // 000000000011 // значение undefined говорит о том, что этот объект, вообще-то, определён, но вот его значение нет
        Boolean = 4 | Undefined,                // 000000000111
        Integer = 8 | Undefined,                // 000000001011
        Double = 16 | Undefined,                // 000000010011
        String = 32 | Undefined,                // 000000100011
        Symbol = 64 | Undefined,                // 000001000011
        Object = 128 | Undefined,               // 000010000011
        Function = 256 | Undefined,             // 000100000011
        Date = 512 | Undefined,                 // 001000000011
        Property = 1024 | Undefined,            // 010000000011
        SpreadOperatorResult = 2048 | Undefined // 100000000011
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum EnumerationMode
    {
        KeysOnly = 0,
        RequireValues,
        RequireValuesForWrite
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Flags]
    internal enum JSValueAttributesInternal : uint
    {
        None = 0,
        DoNotEnumerate = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NonConfigurable = 1 << 4,
        Argument = 1 << 16,
        SystemObject = 1 << 17,
        ProxyPrototype = 1 << 18,
        Field = 1 << 19,
        Eval = 1 << 20,
        Temporary = 1 << 21,
        Cloned = 1 << 22,
        Reassign = 1 << 25,
        IntrinsicFunction = 1 << 26,
        ConstructingObject = 1 << 27
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Flags]
    public enum JSAttributes
    {
        None = 0,
        DoNotEnumerate = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NonConfigurable = 1 << 4,
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class JSValue : IEnumerable<KeyValuePair<string, JSValue>>, IComparable<JSValue>
#if !(PORTABLE || NETCORE)
, ICloneable, IConvertible
#endif
    {
        /*
         * Класс выполняет две роли: представляет значения JS и является контейнером значений в свойствах объектов 
         * и переменных в контектсе выполнения.
         * Преймущества от такого подхода существенные: нет необходимости создавать эти самые контейнеры свойств 
         * со своими аттрибутами, нет нужды создавать ворох классов для реализации оператора присваивания, 
         * чтобы поддерживать весь тот букет возможных случаев lvalue. Один JSValue умеет копировать значение 
         * с другого JSValue'а и, если потребуется, переходить в режим посредника, перенапрвляя вызовы GetMember, 
         * SetMember и DeleteMember. Однако есть и недостатки - необходимо указывать, с какой целью запрашивается 
         * значение. В случаях, когда значение запрашивается для записи, необходимо убедиться, что эта операция 
         * не перепишет системные значения. К примеру, в свойстве объекта может находиться значение null. Для оптимизации,
         * это может быть системная константа JSValue.Null, поэтому по запросу значения для записи нужно вернуть 
         * новый объект, которым следует заменить значение свойства в объекте. 
         */

        [Hidden]
        internal static readonly JSValue undefined = new JSValue() { _valueType = JSValueType.Undefined, _attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSValue notExists = new JSValue() { _valueType = JSValueType.NotExists, _attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject @null = new JSObject() { _valueType = JSValueType.Object, _oValue = null, _attributes = JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSValue nullString = new JSValue() { _valueType = JSValueType.String, _oValue = "null", _attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject };

        [Hidden]
        public static JSValue Undefined { [Hidden] get { return undefined; } }
        [Hidden]
        public static JSValue NotExists
        {
            [Hidden]
            get
            {
                notExists._valueType = JSValueType.NotExists;
                return notExists;
            }
        }
        [Hidden]
        public static JSValue NotExistsInObject
        {
            [Hidden]
            get
            {
                notExists._valueType = JSValueType.NotExistsInObject;
                return notExists;
            }
        }
        [Hidden]
        public static JSValue Null { [Hidden] get { return @null; } }

        [Hidden]
        public virtual JSValue this[string name]
        {
            [Hidden]
            get
            {
                return GetProperty(name);
            }
            [Hidden]
            set
            {
                SetProperty(name, value ?? JSValue.undefined, true);
            }
        }

        internal JSValueAttributesInternal _attributes;
        internal JSValueType _valueType;
        internal int _iValue;
        internal double _dValue;
        internal object _oValue;

        [Hidden]
        public virtual object Value
        {
            [Hidden]
            get
            {
                switch (_valueType)
                {
                    case JSValueType.Boolean:
                        return _iValue != 0;
                    case JSValueType.Integer:
                        return _iValue;
                    case JSValueType.Double:
                        return _dValue;
                    case JSValueType.String:
                        return _oValue.ToString();
                    case JSValueType.Symbol:
                        return _oValue;
                    case JSValueType.Object:
                    case JSValueType.Function:
                    case JSValueType.Property:
                    case JSValueType.SpreadOperatorResult:
                    case JSValueType.Date:
                        {
                            if (_oValue != this && _oValue is JSObject)
                                return (_oValue as JSObject).Value;
                            return _oValue;
                        }
                    case JSValueType.Undefined:
                    case JSValueType.NotExistsInObject:
                    default:
                        return null;
                }
            }
            protected set
            {
                switch (_valueType)
                {
                    case JSValueType.Boolean:
                        {
                            _iValue = (bool)value ? 1 : 0;
                            break;
                        }
                    case JSValueType.Integer:
                        {
                            _iValue = (int)value;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            _dValue = (double)value;
                            break;
                        }
                    case JSValueType.String:
                        {
                            _oValue = (string)value;
                            break;
                        }
                    case JSValueType.Object:
                    case JSValueType.Function:
                    case JSValueType.Property:
                    case JSValueType.Date:
                        {
                            _oValue = value;
                            break;
                        }
                    case JSValueType.Undefined:
                    case JSValueType.NotExistsInObject:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        [Hidden]
        public JSValueType ValueType
        {
            [Hidden]
            get
            {
                return _valueType;
            }
            protected set
            {
                _valueType = value;
            }
        }

        [Hidden]
        public JSAttributes Attributes
        {
            [Hidden]
            get
            {
                return (JSAttributes)((int)_attributes & 0xffff);
            }
        }

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        [CLSCompliant(false)]
        public virtual JSObject __proto__
        {
            [Hidden]
            get
            {
                if (_valueType >= JSValueType.Object
                    && _oValue != this
                    && (_oValue as JSObject) != null)
                    return (_oValue as JSObject).__proto__;
                if (!this.Defined || this.IsNull)
                    ExceptionsHelper.Throw(new TypeError("Can not get prototype of null or undefined"));
                return GetDefaultPrototype();
            }
            [Hidden]
            set
            {
                if ((_attributes & JSValueAttributesInternal.Immutable) != 0)
                    return;
                if (_valueType < JSValueType.Object)
                    return;
                if (_oValue == this)
                    throw new InvalidOperationException();
                if (_oValue == null)
                    ExceptionsHelper.Throw(new ReferenceError("Cannot set __proto__ of null"));
                (_oValue as JSObject).__proto__ = value;
            }
        }

        [Hidden]
        public bool Exists
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get
            { return _valueType >= JSValueType.Undefined; }
        }

        [Hidden]
        public bool Defined
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                return _valueType > JSValueType.Undefined;
            }
        }

        [Hidden]
        public bool IsNull
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                return _valueType >= JSValueType.Object && _oValue == null;
            }
        }

        [Hidden]
        public bool IsNumber
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get
            { return _valueType == JSValueType.Integer || _valueType == JSValueType.Double; }
        }

        internal bool NeedClone
        {
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return (_attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.SystemObject)) == JSValueAttributesInternal.SystemObject; }
        }

        internal bool IsBox
        {
            get
            {
                return _valueType >= JSValueType.Object && _oValue != null && _oValue != this;
            }
        }

        internal virtual JSObject GetDefaultPrototype()
        {
            switch (_valueType)
            {
                case JSValueType.Boolean:
                    return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.Boolean));
                case JSValueType.Double:
                case JSValueType.Integer:
                    return TypeProxy.GetPrototype(typeof(Number));
                case JSValueType.String:
                    return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.String));
            }
            if (_oValue != null && _oValue != this)
            {
                var rojso = _oValue as JSValue;
                if (rojso != null)
                    return rojso.GetDefaultPrototype() ?? @null;
                else
                    return TypeProxy.GetPrototype(_oValue.GetType());
            }
            return TypeProxy.GetPrototype(this.GetType());
        }

        [Hidden]
        public JSValue GetProperty(string name)
        {
            return GetProperty((JSValue)name, false, PropertyScope.Сommon);
        }

        [Hidden]
        public JSValue GetProperty(string name, PropertyScope propertyScope)
        {
            return GetProperty((JSValue)name, false, propertyScope);
        }

        [Hidden]
        public JSValue DefineProperty(string name)
        {
            return GetProperty((JSValue)name, true, PropertyScope.Own);
        }

        /// <summary>
        /// Creates new property with Getter and Setter in the object
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="getter">Function called when there is an attempt to get a value. Can be null</param>
        /// <param name="setter">Function called when there is an attempt to set a value. Can be null</param>
        /// <exception cref="System.ArgumentException">if property already exists</exception>
        /// <exception cref="System.InvalidOperationException">if unable to create property</exception>
        [Hidden]
        public void DefineGetSetProperty(string name, Func<object> getter, Action<object> setter)
        {
            var property = GetProperty(name);
            if (property.ValueType >= JSValueType.Undefined)
                throw new ArgumentException();

            property = DefineProperty(name);
            if (property.ValueType < JSValueType.Undefined)
                throw new InvalidOperationException();

            property._valueType = JSValueType.Property;

            Function jsGetter = null;
            if (getter != null)
            {
#if NET40
                jsGetter = new MethodProxy(getter.Method, getter.Target);
#else
                jsGetter = new MethodProxy(getter.GetMethodInfo(), getter.Target);
#endif
            }

            Function jsSetter = null;
            if (setter != null)
            {
#if NET40
                jsSetter = new MethodProxy(setter.Method, setter.Target);
#else
                jsSetter = new MethodProxy(setter.GetMethodInfo(), setter.Target);
#endif
            }

            property._oValue = new GsPropertyPair(jsGetter, jsSetter);            
        }

        [Hidden]
        public bool DeleteProperty(string name)
        {
            if (name == null)
                throw new ArgumentNullException("memberName");
            return DeleteProperty((JSObject)name);
        }

        internal protected JSValue GetProperty(string name, bool forWrite, PropertyScope propertyScope)
        {
            return GetProperty((JSValue)name, forWrite, propertyScope);
        }

        internal protected virtual JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope)
        {
            switch (_valueType)
            {
                case JSValueType.Boolean:
                    {
                        if (propertyScope == PropertyScope.Own)
                            return notExists;
                        forWrite = false;
                        return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.Boolean)).GetProperty(key, false, PropertyScope.Сommon);
                    }
                case JSValueType.Integer:
                case JSValueType.Double:
                    {
                        if (propertyScope == PropertyScope.Own)
                            return notExists;
                        forWrite = false;
                        return TypeProxy.GetPrototype(typeof(Number)).GetProperty(key, false, PropertyScope.Сommon);
                    }
                case JSValueType.String:
                    {
                        return stringGetProperty(key, forWrite, propertyScope);
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExists:
                case JSValueType.NotExistsInObject:
                    {
                        ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToGetProperty, key, "undefined"));
                        return null;
                    }
                default:
                    {
                        if (_oValue == this)
                            break;
                        if (_oValue == null)
                            ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToGetProperty, key, "null"));
                        var inObj = _oValue as JSObject;
                        if (inObj != null)
                            return inObj.GetProperty(key, forWrite, propertyScope);
                        break;
                    }
            }
            ExceptionsHelper.Throw(new InvalidOperationException("Method GetProperty(...) of custom types must be overriden"));
            return null;
        }

        private JSValue stringGetProperty(JSValue name, bool forWrite, PropertyScope propertyScope)
        {
            if ((name._valueType == JSValueType.String || name._valueType >= JSValueType.Object)
                && string.CompareOrdinal(name._oValue.ToString(), "length") == 0)
                return _oValue.ToString().Length;

            double dindex = 0.0;
            int index = 0;
            dindex = Tools.JSObjectToDouble(name);

            if (dindex >= 0.0
                && ((index = (int)dindex) == dindex)
                && _oValue.ToString().Length > index)
                return _oValue.ToString()[index];

            if (propertyScope == PropertyScope.Own)
                return notExists;

            return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.String)).GetProperty(name, false, PropertyScope.Сommon);
        }

        internal protected void SetProperty(JSValue name, JSValue value, bool throwOnError)
        {
            SetProperty(name, value, PropertyScope.Сommon, throwOnError);
        }

        internal protected virtual void SetProperty(JSValue name, JSValue value, PropertyScope propertyScope, bool throwOnError)
        {
            JSValue field;
            if (_valueType >= JSValueType.Object)
            {
                if (_oValue == null)
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToSetProperty, name, "null"));

                if (_oValue == this)
                {
                    System.Diagnostics.Debug.WriteLine(typeof(JSValue).Name + "." + nameof(SetProperty) + " must be overridden for objects");

                    GetProperty(name, true, propertyScope).Assign(value);
                }

                field = _oValue as JSObject;
                if (field != null)
                {
                    field.SetProperty(name, value, propertyScope, throwOnError);
                    return;
                }
            }
            else if (_valueType <= JSValueType.Undefined)
            {
                ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToSetProperty, name, "undefined"));
            }
        }

        internal protected virtual bool DeleteProperty(JSValue name)
        {
            if (_valueType >= JSValueType.Object)
            {
                if (_oValue == null)
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToGetProperty, name, "null"));
                if (_oValue == this)
                    throw new InvalidOperationException();
                var obj = _oValue as JSObject;
                if (obj != null)
                    return obj.DeleteProperty(name);
            }
            else if (_valueType <= JSValueType.Undefined)
                ExceptionsHelper.ThrowTypeError(string.Format(Strings.TryingToGetProperty, name, "undefined"));
            return true;
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            if (!(obj is JSValue))
                return false;
            if (object.ReferenceEquals(obj, this))
                return true;
            return Expressions.StrictEqual.Check(this, obj as JSValue);
        }

        #region Do not remove

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Hidden]
        public static implicit operator JSValue(char value)
        {
            return new NiL.JS.BaseLibrary.String(value.ToString());
        }
        #endregion

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator JSValue(bool value)
        {
            return (NiL.JS.BaseLibrary.Boolean)value;
        }

        [Hidden]
        public static implicit operator JSValue(int value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSValue(long value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSValue(double value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSValue(string value)
        {
            return new NiL.JS.BaseLibrary.String(value);
        }

        [Hidden]
        public static explicit operator int(JSValue obj)
        {
            return Tools.JSObjectToInt32(obj);
        }

        [Hidden]
        public static explicit operator long(JSValue obj)
        {
            return Tools.JSObjectToInt64(obj);
        }

        [Hidden]
        public static explicit operator double(JSValue obj)
        {
            return Tools.JSObjectToDouble(obj);
        }

        [Hidden]
        public static explicit operator bool(JSValue obj)
        {
            switch (obj._valueType)
            {
                case JSValueType.Integer:
                case JSValueType.Boolean:
                    return obj._iValue != 0;
                case JSValueType.Double:
                    return !(obj._dValue == 0.0 || double.IsNaN(obj._dValue));
                case JSValueType.String:
                    return !string.IsNullOrEmpty(obj._oValue.ToString());
                case JSValueType.Object:
                case JSValueType.Date:
                case JSValueType.Function:
                    return obj._oValue != null;
            }
            return false;
        }

        [Hidden]
        public static implicit operator JSValue(Delegate action) => new MethodProxy(action.GetMethodInfo(), action.Target);

        [Hidden]
        public object Clone()
        {
            return CloneImpl();
        }

        internal JSValue CloneImpl()
        {
            return CloneImpl(true);
        }

        internal JSValue CloneImpl(bool force)
        {
            if (!force && (_attributes & JSValueAttributesInternal.Cloned) != 0)
            {
                _attributes &= ~JSValueAttributesInternal.Cloned;
                return this;
            }

            var res = new JSValue();
            res.Assign(this);
            res._valueType = _valueType;
            res._attributes = this._attributes &
                ~(JSValueAttributesInternal.ReadOnly
                | JSValueAttributesInternal.SystemObject
                | JSValueAttributesInternal.Temporary
                | JSValueAttributesInternal.Reassign
                | JSValueAttributesInternal.ProxyPrototype);
            return res;
        }

        internal virtual JSValue CloneImpl(JSValueAttributesInternal resetMask)
        {
            var res = new JSValue();
            res.Assign(this);
            res._attributes = this._attributes & ~resetMask;
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (_valueType == JSValueType.String)
                return _oValue.ToString();
            if (_valueType <= JSValueType.Undefined)
                return "undefined";
            if (_valueType == JSValueType.Property)
            {
                var tempStr = "[";
                if ((_oValue as GsPropertyPair).get != null)
                    tempStr += "Getter";
                if ((_oValue as GsPropertyPair).set != null)
                    tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
                if (tempStr.Length == 1)
                    return "[Invalid Property]";
                tempStr += "]";
                return tempStr;
            }
            var res = this._valueType >= JSValueType.Object ? ToPrimitiveValue_String_Value() : this;
            switch (res._valueType)
            {
                case JSValueType.Boolean:
                    return res._iValue != 0 ? "true" : "false";
                case JSValueType.Integer:
                    return Tools.Int32ToString(res._iValue);
                case JSValueType.Double:
                    return Tools.DoubleToString(res._dValue);
                case JSValueType.String:
                    return res._oValue.ToString();
                default:
                    return (res._oValue ?? "null").ToString();
            }
        }

        [Hidden]
        public virtual void Assign(JSValue value)
        {
#if DEBUG
            if (_valueType == JSValueType.Property)
                throw new InvalidOperationException("Try to assign to property.");
#endif
            if ((_attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.SystemObject)) != 0)
                return;
            this._valueType = value._valueType | JSValueType.Undefined;
            this._iValue = value._iValue;
            this._dValue = value._dValue;
            this._oValue = value._oValue;
        }

        internal JSValue ToPrimitiveValue_Value_String()
        {
            return ToPrimitiveValue("valueOf", "toString");
        }

        internal JSValue ToPrimitiveValue_LocaleString_Value()
        {
            return ToPrimitiveValue("toLocaleString", "valueOf");
        }

        internal JSValue ToPrimitiveValue_String_Value()
        {
            return ToPrimitiveValue("toString", "valueOf");
        }

        internal JSValue ToPrimitiveValue(string func0, string func1)
        {
            if (_valueType >= JSValueType.Object && _oValue != null)
            {
                if (_oValue == null)
                    return nullString;
                var tpvs = GetProperty(func0);
                JSValue res = null;
                if (tpvs._valueType == JSValueType.Function)
                {
                    res = (tpvs._oValue as NiL.JS.BaseLibrary.Function).Call(this, null);
                    if (res._valueType == JSValueType.Object)
                    {
                        if (res._oValue is NiL.JS.BaseLibrary.String)
                            res = res._oValue as NiL.JS.BaseLibrary.String;
                    }
                    if (res._valueType < JSValueType.Object)
                        return res;
                }
                tpvs = GetProperty(func1);
                if (tpvs._valueType == JSValueType.Function)
                {
                    res = (tpvs._oValue as NiL.JS.BaseLibrary.Function).Call(this, null);
                    if (res._valueType == JSValueType.Object)
                    {
                        if (res._oValue is NiL.JS.BaseLibrary.String)
                            res = res._oValue as NiL.JS.BaseLibrary.String;
                    }
                    if (res._valueType < JSValueType.Object)
                        return res;
                }
                ExceptionsHelper.Throw(new TypeError("Can't convert object to primitive value."));
            }
            return this;
        }

        [Hidden]
        public JSObject ToObject()
        {
            if (_valueType >= JSValueType.Object)
                return _oValue as JSObject;

            if (_valueType >= JSValueType.Undefined)
                return new ObjectWrapper(ToPrimitiveTypeContainer());

            return new JSObject() { _valueType = JSValueType.Object };
        }

        [Hidden]
        public JSValue ToPrimitiveTypeContainer()
        {
            if (_valueType >= JSValueType.Object)
                return null;

            switch (_valueType)
            {
                case JSValueType.Boolean:
                    return this is BaseLibrary.Boolean ? this : new BaseLibrary.Boolean(_iValue != 0);
                case JSValueType.Integer:
                    return this is BaseLibrary.Number ? this : new BaseLibrary.Number(_iValue);
                case JSValueType.Double:
                    return this is BaseLibrary.Number ? this : new BaseLibrary.Number(_dValue);
                case JSValueType.String:
                    return this is BaseLibrary.String ? this : new BaseLibrary.String(_oValue.ToString());
                case JSValueType.Symbol:
                    return _oValue as Symbol;
            }

            return new JSValue() { _valueType = JSValueType.Undefined };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [Hidden]
        public IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator()
        {
            return GetEnumerator(true, EnumerationMode.RequireValuesForWrite);
        }

        protected internal virtual IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumeratorMode)
        {
            if (_valueType >= JSValueType.Object && _oValue != this)
            {
                var innerObject = _oValue as JSValue;
                if (innerObject != null)
                    return innerObject.GetEnumerator(hideNonEnumerable, enumeratorMode);
            }
            return GetEnumeratorImpl(hideNonEnumerable);
        }

        private IEnumerator<KeyValuePair<string, JSValue>> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (_valueType == JSValueType.String)
            {
                var strValue = _oValue.ToString();
                var len = strValue.Length;
                for (var i = 0; i < len; i++)
                    yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), strValue[i].ToString());
                if (!hideNonEnum)
                    yield return new KeyValuePair<string, JSValue>("length", len);
            }
            else if (_valueType == JSValueType.Object)
            {
                if (_oValue == this)
                    throw new InvalidOperationException("Internal error. #VaO");
            }
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ArgumentsLength(0)]
        [AllowNullArguments]
        public virtual JSValue toString(Arguments args)
        {
            var self = this._oValue as JSValue ?? this;
            switch (self._valueType)
            {
                case JSValueType.Integer:
                case JSValueType.Double:
                    {
                        return "[object Number]";
                    }
                case JSValueType.Undefined:
                    {
                        return "[object Undefined]";
                    }
                case JSValueType.String:
                    {
                        return "[object String]";
                    }
                case JSValueType.Boolean:
                    {
                        return "[object Boolean]";
                    }
                case JSValueType.Function:
                    {
                        return "[object Function]";
                    }
                case JSValueType.Date:
                case JSValueType.Object:
                    {
                        if (self._oValue != null)
                        {
                            if (self._oValue is GlobalObject)
                                return self._oValue.ToString();
                            if (self._oValue is TypeProxy)
                            {
                                var ht = (self._oValue as TypeProxy).hostedType;
                                return "[object " + (ht == typeof(JSObject) ? typeof(System.Object) : ht).Name + "]";
                            }
                            return "[object " + (self.Value.GetType() == typeof(JSObject) ? typeof(System.Object) : self.Value.GetType()).Name + "]";
                        }
                        else
                            return "[object Null]";
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        [DoNotEnumerate]
        public virtual JSValue toLocaleString()
        {
            var self = this._oValue as JSValue ?? this;
            if (self._valueType >= JSValueType.Object && self._oValue == null)
                ExceptionsHelper.Throw(new TypeError("toLocaleString calling on null."));
            if (self._valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("toLocaleString calling on undefined value."));
            if (self == this)
                return toString(null);
            return self.toLocaleString();
        }

        [DoNotEnumerate]
        public virtual JSValue valueOf()
        {
            if (_valueType >= JSValueType.Object && _oValue == null)
                ExceptionsHelper.Throw(new TypeError("valueOf calling on null."));
            if (_valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("valueOf calling on undefined value."));
            return _valueType < JSValueType.Object ? new JSObject() { _valueType = JSValueType.Object, _oValue = this } : this;
        }

        [DoNotEnumerate]
        public virtual JSValue propertyIsEnumerable(Arguments args)
        {
            if (_valueType >= JSValueType.Object && _oValue == null)
                ExceptionsHelper.Throw(new TypeError("propertyIsEnumerable calling on null."));
            if (_valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("propertyIsEnumerable calling on undefined value."));
            var name = args[0];
            string n = name.ToString();
            var res = GetProperty(n, PropertyScope.Own);
            res = (res.Exists) && ((res._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0);
            return res;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DoNotEnumerate]
        public virtual JSValue isPrototypeOf(Arguments args)
        {
            if (_valueType >= JSValueType.Object && _oValue == null)
                ExceptionsHelper.Throw(new TypeError("isPrototypeOf calling on null."));
            if (_valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("isPrototypeOf calling on undefined value."));
            if (args.GetProperty("length")._iValue == 0)
                return false;
            var a = args[0];
            a = a.__proto__;
            if (this._valueType >= JSValueType.Object)
            {
                if (this._oValue != null)
                {
                    while (a != null && a._valueType >= JSValueType.Object && a._oValue != null)
                    {
                        if (a._oValue == this._oValue)
                            return true;
                        var pi = (a._oValue is TypeProxy) ? (a._oValue as TypeProxy).prototypeInstance : null;
                        if (pi != null && (this == pi || this == pi._oValue))
                            return true;
                        a = a.__proto__;
                    }
                }
            }
            else
            {
                if (a._oValue == this._oValue)
                    return true;
                var pi = (a._oValue is TypeProxy) ? (a._oValue as TypeProxy).prototypeInstance : null;
                if (pi != null && (this == pi || this == pi._oValue))
                    return true;
            }
            return false;
        }

        [DoNotEnumerate]
        public virtual JSValue hasOwnProperty(Arguments args)
        {
            JSValue name = args[0];
            var res = GetProperty(name, false, PropertyScope.Own);
            return res.Exists;
        }

        #region Члены IConvertible
#if !(PORTABLE || NETCORE)
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return (bool)this;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return (byte)Tools.JSObjectToInt32(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            var s = this.ToString();
            return s.Length > 0 ? s[0] : '\0';
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            if (_valueType == JSValueType.Date)
                return (_oValue as Date).ToDateTime();
            throw new InvalidCastException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return (decimal)Tools.JSObjectToDouble(this);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Tools.JSObjectToDouble(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return (short)Tools.JSObjectToInt32(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            if (_valueType == JSValueType.Integer)
                return _iValue;
            return Tools.JSObjectToInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return (byte)Tools.JSObjectToInt64(this);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return (sbyte)Tools.JSObjectToInt32(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (float)Tools.JSObjectToDouble(this);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return Tools.convertJStoObj(this, conversionType, true);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return (ushort)Tools.JSObjectToInt32(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return (uint)Tools.JSObjectToInt32(this);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return (ulong)Tools.JSObjectToInt64(this);
        }
#endif
        #endregion

        #region Члены IComparable<JSValue>

        [Hidden]
        public virtual int CompareTo(JSValue other)
        {
            if (_valueType == other._valueType)
            {
                switch (_valueType)
                {
                    case JSValueType.Undefined:
                    case JSValueType.NotExists:
                    case JSValueType.NotExistsInObject:
                        return 0;
                    case JSValueType.Boolean:
                    case JSValueType.Integer:
                        return _iValue - other._iValue;
                    case JSValueType.Double:
                        return System.Math.Sign(_dValue - other._dValue);
                    case JSValueType.String:
                        return string.CompareOrdinal(_oValue.ToString(), other._oValue.ToString());
                    default:
                        throw new NotImplementedException("Try to compare two values of " + _valueType);
                }
            }
            else
                throw new InvalidOperationException("Type mismatch");
        }

        #endregion

        public static JSValue Marshal(object value)
        {
            return TypeProxy.Proxy(value);
        }

        public static JSValue Wrap(object value)
        {
            if (value == null)
                return Null;

            return new ObjectWrapper(value);
        }

        public static JSValue GetConstructor(Type type)
        {
            return TypeProxy.GetConstructor(type);
        }

        public static Function GetGenericTypeSelector(IList<Type> types)
        {
            for (var i = 0; i < types.Count; i++)
            {
                for (var j = i + 1; j < types.Count; j++)
                {
                    if (types[i].GetGenericArguments().Length == types[j].GetGenericArguments().Length)
                        ExceptionsHelper.Throw(new InvalidOperationException("Types have the same arguments"));
                }
            }

            return new ExternalFunction((_this, args) =>
            {
                Type type = null;

                for (var i = 0; i < types.Count; i++)
                {
                    if (types[i].GetGenericArguments().Length == args.length)
                    {
                        type = types[i];
                        break;
                    }
                }

                if (type == null)
                    ExceptionsHelper.ThrowTypeError("Invalid arguments count for generic constructor");

                if (args.length == 0)
                    return TypeProxy.GetConstructor(type);

                var parameters = new Type[args.length];
                for (var i = 0; i < args.length; i++)
                {
                    parameters[i] = args[i].As<Type>();
                    if (types[i] == null)
                        ExceptionsHelper.ThrowTypeError("Invalid argument #" + i + " for generic constructor");
                }

                return TypeProxy.GetConstructor(type.MakeGenericType(parameters));
            });
        }
    }
}
