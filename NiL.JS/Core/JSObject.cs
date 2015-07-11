using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !PORTABLE
#if !PORTABLE
    [Serializable]
#endif
#endif
    public enum JSObjectType
    {
        NotExists = 0,
        NotExistsInObject = 1,
        Undefined = 3,
        Bool = 7,
        Int = 11,
        Double = 19,
        String = 35,
        Object = 67,
        Function = 131,
        Date = 259,
        Property = 515
    }

#if !PORTABLE
#if !PORTABLE
    [Serializable]
#endif
#endif
    [Flags]
    internal enum JSObjectAttributesInternal : uint
    {
        None = 0,
        DoNotEnum = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NotConfigurable = 1 << 4,
        Argument = 1 << 16,
        SystemObject = 1 << 17,
        ProxyPrototype = 1 << 18,
        Field = 1 << 19,
        Eval = 1 << 20,
        Temporary = 1 << 21,
        Cloned = 1 << 22,
        ContainsParsedInt = 1 << 23,
        ContainsParsedDouble = 1 << 24,
        Reassign = 1 << 25,
        IntrinsicFunction = 1 << 26,
        /// <summary>
        /// јттрибуты, не передающиес€ при присваивании
        /// </summary>
        PrivateAttributes = Immutable | ProxyPrototype | Field,
    }

#if !PORTABLE
#if !PORTABLE
    [Serializable]
#endif
#endif
    [Flags]
    public enum JSObjectAttributes : int
    {
        None = 0,
        DoNotEnum = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NotConfigurable = 1 << 4,
    }

#if !PORTABLE
#if !PORTABLE
    [Serializable]
#endif
#endif
    [StructLayout(LayoutKind.Sequential)]
    public class JSObject : IEnumerable<string>, IEnumerable, IComparable<JSObject>
#if !PORTABLE
, ICloneable, IConvertible
#endif
    {
        [Hidden]
        internal static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Hidden]
        internal static readonly JSObject undefined = new JSObject() { valueType = JSObjectType.Undefined, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject notExists = new JSObject() { valueType = JSObjectType.NotExists, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject Null = new JSObject() { valueType = JSObjectType.Object, oValue = null, attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject nullString = new JSObject() { valueType = JSObjectType.String, oValue = "null", attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static JSObject GlobalPrototype;

        [Hidden]
        public static JSObject Undefined { [Hidden] get { return undefined; } }
        [Hidden]
        public static JSObject NotExists
        {
            [Hidden]
            get
            {
                notExists.valueType = JSObjectType.NotExists;
                return notExists;
            }
        }
        [Hidden]
        public static JSObject NotExistsInObject
        {
            [Hidden]
            get
            {
                notExists.valueType = JSObjectType.NotExistsInObject;
                return notExists;
            }
        }
        [Hidden]
        public static JSObject JSNull { [Hidden] get { return Null; } }

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        [CLSCompliant(false)]
        public JSObject __proto__
        {
            [Hidden]
            get
            {
                if (GlobalPrototype == this)
                    return Null;
                if (valueType >= JSObjectType.Object
                    && oValue != this
                    && (oValue as JSObject) != null)
                    return (oValue as JSObject).__proto__;
                if (!this.IsDefinded || (this.valueType >= JSObjectType.Object && oValue == null))
                    throw new JSException(new TypeError("Can not get prototype of null or undefined"));
                if (__prototype != null)
                {
                    if (__prototype.valueType < JSObjectType.Object)
                        __prototype = getDefaultPrototype();
                    else if (__prototype.oValue == null)
                        return Null;
                    return __prototype;
                }
                return __prototype = getDefaultPrototype();
            }
            [Hidden]
            set
            {
                if ((attributes & JSObjectAttributesInternal.Immutable) != 0)
                    return;
                if (valueType < JSObjectType.Object)
                    return;
                if (oValue != this
                    && (oValue as JSObject) != null)
                {
                    (oValue as JSObject).__proto__ = value;
                    __prototype = null;
                    return;
                }
                if (value == null)
                {
                    __prototype = Null;
                    return;
                }
                if (value.valueType < JSObjectType.Object)
                    return;
                if (value.oValue == null)
                {
                    __prototype = Null;
                    return;
                }
                var c = value.oValue as JSObject ?? value;
                while (c != null && c != Null && c.valueType > JSObjectType.Undefined)
                {
                    if (c == this || c.oValue == this)
                        throw new JSException(new Error("Try to set cyclic __proto__ value."));
                    c = c.__proto__;
                }
                __prototype = value.oValue as JSObject ?? value;
            }
        }

        protected virtual JSObject getDefaultPrototype()
        {
            switch (valueType)
            {
                case JSObjectType.Bool:
                    return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.Boolean));
                case JSObjectType.Double:
                case JSObjectType.Int:
                    return TypeProxy.GetPrototype(typeof(Number));
                case JSObjectType.String:
                    return TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.String));
            }
            if (valueType >= JSObjectType.Object && oValue != null && oValue != this)
            {
                var rojso = oValue as JSObject;
                if (rojso != null)
                    return rojso.getDefaultPrototype() ?? Null;
                else
                    return TypeProxy.GetPrototype(oValue.GetType());
            }
            return TypeProxy.GetPrototype(this.GetType());
        }

        internal JSObjectType valueType;
        internal int iValue;
        internal double dValue;
        internal object oValue;

        internal IDictionary<string, JSObject> fields;
        internal JSObject __prototype;
        internal JSObjectAttributesInternal attributes;

        [Hidden]
        public virtual JSObject this[string name]
        {
            [Hidden]
            get
            {
                return this.GetMember(name);
            }
            [Hidden]
            set
            {
                this.GetMember(name, true, true).Assign(value ?? JSObject.undefined);
            }
        }

        [Hidden]
        public virtual object Value
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
                        return oValue.ToString();
                    case JSObjectType.Object:
                    case JSObjectType.Function:
                    case JSObjectType.Property:
                    case JSObjectType.Date:
                        {
                            if (oValue != this && oValue is JSObject)
                                return (oValue as JSObject).Value;
                            return oValue;
                        }
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistsInObject:
                    default:
                        return null;
                }
            }
            protected set
            {
                switch (valueType)
                {
                    case JSObjectType.Bool:
                        {
                            iValue = (bool)value ? 1 : 0;
                            break;
                        }
                    case JSObjectType.Int:
                        {
                            iValue = (int)value;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            dValue = (double)value;
                            break;
                        }
                    case JSObjectType.String:
                        {
                            oValue = (string)value;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Function:
                    case JSObjectType.Property:
                    case JSObjectType.Date:
                        {
                            oValue = value;
                            break;
                        }
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistsInObject:
                    default:
                        throw new InvalidOperationException();
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
            protected set
            {
                valueType = value;
            }
        }

        [Hidden]
        public JSObjectAttributes Attributes
        {
            [Hidden]
            get
            {
                return (JSObjectAttributes)((int)attributes & 0xffff);
            }
        }

        [Hidden]
        public JSObject()
        {
            //valueType = JSObjectType.Undefined;
        }

        [Hidden]
        public JSObject(bool createFields)
            : this()
        {
            if (createFields)
                fields = JSObject.createFields();
        }

        [Hidden]
        public JSObject(object content)
            : this(true)
        {
            oValue = content;
            valueType = JSObjectType.Object;
        }

        [Hidden]
        public static JSObject CreateObject()
        {
            return CreateObject(true);
        }

        [Hidden]
        public static JSObject CreateObject(bool createFields)
        {
            var t = new JSObject(true)
            {
                valueType = JSObjectType.Object,
                __prototype = GlobalPrototype
            };
            t.oValue = t;
            return t;
        }

        [Hidden]
        public JSObject GetMember(string name)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, false, false);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc.wrap(name), false, false);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        public JSObject GetMember(string name, bool own)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, false, own);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc.wrap(name), false, own);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        public JSObject DefineMember(string name)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, true, true);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc.wrap(name), true, true);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        internal protected JSObject GetMember(string name, bool createMember, bool own)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, createMember, own);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc.wrap(name), createMember, own);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        internal protected virtual JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            switch (valueType)
            {
                case JSObjectType.Bool:
                    {
                        if (own)
                            return notExists;
                        forWrite = false;
                        if (__prototype == null)
                            __prototype = TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.Boolean));
#if DEBUG
                        else if (__prototype.oValue != TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.Boolean)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        return __prototype.GetMember(name, false, false);
                    }
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        if (own)
                            return notExists;
                        forWrite = false;
                        if (__prototype == null)
                            __prototype = TypeProxy.GetPrototype(typeof(Number));
#if DEBUG
                        else if (__prototype.oValue != TypeProxy.GetPrototype(typeof(Number)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        return __prototype.GetMember(name, false, false);
                    }
                case JSObjectType.String:
                    {
                        if (own)
                            return notExists;

                        return stringGetMember(name, ref forWrite);
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (oValue == this)
                            return DefaultFieldGetter(name, forWrite, own);
                        if (oValue == null)
                            throw can_not_get_property_of_null(name);
                        var inObj = oValue as JSObject;
                        if (inObj != null)
                            return inObj.GetMember(name, forWrite, own);
                        break;
                    }
                case JSObjectType.Property:
                    return notExists;
                default:
                    throw can_not_get_property_of_undefined(name);
            }
            return DefaultFieldGetter(name, forWrite, own);
        }

        private Exception can_not_get_property_of_undefined(JSObject name)
        {
            return new JSException(new TypeError("Can't get property \"" + name + "\" of \"undefined\""));
        }

        private static Exception can_not_get_property_of_null(JSObject name)
        {
            return new JSException(new TypeError("Can't get property \"" + name + "\" of \"null\""));
        }

        private JSObject stringGetMember(JSObject name, ref bool forWrite)
        {
            forWrite = false;
            if (name.valueType == JSObjectType.String
                && string.CompareOrdinal(name.oValue.ToString(), "length") == 0)
                return oValue.ToString().Length;

            double dindex = 0.0;
            int index = 0;
            dindex = Tools.JSObjectToDouble(name);

            if (dindex >= 0.0
                && ((index = (int)dindex) == dindex)
                && oValue.ToString().Length > index)
                return oValue.ToString()[index];

            if (__prototype == null)
                __prototype = TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.String));
#if DEBUG
            else if (__prototype.oValue != TypeProxy.GetPrototype(typeof(NiL.JS.BaseLibrary.String)).oValue)
                System.Diagnostics.Debugger.Break();
#endif
            return __prototype.GetMember(name, false, false);
        }

        [Hidden]
        protected JSObject DefaultFieldGetter(JSObject nameObj, bool forWrite, bool own)
        {
            string name = null;
            if (forWrite || fields != null)
                name = nameObj.ToString();
            JSObject res = null;
            JSObject proto = null;
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res) || res.valueType < JSObjectType.Undefined) && ((proto = __proto__).oValue != null);
            if (fromProto)
            {
                res = proto.GetMember(nameObj, false, own);
                if (((own && ((res.attributes & JSObjectAttributesInternal.Field) == 0 || res.valueType != JSObjectType.Property))) || res.valueType < JSObjectType.Undefined)
                    res = null;
            }
            if (res == null)
            {
                if (!forWrite || (attributes & JSObjectAttributesInternal.Immutable) != 0)
                {
                    if (!own && string.CompareOrdinal(name, "__proto__") == 0)
                        return proto;
                    return notExists;
                }
                res = new JSObject()
                {
                    valueType = JSObjectType.NotExistsInObject
                };
                if (fields == null)
                    fields = createFields();
                fields[name] = res;
            }
            else if (forWrite && ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0 || fromProto))
            {
                if ((res.attributes & JSObjectAttributesInternal.ReadOnly) == 0
                    && (res.valueType != JSObjectType.Property || own))
                {
                    res = res.CloneImpl();
                    if (fields == null)
                        fields = createFields();
                    fields[name] = res;
                }
            }
            res.valueType |= JSObjectType.NotExistsInObject;
            return res;
        }

        internal protected virtual void SetMember(JSObject name, JSObject value, bool strict)
        {
            JSObject field;
            if (valueType >= JSObjectType.Object && oValue != this)
            {
                if (oValue == null)
                    throw new JSException(new TypeError("Can not get property \"" + name + "\" of \"null\""));
                field = oValue as JSObject;
                if (field != null)
                {
                    field.SetMember(name, value, strict);
                    return;
                }
            }
            field = GetMember(name, true, false);
            if (field.valueType == JSObjectType.Property)
            {
                var setter = (field.oValue as PropertyPair).set;
                if (setter != null)
                    setter.Invoke(this, new Arguments { value });
                else if (strict)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + name + "\""));
                return;
            }
            else
                if (strict && (field.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + name + "\""));
            field.Assign(value);
        }

        [Hidden]
        public bool Delete(string memberName)
        {
            if (memberName == null)
                throw new ArgumentNullException("memberName can not be null");
            var cc = Context.CurrentContext;
            if (cc == null)
                return DeleteMember((JSObject)memberName);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return DeleteMember(cc.wrap(memberName));
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        internal protected virtual bool DeleteMember(JSObject name)
        {
            JSObject field;
            if (valueType >= JSObjectType.Object && oValue != this)
            {
                if (oValue == null)
                    throw new JSException(new TypeError("Can't get property \"" + name + "\" of \"null\""));
                field = oValue as JSObject;
                if (field != null)
                    return field.DeleteMember(name);
            }
            string tname = null;
            if (fields != null
                && fields.TryGetValue(tname = name.ToString(), out field)
                && (!field.IsExist || (field.attributes & JSObjectAttributesInternal.DoNotDelete) == 0))
            {
                if ((field.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    field.valueType = JSObjectType.NotExistsInObject;
                return fields.Remove(tname);
            }
            field = GetMember(name, false, true);
            if (!field.IsExist)
                return true;
            if ((field.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                field = GetMember(name, true, true);
            if ((field.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
            {
                field.valueType = JSObjectType.NotExistsInObject;
                field.oValue = null;
                return true;
            }
            return false;
        }

        [Hidden]
        internal JSObject ToPrimitiveValue_Value_String()
        {
            return ToPrimitiveValue("valueOf", "toString");
        }

        internal JSObject ToPrimitiveValue_LocaleString_Value()
        {
            return ToPrimitiveValue("toLocaleString", "valueOf");
        }

        internal JSObject ToPrimitiveValue_String_Value()
        {
            return ToPrimitiveValue("toString", "valueOf");
        }

        internal JSObject ToPrimitiveValue(string func0, string func1)
        {
            if (valueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetMember(func0);
                JSObject res = null;
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.BaseLibrary.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is NiL.JS.BaseLibrary.String)
                            res = res.oValue as NiL.JS.BaseLibrary.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetMember(func1);
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.BaseLibrary.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is NiL.JS.BaseLibrary.String)
                            res = res.oValue as NiL.JS.BaseLibrary.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(new TypeError("Can't convert object to primitive value."));
            }
            return this;
        }

        //private static readonly Func<object, IntPtr> getPtr = Activator.CreateInstance(typeof(Func<object, IntPtr>), null, (new Func<IntPtr, IntPtr>(x => x)).Method.MethodHandle.GetFunctionPointer()) as Func<object, IntPtr>;

        [Hidden]
        public virtual void Assign(JSObject value)
        {
#if DEBUG
            if (valueType == JSObjectType.Property)
                throw new InvalidOperationException("Try to assign to property.");
#endif
            if (this == value || (attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) != 0)
                return;
            this.valueType = value.valueType | JSObjectType.Undefined;
            this.iValue = value.iValue;
            this.dValue = value.dValue;
            this.oValue = value.oValue;
            this.fields = null;
            this.__prototype = null;
            this.attributes =
                (this.attributes & ~JSObjectAttributesInternal.PrivateAttributes)
                | (value.attributes & JSObjectAttributesInternal.PrivateAttributes);
        }

        [Hidden]
        public object Clone()
        {
            return CloneImpl();
        }

        internal virtual JSObject CloneImpl()
        {
            var res = new JSObject();
            res.Assign(this);
            res.attributes = this.attributes & ~(JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly);
            return res;
        }

        internal virtual JSObject CloneImpl(JSObjectAttributesInternal resetMask)
        {
            var res = new JSObject();
            res.Assign(this);
            res.attributes = this.attributes & ~resetMask;
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType == JSObjectType.String)
                return oValue.ToString();
            if (valueType <= JSObjectType.Undefined)
                return "undefined";
            if (valueType == JSObjectType.Property)
            {
                var tempStr = "[";
                if ((oValue as PropertyPair).get != null)
                    tempStr += "Getter";
                if ((oValue as PropertyPair).set != null)
                    tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
                if (tempStr.Length == 1)
                    return "[Invalid Property]";
                tempStr += "]";
                return tempStr;
            }
            var res = this.valueType >= JSObjectType.Object ? ToPrimitiveValue_String_Value() : this;
            switch (res.valueType)
            {
                case JSObjectType.Bool:
                    return res.iValue != 0 ? "true" : "false";
                case JSObjectType.Int:
                    return res.iValue >= 0 && res.iValue < 16 ? Tools.NumString[res.iValue] : Tools.Int32ToString(res.iValue);
                case JSObjectType.Double:
                    return Tools.DoubleToString(res.dValue);
                case JSObjectType.String:
                    return res.oValue.ToString();
                default:
                    return (res.oValue ?? "null").ToString();
            }
        }

        [Hidden]
        public JSObject ToObject()
        {
            if (valueType >= JSObjectType.Object)
                return this;
            switch (valueType)
            {
                case JSObjectType.Bool:
                    return new ObjectContainer(new NiL.JS.BaseLibrary.Boolean(iValue != 0));
                case JSObjectType.Int:
                    return new ObjectContainer(new Number(iValue));
                case JSObjectType.Double:
                    return new ObjectContainer(new Number(dValue));
                case JSObjectType.String:
                    return new ObjectContainer(new NiL.JS.BaseLibrary.String(oValue.ToString()));
            }
            return new JSObject() { valueType = JSObjectType.Object };
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

        [Hidden]
        public IEnumerator<string> GetEnumerator(bool hideNonEnum)
        {
            if (this is JSObject && valueType >= JSObjectType.Object)
            {
                if (oValue != this && oValue is JSObject)
                    return (oValue as JSObject).GetEnumeratorImpl(hideNonEnum);
            }
            return GetEnumeratorImpl(hideNonEnum);
        }

        protected internal virtual IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (valueType == JSObjectType.String || this.GetType() == typeof(NiL.JS.BaseLibrary.String))
            {
                var len = (oValue.ToString()).Length;
                for (var i = 0; i < len; i++)
                    yield return i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture);
                if (!hideNonEnum)
                    yield return "length";
            }
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.IsExist && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ArgumentsLength(0)]
        [AllowNullArguments]
        public virtual JSObject toString(Arguments args)
        {
            var self = this.oValue as JSObject ?? this;
            switch (self.valueType)
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
                        if (self.oValue is GlobalObject)
                            return self.oValue.ToString();
                        if (self.oValue is TypeProxy)
                        {
                            var ht = (self.oValue as TypeProxy).hostedType;
                            return "[object " + (ht == typeof(JSObject) ? typeof(System.Object) : ht).Name + "]";
                        }
                        if (self.oValue != null)
                            return "[object " + (self.Value.GetType() == typeof(JSObject) ? typeof(System.Object) : self.Value.GetType()).Name + "]";
                        else
                            return "[object Null]";
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        [DoNotEnumerate]
        public virtual JSObject toLocaleString()
        {
            var self = this.oValue as JSObject ?? this;
            if (self.valueType >= JSObjectType.Object && self.oValue == null)
                throw new JSException(new TypeError("toLocaleString calling on null."));
            if (self.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("toLocaleString calling on undefined value."));
            if (self == this)
                return toString(null);
            return self.toLocaleString();
        }

        [DoNotEnumerate]
        public virtual JSObject valueOf()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("valueOf calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("valueOf calling on undefined value."));
            return valueType < JSObjectType.Object ? new JSObject() { valueType = JSObjectType.Object, oValue = this } : this;
        }

        [DoNotEnumerate]
        public virtual JSObject propertyIsEnumerable(Arguments args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("propertyIsEnumerable calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("propertyIsEnumerable calling on undefined value."));
            JSObject name = args[0];
            string n = name.ToString();
            var res = GetMember(n, true);
            res = (res.IsExist) && ((res.attributes & JSObjectAttributesInternal.DoNotEnum) == 0);
            return res;
        }

        [DoNotEnumerate]
        public virtual JSObject isPrototypeOf(Arguments args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("isPrototypeOf calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("isPrototypeOf calling on undefined value."));
            if (args.GetMember("length").iValue == 0)
                return false;
            var a = args[0];
            a = a.__proto__;
            if (this.valueType >= JSObjectType.Object)
            {
                if (this.oValue != null)
                {
                    while (a != null && a.valueType >= JSObjectType.Object && a.oValue != null)
                    {
                        if (a.oValue == this.oValue)
                            return true;
                        var pi = (a.oValue is TypeProxy) ? (a.oValue as TypeProxy).prototypeInstance : null;
                        if (pi != null && (this == pi || this == pi.oValue))
                            return true;
                        a = a.__proto__;
                    }
                }
            }
            else
            {
                if (a.oValue == this.oValue)
                    return true;
                var pi = (a.oValue is TypeProxy) ? (a.oValue as TypeProxy).prototypeInstance : null;
                if (pi != null && (this == pi || this == pi.oValue))
                    return true;
            }
            return false;
        }

        [DoNotEnumerate]
        public virtual JSObject hasOwnProperty(Arguments args)
        {
            JSObject name = args[0];
            var res = GetMember(name, false, true);
            return res.IsExist;
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSObject> createFields()
        {
            return createFields(0);
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSObject> createFields(int p)
        {
            //return new Dictionary<string, JSObject>(p, System.StringComparer.Ordinal);
            return new StringMap2<JSObject>();
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj as JSObject, null))
                return false;
            if (object.ReferenceEquals(obj, this))
                return true;
            return Expressions.StrictEqual.Check(this, obj as JSObject);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Hidden]
        public static implicit operator JSObject(char value)
        {
            return new NiL.JS.BaseLibrary.String(value.ToString());
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator JSObject(bool value)
        {
            return (NiL.JS.BaseLibrary.Boolean)value;
        }

        [Hidden]
        public static implicit operator JSObject(int value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSObject(long value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSObject(double value)
        {
            return new Number(value);
        }

        [Hidden]
        public static implicit operator JSObject(string value)
        {
            return new NiL.JS.BaseLibrary.String(value);
        }

        [Hidden]
        public static explicit operator int(JSObject obj)
        {
            return Tools.JSObjectToInt32(obj);
        }

        [Hidden]
        public static explicit operator long(JSObject obj)
        {
            return Tools.JSObjectToInt64(obj);
        }

        [Hidden]
        public static explicit operator double(JSObject obj)
        {
            return Tools.JSObjectToDouble(obj);
        }

        [Hidden]
        public static explicit operator bool(JSObject obj)
        {
            switch (obj.valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    return obj.iValue != 0;
                case JSObjectType.Double:
                    return !(obj.dValue == 0.0 || double.IsNaN(obj.dValue));
                case JSObjectType.String:
                    return !string.IsNullOrEmpty(obj.oValue.ToString());
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                    return obj.oValue != null;
            }
            return false;
        }

        [Hidden]
        public bool IsExist
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType >= JSObjectType.Undefined; }
        }

        [Hidden]
        public bool IsDefinded
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType > JSObjectType.Undefined; }
        }

        [Hidden]
        public bool IsNumber
        {
            [Hidden]
#if INLINE
            [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType == JSObjectType.Int || valueType == JSObjectType.Double; }
        }

        #region „лены IComparable<JSObject>

        int IComparable<JSObject>.CompareTo(JSObject other)
        {
            throw new NotImplementedException();
        }

        #endregion

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSObject create(Arguments args)
        {
            var proto = args[0];
            if (proto.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Prototype may be only Object or null."));
            proto = proto.oValue as JSObject ?? proto;
            var members = args[1];
            if (members.valueType >= JSObjectType.Object && members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor may be only Object."));
            var res = CreateObject();
            if (proto.valueType >= JSObjectType.Object)
                res.__prototype = proto;
            if (members.valueType >= JSObjectType.Object)
            {
                members = (members.oValue as JSObject ?? members);
                foreach (var member in members)
                {
                    var desc = members[member];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as PropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                    var value = desc["value"];
                    if (value.valueType == JSObjectType.Property)
                    {
                        value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (value.valueType < JSObjectType.Undefined)
                            value = undefined;
                    }
                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSObjectType.Property)
                    {
                        configurable = ((configurable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (configurable.valueType < JSObjectType.Undefined)
                            configurable = undefined;
                    }
                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSObjectType.Property)
                    {
                        enumerable = ((enumerable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (enumerable.valueType < JSObjectType.Undefined)
                            enumerable = undefined;
                    }
                    var writable = desc["writable"];
                    if (writable.valueType == JSObjectType.Property)
                    {
                        writable = ((writable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (writable.valueType < JSObjectType.Undefined)
                            writable = undefined;
                    }
                    var get = desc["get"];
                    if (get.valueType == JSObjectType.Property)
                    {
                        get = ((get.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (get.valueType < JSObjectType.Undefined)
                            get = undefined;
                    }
                    var set = desc["set"];
                    if (set.valueType == JSObjectType.Property)
                    {
                        set = ((set.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                        if (set.valueType < JSObjectType.Undefined)
                            set = undefined;
                    }
                    if (value.IsExist && (get.IsExist || set.IsExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and default value."));
                    if (writable.IsExist && (get.IsExist || set.IsExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
                    if (get.IsDefinded && get.valueType != JSObjectType.Function)
                        throw new JSException(new TypeError("Getter mast be a function."));
                    if (set.IsDefinded && set.valueType != JSObjectType.Function)
                        throw new JSException(new TypeError("Setter mast be a function."));
                    JSObject obj = new JSObject() { valueType = JSObjectType.Undefined };
                    res.fields[member] = obj;
                    obj.attributes |=
                        JSObjectAttributesInternal.DoNotEnum
                        | JSObjectAttributesInternal.NotConfigurable
                        | JSObjectAttributesInternal.DoNotDelete
                        | JSObjectAttributesInternal.ReadOnly;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                    if (value.IsExist)
                    {
                        var atr = obj.attributes;
                        obj.attributes = 0;
                        obj.Assign(value);
                        obj.attributes = atr;
                        if ((bool)writable)
                            obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                    }
                    else if (get.IsExist || set.IsExist)
                    {
                        Function setter = null, getter = null;
                        if (obj.valueType == JSObjectType.Property)
                        {
                            setter = (obj.oValue as PropertyPair).set;
                            getter = (obj.oValue as PropertyPair).get;
                        }
                        obj.valueType = JSObjectType.Property;
                        obj.oValue = new PropertyPair
                        {
                            set = set.IsExist ? set.oValue as Function : setter,
                            get = get.IsExist ? get.oValue as Function : getter
                        };
                    }
                    else if ((bool)writable)
                        obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                }
            }
            return res;
        }

        [ArgumentsLength(2)]
        [DoNotEnumerate]
        public static JSObject defineProperties(Arguments args)
        {
            var target = args[0];
            if (target.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Property define may only for Objects."));
            if (target.oValue == null)
                throw new JSException(new TypeError("Can not define properties of null."));
            target = target.oValue as JSObject ?? target;
            if (target is TypeProxy)
                target = (target as TypeProxy).prototypeInstance ?? target;
            var members = args[1];
            if (!members.IsDefinded)
                throw new JSException(new TypeError("Properties descriptor can not be undefined."));
            if (members.valueType < JSObjectType.Object)
                return target;
            if (members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor can not be null."));
            if (members.valueType > JSObjectType.Undefined)
            {
                if (members.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Properties descriptor may be only Object."));
                foreach (var memberName in members)
                {
                    var desc = members[memberName];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as PropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                    definePropertyImpl(target, desc, memberName);
                }
            }
            return target;
        }

        [DoNotEnumerate]
        [ArgumentsLength(3)]
        public static JSObject defineProperty(Arguments args)
        {
            var target = args[0];
            if (target.valueType < JSObjectType.Object || target.oValue == null)
                throw new JSException(new TypeError("Object.defineProperty cannot apply to non-object."));
            target = target.oValue as JSObject ?? target;
            if (target.valueType <= JSObjectType.Undefined)
                return undefined;
            if (target is TypeProxy)
                target = (target as TypeProxy).prototypeInstance ?? target;
            var desc = args[2];
            if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                throw new JSException(new TypeError("Invalid property descriptor."));
            string memberName = args[1].ToString();
            return definePropertyImpl(target, desc, memberName);
        }

        private static JSObject definePropertyImpl(JSObject target, JSObject desc, string memberName)
        {
            var value = desc["value"];
            if (value.valueType == JSObjectType.Property)
            {
                value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (value.valueType < JSObjectType.Undefined)
                    value = undefined;
            }
            var configurable = desc["configurable"];
            if (configurable.valueType == JSObjectType.Property)
            {
                configurable = ((configurable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (configurable.valueType < JSObjectType.Undefined)
                    configurable = undefined;
            }
            var enumerable = desc["enumerable"];
            if (enumerable.valueType == JSObjectType.Property)
            {
                enumerable = ((enumerable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (enumerable.valueType < JSObjectType.Undefined)
                    enumerable = undefined;
            }
            var writable = desc["writable"];
            if (writable.valueType == JSObjectType.Property)
            {
                writable = ((writable.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (writable.valueType < JSObjectType.Undefined)
                    writable = undefined;
            }
            var get = desc["get"];
            if (get.valueType == JSObjectType.Property)
            {
                get = ((get.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (get.valueType < JSObjectType.Undefined)
                    get = undefined;
            }
            var set = desc["set"];
            if (set.valueType == JSObjectType.Property)
            {
                set = ((set.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(desc, null);
                if (set.valueType < JSObjectType.Undefined)
                    set = undefined;
            }
            if (value.IsExist && (get.IsExist || set.IsExist))
                throw new JSException(new TypeError("Property can not have getter or setter and default value."));
            if (writable.IsExist && (get.IsExist || set.IsExist))
                throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
            if (get.IsDefinded && get.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Getter mast be a function."));
            if (set.IsDefinded && set.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Setter mast be a function."));
            JSObject obj = null;
            obj = target.DefineMember(memberName);
            if ((obj.attributes & JSObjectAttributesInternal.Argument) != 0 && (set.IsExist || get.IsExist))
            {
                var ti = 0;
                if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                    (target as Arguments)[ti] = obj = obj.CloneImpl(JSObjectAttributesInternal.SystemObject);
                else
                    target.fields[memberName] = obj = obj.CloneImpl(JSObjectAttributesInternal.SystemObject);
                obj.attributes &= ~JSObjectAttributesInternal.Argument;
            }
            if ((obj.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                throw new JSException(new TypeError("Can not define property \"" + memberName + "\". Object is immutable."));

            if (target is NiL.JS.BaseLibrary.Array)
            {
                if (memberName == "length")
                {
                    try
                    {
                        if (value.IsExist)
                        {
                            var nlenD = Tools.JSObjectToDouble(value);
                            var nlen = (uint)nlenD;
                            if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                                throw new JSException(new RangeError("Invalid array length"));
                            if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) != 0
                                && ((obj.valueType == JSObjectType.Double && nlenD != obj.dValue)
                                    || (obj.valueType == JSObjectType.Int && nlen != obj.iValue)))
                                throw new JSException(new TypeError("Cannot change length of fixed size array"));
                            if (!(target as NiL.JS.BaseLibrary.Array).setLength(nlen))
                                throw new JSException(new TypeError("Unable to reduce length because not configurable elements"));
                            value = notExists;
                        }
                    }
                    finally
                    {
                        if (writable.IsExist && !(bool)writable)
                            obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                    }
                }
            }

            var newProp = obj.valueType < JSObjectType.Undefined;
            var config = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || newProp;

            if (!config)
            {
                if (enumerable.IsExist && (obj.attributes & JSObjectAttributesInternal.DoNotEnum) != 0 == (bool)enumerable)
                    throw new JSException(new TypeError("Cannot change enumerable attribute for non configurable property."));

                if (writable.IsExist && (obj.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && (bool)writable)
                    throw new JSException(new TypeError("Cannot change writable attribute for non configurable property."));

                if (configurable.IsExist && (bool)configurable)
                    throw new JSException(new TypeError("Cannot set configurate attribute to true."));

                if ((obj.valueType != JSObjectType.Property || ((obj.attributes & JSObjectAttributesInternal.Field) != 0)) && (set.IsExist || get.IsExist))
                    throw new JSException(new TypeError("Cannot redefine not configurable property from immediate value to accessor property"));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0 && value.IsExist)
                    throw new JSException(new TypeError("Cannot redefine not configurable property from accessor property to immediate value"));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0
                    && set.IsExist
                    && (((obj.oValue as PropertyPair).set != null && (obj.oValue as PropertyPair).set.oValue != set.oValue)
                        || ((obj.oValue as PropertyPair).set == null && set.IsDefinded)))
                    throw new JSException(new TypeError("Cannot redefine setter of not configurable property."));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0
                    && get.IsExist
                    && (((obj.oValue as PropertyPair).get != null && (obj.oValue as PropertyPair).get.oValue != get.oValue)
                        || ((obj.oValue as PropertyPair).get == null && get.IsDefinded)))
                    throw new JSException(new TypeError("Cannot redefine getter of not configurable property."));
            }

            if (value.IsExist)
            {
                if (!config
                    && (obj.attributes & JSObjectAttributesInternal.ReadOnly) != 0
                    && !((StrictEqual.Check(obj, value) && ((obj.valueType == JSObjectType.Undefined && value.valueType == JSObjectType.Undefined) || !obj.IsNumber || !value.IsNumber || (1.0 / Tools.JSObjectToDouble(obj) == 1.0 / Tools.JSObjectToDouble(value))))
                        || (obj.valueType == JSObjectType.Double && value.valueType == JSObjectType.Double && double.IsNaN(obj.dValue) && double.IsNaN(value.dValue))))
                    throw new JSException(new TypeError("Cannot change value of not configurable not writable peoperty."));
                //if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0 || obj.valueType == JSObjectType.Property)
                {
                    obj.valueType = JSObjectType.Undefined;
                    var atrbts = obj.attributes;
                    obj.attributes = 0;
                    obj.Assign(value);
                    obj.attributes = atrbts;
                }
            }
            else if (get.IsExist || set.IsExist)
            {
                Function setter = null, getter = null;
                if (obj.valueType == JSObjectType.Property)
                {
                    setter = (obj.oValue as PropertyPair).set;
                    getter = (obj.oValue as PropertyPair).get;
                }
                obj.valueType = JSObjectType.Property;
                obj.oValue = new PropertyPair
                {
                    set = set.IsExist ? set.oValue as Function : setter,
                    get = get.IsExist ? get.oValue as Function : getter
                };
            }
            else if (newProp)
                obj.valueType = JSObjectType.Undefined;
            if (newProp)
            {
                obj.attributes |=
                    JSObjectAttributesInternal.DoNotEnum
                    | JSObjectAttributesInternal.DoNotDelete
                    | JSObjectAttributesInternal.NotConfigurable
                    | JSObjectAttributesInternal.ReadOnly;
            }
            else
            {
                var atrbts = obj.attributes;
                if (configurable.IsExist && (config || !(bool)configurable))
                    obj.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
                if (enumerable.IsExist && (config || !(bool)enumerable))
                    obj.attributes |= JSObjectAttributesInternal.DoNotEnum;
                if (writable.IsExist && (config || !(bool)writable))
                    obj.attributes |= JSObjectAttributesInternal.ReadOnly;

                if (obj.attributes != atrbts && (obj.attributes & JSObjectAttributesInternal.Argument) != 0)
                {
                    var ti = 0;
                    if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                        (target as Arguments)[ti] = obj = obj.CloneImpl(JSObjectAttributesInternal.SystemObject);
                    else
                        target.fields[memberName] = obj = obj.CloneImpl(JSObjectAttributesInternal.SystemObject);
                    obj.attributes &= ~JSObjectAttributesInternal.Argument;
                }
            }

            if (config)
            {
                if ((bool)enumerable)
                    obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                if ((bool)configurable)
                    obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                if ((bool)writable)
                    obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
            }
            return target;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineGetter__(Arguments args)
        {
            if (args.length < 2)
                throw new JSException(new TypeError("Missed parameters"));
            if (args[1].valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Expecting function as second parameter"));
            var field = GetMember(args[0], true, true);
            if ((field.attributes & JSObjectAttributesInternal.NotConfigurable) != 0)
                throw new JSException(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                throw new JSException(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSObjectType.Property)
                (field.oValue as PropertyPair).get = args.a1.oValue as Function;
            else
            {
                field.valueType = JSObjectType.Property;
                field.oValue = new PropertyPair
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
                throw new JSException(new TypeError("Missed parameters"));
            if (args[1].valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Expecting function as second parameter"));
            var field = GetMember(args[0], true, true);
            if ((field.attributes & JSObjectAttributesInternal.NotConfigurable) != 0)
                throw new JSException(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                throw new JSException(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSObjectType.Property)
                (field.oValue as PropertyPair).set = args.a1.oValue as Function;
            else
            {
                field.valueType = JSObjectType.Property;
                field.oValue = new PropertyPair
                {
                    set = args.a1.oValue as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupGetter(Arguments args)
        {
            var field = GetMember(args[0], false, false);
            if (field.valueType == JSObjectType.Property)
                return (field.oValue as PropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupSetter(Arguments args)
        {
            var field = GetMember(args[0], false, false);
            if (field.valueType == JSObjectType.Property)
                return (field.oValue as PropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        public static JSObject freeze(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.freeze called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.freeze called on null."));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            obj = obj.Value as JSObject ?? obj;
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            if (obj is NiL.JS.BaseLibrary.Array)
            {
                var arr = obj as NiL.JS.BaseLibrary.Array;
                foreach (var element in arr.data)
                    if (element != null && element.IsExist)
                        element.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                    arg[i].attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                    f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
            return obj;
        }

        [DoNotEnumerate]
        public static JSObject preventExtensions(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Prevent the expansion can only for objects"));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Can not prevent extensions for null"));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            var res = obj.Value as JSObject;
            if (res != null)
                res.attributes |= JSObjectAttributesInternal.Immutable;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject isExtensible(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isExtensible called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isExtensible called on null."));
            return ((obj.Value as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSObject isSealed(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isSealed called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isSealed called on null."));
            if (((obj = obj.Value as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data)
                {
                    if (node != null
                        && node.IsExist
                        && node.valueType >= JSObjectType.Object && node.oValue != null
                        && (node.attributes & JSObjectAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if (f.Value.valueType >= JSObjectType.Object && f.Value.oValue != null && (f.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject seal(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.seal called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.seal called on null."));
            obj = obj.Value as JSObject ?? obj;
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            (obj.Value as JSObject ?? obj).attributes |= JSObjectAttributesInternal.Immutable;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var element in arr.data)
                    if (element != null && element.IsExist)
                        element.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                    arg[i].attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                    f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            return obj;
        }

        [DoNotEnumerate]
        public static JSObject isFrozen(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isFrozen called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isFrozen called on null."));
            obj = obj.Value as JSObject ?? obj;
            if ((obj.attributes & JSObjectAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data.DirectOrder)
                {
                    if (node.Value != null && node.Value.IsExist &&
                        ((node.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                        || (node.Value.valueType != JSObjectType.Property && (node.Value.attributes & JSObjectAttributesInternal.ReadOnly) == 0)))
                        return false;
                }
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                {
                    if ((arg[i].attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                            || (arg[i].valueType != JSObjectType.Property && (arg[i].attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if ((f.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                            || (f.Value.valueType != JSObjectType.Property && (f.Value.attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(Arguments args)
        {
            if (args[0].valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Parameter isn't an Object."));
            var res = args[0].__proto__;
            //if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
            //    res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSObject getOwnPropertyDescriptor(Arguments args)
        {
            var source = args[0];
            if (source.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on undefined."));
            if (source.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on non-object."));
            var obj = source.GetMember(args[1], false, true);
            if (obj.valueType < JSObjectType.Undefined)
                return undefined;
            if ((obj.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                obj = source.GetMember(args[1], true, true);
            var res = CreateObject();
            if (obj.valueType != JSObjectType.Property || (obj.attributes & JSObjectAttributesInternal.Field) != 0)
            {
                if (obj.valueType == JSObjectType.Property)
                    res["value"] = (obj.oValue as PropertyPair).get.Invoke(source, null);
                else
                    res["value"] = obj;
                res["writable"] = obj.valueType < JSObjectType.Undefined || (obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj.oValue as PropertyPair).set;
                res["get"] = (obj.oValue as PropertyPair).get;
            }
            res["configurable"] = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || (obj.attributes & JSObjectAttributesInternal.DoNotDelete) == 0;
            res["enumerable"] = (obj.attributes & JSObjectAttributesInternal.DoNotEnum) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
            return new NiL.JS.BaseLibrary.Array((obj.oValue as JSObject ?? obj).GetEnumeratorImpl(false));
        }

        [DoNotEnumerate]
        public static JSObject keys(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.keys called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
            return new NiL.JS.BaseLibrary.Array((obj.Value as JSObject ?? obj).GetEnumeratorImpl(true));
        }

        internal bool isNeedClone { get { return (attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) == JSObjectAttributesInternal.SystemObject; } }

        #region „лены IConvertible
#if !PORTABLE
        [Hidden]
        public virtual T As<T>()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)(bool)this; // оптимизатор разруливает такой каскад преобразований
                case TypeCode.Byte:
                    {
                        return (T)(object)(byte)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.Char:
                    {
                        if (valueType == JSObjectType.Object
                            && oValue is char)
                            return (T)oValue;
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        return (T)(object)(decimal)Tools.JSObjectToDouble(this);
                    }
                case TypeCode.Double:
                    {
                        return (T)(object)Tools.JSObjectToDouble(this);
                    }
                case TypeCode.Int16:
                    {
                        return (T)(object)(Int16)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.Int32:
                    {
                        return (T)(object)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.Int64:
                    {
                        return (T)(object)Tools.JSObjectToInt64(this);
                    }
                case TypeCode.Object:
                    {
                        return (T)Value;
                    }
                case TypeCode.SByte:
                    {
                        return (T)(object)(sbyte)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.Single:
                    {
                        return (T)(object)(float)Tools.JSObjectToDouble(this);
                    }
                case TypeCode.String:
                    {
                        return (T)(object)this.ToString();
                    }
                case TypeCode.UInt16:
                    {
                        return (T)(object)(ushort)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.UInt32:
                    {
                        return (T)(object)(uint)Tools.JSObjectToInt32(this);
                    }
                case TypeCode.UInt64:
                    {
                        return (T)(object)(ulong)Tools.JSObjectToInt64(this);
                    }
            }
            throw new InvalidCastException();
        }

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
            if (valueType == JSObjectType.Date)
                return (oValue as Date).ToDateTime();
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
            if (valueType == JSObjectType.Int)
                return iValue;
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
            return Tools.convertJStoObj(this, conversionType);
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
    }
}
