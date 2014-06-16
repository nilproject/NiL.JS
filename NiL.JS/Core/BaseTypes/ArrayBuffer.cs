using System;
using System.Collections.Generic;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class ArrayBuffer : EmbeddedType
    {
        [Serializable]
        private sealed class Element : JSObject
        {
            private int index;
            private byte[] data;

            public Element(int index, byte[] data)
            {
                this.valueType = JSObjectType.Int;
                this.index = index;
                this.iValue = data[index];
                this.data = data;
            }

            public override void Assign(JSObject value)
            {
                data[index] = (byte)Tools.JSObjectToInt(value);
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
        {
            Data = new byte[0];
        }

        [DoNotEnumerate]
        public ArrayBuffer(int length)
        {
            Data = new byte[length];
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
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid begin or end index")));
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

        public ArrayBuffer slice(JSObject args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var l = Tools.JSObjectToInt(args.GetMember("length"));
            if (l == 0)
                return this;
            if (l == 1)
                return slice(Tools.JSObjectToInt(args.GetMember("0")), Data.Length - 1);
            else
                return slice(Tools.JSObjectToInt(args.GetMember("0")), Tools.JSObjectToInt(args.GetMember("1")));
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
        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            int index = 0;
            double dindex = 0.0;
            if (name != "NaN" && name != "Infinity" && name != "-Infinity" &&
                Tools.ParseNumber(name, index, out dindex))
            {
                if (dindex > 0x7fffffff || dindex < 0)
                    throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array index")));
                if (((index = (int)dindex) == dindex))
                {
                    if (index >= Data.Length)
                        return undefined;
                    return new Element(index, Data);
                }
            }
            return base.GetMember(name, create, own);
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
