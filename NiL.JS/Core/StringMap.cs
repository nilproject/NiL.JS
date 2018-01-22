using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        private static readonly Record[] emptyRecords = new Record[0];

        private int _count;
        private int _eicount;
        private int _previousIndex;

        private Record[] _records = emptyRecords;
        private int[] _existsedIndexes;

        private bool _emptyKeyValueExists = false;
        private TValue _emptyKeyValue;

        public StringMap()
        {
            Clear();
        }

        private void insert(string key, TValue value, int hash, bool @throw)
        {
            if (key == null)
                ExceptionHelper.ThrowArgumentNull("key");

            if (key.Length == 0)
            {
                if (@throw && _emptyKeyValueExists)
                    ExceptionHelper.Throw(new InvalidOperationException("Item already exists"));
                _emptyKeyValueExists = true;
                _emptyKeyValue = value;
                return;
            }

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
                        return;
                    }

                    if (string.CompareOrdinal(_records[i].key, key) == 0)
                    {
                        if (@throw)
                            ExceptionHelper.Throw(new InvalidOperationException("Item already Exists"));

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
                    return;
                }
            }
            else
            {
                index = hash & mask;
                do
                {
                    if (_records[index].hash == hash && string.CompareOrdinal(_records[index].key, key) == 0)
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

            if ((_count == mask + 1)
                || (_count > 50 && _count * 8 / 5 >= mask))
            {
                mask = increaseSize() - 1;
            }

            int prewIndex = -1;
            index = hash & mask;

            if (_records[index].key != null)
            {
                while (_records[index].next > 0)
                {
                    index = _records[index].next - 1;
                    colisionCount++;
                }

                prewIndex = index;
                while (_records[index].key != null)
                    index = (index + 3) & mask;
            }

            _records[index].hash = hash;
            _records[index].key = key;
            _records[index].value = value;
            if (prewIndex >= 0)
                _records[prewIndex].next = index + 1;

            ensureExistedIndexCapacity();

            _existsedIndexes[_eicount] = index;
            _eicount++;
            _count++;

            if (colisionCount > 17)
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
                int isNumber = int.MinValue & (-keyLen);
                char c;
                hash = (int)((uint)keyLen * 0x55) ^ 0xe5b5e5;

                for (var i = 0; i < keyLen; i++)
                {
                    c = key[i];
                    c -= (char)((uint)((i - 1) & ~(keyLen - 2)) >> 31);
                    hash += (hash >> 28) + (hash << 4) + c;
                    isNumber &= ('0' - c - 1) & (c - '9' - 1);
                }

                hash &= int.MaxValue;
                hash |= isNumber;

                return hash;
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
                ExceptionHelper.ThrowArgumentNull("key");

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

            var previousIndex = _previousIndex;
            var records = _records;
            if (records.Length <= MaxAsListSize)
            {
                for (var i = 0; i < records.Length; i++)
                {
                    if (records[i].key == null)
                        break;

                    if (string.CompareOrdinal(records[i].key, key) == 0)
                    {
                        value = records[i].value;
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
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
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
            int targetIndex = -1;
            int prewIndex;

            for (index = hash & mask; index >= 0; index = _records[index].next - 1)
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
                            if ((_records[index].hash & mask) == targetIndex)
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

                        index = targetIndex;
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

                    _count--;
                    _eicount--;

                    var indexInExIndex = Array.IndexOf(_existsedIndexes, index);
                    Array.Copy(_existsedIndexes, indexInExIndex + 1, _existsedIndexes, indexInExIndex, _existsedIndexes.Length - indexInExIndex - 1);

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
                _records = new Record[InitialSize];
                _existsedIndexes = new int[InitialSize];
            }
            else
            {
                var oldRecords = _records;
                var newLength = _records.Length << 1;
                _records = new Record[newLength];

                var i = 0;
                var c = _eicount;
                _count = 0;
                _eicount = 0;
                for (; i < c; i++)
                {
                    var index = _existsedIndexes[i];
                    if (oldRecords[index].key != null)
                    {
                        if (newLength == MaxAsListSize << 1)
                            insert(oldRecords[index].key, oldRecords[index].value, computeHash(oldRecords[index].key), false);
                        else
                            insert(oldRecords[index].key, oldRecords[index].value, oldRecords[index].hash, false);
                    }
                }
            }

            _previousIndex = -1;
            return _records.Length;
        }

        public void Add(string key, TValue value)
        {
            lock (_records)
            {
                insert(key, value, computeHash(key), true);
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
                    insert(key, value, computeHash(key), false);
                }
            }
        }

        public virtual void Add(KeyValuePair<string, TValue> item)
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

        public virtual bool Contains(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        public int Count
        {
            get { return _count + (_emptyKeyValueExists ? 1 : 0); }
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
            if (_emptyKeyValueExists)
                yield return new KeyValuePair<string, TValue>("", _emptyKeyValue);

            uint number;
            uint foundNumber;
            uint exprected = 0;
            bool repeat = false;
            do
            {
                repeat = false;
                foundNumber = uint.MaxValue;
                int index = -1;
                for (int i = 0; i < _records.Length; i++)
                {
                    if (_records[i].key != null
                        && _records[i].hash < 0
                        && uint.TryParse(_records[i].key, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                    {
                        if (number >= exprected && number <= foundNumber)
                        {
                            if (number == exprected)
                            {
                                yield return new KeyValuePair<string, TValue>(_records[i].key, _records[i].value);
                                if (number == uint.MaxValue)
                                {
                                    repeat = false;
                                    break;
                                }

                                exprected = number + 1;
                                repeat = true;
                                if (index != -1)
                                {
                                    foundNumber = uint.MaxValue;
                                    i = index - 1;
                                    index = -1;
                                }
                            }
                            else
                            {
                                repeat |= index != -1;
                                foundNumber = number;
                                index = i;
                            }
                        }
                        else
                        {
                            repeat |= number > exprected;
                        }
                    }
                }

                if (index != -1)
                {
                    yield return new KeyValuePair<string, TValue>(_records[index].key, _records[index].value);
                    if (foundNumber == uint.MaxValue)
                    {
                        break;
                    }

                    exprected = foundNumber + 1;
                    repeat = true;
                }
            }
            while (repeat);

            for (int i = 0; i < _eicount; i++)
            {
                int index = _existsedIndexes[i];
                if (_records[index].key != null
                    && (_records[i].hash >= 0
                        || !uint.TryParse(_records[i].key, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)))
                {
                    yield return new KeyValuePair<string, TValue>(
                        _records[index].key,
                        _records[index].value);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
