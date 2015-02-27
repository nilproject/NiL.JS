using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Uint16Array : TypedArray
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
                var v = (ushort)Tools.JSObjectToInt32(value, 0, false);
                buffer.Data[index * BYTES_PER_ELEMENT + byteOffset] = (byte)v;
                buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 1] = (byte)(v >> 8);
            }
        }

        private ushort getValue(int index)
        {
            return (ushort)(buffer.Data[index * BYTES_PER_ELEMENT + byteOffset] | (buffer.Data[index * BYTES_PER_ELEMENT + byteOffset + 1] << 8));
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return 2; }
        }

        public Uint16Array()
            : base()
        {
        }

        public Uint16Array(int length)
            : base(length)
        {
        }

        public Uint16Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Uint16Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Uint16Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Uint16Array(JSObject src)
            : base(src) { }

        [ParametersCount(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Uint16Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(ushort); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new ushort[length.iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
