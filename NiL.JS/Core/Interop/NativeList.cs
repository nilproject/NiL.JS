using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Interop
{
    [Prototype(typeof(BaseLibrary.Array))]
    public sealed class NativeList : CustomType
    {
        private sealed class Element : JSValue
        {
            private readonly NativeList owner;
            private int index;

            public Element(NativeList owner, int index)
            {
                this.owner = owner;
                this.index = index;
                attributes |= JSValueAttributesInternal.Reassign;
                var value = owner.data[index];
                valueType = JSValueType.Undefined;
                if (value != null)
                {
                    if (value is JSValue)
                        base.Assign(value as JSValue);
                    else
                    {
#if PORTABLE
                        switch (value.GetType().GetTypeCode())
#else
                        switch (Type.GetTypeCode(value.GetType()))
#endif
                        {
                            case TypeCode.Boolean:
                                {
                                    iValue = (bool)value ? 1 : 0;
                                    valueType = JSValueType.Boolean;
                                    break;
                                }
                            case TypeCode.Byte:
                                {
                                    iValue = (byte)value;
                                    valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Char:
                                {
                                    oValue = ((char)value).ToString();
                                    valueType = JSValueType.String;
                                    break;
                                }
                            case TypeCode.DateTime:
                                {
                                    var dateTime = (DateTime)value;
                                    base.Assign(new ObjectWrapper(new Date(dateTime.ToUniversalTime().Ticks, dateTime.ToLocalTime().Ticks - dateTime.ToUniversalTime().Ticks)));
                                    break;
                                }
                            case TypeCode.Decimal:
                                {
                                    dValue = (double)(decimal)value;
                                    valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.Double:
                                {
                                    dValue = (double)value;
                                    valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.Int16:
                                {
                                    iValue = (short)value;
                                    valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Int32:
                                {
                                    iValue = (int)value;
                                    valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Int64:
                                {
                                    dValue = (long)value;
                                    valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.SByte:
                                {
                                    iValue = (sbyte)value;
                                    valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Single:
                                {
                                    dValue = (float)value;
                                    valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.String:
                                {
                                    oValue = value;
                                    valueType = JSValueType.String;
                                    break;
                                }
                            case TypeCode.UInt16:
                                {
                                    iValue = (ushort)value;
                                    valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.UInt32:
                                {
                                    var v = (uint)value;
                                    if (v > int.MaxValue)
                                    {
                                        dValue = v;
                                        valueType = JSValueType.Double;
                                    }
                                    else
                                    {
                                        iValue = (int)v;
                                        valueType = JSValueType.Integer;
                                    }
                                    break;
                                }
                            case TypeCode.UInt64:
                                {
                                    var v = (long)value;
                                    if (v > int.MaxValue)
                                    {
                                        dValue = v;
                                        valueType = JSValueType.Double;
                                    }
                                    else
                                    {
                                        iValue = (int)v;
                                        valueType = JSValueType.Integer;
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (value is Delegate)
                                    {
#if (PORTABLE || NETCORE)
                                        oValue = new MethodProxy(((Delegate)value).GetMethodInfo(), ((Delegate)value).Target);
#else
                                        oValue = new MethodProxy(((Delegate)value).Method, ((Delegate)value).Target);
#endif
                                        valueType = JSValueType.Function;
                                    }
                                    else if (value is IList)
                                    {
                                        oValue = new NativeList(value as IList);
                                        valueType = JSValueType.Object;
                                    }
                                    else
                                    {
                                        oValue = Marshal(value);
                                        valueType = JSValueType.Object;
                                    }
                                    break;
                                }
                        }
                    }
                }
            }

            public override void Assign(JSValue value)
            {
                owner.data[index] = value.Value;
            }
        }

        private readonly Number lenObj;
        private readonly IList data;
        private readonly Type elementType;

        public override object Value
        {
            get
            {
                return data;
            }

            protected set
            {

            }
        }

        [Hidden]
        public NativeList()
        {
            this.data = new List<object>();
            elementType = typeof(object);
            lenObj = new Number(0);
        }

        [Hidden]
        public NativeList(IList data)
        {
            this.data = data;
            this.elementType = data.GetType().GetElementType();
            if (elementType == null)
            {
#if PORTABLE
                var @interface = data.GetType().GetInterface(typeof(IList<>).Name);
#else
                var @interface = data.GetType().GetTypeInfo().GetInterface(typeof(IList<>).Name);
#endif
                if (@interface != null)
                    elementType = @interface.GetGenericArguments()[0];
                else
                    elementType = typeof(object);
            }

            lenObj = new Number(data.Count);
        }

        public void push(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
                data.Add(Tools.convertJStoObj(args[i], elementType, true));
        }

        public JSValue pop()
        {
            if (data.Count == 0)
            {
                notExists.valueType = JSValueType.NotExistsInObject;
                return notExists;
            }
            var result = data[data.Count - 1];
            data.RemoveAt(data.Count - 1);
            if (result is IList)
                return new NativeList(result as IList);
            else
                return TypeProxy.Proxy(result);
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key.valueType != JSValueType.Symbol)
            {
                forWrite &= (attributes & JSValueAttributesInternal.Immutable) == 0;
                if (key.valueType == JSValueType.String && string.CompareOrdinal("length", key.oValue.ToString()) == 0)
                {
                    lenObj.iValue = data.Count;
                    return lenObj;
                }
                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname.valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();
                switch (tname.valueType)
                {
                    case JSValueType.Integer:
                        {
                            isIndex = tname.iValue >= 0;
                            index = tname.iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                            if (isIndex)
                                index = (int)(uint)tname.dValue;
                            break;
                        }
                    case JSValueType.String:
                        {
                            var fc = tname.oValue.ToString()[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var dindex = 0.0;
                                int si = 0;
                                if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                    && (si == tname.oValue.ToString().Length)
                                    && dindex >= 0
                                    && dindex < uint.MaxValue
                                    && (long)dindex == dindex)
                                {
                                    isIndex = true;
                                    index = (int)(uint)dindex;
                                }
                            }
                            break;
                        }
                }
                if (isIndex)
                {
                    notExists.valueType = JSValueType.NotExistsInObject;
                    if (index < 0 || index > data.Count)
                        return notExists;
                    return new Element(this, index);
                }
            }
            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override void SetProperty(JSValue key, JSValue value, PropertyScope memberScope, bool strict)
        {
            if (key.valueType != JSValueType.Symbol)
            {
                if (key.valueType == JSValueType.String && string.CompareOrdinal("length", key.oValue.ToString()) == 0)
                    return;
                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname.valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();
                switch (tname.valueType)
                {
                    case JSValueType.Integer:
                        {
                            isIndex = tname.iValue >= 0;
                            index = tname.iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                            if (isIndex)
                                index = (int)(uint)tname.dValue;
                            break;
                        }
                    case JSValueType.String:
                        {
                            var fc = tname.oValue.ToString()[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var dindex = 0.0;
                                int si = 0;
                                if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                    && (si == tname.oValue.ToString().Length)
                                    && dindex >= 0
                                    && dindex < uint.MaxValue
                                    && (long)dindex == dindex)
                                {
                                    isIndex = true;
                                    index = (int)(uint)dindex;
                                }
                            }
                            break;
                        }
                }
                if (isIndex)
                {
                    notExists.valueType = JSValueType.NotExistsInObject;
                    if (index < 0 || index > data.Count)
                        return;
                    data[index] = value.Value;
                    return;
                }
            }

            base.SetProperty(key, value, strict);
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            for (var i = 0; i < data.Count; i++)
                yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumerationMode > 0 ? new Element(this, i) : null);

            for (var e = base.GetEnumerator(hideNonEnumerable, enumerationMode); e.MoveNext();)
                yield return e.Current;
        }
    }
}
