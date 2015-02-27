using System;
using System.Collections.Generic;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.BaseTypes
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ArrayBuffer : CustomType
    {
#if !PORTABLE
        [Serializable]
#endif
        private sealed class Element : JSObject
        {
            private int index;
            private byte[] data;

            public Element(int index, ArrayBuffer parent)
            {
                this.valueType = JSObjectType.Int;
                this.index = index;
                this.iValue = parent.Data[index];
                this.data = parent.Data;
                this.attributes |= JSObjectAttributesInternal.Reassign;
            }

            public override void Assign(JSObject value)
            {
                data[index] = (byte)Tools.JSObjectToInt32(value);
            }
        }

        [Hidden]
        public byte[] Data
        {
            [Hidden]
            get;
            [Hidden]
            private set;
        }

        [DoNotEnumerate]
        public ArrayBuffer()
            : this(0)
        {
        }

        [DoNotEnumerate]
        public ArrayBuffer(int length)
            : this(new byte[length])
        {
        }

        [Hidden]
        public ArrayBuffer(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException();
            Data = data;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        public int byteLength
        {
            [Hidden]
            get
            {
                return Data.Length;
            }
        }

        [Hidden]
        public ArrayBuffer slice(int begin, int end)
        {
            if (end < begin || begin >= Data.Length || end >= Data.Length)
                throw new JSException((new RangeError("Invalid begin or end index")));
            var res = new ArrayBuffer(end - begin + 1);
            for (int i = 0, j = begin; j <= end; j++, i++)
                res.Data[i] = Data[j];
            return res;
        }

        [Hidden]
        public ArrayBuffer slice(int begin)
        {
            return slice(begin, Data.Length - 1);
        }

        public ArrayBuffer slice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var l = Tools.JSObjectToInt32(args.GetMember("length"));
            if (l == 0)
                return this;
            if (l == 1)
                return slice(Tools.JSObjectToInt32(args[0]), Data.Length - 1);
            else
                return slice(Tools.JSObjectToInt32(args[0]), Tools.JSObjectToInt32(args[1]));
        }

        [Hidden]
        public byte this[int index]
        {
            [Hidden]
            get
            {
                return Data[index];
            }
            [Hidden]
            set
            {
                Data[index] = value;
            }
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            int index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsInfinity(dindex) && !double.IsNaN(dindex) && ((index = (int)dindex) == dindex))
            {
                if (dindex > 0x7fffffff || dindex < 0)
                    throw new JSException((new RangeError("Invalid array index")));
                if (((index = (int)dindex) == dindex))
                {
                    if (index >= Data.Length)
                        return undefined;
                    return new Element(index, this);
                }
            }
            return base.GetMember(name, forWrite, own);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            var be = base.GetEnumerator();
            while (be.MoveNext())
                yield return be.Current;
            for (var i = 0; i < Data.Length; i++)
                yield return i < 16 ? Tools.NumString[i] : i.ToString();
        }
    }
}
