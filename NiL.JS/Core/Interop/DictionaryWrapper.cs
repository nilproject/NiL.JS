using System.Collections.Generic;
using System.Linq;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Interop
{
    public static class DictionaryWrapper
    {
        public static DictionaryWrapper<TKey, TValue> Of<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            => new DictionaryWrapper<TKey, TValue>(dictionary);
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
                    base.Assign(Marshal(value));
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

                return Marshal(_target[oKey]);
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
                var value = forWrite ? new ValueWrapper(this, key) : Marshal(_target[key]);
                yield return new KeyValuePair<string, JSValue>(key.ToString(), value);
            }
        }
    }
}
