using System;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.BaseTypes
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class TypedArray : JSObject
    {
        protected sealed class Element : JSObject
        {
            private readonly TypedArray owner;
            private int index;

            public Element(TypedArray owner, int index)
            {
                this.owner = owner;
                this.index = index;
                attributes |= JSObjectAttributesInternal.Reassign;
            }

            public override void Assign(JSObject value)
            {
                owner[index] = value;
            }
        }

        protected abstract JSObject this[int index]
        {
            get;
            set;
        }

        [Field]
        public ArrayBuffer buffer
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public int byteLength
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public int byteOffset
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public Number length
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public abstract int BYTES_PER_ELEMENT
        {
            [Hidden]
            [CallOverloaded]
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
            valueType = JSObjectType.Object;
            oValue = this;
        }

        [DoNotEnumerate]
        protected TypedArray(int length)
        {
            this.length = length;
            this.buffer = new ArrayBuffer(length * BYTES_PER_ELEMENT);
            this.byteLength = length * BYTES_PER_ELEMENT;
            this.byteOffset = 0;
            this.valueType = JSObjectType.Object;
            this.oValue = this;
        }

        [DoNotEnumerate]
        protected TypedArray(ArrayBuffer buffer, int byteOffset, int length)
        {
            if (byteOffset % BYTES_PER_ELEMENT != 0)
                throw new JSException(new RangeError("Offset is not alligned"));
            if (buffer.byteLength % BYTES_PER_ELEMENT != 0)
                throw new JSException(new RangeError("buffer.byteLength is not alligned"));
            if (buffer.byteLength < byteOffset)
                throw new JSException(new RangeError("Invalid offset"));
            this.byteLength = System.Math.Min(buffer.byteLength - byteOffset, length * BYTES_PER_ELEMENT);
            this.buffer = buffer;
            this.length = byteLength / BYTES_PER_ELEMENT;
            this.byteOffset = byteOffset;
            this.valueType = JSObjectType.Object;
            this.oValue = this;
        }

        [DoNotEnumerate]
        protected TypedArray(JSObject iterablyObject)
        {
            var src = Tools.iterableToArray(iterablyObject, true, false, false, -1);
            if (src.data.Length > int.MaxValue)
                throw new System.OutOfMemoryException();
            var length = (int)src.data.Length;
            this.buffer = new ArrayBuffer(length * BYTES_PER_ELEMENT);
            this.length = length;
            this.byteLength = length * BYTES_PER_ELEMENT;
            this.valueType = JSObjectType.Object;
            this.oValue = this;
            foreach (var item in src.data.Reversed)
                this[item.Key] = item.Value;
        }

        [AllowNullArguments]
        [ParametersCount(2)]
        public void set(Arguments args)
        {
            if (args == null)
                return;
            var offset = Tools.JSObjectToInt64(args.a1 ?? undefined, 0, false);
            var src = args.a0 ?? undefined;
            if (src.valueType < JSObjectType.String)
                return;
            var length = Tools.JSObjectToInt64(src["length"], 0, false);
            if (this.length.iValue - offset < length)
                throw new JSException(new RangeError("Invalid source length or offset argument"));
            JSObject index = 0;
            var dummyArgs = new Arguments();
            for (var i = 0L; i < length; i++)
            {
                if (i > int.MaxValue)
                {
                    index.valueType = JSObjectType.Double;
                    index.dValue = i;
                }
                else
                    index.iValue = (int)i;
                var value = src.GetMember(index, false, false);
                if (value.valueType == JSObjectType.Property)
                {
                    value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(src, dummyArgs);
                    dummyArgs.Reset();
                }
                this[(int)(i + offset)] = value;
            }
        }

        [ParametersCount(2)]
        public abstract TypedArray subarray(Arguments args);

        protected T subarrayImpl<T>(JSObject begin, JSObject end) where T : TypedArray, new()
        {
            var bi = Tools.JSObjectToInt32(begin, 0, false);
            var ei = end.IsExist ? Tools.JSObjectToInt32(end, 0, false) : Tools.JSObjectToInt32(length);
            if (bi == 0 && ei >= length.iValue)
                return (T)this;
            var r = new T();
            r.buffer = buffer;
            r.byteLength = System.Math.Max(0, System.Math.Min(ei, length.iValue) - bi) * BYTES_PER_ELEMENT;
            r.byteOffset = byteOffset + bi * BYTES_PER_ELEMENT;
            r.length = r.byteLength / BYTES_PER_ELEMENT;
            return r;
        }

        protected internal sealed override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (name.valueType == JSObjectType.String && "length".Equals(name.oValue))
                return length;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Bool:
                    break;
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
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
                if (index > 0x7fffffff || index < 0)
                    throw new JSException(new RangeError("Invalid array index"));
                if (index >= length.iValue)
                    return undefined;
                return this[index];
            }
            return base.GetMember(name, forWrite, own);
        }

        protected internal override void SetMember(JSObject name, JSObject value, bool strict)
        {
            if (name.valueType == JSObjectType.String && "length".Equals(name.oValue))
                return;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Bool:
                    break;
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
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
                if (index > 0x7fffffff || index < 0)
                    throw new JSException(new RangeError("Invalid array index"));
                if (index >= length.iValue)
                    return;
                this[index] = value;
                return;
            }
            base.SetMember(name, value, strict);
        }

        protected internal abstract System.Array ToNativeArray();

    }
}
