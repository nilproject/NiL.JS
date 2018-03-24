using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    public class JSObject : JSValue
    {
        internal IDictionary<string, JSValue> _fields;
        internal IDictionary<Symbol, JSValue> _symbols;
        internal JSObject _objectPrototype;

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        [CLSCompliant(false)]
        public sealed override JSObject __proto__
        {
            [Hidden]
            get
            {
                if (!Defined || IsNull)
                    ExceptionHelper.Throw(new TypeError("Can not get prototype of null or undefined"));

                if (_valueType >= JSValueType.Object && _oValue != this)
                {
                    var oValueAsJs = _oValue as JSValue;
                    if (oValueAsJs != null)
                    {
                        return oValueAsJs.__proto__;
                    }
                }

                if (_objectPrototype == null || _objectPrototype._valueType < JSValueType.Object)
                {
                    _objectPrototype = GetDefaultPrototype();
                    if (_objectPrototype == null)
                        _objectPrototype = @null;
                }

                if (_objectPrototype._oValue == null)
                    return @null;

                return _objectPrototype;
            }
            [Hidden]
            set
            {
                if ((_attributes & JSValueAttributesInternal.Immutable) != 0)
                    return;
                if (_valueType < JSValueType.Object)
                    return;
                if (value != null && value._valueType < JSValueType.Object)
                    return;

                if (_oValue != this && (_oValue as JSObject) != null)
                {
                    (_oValue as JSObject).__proto__ = value;
                    _objectPrototype = null;
                    return;
                }

                if (value == null || value._oValue == null)
                {
                    _objectPrototype = @null;
                }
                else
                {
                    var c = value._oValue as JSObject ?? value;
                    while (c != null && c != @null && c._valueType > JSValueType.Undefined)
                    {
                        if (c == this || c._oValue == this)
                            ExceptionHelper.Throw(new Error("Try to set cyclic __proto__ value."));
                        c = c.__proto__;
                    }

                    _objectPrototype = value._oValue as JSObject ?? value;
                }
            }
        }

        [Hidden]
        protected internal JSObject()
        {
            /// На будущее. Наверное, нужно будет сделать переходную версию,
            /// но я пока не знаю как это сделать получше.
            /*
            valueType = JSValueType.Object;
            oValue = this;
            */
        }

        [Hidden]
        public static JSObject CreateObject()
        {
            return CreateObject(false);
        }

        internal static JSObject CreateObject(bool createFields = false, JSAttributes attributes = JSAttributes.None)
        {
            var t = new JSObject()
            {
                _valueType = JSValueType.Object
            };

            t._oValue = t;
            t._attributes = (JSValueAttributesInternal)attributes;

            if (createFields)
                t._fields = getFieldsContainer();

            return t;
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(_oValue == this || !(_oValue is JSValue), "Look out!");
#endif
            JSValue res = null;
            JSObject proto = null;
            bool fromProto = false;
            string name = null;
            if (key._valueType == JSValueType.Symbol)
            {
                res = getSymbol(key, forWrite, propertyScope);
            }
            else
            {
                if (forWrite || _fields != null)
                    name = key.ToString();

                fromProto = (propertyScope >= PropertyScope.Super || _fields == null || !_fields.TryGetValue(name, out res) || res._valueType < JSValueType.Undefined) && ((proto = __proto__)._oValue != null);

                if (fromProto)
                {
                    res = proto.GetProperty(key, false, propertyScope > 0 ? propertyScope - 1 : 0);
                    if (((propertyScope == PropertyScope.Own && ((res._attributes & JSValueAttributesInternal.Field) == 0 || res._valueType != JSValueType.Property)))
                        || res._valueType < JSValueType.Undefined)
                        res = null;
                }

                if (res == null)
                {
                    if (!forWrite || (_attributes & JSValueAttributesInternal.Immutable) != 0)
                    {
                        if (propertyScope != PropertyScope.Own && string.CompareOrdinal(name, "__proto__") == 0)
                            return proto;
                        return notExists;
                    }
                    res = new JSValue { _valueType = JSValueType.NotExistsInObject };
                    if (_fields == null)
                        _fields = getFieldsContainer();
                    _fields[name] = res;
                }
                else if (forWrite)
                {
                    if (((res._attributes & JSValueAttributesInternal.SystemObject) != 0 || fromProto))
                    {
                        if ((res._attributes & JSValueAttributesInternal.ReadOnly) == 0
                            && (res._valueType != JSValueType.Property || propertyScope == PropertyScope.Own))
                        {
                            res = res.CloneImpl(false);
                            if (_fields == null)
                                _fields = getFieldsContainer();
                            _fields[name] = res;
                        }
                    }
                }
            }

            res._valueType |= JSValueType.NotExistsInObject;
            return res;
        }

        private JSValue getSymbol(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            JSObject proto = null;
            JSValue res = null;
            var symbol = key._oValue as Symbol;

            var fromProto = (_symbols == null
                            || !_symbols.TryGetValue(symbol, out res)
                            || res._valueType < JSValueType.Undefined)
                                  && ((proto = __proto__)._oValue != null);
            if (fromProto)
            {
                res = proto.GetProperty(key, false, memberScope);
                if ((memberScope == PropertyScope.Own && ((res._attributes & JSValueAttributesInternal.Field) == 0 || res._valueType != JSValueType.Property)) || res._valueType < JSValueType.Undefined)
                    res = null;
            }

            if (res == null)
            {
                if (!forWrite || (_attributes & JSValueAttributesInternal.Immutable) != 0)
                    return notExists;

                res = new JSValue { _valueType = JSValueType.NotExistsInObject };
                if (_symbols == null)
                    _symbols = new Dictionary<Symbol, JSValue>();
                _symbols[symbol] = res;
            }
            else if (forWrite)
            {
                if ((res._attributes & JSValueAttributesInternal.SystemObject) != 0 || fromProto)
                {
                    if ((res._attributes & JSValueAttributesInternal.ReadOnly) == 0
                        && (res._valueType != JSValueType.Property || memberScope == PropertyScope.Own))
                    {
                        res = res.CloneImpl(false);
                        if (_symbols == null)
                            _symbols = new Dictionary<Symbol, JSValue>();
                        _symbols[symbol] = res;
                    }
                }
            }
            return res;
        }

        protected internal override void SetProperty(JSValue key, JSValue value, PropertyScope propertyScope, bool throwOnError)
        {
            JSValue field;
            if (_valueType >= JSValueType.Object && _oValue != this)
            {
                if (_oValue == null)
                    ExceptionHelper.Throw(new TypeError("Can not get property \"" + key + "\" of \"null\""));

                field = _oValue as JSObject;
                if (field != null)
                {
                    field.SetProperty(key, value, propertyScope, throwOnError);
                    return;
                }
            }

            field = GetProperty(key, true, PropertyScope.Common);
            if (field._valueType == JSValueType.Property)
            {
                var setter = (field._oValue as PropertyPair).setter;
                if (setter != null)
                    setter.Call(this, new Arguments { value });
                else if (throwOnError)
                    ExceptionHelper.Throw(new TypeError("Can not assign value to readonly property \"" + key + "\""));
                return;
            }
            else if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (throwOnError)
                    ExceptionHelper.Throw(new TypeError("Can not assign value to readonly property \"" + key + "\""));
            }
            else
            {
                field.Assign(value);
            }
        }

        protected internal override bool DeleteProperty(JSValue key)
        {
            JSValue field;
            if (_valueType >= JSValueType.Object && _oValue != this)
            {
                if (_oValue == null)
                    ExceptionHelper.Throw(new TypeError("Can't get property \"" + key + "\" of \"null\""));

                field = _oValue as JSObject;
                if (field != null)
                    return field.DeleteProperty(key);
            }

            string tname = null;
            if (_fields != null
                && _fields.TryGetValue(tname = key.ToString(), out field)
                && (!field.Exists || (field._attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field._attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field._valueType = JSValueType.NotExistsInObject;

                return _fields.Remove(tname);
            }

            field = GetProperty(key, true, PropertyScope.Own);
            if (!field.Exists)
                return true;

            if ((field._attributes & JSValueAttributesInternal.SystemObject) != 0)
                field = GetProperty(key, true, PropertyScope.Own);

            if ((field._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                field._valueType = JSValueType.NotExistsInObject;
                field._oValue = null;
                return true;
            }
            return false;
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            if (_fields != null)
            {
                foreach (var f in _fields)
                {
                    if (f.Value.Exists && (!hideNonEnum || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                        yield return f;
                }
            }

            if (_objectPrototype != null)
            {
                for (var e = _objectPrototype.GetEnumerator(hideNonEnum, EnumerationMode.RequireValues); e.MoveNext();)
                {
                    if (e.Current.Value._valueType >= JSValueType.Undefined
                        && (e.Current.Value._attributes & JSValueAttributesInternal.Field) != 0)
                    {
                        yield return e.Current;
                    }
                }
            }
        }

        [Hidden]
        public sealed override void Assign(JSValue value)
        {
            if ((_attributes & JSValueAttributesInternal.ReadOnly) == 0)
            {
                if (this is GlobalObject)
                    ExceptionHelper.Throw(new NiL.JS.BaseLibrary.ReferenceError("Invalid left-hand side"));
                throw new InvalidOperationException("Try to assign to a non-primitive value");
            }
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSValue> getFieldsContainer()
        {
            // return new Dictionary<string, JSValue>(System.StringComparer.Ordinal);
            return new StringMap<JSValue>();
//#if !PORTABLE
//            return new System.Collections.Concurrent.ConcurrentDictionary<string, JSValue>(StringComparer.Ordinal);
//#else
//            return new Dictionary<string, JSValue>(System.StringComparer.Ordinal);
//#endif
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static JSValue create(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Prototype may be only Object or null."));
            var proto = args[0]._oValue as JSObject ?? @null;
            var members = args[1]._oValue as JSObject ?? @null;
            if (args[1]._valueType >= JSValueType.Object && members._oValue == null)
                ExceptionHelper.Throw(new TypeError("Properties descriptor may be only Object."));
            var res = CreateObject(true);
            if (proto._valueType >= JSValueType.Object)
                res._objectPrototype = proto;
            if (members._valueType >= JSValueType.Object)
            {
                foreach (var item in members)
                {
                    var desc = item.Value;
                    if (desc._valueType == JSValueType.Property)
                    {
                        var getter = (desc._oValue as PropertyPair).getter;
                        if (getter == null || getter._oValue == null)
                            ExceptionHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                        desc = (getter._oValue as Function).Call(members, null);
                    }
                    if (desc._valueType < JSValueType.Object || desc._oValue == null)
                        ExceptionHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                    var value = desc["value"];
                    if (value._valueType == JSValueType.Property)
                        value = Tools.InvokeGetter(value, desc);

                    var configurable = desc["configurable"];
                    if (configurable._valueType == JSValueType.Property)
                        configurable = Tools.InvokeGetter(configurable, desc);

                    var enumerable = desc["enumerable"];
                    if (enumerable._valueType == JSValueType.Property)
                        enumerable = Tools.InvokeGetter(enumerable, desc);

                    var writable = desc["writable"];
                    if (writable._valueType == JSValueType.Property)
                        writable = Tools.InvokeGetter(writable, desc);

                    var get = desc["get"];
                    if (get._valueType == JSValueType.Property)
                        get = Tools.InvokeGetter(get, desc);

                    var set = desc["set"];
                    if (set._valueType == JSValueType.Property)
                        set = Tools.InvokeGetter(set, desc);

                    if (value.Exists && (get.Exists || set.Exists))
                        ExceptionHelper.Throw(new TypeError("Property can not have getter or setter and default value."));
                    if (writable.Exists && (get.Exists || set.Exists))
                        ExceptionHelper.Throw(new TypeError("Property can not have getter or setter and writable attribute."));
                    if (get.Defined && get._valueType != JSValueType.Function)
                        ExceptionHelper.Throw(new TypeError("Getter mast be a function."));
                    if (set.Defined && set._valueType != JSValueType.Function)
                        ExceptionHelper.Throw(new TypeError("Setter mast be a function."));
                    var obj = new JSValue() { _valueType = JSValueType.Undefined };
                    res._fields[item.Key] = obj;
                    obj._attributes |=
                          JSValueAttributesInternal.DoNotEnumerate
                        | JSValueAttributesInternal.NonConfigurable
                        | JSValueAttributesInternal.DoNotDelete
                        | JSValueAttributesInternal.ReadOnly;
                    if ((bool)enumerable)
                        obj._attributes &= ~JSValueAttributesInternal.DoNotEnumerate;
                    if ((bool)configurable)
                        obj._attributes &= ~(JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete);
                    if (value.Exists)
                    {
                        var atr = obj._attributes;
                        obj._attributes = 0;
                        obj.Assign(value);
                        obj._attributes = atr;
                        if ((bool)writable)
                            obj._attributes &= ~JSValueAttributesInternal.ReadOnly;
                    }
                    else if (get.Exists || set.Exists)
                    {
                        Function setter = null, getter = null;
                        if (obj._valueType == JSValueType.Property)
                        {
                            setter = (obj._oValue as PropertyPair).setter;
                            getter = (obj._oValue as PropertyPair).getter;
                        }
                        obj._valueType = JSValueType.Property;
                        obj._oValue = new PropertyPair
                        {
                            setter = set.Exists ? set._oValue as Function : setter,
                            getter = get.Exists ? get._oValue as Function : getter
                        };
                    }
                    else if ((bool)writable)
                        obj._attributes &= ~JSValueAttributesInternal.ReadOnly;
                }
            }
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        public static JSValue defineProperties(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Property define may only for Objects."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Can not define properties of null."));
            var target = args[0]._oValue as JSObject ?? @null;
            var members = args[1]._oValue as JSObject ?? @null;
            if (!args[1].Defined)
                ExceptionHelper.Throw(new TypeError("Properties descriptor can not be undefined."));
            if (args[1]._valueType < JSValueType.Object)
                return target;
            if (members._oValue == null)
                ExceptionHelper.Throw(new TypeError("Properties descriptor can not be null."));
            if (target._valueType < JSValueType.Object || target._oValue == null)
                return target;
            if (members._valueType > JSValueType.Undefined)
            {
                foreach (var item in members)
                {
                    var desc = item.Value;
                    if (desc._valueType == JSValueType.Property)
                    {
                        var getter = (desc._oValue as PropertyPair).getter;
                        if (getter == null || getter._oValue == null)
                            ExceptionHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                        desc = (getter._oValue as Function).Call(members, null);
                    }

                    if (desc._valueType < JSValueType.Object || desc._oValue == null)
                        ExceptionHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));

                    definePropertyImpl(target, desc._oValue as JSObject, item.Key);
                }
            }
            return target;
        }

        [DoNotEnumerate]
        [ArgumentsCount(3)]
        [CLSCompliant(false)]
        public static JSValue defineProperty(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object || args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.defineProperty cannot apply to non-object."));
            var target = args[0]._oValue as JSObject ?? @null;
            var desc = args[2]._oValue as JSObject ?? @null;
            if (desc._valueType < JSValueType.Object || desc._oValue == null)
                ExceptionHelper.Throw(new TypeError("Invalid property descriptor."));
            if (target._valueType < JSValueType.Object || target._oValue == null)
                return target;
            if (target is Proxy)
                target = (target as Proxy).PrototypeInstance ?? target;

            string memberName = args[1].ToString();
            return definePropertyImpl(target, desc, memberName);
        }

        private static JSObject definePropertyImpl(JSObject target, JSObject desc, string memberName)
        {
            var value = desc["value"];
            if (value._valueType == JSValueType.Property)
                value = Tools.InvokeGetter(value, desc);

            var configurable = desc["configurable"];
            if (configurable._valueType == JSValueType.Property)
                configurable = Tools.InvokeGetter(configurable, desc);

            var enumerable = desc["enumerable"];
            if (enumerable._valueType == JSValueType.Property)
                enumerable = Tools.InvokeGetter(enumerable, desc);

            var writable = desc["writable"];
            if (writable._valueType == JSValueType.Property)
                writable = Tools.InvokeGetter(writable, desc);

            var get = desc["get"];
            if (get._valueType == JSValueType.Property)
                get = Tools.InvokeGetter(get, desc);

            var set = desc["set"];
            if (set._valueType == JSValueType.Property)
                set = Tools.InvokeGetter(set, desc);

            if (value.Exists && (get.Exists || set.Exists))
                ExceptionHelper.Throw(new TypeError("Property can not have getter or setter and default value."));
            if (writable.Exists && (get.Exists || set.Exists))
                ExceptionHelper.Throw(new TypeError("Property can not have getter or setter and writable attribute."));
            if (get.Defined && get._valueType != JSValueType.Function)
                ExceptionHelper.Throw(new TypeError("Getter mast be a function."));
            if (set.Defined && set._valueType != JSValueType.Function)
                ExceptionHelper.Throw(new TypeError("Setter mast be a function."));

            JSValue obj = null;
            obj = target.DefineProperty(memberName);
            if ((obj._attributes & JSValueAttributesInternal.Argument) != 0 && (set.Exists || get.Exists))
            {
                var ti = 0;
                if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                    (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                else
                    target._fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                obj._attributes &= ~JSValueAttributesInternal.Argument;
            }

            if ((obj._attributes & JSValueAttributesInternal.SystemObject) != 0)
                ExceptionHelper.Throw(new TypeError("Can not define property \"" + memberName + "\". Object immutable."));

            if (target is BaseLibrary.Array)
            {
                if (memberName == "length")
                {
                    try
                    {
                        if (value.Exists)
                        {
                            var nlenD = Tools.JSObjectToDouble(value);
                            var nlen = (uint)nlenD;
                            if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                                ExceptionHelper.Throw(new RangeError("Invalid array length"));

                            if ((obj._attributes & JSValueAttributesInternal.ReadOnly) != 0
                                && ((obj._valueType == JSValueType.Double && nlenD != obj._dValue)
                                    || (obj._valueType == JSValueType.Integer && nlen != obj._iValue)))
                                ExceptionHelper.Throw(new TypeError("Cannot change length of fixed size array"));

                            if (!(target as BaseLibrary.Array).SetLenght(nlen))
                                ExceptionHelper.Throw(new TypeError("Unable to reduce length because Exists not configurable elements"));

                            value = notExists;
                        }
                    }
                    finally
                    {
                        if (writable.Exists && !(bool)writable)
                            obj._attributes |= JSValueAttributesInternal.ReadOnly;
                    }
                }
            }

            var newProp = obj._valueType < JSValueType.Undefined;
            var config = (obj._attributes & JSValueAttributesInternal.NonConfigurable) == 0 || newProp;

            if (!config)
            {
                if (enumerable.Exists && (obj._attributes & JSValueAttributesInternal.DoNotEnumerate) != 0 == (bool)enumerable)
                    ExceptionHelper.Throw(new TypeError("Cannot change enumerable attribute for non configurable property."));

                if (writable.Exists && (obj._attributes & JSValueAttributesInternal.ReadOnly) != 0 && (bool)writable)
                    ExceptionHelper.Throw(new TypeError("Cannot change writable attribute for non configurable property."));

                if (configurable.Exists && (bool)configurable)
                    ExceptionHelper.Throw(new TypeError("Cannot set configurable attribute to true."));

                if ((obj._valueType != JSValueType.Property || ((obj._attributes & JSValueAttributesInternal.Field) != 0)) && (set.Exists || get.Exists))
                    ExceptionHelper.Throw(new TypeError("Cannot redefine not configurable property from immediate value to accessor property"));

                if (obj._valueType == JSValueType.Property && (obj._attributes & JSValueAttributesInternal.Field) == 0 && value.Exists)
                    ExceptionHelper.Throw(new TypeError("Cannot redefine not configurable property from accessor property to immediate value"));

                if (obj._valueType == JSValueType.Property && (obj._attributes & JSValueAttributesInternal.Field) == 0
                    && set.Exists
                    && (((obj._oValue as PropertyPair).setter != null && (obj._oValue as PropertyPair).setter._oValue != set._oValue)
                        || ((obj._oValue as PropertyPair).setter == null && set.Defined)))
                    ExceptionHelper.Throw(new TypeError("Cannot redefine setter of not configurable property."));

                if (obj._valueType == JSValueType.Property && (obj._attributes & JSValueAttributesInternal.Field) == 0
                    && get.Exists
                    && (((obj._oValue as PropertyPair).getter != null && (obj._oValue as PropertyPair).getter._oValue != get._oValue)
                        || ((obj._oValue as PropertyPair).getter == null && get.Defined)))
                    ExceptionHelper.Throw(new TypeError("Cannot redefine getter of not configurable property."));
            }

            if (value.Exists)
            {
                if (!config
                    && (obj._attributes & JSValueAttributesInternal.ReadOnly) != 0
                    && !((StrictEqual.Check(obj, value) && ((obj._valueType == JSValueType.Undefined && value._valueType == JSValueType.Undefined) || !obj.IsNumber || !value.IsNumber || (1.0 / Tools.JSObjectToDouble(obj) == 1.0 / Tools.JSObjectToDouble(value))))
                        || (obj._valueType == JSValueType.Double && value._valueType == JSValueType.Double && double.IsNaN(obj._dValue) && double.IsNaN(value._dValue))))
                    ExceptionHelper.Throw(new TypeError("Cannot change value of not configurable not writable peoperty."));
                //if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0 || obj.valueType == JSObjectType.Property)
                {
                    obj._valueType = JSValueType.Undefined;
                    var atrbts = obj._attributes;
                    obj._attributes = 0;
                    obj.Assign(value);
                    obj._attributes = atrbts;
                }
            }
            else if (get.Exists || set.Exists)
            {
                Function setter = null, getter = null;
                if (obj._valueType == JSValueType.Property)
                {
                    setter = (obj._oValue as PropertyPair).setter;
                    getter = (obj._oValue as PropertyPair).getter;
                }
                obj._valueType = JSValueType.Property;
                obj._oValue = new PropertyPair
                {
                    setter = set.Exists ? set._oValue as Function : setter,
                    getter = get.Exists ? get._oValue as Function : getter
                };
            }
            else if (newProp)
                obj._valueType = JSValueType.Undefined;
            if (newProp)
            {
                obj._attributes |=
                    JSValueAttributesInternal.DoNotEnumerate
                    | JSValueAttributesInternal.DoNotDelete
                    | JSValueAttributesInternal.NonConfigurable
                    | JSValueAttributesInternal.ReadOnly;
            }
            else
            {
                var atrbts = obj._attributes;
                if (configurable.Exists && (config || !(bool)configurable))
                    obj._attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete;
                if (enumerable.Exists && (config || !(bool)enumerable))
                    obj._attributes |= JSValueAttributesInternal.DoNotEnumerate;
                if (writable.Exists && (config || !(bool)writable))
                    obj._attributes |= JSValueAttributesInternal.ReadOnly;

                if (obj._attributes != atrbts && (obj._attributes & JSValueAttributesInternal.Argument) != 0)
                {
                    var ti = 0;
                    if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                        (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                    else
                        target._fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                }
            }

            if (config)
            {
                if ((bool)enumerable)
                    obj._attributes &= ~JSValueAttributesInternal.DoNotEnumerate;
                if ((bool)configurable)
                    obj._attributes &= ~(JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete);
                if ((bool)writable)
                    obj._attributes &= ~JSValueAttributesInternal.ReadOnly;
            }
            return target;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineGetter__(Arguments args)
        {
            if (args.length < 2)
                ExceptionHelper.Throw(new TypeError("Missed parameters"));
            if (args[1]._valueType != JSValueType.Function)
                ExceptionHelper.Throw(new TypeError("Expecting function as second parameter"));
            var field = GetProperty(args[0], true, PropertyScope.Own);
            if ((field._attributes & JSValueAttributesInternal.NonConfigurable) != 0)
                ExceptionHelper.Throw(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0)
                ExceptionHelper.Throw(new TypeError("Cannot change value of readonly peoperty."));

            if (field._valueType == JSValueType.Property)
            {
                (field._oValue as PropertyPair).getter = args[1].Value as Function;
            }
            else
            {
                field._valueType = JSValueType.Property;
                field._oValue = new PropertyPair
                {
                    getter = args[1].Value as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineSetter__(Arguments args)
        {
            if (args.length < 2)
                ExceptionHelper.Throw(new TypeError("Missed parameters"));
            if (args[1]._valueType != JSValueType.Function)
                ExceptionHelper.Throw(new TypeError("Expecting function as second parameter"));
            var field = GetProperty(args[0], true, PropertyScope.Own);
            if ((field._attributes & JSValueAttributesInternal.NonConfigurable) != 0)
                ExceptionHelper.Throw(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0)
                ExceptionHelper.Throw(new TypeError("Cannot change value of readonly peoperty."));
            if (field._valueType == JSValueType.Property)
                (field._oValue as PropertyPair).setter = args[1]._oValue as Function;
            else
            {
                field._valueType = JSValueType.Property;
                field._oValue = new PropertyPair
                {
                    setter = args[1].Value as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupGetter__(Arguments args)
        {
            var field = GetProperty(args[0], false, PropertyScope.Common);
            if (field._valueType == JSValueType.Property)
                return (field._oValue as PropertyPair).getter;
            return null;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupSetter__(Arguments args)
        {
            var field = GetProperty(args[0], false, PropertyScope.Common);
            if (field._valueType == JSValueType.Property)
                return (field._oValue as PropertyPair).getter;
            return null;
        }

        [DoNotEnumerate]
        public static JSValue freeze(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.freeze called on non-object."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.freeze called on null."));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            obj._attributes |= JSValueAttributesInternal.Immutable;
            for (var e = obj.GetEnumerator(false, EnumerationMode.RequireValuesForWrite); e.MoveNext();)
            {
                var value = e.Current.Value;
                if ((value._attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value._attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue preventExtensions(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Prevent the expansion can only for objects"));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Can not prevent extensions for null"));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            obj._attributes |= JSValueAttributesInternal.Immutable;
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isExtensible(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.isExtensible called on non-object."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.isExtensible called on null."));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            return (obj._attributes & JSValueAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSValue isSealed(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.isSealed called on non-object."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.isSealed called on null."));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            if ((obj._attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is Proxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr._data)
                {
                    if (node != null
                        && node.Exists
                        && node._valueType >= JSValueType.Object && node._oValue != null
                        && (node._attributes & JSValueAttributesInternal.NonConfigurable) == 0)
                        return false;
                }
            }
            if (obj._fields != null)
                foreach (var f in obj._fields)
                {
                    if (f.Value._valueType >= JSValueType.Object && f.Value._oValue != null && (f.Value._attributes & JSValueAttributesInternal.NonConfigurable) == 0)
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject seal(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.seal called on non-object."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.seal called on null."));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            obj._attributes |= JSValueAttributesInternal.Immutable;
            for (var e = obj.GetEnumerator(false, EnumerationMode.RequireValuesForWrite); e.MoveNext();)
            {
                var value = e.Current.Value;
                if ((value._attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value._attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isFrozen(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.isFrozen called on non-object."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Object.isFrozen called on null."));
            var obj = args[0].Value as JSObject ?? args[0]._oValue as JSObject;
            if ((obj._attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is Proxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr._data.DirectOrder)
                {
                    if (node.Value != null && node.Value.Exists &&
                        ((node.Value._attributes & JSValueAttributesInternal.NonConfigurable) == 0
                        || (node.Value._valueType != JSValueType.Property && (node.Value._attributes & JSValueAttributesInternal.ReadOnly) == 0)))
                        return false;
                }
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                {
                    if ((arg[i]._attributes & JSValueAttributesInternal.NonConfigurable) == 0
                            || (arg[i]._valueType != JSValueType.Property && (arg[i]._attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            }
            if (obj._fields != null)
                foreach (var f in obj._fields)
                {
                    if ((f.Value._attributes & JSValueAttributesInternal.NonConfigurable) == 0
                            || (f.Value._valueType != JSValueType.Property && (f.Value._attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Parameter isn't an Object."));
            var res = args[0].__proto__;
            //if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
            //    res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        public static JSValue getOwnPropertyDescriptor(Arguments args)
        {
            if (args[0]._valueType <= JSValueType.Undefined)
                ExceptionHelper.Throw(new TypeError("Object.getOwnPropertyDescriptor called on undefined."));
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.getOwnPropertyDescriptor called on non-object."));
            var source = args[0]._oValue as JSObject ?? @null;
            var obj = source.GetProperty(args[1], false, PropertyScope.Own);
            if (obj._valueType < JSValueType.Undefined)
                return undefined;
            if ((obj._attributes & JSValueAttributesInternal.SystemObject) != 0)
                obj = source.GetProperty(args[1], true, PropertyScope.Own);
            var res = CreateObject();
            if (obj._valueType != JSValueType.Property || (obj._attributes & JSValueAttributesInternal.Field) != 0)
            {
                if (obj._valueType == JSValueType.Property)
                    res["value"] = (obj._oValue as PropertyPair).getter.Call(source, null);
                else
                    res["value"] = obj;
                res["writable"] = obj._valueType < JSValueType.Undefined || (obj._attributes & JSValueAttributesInternal.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj._oValue as PropertyPair).setter;
                res["get"] = (obj._oValue as PropertyPair).getter;
            }
            res["configurable"] = (obj._attributes & JSValueAttributesInternal.NonConfigurable) == 0 || (obj._attributes & JSValueAttributesInternal.DoNotDelete) == 0;
            res["enumerable"] = (obj._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Cannot get property names of null"));
            var obj = args[0]._oValue as JSObject;

            var result = new BaseLibrary.Array();
            for (var e = obj.GetEnumerator(false, EnumerationMode.KeysOnly); e.MoveNext();)
                result.Add(e.Current.Key);

            return result;
        }

        [DoNotEnumerate]
        public static JSObject keys(Arguments args)
        {
            if (args[0]._valueType < JSValueType.Object)
                ExceptionHelper.Throw(new TypeError("Object.keys called on non-object value."));
            if (args[0]._oValue == null)
                ExceptionHelper.Throw(new TypeError("Cannot get property names of null"));
            var obj = args[0]._oValue as JSObject;

            var result = new BaseLibrary.Array();
            for (var e = obj.GetEnumerator(true, EnumerationMode.KeysOnly); e.MoveNext();)
                result.Add(e.Current.Key);

            return result;
        }

        public static bool @is(JSValue value1, JSValue value2)
        {
            if (value1 == value2)
                return true;

            if ((value1 != null && value2 == null) || (value1 == null && value2 != null))
                return false;

            if ((value1._valueType | JSValueType.Undefined) != (value2._valueType | JSValueType.Undefined))
                return false;

            if (value1._valueType == JSValueType.Double
                && double.IsNaN(value1._dValue)
                && double.IsNaN(value2._dValue))
                return true;

            return StrictEqual.Check(value1, value2);
        }

        public static BaseLibrary.Array getOwnPropertySymbols(JSObject obj)
        {
            return new BaseLibrary.Array(obj?._symbols.Keys ?? new Symbol[0]);
        }

        [JavaScriptName("assign")]
        public static JSValue JSAssign(Arguments args)
        {
            if (args.length == 0 || !args[0].Defined)
                ExceptionHelper.ThrowTypeError("Cannot convert undefined or null to object");

            var target = args[0].ToObject();
            for (var i = 1; i < args.length; i++)
            {
                var enumerator = args[i].GetEnumerator(true, EnumerationMode.RequireValues);
                while (enumerator.MoveNext())
                {
                    target.SetProperty(enumerator.Current.Key, enumerator.Current.Value, PropertyScope.Own, true);
                }
            }

            return target;
        }
    }
}
