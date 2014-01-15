using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace NiL.JS.Core
{
    public delegate JSObject CallableField(Context context, JSObject[] args);

    internal enum ObjectValueType : int
    {
        NotExist = 0,
        NotExistInObject = 1,
        Undefined = 2,
        Bool = 3,
        Int = 4,
        Double = 5,
        String = 6,
        Object = 7,
        Statement = 8,
        Date = 9,
        Property = 10
    }

    [Flags]
    internal enum ObjectAttributes : int
    {
        None = 0,
        DontEnum = 1,
        DontDelete = 2
    }

    public class JSObject : IEnumerable<string>, IEnumerable
    {
        private static readonly System.Reflection.MemberInfo DefaultGetter = typeof(JSObject).GetMethod("DefaultFieldGetter", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        [Modules.Hidden]
        private static readonly Number tempNumber = new Number() { attributes = ObjectAttributes.DontDelete };
        [Modules.Hidden]
        private static readonly BaseTypes.String tempString = new BaseTypes.String() { attributes = ObjectAttributes.DontDelete };
        [Modules.Hidden]
        private static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Modules.Hidden]
        internal static readonly Func<bool> ErrorAssignCallback = () => { throw new InvalidOperationException("Invalid left-hand side"); };
        [Modules.Hidden]
        internal static readonly JSObject undefined = new JSObject() { ValueType = ObjectValueType.Undefined };
        [Modules.Hidden]
        internal static readonly JSObject Null = new JSObject() { ValueType = ObjectValueType.Object, oValue = null, assignCallback = ErrorAssignCallback };

        static JSObject()
        {
            undefined.assignCallback = null;
            undefined.Protect();
        }

        [Modules.Hidden]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal Func<bool> assignCallback;
        [Modules.Hidden]
        internal JSObject firstContainer;
        [Modules.Hidden]
        internal JSObject prototype;
        [Modules.Hidden]
        internal Dictionary<string, JSObject> fields;

        [Modules.Hidden]
        internal ObjectValueType ValueType;
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
                    case ObjectValueType.Property:
                        return (oValue as ContextStatement[])[1].Invoke(null).Value;
                    case ObjectValueType.Bool:
                        return iValue != 0;
                    case ObjectValueType.Int:
                        return iValue;
                    case ObjectValueType.Double:
                        return dValue;
                    case ObjectValueType.String:
                    case ObjectValueType.Object:
                    case ObjectValueType.Statement:
                        return oValue;
                    case ObjectValueType.Undefined:
                    case ObjectValueType.NotExistInObject:
                    default:
                        return null;
                }
            }
            set
            {
                switch (ValueType)
                {
                    case ObjectValueType.Property:
                        {
                            JSObject val;
                            if (value == null)
                                val = null;
                            else if (value is JSObject)
                                val = value as JSObject;
                            else if (value is int)
                                val = (int)value;
                            else if (value is long)
                                val = (long)value;
                            else if (value is double)
                                val = (double)value;
                            else if (value is string)
                                val = (string)value;
                            else if (value is bool)
                                val = (bool)value;
                            else if (value is ContextStatement)
                                val = (JSObject)(ContextStatement)value;
                            else val = NiL.JS.Core.TypeProxy.Proxy(value);
                            (oValue as ContextStatement[])[0].Invoke(new JSObject[] { val });
                            break;
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public JSObject()
        {
            ValueType = ObjectValueType.Undefined;
        }

        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        [Modules.Hidden]
        public JSObject GetField(string name)
        {
            return GetField(name, false, false);
        }

        [Modules.Hidden]
        public JSObject GetField(string name, bool fast)
        {
            return GetField(name, fast, false);
        }

        [Modules.Hidden]
        public virtual JSObject GetField(string name, bool fast, bool own)
        {
            if (firstContainer == null)
                return DefaultFieldGetter(name, fast, own);
            return firstContainer.GetField(name, fast, own);
        }

        [Modules.Hidden]
        protected JSObject DefaultFieldGetter(string name, bool fast, bool own)
        {
            switch (ValueType)
            {
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                case ObjectValueType.Undefined:
                case ObjectValueType.NotExistInObject:
                    throw new InvalidOperationException("Can't access to property value of \"undefined\".");
                case ObjectValueType.Int:
                case ObjectValueType.Double:
                    {
                        tempNumber.iValue = iValue;
                        tempNumber.dValue = dValue;
                        tempNumber.ValueType = ValueType;
                        firstContainer = tempNumber;
                        return tempNumber.GetField(name, true, own);
                    }
                case ObjectValueType.String:
                    {
                        tempString.oValue = oValue;
                        tempString.length.iValue = (oValue as string).Length;
                        firstContainer = tempString;
                        return tempString.GetField(name, true, own);
                    }
                case ObjectValueType.Bool:
                    {
                        fast = true;
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                case ObjectValueType.Property:
                case ObjectValueType.Statement:
                    {
                        if (oValue == null)
                            throw new InvalidOperationException("Can't access to property value of \"null\"");
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            if (name == "__proto__")
                return prototype ?? (fast ? Null : prototype = new JSObject(false) { ValueType = ObjectValueType.Object, oValue = null });
            JSObject res = null;
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && !own;
            if (fromProto && prototype != null)
            {
                res = prototype.GetField(name, true);
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
                        return true;
                    },
                    ValueType = ObjectValueType.NotExistInObject
                };
            }
            else if (fromProto && !fast)
            {
                var t = new JSObject() { ValueType = ObjectValueType.NotExistInObject };
                t.Assign(res);
                t.assignCallback = () =>
                {
                    if (fields == null)
                        fields = new Dictionary<string, JSObject>();
                    fields[name] = t;
                    t.assignCallback = null;
                    return true;
                };
                res = t;
            }
            if (res.ValueType == ObjectValueType.NotExist)
                res.ValueType = ObjectValueType.NotExistInObject;
            return res;
        }

        [Modules.Hidden]
        public void Protect()
        {
            if (assignCallback != null)
                assignCallback();
            assignCallback = () => { return false; };
        }

        [Modules.Hidden]
        internal JSObject ToPrimitiveValue_Value_String(Context context)
        {
            var otb = context.thisBind;
            context.thisBind = this;
            try
            {
                if ((ValueType >= ObjectValueType.Object) && (oValue != null))
                {
                    var tpvs = GetField("valueOf", true);
                    JSObject res = null;
                    if (tpvs.ValueType == ObjectValueType.Statement)
                    {
                        res = (tpvs.oValue as Statement).Invoke(context, null);
                        if (res.ValueType == ObjectValueType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > ObjectValueType.Undefined && res.ValueType < ObjectValueType.Object)
                            return res;
                    }
                    tpvs = GetField("toString", true);
                    if (tpvs.ValueType == ObjectValueType.Statement)
                    {
                        res = (tpvs.oValue as Statement).Invoke(context, null);
                        if (res.ValueType == ObjectValueType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > ObjectValueType.Undefined && res.ValueType < ObjectValueType.Object)
                            return res;
                    }
                    context.thisBind = otb;
                    throw new JSException(Context.eval(context, new[] { new JSObject() {
                        ValueType = ObjectValueType.String,
                        oValue = "{ message: 'Can not convert object to primitive value' }" 
                    } }));
                }
                else
                    context.thisBind = otb;
            }
            catch
            {
                context.thisBind = otb;
                throw;
            }
            return this;
        }

        [Modules.Hidden]
        internal JSObject ToPrimitiveValue_String_Value(Context context)
        {
            var otb = context.thisBind;
            context.thisBind = this;
            try
            {
                if ((ValueType >= ObjectValueType.Object) && (oValue != null))
                {
                    var tpvs = GetField("toString", true);
                    JSObject res = null;
                    if (tpvs.ValueType == ObjectValueType.Statement)
                    {
                        res = (tpvs.oValue as Statement).Invoke(context, null);
                        if (res.ValueType == ObjectValueType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > ObjectValueType.Undefined && res.ValueType < ObjectValueType.Object)
                            return res;
                    }
                    tpvs = GetField("valueOf", true);
                    if (tpvs.ValueType == ObjectValueType.Statement)
                    {
                        res = (tpvs.oValue as Statement).Invoke(context, null);
                        if (res.ValueType == ObjectValueType.Object)
                        {
                            if (res.oValue is BaseTypes.String)
                                res = res.oValue as BaseTypes.String;
                        }
                        if (res.ValueType > ObjectValueType.Undefined && res.ValueType < ObjectValueType.Object)
                            return res;
                    }
                    context.thisBind = otb;
                    throw new JSException(Context.eval(context, new[] { new JSObject() {
                        ValueType = ObjectValueType.String,
                        oValue = "{ message: 'Can not convert object to primitive value' }" 
                    } }));
                }
                else
                    context.thisBind = otb;
            }
            catch
            {
                context.thisBind = otb;
                throw;
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
                if (!this.assignCallback())
                    return;
            if (right == this)
                return;
            if (right != null)
            {
                switch (right.ValueType)
                {
                    case ObjectValueType.Bool:
                    case ObjectValueType.Int:
                        {
                            this.iValue = right.iValue;
                            this.fields = null;
                            this.prototype = null;
                            this.firstContainer = null;
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            this.dValue = right.dValue;
                            this.fields = null;
                            this.prototype = null;
                            this.firstContainer = null;
                            break;
                        }
                    case ObjectValueType.Statement:
                    case ObjectValueType.Object:
                    case ObjectValueType.Date:
                        {
                            this.oValue = right.oValue;
                            if (oValue != null)
                                this.prototype = right.GetField("__proto__", true);
                            else
                                this.prototype = BaseObject.Prototype;
                            this.fields = right.fields;
                            this.firstContainer = right.firstContainer ?? right;
                            break;
                        }
                    case ObjectValueType.Property:
                    case ObjectValueType.String:
                        {
                            this.oValue = right.oValue;
                            this.fields = null;
                            this.prototype = null;
                            this.firstContainer = null;
                            break;
                        }
                    case ObjectValueType.Undefined:
                        {
                            this.fields = null;
                            this.prototype = null;
                            this.firstContainer = null;
                            break;
                        }
                    default: throw new InvalidOperationException();
                }
                this.ValueType = right.ValueType;
                return;
            }
            this.prototype = null;
            this.ValueType = ObjectValueType.Undefined;
            this.firstContainer = null;
        }

        [Modules.Hidden]
        public void Delete()
        {
            ValueType = ObjectValueType.NotExist;
        }

        [Modules.Hidden]
        public override string ToString()
        {
            if (ValueType <= ObjectValueType.Undefined)
                return "undefined";
            if (ValueType < ObjectValueType.Object)
                GetField("__proto__", true, true);
            if (firstContainer != null)
                return firstContainer.ToString();
            var tstr = GetField("toString", true);
            if (tstr.ValueType == ObjectValueType.Statement)
                return (tstr.oValue as ContextStatement).Invoke(null).oValue as string;
            tstr = GetField("valueOf", true);
            if (tstr.ValueType == ObjectValueType.Statement)
                return (tstr.oValue as ContextStatement).Invoke(null).Value.ToString();
            return "" + (Value ?? "null");
        }

        [Modules.Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Modules.Hidden]
        public virtual IEnumerator<string> GetEnumerator()
        {
            if (firstContainer != null)
                return firstContainer.GetEnumerator();
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
            return new JSObject() { ValueType = ObjectValueType.Bool, iValue = value ? 1 : 0, assignCallback = ErrorAssignCallback };
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
            return new JSObject() { ValueType = ObjectValueType.Object, oValue = value, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(ContextStatement value)
        {
            return new JSObject() { ValueType = ObjectValueType.Statement, oValue = value, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(CallableField value)
        {
            return new JSObject() { ValueType = ObjectValueType.Statement, oValue = new ExternalFunction(value), assignCallback = ErrorAssignCallback };
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static explicit operator bool(JSObject obj)
        {
            var vt = obj.ValueType;
            if (vt == ObjectValueType.Int || vt == ObjectValueType.Bool)
                return obj.iValue != 0;
            if (vt == ObjectValueType.Double)
                return obj.dValue != 0.0;
            return (obj.oValue != null) && ((vt != ObjectValueType.String) || !string.IsNullOrEmpty(obj.oValue as string));
        }
    }
}
