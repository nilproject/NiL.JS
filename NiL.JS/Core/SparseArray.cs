using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace NiL.JS.Core;

public sealed class SparseArray<TValue> : IList<TValue>, IDictionary<int, TValue>
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NavyItem
    {
        public uint index;
        public uint zeroContinue;
        public uint oneContinue;

        public override string ToString()
        {
            return index + "[" + zeroContinue + ";" + oneContinue + "]";
        }
    }

    private static TValue _fictive;

    private uint _pseudoLength;
    private NavyItem[][] _navyData;
    private TValue[][] _values;
    private uint[] _used;

    [CLSCompliant(false)]
    public uint Length => _pseudoLength;

    public SparseArray()
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<uint>();
    }

    public SparseArray(TValue[] values)
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<uint>();

        for (var i = 0; i < values.Length; i++)
            this[i] = values[i];
    }

    #region Члены IList<TValue>

    public int IndexOf(TValue item)
    {
        for (var i = 0; i < _pseudoLength; i++)
        {
            if (Equals(this[i], item))
                return i;
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

        this[(int)(_pseudoLength - 1)] = default!;
        _pseudoLength--;
    }

    public ref TValue GetExistent(int index)
    {
        return ref getInternal(index, false, out _);
    }

    private ref TValue getInternal(int index, bool forRead, out bool got)
    {
        uint unsignedIndex = (uint)index;

        var log = NumberUtils.IntLog(unsignedIndex);

        if (_navyData.Length <= log)
        {
            if (forRead)
            {
                got = false;
                return ref _fictive;
            }

            Array.Resize(ref _navyData, log + 1);
            Array.Resize(ref _values, log + 1);
            Array.Resize(ref _used, log + 1);
        }

        if (_navyData[log] == null)
        {
            if (forRead)
            {
                got = false;
                return ref _fictive;
            }

            if (log <= 4)
            {
                _navyData[log] = Array.Empty<NavyItem>();
                _values[log] = new TValue[1 << Math.Max(1, log)];
            }
            else
            {
                _navyData[log] = new NavyItem[2];
                _values[log] = new TValue[2];
            }
        }

        if (log <= 4)
        {
            var itemIndex = index & ((1 << Math.Max(1, log)) - 1);
            _used[log] = (uint)Math.Max(_used[log], itemIndex + 1);
            got = true;

            if (!forRead && _pseudoLength <= unsignedIndex)
                _pseudoLength = unsignedIndex + 1;

            return ref _values[log][itemIndex];
        }

        var navy = _navyData[log];
        var values = _values[log];

        var mask = 1 << (log - 1);
        uint i = 0;
        uint ni;
        while (true)
        {
            ref var navyItem = ref navy[i];
            if (navyItem.index > unsignedIndex)
            {
                if (forRead)
                {
                    got = false;
                    return ref _fictive;
                }

                var oldIndex = navyItem.index;
                var oldValue = values[i];

                navyItem.index = unsignedIndex;

                if (oldIndex < _pseudoLength)
                    this[(int)oldIndex] = oldValue!;

                values = _values[log];
                values[i] = default!;

                got = true;
                return ref values[i];
            }
            else if (navyItem.index < unsignedIndex)
            {
                var b = (index & mask) == 0;
                ni = b ? navyItem.zeroContinue : navyItem.oneContinue;
                if (ni == 0)
                {
                    if (forRead)
                    {
                        got = false;
                        return ref _fictive;
                    }

                    if (_pseudoLength <= unsignedIndex)
                        _pseudoLength = unsignedIndex + 1;

                    if (b)
                        navyItem.zeroContinue = ni = _used[log]++;
                    else
                        navyItem.oneContinue = ni = _used[log]++;

                    if (navy.Length < _used[log])
                    {
                        var newSize = _navyData[log].Length * 2;
                        Array.Resize(ref _navyData[log], newSize);
                        Array.Resize(ref _values[log], newSize);

                        navy = _navyData[log];
                        values = _values[log];
                    }

                    navy[ni].index = unsignedIndex;
                    got = true;
                    return ref values[ni];
                }

                i = ni;
                mask >>= 1;
            }
            else
            {
                if (_pseudoLength <= index)
                    _pseudoLength = unsignedIndex + 1;

                got = true;
                return ref values[i];
            }
        }
    }

    public TValue this[int index]
    {
        get
        {
            var r = getInternal(index, true, out var got);
            return got ? r! : default!;
        }
        set
        {
            bool isDefault = value is null; // структуры мы будем записывать, иначе пришлось бы вызывать тяжелые операции сравнения.

            if (isDefault)
            {
                ref var maybeExists = ref getInternal(index, true, out var got);
                if (got)
                    maybeExists = default;

                if (_pseudoLength <= (uint)index)
                {
                    _pseudoLength = ((uint)index + 1);
                }
            }
            else
                GetExistent(index) = value;
        }
    }

    #endregion

    #region Члены ICollection<TValue>

    public void Add(TValue item)
    {
        if (_pseudoLength == uint.MaxValue)
            throw new InvalidOperationException();

        this[(int)_pseudoLength++] = item;
    }

    public void TrimLength()
    {
        _pseudoLength = (uint)findNearest(_pseudoLength - 1, false) + 1;
    }

    public void Clear()
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<uint>();
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
            array[i] = default;
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
        uint unsignedIndex = (uint)index;

        var log = NumberUtils.IntLog(unsignedIndex);

        if (_navyData.Length <= log)
        {
            if (notLess)
                return -1;

            log = _navyData.Length - 1;
            index = (1 << (log + 1)) - 1;
        }

        while (log >= 0 && log < _navyData.Length && _navyData[log] == null)
        {
            if (notLess)
                log++;
            else
                log--;
        }

        if (log < 0 || log > _navyData.Length)
            return -1;

        if (log <= 4)
        {
            if (notLess)
            {
                if (log == 0)
                {
                    if (index > 1)
                        return -1;

                    return index;
                }

                return Math.Max(index, 1 << log);
            }
            else
            {
                return Math.Min(index, (2 << log) - 1);
            }
        }

        var navy = _navyData[log];

        var oneContinueIndex = 0u;
        var zeroContinueIndex = 0u;

        var mask = 1 << (log - 1);
        uint i = 0;
        uint ni;
        while (true)
        {
            ref var navyItem = ref navy[i];
            if (navyItem.index > unsignedIndex)
            {
                if (notLess)
                {
                    return navyItem.index;
                }
                else
                {
                    log--;
                    while (log > 0 && _navyData[log] is null)
                    {
                        log--;
                    }

                    if (log < 0 || _navyData[log] is null)
                    {
                        return -1;
                    }

                    navy = _navyData[log];

                    if (navy.Length == 0)
                    {
                        return (1 << (log + 1)) - 1;
                    }

                    i = 0;
                    mask = int.MaxValue;
                    continue;
                }
            }
            else if (navyItem.index < unsignedIndex)
            {
                var isZero = (index & mask) == 0;

                if (isZero)
                {
                    if (navyItem.oneContinue != 0)
                        oneContinueIndex = navyItem.oneContinue;
                }
                else
                {
                    if (navyItem.zeroContinue != 0)
                        zeroContinueIndex = navyItem.zeroContinue;
                }

                ni = isZero ? navyItem.zeroContinue : navyItem.oneContinue;

                if (ni == 0)
                {
                    if (notLess && oneContinueIndex != 0)
                    {
                        mask = 0;
                        ni = oneContinueIndex;
                        oneContinueIndex = 0;
                    }
                    else if (!notLess && zeroContinueIndex != 0)
                    {
                        mask = int.MaxValue;
                        ni = zeroContinueIndex;
                        zeroContinueIndex = 0;
                    }
                    else
                    {
                        if (notLess)
                        {
                            log++;
                            while (log < _navyData.Length && _navyData[log] is null)
                            {
                                log++;
                            }

                            if (log >= _navyData.Length || _navyData[log] is null)
                            {
                                return -1;
                            }

                            return _navyData[log][0].index;
                        }

                        return navyItem.index;
                    }
                }

                if (!notLess && navy[ni].index > index)
                    return navyItem.index;

                i = ni;
                mask >>= 1;
            }
            else
            {
                return index;
            }
        }
    }

    public long NearestIndexNotMore(long index)
    {
        return findNearest(index, false);
    }

    public IEnumerable<int> KeysForwardOrder
    {
        get
        {
            var index = 0L;

            while (index < _pseudoLength)
            {
                index = NearestIndexNotLess(index);

                if (index < 0)
                    break;

                yield return (int)index;

                index++;
            }
        }
    }

    public IEnumerable<KeyValuePair<int, TValue>> DirectOrder => KeysForwardOrder.Select(x => new KeyValuePair<int, TValue>(x, this[x]));

    public IEnumerable<KeyValuePair<int, TValue>> Unordered
    {
        get
        {
            for (var i = 0; i < _navyData.Length; i++)
            {
                for (var j = 0; j < _used.Length; j++)
                {
                    yield return new KeyValuePair<int, TValue>((int)_navyData[i][j].index, _values[i][j]!);
                }
            }
        }
    }

    public IEnumerable<int> KeysReverseOrder
    {
        get
        {
            long index = _pseudoLength - 1;

            while (index >= 0)
            {
                index = NearestIndexNotMore(index);

                if (index < 0)
                    break;

                yield return (int)index;

                index--;
            }
        }
    }

    public IEnumerable<KeyValuePair<int, TValue>> ReverseOrder => KeysReverseOrder.Select(x => new KeyValuePair<int, TValue>(x, this[x]));

    public ICollection<int> Keys => KeysForwardOrder.ToList();

    public ICollection<TValue> Values => DirectOrder.Select(x => x.Value).ToList();

    public int Count => (int)Length;

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
            this[key] = default;

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
        foreach (var item in DirectOrder)
            yield return item;
    }
}