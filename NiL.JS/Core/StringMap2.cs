using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public sealed class StringMap2<TValue> : IDictionary<string, TValue>
    {
        private struct Record
        {
            public int next;
            public int hash;
            public string key;
            public TValue value;

#if DEBUG
            public override string ToString()
            {
                return "[" + key + ", " + value + "]";
            }
#endif
        }

        private static readonly Record[] emptyRecords = new Record[0];

        private bool emptyKeyValueExists = false;
        private TValue emptyKeyValue;

        private Record[] records = emptyRecords;
        private int[] existedIndexes;

        private int count;
        private int eicount;

        private void insert(string key, TValue value, int hash, bool @throw, bool resizeMode)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (@throw && emptyKeyValueExists)
                    throw new InvalidOperationException("Item already exists");
                emptyKeyValueExists = true;
                emptyKeyValue = value;
                //count++;
                return;
            }
            var elen = records.Length - 1;
            if (records.Length == 0)
                elen = increaseSize() - 1;
            int index;
            int colisionCount = 0;
            index = hash & elen;
            do
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    if (@throw)
                        throw new InvalidOperationException("Item already exists");
                    records[index].value = value;
                    return;
                }
                index = records[index].next - 1;
            }
            while (index >= 0);
            // не нашли

            if (
                (count > 50 && count * 9 / 5 >= elen) ||
                count == elen + 1)
                elen = increaseSize() - 1;
            int prewIndex = -1;
            index = hash & elen;

            if (records[index].key != null)
            {
                while (records[index].next > 0)
                {
                    index = records[index].next - 1;
                    colisionCount++;
                }
                prewIndex = index;
                while (records[index].key != null)
                    index = (index + 61) & elen;
            }
            records[index].hash = hash;
            records[index].key = key;
            records[index].value = value;
            if (prewIndex >= 0)
                records[prewIndex].next = index + 1;
            if (eicount == existedIndexes.Length)
            {
                var newEI = new int[existedIndexes.Length << 1];
                Array.Copy(existedIndexes, newEI, existedIndexes.Length);
                existedIndexes = newEI;
            }
            existedIndexes[eicount++] = index;
            count++;

            if (colisionCount > 17)
                increaseSize();
        }

        private static int computeHash(string key)
        {
            int hash;
            var keyLen = key.Length;
            hash = keyLen * 0x51 ^ 0xe5b5e5;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 28) + (hash << 4) + key[i];
            return hash;
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (!emptyKeyValueExists)
                {
                    value = default(TValue);
                    return false;
                }
                value = emptyKeyValue;
                return true;
            }
            if (records.Length == 0)
            {
                value = default(TValue);
                return false;
            }
            var elen = records.Length - 1;
            int hash;
            int index;
            hash = key[0];
            index = hash & elen;
            hash = computeHash(key);
            //hash = key.GetHashCode();
            for (index = hash & elen; index >= 0; index = records[index].next - 1)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    return true;
                }
            }
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
                if (!emptyKeyValueExists)
                {
                    return false;
                }
                emptyKeyValue = default(TValue);
                emptyKeyValueExists = false;
                return true;
            }
            if (records.Length == 0)
                return false;
            var elen = records.Length - 1;
            int hash;
            int index;
            int targetIndex = -1, prewIndex;
            hash = key[0];
            index = hash & elen;
            hash = computeHash(key);
            //hash = key.GetHashCode();
            for (index = hash & elen; index >= 0; index = records[index].next - 1)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    if (records[index].next > 0)
                    {
                        prewIndex = targetIndex;
                        targetIndex = index;
                        index = records[index].next - 1;
                        do
                        {
                            if ((records[index].hash & elen) >= targetIndex)
                            {
                                records[targetIndex] = records[index];
                                records[targetIndex].next = index + 1;
                                prewIndex = targetIndex;
                                targetIndex = index;
                            }
                            index = records[index].next - 1;
                        }
                        while (index >= 0);
                        records[targetIndex].key = null;
                        records[targetIndex].value = default(TValue);
                        records[targetIndex].hash = 0;
                        if (prewIndex >= 0)
                            records[prewIndex].next = 0;
                    }
                    else
                    {
                        records[index].key = null;
                        records[index].value = default(TValue);
                        records[index].hash = 0;
                        if (targetIndex >= 0)
                            records[targetIndex].next = 0;
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
            if (records.Length == 0)
            {
                records = new Record[1];
                existedIndexes = new int[1];
                return 1;
            }
            //if (count > 100 && records.Length / count > 50)
            //    throw new InvalidOperationException();
            var oldRecords = records;
            records = new Record[records.Length << 1];
            int i = 0, c = eicount;
            count = 0;
            eicount = 0;
            for (; i < c; i++)
            {
                var index = existedIndexes[i];
                if (oldRecords[index].key != null)
                    insert(oldRecords[index].key, oldRecords[index].value, oldRecords[index].hash, false, true);
            }
            return records.Length;
        }

        public void Add(string key, TValue value)
        {
            insert(key, value, computeHash(key), true, false);
        }

        public bool ContainsKey(string key)
        {
            TValue fake;
            return TryGetValue(key, out fake);
        }

        public ICollection<string> Keys
        {
            get { return (from item in records where item.key != null select item.key).ToArray(); }
        }

        public ICollection<TValue> Values
        {
            get { return (from item in records where item.key != null select item.value).ToArray(); }
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
                insert(key, value, computeHash(key), false, false);
            }
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            if (existedIndexes != null)
                Array.Clear(existedIndexes, 0, existedIndexes.Length);
            Array.Clear(records, 0, records.Length);
            count = 0;
            eicount = 0;
            emptyKeyValue = default(TValue);
            emptyKeyValueExists = false;
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
            get { return count; }
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
            if (emptyKeyValueExists)
                yield return new KeyValuePair<string, TValue>("", emptyKeyValue);
            for (int i = 0; i < eicount; i++)
            {
                if (records[existedIndexes[i]].key != null)
                    yield return new KeyValuePair<string, TValue>(records[existedIndexes[i]].key, records[existedIndexes[i]].value);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
