using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.Backward;

namespace NiL.JS.Core.Interop
{
#if !NETCORE
    [Serializable]
#endif
    public class JsonSerializersRegistry
    {
        private readonly List<JsonSerializer> _serializers;

        public JsonSerializersRegistry()
        {
            _serializers = new List<JsonSerializer>();
        }

        public void AddJsonSerializer(JsonSerializer jsonSerializer)
        {
            if (jsonSerializer == null)
                throw new ArgumentNullException(nameof(jsonSerializer));

            var index = getSerializerIndex(jsonSerializer.TargetType, true);
            if (index < 0)
                _serializers.Add(jsonSerializer);
            else
                _serializers.Insert(index, jsonSerializer);
        }

        public JsonSerializer GetJsonSerializer(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var index = getSerializerIndex(targetType, false);
            if (index < 0)
                return null;

            return _serializers[index];
        }

        public JsonSerializer GetSuitableJsonSerializer(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var index = getSerializerIndex(value.GetType(), true);
            if (index < 0)
                return null;

            while (index < _serializers.Count)
            {
                if (_serializers[index].CanSerialize(value))
                    return _serializers[index];

                index++;
            }

            return null;
        }

        public bool RemoveJsonSerializer(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var index = getSerializerIndex(targetType, false);
            if (index < 0)
                return false;

            _serializers.RemoveAt(index);
            return true;
        }

        private int getSerializerIndex(Type type, bool skipTypeCheck)
        {
            if (_serializers.Count == 0)
                return -1;

            var weight = 0;
            var curType = type;
            while (curType != null && curType != typeof(object))
            {
                weight++;
                curType = curType.GetTypeInfo().BaseType;
            }

            var indexMore = 0;
            var indexLess = _serializers.Count - 1;
            while (indexMore < indexLess - 1)
            {
                var index = indexMore + (indexLess - indexMore) / 2;
                if (_serializers[index].Weight > weight)
                    indexMore = index;
                else
                    indexLess = index;
            }

            if (skipTypeCheck)
            {
                if (weight >= _serializers[indexMore].Weight)
                    return indexMore;

                return indexLess;
            }

            if (weight > _serializers[indexMore].Weight || weight < _serializers[indexLess].Weight)
                return -1;

            for (; indexMore < _serializers.Count && weight <= _serializers[indexMore].Weight; indexMore++)
            {
                if (_serializers[indexMore].TargetType == type)
                    return indexMore;
            }

            return -1;
        }
    }
}