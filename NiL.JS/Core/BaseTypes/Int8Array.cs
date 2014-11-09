using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                res.iValue = (sbyte)buffer[index + byteOffset];
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

        public override TypedArray subarray()
        {
            return this;
        }

        public override TypedArray subarray(JSObject begin)
        {
            var bi = Tools.JSObjectToInt32(begin, 0, false);
            if (bi == 0)
                return this;
            return new Int8Array(buffer, byteOffset + bi, length.iValue - bi);
        }

        public override TypedArray subarray(JSObject begin, JSObject end)
        {
            var bi = Tools.JSObjectToInt32(begin, 0, false);
            var ei = Tools.JSObjectToInt32(end, 0, false);
            if (bi == 0 && ei >= length.iValue)
                return this;
            return new Int8Array(buffer, byteOffset + bi, Math.Max(0, Math.Min(ei, length.iValue) - bi));
        }
    }
}
