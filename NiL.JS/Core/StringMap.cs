using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NiL.JS.Backward;

namespace NiL.JS.Core
{
    internal sealed class StringMapDebugView<TValue>
    {
        private StringMap<TValue> stringMap;

        public StringMapDebugView(StringMap<TValue> stringMap)
        {
            this.stringMap = stringMap;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, TValue>[] Items
        {
            get
            {
                return new List<KeyValuePair<string, TValue>>(stringMap).ToArray();
            }
        }
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StringMapDebugView<>))]
    public class StringMap<TValue> : IDictionary<string, TValue>
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

        private const int InitialSize = 2;
        private const int MaxAsListSize = 4;

        private int _version;
        private int _count;
        private int _eicount;
        private int _previousIndex;

        private Record[] _records = EmpryArrayHelper.Empty<Record>();
        private int[] _existsedIndexes;

        public StringMap()
        {
            Clear();
        }

        private void insert(string key, TValue value, int hash, bool @throw, bool allowIncrease)
        {
            if (key == null)
                ExceptionHelper.ThrowArgumentNull("key");

            int index;
            int colisionCount = 0;
            var mask = _records.Length - 1;
            if (_records.Length == 0)
                mask = increaseSize() - 1;

            if (_records.Length <= MaxAsListSize)
            {
                for (var i = 0; i < _records.Length; i++)
                {
                    if (_records[i].key == null)
                    {
                        _records[i].hash = -1;
                        _records[i].key = key;
                        _records[i].value = value;

                        ensureExistedIndexCapacity();
                        _existsedIndexes[_eicount] = i;

                        _count++;
                        _eicount++;
                        _version++;
                        return;
                    }

                    if (string.CompareOrdinal(_records[i].key, key) == 0)
                    {
                        if (@throw)
                            ExceptionHelper.Throw(new InvalidOperationException("Item already exists"));

                        _records[i].value = value;
                        return;
                    }
                }

                if (_records.Length * 2 <= MaxAsListSize)
                {
                    index = _records.Length;
                    increaseSize();

                    _records[index].hash = -1;
                    _records[index].key = key;
                    _records[index].value = value;

                    ensureExistedIndexCapacity();
                    _existsedIndexes[_eicount] = index;

                    _count++;
                    _eicount++;
                    _version++;
                    return;
                }
            }
            else
            {
                index = hash & mask;
                do
                {
                    if (_records[index].hash == hash
                        && _records[index].key is not null
                        && string.CompareOrdinal(_records[index].key, key) == 0)
                    {
                        if (@throw)
                            ExceptionHelper.Throw(new InvalidOperationException("Item already Exists"));

                        _records[index].value = value;
                        return;
                    }

                    index = _records[index].next - 1;
                }
                while (index >= 0);
            }

            // не нашли

            if (allowIncrease)
            {
                if ((_count == mask + 1)
                    || (_count > 50 && _count * 6 / 5 >= mask))
                {
                    mask = increaseSize() - 1;
                }
            }

            int prewIndex = -1;
            index = hash & mask;
            var emptySlot = -1;

            if (_records[index].key is null && emptySlot == -1)
                emptySlot = index;

            while (_records[index].next > 0)
            {
                index = _records[index].next - 1;
                colisionCount++;

                if (_records[index].key is null && emptySlot == -1)
                    emptySlot = index;
            }

            prewIndex = index;

            if (emptySlot == -1)
            {
                while (_records[index].key != null)
                    index = (index + 3) & mask;
            }
            else
                index = emptySlot;

            _records[index].hash = hash;
            _records[index].key = key;
            _records[index].value = value;

            if (emptySlot == -1 && prewIndex >= 0)
                _records[prewIndex].next = index + 1;

            ensureExistedIndexCapacity();

            _existsedIndexes[_eicount] = index;
            _eicount++;
            _count++;
            _version++;

            if (colisionCount > 17 && allowIncrease && _eicount * 10 > _records.Length)
                increaseSize();
        }

        private void ensureExistedIndexCapacity()
        {
            if (_eicount == _existsedIndexes.Length)
            {
                // Увеличиваем размер массива с занятыми номерами
                var newEI = new int[_existsedIndexes.Length << 1];
                Array.Copy(_existsedIndexes, newEI, _existsedIndexes.Length);
                _existsedIndexes = newEI;
            }
        }

        private static int computeHash(string key)
        {
            unchecked
            {
                int hash;
                var keyLen = key.Length;
                var m = (keyLen >> 2) | 4;
                hash = (int)((uint)keyLen * 0x30303) ^ 0xb7b7b7;
                if (keyLen > 0)
                {
                    for (var i = 0; i < m; i++)
                    {
                        hash = (hash >> 14)
                             ^ (hash * (int)0xf351_f351)
                             ^ (key[i % keyLen] * 0x34df_5981);
                    }
                }

                return hash;
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var previousIndex = _previousIndex;
            var records = _records;
            if (records.Length <= MaxAsListSize)
            {
                for (var i = 0; i < records.Length; i++)
                {
                    ref var record = ref records[i];
                    if (record.key == null)
                        break;

                    if (string.CompareOrdinal(record.key, key) == 0)
                    {
                        value = record.value;
                        return true;
                    }
                }

                value = default(TValue);
                return false;
            }

            if (previousIndex != -1 && string.CompareOrdinal(records[previousIndex].key, key) == 0)
            {
                value = records[previousIndex].value;
                return true;
            }

            var rcount = records.Length;

            if (rcount == 0)
            {
                value = default(TValue);
                return false;
            }

            int hash = computeHash(key);
            int index = hash & (rcount - 1);

            do
            {
                if (records[index].hash == hash
                    && records[index].key is not null
                    && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    _previousIndex = index;
                    return true;
                }

                index = records[index].next - 1;
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

            if (_records.Length <= MaxAsListSize)
            {
                var found = false;
                for (var i = 0; i < _records.Length; i++)
                {
                    if (found)
                    {
                        _records[i - 1].key = _records[i].key;
                        _records[i - 1].value = _records[i].value;
                        _records[i].key = null;
                        _records[i].value = default(TValue);
                    }
                    else if (string.CompareOrdinal(_records[i].key, key) == 0)
                    {
                        _count--;
                        _eicount--;
                        _version++;
                        found = true;
                        _records[i].key = null;
                        _records[i].value = default(TValue);
                    }
                }

                return found;
            }

            if (_records.Length == 0)
                return false;

            var mask = _records.Length - 1;
            int hash = computeHash(key);
            int index;
            int prevItemIndex = -1;

            for (index = hash & mask; index >= 0; index = _records[index].next - 1)
            {
                if (_records[index].hash == hash
                    && _records[index].key is not null
                    && string.CompareOrdinal(_records[index].key, key) == 0)
                {
                    if (index == _previousIndex)
                        _previousIndex = -1;

                    _records[index].key = null;
                    _records[index].value = default(TValue);
                    _records[index].hash = 0;

                    if (prevItemIndex >= 0 && _records[index].next == 0)
                        _records[prevItemIndex].next = 0;

                    var indexInExIndex = Array.IndexOf(_existsedIndexes, index);
                    Array.Copy(_existsedIndexes, indexInExIndex + 1, _existsedIndexes, indexInExIndex, _existsedIndexes.Length - indexInExIndex - 1);

                    _count--;
                    _eicount--;
                    _version++;

                    return true;
                }

                prevItemIndex = index;
            }

            return false;
        }

        private int increaseSize()
        {
            if (_records.Length == 0)
            {
                _records = new Record[InitialSize];
                _existsedIndexes = new int[InitialSize];
            }
            else
            {
                var oldRecords = _records;
                var newLength = _records.Length << 1;
                _records = new Record[newLength];

                var c = _eicount;
                _count = 0;
                _eicount = 0;

                if (oldRecords.Length == MaxAsListSize)
                {
                    for (var i = 0; i < c; i++)
                    {
                        var index = _existsedIndexes[i];
                        ref var record = ref oldRecords[index];
                        if (record.key != null)
                            record.hash = computeHash(record.key);
                    }
                }

                for (var i = 0; i < c; i++)
                {
                    var index = _existsedIndexes[i];
                    ref var record = ref oldRecords[index];
                    if (record.key != null)
                        insert(record.key, record.value, record.hash, false, false);
                }
            }

            _previousIndex = -1;
            return _records.Length;
        }

        public void Add(string key, TValue value)
        {
            lock (_records)
            {
                insert(key, value, computeHash(key), true, true);
            }
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
                lock (_records)
                {
                    insert(key, value, computeHash(key), false, true);
                }
            }
        }

        public virtual void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (_existsedIndexes != null)
                Array.Clear(_existsedIndexes, 0, _existsedIndexes.Length);
            Array.Clear(_records, 0, _records.Length);
            _count = 0;
            _eicount = 0;
            _version++;
            _previousIndex = -1;
        }

        public virtual bool Contains(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                if (arrayIndex < array.Length)
                {
                    array[arrayIndex++] = item;
                }
                else
                {
                    break;
                }
            }
        }

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            List<KeyValuePair<uint, string>> numbers = null;
            uint exprected = 0;

            var i = 0;
            while (i < _eicount)
            {
                var prevVersion = _version;

                for (; i < _records.Length; i++)
                {
                    int index = i;
                    if (_records[index].key != null
                        && uint.TryParse(_records[index].key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                    {
                        if (exprected == number)
                        {
                            yield return new KeyValuePair<string, TValue>(_records[index].key, _records[index].value);
                            exprected++;
                        }
                        else
                        {
                            if (numbers == null)
                                numbers = new List<KeyValuePair<uint, string>>();

                            numbers.Add(new KeyValuePair<uint, string>(number, _records[i].key));
                        }
                    }
                }

                if (prevVersion != _version)
                {
                    i = 0;
                }

                if (numbers != null)
                {
                    numbers.Sort((x, y) => x.Key.CompareTo(y.Key));

                    for (var ni = 0; ni < numbers.Count && prevVersion == _version; ni++)
                    {
                        if (numbers[ni].Key >= exprected && TryGetValue(numbers[ni].Value, out var item))
                        {
                            yield return new KeyValuePair<string, TValue>(numbers[ni].Value, item);
                            exprected = numbers[ni].Key + 1;
                        }
                    }
                }
            }

            i = 0;
            while (i < _eicount)
            {
                for (; i < _eicount; i++)
                {
                    int index = _existsedIndexes[i];
                    if (_records[index].key != null
                        && (!uint.TryParse(_records[index].key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
                    {
                        yield return new KeyValuePair<string, TValue>(_records[index].key, _records[index].value);
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
