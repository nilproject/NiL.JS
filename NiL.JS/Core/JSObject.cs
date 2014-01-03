using NiL.JS.Core;
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
        [Modules.Protected]
        private static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        internal static readonly Func<bool> ErrorAssignCallback = () => { throw new InvalidOperationException("Invalid left-hand side"); };
        internal static readonly JSObject undefined = new JSObject() { ValueType = ObjectValueType.Undefined };
        internal static readonly JSObject Null = new JSObject() { ValueType = ObjectValueType.Object, oValue = null, assignCallback = ErrorAssignCallback };

        static JSObject()
        {
            undefined.assignCallback = null;
            undefined.Protect();
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal Func<bool> assignCallback;
        internal Func<string, bool, JSObject> fieldGetter;
        internal Func<IEnumerator<string>> enumeratorGetter;
        internal JSObject prototype;
        internal Dictionary<string, JSObject> fields;

        internal bool temporary;
        internal ObjectValueType ValueType;
        internal int iValue;
        internal double dValue;
        internal object oValue;
        internal ObjectAttributes attributes;

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
                            else val = new NiL.JS.Modules.ClassProxy(value);
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
            enumeratorGetter = null;
            ValueType = ObjectValueType.Undefined;
        }

        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        public JSObject GetField(string name)
        {
            if (fieldGetter == null)
                fieldGetter = DefaultFieldGetter;
            return fieldGetter(name, false);
        }

        public virtual JSObject GetField(string name, bool fast)
        {
            if (fieldGetter == null || fieldGetter.Method == DefaultGetter)
                return DefaultFieldGetter(name, fast);
            return fieldGetter(name, fast);
        }

        protected JSObject DefaultFieldGetter(string name, bool fast)
        {
            if (ValueType == ObjectValueType.Undefined)
                throw new InvalidOperationException("Can't access to property value of \"undefined\"");
            if ((int)ValueType < (int)ObjectValueType.Object)
                fast = true;
            else if (oValue == null)
                throw new InvalidOperationException("Can't access to property value of \"null\"");
            if (name == "__proto__")
                return prototype ?? Null;
            JSObject res = null;
            bool fromProto = fields == null || !fields.TryGetValue(name, out res);
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

        public void Protect()
        {
            if (assignCallback != null)
                assignCallback();
            assignCallback = () => { return false; };
        }

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
        public void Assign(JSObject right)
        {
            if (this.assignCallback != null)
                if (!this.assignCallback())
                    return;
            if (ValueType == ObjectValueType.Property)
            {
                (oValue as ContextStatement[])[0].Invoke(new JSObject[] { right });
                return;
            }
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
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            this.dValue = right.dValue;
                            break;
                        }
                    case ObjectValueType.Property:
                    case ObjectValueType.Date:
                    case ObjectValueType.Statement:
                    case ObjectValueType.Object:
                    case ObjectValueType.String:
                        {
                            this.oValue = right.oValue;
                            break;
                        }
                    case ObjectValueType.Undefined:
                        {
                            break;
                        }
                    default: throw new InvalidOperationException();
                }
                this.prototype = right.prototype;
                this.ValueType = right.ValueType;
                this.fields = right.fields;
                this.fieldGetter = right.fieldGetter;
                return;
            }
            this.prototype = null;
            this.ValueType = ObjectValueType.Undefined;
            this.fieldGetter = null;
        }

        public void Delete()
        {
            ValueType = ObjectValueType.NotExist;
        }

        public override string ToString()
        {
            if (ValueType <= ObjectValueType.Undefined)
                return "undefined";
            var tstr = GetField("toString", true);
            if (tstr.ValueType == ObjectValueType.Statement)
                return (tstr.oValue as ContextStatement).Invoke(null).oValue as string;
            return "" + (Value ?? "null");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (enumeratorGetter == null)
            {
                if (fields == null)
                    return new string[0].GetEnumerator();
                return fields.Keys.GetEnumerator();
            }
            else
                return enumeratorGetter();
        }

        public virtual IEnumerator<string> GetEnumerator()
        {
            if (enumeratorGetter == null)
            {
                if (fields == null)
                    return EmptyEnumerator;
                return fields.Keys.GetEnumerator();
            }
            else
                return enumeratorGetter();
        }

        public static implicit operator JSObject(char value)
        {
            return new JSObject() { ValueType = ObjectValueType.String, oValue = value.ToString(), temporary = true, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(bool value)
        {
            return new JSObject() { ValueType = ObjectValueType.Bool, iValue = value ? 1 : 0, temporary = true, assignCallback = ErrorAssignCallback };
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
            return new JSObject() { ValueType = ObjectValueType.Object, oValue = value, temporary = true, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(ContextStatement value)
        {
            return new JSObject() { ValueType = ObjectValueType.Statement, oValue = value, temporary = true, assignCallback = ErrorAssignCallback };
        }

        public static implicit operator JSObject(CallableField value)
        {
            return new JSObject() { ValueType = ObjectValueType.Statement, oValue = new Statements.ExternalFunction(value), temporary = true, assignCallback = ErrorAssignCallback };
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
