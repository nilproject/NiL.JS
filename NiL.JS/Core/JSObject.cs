using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Linq.Expressions;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using System.ComponentModel;

namespace NiL.JS.Core
{
    public class JSObject : JSValue
    {
        internal static JSObject GlobalPrototype;

        internal IDictionary<string, JSValue> fields;
        internal IDictionary<Symbol, JSValue> symbols;
        internal JSObject __prototype;

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        [CLSCompliant(false)]
        public sealed override JSObject __proto__
        {
            [Hidden]
            get
            {
                if (GlobalPrototype == this)
                    return @null;
                if (!this.Defined || this.IsNull)
                    ExceptionsHelper.Throw(new TypeError("Can not get prototype of null or undefined"));
                if (valueType >= JSValueType.Object
                    && oValue != this // вот такого теперь быть не должно
                    && (oValue as JSObject) != null)
                    return (oValue as JSObject).__proto__;
                if (__prototype != null)
                {
                    if (__prototype.valueType < JSValueType.Object)
                        __prototype = GetDefaultPrototype(); // такого тоже
                    else if (__prototype.oValue == null)
                        return @null;
                    return __prototype;
                }
                return __prototype = GetDefaultPrototype();
            }
            [Hidden]
            set
            {
                if ((attributes & JSValueAttributesInternal.Immutable) != 0)
                    return;
                if (valueType < JSValueType.Object)
                    return;
                if (value != null && value.valueType < JSValueType.Object)
                    return;
                if (oValue != this && (oValue as JSObject) != null)
                {
                    (oValue as JSObject).__proto__ = value;
                    __prototype = null;
                    return;
                }
                if (value == null || value.oValue == null)
                {
                    __prototype = @null;
                }
                else
                {
                    var c = value.oValue as JSObject ?? value;
                    while (c != null && c != @null && c.valueType > JSValueType.Undefined)
                    {
                        if (c == this || c.oValue == this)
                            ExceptionsHelper.Throw(new Error("Try to set cyclic __proto__ value."));
                        c = c.__proto__;
                    }
                    __prototype = value.oValue as JSObject ?? value;
                }
            }
        }

        [Hidden]
        public JSObject()
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
                valueType = JSValueType.Object,
                __prototype = GlobalPrototype,
            };
            t.oValue = t;
            if (createFields)
                t.fields = getFieldsContainer();
            t.attributes = (JSValueAttributesInternal)attributes;
            return t;
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
#if DEBUG
            // Это ошибочная ситуация, но, по крайней мере, так положение будет исправлено
            if (oValue != this && oValue is JSValue)
                return base.GetProperty(key, forWrite, memberScope);
#endif
            JSValue res = null;
            JSObject proto = null;
            bool fromProto = false;
            string name = null;
            if (key.valueType == JSValueType.Symbol)
            {
                res = getSymbol(key, forWrite, memberScope);
            }
            else
            {
                if (forWrite || fields != null)
                    name = key.ToString();
                fromProto = (memberScope >= PropertyScope.Super || fields == null || !fields.TryGetValue(name, out res) || res.valueType < JSValueType.Undefined) && ((proto = __proto__).oValue != null);
                if (fromProto)
                {
                    res = proto.GetProperty(key, false, memberScope > 0 ? (PropertyScope)(memberScope - 1) : 0);
                    if (((memberScope == PropertyScope.Own && ((res.attributes & JSValueAttributesInternal.Field) == 0 || res.valueType != JSValueType.Property)))
                        || res.valueType < JSValueType.Undefined)
                        res = null;
                }
                if (res == null)
                {
                    if (!forWrite || (attributes & JSValueAttributesInternal.Immutable) != 0)
                    {
                        if (memberScope != PropertyScope.Own && string.CompareOrdinal(name, "__proto__") == 0)
                            return proto;
                        return notExists;
                    }
                    res = new JSValue { valueType = JSValueType.NotExistsInObject };
                    if (fields == null)
                        fields = getFieldsContainer();
                    fields[name] = res;
                }
                else if (forWrite)
                {
                    if (((res.attributes & JSValueAttributesInternal.SystemObject) != 0 || fromProto))
                    {
                        if ((res.attributes & JSValueAttributesInternal.ReadOnly) == 0
                            && (res.valueType != JSValueType.Property || memberScope == PropertyScope.Own))
                        {
                            res = res.CloneImpl(false);
                            if (fields == null)
                                fields = getFieldsContainer();
                            fields[name] = res;
                        }
                    }
                }
            }

            res.valueType |= JSValueType.NotExistsInObject;
            return res;
        }

        private JSValue getSymbol(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            JSObject proto = null;
            JSValue res = null;
            var symbol = key.oValue as Symbol;
            var fromProto = (symbols == null || !symbols.TryGetValue(symbol, out res) || res.valueType < JSValueType.Undefined) && ((proto = __proto__).oValue != null);
            if (fromProto)
            {
                res = proto.GetProperty(key, false, memberScope);
                if ((memberScope == PropertyScope.Own && ((res.attributes & JSValueAttributesInternal.Field) == 0 || res.valueType != JSValueType.Property)) || res.valueType < JSValueType.Undefined)
                    res = null;
            }
            if (res == null)
            {
                if (!forWrite || (attributes & JSValueAttributesInternal.Immutable) != 0)
                    return notExists;

                res = new JSValue { valueType = JSValueType.NotExistsInObject };
                if (symbols == null)
                    symbols = new Dictionary<Symbol, JSValue>();
                symbols[symbol] = res;
            }
            else if (forWrite)
            {
                if ((res.attributes & JSValueAttributesInternal.SystemObject) != 0 || fromProto)
                {
                    if ((res.attributes & JSValueAttributesInternal.ReadOnly) == 0
                        && (res.valueType != JSValueType.Property || memberScope == PropertyScope.Own))
                    {
                        res = res.CloneImpl(false);
                        if (symbols == null)
                            symbols = new Dictionary<Symbol, JSValue>();
                        symbols[symbol] = res;
                    }
                }
            }
            return res;
        }

        protected internal override void SetProperty(JSValue key, JSValue value, PropertyScope memberScope, bool throwOnError)
        {
            JSValue field;
            if (valueType >= JSValueType.Object && oValue != this)
            {
                if (oValue == null)
                    ExceptionsHelper.Throw(new TypeError("Can not get property \"" + key + "\" of \"null\""));
                field = oValue as JSObject;
                if (field != null)
                {
                    field.SetProperty(key, value, memberScope, throwOnError);
                    return;
                }
            }
            field = GetProperty(key, true, PropertyScope.Сommon);
            if (field.valueType == JSValueType.Property)
            {
                var setter = (field.oValue as GsPropertyPair).set;
                if (setter != null)
                    setter.Call(this, new Arguments { value });
                else if (throwOnError)
                    ExceptionsHelper.Throw(new TypeError("Can not assign to readonly property \"" + key + "\""));
                return;
            }
            else if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (throwOnError)
                    ExceptionsHelper.Throw(new TypeError("Can not assign to readonly property \"" + key + "\""));
            }
            else
                field.Assign(value);
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            JSValue field;
            if (valueType >= JSValueType.Object && oValue != this)
            {
                if (oValue == null)
                    ExceptionsHelper.Throw(new TypeError("Can't get property \"" + name + "\" of \"null\""));

                field = oValue as JSObject;
                if (field != null)
                    return field.DeleteProperty(name);
            }

            string tname = null;
            if (fields != null
                && fields.TryGetValue(tname = name.ToString(), out field)
                && (!field.Exists || (field.attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field.valueType = JSValueType.NotExistsInObject;

                return fields.Remove(tname);
            }

            field = GetProperty(name, true, PropertyScope.Own);
            if (!field.Exists)
                return true;

            if ((field.attributes & JSValueAttributesInternal.SystemObject) != 0)
                field = GetProperty(name, true, PropertyScope.Own);

            if ((field.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                field.valueType = JSValueType.NotExistsInObject;
                field.oValue = null;
                return true;
            }
            return false;
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.Exists && (!hideNonEnum || (f.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                        yield return f;
                }
            }
            if (__prototype != null)
            {
                for (var e = __prototype.GetEnumerator(hideNonEnum, EnumerationMode.RequireValues); e.MoveNext();)
                {
                    if (e.Current.Value.valueType >= JSValueType.Undefined
                        && (e.Current.Value.attributes & JSValueAttributesInternal.Field) != 0)
                    {
                        yield return e.Current;
                    }
                }
            }
        }

        [Hidden]
        public sealed override void Assign(JSValue value)
        {
            if ((attributes & JSValueAttributesInternal.ReadOnly) == 0)
            {
                if (this is GlobalObject)
                    ExceptionsHelper.Throw(new NiL.JS.BaseLibrary.ReferenceError("Invalid left-hand side"));
                throw new InvalidOperationException("Try to assign to a non-primitive value");
            }
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSValue> getFieldsContainer()
        {
            //return new Dictionary<string, JSObject>(System.StringComparer.Ordinal);
            return new StringMap<JSValue>();
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static JSValue create(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Prototype may be only Object or null."));
            var proto = args[0].oValue as JSObject ?? @null;
            var members = args[1].oValue as JSObject ?? @null;
            if (args[1].valueType >= JSValueType.Object && members.oValue == null)
                ExceptionsHelper.Throw(new TypeError("Properties descriptor may be only Object."));
            var res = CreateObject(true);
            if (proto.valueType >= JSValueType.Object)
                res.__prototype = proto;
            if (members.valueType >= JSValueType.Object)
            {
                foreach (var item in members)
                {
                    var desc = item.Value;
                    if (desc.valueType == JSValueType.Property)
                    {
                        var getter = (desc.oValue as GsPropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            ExceptionsHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                        desc = (getter.oValue as Function).Call(members, null);
                    }
                    if (desc.valueType < JSValueType.Object || desc.oValue == null)
                        ExceptionsHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                    var value = desc["value"];
                    if (value.valueType == JSValueType.Property)
                        value = Tools.InvokeGetter(value, desc);

                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSValueType.Property)
                        configurable = Tools.InvokeGetter(configurable, desc);

                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSValueType.Property)
                        enumerable = Tools.InvokeGetter(enumerable, desc);

                    var writable = desc["writable"];
                    if (writable.valueType == JSValueType.Property)
                        writable = Tools.InvokeGetter(writable, desc);

                    var get = desc["get"];
                    if (get.valueType == JSValueType.Property)
                        get = Tools.InvokeGetter(get, desc);

                    var set = desc["set"];
                    if (set.valueType == JSValueType.Property)
                        set = Tools.InvokeGetter(set, desc);

                    if (value.Exists && (get.Exists || set.Exists))
                        ExceptionsHelper.Throw(new TypeError("Property can not have getter or setter and default value."));
                    if (writable.Exists && (get.Exists || set.Exists))
                        ExceptionsHelper.Throw(new TypeError("Property can not have getter or setter and writable attribute."));
                    if (get.Defined && get.valueType != JSValueType.Function)
                        ExceptionsHelper.Throw(new TypeError("Getter mast be a function."));
                    if (set.Defined && set.valueType != JSValueType.Function)
                        ExceptionsHelper.Throw(new TypeError("Setter mast be a function."));
                    var obj = new JSValue() { valueType = JSValueType.Undefined };
                    res.fields[item.Key] = obj;
                    obj.attributes |=
                          JSValueAttributesInternal.DoNotEnumerate
                        | JSValueAttributesInternal.NonConfigurable
                        | JSValueAttributesInternal.DoNotDelete
                        | JSValueAttributesInternal.ReadOnly;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSValueAttributesInternal.DoNotEnumerate;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete);
                    if (value.Exists)
                    {
                        var atr = obj.attributes;
                        obj.attributes = 0;
                        obj.Assign(value);
                        obj.attributes = atr;
                        if ((bool)writable)
                            obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
                    }
                    else if (get.Exists || set.Exists)
                    {
                        Function setter = null, getter = null;
                        if (obj.valueType == JSValueType.Property)
                        {
                            setter = (obj.oValue as GsPropertyPair).set;
                            getter = (obj.oValue as GsPropertyPair).get;
                        }
                        obj.valueType = JSValueType.Property;
                        obj.oValue = new GsPropertyPair
                        {
                            set = set.Exists ? set.oValue as Function : setter,
                            get = get.Exists ? get.oValue as Function : getter
                        };
                    }
                    else if ((bool)writable)
                        obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
                }
            }
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSValue defineProperties(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Property define may only for Objects."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Can not define properties of null."));
            var target = args[0].oValue as JSObject ?? @null;
            var members = args[1].oValue as JSObject ?? @null;
            if (!args[1].Defined)
                ExceptionsHelper.Throw(new TypeError("Properties descriptor can not be undefined."));
            if (args[1].valueType < JSValueType.Object)
                return target;
            if (members.oValue == null)
                ExceptionsHelper.Throw(new TypeError("Properties descriptor can not be null."));
            if (target.valueType < JSValueType.Object || target.oValue == null)
                return target;
            if (members.valueType > JSValueType.Undefined)
            {
                foreach (var item in members)
                {
                    var desc = item.Value;
                    if (desc.valueType == JSValueType.Property)
                    {
                        var getter = (desc.oValue as GsPropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            ExceptionsHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                        desc = (getter.oValue as Function).Call(members, null);
                    }
                    if (desc.valueType < JSValueType.Object || desc.oValue == null)
                        ExceptionsHelper.Throw(new TypeError("Invalid property descriptor for property " + item.Key + " ."));
                    definePropertyImpl(target, desc.oValue as JSObject, item.Key);
                }
            }
            return target;
        }

        [DoNotEnumerate]
        [ArgumentsLength(3)]
        [CLSCompliant(false)]
        public static JSValue defineProperty(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object || args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.defineProperty cannot apply to non-object."));
            var target = args[0].oValue as JSObject ?? @null;
            var desc = args[2].oValue as JSObject ?? @null;
            if (desc.valueType < JSValueType.Object || desc.oValue == null)
                ExceptionsHelper.Throw(new TypeError("Invalid property descriptor."));
            if (target.valueType < JSValueType.Object || target.oValue == null)
                return target;
            if (target is TypeProxy)
                target = (target as TypeProxy).prototypeInstance ?? target;
            string memberName = args[1].ToString();
            return definePropertyImpl(target, desc, memberName);
        }

        private static JSObject definePropertyImpl(JSObject target, JSObject desc, string memberName)
        {
            var value = desc["value"];
            if (value.valueType == JSValueType.Property)
                value = Tools.InvokeGetter(value, desc);

            var configurable = desc["configurable"];
            if (configurable.valueType == JSValueType.Property)
                configurable = Tools.InvokeGetter(configurable, desc);

            var enumerable = desc["enumerable"];
            if (enumerable.valueType == JSValueType.Property)
                enumerable = Tools.InvokeGetter(enumerable, desc);

            var writable = desc["writable"];
            if (writable.valueType == JSValueType.Property)
                writable = Tools.InvokeGetter(writable, desc);

            var get = desc["get"];
            if (get.valueType == JSValueType.Property)
                get = Tools.InvokeGetter(get, desc);

            var set = desc["set"];
            if (set.valueType == JSValueType.Property)
                set = Tools.InvokeGetter(set, desc);
            if (value.Exists && (get.Exists || set.Exists))
                ExceptionsHelper.Throw(new TypeError("Property can not have getter or setter and default value."));
            if (writable.Exists && (get.Exists || set.Exists))
                ExceptionsHelper.Throw(new TypeError("Property can not have getter or setter and writable attribute."));
            if (get.Defined && get.valueType != JSValueType.Function)
                ExceptionsHelper.Throw(new TypeError("Getter mast be a function."));
            if (set.Defined && set.valueType != JSValueType.Function)
                ExceptionsHelper.Throw(new TypeError("Setter mast be a function."));
            JSValue obj = null;
            obj = target.DefineProperty(memberName);
            if ((obj.attributes & JSValueAttributesInternal.Argument) != 0 && (set.Exists || get.Exists))
            {
                var ti = 0;
                if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                    (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                else
                    target.fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                obj.attributes &= ~JSValueAttributesInternal.Argument;
            }
            if ((obj.attributes & JSValueAttributesInternal.SystemObject) != 0)
                ExceptionsHelper.Throw(new TypeError("Can not define property \"" + memberName + "\". Object immutable."));

            if (target is NiL.JS.BaseLibrary.Array)
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
                                ExceptionsHelper.Throw(new RangeError("Invalid array length"));
                            if ((obj.attributes & JSValueAttributesInternal.ReadOnly) != 0
                                && ((obj.valueType == JSValueType.Double && nlenD != obj.dValue)
                                    || (obj.valueType == JSValueType.Integer && nlen != obj.iValue)))
                                ExceptionsHelper.Throw(new TypeError("Cannot change length of fixed size array"));
                            if (!(target as NiL.JS.BaseLibrary.Array).SetLenght(nlen))
                                ExceptionsHelper.Throw(new TypeError("Unable to reduce length because Exists not configurable elements"));
                            value = notExists;
                        }
                    }
                    finally
                    {
                        if (writable.Exists && !(bool)writable)
                            obj.attributes |= JSValueAttributesInternal.ReadOnly;
                    }
                }
            }

            var newProp = obj.valueType < JSValueType.Undefined;
            var config = (obj.attributes & JSValueAttributesInternal.NonConfigurable) == 0 || newProp;

            if (!config)
            {
                if (enumerable.Exists && (obj.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0 == (bool)enumerable)
                    ExceptionsHelper.Throw(new TypeError("Cannot change enumerable attribute for non configurable property."));

                if (writable.Exists && (obj.attributes & JSValueAttributesInternal.ReadOnly) != 0 && (bool)writable)
                    ExceptionsHelper.Throw(new TypeError("Cannot change writable attribute for non configurable property."));

                if (configurable.Exists && (bool)configurable)
                    ExceptionsHelper.Throw(new TypeError("Cannot set configurable attribute to true."));

                if ((obj.valueType != JSValueType.Property || ((obj.attributes & JSValueAttributesInternal.Field) != 0)) && (set.Exists || get.Exists))
                    ExceptionsHelper.Throw(new TypeError("Cannot redefine not configurable property from immediate value to accessor property"));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0 && value.Exists)
                    ExceptionsHelper.Throw(new TypeError("Cannot redefine not configurable property from accessor property to immediate value"));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0
                    && set.Exists
                    && (((obj.oValue as GsPropertyPair).set != null && (obj.oValue as GsPropertyPair).set.oValue != set.oValue)
                        || ((obj.oValue as GsPropertyPair).set == null && set.Defined)))
                    ExceptionsHelper.Throw(new TypeError("Cannot redefine setter of not configurable property."));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0
                    && get.Exists
                    && (((obj.oValue as GsPropertyPair).get != null && (obj.oValue as GsPropertyPair).get.oValue != get.oValue)
                        || ((obj.oValue as GsPropertyPair).get == null && get.Defined)))
                    ExceptionsHelper.Throw(new TypeError("Cannot redefine getter of not configurable property."));
            }

            if (value.Exists)
            {
                if (!config
                    && (obj.attributes & JSValueAttributesInternal.ReadOnly) != 0
                    && !((StrictEqual.Check(obj, value) && ((obj.valueType == JSValueType.Undefined && value.valueType == JSValueType.Undefined) || !obj.IsNumber || !value.IsNumber || (1.0 / Tools.JSObjectToDouble(obj) == 1.0 / Tools.JSObjectToDouble(value))))
                        || (obj.valueType == JSValueType.Double && value.valueType == JSValueType.Double && double.IsNaN(obj.dValue) && double.IsNaN(value.dValue))))
                    ExceptionsHelper.Throw(new TypeError("Cannot change value of not configurable not writable peoperty."));
                //if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0 || obj.valueType == JSObjectType.Property)
                {
                    obj.valueType = JSValueType.Undefined;
                    var atrbts = obj.attributes;
                    obj.attributes = 0;
                    obj.Assign(value);
                    obj.attributes = atrbts;
                }
            }
            else if (get.Exists || set.Exists)
            {
                Function setter = null, getter = null;
                if (obj.valueType == JSValueType.Property)
                {
                    setter = (obj.oValue as GsPropertyPair).set;
                    getter = (obj.oValue as GsPropertyPair).get;
                }
                obj.valueType = JSValueType.Property;
                obj.oValue = new GsPropertyPair
                {
                    set = set.Exists ? set.oValue as Function : setter,
                    get = get.Exists ? get.oValue as Function : getter
                };
            }
            else if (newProp)
                obj.valueType = JSValueType.Undefined;
            if (newProp)
            {
                obj.attributes |=
                    JSValueAttributesInternal.DoNotEnumerate
                    | JSValueAttributesInternal.DoNotDelete
                    | JSValueAttributesInternal.NonConfigurable
                    | JSValueAttributesInternal.ReadOnly;
            }
            else
            {
                var atrbts = obj.attributes;
                if (configurable.Exists && (config || !(bool)configurable))
                    obj.attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete;
                if (enumerable.Exists && (config || !(bool)enumerable))
                    obj.attributes |= JSValueAttributesInternal.DoNotEnumerate;
                if (writable.Exists && (config || !(bool)writable))
                    obj.attributes |= JSValueAttributesInternal.ReadOnly;

                if (obj.attributes != atrbts && (obj.attributes & JSValueAttributesInternal.Argument) != 0)
                {
                    var ti = 0;
                    if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                        (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                    else
                        target.fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                }
            }

            if (config)
            {
                if ((bool)enumerable)
                    obj.attributes &= ~JSValueAttributesInternal.DoNotEnumerate;
                if ((bool)configurable)
                    obj.attributes &= ~(JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete);
                if ((bool)writable)
                    obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
            }
            return target;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineGetter__(Arguments args)
        {
            if (args.length < 2)
                ExceptionsHelper.Throw(new TypeError("Missed parameters"));
            if (args[1].valueType != JSValueType.Function)
                ExceptionsHelper.Throw(new TypeError("Expecting function as second parameter"));
            var field = GetProperty(args[0], true, PropertyScope.Own);
            if ((field.attributes & JSValueAttributesInternal.NonConfigurable) != 0)
                ExceptionsHelper.Throw(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
                ExceptionsHelper.Throw(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSValueType.Property)
                (field.oValue as GsPropertyPair).get = args.a1.oValue as Function;
            else
            {
                field.valueType = JSValueType.Property;
                field.oValue = new GsPropertyPair
                {
                    get = args.a1.oValue as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineSetter__(Arguments args)
        {
            if (args.length < 2)
                ExceptionsHelper.Throw(new TypeError("Missed parameters"));
            if (args[1].valueType != JSValueType.Function)
                ExceptionsHelper.Throw(new TypeError("Expecting function as second parameter"));
            var field = GetProperty(args[0], true, PropertyScope.Own);
            if ((field.attributes & JSValueAttributesInternal.NonConfigurable) != 0)
                ExceptionsHelper.Throw(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
                ExceptionsHelper.Throw(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSValueType.Property)
                (field.oValue as GsPropertyPair).set = args.a1.oValue as Function;
            else
            {
                field.valueType = JSValueType.Property;
                field.oValue = new GsPropertyPair
                {
                    set = args.a1.oValue as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupGetter(Arguments args)
        {
            var field = GetProperty(args[0], false, PropertyScope.Сommon);
            if (field.valueType == JSValueType.Property)
                return (field.oValue as GsPropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupSetter(Arguments args)
        {
            var field = GetProperty(args[0], false, PropertyScope.Сommon);
            if (field.valueType == JSValueType.Property)
                return (field.oValue as GsPropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        public static JSValue freeze(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.freeze called on non-object."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.freeze called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            for (var e = obj.GetEnumerator(false, EnumerationMode.RequireValuesForWrite); e.MoveNext();)
            {
                var value = e.Current.Value;
                if ((value.attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value.attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue preventExtensions(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Prevent the expansion can only for objects"));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Can not prevent extensions for null"));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isExtensible(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.isExtensible called on non-object."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.isExtensible called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            return (obj.attributes & JSValueAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSValue isSealed(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.isSealed called on non-object."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.isSealed called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            if ((obj.attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data)
                {
                    if (node != null
                        && node.Exists
                        && node.valueType >= JSValueType.Object && node.oValue != null
                        && (node.attributes & JSValueAttributesInternal.NonConfigurable) == 0)
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if (f.Value.valueType >= JSValueType.Object && f.Value.oValue != null && (f.Value.attributes & JSValueAttributesInternal.NonConfigurable) == 0)
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject seal(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.seal called on non-object."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.seal called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            for (var e = obj.GetEnumerator(false, EnumerationMode.RequireValuesForWrite); e.MoveNext();)
            {
                var value = e.Current.Value;
                if ((value.attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value.attributes |= JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isFrozen(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.isFrozen called on non-object."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Object.isFrozen called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            if ((obj.attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data.DirectOrder)
                {
                    if (node.Value != null && node.Value.Exists &&
                        ((node.Value.attributes & JSValueAttributesInternal.NonConfigurable) == 0
                        || (node.Value.valueType != JSValueType.Property && (node.Value.attributes & JSValueAttributesInternal.ReadOnly) == 0)))
                        return false;
                }
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                {
                    if ((arg[i].attributes & JSValueAttributesInternal.NonConfigurable) == 0
                            || (arg[i].valueType != JSValueType.Property && (arg[i].attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if ((f.Value.attributes & JSValueAttributesInternal.NonConfigurable) == 0
                            || (f.Value.valueType != JSValueType.Property && (f.Value.attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Parameter isn't an Object."));
            var res = args[0].__proto__;
            //if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
            //    res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSValue getOwnPropertyDescriptor(Arguments args)
        {
            if (args[0].valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("Object.getOwnPropertyDescriptor called on undefined."));
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.getOwnPropertyDescriptor called on non-object."));
            var source = args[0].oValue as JSObject ?? @null;
            var obj = source.GetProperty(args[1], false, PropertyScope.Own);
            if (obj.valueType < JSValueType.Undefined)
                return undefined;
            if ((obj.attributes & JSValueAttributesInternal.SystemObject) != 0)
                obj = source.GetProperty(args[1], true, PropertyScope.Own);
            var res = CreateObject();
            if (obj.valueType != JSValueType.Property || (obj.attributes & JSValueAttributesInternal.Field) != 0)
            {
                if (obj.valueType == JSValueType.Property)
                    res["value"] = (obj.oValue as GsPropertyPair).get.Call(source, null);
                else
                    res["value"] = obj;
                res["writable"] = obj.valueType < JSValueType.Undefined || (obj.attributes & JSValueAttributesInternal.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj.oValue as GsPropertyPair).set;
                res["get"] = (obj.oValue as GsPropertyPair).get;
            }
            res["configurable"] = (obj.attributes & JSValueAttributesInternal.NonConfigurable) == 0 || (obj.attributes & JSValueAttributesInternal.DoNotDelete) == 0;
            res["enumerable"] = (obj.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Cannot get property names of null"));
            var obj = args[0].oValue as JSObject;

            var result = new BaseLibrary.Array();
            for (var e = obj.GetEnumerator(false, EnumerationMode.KeysOnly); e.MoveNext();)
                result.Add(e.Current.Key);

            return result;
        }

        [DoNotEnumerate]
        public static JSObject keys(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Object.keys called on non-object value."));
            if (args[0].oValue == null)
                ExceptionsHelper.Throw(new TypeError("Cannot get property names of null"));
            var obj = args[0].oValue as JSObject;

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

            if ((value1.valueType | JSValueType.Undefined) != (value2.valueType | JSValueType.Undefined))
                return false;

            if (value1.valueType == JSValueType.Double
                && double.IsNaN(value1.dValue)
                && double.IsNaN(value2.dValue))
                return true;

            return StrictEqual.Check(value1, value2);
        }

        public static BaseLibrary.Array getOwnPropertySymbols(JSObject obj)
        {
            return new BaseLibrary.Array(obj?.symbols.Keys ?? new Symbol[0]);
        }
    }
}
