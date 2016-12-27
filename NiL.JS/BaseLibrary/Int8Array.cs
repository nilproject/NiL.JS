using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Int8Array : TypedArray
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
                buffer.data[index + byteOffset] = (byte)Tools.JSObjectToInt32(value, 0, false);
            }
        }

        private sbyte getValue(int index)
        {
            return (sbyte)buffer.data[index + byteOffset];
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return sizeof(sbyte); }
        }

        public Int8Array()
        {
        }

        public Int8Array(int length)
            : base(length)
        {
        }

        public Int8Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Int8Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Int8Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Int8Array(JSValue src)
            : base(src) { }

        [ArgumentsCount(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Int8Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(sbyte); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new sbyte[length._iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
