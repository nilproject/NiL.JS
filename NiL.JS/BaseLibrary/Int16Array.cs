using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Int16Array : TypedArray
    {
        protected override JSValue this[int index]
        {
            get
            {
                var res = new Element(this, index);
                res._iValue = getValue(index);
                res._valueType = JSValueType.Integer;
                return res;
            }
            set
            {
                if (index < 0 || index > length._iValue)
                    ExceptionHelper.Throw(new RangeError());
                var v = (ushort)Tools.JSObjectToInt32(value, 0, false);
                buffer.data[index * BYTES_PER_ELEMENT + byteOffset] = (byte)v;
                buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 1] = (byte)(v >> 8);
            }
        }

        private short getValue(int index)
        {
            return (short)(buffer.data[index * BYTES_PER_ELEMENT + byteOffset] | (buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 1] << 8));
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return sizeof(short); }
        }

        public Int16Array()
            : base()
        {
        }

        public Int16Array(int length)
            : base(length)
        {
        }

        public Int16Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Int16Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Int16Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Int16Array(JSValue src)
            : base(src) { }

        [ArgumentsCount(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Int16Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(short); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new short[length._iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
