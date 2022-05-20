using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Backward;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Interop
{
    [Prototype(typeof(BaseLibrary.Array))]
    public sealed class NativeList : CustomType, IIterable
    {
        private sealed class Element : JSValue
        {
            private readonly NativeList _owner;
            private int _index;

            public Element(NativeList owner, int index)
            {
                _owner = owner;
                _index = index;
                _attributes |= JSValueAttributesInternal.Reassign;
                var value = owner._data[index];
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
                                base.Assign(new ObjectWrapper(new Date(dateTime)));
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
                                if (value is Delegate @delegate)
                                {
                                    var context = Context.CurrentGlobalContext;
#if (PORTABLE || NETCORE)
                                    _oValue = new MethodProxy(context, ((Delegate)value).GetMethodInfo(), ((Delegate)value).Target);
#else
                                    _oValue = new MethodProxy(context, @delegate.Method, @delegate.Target);
#endif
                                    _valueType = JSValueType.Function;
                                }
                                else if (value is IList list)
                                {
                                    _oValue = new NativeList(list);
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
                _owner._data[_index] = value.Value;
            }
        }

        private readonly Number _lenObj;
        private readonly IList _data;
        private readonly Type _elementType;

        public override object Value
        {
            get => _data;
            protected set { }
        }

        [Hidden]
        public NativeList()
        {
            _data = new List<object>();
            _elementType = typeof(object);
            _lenObj = new Number(0);
        }

        [Hidden]
        public NativeList(IList data)
        {
            _data = data;
            _elementType = data.GetType().GetElementType();
            if (_elementType == null)
            {
#if PORTABLE || NETCORE
                var @interface = data.GetType().GetInterface(typeof(IList<>).Name);
#else
                var @interface = data.GetType().GetTypeInfo().GetInterface(typeof(IList<>).Name);
#endif
                if (@interface != null)
                    _elementType = @interface.GetGenericArguments()[0];
                else
                    _elementType = typeof(object);
            }

            _lenObj = new Number(data.Count);
        }

        public void push(Arguments args)
        {
            for (var i = 0; i < args._iValue; i++)
                _data.Add(Tools.ConvertJStoObj(args[i], _elementType, true));
        }

        public JSValue pop()
        {
            if (_data.Count == 0)
            {
                notExists._valueType = JSValueType.NotExistsInObject;
                return notExists;
            }

            var result = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);
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
                    _lenObj._iValue = _data.Count;
                    return _lenObj;
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
                        isIndex = tname._dValue >= 0
                            && tname._dValue < uint.MaxValue
                            && (long)tname._dValue == tname._dValue;

                        if (isIndex)
                            index = (int)(uint)tname._dValue;

                        break;
                    }
                    case JSValueType.String:
                    {
                        var str = tname._oValue.ToString();
                        if (str.Length > 0)
                        {
                            var fc = str[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var si = 0;
                                if (Tools.ParseJsNumber(tname._oValue.ToString(), ref si, out double dindex)
                                    && (si == tname._oValue.ToString().Length)
                                    && dindex >= 0
                                    && dindex < uint.MaxValue
                                    && (long)dindex == dindex)
                                {
                                    isIndex = true;
                                    index = (int)(uint)dindex;
                                }
                            }
                        }
                        break;
                    }
                }

                if (isIndex && index >= 0 && index < _data.Count)
                {
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
                        var str = tname._oValue.ToString();
                        if (str.Length > 0)
                        {
                            var fc = str[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var dindex = 0.0;
                                int si = 0;
                                if (Tools.ParseJsNumber(tname._oValue.ToString(), ref si, out dindex)
                                    && (si == tname._oValue.ToString().Length)
                                    && dindex >= 0
                                    && dindex < uint.MaxValue
                                    && (long)dindex == dindex)
                                {
                                    isIndex = true;
                                    index = (int)(uint)dindex;
                                }
                            }
                        }
                        break;
                    }
                }

                if (isIndex)
                {
                    notExists._valueType = JSValueType.NotExistsInObject;
                    if (index < 0 || index > _data.Count)
                        return;

                    _data[index] = value.Value;
                    return;
                }
            }

            SetProperty(key, value, strict);
        }

        public BaseLibrary.Array toJSON()
        {
            var context = Context.CurrentGlobalContext;
            var result = new BaseLibrary.Array();
            for (var i = 0; i < _data.Count; i++)
            {
                result[i] = context.ProxyValue(_data[i]);
            }

            return result;
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            if (propertyScope is PropertyScope.Own or PropertyScope.Common)
            {
                for (var i = 0; i < _data.Count; i++)
                    yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumerationMode > 0 ? new Element(this, i) : null);
            }

            for (var e = base.GetEnumerator(hideNonEnumerable, enumerationMode, propertyScope); e.MoveNext();)
                yield return e.Current;
        }

        public IIterator iterator()
        {
            return _data.GetEnumerator().AsIterator();
        }
    }
}
