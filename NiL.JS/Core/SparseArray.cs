using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NiL.JS.Statements;

namespace NiL.JS.Core;

public sealed class SparseArray<TValue> : IList<TValue>, IDictionary<int, TValue>, ICloneable
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NavyItem
    {
        public uint index;
        public short zeroContinue;
        public short oneContinue;

        public override string ToString()
        {
            return index + "[" + zeroContinue + ";" + oneContinue + "]";
        }
    }

    private const int SegmentSize = 8192;

    private static TValue _fictive;

    private uint _pseudoLength;
    private NavyItem[][] _navyData;
    private TValue[][] _values;
    private short[] _used;

    [CLSCompliant(false)]
    public uint Length => _pseudoLength;

    public SparseArray()
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<short>();
    }

    public SparseArray(TValue[] values)
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<short>();

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
        return ref TryGetInternalForWrite((uint)index, out _);
    }

    internal ref TValue TryGetInternalForRead(uint index, out bool got)
    {
        var segment = index / SegmentSize;

        if (_navyData.Length <= segment)
        {
            got = false;
            _fictive = default;
            return ref _fictive;
        }

        if (_navyData[segment].Length == 0)
        {
            var values = _values[segment];
            var itemIndex = index & (SegmentSize - 1);

            if (itemIndex >= values.Length)
            {
                got = false;
                _fictive = default;
                return ref _fictive;
            }

            got = true;
            return ref values[itemIndex];
        }

        return ref getFromTree(index, true, out got, segment);
    }

    internal ref TValue TryGetInternalForWrite(uint index, out bool got)
    {
        var segment = index / SegmentSize;

        if (_navyData.Length <= segment)
        {
            resizeL0(segment);
        }

        if (_navyData[segment].Length == 0)
        {
            var values = _values[segment];
            var itemIndex = (int)index & (SegmentSize - 1);

            if (itemIndex >= values.Length)
            {
                if (itemIndex <= values.Length + 4)
                {
                    Array.Resize(ref values, Math.Max(values.Length * 2, 8));

                    _values[segment] = values;
                }
                else
                {
                    rebuildSegmentToSparse(segment, values);

                    return ref getFromTree(index, false, out got, segment);
                }
            }

            if (_pseudoLength <= index)
                _pseudoLength = index + 1;

            got = true;
            return ref values[itemIndex];
        }

        return ref getFromTree(index, false, out got, segment);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void rebuildSegmentToSparse(uint segment, TValue[] values)
    {
        var oldValues = values;
        var len = oldValues.Length;

        if (oldValues.Length == 0)
            len = 8;
        else if (oldValues[oldValues.Length - 1] is not null)
            len *= 2;

        values = new TValue[len];
        _values[segment] = values;
        _navyData[segment] = new NavyItem[len];

        var bias = segment * SegmentSize;
        for (var valueIndex = 0; valueIndex < oldValues.Length; valueIndex++)
        {
            getFromTree((uint)(valueIndex + bias), false, out _, segment) = oldValues[valueIndex];
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void resizeL0(uint segment)
    {
        Array.Resize(ref _navyData, 2 << NumberUtils.IntLog(segment));
        Array.Resize(ref _values, _navyData.Length);
        Array.Resize(ref _used, _navyData.Length);

        for (var i = _navyData.Length - 1; i >= 0 && _navyData[i] is null; i--)
            _navyData[i] = Array.Empty<NavyItem>();

        for (var i = _values.Length - 1; i >= 0 && _values[i] is null; i--)
        {
            _values[i] = Array.Empty<TValue>();
        }
    }

    private (int segmentIndex, int itemIndex) findNearest(long index, bool notLess)
    {
        uint unsignedIndex = (uint)index;

        var segment = (int)(unsignedIndex / SegmentSize);

        if (_navyData.Length <= segment)
        {
            if (notLess)
            {
                return (-1, -1);
            }

            segment = _navyData.Length - 1;
            index = (segment + 1) * SegmentSize - 1;
        }

        while (segment >= 0
            && segment < _navyData.Length
            && (_values[segment].Length == 0))
        {
            if (notLess)
                segment++;
            else
                segment--;
        }

        if (segment < 0 || segment >= _navyData.Length)
        {
            return (-1, -1);
        }

        if (_navyData[segment].Length == 0)
        {
            var itemIndex = (int)index & (SegmentSize - 1);

            if (notLess)
            {
                if (_values[segment].Length > itemIndex)
                {
                    return (segment, itemIndex);
                }

                segment++;
                while (segment < _navyData.Length
                    && (_values[segment] is null || _values[segment].Length == 0))
                    segment++;

                if (segment >= _navyData.Length)
                {
                    return (-1, -1);
                }

                return (segment, 0);
            }
            else
            {
                if (_values[segment].Length < itemIndex)
                {
                    return (segment, _values[segment].Length - 1);
                }
            }

            return (segment, itemIndex);
        }

        var mask = SegmentSize >> 1;
        var navy = _navyData[segment];

        int i = (int)(unsignedIndex & (mask << 1) - 1);
        if (i < navy.Length && navy[i].index == unsignedIndex)
        {
            return (segment, i);
        }

        i = 0;

        short oneContinueIndex = 0;
        short zeroContinueIndex = 0;

        short ni;
        while (true)
        {
            ref var navyItem = ref navy[i];
            if (navyItem.index > unsignedIndex)
            {
                if (notLess)
                {
                    return (segment, i);
                }
                else
                {
                    segment--;
                    while (segment >= 0 && _values[segment].Length == 0)
                    {
                        segment--;
                    }

                    if (segment < 0 || _values[segment].Length == 0)
                    {
                        return (-1, -1);
                    }

                    navy = _navyData[segment];

                    if (navy.Length == 0)
                    {
                        return (segment, _values[segment].Length - 1);
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
                            segment++;
                            while (segment < _navyData.Length && _values[segment].Length == 0)
                            {
                                segment++;
                            }

                            if (segment >= _navyData.Length || _values[segment].Length == 0)
                            {
                                return (-1, -1);
                            }

                            return (segment, 0);
                        }

                        return (segment, i);
                    }
                }

                if (!notLess && navy[ni].index > index)
                {
                    return (segment, i);
                }

                i = ni;
                mask >>= 1;
            }
            else
            {
                return (segment, i);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref TValue getFromTree(uint index, bool forRead, out bool got, uint segment)
    {
        var mask = SegmentSize >> 1;
        var values = _values[segment];
        var navy = _navyData[segment];

        var i = (int)index & (SegmentSize - 1);
        if (i < navy.Length && navy[i].index == index)
        {
            if (i == 0 && _used[segment] == 0)
            {
                if (forRead)
                {
                    got = false;
                    _fictive = default;
                    return ref _fictive;
                }

                _used[segment] = 1;
            }

            got = true;
            return ref values[i];
        }

        i = 0;

        short ni;
        while (true)
        {
            ref var navyItem = ref navy[i];
            if (navyItem.index < index)
            {
                var isNotZero = (index & mask) != 0;
                ni = isNotZero ? navyItem.oneContinue : navyItem.zeroContinue;

                if (ni == 0)
                {
                    if (forRead)
                    {
                        got = false;
                        _fictive = default;
                        return ref _fictive;
                    }

                    if (_pseudoLength <= index)
                        _pseudoLength = index + 1;

                    ni = _used[segment]++;

                    if (isNotZero)
                        navyItem.oneContinue = ni;
                    else
                        navyItem.zeroContinue = ni;

                    if (ni >= navy.Length)
                    {
                        var newSize = ni * 2;
                        Array.Resize(ref _navyData[segment], newSize);
                        Array.Resize(ref _values[segment], newSize);

                        navy = _navyData[segment];
                        values = _values[segment];
                    }

                    navy[ni].index = index;
                    got = true;
                    return ref values[ni];
                }

                i = ni;
                mask >>= 1;
            }
            else if (navyItem.index > index)
            {
                if (forRead)
                {
                    got = false;
                    _fictive = default;
                    return ref _fictive;
                }

                var oldIndex = navyItem.index;
                var oldValue = values[i];

                navyItem.index = index;

                if (oldIndex < _pseudoLength)
                    this[(int)oldIndex] = oldValue!;

                values = _values[segment];
                values[i] = default!;

                got = true;
                return ref values[i];
            }
            else
            {
                if (!forRead && _pseudoLength <= index)
                    _pseudoLength = index + 1;

                got = true;
                return ref values[i];
            }
        }
    }

    public TValue this[int index]
    {
        get
        {
            ref var r = ref TryGetInternalForRead((uint)index, out var got);

            if (got)
                return r;

            return default;
        }
        set
        {
            bool isDefault = value is null; // структуры мы будем записывать, иначе пришлось бы вызывать тяжелые операции сравнения.

            if (isDefault)
            {
                if (_pseudoLength <= (uint)index)
                    _pseudoLength = (uint)index + 1;
                else
                {
                    ref var maybeExists = ref TryGetInternalForRead((uint)index, out var got);
                    if (got)
                        maybeExists = default;
                }
            }
            else
                TryGetInternalForWrite((uint)index, out _) = value;
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
        var coord = findNearest(_pseudoLength - 1, false);
        var externalIndex = getExternalIndex(coord);
        _pseudoLength = (uint)externalIndex + 1;
    }

    private long getExternalIndex((int segmentIndex, int itemIndex) coord)
    {
        if (coord.segmentIndex == -1)
            return -1;

        var navyItems = _navyData[coord.segmentIndex];
        if (navyItems.Length != 0)
            return navyItems[coord.itemIndex].index;

        var externalIndex = coord.segmentIndex * SegmentSize + (uint)coord.itemIndex;
        return externalIndex;
    }

    public void Clear()
    {
        _values = Array.Empty<TValue[]>();
        _navyData = Array.Empty<NavyItem[]>();
        _used = Array.Empty<short>();
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

        foreach (var v in ForwardOrder)
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
    public long NearestNotLess(long index, out TValue value)
    {
        if (_pseudoLength < index)
        {
            value = default;
            return -1;
        }

        var coord = findNearest(index, true);

        if (coord.itemIndex == -1)
        {
            value = default;
            return -1;
        }

        value = _values[coord.segmentIndex][coord.itemIndex];
        var externalIndex = getExternalIndex(coord);
        return externalIndex;
    }

    public long NearestNotMore(long index, out TValue value)
    {
        var coord = findNearest(index, false);

        if (coord.itemIndex == -1)
        {
            value = default;
            return -1;
        }

        value = _values[coord.segmentIndex][coord.itemIndex];
        var externalIndex = getExternalIndex(coord);
        return externalIndex;
    }

    public IEnumerable<int> KeysForwardOrder => ForwardOrder.Select(x => x.Key);

    public IEnumerable<KeyValuePair<int, TValue>> ForwardOrder
    {
        get
        {
            var index = 0L;

            while (index < _pseudoLength)
            {
                index = NearestNotLess(index, out var value);

                if (index < 0)
                    break;

                yield return new KeyValuePair<int, TValue>((int)index, value);

                index++;
            }

            if (index < _pseudoLength)
                yield return new KeyValuePair<int, TValue>((int)_pseudoLength - 1, default);
        }
    }

    public IEnumerable<KeyValuePair<int, TValue>> Unordered
    {
        get
        {
            for (var i = 0; i < _navyData.Length; i++)
            {
                if (_navyData[i].Length != 0)
                {
                    for (var j = 0; j < _used[i]; j++)
                    {
                        yield return new KeyValuePair<int, TValue>(_navyData[i].Length is 0 ? i * SegmentSize + j : (int)_navyData[i][j].index, _values[i][j]!);
                    }
                }
                else
                {
                    for (var j = 0; j < _values[i].Length; j++)
                    {
                        if (i * SegmentSize + j >= _pseudoLength)
                            break;

                        yield return new KeyValuePair<int, TValue>(i * SegmentSize + j, _values[i][j]!);
                    }
                }
            }
        }
    }

    public IEnumerable<int> KeysReverseOrder
    {
        get
        {
            if (_pseudoLength == 0)
                yield break;

            long index = (long)_pseudoLength - 1;

            var lastYielded = false;
            var zeroYielded = index == 0;

            while (index >= 0)
            {
                var coord = findNearest(index, false);
                index = getExternalIndex(coord);

                if (!lastYielded)
                {
                    if (index < _pseudoLength - 1)
                        yield return (int)(_pseudoLength - 1);

                    lastYielded = true;
                }

                if (index < 0)
                    break;

                if (index == 0)
                    zeroYielded = true;

                yield return (int)index;

                index--;
            }

            if (!zeroYielded)
                yield return 0;
        }
    }

    public IEnumerable<KeyValuePair<int, TValue>> ReverseOrder
    {
        get
        {
            if (_pseudoLength == 0)
                yield break;

            long index = (long)_pseudoLength - 1;

            var lastYielded = false;
            var zeroYielded = index == 0;

            while (index >= 0)
            {
                index = NearestNotMore(index, out var value);

                if (!lastYielded)
                {
                    if (index < _pseudoLength - 1)
                        yield return new KeyValuePair<int, TValue>((int)(_pseudoLength - 1), default);

                    lastYielded = true;
                }

                if (index < 0)
                    break;

                if (index == 0)
                    zeroYielded = true;

                yield return new KeyValuePair<int, TValue>((int)index, value);

                index--;
            }

            if (!zeroYielded)
                yield return new KeyValuePair<int, TValue>(0, default);
        }
    }

    public ICollection<int> Keys => KeysForwardOrder.ToList();

    public ICollection<TValue> Values => ForwardOrder.Select(x => x.Value).ToList();

    public int Count => (int)Length;

    public void Add(int key, TValue value)
    {
        if (NearestNotLess(key, out _) == key)
            throw new InvalidOperationException();

        this[key] = value;
    }

    public bool ContainsKey(int key)
    {
        return NearestNotLess(key, out _) == key;
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
        foreach (var item in ForwardOrder)
            yield return item;
    }

    public object Clone()
    {
        var result = new SparseArray<TValue>();

        result._navyData = new NavyItem[_navyData.Length][];
        for (var i = 0; i < _navyData.Length; i++)
        {
            result._navyData[i] = _navyData[i]?.Clone() as NavyItem[];
        }

        result._values = new TValue[_values.Length][];
        for (var i = 0; i < _values.Length; i++)
        {
            result._values[i] = _values[i]?.Clone() as TValue[];
        }

        result._used = _used.Clone() as short[];

        result._pseudoLength = _pseudoLength;

        return result;
    }
}