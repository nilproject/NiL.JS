using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace NiL.JS.Core
{
    public delegate JSObject CallableField(Context context, JSObject args);

    internal enum JSObjectType : int
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

    [Flags]
    internal enum ObjectAttributes : int
    {
        None = 0,
        DontEnum = 1 << 0,
        DontDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 16
    }

    public class JSObject : IEnumerable<string>, IEnumerable, ICloneable
    {
        [Modules.Hidden]
        internal static readonly Action ErrorAssignCallback = () => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Modules.Hidden]
        internal protected static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Modules.Hidden]
        public static readonly JSObject undefined = new JSObject() { ValueType = JSObjectType.Undefined, attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
        [Modules.Hidden]
        public static readonly JSObject Null = new JSObject() { ValueType = JSObjectType.Object, oValue = null, assignCallback = ErrorAssignCallback, attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
        [Modules.Hidden]
        internal static readonly JSObject nullString = new JSObject() { ValueType = JSObjectType.String, oValue = "null", assignCallback = ErrorAssignCallback, attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
        [Modules.Hidden]
        internal static JSObject Prototype;

        static JSObject()
        {
            undefined.Protect();
        }

        [Modules.Hidden]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal Action assignCallback;
        [Modules.Hidden]
        internal JSObject prototype;
        [Modules.Hidden]
        internal Dictionary<string, JSObject> fields;

        [Modules.Hidden]
        internal JSObjectType ValueType;
        [Modules.Hidden]
        internal int iValue;
        [Modules.Hidden]
        internal double dValue;
        [Modules.Hidden]
        internal object oValue;
        [Modules.Hidden]
        internal ObjectAttributes attributes;

        [Modules.Hidden]
        public object Value
        {
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
        public JSObject()
        {
            ValueType = JSObjectType.Undefined;
        }

        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        public JSObject(JSObject args)
        {
            object oVal = null;
            if (args != null && args.GetField("length", true, false).iValue > 0)
                oVal = args.GetField("0", true, false);
            if ((oVal == null) ||
                (oVal is JSObject && (((oVal as JSObject).ValueType >= JSObjectType.Object && (oVal as JSObject).oValue == null) || (oVal as JSObject).ValueType <= JSObjectType.Undefined)))
                oVal = new object();
            ValueType = JSObjectType.Object;
            oValue = oVal;
            if (oVal is JSObject)
                prototype = (oVal as JSObject).GetField("__proto__", true, true);
            else
                prototype = Prototype;
        }

        [Modules.Hidden]
        public virtual JSObject GetField(string name, bool fast, bool own)
        {
            if (ValueType >= JSObjectType.Object && (oValue is JSObject) && ((oValue as JSObject).ValueType >= JSObjectType.Object))
                return (oValue as JSObject).GetField(name, fast, own);
            return DefaultFieldGetter(name, fast, own);
        }

        [Modules.Hidden]
        protected JSObject DefaultFieldGetter(string name, bool fast, bool own)
        {
            if ((attributes & ObjectAttributes.Immutable) != 0)
                fast = true;
            switch (ValueType)
            {
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new ReferenceError("Varible not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        if (this is TypeProxy)
                            prototype = JSObject.Prototype;
                        else
                            prototype = TypeProxy.GetPrototype(typeof(Number));
                        fast = true;
                        break;
                    }
                case JSObjectType.String:
                    {
                        if (this is TypeProxy)
                            prototype = JSObject.Prototype;
                        else
                            prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.String));
                        fast = true;
                        break;
                    }
                case JSObjectType.Bool:
                    {
                        if (this is TypeProxy)
                            prototype = JSObject.Prototype;
                        else
                            prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.Boolean));
                        fast = true;
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                case JSObjectType.Property:
                    {
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\"")));
                        break;
                    }
                case JSObjectType.Function:
                    {
                        if (prototype == null)
                        {
                            if (this is TypeProxy)
                                prototype = JSObject.Prototype;
                            else
                                prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.Function)).Clone() as JSObject;
                        }
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\"")));
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
            switch (name)
            {
                case "__proto__": 
                    return prototype ?? (fast ? Null : prototype = new JSObject(false) { ValueType = JSObjectType.Object, oValue = null });
                default:
                    {
                        JSObject res = null;
                        bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && (prototype != null) && (!own || prototype.oValue is TypeProxy);
                        if (fromProto)
                        {
                            res = prototype.GetField(name, true, false);
                            if (res == undefined)
                                res = null;
                        }
                        if (res == null)
                        {
                            if (fast)
                                return undefined;
                            res = new JSObject()
                            {
                                assignCallback = () =>
                                {
                                    if (fields == null)
                                        fields = new Dictionary<string, JSObject>();
                                    fields[name] = res;
                                    res.assignCallback = null;
                                },
                                ValueType = JSObjectType.NotExistInObject
                            };
                        }
                        else if (fromProto && !fast)
                        {
                            var t = new JSObject() { ValueType = JSObjectType.NotExistInObject };
                            t.Assign(res);
                            t.assignCallback = () =>
                            {
                                if (fields == null)
                                    fields = new Dictionary<string, JSObject>();
                                fields[name] = t;
                                t.assignCallback = null;
                            };
                            t.attributes = res.attributes;
                            res = t;
                        }
                        if (res.ValueType == JSObjectType.NotExist)
                            res.ValueType = JSObjectType.NotExistInObject;
                        return res;
                    }
            }
        }

        [Modules.Hidden]
        public void Protect()
        {
            if (assignCallback != null)
                assignCallback();
            attributes |= ObjectAttributes.DontDelete | ObjectAttributes.ReadOnly;
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
        public void Assign(JSObject right)
        {
            if (this.assignCallback != null)
                this.assignCallback();
            if ((attributes & ObjectAttributes.ReadOnly) != 0)
                return;
            if (right == this)
                return;
            if (right != null)
            {
                this.ValueType = right.ValueType;
                this.iValue = right.iValue;
                this.oValue = right.oValue;
                this.dValue = right.dValue;
                this.prototype = right.prototype;
                this.fields = right.fields;
                this.attributes = this.attributes & ~ObjectAttributes.Immutable | (right.attributes & ObjectAttributes.Immutable);
                return;
            }
            this.fields = null;
            this.prototype = null;
            this.ValueType = JSObjectType.Undefined;
            this.oValue = null;
            this.prototype = null;
        }

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
            if (ValueType < JSObjectType.Object)
                GetField("__proto__", true, true);
            else if (oValue is JSObject)
                return oValue.ToString();
            var res = ToPrimitiveValue_String_Value().Value;
            if (res is bool)
                return (bool)res ? "true":"false";
            if (res is double)
                return Tools.DoubleToString((double)res);
            return (res ?? "null").ToString();
        }

        [Modules.Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Modules.Hidden]
        public virtual IEnumerator<string> GetEnumerator()
        {
            if (ValueType < JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't enumerate properties of undefined.")));
            if (ValueType >= JSObjectType.Object)
            {
                if (oValue is JSObject)
                    return (oValue as JSObject).GetEnumerator();
            }
            if (fields == null)
                return EmptyEnumerator;
            return fields.Keys.GetEnumerator();
        }

        public virtual JSObject toString()
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
                            if ((this.oValue as TypeProxy).hostedType == typeof(RegExp))
                                return "[object Object]";
                            return "[object " + (this.oValue as TypeProxy).hostedType.Name + "]";
                        }
                        if (this.oValue is string)
                            return this.oValue as string;
                        if (this.oValue != null)
                            return "[object " + this.oValue.GetType().Name + "]";
                        else
                            return "[object Null]";
                    }
                default: throw new NotImplementedException();
            }
        }

        public virtual JSObject toLocaleString()
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
                        throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError("toLocaleString called on undefined value")));
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
                            if ((this.oValue as TypeProxy).hostedType == typeof(RegExp))
                                return "[object Object]";
                            return "[object " + (this.oValue as TypeProxy).hostedType.Name + "]";
                        }
                        if (this.oValue is string)
                            return this.oValue as string;
                        if (this.oValue != null)
                            return "[object " + this.oValue.GetType().Name + "]";
                        else
                            throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError("toLocaleString called on null")));
                    }
                default: throw new NotImplementedException();
            }
        }

        public virtual JSObject valueOf()
        {
            if (ValueType >= JSObjectType.Object && oValue is JSObject)
                return (oValue as JSObject).valueOf();
            else
                return this;
        }

        public virtual JSObject isPrototypeOf(JSObject args)
        {
            if (args.GetField("length", true, false).iValue == 0)
                return false;
            var a = args.GetField("0", true, false);
            var c = this;
            JSObject o = false;
            o.ValueType = JSObjectType.Bool;
            o.iValue = 0;
            if (c.ValueType >= JSObjectType.Object && c.oValue != null)
            {
                bool tpmode = c.oValue is TypeProxy;
                Type type = null;
                if (tpmode)
                    type = (c.oValue as TypeProxy).hostedType;
                while (a.ValueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == c.oValue || (tpmode && a.oValue is TypeProxy && (a.oValue as TypeProxy).hostedType == type))
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true, false);
                }
            }
            return o;
        }

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
            return new BaseTypes.String(value);
        }

        public static implicit operator JSObject(object[] value)
        {
            return new JSObject() { ValueType = JSObjectType.Object, oValue = value, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(CallableField value)
        {
            return new JSObject() { ValueType = JSObjectType.Function, oValue = new ExternalFunction(value), assignCallback = ErrorAssignCallback };
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
