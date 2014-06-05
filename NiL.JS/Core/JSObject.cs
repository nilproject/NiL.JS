using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace NiL.JS.Core
{
    [Serializable]
    public enum JSObjectType : int
    {
        NotExist = 0,
        NotExistInObject = 1,
        Undefined = 2,
        Bool = 3,
        Int = 4,
        Double = 5,
        String = 6,
        Object = 7,
        Function = 8,
        Date = 9,
        Property = 10
    }

    [Serializable]
    [Flags]
    public enum JSObjectAttributes : int
    {
        None = 0,
        DontEnum = 1 << 0,
        DontDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        Argument = 1 << 16,
        GetValue = 1 << 17
    }

    public delegate void AssignCallback(JSObject sender);

    [Serializable]
    /// <summary>
    /// Базовый объект для всех объектов, участвующих в выполнении скрипта.
    /// Для создания пользовательских объектов, в качестве базового типа, рекомендуется использовать тип NiL.JS.Core.EmbeddedType
    /// </summary>
    public class JSObject : IEnumerable<string>, IEnumerable, ICloneable
    {
        private NiL.JS.Core.BaseTypes.String @string;

        [Modules.Hidden]
        internal static readonly AssignCallback ErrorAssignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Modules.Hidden]
        internal static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Modules.Hidden]
        internal static readonly JSObject undefined = new JSObject() { ValueType = JSObjectType.Undefined, attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum | JSObjectAttributes.ReadOnly };
        [Modules.Hidden]
        internal static readonly JSObject Null = new JSObject() { ValueType = JSObjectType.Object, oValue = null, assignCallback = ErrorAssignCallback, attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum };
        [Modules.Hidden]
        internal static readonly JSObject nullString = new JSObject() { ValueType = JSObjectType.String, oValue = "null", assignCallback = ErrorAssignCallback, attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum };
        [Modules.Hidden]
        internal static JSObject GlobalPrototype;

        static JSObject()
        {
            undefined.Protect();
        }

        [NonSerialized]
        [Modules.Hidden]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal AssignCallback assignCallback;
        [Modules.Hidden]
        internal JSObject prototype;
        [Modules.Hidden]
        internal Dictionary<string, JSObject> fields;

        [Modules.Hidden]
        internal string lastRequestedName;
        [Modules.Hidden]
        internal JSObjectType ValueType;
        [Modules.Hidden]
        internal int iValue;
        [Modules.Hidden]
        internal double dValue;
        [Modules.Hidden]
        internal object oValue;
        [Modules.Hidden]
        internal JSObjectAttributes attributes;

        [Modules.Hidden]
        public object Value
        {
            [Modules.Hidden]
            get
            {
                switch (ValueType)
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
                        return oValue;
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                    default:
                        return null;
                }
            }
        }

        [Modules.Hidden]
        public JSObjectType Type
        {
            [Modules.Hidden]
            get
            {
                return ValueType;
            }
        }

        [Modules.Hidden]
        public JSObjectAttributes Attributes
        {
            [Modules.Hidden]
            get
            {
                return attributes;
            }
        }

        [Modules.Hidden]
        internal static JSObject Object(Context context, JSObject args)
        {
            object oVal = null;
            if (args != null && args.GetField("length", true, false).iValue > 0)
                oVal = args.GetField("0", true, false);
            JSObject res = null;
            if ((oVal == null) ||
                (oVal is JSObject && (((oVal as JSObject).ValueType >= JSObjectType.Object && (oVal as JSObject).oValue == null) || (oVal as JSObject).ValueType <= JSObjectType.Undefined)))
                return CreateObject();
            else if ((oVal as JSObject).ValueType >= JSObjectType.Object && (oVal as JSObject).oValue != null)
                return oVal as JSObject;

            if (context.thisBind != null && context.thisBind.ValueType == JSObjectType.Object && context.thisBind.prototype != null && context.thisBind.prototype.oValue == GlobalPrototype)
                res = context.thisBind;
            else
                res = new JSObject(true) { prototype = GlobalPrototype };

            res.ValueType = JSObjectType.Object;
            res.oValue = oVal ?? res;
            if (oVal is JSObject)
                res.prototype = (oVal as JSObject).GetField("__proto__", true, true);
            else
                res.prototype = GlobalPrototype;
            return res;
        }

        [Modules.Hidden]
        public JSObject()
        {
            ValueType = JSObjectType.Undefined;
        }

        [Modules.Hidden]
        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        [Modules.Hidden]
        public static JSObject CreateObject()
        {
            var t = new JSObject(true)
            {
                ValueType = JSObjectType.Object,
                prototype = GlobalPrototype.Clone()
                as JSObject
            };
            t.oValue = t;
            return t;
        }

        [Modules.Hidden]
        public JSObject GetField(string name)
        {
            return GetField(name, true, false);
        }

        [Modules.Hidden]
        public virtual JSObject GetField(string name, bool fast, bool own)
        {
            fast |= (attributes & JSObjectAttributes.Immutable) != 0;
            switch (ValueType)
            {
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new ReferenceError("Varible not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"undefined\".")));
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        prototype = TypeProxy.GetPrototype(typeof(Number));
                        fast = true;
                        break;
                    }
                case JSObjectType.String:
                    {
                        if (@string == null)
                            @string = new BaseTypes.String();
                        @string.oValue = oValue;
                        return @string.GetField(name, fast, own);
                    }
                case JSObjectType.Bool:
                    {
                        prototype = TypeProxy.GetPrototype(typeof(BaseTypes.Boolean));
                        fast = true;
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                case JSObjectType.Property:
                    {
                        if (oValue != this && (oValue is JSObject) && ((oValue as JSObject).ValueType >= JSObjectType.Object))
                            return (oValue as JSObject).GetField(name, fast, own);
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        break;
                    }
                case JSObjectType.Function:
                    {
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        if (oValue == this)
                            break;
                        return (oValue as JSObject).GetField(name, false, own);
                    }
                default:
                    throw new NotImplementedException();
            }
            return DefaultFieldGetter(name, fast, own);
        }

        [Modules.Hidden]
        protected JSObject DefaultFieldGetter(string name, bool fast, bool own)
        {
            switch (name)
            {
                case "__proto__":
                    return prototype ?? (fast ? Null : prototype = new JSObject(false) { ValueType = JSObjectType.Object, oValue = null });
                default:
                    {
                        JSObject res = null;
                        bool fromProto = (fields == null || !fields.TryGetValue(name, out res) || res.ValueType < JSObjectType.Undefined) && (prototype != null) && (!own || prototype.oValue is TypeProxy);
                        if (fromProto)
                        {
                            res = prototype.GetField(name, true, own);
                            if (own && (prototype.oValue as TypeProxy).prototypeInstance != this.oValue && res.ValueType != JSObjectType.Property)
                                res = null;
                            if (res == undefined)
                                res = null;
                        }
                        if (res == null)
                        {
                            if (fast)
                                return undefined;
                            res = new JSObject()
                            {
                                oValue = this,
                                lastRequestedName = name,
                                assignCallback = (sender) =>
                                {
                                    var owner = sender.oValue as JSObject;
                                    sender.oValue = null;
                                    if (owner.fields == null)
                                        owner.fields = new Dictionary<string, JSObject>();
                                    owner.fields[sender.lastRequestedName] = sender;
                                    sender.assignCallback = null;
                                },
                                ValueType = JSObjectType.NotExistInObject
                            };
                        }
                        else if (fromProto && !fast)
                        {
                            var t = new JSObject();
                            t.Assign(res);
                            t.lastRequestedName = name;
                            t.assignCallback = (sender) =>
                            {
                                if (fields == null)
                                    fields = new Dictionary<string, JSObject>();
                                fields[sender.lastRequestedName] = sender;
                                sender.assignCallback = null;
                            };
                            t.attributes = res.attributes;
                            res = t;
                        }
                        else
                            res.lastRequestedName = name;
                        if (res.ValueType == JSObjectType.NotExist)
                            res.ValueType = JSObjectType.NotExistInObject;
                        return res;
                    }
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Modules.Hidden]
        public void Protect()
        {
            if (assignCallback != null)
                assignCallback(this);
            attributes |= JSObjectAttributes.DontDelete | JSObjectAttributes.ReadOnly;
        }

        [Modules.Hidden]
        internal JSObject ToPrimitiveValue_Value_String()
        {
            if (ValueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetField("valueOf", true, false);
                JSObject res = null;
                if (tpvs.ValueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.ValueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.ValueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetField("toString", true, false);
                if (tpvs.ValueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.ValueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.ValueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

        [Modules.Hidden]
        internal JSObject ToPrimitiveValue_String_Value()
        {
            if (ValueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetField("toString", true, false);
                JSObject res = null;
                if (tpvs.ValueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.ValueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.ValueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetField("valueOf", true, false);
                if (tpvs.ValueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.ValueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.ValueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Modules.Hidden]
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
                this.ValueType = value.ValueType;
                this.iValue = value.iValue;
                this.oValue = value.oValue;
                this.dValue = value.dValue;
                this.prototype = value.prototype;
                this.fields = value.fields;
                this.attributes = this.attributes & ~JSObjectAttributes.Immutable | (value.attributes & JSObjectAttributes.Immutable);
                return;
            }
            this.fields = null;
            this.prototype = null;
            this.ValueType = JSObjectType.Undefined;
            this.oValue = null;
            this.prototype = null;
        }

        [Modules.Hidden]
        public virtual object Clone()
        {
            var res = new JSObject();
            res.Assign(this);
            return res;
        }

        [Modules.Hidden]
        public override string ToString()
        {
            if (ValueType <= JSObjectType.Undefined)
                return "undefined";
            var res = ToPrimitiveValue_String_Value();
            switch (res.ValueType)
            {
                case JSObjectType.Bool:
                    return res.iValue != 0 ? "true" : "false";
                case JSObjectType.Int:
                    return res.iValue >= 0 && res.iValue < 16 ? Tools.NumString[res.iValue] : res.iValue.ToString();
                case JSObjectType.Double:
                    return Tools.DoubleToString(res.dValue);
                case JSObjectType.String:
                    return res.oValue as string;
                default:
                    return (res.oValue ?? "null").ToString();
            }
        }

        [Modules.Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Modules.Hidden]
        public virtual IEnumerator<string> GetEnumerator()
        {
            if (this.GetType() == typeof(JSObject) && ValueType >= JSObjectType.Object)
            {
                if (oValue != this && oValue is JSObject)
                    return (oValue as JSObject).GetEnumerator();
            }
            if (fields == null)
                return EmptyEnumerator;
            return enumerate();
        }

        private IEnumerator<string> enumerate()
        {
            foreach (var f in fields)
            {
                if (f.Value.ValueType >= JSObjectType.Undefined && (f.Value.attributes & JSObjectAttributes.DontEnum) == 0)
                    yield return f.Key;
            }
        }

        [Modules.DoNotEnumerateAttribute]
        [Modules.ParametersCount(0)]
        public virtual JSObject toString(JSObject args)
        {
            switch (this.ValueType)
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
                        if (this.oValue is ThisObject)
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

        [Modules.DoNotEnumerateAttribute]
        public virtual JSObject toLocaleString()
        {
            if (ValueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on null.")));
            if (ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on undefined value.")));
            return toString(null);
        }

        [Modules.DoNotEnumerateAttribute]
        public virtual JSObject valueOf()
        {
            if (ValueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on null.")));
            if (ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on undefined value.")));
            if (ValueType >= JSObjectType.Object && oValue is JSObject && oValue != this)
                return (oValue as JSObject).valueOf();
            else
                return this;
        }

        [Modules.DoNotEnumerateAttribute]
        public virtual JSObject isPrototypeOf(JSObject args)
        {
            if (ValueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on null.")));
            if (ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on undefined value.")));
            if (args.GetField("length", true, false).iValue == 0)
                return false;
            var a = args.GetField("0", true, false);
            JSObject o = false;
            o.ValueType = JSObjectType.Bool;
            o.iValue = 0;
            if (this.ValueType >= JSObjectType.Object && this.oValue != null)
            {
                while (a.ValueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == this.oValue)
                    {
                        o.iValue = 1;
                        return o;
                    }
                    if ((a.oValue is TypeProxy) && (a.oValue as TypeProxy).prototypeInstance == this)
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true, false);
                }
            }
            return o;
        }

        [Modules.DoNotEnumerateAttribute]
        public virtual JSObject hasOwnProperty(JSObject args)
        {
            JSObject name = args.GetField("0", true, false);
            string n = "";
            switch (name.ValueType)
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
                        if (args.ValueType == JSObjectType.String)
                            n = name.oValue as string;
                        if (args.ValueType == JSObjectType.Int)
                            n = name.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (args.ValueType == JSObjectType.Double)
                            n = Tools.DoubleToString(name.dValue);
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = GetField(n, true, true);
            res = (res.ValueType >= JSObjectType.Undefined) && (res != JSObject.undefined);
            return res;
        }

        [Modules.DoNotEnumerateAttribute]
        public virtual JSObject propertyIsEnumerable(JSObject args)
        {
            if (ValueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on null.")));
            if (ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on undefined value.")));
            JSObject name = args.GetField("0", true, false);
            string n = name.ToString();
            var res = GetField(n, true, true);
            res = (res.ValueType >= JSObjectType.Undefined) && (res != JSObject.undefined) && ((res.attributes & JSObjectAttributes.DontEnum) == 0);
            return res;
        }

        [Modules.Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Modules.Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static JSObject getPrototypeOf(JSObject args)
        {
            if (args.GetField("0", true, false).ValueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Parameter isn't an Object.")));
            return args.GetField("0", true, false).prototype;
        }

        public static implicit operator JSObject(char value)
        {
            return new BaseTypes.String(value.ToString());
        }

        public static implicit operator JSObject(bool value)
        {
            return new BaseTypes.Boolean(value);
        }

        public static implicit operator JSObject(int value)
        {
            return new BaseTypes.Number(value);
        }

        public static implicit operator JSObject(long value)
        {
            return new BaseTypes.Number((double)value);
        }

        public static implicit operator JSObject(double value)
        {
            return new BaseTypes.Number(value);
        }

        public static implicit operator JSObject(string value)
        {
            if (string.IsNullOrEmpty(value))
                return BaseTypes.String.EmptyString;
            return new BaseTypes.String(value);
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static explicit operator bool(JSObject obj)
        {
            var vt = obj.ValueType;
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
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
