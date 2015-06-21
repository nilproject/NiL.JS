using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.TypeProxing
{
    public sealed class NativeList : CustomType
    {
        private sealed class Element : JSObject
        {
            private readonly NativeList owner;
            private int index;

            public Element(NativeList owner, int index)
            {
                this.owner = owner;
                this.index = index;
                attributes |= JSObjectAttributesInternal.Reassign;
                var value = owner.data[index];
                valueType = JSObjectType.Undefined;
                if (value is JSObject)
                    base.Assign(value as JSObject);
                else if (value is sbyte)
                {
                    iValue = (int)(sbyte)value;
                    valueType = JSObjectType.Int;
                }
                else if (value is byte)
                {
                    iValue = (int)(byte)value;
                    valueType = JSObjectType.Int;
                }
                else if (value is short)
                {
                    iValue = (int)(short)value;
                    valueType = JSObjectType.Int;
                }
                else if (value is ushort)
                {
                    iValue = (int)(ushort)value;
                    valueType = JSObjectType.Int;
                }
                else if (value is int)
                {
                    iValue = (int)value;
                    valueType = JSObjectType.Int;
                }
                else if (value is uint)
                {
                    dValue = (long)(uint)value;
                    valueType = JSObjectType.Double;
                }
                else if (value is long)
                {
                    dValue = (long)value;
                    valueType = JSObjectType.Double;
                }
                else if (value is ulong)
                {
                    dValue = (double)(ulong)value;
                    valueType = JSObjectType.Double;
                }
                else if (value is float)
                {
                    dValue = (double)(float)value;
                    valueType = JSObjectType.Double;
                }
                else if (value is double)
                {
                    dValue = (double)value;
                    valueType = JSObjectType.Double;
                }
                else if (value is string)
                {
                    oValue = value.ToString();
                    valueType = JSObjectType.String;
                }
                else if (value is char)
                {
                    oValue = value.ToString();
                    valueType = JSObjectType.String;
                }
                else if (value is bool)
                {
                    iValue = (bool)value ? 1 : 0;
                    valueType = JSObjectType.Bool;
                }
                else if (value is Delegate)
                {
#if PORTABLE
                    oValue = new MethodProxy(((Delegate)value).GetMethodInfo(), ((Delegate)value).Target);
#else
                    oValue = new MethodProxy(((Delegate)value).Method, ((Delegate)value).Target);
#endif
                    valueType = JSObjectType.Function;
                }
                else if (value is IList)
                {
                    oValue = new NativeList(value as IList);
                    valueType = JSObjectType.Object;
                }
                else
                {
                    var type = value.GetType();
                    __proto__ = TypeProxy.GetPrototype(type);
                    attributes |= __proto__.attributes & JSObjectAttributesInternal.Immutable;
                }
            }

            public override void Assign(JSObject value)
            {
                owner.data[index] = value.Value;
            }
        }

        private readonly Number lenObj;
        private readonly IList data;

        [Hidden]
        public NativeList()
        {
            this.data = new List<object>();
            lenObj = new Number(0);
        }

        [Hidden]
        public NativeList(IList data)
        {
            this.data = data;
            lenObj = new Number(data.Count);
        }

        public void push(Arguments args)
        {
            for (var i = 0; i < args.length; i++)
                data.Add(args[i].Value);
        }

        public JSObject pop()
        {
            if (data.Count == 0)
            {
                notExists.valueType = JSObjectType.NotExistsInObject;
                return notExists;
            }
            var result = data[data.Count - 1];
            data.RemoveAt(data.Count - 1);
            if (result is IList)
                return new NativeList(result as IList);
            else
                return TypeProxy.Proxy(result);
        }

        protected internal override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            forWrite &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
            if (name.valueType == JSObjectType.String && string.CompareOrdinal("length", name.oValue.ToString()) == 0)
            {
                lenObj.iValue = data.Count;
                return lenObj;
            }
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
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
                notExists.valueType = JSObjectType.NotExistsInObject;
                if (index < 0 || index > data.Count)
                    return notExists;
                return new Element(this, index);
            }
            return DefaultFieldGetter(name, forWrite, own);
        }

        protected internal override void SetMember(JSObject name, JSObject value, bool strict)
        {
            if (name.valueType == JSObjectType.String && string.CompareOrdinal("length", name.oValue.ToString()) == 0)
                return;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
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
                notExists.valueType = JSObjectType.NotExistsInObject;
                if (index < 0 || index > data.Count)
                    return;
                data[index] = value.Value;
                return;
            }
            base.SetMember(name, value, strict);
        }
    }
}
