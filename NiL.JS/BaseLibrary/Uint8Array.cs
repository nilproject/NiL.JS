using System;
using NiL.JS.Core;
using NiL.JS.Core.Modules;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Uint8Array : TypedArray
    {
        protected override JSObject this[int index]
        {
            get
            {
                var res = new Element(this, index);
                res.iValue = getValue(index);
                res.valueType = JSObjectType.Int;
                return res;
            }
            set
            {
                if (index < 0 || index > length.iValue)
                    throw new JSException(new RangeError());
                buffer.Data[index + byteOffset] = (byte)Tools.JSObjectToInt32(value, 0, false);
            }
        }

        private byte getValue(int index)
        {
            return buffer.Data[index + byteOffset];
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return 1; }
        }

        public Uint8Array()
            : base()
        {
        }

        public Uint8Array(int length)
            : base(length)
        {
        }

        public Uint8Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Uint8Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Uint8Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Uint8Array(JSObject src)
            : base(src) { }

        [ArgumentsLength(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Uint8Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(byte); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new byte[length.iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
