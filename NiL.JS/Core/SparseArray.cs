using System;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    public sealed class SparseArray<TValue> : IList<TValue>, IEnumerable<TValue>, IEnumerable<KeyValuePair<int, TValue>>
    {
        private readonly static Random random = new Random(Environment.TickCount);
        private readonly static byte[] rbarray = new byte[4];
        private static int rnd()
        {
            random.NextBytes(rbarray);
            return (rbarray[0] << 24) | (rbarray[1] << 16) | (rbarray[2] << 8) | rbarray[3];
        }

        private struct _Item
        {
            public uint index;
            public uint zeroContinue;
            public uint oneContinue;
            public int bitIndex;
        }

        private static readonly _Item[] emptyData = new _Item[0]; // data dummy. In cases where instance of current class was created, but not used

        private _Item[] navyData;
        private TValue[] values;
        private uint allocatedCount;
        private uint pseudoLength = 0;

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
            navyData = emptyData;
        }

        public SparseArray(int capacity)
        {
            navyData = emptyData;
            if (capacity > 0)
                ensureCapacity(capacity);
        }

        #region Члены IList<TValue>

        public int IndexOf(TValue item)
        {
            for (var i = 0; i < allocatedCount; i++)
                if (object.Equals(values[i], item))
                    return (int)navyData[i].index;
            return -1;
        }

        void IList<TValue>.Insert(int index, TValue item)
        {
            throw new InvalidOperationException();
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

        /// <summary>
        /// Reduce length to "index of last item with non-default value" + 1
        /// </summary>
        public void Trim()
        {
            long len = -1;
            for (var i = 0; i < allocatedCount; i++)
            {
                if (navyData[i].index > len && !object.Equals(values[i], default(TValue)))
                    len = navyData[i].index;
            }
            pseudoLength = (uint)(len + 1);
        }

        public TValue this[int index]
        {
            get
            {
                if (navyData.Length == 0)
                    return default(TValue);
                uint _index = (uint)index;
                bool first = true;
                int bi = 31;
                for (uint i = 0; ; bi--)
                {
                    if (first && i == 0 && _index < allocatedCount)
                    {
                        bi = navyData[(int)_index].bitIndex;
                        i = (uint)(-1 ^ ((1 << bi) - 1));
                        if ((navyData[_index].index & i) != (_index & i))
                        {
                            bi = 31;
                            i = 0;
                        }
                        else
                            i = _index;
                    }
                    if (navyData[i].index != _index)
                    {
                        i = (_index & (1 << bi)) == 0 ? navyData[i].zeroContinue : navyData[i].oneContinue;
                        if (i == 0)
                        {
                            if (first)
                            {
                                first = false;
                                i = 0;
                                bi = 32;
                                continue;
                            }
                            return default(TValue);
                        }
                    }
                    else
                        return values[i];
                }
            }
            set
            {
                bool @default = object.Equals(value, default(TValue));
                if (navyData.Length <= allocatedCount)
                    ensureCapacity(navyData.Length * 2);
                if (allocatedCount == 0)
                {
                    allocatedCount = 1;
                    pseudoLength = 1;
                }
                uint _index = (uint)index;
                int bi = 31;
                for (uint i = 0, ni = 0; ; bi--)
                {
                    if (navyData[i].index != _index)
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
                            navyData[i].index = _index;
                            var ov = values[i];
                            values[i] = value;
                            this[(int)oi] = ov;
                            return;
                        }
                        else
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
                                navyData[ni].index = _index;
                                navyData[ni].bitIndex = bi - 1;
                                values[ni] = value;
                                return;
                            }
                            i = ni;
                        }
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

        private void ensureCapacity(int p)
        {
            var newData = new _Item[Math.Max(2, p)];
            for (var i = 0; i < navyData.Length; i++)
                newData[i] = navyData[i];
            navyData = newData;
            var newValues = new TValue[newData.Length];
            if (values != null)
                for (var i = 0; i < values.Length; i++)
                    newValues[i] = values[i];
            values = newValues;
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
                navyData[(int)(--allocatedCount)] = default(_Item);
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
            for (var i = Math.Min(pseudoLength, int.MaxValue) + arrayIndex; i-- > arrayIndex; )
                array[i] = default(TValue);
            foreach (var v in (this as IEnumerable<KeyValuePair<int, TValue>>))
                if (v.Key >= 0)
                    array[v.Key + arrayIndex] = v.Value;
        }

        public int Count
        {
            get { return (int)allocatedCount; }
        }

        bool ICollection<TValue>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Члены IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            if (navyData.Length == 0)
                yield break;
            var stack = new uint[33];
            var stepstack = new byte[33];
            int stackIndex = 0;
            var node = 0u;
            for (; ; )
            {
                if ((stepstack[stackIndex] & 4) == 0)
                {
                    stepstack[stackIndex] |= 4;
                    yield return values[node];
                }
                if ((stepstack[stackIndex] & 1) == 0)
                {
                    if (navyData[node].zeroContinue != 0)
                    {
                        stepstack[stackIndex] |= 1;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = navyData[node].zeroContinue;
                        continue;
                    }
                }
                if ((stepstack[stackIndex] & 2) == 0)
                {
                    if (navyData[node].oneContinue != 0)
                    {
                        stepstack[stackIndex] |= 2;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = navyData[node].oneContinue;
                        continue;
                    }
                }
                if (stackIndex == 0)
                    yield break;
                node = stack[--stackIndex];
            }
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<int,TValue>>

        IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
        {
            if (navyData.Length == 0)
                yield break;
            var stack = new uint[33];
            var stepstack = new byte[33];
            int stackIndex = 0;
            var node = 0u;
            for (; ; )
            {
                if ((stepstack[stackIndex] & 4) == 0)
                {
                    stepstack[stackIndex] |= 4;
                    yield return new KeyValuePair<int, TValue>((int)navyData[node].index, values[node]);
                }
                if ((stepstack[stackIndex] & 1) == 0)
                {
                    if (navyData[node].zeroContinue != 0)
                    {
                        stepstack[stackIndex] |= 1;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = navyData[node].zeroContinue;
                        continue;
                    }
                }
                if ((stepstack[stackIndex] & 2) == 0)
                {
                    if (navyData[node].oneContinue != 0)
                    {
                        stepstack[stackIndex] |= 2;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = navyData[node].oneContinue;
                        continue;
                    }
                }
                if (stackIndex == 0)
                    yield break;
                node = stack[--stackIndex];
            }
        }

        #endregion

        public IEnumerable<KeyValuePair<int, TValue>> Reversed
        {
            get
            {
                if (navyData.Length == 0)
                    yield break;
                var stack = new uint[33];
                var stepstack = new byte[33];
                int stackIndex = 0;
                var node = 0u;
                for (; ; )
                {
                    if ((stepstack[stackIndex] & 1) == 0)
                    {
                        if (navyData[node].oneContinue != 0)
                        {
                            stepstack[stackIndex] |= 1;
                            stack[stackIndex++] = node;
                            stepstack[stackIndex] = 0;
                            node = navyData[node].oneContinue;
                            continue;
                        }
                    }
                    if ((stepstack[stackIndex] & 2) == 0)
                    {
                        if (navyData[node].zeroContinue != 0)
                        {
                            stepstack[stackIndex] |= 2;
                            stack[stackIndex++] = node;
                            stepstack[stackIndex] = 0;
                            node = navyData[node].zeroContinue;
                            continue;
                        }
                    }
                    if ((stepstack[stackIndex] & 4) == 0)
                    {
                        stepstack[stackIndex] |= 4;
                        yield return new KeyValuePair<int, TValue>((int)navyData[node].index, values[node]);
                    }
                    if (stackIndex == 0)
                        yield break;
                    node = stack[--stackIndex];
                }
            }
        }
    }
}
