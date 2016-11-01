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
                res._dValue = BitConverter.ToSingle(buffer.data, index * BYTES_PER_ELEMENT + byteOffset);
                res._valueType = JSValueType.Double;
                return res;
            }
            set
            {
                if (index < 0 || index > length._iValue)
                    ExceptionHelper.Throw(new RangeError());
                var v = BitConverter.GetBytes((float)Tools.JSObjectToDouble(value));
                if (BitConverter.IsLittleEndian)
                {
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 0] = v[3];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 1] = v[2];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 2] = v[1];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 3] = v[0];
                }
                else
                {
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 0] = v[0];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 1] = v[1];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 2] = v[2];
                    buffer.data[index * BYTES_PER_ELEMENT + byteOffset + 3] = v[3];
                }
            }
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
            throw new NotImplementedException();
        }
    }
}
