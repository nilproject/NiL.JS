using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ArrayBuffer : CustomType
    {
#if !PORTABLE
        [Serializable]
#endif
        private sealed class Element : JSValue
        {
            private int index;
            private byte[] data;

            public Element(int index, ArrayBuffer parent)
            {
                this.valueType = JSValueType.Int;
                this.index = index;
                this.iValue = parent.data[index];
                this.data = parent.data;
                this.attributes |= JSValueAttributesInternal.Reassign;
            }

            public override void Assign(JSValue value)
            {
                data[index] = (byte)Tools.JSObjectToInt32(value);
            }
        }

        internal byte[] data;

        [Hidden]
        public byte this[int index]
        {
            [Hidden]
            get
            {
                return data[index];
            }
            [Hidden]
            set
            {
                data[index] = value;
            }
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
            this.data = data;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        public int byteLength
        {
            [Hidden]
            get
            {
                return data.Length;
            }
        }

        [Hidden]
        public ArrayBuffer slice(int begin, int end)
        {
            if (end < begin || begin >= data.Length || end >= data.Length)
                ExceptionsHelper.Throw((new RangeError("Invalid begin or end index")));
            var res = new ArrayBuffer(end - begin + 1);
            for (int i = 0, j = begin; j <= end; j++, i++)
                res.data[i] = data[j];
            return res;
        }

        [Hidden]
        public ArrayBuffer slice(int begin)
        {
            return slice(begin, data.Length - 1);
        }

        public ArrayBuffer slice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var l = Tools.JSObjectToInt32(args.GetMember("length"));
            if (l == 0)
                return this;
            if (l == 1)
                return slice(Tools.JSObjectToInt32(args[0]), data.Length - 1);
            else
                return slice(Tools.JSObjectToInt32(args[0]), Tools.JSObjectToInt32(args[1]));
        }

        [Hidden]
        internal protected override JSValue GetMember(JSValue key, bool forWrite, MemberScope memberScope)
        {
            if (key.valueType != JSValueType.Symbol)
            {
                uint index = 0;
                double dindex = Tools.JSObjectToDouble(key);
                if (!double.IsInfinity(dindex)
                    && !double.IsNaN(dindex)
                    && ((index = (uint)dindex) == dindex))
                {
                    return getElement((int)index);
                }
            }
            return base.GetMember(key, forWrite, memberScope);
        }

        private JSValue getElement(int index)
        {
            if (index < 0)
                ExceptionsHelper.Throw(new RangeError("Invalid array index"));
            if (index >= data.Length)
                return undefined;
            return new Element(index, this);
        }

        public override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            var be = GetEnumerator();
            while (be.MoveNext())
                yield return be.Current;
            for (var i = 0; i < data.Length; i++)
                yield return new KeyValuePair<string, JSValue>(Tools.Int32ToString(i), (int)enumerationMode > 0 ? getElement(i) : null);
        }

        [Hidden]
        public byte[] GetData()
        {
            return data;
        }
    }
}
