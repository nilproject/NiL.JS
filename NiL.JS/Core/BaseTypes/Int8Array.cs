using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Int8Array : TypedArray
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

        private sbyte getValue(int index)
        {
            return (sbyte)buffer.Data[index + byteOffset];
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return 1; }
        }

        public Int8Array()
            : base()
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

        public Int8Array(JSObject src)
            : base(src) { }

        [ParametersCount(2)]
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
            var res = new sbyte[length.iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
