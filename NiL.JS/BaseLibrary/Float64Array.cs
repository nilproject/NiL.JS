using System;
using NiL.JS.Core;
using NiL.JS.Core.Modules;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Float64Array : TypedArray
    {
        protected override JSObject this[int index]
        {
            get
            {
                var res = new Element(this, index);
                res.dValue = getValue(index);
                res.valueType = JSObjectType.Double;
                return res;
            }
            set
            {
                if (index < 0 || index > length.iValue)
                    throw new JSException(new RangeError());
                var v = BitConverter.DoubleToInt64Bits(Tools.JSObjectToDouble(value));
                if (BitConverter.IsLittleEndian)
                {
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 0] = (byte)(v >> 0);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 1] = (byte)(v >> 8);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 2] = (byte)(v >> 16);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 3] = (byte)(v >> 24);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 4] = (byte)(v >> 32);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 5] = (byte)(v >> 40);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 6] = (byte)(v >> 48);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 7] = (byte)(v >> 56);
                }
                else
                {
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 0] = (byte)(v >> 56);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 1] = (byte)(v >> 48);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 2] = (byte)(v >> 40);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 3] = (byte)(v >> 32);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 4] = (byte)(v >> 24);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 5] = (byte)(v >> 16);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 6] = (byte)(v >> 8);
                    buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 7] = (byte)(v >> 0);
                }
            }
        }

        private double getValue(int index)
        {
            return BitConverter.ToDouble(buffer.Data, index * BYTES_PER_ELEMENT + byteOffset);
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return 8; }
        }

        public Float64Array()
            : base()
        {
        }

        public Float64Array(int length)
            : base(length)
        {
        }

        public Float64Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Float64Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Float64Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Float64Array(JSObject src)
            : base(src) { }

        [ArgumentsLength(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Float64Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(double); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new double[length.iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
