using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    public sealed class ArrayBuffer : EmbeddedType
    {
        private sealed class Element : JSObject
        {
            private int index;
            private byte[] data;

            public Element(int index, byte[] data)
            {
                this.ValueType = JSObjectType.Int;
                this.index = index;
                this.iValue = data[index];
                this.data = data;
            }

            public override void Assign(JSObject value)
            {
                data[index] = (byte)Tools.JSObjectToInt(value);
            }
        }

        [Modules.Hidden]
        public byte[] Data { get; private set; }

        public ArrayBuffer(int length)
        {
            Data = new byte[length];
        }

        public int byteLength
        {
            get
            {
                return Data.Length;
            }
        }

        public ArrayBuffer slice(int begin, int end)
        {
            if (end < begin || begin >= Data.Length || end >= Data.Length)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid begin or end index")));
            var res = new ArrayBuffer(end - begin);
            for (int i = 0, j = begin; j <= end; j++, i++)
                res.Data[i] = Data[j];
            return res;
        }

        public ArrayBuffer slice(int begin)
        {
            return slice(begin, Data.Length - 1);
        }

        [Modules.Hidden]
        public byte this[int index]
        {
            get
            {
                return Data[index];
            }
            set
            {
                Data[index] = value;
            }
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            int index = 0;
            double dindex = 0.0;
            if (name != "NaN" && name != "Infinity" && name != "-Infinity" &&
                Tools.ParseNumber(name, ref index, false, out dindex))
            {
                if (dindex > 0x7fffffff || dindex < 0)
                    throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array index")));
                if (((index = (int)dindex) == dindex))
                {
                    if (index < 0 || index >= Data.Length)
                        return undefined;
                    return new Element(index, Data);
                }
            }
            return base.GetField(name, fast, own);
        }
    }
}
