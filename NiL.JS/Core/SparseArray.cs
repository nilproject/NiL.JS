using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NiL.JS.Core
{
    public enum ArrayMode
    {
        Flat,
        Sparse
    }

    public sealed class SparseArray<TValue> : IList<TValue>
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct _NavyItem
        {
            public uint index;
            public uint zeroContinue;
            public uint oneContinue;

            public override string ToString()
            {
                return index + "[" + zeroContinue + ";" + oneContinue + "]";
            }
        }

        private static readonly _NavyItem[] emptyNavyData = new _NavyItem[0]; // data dummy. In cases where instance of current class was created, but not used
        private static readonly TValue[] emptyData = new TValue[0];

        private _NavyItem[] navyData;
        private uint allocatedCount;
        private TValue[] values;
        private uint pseudoLength;
        private ArrayMode mode;

        public ArrayMode Mode
        {
            get
            {
                return mode;
            }
        }

        [CLSCompliant(false)]
        public uint Length
        {
            get
            {
                return pseudoLength;
            }
        }

        public SparseArray()
        {
            mode = ArrayMode.Flat;
            values = emptyData;
            navyData = emptyNavyData;
        }

        public SparseArray(int capacity)
        {
            mode = ArrayMode.Flat;
            values = emptyData;
            navyData = emptyNavyData;
            if (capacity > 0)
                ensureCapacity(capacity);
        }

        #region Члены IList<TValue>

        public int IndexOf(TValue item)
        {
            for (var i = 0; i < allocatedCount; i++)
                if (object.Equals(values[i], item))
                {
                    if (mode == ArrayMode.Flat)
                        return i;
                    return (int)navyData[i].index;
                }
            return -1;
        }

        void IList<TValue>.Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If "index" not equal "Length - 1", method will fail
        /// </summary>
        /// <param name="index">Index of item for removing</param>
        public void RemoveAt(int index)
        {
            if (pseudoLength == 0 || index != (int)(pseudoLength - 1))
                throw new InvalidOperationException();
            this[(int)(pseudoLength - 1)] = default(TValue);
            pseudoLength--;
        }

        public TValue this[int index]
        {
            get
            {
                if (mode == ArrayMode.Flat)
                {
                    if (index < 0 || pseudoLength <= index || values.Length <= index)
                        return default(TValue);

                    return values[index];
                }

                if (navyData.Length == 0)
                    return default(TValue);

                uint _index = (uint)index;
                int bi = 31;
                uint i = 0;

                if (_index < allocatedCount)
                {
                    if (navyData[index].index == _index)
                        return values[index];
                }

                for (; ; bi--)
                {
                    i = (_index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                    if (i == 0)
                    {
                        return default(TValue);
                    }
                    else if (navyData[i].index == _index)
                    {
                        return values[i];
                    }
                }
            }
            set
            {
                bool @default = value == null; // структуры мы будем записывать, иначе пришлось бы вызывать тяжелые операции сравнения.
                //if (navyData.Length <= allocatedCount)
                //    ensureCapacity(navyData.Length * 2);
                uint _index = (uint)index;
                if (mode == ArrayMode.Flat)
                {
                    if (index < 0 || index > pseudoLength)
                    {
                        if (@default)
                        {
                            if (_index >= pseudoLength)
                                pseudoLength = _index + 1;
                            return;
                        }

                        if (_index < 8)
                        {
                            // Покрывает много тех случаев, когда относительно маленький массив заполняют с конца. 
                            // Кто-то верит, что это должно работать быстрее. 
                            // Вот именно из-за таких кусков кода так и может показаться.
                            // Не время для попыток исправить мир
                            ensureCapacity(8);
                            pseudoLength = _index + 1;
                            this[index] = value;
                            return;
                        }
                        else
                            rebuildToSparse();
                    }
                    else
                    {
                        if (values.Length <= index)
                            ensureCapacity(Math.Max(index + 1, values.Length * 2));
                        if (pseudoLength == index)
                            pseudoLength = _index + 1;
                        values[index] = value;
                        return;
                    }
                }

                if (allocatedCount == 0)
                {
                    allocatedCount = 1;
                    pseudoLength = 1;
                }

                if (_index < allocatedCount)
                {
                    if (navyData[index].index == _index)
                    {
                        values[index] = value;
                        if (pseudoLength <= index)
                            pseudoLength = _index + 1;
                        return;
                    }
                }

                int bi = 31;
                for (uint i = 0, ni = 0; ; bi--)
                {
                    if (navyData[i].index > _index)
                    {
                        if (@default)
                        {
                            if (pseudoLength <= _index)
                                pseudoLength = _index + 1; // длина может быть меньше 
                            // уже записанных элементов если эти элементы имеют значение 
                            // по-умолчанию и был вызван Trim
                            return;
                        }

                        var oi = navyData[i].index;
                        var ov = values[i];
                        navyData[i].index = _index;
                        values[i] = value;
                        if (oi < pseudoLength)
                            this[(int)oi] = ov;
                        return;
                    }
                    else if (navyData[i].index < _index)
                    {
                        var b = (_index & (1 << bi)) == 0;
                        ni = b ? navyData[i].zeroContinue : navyData[i].oneContinue;
                        if (ni == 0)
                        {
                            if (pseudoLength <= _index)
                                pseudoLength = _index + 1;

                            if (@default)
                                return;

                            if (b)
                                navyData[i].zeroContinue = ni = allocatedCount++;
                            else
                                navyData[i].oneContinue = ni = allocatedCount++;

                            if (navyData.Length <= allocatedCount)
                                ensureCapacity(navyData.Length * 2);

                            navyData[ni].index = _index;
                            values[ni] = value;
                            return;
                        }

                        i = ni;
                    }
                    else
                    {
                        values[i] = value;
                        if (pseudoLength <= index)
                            pseudoLength = _index + 1;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Члены ICollection<TValue>

        public void Add(TValue item)
        {
            if (pseudoLength == uint.MaxValue)
                throw new InvalidOperationException();

            this[(int)(pseudoLength)] = item;
        }

        public void Clear()
        {
            while (allocatedCount > 0)
            {
                navyData[(int)(--allocatedCount)] = default(_NavyItem);
                values[(int)allocatedCount] = default(TValue);
            }
            pseudoLength = 0;
        }

        public bool Contains(TValue item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array == null)
                throw new NullReferenceException();
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (Math.Min(pseudoLength, int.MaxValue) - arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException();
            for (var i = Math.Min(pseudoLength, int.MaxValue) + arrayIndex; i-- > arrayIndex;)
                array[i] = default(TValue);
            foreach (var v in DirectOrder)
                if (v.Key >= 0)
                    array[v.Key + arrayIndex] = v.Value;
        }

        int ICollection<TValue>.Count
        {
            get { return (int)pseudoLength; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            for (var i = 0u; i < pseudoLength; i++)
                yield return this[(int)i];
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Zero if the requested index does not Exists</returns>
        public long NearestIndexNotLess(long index)
        {
            if (mode == ArrayMode.Sparse)
            {
                if (navyData.Length == 0)
                    return index;
            }
            else
            {
                if (values.Length < index)
                    return 0;
                return index;
            }
            int bi = 31;
            long i = 0;
            long pm = -1;
            for (; ; bi--)
            {
                if (navyData[i].oneContinue != 0)
                    pm = i;
                i = (index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                if (i == 0)
                {
                    if (pm == -1)
                        return 0;
                    i = navyData[pm].oneContinue;
                    for (;;)
                    {
                        if (navyData[i].zeroContinue != 0)
                        {
                            i = navyData[i].zeroContinue;
                            continue;
                        }
                        if (navyData[i].oneContinue != 0)
                        {
                            i = navyData[i].oneContinue;
                            continue;
                        }
                        break;
                    }
                }
                if (navyData[i].index >= (uint)index)
                {
                    return navyData[i].index;
                }
            }
        }

        public long NearestIndexNotMore(long index)
        {
            if (mode == ArrayMode.Sparse)
            {
                if (navyData.Length == 0)
                    return 0;
            }
            else
            {
                return Math.Min(values.Length, index);
            }
            int bi = 31;
            long i = 0;
            for (; ; bi--)
            {
                var ni = (index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                if (ni == 0 || navyData[ni].index > index)
                    index = navyData[i].index;
                else
                    i = ni;
                if (navyData[i].index == index)
                {
                    return navyData[i].index;
                }
            }
        }

        public IEnumerable<KeyValuePair<int, TValue>> DirectOrder
        {
            get
            {
                var index = 1U;
                if (mode == ArrayMode.Flat)
                {
                    for (var i = 0; i < pseudoLength; i++)
                    {
                        if (i >= values.Length) // была насильно установлена длина фиктивным элементом.
                        {
                            yield return new KeyValuePair<int, TValue>((int)(pseudoLength - 1), default(TValue));
                            yield break;
                        }
                        yield return new KeyValuePair<int, TValue>(i, values[i]);
                        if (mode != ArrayMode.Flat)
                        {
                            index = (uint)(i + 1);
                            break;
                        }
                    }
                }
                if (mode == ArrayMode.Sparse) // Режим может поменяться во время итерации в режиме Flat
                {
                    if (allocatedCount > 0)
                        yield return new KeyValuePair<int, TValue>(0, values[0]);
                    else
                    {
                        yield return new KeyValuePair<int, TValue>((int)(pseudoLength - 1), default(TValue));
                        yield break;
                    }
                    while (index < pseudoLength)
                    {
                        int bi = 31;
                        long i = 0;
                        long pm = -1;
                        for (; ; bi--)
                        {
                            if (navyData[i].oneContinue != 0)
                                pm = i;
                            i = (index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                            if (i == 0)
                            {
                                if (pm == -1)
                                {
                                    yield return new KeyValuePair<int, TValue>((int)(pseudoLength - 1), default(TValue));
                                    yield break;
                                }
                                i = navyData[pm].oneContinue;
                                for (;;)
                                {
                                    if (navyData[i].zeroContinue != 0)
                                    {
                                        i = navyData[i].zeroContinue;
                                        continue;
                                    }
                                    if (navyData[i].oneContinue != 0)
                                    {
                                        i = navyData[i].oneContinue;
                                        continue;
                                    }
                                    break;
                                }
                            }
                            if (navyData[i].index >= index)
                            {
                                index = navyData[i].index;
                                yield return new KeyValuePair<int, TValue>((int)index, values[i]);
                                index++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<int, TValue>> ReversOrder
        {
            get
            {
                var index = pseudoLength - 1;
                if (mode == ArrayMode.Flat)
                {
                    if (pseudoLength > values.Length)
                        yield return new KeyValuePair<int, TValue>((int)(pseudoLength - 1), default(TValue));
                    for (var i = Math.Min(values.Length, pseudoLength); i-- > 0;)
                    {
                        if (mode != ArrayMode.Flat)
                        {
                            index = (uint)i;
                            break;
                        }
                        yield return new KeyValuePair<int, TValue>((int)i, values[i]);
                    }
                }
                if (mode == ArrayMode.Sparse)
                {
                    if (allocatedCount == 0)
                        yield break;
                    while (index > 0)
                    {
                        int bi = 31;
                        long i = 0;
                        for (; ; bi--)
                        {
                            var ni = (index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                            if (ni == 0 || navyData[ni].index > index)
                                index = navyData[i].index;
                            else
                                i = ni;
                            if (navyData[i].index == index)
                            {
                                yield return new KeyValuePair<int, TValue>((int)index, values[i]);
                                if (index == 0)
                                    yield break;
                                index--;
                                break;
                            }
                        }
                    }
                    yield return new KeyValuePair<int, TValue>(0, values[0]);
                }
            }
        }

        /// <summary>
        /// Reduce length to "index of last item with non-default value" + 1
        /// </summary>
        public void Trim()
        {
            long len = -1;
            if (mode == ArrayMode.Flat)
            {
                for (var i = values.Length; i-- > 0;)
                {
                    if (!object.Equals(values[i], default(TValue)))
                    {
                        len = i;
                        break;
                    }
                }
            }
            else
            {
                for (var i = allocatedCount; i-- > 0;)
                {
                    if (navyData[i].index > len && !object.Equals(values[i], default(TValue)))
                        len = navyData[i].index;
                }
            }
            pseudoLength = (uint)(len + 1);
        }

        private void ensureCapacity(int p)
        {
            p = Math.Max(4, p);
            var newValues = new TValue[p];
            if (values != null)
                for (var i = 0; i < values.Length; i++)
                    newValues[i] = values[i];
            values = newValues;

            if (mode == ArrayMode.Sparse)
            {
                var newData = new _NavyItem[p];
                for (var i = 0; i < navyData.Length; i++)
                    newData[i] = navyData[i];
                navyData = newData;
            }
        }

        private void rebuildToSparse()
        {
            allocatedCount = 0;
            mode = ArrayMode.Sparse;
            var len = pseudoLength;
            if (len == 0)
            {
                ensureCapacity(0);
                return;
            }
            navyData = new _NavyItem[values.Length];
            for (var i = 0; i < len; i++)
                this[i] = values[i];
        }
    }
}
