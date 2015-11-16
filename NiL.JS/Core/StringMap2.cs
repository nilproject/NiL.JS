using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NiL.JS.Core
{
    public sealed class StringMap2<TValue> : IDictionary<string, TValue>
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            // Порядок полей не менять!
            public int hash;
            public string key;
            public int next;
            public TValue value;
#if DEBUG
            public override string ToString()
            {
                return "[" + key + ", " + value + "]";
            }
#endif
        }

        private static readonly Record[] emptyRecords = new Record[0];
        
        private Record[] _records = emptyRecords;
        private int[] _existsedIndexes;

        private bool _emptyKeyValueExists = false;
        private TValue _emptyKeyValue;

        private int _count;
        private int _eicount;
        private int _previousIndex;

        public StringMap2()
        {
            Clear();
        }

        private void insert(string key, TValue value, int hash, bool @throw)
        {
            if (key == null)
                ExceptionsHelper.ThrowArgumentNull("key");
            if (key.Length == 0)
            {
                if (@throw && _emptyKeyValueExists)
                    ExceptionsHelper.Throw(new InvalidOperationException("Item already Exists"));
                _emptyKeyValueExists = true;
                _emptyKeyValue = value;
                //count++;
                return;
            }
            var elen = _records.Length - 1;
            if (_records.Length == 0)
                elen = increaseSize() - 1;
            int index;
            int colisionCount = 0;
            index = hash & elen;
            do
            {
                if (_records[index].hash == hash && string.CompareOrdinal(_records[index].key, key) == 0)
                {
                    if (@throw)
                        ExceptionsHelper.Throw(new InvalidOperationException("Item already Exists"));
                    _records[index].value = value;
                    return;
                }
                index = _records[index].next - 1;
            }
            while (index >= 0);
            // не нашли

            if ((_count > 50 && _count * 9 / 5 >= elen) || _count == elen + 1)
                elen = increaseSize() - 1;
            int prewIndex = -1;
            index = hash & elen;

            if (_records[index].key != null)
            {
                while (_records[index].next > 0)
                {
                    index = _records[index].next - 1;
                    colisionCount++;
                }
                prewIndex = index;
                while (_records[index].key != null)
                    index = (index + 61) & elen;
            }
            _records[index].hash = hash;
            _records[index].key = key;
            _records[index].value = value;
            if (prewIndex >= 0)
                _records[prewIndex].next = index + 1;
            if (_eicount == _existsedIndexes.Length)
            {
                // Увеличиваем размер массива с занятыми номерами
                var newEI = new int[_existsedIndexes.Length << 1];
                Array.Copy(_existsedIndexes, newEI, _existsedIndexes.Length);
                _existsedIndexes = newEI;
            }
            _existsedIndexes[_eicount++] = index;
            _count++;

            if (colisionCount > 17)
                increaseSize();
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static int computeHash(string key)
        {
            unchecked
            {
                int hash;
                var keyLen = key.Length;
                hash = keyLen * 0x55 ^ 0xe5b5e5;
                for (var i = 0; i < keyLen; i++)
                    hash += (hash >> 28) + (hash << 4) + key[i];
                return hash;
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
                ExceptionsHelper.ThrowArgumentNull("key");
            if (key.Length == 0)
            {
                if (!_emptyKeyValueExists)
                {
                    value = default(TValue);
                    return false;
                }
                value = _emptyKeyValue;
                return true;
            }
            if (_records.Length == 0)
            {
                value = default(TValue);
                return false;
            }
            if (_previousIndex != -1 && string.CompareOrdinal(_records[_previousIndex].key, key) == 0)
            {
                value = _records[_previousIndex].value;
                return true;
            }
            int hash = computeHash(key);
            int index = hash & (_records.Length - 1);
            do
            {
                if (_records[index].hash == hash && string.CompareOrdinal(_records[index].key, key) == 0)
                {
                    _previousIndex = index;
                    value = _records[index].value;
                    return true;
                }
                index = _records[index].next - 1;
            }
            while (index >= 0);
            value = default(TValue);
            return false;
        }

        public bool Remove(string key)
        {
            /*
             * Нужно найти удаляемую запись, пометить её пустой и передвинуть всю цепочку next на один элемент назад по списку next.
             * При этом возможны ситуации когда элемент встанет на свою законную позицию (при вставке была псевдоколлизия).
             * в таком случае нужно убрать его из цепочки и таким образом уменьшить список коллизии.
             */
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (!_emptyKeyValueExists)
                {
                    return false;
                }
                _emptyKeyValue = default(TValue);
                _emptyKeyValueExists = false;
                return true;
            }
            if (_records.Length == 0)
                return false;
            var elen = _records.Length - 1;
            int hash;
            int index;
            int targetIndex = -1;
            int prewIndex;
            hash = key[0];
            index = hash & elen;
            hash = computeHash(key);
            //hash = key.GetHashCode();
            for (index = hash & elen; index >= 0; index = _records[index].next - 1)
            {
                if (_records[index].hash == hash && string.CompareOrdinal(_records[index].key, key) == 0)
                {
                    if (_records[index].next > 0)
                    {
                        prewIndex = targetIndex;
                        targetIndex = index;
                        index = _records[index].next - 1;
                        do
                        {
                            if ((_records[index].hash & elen) >= targetIndex)
                            {
                                _records[targetIndex] = _records[index];
                                _records[targetIndex].next = index + 1;
                                prewIndex = targetIndex;
                                targetIndex = index;
                            }
                            index = _records[index].next - 1;
                        }
                        while (index >= 0);
                        _records[targetIndex].key = null;
                        _records[targetIndex].value = default(TValue);
                        _records[targetIndex].hash = 0;
                        if (targetIndex == _previousIndex)
                            _previousIndex = -1;
                        if (prewIndex >= 0)
                            _records[prewIndex].next = 0;
                    }
                    else
                    {
                        if (index == _previousIndex)
                            _previousIndex = -1;
                        _records[index].key = null;
                        _records[index].value = default(TValue);
                        _records[index].hash = 0;
                        if (targetIndex >= 0)
                            _records[targetIndex].next = 0;
                    }
                    return true;
                }
                prewIndex = targetIndex;
                targetIndex = index;
            }
            return false;
        }

        private int increaseSize()
        {
            if (_records.Length == 0)
            {
                _records = new Record[4];
                _existsedIndexes = new int[4];
            }
            else
            {
                //if (count > 100 && records.Length / count > 50)
                //    throw new InvalidOperationException();
                var oldRecords = _records;
                _records = new Record[_records.Length << 1];
                int i = 0, c = _eicount;
                _count = 0;
                _eicount = 0;
                for (; i < c; i++)
                {
                    var index = _existsedIndexes[i];
                    if (oldRecords[index].key != null)
                        insert(oldRecords[index].key, oldRecords[index].value, oldRecords[index].hash, false);
                }
            }
            _previousIndex = -1;
            return _records.Length;
        }

        public void Add(string key, TValue value)
        {
            insert(key, value, computeHash(key), true);
        }

        public bool ContainsKey(string key)
        {
            TValue fake;
            return TryGetValue(key, out fake);
        }

        public ICollection<string> Keys
        {
            get { return (from item in _records where item.key != null select item.key).ToArray(); }
        }

        public ICollection<TValue> Values
        {
            get { return (from item in _records where item.key != null select item.value).ToArray(); }
        }

        public TValue this[string key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                    throw new KeyNotFoundException();
                return result;
            }
            set
            {
                insert(key, value, computeHash(key), false);
            }
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            if (_existsedIndexes != null)
                Array.Clear(_existsedIndexes, 0, _existsedIndexes.Length);
            Array.Clear(_records, 0, _records.Length);
            _count = 0;
            _eicount = 0;
            _emptyKeyValue = default(TValue);
            _emptyKeyValueExists = false;
            _previousIndex = -1;
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            if (_emptyKeyValueExists)
                yield return new KeyValuePair<string, TValue>("", _emptyKeyValue);
            for (int i = 0; i < _eicount; i++)
            {
                if (_records[_existsedIndexes[i]].key != null)
                    yield return new KeyValuePair<string, TValue>(_records[_existsedIndexes[i]].key, _records[_existsedIndexes[i]].value);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
