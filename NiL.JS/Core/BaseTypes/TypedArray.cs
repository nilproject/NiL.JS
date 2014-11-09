using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public abstract class TypedArray : JSObject
    {
        protected sealed class Element : JSObject
        {
            private readonly TypedArray owner;
            private int index;

            public Element(TypedArray owner, int index)
            {
                this.owner = owner;
                this.index = index;
            }

            public override void Assign(JSObject value)
            {
                owner[index] = value;
            }
        }

        protected abstract JSObject this[int index]
        {
            get;
            set;
        }

        [Field]
        public ArrayBuffer buffer
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public int byteLength
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public int byteOffset
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public Number length
        {
            [Hidden]
            [CallOverloaded]
            get;
            private set;
        }
        [Field]
        public abstract int BYTES_PER_ELEMENT
        {
            [Hidden]
            [CallOverloaded]
            get;
        }

        protected TypedArray()
        {
            buffer = new ArrayBuffer();
            valueType = JSObjectType.Object;
            oValue = this;
        }

        protected TypedArray(int length)
        {
            this.length = length;
            buffer = new ArrayBuffer(length * BYTES_PER_ELEMENT);
            valueType = JSObjectType.Object;
            oValue = this;
        }

        protected TypedArray(ArrayBuffer buffer, int bytesOffset, int length)
        {
            this.length = System.Math.Min(((buffer.byteLength - byteOffset) / BYTES_PER_ELEMENT) * BYTES_PER_ELEMENT, length);
            this.buffer = buffer;
            valueType = JSObjectType.Object;
            oValue = this;
        }

        public void set(Arguments args)
        {
            var offset = Tools.JSObjectToInt64(args.a1 ?? undefined, 0, false);
            var src = args.a0 ?? undefined;
            if (src.valueType < JSObjectType.String)
                return;
            var length = Tools.JSObjectToInt64(src["length"], 0, false);
            if (this.length.iValue - offset < length)
                throw new JSException(new RangeError("Invalid source length or offset argument"));
            JSObject index = 0;
            var dummyArgs = new Arguments();
            for (var i = 0L; i < length; i++)
            {
                if (i > int.MaxValue)
                {
                    index.valueType = JSObjectType.Double;
                    index.dValue = i;
                }
                else
                    index.iValue = (int)i;
                var value = src.GetMember(index, false, false);
                if (value.valueType == JSObjectType.Property)
                {
                    value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(src, dummyArgs);
                    if (dummyArgs.fields != null)
                        dummyArgs.fields.Clear();
                    dummyArgs.a0 =
                        dummyArgs.a1 =
                        dummyArgs.a2 =
                        dummyArgs.a3 =
                        dummyArgs.a4 =
                        dummyArgs.a5 =
                        dummyArgs.a6 =
                        dummyArgs.a7 = null;
                    dummyArgs.length = 0;
                }
                this[(int)(i + offset)] = value;
            }
        }

        public abstract TypedArray subarray();
        public abstract TypedArray subarray(JSObject begin);
        public abstract TypedArray subarray(JSObject begin, JSObject end);

        protected internal sealed override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (name.valueType == JSObjectType.String && "length".Equals(name.oValue))
                return length;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Bool:
                    break;
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var fc = tname.oValue.ToString()[0];
                        if ('0' <= fc && '9' >= fc)
                        {
                            var dindex = 0.0;
                            int si = 0;
                            if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                && (si == tname.oValue.ToString().Length)
                                && dindex >= 0
                                && dindex < uint.MaxValue
                                && (long)dindex == dindex)
                            {
                                isIndex = true;
                                index = (int)(uint)dindex;
                            }
                        }
                        break;
                    }
            }
            if (isIndex)
            {
                if (index > 0x7fffffff || index < 0)
                    throw new JSException(new RangeError("Invalid array index"));
                if (index >= length.iValue)
                    return undefined;
                return this[index];
            }
            return base.GetMember(name, forWrite, own);
        }
    }
}
