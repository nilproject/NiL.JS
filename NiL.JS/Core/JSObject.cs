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
        Property = 10,
        Proxy = 11
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
        private static readonly System.Reflection.MemberInfo DefaultGetter = typeof(JSObject).GetMethod("DefaultFieldGetter", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        [Modules.Hidden]
        internal static readonly Action ErrorAssignCallback = () => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Modules.Hidden]
        protected static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Modules.Hidden]
        internal static readonly JSObject undefined = new JSObject() { ValueType = JSObjectType.Undefined };
        [Modules.Hidden]
        internal static readonly JSObject Null = new JSObject() { ValueType = JSObjectType.Object, oValue = null, assignCallback = ErrorAssignCallback };
        [Modules.Hidden]
        internal static readonly JSObject nullString = "null";

        static JSObject()
        {
            undefined.assignCallback = null;
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
                    case JSObjectType.Proxy:
                        return oValue;
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                    default:
                        return null;
                }
            }
        }

        public JSObject()
        {
            ValueType = JSObjectType.Undefined;
        }

        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        [Modules.Hidden]
        public virtual JSObject GetField(string name, bool fast, bool own)
        {
            if (ValueType >= JSObjectType.Object && oValue is JSObject)
                return (oValue as JSObject).GetField(name, fast, own);
            return DefaultFieldGetter(name, fast, own);
        }

        [Modules.Hidden]
        protected JSObject DefaultFieldGetter(string name, bool fast, bool own)
        {
            switch (ValueType)
            {
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Varible not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"undefined\".")));
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        prototype = TypeProxy.GetPrototype(typeof(Number));
                        fast = true;
                        break;
                    }
                case JSObjectType.String:
                    {
                        prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.String));
                        fast = true;
                        break;
                    }
                case JSObjectType.Bool:
                    {
                        prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.Boolean));
                        fast = true;
                        break;
                    }
                case JSObjectType.Proxy:
                    {
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't access to property value of \"null\"")));
                        if (this is TypeProxy)
                            break;
                        return TypeProxy.GetPrototype(oValue as Type).GetField(name, fast, own);
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
                            prototype = TypeProxy.GetPrototype(typeof(NiL.JS.Core.BaseTypes.String)).Clone() as JSObject;
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
                        bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && !own;
                        if (fromProto && prototype != null)
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
        internal JSObject ToPrimitiveValue_Value_String(Context context)
        {
            var otb = context.thisBind;
            context.thisBind = this;
            try
            {
                if (ValueType >= JSObjectType.Object && oValue != null)
                {
                    if (oValue == null)
                        return nullString;
                    var tpvs = GetField("valueOf", true, false);
                    JSObject res = null;
                    if (tpvs.ValueType == JSObjectType.Function)
                    {
                        res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(context, null);
                        if (res.ValueType == JSObjectType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > JSObjectType.Undefined && res.ValueType < JSObjectType.Object)
                            return res;
                    }
                    tpvs = GetField("toString", true, false);
                    if (tpvs.ValueType == JSObjectType.Function)
                    {
                        res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(context, null);
                        if (res.ValueType == JSObjectType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > JSObjectType.Undefined && res.ValueType < JSObjectType.Object)
                            return res;
                    }
                    context.thisBind = otb;
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
                }
                return this;
            }
            finally
            {
                context.thisBind = otb;
            }
        }

        [Modules.Hidden]
        internal JSObject ToPrimitiveValue_String_Value(Context context)
        {
            var otb = context.thisBind;
            context.thisBind = this;
            try
            {
                if (ValueType >= JSObjectType.Object && oValue != null)
                {
                    if (oValue == null)
                        return nullString;
                    var tpvs = GetField("toString", true, false);
                    JSObject res = null;
                    if (tpvs.ValueType == JSObjectType.Function)
                    {
                        res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(context, null);
                        if (res.ValueType == JSObjectType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > JSObjectType.Undefined && res.ValueType < JSObjectType.Object)
                            return res;
                    }
                    tpvs = GetField("valueOf", true, false);
                    if (tpvs.ValueType == JSObjectType.Function)
                    {
                        res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(context, null);
                        if (res.ValueType == JSObjectType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > JSObjectType.Undefined && res.ValueType < JSObjectType.Object)
                            return res;
                    }
                    context.thisBind = otb;
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
                }
                return this;
            }
            finally
            {
                context.thisBind = otb;
            }
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
            var res = ToPrimitiveValue_String_Value(new Context(Context.currentRootContext) { thisBind = this }).Value;
            if (res is bool)
                return (bool)res ? "true":"false";
            if (res is double)
                return Tools.DoubleToString((double)res);
            return (res ?? "null").ToString();
        }

        public virtual string Stringify()
        {
            if (ValueType <= JSObjectType.Undefined)
                return "undefined";
            switch (ValueType)
            {
                case JSObjectType.Bool:
                    return iValue != 0 ? "true" : "false";
                case JSObjectType.Double:
                    return dValue.ToString();
                case JSObjectType.Int:
                    return iValue.ToString();
                case JSObjectType.String:
                    return "\"" + oValue + "\"";
            }
            return "<" + ValueType + ">";
        }

        [Modules.Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Modules.Hidden]
        public virtual IEnumerator<string> GetEnumerator()
        {
            if (ValueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't enumerate properties of undefined.")));
            if (ValueType >= JSObjectType.Object && oValue is JSObject)
                return (oValue as JSObject).GetEnumerator();
            if (fields == null)
                return EmptyEnumerator;
            return fields.Keys.GetEnumerator();
        }

        public static implicit operator JSObject(char value)
        {
            return new BaseTypes.String(value.ToString());
        }

        public static implicit operator JSObject(bool value)
        {
            return new JSObject() { ValueType = JSObjectType.Bool, iValue = value ? 1 : 0, assignCallback = ErrorAssignCallback };
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
                case JSObjectType.Proxy:
                    return obj.oValue != null;
                case JSObjectType.String:
                    return !string.IsNullOrEmpty(obj.oValue as string);
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return false;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("varible not defined")));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
