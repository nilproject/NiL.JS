using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;

#if NET40 || NETCORE
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
                _attributes |= JSValueAttributesInternal.Reassign;
                var value = owner.data[index];
                _valueType = JSValueType.Undefined;
                if (value != null)
                {
                    if (value is JSValue)
                        base.Assign(value as JSValue);
                    else
                    {
#if PORTABLE || NETCORE
                        switch (value.GetType().GetTypeCode())
#else
                        switch (Type.GetTypeCode(value.GetType()))
#endif
                        {
                            case TypeCode.Boolean:
                                {
                                    _iValue = (bool)value ? 1 : 0;
                                    _valueType = JSValueType.Boolean;
                                    break;
                                }
                            case TypeCode.Byte:
                                {
                                    _iValue = (byte)value;
                                    _valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Char:
                                {
                                    _oValue = ((char)value).ToString();
                                    _valueType = JSValueType.String;
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
                                    _dValue = (double)(decimal)value;
                                    _valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.Double:
                                {
                                    _dValue = (double)value;
                                    _valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.Int16:
                                {
                                    _iValue = (short)value;
                                    _valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Int32:
                                {
                                    _iValue = (int)value;
                                    _valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Int64:
                                {
                                    _dValue = (long)value;
                                    _valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.SByte:
                                {
                                    _iValue = (sbyte)value;
                                    _valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.Single:
                                {
                                    _dValue = (float)value;
                                    _valueType = JSValueType.Double;
                                    break;
                                }
                            case TypeCode.String:
                                {
                                    _oValue = value;
                                    _valueType = JSValueType.String;
                                    break;
                                }
                            case TypeCode.UInt16:
                                {
                                    _iValue = (ushort)value;
                                    _valueType = JSValueType.Integer;
                                    break;
                                }
                            case TypeCode.UInt32:
                                {
                                    var v = (uint)value;
                                    if (v > int.MaxValue)
                                    {
                                        _dValue = v;
                                        _valueType = JSValueType.Double;
                                    }
                                    else
                                    {
                                        _iValue = (int)v;
                                        _valueType = JSValueType.Integer;
                                    }
                                    break;
                                }
                            case TypeCode.UInt64:
                                {
                                    var v = (long)value;
                                    if (v > int.MaxValue)
                                    {
                                        _dValue = v;
                                        _valueType = JSValueType.Double;
                                    }
                                    else
                                    {
                                        _iValue = (int)v;
                                        _valueType = JSValueType.Integer;
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (value is Delegate)
                                    {
                                        var context = Context.CurrentGlobalContext;
#if (PORTABLE || NETCORE)
                                        _oValue = new MethodProxy(context, ((Delegate)value).GetMethodInfo(), ((Delegate)value).Target);
#else
                                        _oValue = new MethodProxy(context, ((Delegate)value).Method, ((Delegate)value).Target);
#endif
                                        _valueType = JSValueType.Function;
                                    }
                                    else if (value is IList)
                                    {
                                        _oValue = new NativeList(value as IList);
                                        _valueType = JSValueType.Object;
                                    }
                                    else
                                    {
                                        _oValue = Marshal(value);
                                        _valueType = JSValueType.Object;
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
#if PORTABLE || NETCORE
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
                notExists._valueType = JSValueType.NotExistsInObject;
                return notExists;
            }
            var result = data[data.Count - 1];
            data.RemoveAt(data.Count - 1);
            if (result is IList)
                return new NativeList(result as IList);
            else
                return Context.CurrentGlobalContext.ProxyValue(result);
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;
                if (key._valueType == JSValueType.String && string.CompareOrdinal("length", key._oValue.ToString()) == 0)
                {
                    lenObj._iValue = data.Count;
                    return lenObj;
                }
                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname._valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();
                switch (tname._valueType)
                {
                    case JSValueType.Integer:
                        {
                            isIndex = tname._iValue >= 0;
                            index = tname._iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            isIndex = tname._dValue >= 0 && tname._dValue < uint.MaxValue && (long)tname._dValue == tname._dValue;
                            if (isIndex)
                                index = (int)(uint)tname._dValue;
                            break;
                        }
                    case JSValueType.String:
                        {
                            var fc = tname._oValue.ToString()[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var dindex = 0.0;
                                int si = 0;
                                if (Tools.ParseNumber(tname._oValue.ToString(), ref si, out dindex)
                                    && (si == tname._oValue.ToString().Length)
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
                    notExists._valueType = JSValueType.NotExistsInObject;
                    if (index < 0 || index > data.Count)
                        return notExists;
                    return new Element(this, index);
                }
            }
            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override void SetProperty(JSValue key, JSValue value, PropertyScope memberScope, bool strict)
        {
            if (key._valueType != JSValueType.Symbol)
            {
                if (key._valueType == JSValueType.String && string.CompareOrdinal("length", key._oValue.ToString()) == 0)
                    return;
                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname._valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();
                switch (tname._valueType)
                {
                    case JSValueType.Integer:
                        {
                            isIndex = tname._iValue >= 0;
                            index = tname._iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            isIndex = tname._dValue >= 0 && tname._dValue < uint.MaxValue && (long)tname._dValue == tname._dValue;
                            if (isIndex)
                                index = (int)(uint)tname._dValue;
                            break;
                        }
                    case JSValueType.String:
                        {
                            var fc = tname._oValue.ToString()[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var dindex = 0.0;
                                int si = 0;
                                if (Tools.ParseNumber(tname._oValue.ToString(), ref si, out dindex)
                                    && (si == tname._oValue.ToString().Length)
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
                    notExists._valueType = JSValueType.NotExistsInObject;
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
