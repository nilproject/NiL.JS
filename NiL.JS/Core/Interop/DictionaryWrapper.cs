﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Interop
{
    public static class DictionaryWrapper
    {
        internal static readonly MethodInfo OfMethod = typeof(DictionaryWrapper)
                                .GetMethod(nameof(Of));

        internal static JSObject Of(Type keyType, Type valueType, object value)
        {
            return (JSObject)OfMethod
                .MakeGenericMethod(keyType, valueType)
                .Invoke(null, new[] { value })!;
        }

        public static DictionaryWrapper<TKey, TValue> Of<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            => new DictionaryWrapper<TKey, TValue>(dictionary);
    }

    public sealed class UntypedDictionaryWrapper : IDictionary<object, object>
    {
        private readonly IDictionary _target;

        public UntypedDictionaryWrapper(IDictionary target)
        {
            _target = target;
        }

        public object this[object key] { get => _target[key]; set => _target[key] = value; }

        public ICollection<object> Keys => _target.Keys.OfType<object>().ToList();

        public ICollection<object> Values => _target.Values.OfType<object>().ToList();

        public int Count => _target.Count;

        public bool IsReadOnly => _target.IsReadOnly;

        public void Add(object key, object value) => _target.Add(key, value);

        public void Add(KeyValuePair<object, object> item) => _target.Add(item.Key, item.Value);

        public void Clear() => _target.Clear();

        public bool Contains(KeyValuePair<object, object> item) => _target.Contains(item.Key) && _target[item.Key].Equals(item.Value);

        public bool ContainsKey(object key) => _target.Contains(key);

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) => _target.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            for (var e = _target.GetEnumerator(); e.MoveNext();)
                yield return new KeyValuePair<object, object>(e.Key, e.Value);
        }

        public bool Remove(object key)
        {
            var contains = _target.Contains(key);
            if (contains)
                _target.Remove(key);
            return contains;
        }

        public bool Remove(KeyValuePair<object, object> item) => Contains(item) && Remove(item.Key);

        public bool TryGetValue(object key, [MaybeNullWhen(false)] out object value)
        {
            var contains = _target.Contains(key);
            value = _target[key];
            return contains;
        }

        IEnumerator IEnumerable.GetEnumerator() => _target.GetEnumerator();
    }

    public sealed class DictionaryWrapper<TKey, TValue> : JSObject
    {
        private readonly IDictionary<TKey, TValue> _target;

        private sealed class ValueWrapper : JSValue
        {
            private readonly TKey _key;
            private readonly DictionaryWrapper<TKey, TValue> _owner;

            public ValueWrapper(DictionaryWrapper<TKey, TValue> owner, TKey key)
            {
                _owner = owner;
                _key = key;
                _attributes |= JSValueAttributesInternal.Reassign;

                if (owner._target.TryGetValue(key, out var value))
                    base.Assign(Context.CurrentGlobalContext.ProxyValue(value));
            }

            public override void Assign(JSValue value)
            {
                _owner._target[_key] = value.As<TValue>();

                base.Assign(value);
            }
        }

        public DictionaryWrapper(IDictionary<TKey, TValue> target)
        {
            _valueType = JSValueType.Object;
            _oValue = this;
            _target = target;
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope)
        {
            if (key.ValueType == JSValueType.Symbol || propertyScope >= PropertyScope.Super)
                return base.GetProperty(key, forWrite, propertyScope);

            var oKey = key.As<TKey>();

            if (!forWrite)
            {
                if (!_target.ContainsKey(oKey))
                    return undefined;

                return Context.CurrentGlobalContext.ProxyValue(_target[oKey]);
            }

            return new ValueWrapper(this, oKey);
        }

        protected internal override void SetProperty(JSValue key, JSValue value, PropertyScope propertyScope, bool throwOnError)
        {
            if (key.ValueType == JSValueType.Symbol || propertyScope >= PropertyScope.Super)
                base.SetProperty(key, value, propertyScope, throwOnError);

            _target[key.As<TKey>()] = value.As<TValue>();
        }

        protected internal override bool DeleteProperty(JSValue key)
        {
            if (key.ValueType == JSValueType.Symbol)
                return base.DeleteProperty(key);

            return _target.Remove(key.As<TKey>());
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            if (propertyScope is PropertyScope.Own or PropertyScope.Common)
            {
                if (enumeratorMode == EnumerationMode.KeysOnly)
                    return (_target as IDictionary<string, object>).Keys.Select(x => new KeyValuePair<string, JSValue>(x, null)).GetEnumerator();

                if (enumeratorMode == EnumerationMode.RequireValues)
                {
                    return enumItems(false);
                }

                return enumItems(true);
            }

            return base.GetEnumerator(hideNonEnum, enumeratorMode, propertyScope);
        }

        private IEnumerator<KeyValuePair<string, JSValue>> enumItems(bool forWrite)
        {
            foreach (var key in _target.Keys)
            {
                var value = forWrite ? new ValueWrapper(this, key) : Context.CurrentGlobalContext.ProxyValue(_target[key]);
                yield return new KeyValuePair<string, JSValue>(key.ToString(), value);
            }
        }
    }
}
