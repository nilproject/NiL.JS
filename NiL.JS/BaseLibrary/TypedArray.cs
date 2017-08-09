using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Prototype(typeof(Array))]
    public abstract class TypedArray : JSObject
    {
        protected sealed class Element : JSValue
        {
            private readonly TypedArray owner;
            private int index;

            public Element(TypedArray owner, int index)
            {
                this.owner = owner;
                this.index = index;
                _attributes |= JSValueAttributesInternal.Reassign;
            }

            public override void Assign(JSValue value)
            {
                owner[index] = value;
            }
        }

        protected abstract JSValue this[int index]
        {
            get;
            set;
        }

        [Field]
        [DoNotEnumerate]
        public ArrayBuffer buffer
        {
            [Hidden]
            get;
            private set;
        }

        [Field]
        [DoNotEnumerate]
        public int byteLength
        {
            [Hidden]
            get;
            private set;
        }

        [Field]
        [DoNotEnumerate]
        public int byteOffset
        {
            [Hidden]
            get;
            private set;
        }

        [Field]
        [DoNotEnumerate]
        public Number length
        {
            [Hidden]
            get;
            private set;
        }

        [Field]
        [DoNotEnumerate]
        public abstract int BYTES_PER_ELEMENT
        {
            [Hidden]
            get;
        }

        [Hidden]
        public abstract Type ElementType
        {
            [Hidden]
            get;
        }

        protected TypedArray()
        {
            buffer = new ArrayBuffer();

            _valueType = JSValueType.Object;
            _oValue = this;
            length = 0;
        }

        [DoNotEnumerate]
        protected TypedArray(int length)
        {
            this.length = new Number(length);
            buffer = new ArrayBuffer(length * BYTES_PER_ELEMENT);
            byteLength = length * BYTES_PER_ELEMENT;
            byteOffset = 0;
            _valueType = JSValueType.Object;
            _oValue = this;
        }

        [DoNotEnumerate]
        protected TypedArray(ArrayBuffer buffer, int byteOffset, int length)
        {
            if (byteOffset % BYTES_PER_ELEMENT != 0)
                ExceptionHelper.Throw(new RangeError("Offset is not alligned"));
            if (buffer.byteLength % BYTES_PER_ELEMENT != 0)
                ExceptionHelper.Throw(new RangeError("buffer.byteLength is not alligned"));
            if (buffer.byteLength < byteOffset)
                ExceptionHelper.Throw(new RangeError("Invalid offset"));
            this.byteLength = System.Math.Min(buffer.byteLength - byteOffset, length * BYTES_PER_ELEMENT);
            this.buffer = buffer;
            this.length = new Number(byteLength / BYTES_PER_ELEMENT);
            this.byteOffset = byteOffset;
            this._valueType = JSValueType.Object;
            this._oValue = this;
        }

        [DoNotEnumerate]
        protected TypedArray(JSValue iterablyObject)
        {
            var src = Tools.arraylikeToArray(iterablyObject, true, false, false, -1);
            if (src._data.Length > int.MaxValue)
                throw new System.OutOfMemoryException();
            var length = (int)src._data.Length;
            this.buffer = new ArrayBuffer(length * BYTES_PER_ELEMENT);
            this.length = new Number(length);
            this.byteLength = length * BYTES_PER_ELEMENT;
            this._valueType = JSValueType.Object;
            this._oValue = this;
            foreach (var item in src._data.ReversOrder)
                this[item.Key] = item.Value;
        }

        [AllowNullArguments]
        [ArgumentsCount(2)]
        public void set(Arguments args)
        {
            if (args == null)
                return;

            var offset = Tools.JSObjectToInt64(args[1], 0, false);
            var src = args[0] ?? undefined;
            if (src._valueType < JSValueType.String)
                return;

            var length = Tools.JSObjectToInt64(src["length"], 0, false);
            if (this.length._iValue - offset < length)
                ExceptionHelper.Throw(new RangeError("Invalid source length or offset argument"));
            JSValue index = 0;
            var dummyArgs = new Arguments();
            for (var i = 0L; i < length; i++)
            {
                if (i > int.MaxValue)
                {
                    index._valueType = JSValueType.Double;
                    index._dValue = i;
                }
                else
                    index._iValue = (int)i;
                var value = src.GetProperty(index, false, PropertyScope.Common);
                if (value._valueType == JSValueType.Property)
                {
                    value = ((value._oValue as PropertyPair).getter ?? Function.Empty).Call(src, dummyArgs);
                    dummyArgs.Reset();
                }
                this[(int)(i + offset)] = value;
            }
        }

        [ArgumentsCount(2)]
        public abstract TypedArray subarray(Arguments args);

        protected T subarrayImpl<T>(JSValue begin, JSValue end) where T : TypedArray, new()
        {
            var bi = Tools.JSObjectToInt32(begin, 0, false);
            var ei = end.Exists ? Tools.JSObjectToInt32(end, 0, false) : Tools.JSObjectToInt32(length);
            if (bi == 0 && ei >= length._iValue)
                return (T)this;
            var r = new T();
            r.buffer = buffer;
            r.byteLength = System.Math.Max(0, System.Math.Min(ei, length._iValue) - bi) * BYTES_PER_ELEMENT;
            r.byteOffset = byteOffset + bi * BYTES_PER_ELEMENT;
            r.length = new Number(r.byteLength / BYTES_PER_ELEMENT);
            return r;
        }

        protected internal sealed override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                if (key._valueType == JSValueType.String && "length".Equals(key._oValue))
                    return length;
                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname._valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();
                switch (tname._valueType)
                {
                    case JSValueType.Object:
                    case JSValueType.Boolean:
                        break;
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
                    if (index < 0)
                        ExceptionHelper.Throw(new RangeError("Invalid array index"));
                    if (index >= length._iValue)
                        return undefined;
                    return this[index];
                }
            }
            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override void SetProperty(JSValue name, JSValue value, PropertyScope memberScope, bool strict)
        {
            if (name._valueType == JSValueType.String && "length".Equals(name._oValue))
                return;
            bool isIndex = false;
            int index = 0;
            JSValue tname = name;
            if (tname._valueType >= JSValueType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname._valueType)
            {
                case JSValueType.Object:
                case JSValueType.Boolean:
                    break;
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
                if (index < 0)
                    ExceptionHelper.Throw(new RangeError("Invalid array index"));
                if (index >= length._iValue)
                    return;
                this[index] = value;
                return;
            }
            base.SetProperty(name, value, strict);
        }

        protected internal abstract System.Array ToNativeArray();

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            var baseEnum = base.GetEnumerator(hideNonEnum, enumeratorMode);
            while(baseEnum.MoveNext())
                yield return baseEnum.Current;

            for (int i = 0, len = Tools.JSObjectToInt32(length); i < len; i++)
                yield return new KeyValuePair<string, JSValue>(i.ToString(), this[i]);
        }
    }
}
