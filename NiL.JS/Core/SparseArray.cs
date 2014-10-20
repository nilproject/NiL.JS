using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            public TValue value;

            public override string ToString()
            {
                return "[" + index + ", " + value + "]";
            }
        }

        private static readonly _Item[] emptyData = new _Item[0]; // data dummy. In cases where instance of current class was created, but not used

        private _Item[] data;
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
            data = emptyData;
        }

        public SparseArray(int capacity)
        {
            data = new _Item[capacity];
        }

        #region Члены IList<TValue>

        public int IndexOf(TValue item)
        {
            for (var i = 0; i < allocatedCount; i++)
                if (object.Equals(data[i].value, item))
                    return (int)data[i].index;
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
            this[(int)(--pseudoLength)] = default(TValue);
        }

        /// <summary>
        /// Reduce length to "index of last item with non-default value" + 1
        /// </summary>
        public void Trim()
        {
            long len = -1;
            for (var i = 0; i < allocatedCount; i++)
            {
                if (data[i].index > len && !object.Equals(data[i].value, default(TValue)))
                    len = data[i].index;
            }
            pseudoLength = (uint)(len + 1);
        }

        public TValue this[int index]
        {
            get
            {
                if (data.Length == 0)
                    return default(TValue);
                uint _index = (uint)index;
                bool first = true;
                int bi = 31;
                for (uint i = 0; ; bi--)
                {
                    if (first && i == 0 && _index < allocatedCount)
                    {
                        bi = data[(int)_index].bitIndex;
                        i = (uint)(-1 ^ ((1 << bi) - 1));
                        if ((data[_index].index & i) != (_index & i))
                        {
                            bi = 31;
                            i = 0;
                        }
                        else
                            i = _index;
                    }
                    if (data[i].index != _index)
                    {
                        i = (_index & (1 << bi)) == 0 ? data[i].zeroContinue : data[i].oneContinue;
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
                        return data[i].value;
                }
            }
            set
            {
                bool @default = object.Equals(value, default(TValue));
                if (data.Length <= allocatedCount)
                    ensureCapacity(data.Length * 2);
                if (allocatedCount == 0)
                {
                    allocatedCount = 1;
                    pseudoLength = 1;
                }
                uint _index = (uint)index;
                int bi = 31;
                for (uint i = 0, ni = 0; ; bi--)
                {
                    if (data[i].index != _index)
                    {
                        if (data[i].index > _index)
                        {
                            if (@default)
                                return;
                            var oi = data[i].index;
                            var ov = data[i].value;
                            data[i].index = _index;
                            data[i].value = value;
                            this[(int)oi] = ov;
                            return;
                        }
                        else
                        {
                            var b = (_index & (1 << bi)) == 0;
                            ni = b ? data[i].zeroContinue : data[i].oneContinue;
                            if (ni == 0)
                            {
                                if (pseudoLength <= _index)
                                    pseudoLength = _index + 1;
                                if (@default)
                                    return;
                                if (b)
                                    data[i].zeroContinue = ni = allocatedCount++;
                                else
                                    data[i].oneContinue = ni = allocatedCount++;
                                data[ni].index = _index;
                                data[ni].value = value;
                                data[ni].bitIndex = bi - 1;
                                return;
                            }
                            i = ni;
                        }
                    }
                    else
                    {
                        data[i].value = value;
                        return;
                    }
                }
            }
        }

        private void ensureCapacity(int p)
        {
            var newData = new _Item[Math.Max(2, p)];
            for (var i = 0; i < data.Length; i++)
                newData[i] = data[i];
            data = newData;
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
                data[(int)(--allocatedCount)] = default(_Item);
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
            if (data.Length == 0)
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
                    yield return data[node].value;
                }
                if ((stepstack[stackIndex] & 1) == 0)
                {
                    if (data[node].zeroContinue != 0)
                    {
                        stepstack[stackIndex] |= 1;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = data[node].zeroContinue;
                        continue;
                    }
                }
                if ((stepstack[stackIndex] & 2) == 0)
                {
                    if (data[node].oneContinue != 0)
                    {
                        stepstack[stackIndex] |= 2;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = data[node].oneContinue;
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
            if (data.Length == 0)
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
                    yield return new KeyValuePair<int, TValue>((int)data[node].index, data[node].value);
                }
                if ((stepstack[stackIndex] & 1) == 0)
                {
                    if (data[node].zeroContinue != 0)
                    {
                        stepstack[stackIndex] |= 1;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = data[node].zeroContinue;
                        continue;
                    }
                }
                if ((stepstack[stackIndex] & 2) == 0)
                {
                    if (data[node].oneContinue != 0)
                    {
                        stepstack[stackIndex] |= 2;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = data[node].oneContinue;
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
                if (data.Length == 0)
                    yield break;
                var stack = new uint[33];
                var stepstack = new byte[33];
                int stackIndex = 0;
                var node = 0u;
                for (; ; )
                {
                    if ((stepstack[stackIndex] & 1) == 0)
                    {
                        if (data[node].oneContinue != 0)
                        {
                            stepstack[stackIndex] |= 1;
                            stack[stackIndex++] = node;
                            stepstack[stackIndex] = 0;
                            node = data[node].oneContinue;
                            continue;
                        }
                    }
                    if ((stepstack[stackIndex] & 2) == 0)
                    {
                        if (data[node].zeroContinue != 0)
                        {
                            stepstack[stackIndex] |= 2;
                            stack[stackIndex++] = node;
                            stepstack[stackIndex] = 0;
                            node = data[node].zeroContinue;
                            continue;
                        }
                    }
                    if ((stepstack[stackIndex] & 4) == 0)
                    {
                        stepstack[stackIndex] |= 4;
                        yield return new KeyValuePair<int, TValue>((int)data[node].index, data[node].value);
                    }
                    if (stackIndex == 0)
                        yield break;
                    node = stack[--stackIndex];
                }
            }
        }
    }
}
