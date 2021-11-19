using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Float32Array : TypedArray
    {
        protected override JSValue this[int index]
        {
            get
            {
                var res = new Element(this, index);
                res._dValue = getValue(index);
                res._valueType = JSValueType.Double;
                return res;
            }
            set
            {
                if (index < 0 || index > length._iValue)
                    ExceptionHelper.Throw(new RangeError());

                setValue(index, (float)Tools.JSObjectToDouble(value));
            }
        }

        private void setValue(int index, float value)
        {
            var v = BitConverter.GetBytes(value);
            int byteIndex = index * BYTES_PER_ELEMENT + byteOffset;
            buffer.data[byteIndex + 0] = v[0];
            buffer.data[byteIndex + 1] = v[1];
            buffer.data[byteIndex + 2] = v[2];
            buffer.data[byteIndex + 3] = v[3];
        }

        private float getValue(int index)
        {
            return BitConverter.ToSingle(buffer.data, index * BYTES_PER_ELEMENT + byteOffset);
        }

        public override int BYTES_PER_ELEMENT
        {
            get { return sizeof(float); }
        }

        public Float32Array()
            : base()
        {
        }

        public Float32Array(int length)
            : base(length)
        {
        }

        public Float32Array(ArrayBuffer buffer)
            : base(buffer, 0, buffer.byteLength)
        {
        }

        public Float32Array(ArrayBuffer buffer, int bytesOffset)
            : base(buffer, bytesOffset, buffer.byteLength - bytesOffset)
        {
        }

        public Float32Array(ArrayBuffer buffer, int bytesOffset, int length)
            : base(buffer, bytesOffset, length)
        {
        }

        public Float32Array(JSValue src)
            : base(src) { }

        [ArgumentsCount(2)]
        public override TypedArray subarray(Arguments args)
        {
            return subarrayImpl<Float32Array>(args[0], args[1]);
        }

        [Hidden]
        public override Type ElementType
        {
            [Hidden]
            get { return typeof(float); }
        }

        protected internal override System.Array ToNativeArray()
        {
            var res = new float[length._iValue];
            for (var i = 0; i < res.Length; i++)
                res[i] = getValue(i);
            return res;
        }
    }
}
