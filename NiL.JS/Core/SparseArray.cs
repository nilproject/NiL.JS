using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NiL.JS.Core
{
    public enum ArrayMode
    {
        Flat,
        Sparse
    }

    public sealed class SparseArray<TValue> : IList<TValue>, IDictionary<int, TValue>
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

        private readonly bool _isClass = typeof(TValue).IsClass;
        private const int _flatSizeLimit = 32;
        private static readonly TValue[] _emptyData = new TValue[0];

        private ArrayMode _mode;
        private uint _pseudoLength;
        private uint _usedCount;
        private _NavyItem[] _navyData;
        private TValue[] _values;
        private bool _zeroExists;

        public ArrayMode Mode
        {
            get
            {
                return _mode;
            }
        }

        [CLSCompliant(false)]
        public uint Length
        {
            get
            {
                return _pseudoLength;
            }
        }

        public SparseArray(ArrayMode arrayMode = ArrayMode.Flat)
        {
            _mode = arrayMode;
            _values = _emptyData;
            _navyData = Array.Empty<_NavyItem>();
        }

        public SparseArray(int capacity)
        {
            _mode = ArrayMode.Flat;
            _values = _emptyData;
            _navyData = Array.Empty<_NavyItem>();
            if (capacity > 0)
                EnsureCapacity(capacity);
        }

        public SparseArray(TValue[] values)
        {
            _mode = ArrayMode.Flat;
            this._values = values;
            _navyData = Array.Empty<_NavyItem>();
            _usedCount = (_pseudoLength = (uint)values.Length);
        }

        #region Члены IList<TValue>

        public int IndexOf(TValue item)
        {
            for (var i = 0; i < _usedCount; i++)
            {
                if (object.Equals(_values[i], item))
                {
                    if (_mode == ArrayMode.Flat)
                        return i;
                    return (int)_navyData[i].index;
                }
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
            if (_pseudoLength == 0 || index != (int)(_pseudoLength - 1))
                throw new InvalidOperationException();

            this[(int)(_pseudoLength - 1)] = default(TValue);
            _pseudoLength--;
        }

        public ref TValue GetExistent(int index)
        {
            uint unsignedIndex = (uint)index;

            if (_mode == ArrayMode.Flat)
            {
                if (index >= 0 && _pseudoLength > index && _values.Length > index)
                    return ref _values[index];

                if (index >= 0 && (index < _flatSizeLimit || index == _pseudoLength))
                {
                    EnsureCapacity(index + 1);
                    if (unsignedIndex >= _pseudoLength)
                        _pseudoLength = unsignedIndex + 1;

                    return ref _values[index];
                }

                RebuildToSparse();
            }

            if (unsignedIndex < _usedCount)
            {
                if (_navyData[index].index == unsignedIndex)
                    return ref _values[index];
            }

            if (_usedCount == 0)
            {
                EnsureCapacity(1);
                _usedCount = 1;
            }

            var log = NumberUtils.IntLog(unsignedIndex);
            var navyIndex = unsignedIndex << 31 - log;
            navyIndex <<= 1;
            uint i = 0;
            uint ni;

            while (true)
            {
                ref var navyItem = ref _navyData[i];
                if (navyItem.index > unsignedIndex)
                {
                    var oldIndex = navyItem.index;
                    var oldValue = _values[i];

                    navyItem.index = unsignedIndex;

                    if (oldIndex < _pseudoLength)
                        this[(int)oldIndex] = oldValue;

                    _values[i] = default;

                    return ref _values[i];
                }
                else if (navyItem.index < unsignedIndex)
                {
                    var b = (navyIndex & (1 << 31)) == 0;
                    ni = b ? navyItem.zeroContinue : navyItem.oneContinue;
                    if (ni == 0)
                    {
                        if (_pseudoLength <= unsignedIndex)
                            _pseudoLength = unsignedIndex + 1;

                        if (b)
                            navyItem.zeroContinue = ni = _usedCount++;
                        else
                            navyItem.oneContinue = ni = _usedCount++;

                        if (_navyData.Length <= _usedCount)
                            EnsureCapacity(_navyData.Length * 2);

                        _navyData[ni].index = unsignedIndex;
                        return ref _values[ni];
                    }

                    i = ni;
                }
                else
                {
                    if (_pseudoLength <= index)
                        _pseudoLength = unsignedIndex + 1;

                    if (_usedCount <= i)
                        _usedCount = i + 1;

                    return ref _values[i];
                }

                navyIndex <<= 1;
            }
        }

        public TValue this[int index]
        {
            get
            {
                if (_mode == ArrayMode.Flat)
                {
                    if (index < 0 || _pseudoLength <= index || _values.Length <= index)
                        return default(TValue);

                    return _values[index];
                }

                if (_navyData.Length == 0)
                    return default(TValue);

                uint unsignedIndex = (uint)index;

                if (unsignedIndex < _usedCount)
                {
                    if (_navyData[index].index == unsignedIndex)
                        return _values[index];
                }

                var log = NumberUtils.IntLog(unsignedIndex);
                var navyIndex = unsignedIndex << 31 - log;
                navyIndex <<= 1;
                uint i = 0;

                while (true)
                {
                    i = (navyIndex >> 31) == 0
                        ? _navyData[i].zeroContinue
                        : _navyData[i].oneContinue;

                    if (i == 0 || _navyData[i].index > unsignedIndex)
                    {
                        return default(TValue);
                    }
                    else if (_navyData[i].index == unsignedIndex)
                    {
                        return _values[i];
                    }

                    navyIndex <<= 1;
                }
            }
            set
            {
                bool isDefault = value is null; // структуры мы будем записывать, иначе пришлось бы вызывать тяжелые операции сравнения.

                uint unsignedIndex = (uint)index;
                if (_mode == ArrayMode.Flat)
                {
                    if (index < 0 || index > _pseudoLength)
                    {
                        if (isDefault)
                        {
                            if (unsignedIndex >= _pseudoLength)
                                _pseudoLength = unsignedIndex + 1;

                            return;
                        }

                        if (unsignedIndex < _flatSizeLimit || unsignedIndex == _pseudoLength)
                        {
                            // Покрывает много тех случаев, когда относительно маленький массив заполняют с конца.
                            // Кто-то верит, что это должно работать быстрее.
                            // Вот именно из-за таких кусков кода так и может показаться.
                            // Не время для попыток исправить мир
                            _pseudoLength = unsignedIndex + 1;
                            EnsureCapacity((int)_pseudoLength);
                            this[index] = value;
                            return;
                        }
                        else
                            RebuildToSparse();
                    }
                    else
                    {
                        if (_values.Length <= index)
                            EnsureCapacity(index + 1);

                        if (_pseudoLength == index)
                            _pseudoLength = unsignedIndex + 1;
                        
                        _values[index] = value;
                        return;
                    }
                }

                if (_usedCount == 0)
                {
                    EnsureCapacity(1);
                    _usedCount = 1;
                }

                if (unsignedIndex < _usedCount)
                {
                    if (_navyData[index].index == unsignedIndex)
                    {
                        if (index == 0)
                            _zeroExists = true;

                        if (_pseudoLength <= index)
                            _pseudoLength = unsignedIndex + 1;

                        _values[index] = value;

                        return;
                    }
                }

                var log = NumberUtils.IntLog(unsignedIndex);
                var navyIndex = unsignedIndex << 31 - log;
                navyIndex <<= 1;

                var i = 0u;
                var ni = 0u;
                while (true)
                {
                    if (_navyData[i].index > unsignedIndex)
                    {
                        if (isDefault)
                        {
                            if (_pseudoLength <= unsignedIndex)
                                _pseudoLength = unsignedIndex + 1; // длина может быть меньше
                            // уже записанных элементов если эти элементы имеют значение
                            // по-умолчанию и был вызван Trim
                            return;
                        }

                        var oi = _navyData[i].index;
                        var ov = _values[i];
                        _navyData[i].index = unsignedIndex;
                        _values[i] = value;

                        if (oi >= _pseudoLength)
                            return;

                        value = ov;
                        unsignedIndex = oi;

                        i = 0;
                        ni = 0;
                        log = NumberUtils.IntLog(unsignedIndex);
                        navyIndex = unsignedIndex << 31 - log;
                        navyIndex <<= 1;

                        continue;
                    }
                    else if (_navyData[i].index < unsignedIndex)
                    {
                        var b = (navyIndex >> 31) == 0;
                        ref var navyItem = ref _navyData[i];
                        ni = b ? navyItem.zeroContinue : navyItem.oneContinue;
                        if (ni == 0)
                        {
                            if (_pseudoLength <= unsignedIndex)
                                _pseudoLength = unsignedIndex + 1;

                            if (isDefault)
                                return;

                            if (b)
                                navyItem.zeroContinue = ni = _usedCount++;
                            else
                                navyItem.oneContinue = ni = _usedCount++;

                            if (_navyData.Length <= _usedCount)
                                EnsureCapacity(_navyData.Length * 2);

                            _navyData[ni].index = unsignedIndex;
                            _values[ni] = value;
                            return;
                        }

                        i = ni;
                    }
                    else
                    {
                        _values[i] = value;

                        if (_pseudoLength <= index)
                            _pseudoLength = unsignedIndex + 1;

                        if (_usedCount <= i)
                            _usedCount = i + 1;

                        return;
                    }

                    navyIndex <<= 1;
                }
            }
        }

        #endregion

        #region Члены ICollection<TValue>

        public void Add(TValue item)
        {
            if (_pseudoLength == uint.MaxValue)
                throw new InvalidOperationException();

            this[(int)(_pseudoLength)] = item;
        }

        public void Clear()
        {
            while (_usedCount > 0)
            {
                _navyData[(int)(--_usedCount)] = default(_NavyItem);
                _values[(int)_usedCount] = default(TValue);
            }
            _pseudoLength = 0;
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
            if (Math.Min(_pseudoLength, int.MaxValue) - arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException();

            for (var i = Math.Min(_pseudoLength, int.MaxValue) + arrayIndex; i-- > arrayIndex;)
            {
                array[i] = default(TValue);
            }

            foreach (var v in DirectOrder)
            {
                if (v.Key >= 0)
                    array[v.Key + arrayIndex] = v.Value;
            }
        }

        int ICollection<TValue>.Count
        {
            get { return (int)_pseudoLength; }
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
            for (var i = 0u; i < _pseudoLength; i++)
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
            if (_pseudoLength < index)
                return -1;

            return findNearest(index, true);
        }

        private long findNearest(long index, bool notLess)
        {
            var log = NumberUtils.IntLog(index);
            var navyIndex = (uint)(index << 31 - log);
            navyIndex <<= 1;
            var mask = 1u << 31;

            long nearestNext = notLess ? (long)uint.MaxValue + 1 : -1;
            long i = 0;
            while (true)
            {
                if (_navyData[i].index == nearestNext)
                    return _navyData[i].index;

                var zeroNext = _navyData[_navyData[i].zeroContinue];
                var oneNext = _navyData[_navyData[i].oneContinue];

                if (zeroNext.index == nearestNext)
                    return zeroNext.index;

                if (oneNext.index == nearestNext)
                    return oneNext.index;

                if ((navyIndex & mask) == 0 && zeroNext.index != 0 && zeroNext.index <= index)
                {
                    i = _navyData[i].zeroContinue;
                }
                else
                {
                    if (_navyData[i].oneContinue == 0 || oneNext.index > index)
                    {
                        if (_navyData[i].index == index)
                            return _navyData[i].index;

                        mask <<= 1;
                        if (notLess)
                        {
                            if (zeroNext.index > index && nearestNext > zeroNext.index)
                                nearestNext = zeroNext.index;

                            if (oneNext.index > index && nearestNext > oneNext.index)
                                nearestNext = oneNext.index;

                            if (_navyData[i].index > index && nearestNext > _navyData[i].index)
                                nearestNext = _navyData[i].index;

                            navyIndex = (uint)(((int)navyIndex & ~(mask - 1)) + mask);
                        }
                        else
                        {
                            if (zeroNext.index < index && nearestNext < zeroNext.index)
                                nearestNext = zeroNext.index;

                            if (oneNext.index < index && nearestNext < oneNext.index)
                                nearestNext = oneNext.index;

                            if (_navyData[i].index < index && nearestNext < _navyData[i].index)
                                nearestNext = _navyData[i].index;

                            navyIndex = (uint)(((uint)navyIndex | (mask - 1)) - mask);
                        }

                        i = 0;
                        mask = 1u << 31;
                        continue;
                    }

                    i = _navyData[i].oneContinue;
                }

                mask >>= 1;

                if (i == 0)
                    return -1;
            }
        }

        public long NearestIndexNotMore(long index)
        {
            return findNearest(index, false);
        }

        public IEnumerable<int> KeysDirectOrder
        {
            get
            {
                var index = 1L;
                var skipFirst = !_zeroExists;
                if (_mode == ArrayMode.Flat)
                {
                    for (var i = 0; i < _pseudoLength; i++)
                    {
                        skipFirst = true;
                        if (i >= _values.Length) // была насильно установлена длина фиктивным элементом.
                        {
                            yield return (int)(_pseudoLength - 1);
                            yield break;
                        }

                        yield return i;

                        if (_mode != ArrayMode.Flat)
                        {
                            index = (uint)(i + 1);
                            break;
                        }
                    }
                }

                if (_mode == ArrayMode.Sparse) // Режим может поменяться во время итерации в режиме Flat
                {
                    if (_usedCount > 0)
                    {
                        if (!skipFirst)
                        {
                            yield return 0;
                        }
                    }
                    else
                    {
                        yield return (int)(_pseudoLength - 1);
                        yield break;
                    }

                    while (index < _pseudoLength)
                    {
                        index = NearestIndexNotLess(index);

                        if (index < 0)
                            break;

                        if (index > 0)
                            yield return (int)index;

                        index++;
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<int, TValue>> DirectOrder => KeysDirectOrder.Select(x => new KeyValuePair<int, TValue>(x, this[x]));

        public IEnumerable<int> KeysReversOrder
        {
            get
            {
                long index = _pseudoLength - 1;
                if (_mode == ArrayMode.Flat)
                {
                    if (_pseudoLength > _values.Length)
                        yield return (int)(_pseudoLength - 1);

                    for (var i = Math.Min(_values.Length, _pseudoLength); i-- > 0;)
                    {
                        if (_mode != ArrayMode.Flat)
                        {
                            index = i;
                            break;
                        }
                        yield return (int)i;
                    }
                }
                if (_mode == ArrayMode.Sparse)
                {
                    if (_usedCount == 0)
                        yield break;

                    while (index > 0)
                    {
                        index = NearestIndexNotMore(index);

                        if (index < 0)
                            break;

                        if (index > 0)
                            yield return (int)index;

                        index--;
                    }

                    if (_zeroExists)
                        yield return 0;
                }
            }
        }

        public IEnumerable<KeyValuePair<int, TValue>> ReversOrder => KeysReversOrder.Select(x => new KeyValuePair<int, TValue>(x, this[x]));

        public ICollection<int> Keys => this.KeysDirectOrder.ToList();

        public ICollection<TValue> Values => this.DirectOrder.Select(x => x.Value).ToList();

        public int Count => Tools.JSObjectToInt32(Length);

        /// <summary>
        /// Reduce length to "index of last item with non-default value" + 1
        /// </summary>
        public void Trim()
        {
            long len = -1;
            if (_mode == ArrayMode.Flat)
            {
                for (var i = _values.Length; i-- > 0;)
                {
                    if (!object.Equals(_values[i], default(TValue)))
                    {
                        len = i;
                        break;
                    }
                }
            }
            else
            {
                for (var i = _usedCount; i-- > 0;)
                {
                    if (_navyData[i].index > len && !object.Equals(_values[i], default(TValue)))
                        len = _navyData[i].index;
                }
            }

            _pseudoLength = (uint)(len + 1);
        }

        public void EnsureCapacity(int capacity)
        {
            capacity = Math.Max(4, capacity);
            if (_values.Length >= capacity)
                return;

            var newCapacity = 1 << NumberUtils.IntLog(capacity);
            if (newCapacity < capacity)
                newCapacity <<= 1;

            var newValues = new TValue[newCapacity];
            for (var i = 0; i < _values.Length; i++)
                newValues[i] = _values[i];

            _values = newValues;

            if (_mode == ArrayMode.Sparse)
            {
                var newData = new _NavyItem[newCapacity];
                for (var i = 0; i < _navyData.Length; i++)
                    newData[i] = _navyData[i];
                _navyData = newData;
            }
        }

        public void RebuildToSparse()
        {
            _usedCount = 0;
            _mode = ArrayMode.Sparse;
            var len = _pseudoLength;
            if (len == 0)
            {
                EnsureCapacity(0);
                return;
            }

            _navyData = new _NavyItem[_values.Length];
            var data = _values;
            _values = new TValue[_values.Length];
            len = (uint)Math.Min(data.Length, len);
            for (var i = 0; i < len; i++)
                this[i] = data[i];

            if (_values.Length < len)
                this[(int)len - 1] = default(TValue);
        }

        public void RebuildToFlat()
        {
            var newValues = new TValue[_pseudoLength];
            for (var i = 0; i < _pseudoLength; i++)
                newValues[i] = this[i];

            _mode = ArrayMode.Flat;
            _values = newValues;
            _navyData = Array.Empty<_NavyItem>();
        }

        public void Add(int key, TValue value)
        {
            if (NearestIndexNotLess(key) == key)
                throw new InvalidOperationException();

            this[key] = value;
        }

        public bool ContainsKey(int key)
        {
            return NearestIndexNotLess(key) == key;
        }

        public bool Remove(int key)
        {
            if (key >= _pseudoLength)
                return false;

            if (key < _pseudoLength)
                this[key] = default(TValue);

            if (key == _pseudoLength - 1)
                _pseudoLength--;

            return true;
        }

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out TValue value)
        {
            if (_pseudoLength <= key)
            {
                value = default;
                return false;
            }

            value = this[key];
            return true;
        }

        public void Add(KeyValuePair<int, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<int, TValue> item)
        {
            return Equals(this[item.Key], item.Value);
        }

        public void CopyTo(KeyValuePair<int, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, TValue> item)
        {
            if (Contains(item))
                return Remove(item.Key);

            return false;
        }

        IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
        {
            foreach (var item in this.DirectOrder)
                yield return item;
        }
    }
}
