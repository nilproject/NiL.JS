using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.BaseLibrary;
using NiL.JS.Extensions;
using NiL.JS.Backward;
using System.Reflection;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Interop
{
#if !NET40
    internal static class NativeReadOnlyListCtors
    {
        public static readonly string ReadOnlyInterfaceName = typeof(IReadOnlyList<>).FullName;

        private static readonly Type ReadOnlyListType = typeof(IReadOnlyList<>);
        private static readonly Dictionary<Type, Func<object, JSValue>> _ctors = new Dictionary<Type, Func<object, JSValue>>();

        public static JSValue Create(object roList)
        {
            lock (_ctors)
            {
                var type = roList.GetType();
                if (!_ctors.TryGetValue(type, out var ctor))
                {
                    var itemType = type.GetInterface(ReadOnlyInterfaceName).GetGenericArguments()[0];
                    var listType = typeof(NativeReadOnlyList<>).MakeGenericType(typeof(int));
                    var prm = Expression.Parameter(typeof(object));
                    _ctors[type] = ctor = Expression.Lambda<Func<object, JSValue>>(
                        Expression.New(
                            listType.GetConstructors()[0],
                            Expression.Convert(prm, type)),
                        prm)
                        .Compile();
                }

                return ctor(roList);
            }
        }

        internal static bool IsReadOnlyList(object value)
        {
            var type = value.GetType();
            var interfaces = type.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsConstructedGenericType
                    && ReferenceEquals(interfaces[i].GetGenericTypeDefinition(), ReadOnlyListType))
                    return true;
            }

            return false;
        }
    }

    [Prototype(typeof(BaseLibrary.Array))]
    public sealed class NativeReadOnlyList<T> : CustomType, IIterable
    {
        private readonly Number _lenObj;
        private readonly IReadOnlyList<T> _list;

        public NativeReadOnlyList(IReadOnlyList<T> list)
        {
            _attributes |= JSValueAttributesInternal.Immutable;
            _list = list ?? throw new System.ArgumentNullException(nameof(list));
            _lenObj = 0;
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;
                if (key._valueType == JSValueType.String && string.CompareOrdinal("length", key._oValue.ToString()) == 0)
                {
                    _lenObj._iValue = _list.Count;
                    return _lenObj;
                }

                bool isIndex = false;
                int index = 0;
                JSValue tname = key;
                if (tname._valueType >= JSValueType.Object)
                    tname = tname.ToPrimitiveValue_String_Value();

                switch (tname._valueType)
                {
                    case JSValueType.Integer:
                    {
                        isIndex = tname._iValue >= 0;
                        index = tname._iValue;
                        break;
                    }
                    case JSValueType.Double:
                    {
                        isIndex = tname._dValue >= 0
                            && tname._dValue < uint.MaxValue
                            && (long)tname._dValue == tname._dValue;

                        if (isIndex)
                            index = (int)(uint)tname._dValue;

                        break;
                    }
                    case JSValueType.String:
                    {
                        var str = tname._oValue.ToString();
                        if (str.Length > 0)
                        {
                            var fc = str[0];
                            if ('0' <= fc && '9' >= fc)
                            {
                                var si = 0;
                                if (Tools.ParseJsNumber(tname._oValue.ToString(), ref si, out double dindex)
                                    && (si == tname._oValue.ToString().Length)
                                    && dindex >= 0
                                    && dindex < uint.MaxValue
                                    && (long)dindex == dindex)
                                {
                                    isIndex = true;
                                    index = (int)(uint)dindex;
                                }
                            }
                        }
                        break;
                    }
                }

                if (isIndex && index >= 0 && index < _list.Count)
                {
                    var context = Context.CurrentGlobalContext;
                    return context.ProxyValue(_list[index]);
                }
            }

            return base.GetProperty(key, forWrite, memberScope);
        }

        public IIterator iterator()
        {
            return _list.GetEnumerator().AsIterator();
        }
    }
#endif
}
