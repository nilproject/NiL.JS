using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NiL.JS.Core;

[DebuggerDisplay("Count = {Count}")]
public sealed class SparseArray<TValue> : IList<TValue>, IDictionary<int, TValue>, ICloneable
{
    private const int SegmentSizeDegree = 11;
    private const int SegmentNextAdditionSize = SegmentSizeDegree / 2;
    private const uint SegmentNextAdditionMask = (1 << (SegmentSizeDegree / 2)) - 1;
    private const int SegmentNextAdditionOffset = 32 - SegmentSizeDegree;
    private const int SegmentSize = 1 << SegmentSizeDegree;
    private const uint SegmentIndexMask = uint.MaxValue / SegmentSize;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NavigationItem
    {
        public uint Index;
        public ushort ZeroNext;
        public ushort OneNext;

        public override string ToString()
        {
            return Index + "[" + ZeroNext + ";" + OneNext + "]";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SegmentNavigationItem
    {
        private uint _index;
        private ushort _zeroNext;
        private ushort _oneNext;

        public int SegmentIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(_index & SegmentIndexMask);
            set => _index = (_index & ~SegmentIndexMask) | (uint)value;
        }
        public int SegmentZeroNext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _zeroNext | (int)(((_index >> SegmentNextAdditionOffset) & SegmentNextAdditionMask) << 16);
            set
            {
                _zeroNext = (ushort)value;
                _index &= ~(SegmentNextAdditionMask << SegmentNextAdditionOffset);
                _index |= (uint)((value >> 16) & SegmentNextAdditionMask) << SegmentNextAdditionOffset;
            }
        }

        public int SegmentOneNext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _oneNext | (int)(((_index >> (SegmentNextAdditionOffset + SegmentNextAdditionSize)) & SegmentNextAdditionMask) << 16);
            set
            {
                _oneNext = (ushort)value;
                _index &= ~(SegmentNextAdditionMask << (SegmentNextAdditionOffset + SegmentNextAdditionSize));
                _index |= (uint)((value >> 16) & SegmentNextAdditionMask) << (SegmentNextAdditionOffset + SegmentNextAdditionSize);
            }
        }

        public override string ToString() => SegmentIndex + "[" + SegmentZeroNext + ";" + SegmentOneNext + "]";
    }

    private static TValue _fictive;

    private uint _pseudoLength;

    private SegmentNavigationItem[] _segmentsNavigationData;
    private int _segmentsCount;

    private NavigationItem[][] _navigationData;
    private TValue[][] _values;
    private ushort[] _used;

    [CLSCompliant(false)]
    public uint Length => _pseudoLength;

    public SparseArray()
    {
        _segmentsNavigationData = [];
        _values = [];
        _navigationData = [];
        _used = [];
    }

    public SparseArray(TValue[] values)
    {
        _segmentsNavigationData = [];
        _values = [];
        _navigationData = [];
        _used = [];

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
        var virtualSegmentIndex = (int)(index / SegmentSize);
        var realSegmentIndex = segmentIndexRecalc(virtualSegmentIndex, true);

        if (realSegmentIndex < 0 || _navigationData.Length <= realSegmentIndex)
        {
            got = false;
            _fictive = default;
            return ref _fictive;
        }

        if (_navigationData[realSegmentIndex].Length == 0)
        {
            var values = _values[realSegmentIndex];
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

        ref var result = ref getFromTree(index, true, out got, realSegmentIndex);
        return ref result;
    }

    internal ref TValue TryGetInternalForWrite(uint index, out bool got)
    {
        var virtaulSegmentIndex = (int)(index / SegmentSize);
        var realSegmentIndex = segmentIndexRecalc(virtaulSegmentIndex, false);

        if (_navigationData[realSegmentIndex].Length == 0)
        {
            var values = _values[realSegmentIndex];
            var itemIndex = (int)index & (SegmentSize - 1);

            if (itemIndex >= values.Length)
            {
                if (itemIndex <= values.Length * 3 || values.Length * 4 >= SegmentSize)
                {
                    var newSize = Math.Min(SegmentSize, Math.Max(values.Length * 2, 1));
                    while (newSize <= itemIndex)
                        newSize *= 2;

                    Array.Resize(ref values, newSize);

                    _values[realSegmentIndex] = values;
                }
                else
                {
                    rebuildSegmentToSparse(realSegmentIndex, values);

                    return ref getFromTree(index, false, out got, realSegmentIndex);
                }
            }

            if (_pseudoLength <= index)
                _pseudoLength = index + 1;

            got = true;
            return ref values[itemIndex];
        }

        return ref getFromTree(index, false, out got, realSegmentIndex);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void rebuildSegmentToSparse(int realSegmentIndex, TValue[] values)
    {
        var oldLen = values.Length;
        var len = values.Length;

        if (values.Length == 0)
            len = 1;

        if (values.Length != len)
        {
            Array.Resize(ref values, len);
            _values[realSegmentIndex] = values;
        }

        _navigationData[realSegmentIndex] = new NavigationItem[len];

        var bias = _segmentsNavigationData[realSegmentIndex].SegmentIndex * SegmentSize;

        if (typeof(TValue).IsClass)
        {
            for (var valueIndex = 0; valueIndex < oldLen; valueIndex++)
            {
                if (values[valueIndex] is not null)
                {
                    var value = values[valueIndex];
                    values[valueIndex] = default;
                    getFromTree((uint)(valueIndex + bias), false, out _, realSegmentIndex) = value;
                }
            }
        }
        else
        {
            for (var valueIndex = 0; valueIndex < oldLen; valueIndex++)
            {
                var value = values[valueIndex];
                values[valueIndex] = default;
                getFromTree((uint)(valueIndex + bias), false, out _, realSegmentIndex) = value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void resizeL0(int newRealSegmentsCount)
    {
        var oldSize = _navigationData.Length;
        Array.Resize(ref _navigationData, newRealSegmentsCount <= 1 ? 1 : 1 << NumberUtils.IntLog(newRealSegmentsCount));
        Array.Resize(ref _segmentsNavigationData, _navigationData.Length);
        Array.Resize(ref _values, _navigationData.Length);
        Array.Resize(ref _used, _navigationData.Length);

        for (var i = oldSize; i < _navigationData.Length; i++)
        {
            _navigationData[i] = [];
            _values[i] = [];
        }
    }


    private enum FindNearestMode { NotLess, NotMore }
    private (int realSegmentIndex, int virtualSegmentIndex, int realItemIndex) findNearest(long index, FindNearestMode mode)
    {
        var segmentNavigation = _segmentsNavigationData;

        if (segmentNavigation.Length == 0)
            return (-1, -1, -1);

        var virtualSegmentIndex = (int)(index / SegmentSize);
        var mask = (int)((1L << 31) / SegmentSize);
        var firstTry = _segmentsCount > index;
        var i = firstTry ? (int)index : 0;
        int nextSegmentIndex;
        var realSegmentIndex = -1;
        var alterI = -1;

        if (mode is FindNearestMode.NotLess)
        {
            var isZeroBranch = false;

            while (true)
            {
                ref var segmentNavItem = ref segmentNavigation[i];
                if (segmentNavItem.SegmentIndex == virtualSegmentIndex)
                {
                    realSegmentIndex = i;

                    if (_navigationData[realSegmentIndex].Length == 0)
                    {
                        if (_values[realSegmentIndex].Length > (int)(index & (SegmentSize - 1)))
                        {
                            return (realSegmentIndex, segmentNavItem.SegmentIndex, (int)(index & (SegmentSize - 1)));
                        }
                    }
                    else
                    {
                        var nestedMask = SegmentSize >> 1;
                        var navItem = _navigationData[realSegmentIndex][0];
                        var itemIndex = 0;
                        var nestedAlterI = -1;
                        while (true)
                        {
                            if (navItem.Index >= index)
                                return (realSegmentIndex, segmentNavItem.SegmentIndex, itemIndex);

                            itemIndex = (index & nestedMask) == 0 ? navItem.ZeroNext : navItem.OneNext;

                            if (itemIndex == 0)
                            {
                                if (navItem.OneNext == 0)
                                {
                                    if (nestedAlterI == -1)
                                        break;

                                    nestedMask = 0;
                                    navItem = _navigationData[realSegmentIndex][nestedAlterI];
                                    itemIndex = nestedAlterI;
                                    nestedAlterI = -1;
                                    continue;
                                }

                                itemIndex = navItem.OneNext;
                                nestedMask = 0;
                            }

                            if (itemIndex != navItem.OneNext && navItem.OneNext != 0)
                                nestedAlterI = navItem.OneNext;

                            nestedMask >>= 1;
                            navItem = _navigationData[realSegmentIndex][itemIndex];
                        }
                    }
                }

                if (firstTry)
                {
                    firstTry = false;
                    i = 0;
                    continue;
                }

                if (segmentNavItem.SegmentIndex > virtualSegmentIndex)
                {
                    // дальнейшее продвижение будет только увеличивать индекс
                    virtualSegmentIndex = segmentNavItem.SegmentIndex;
                    realSegmentIndex = i;
                    return (realSegmentIndex, virtualSegmentIndex, 0);
                }

                var isZero = (virtualSegmentIndex & mask) == 0;
                nextSegmentIndex = isZero ? segmentNavItem.SegmentZeroNext : segmentNavItem.SegmentOneNext;

                if (nextSegmentIndex == 0)
                {
                    if (segmentNavItem.SegmentIndex == virtualSegmentIndex && segmentNavItem.SegmentZeroNext != 0)
                    {
                        isZeroBranch = true;
                        i = segmentNavItem.SegmentZeroNext;
                        mask = 0;
                        continue;
                    }
                    else if (segmentNavItem.SegmentOneNext == 0)
                    {
                        if (alterI == -1)
                            return (-1, -1, -1);

                        mask = 0;
                        i = alterI;
                        continue;
                    }
                    else
                    {
                        mask = 0;
                        nextSegmentIndex = segmentNavItem.SegmentOneNext;
                    }
                }

                if (isZero && segmentNavItem.SegmentOneNext != 0 && !isZeroBranch)
                    alterI = segmentNavItem.SegmentOneNext;

                i = nextSegmentIndex;
                mask >>= 1;
            }
        }
        else
        {
            while (true)
            {
                ref var segmentNavItem = ref segmentNavigation[i];
                if (segmentNavItem.SegmentIndex == virtualSegmentIndex)
                {
                    realSegmentIndex = i;

                    if (_navigationData[realSegmentIndex].Length == 0)
                    {
                        return (realSegmentIndex, segmentNavItem.SegmentIndex, Math.Min(_values[realSegmentIndex].Length - 1, (int)(index & (SegmentSize - 1))));
                    }
                    else
                    {
                        mask = SegmentSize >> 1;
                        var n = _navigationData[realSegmentIndex][0];
                        var itemIndex = 0;
                        while (true)
                        {
                            if (n.Index == index)
                                return (realSegmentIndex, segmentNavItem.SegmentIndex, itemIndex);

                            if (n.Index > index)
                                return (-1, -1, -1);

                            var nextItemIndex = (index & mask) == 0 ? n.ZeroNext : n.OneNext;

                            if (nextItemIndex == 0)
                            {
                                if (n.ZeroNext == 0)
                                    return (realSegmentIndex, segmentNavItem.SegmentIndex, itemIndex);

                                nextItemIndex = n.ZeroNext;
                                mask = int.MaxValue;
                            }

                            if (_navigationData[realSegmentIndex][nextItemIndex].Index > index)
                            {
                                if (n.ZeroNext > 0 && _navigationData[realSegmentIndex][n.ZeroNext].Index < index)
                                {
                                    mask = int.MaxValue;
                                    nextItemIndex = n.ZeroNext;
                                }
                                else
                                    return (realSegmentIndex, segmentNavItem.SegmentIndex, itemIndex);
                            }

                            mask >>= 1;
                            n = _navigationData[realSegmentIndex][nextItemIndex];
                            itemIndex = nextItemIndex;
                        }
                    }
                }

                if (firstTry)
                {
                    firstTry = false;
                    i = 0;
                    continue;
                }

                if (segmentNavItem.SegmentIndex > virtualSegmentIndex)
                {
                    return (-1, -1, -1);
                }

                var isZero = (virtualSegmentIndex & mask) == 0;
                nextSegmentIndex = isZero ? segmentNavItem.SegmentZeroNext : segmentNavItem.SegmentOneNext;

                if (!isZero && segmentNavItem.SegmentZeroNext != 0)
                    alterI = segmentNavItem.SegmentZeroNext;

                if (nextSegmentIndex == 0
                    || segmentNavigation[nextSegmentIndex].SegmentIndex > virtualSegmentIndex
                    || (_navigationData[nextSegmentIndex].Length > 0 && _navigationData[nextSegmentIndex][0].Index > index))
                {
                    if (alterI != -1 && segmentNavItem.SegmentIndex < segmentNavigation[alterI].SegmentIndex)
                    {
                        mask = int.MaxValue;
                        i = alterI;
                        alterI = -1;
                        continue;
                    }

                    realSegmentIndex = i;

                    if (_navigationData[realSegmentIndex].Length == 0)
                        return (realSegmentIndex, segmentNavItem.SegmentIndex, _values[realSegmentIndex].Length - 1);
                    else
                    {
                        var n = _navigationData[realSegmentIndex][0];
                        var itemIndex = 0;
                        while (true)
                        {
                            if (n.OneNext != 0)
                                n = _navigationData[realSegmentIndex][itemIndex = n.OneNext];
                            else if (n.ZeroNext != 0)
                                n = _navigationData[realSegmentIndex][itemIndex = n.ZeroNext];
                            else
                                break;
                        }

                        return (realSegmentIndex, segmentNavItem.SegmentIndex, itemIndex);
                    }
                }

                i = nextSegmentIndex;
                mask >>= 1;
            }
        }
    }

    private int segmentIndexRecalc(int segmentIndex, bool forRead)
    {
        var navi = _segmentsNavigationData;

        if (navi.Length == 0)
        {
            if (forRead)
                return -1;

            _segmentsNavigationData = [new() { SegmentIndex = segmentIndex }];
            _segmentsCount = 1;
            _navigationData = [[]];
            _values = [[]];
            _used = [0];
            return 0;
        }

        if (segmentIndex < _segmentsCount && navi[segmentIndex].SegmentIndex == segmentIndex)
            return segmentIndex;

        var mask = (int)((1L << 31) / SegmentSize);
        var i = 0;
        int nextIndex;
        while (true)
        {
            var navItem = navi[i];
            if (navItem.SegmentIndex == segmentIndex)
                return i;

            var isNotZero = (segmentIndex & mask) != 0;

            if (navItem.SegmentIndex > segmentIndex)
            {
                if (forRead)
                    return -1;

                var oldIndex = navItem.SegmentIndex;
                var oldNavData = _navigationData[i];
                var oldValues = _values[i];
                var oldUsed = _used[i];

                _navigationData[i] = [];
                _values[i] = [];
                _used[i] = 0;
                navi[i].SegmentIndex = segmentIndex;

                isNotZero = (oldIndex & mask) != 0;
                if (isNotZero ? navItem.SegmentOneNext == 0 : navItem.SegmentZeroNext == 0)
                {
                    nextIndex = _segmentsCount++;
                    if (isNotZero)
                        navi[i].SegmentOneNext = nextIndex;
                    else
                        navi[i].SegmentZeroNext = nextIndex;

                    if (_segmentsNavigationData.Length >= nextIndex)
                        resizeL0(nextIndex * 2);
                }
                else
                {
                    nextIndex = segmentIndexRecalc(oldIndex, false);
                }

                _segmentsNavigationData[nextIndex].SegmentIndex = oldIndex;
                _navigationData[nextIndex] = oldNavData;
                _values[nextIndex] = oldValues;
                _used[nextIndex] = oldUsed;

                return i;
            }

            nextIndex = isNotZero ? navItem.SegmentOneNext : navItem.SegmentZeroNext;

            if (nextIndex == 0)
            {
                if (forRead)
                    return -1;

                _segmentsCount++;
                nextIndex = _segmentsCount - 1;

                if (isNotZero)
                    navi[i].SegmentOneNext = nextIndex;
                else
                    navi[i].SegmentZeroNext = nextIndex;

                if (nextIndex >= navi.Length)
                {
                    var newSize = nextIndex * 2;
                    resizeL0(newSize);

                    navi = _segmentsNavigationData;
                }

                navi[nextIndex].SegmentIndex = segmentIndex;
            }

            i = nextIndex;
            mask >>= 1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref TValue getFromTree(uint index, bool forRead, out bool got, int realSegmentIndex)
    {
        var mask = SegmentSize >> 1;
        var values = _values[realSegmentIndex];
        var navigationData = _navigationData[realSegmentIndex];

        var i = (int)index & (SegmentSize - 1);

        if (i < navigationData.Length && navigationData[i].Index == index)
        {
            if (!forRead && i == 0 && _used[realSegmentIndex] == 0)
            {
                _used[realSegmentIndex] = 1;

                if (_pseudoLength <= index)
                    _pseudoLength = index + 1;
            }

            got = true;
            return ref values[i];
        }

        i = 0;

        ushort ni;
        while (true)
        {
            ref var navItem = ref navigationData[i];
            if (navItem.Index < index)
            {
                var isNotZero = (index & mask) != 0;
                ni = isNotZero ? navItem.OneNext : navItem.ZeroNext;

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

                    ni = _used[realSegmentIndex]++;

                    if (isNotZero)
                        navItem.OneNext = ni;
                    else
                        navItem.ZeroNext = ni;

                    got = true;

                    if (ni >= navigationData.Length)
                    {
                        var newSize = ni * 2;

                        if (newSize * 2 >= SegmentSize)
                        {
                            values = new TValue[SegmentSize];

                            navigationData = _navigationData[realSegmentIndex];
                            for (var n = 0; n < navigationData.Length; n++)
                            {
                                var relativeIndex = navigationData[n].Index & (SegmentSize - 1);
                                if (relativeIndex == 0 && n != 0)
                                    break;

                                values[relativeIndex] = _values[realSegmentIndex][n];
                            }

                            _values[realSegmentIndex] = values;
                            _navigationData[realSegmentIndex] = [];

                            return ref values[index & (SegmentSize - 1)];
                        }
                        else
                        {
                            Array.Resize(ref navigationData, newSize);
                            Array.Resize(ref values, newSize);

                            _navigationData[realSegmentIndex] = navigationData;
                            _values[realSegmentIndex] = values;
                        }
                    }

                    navigationData[ni].Index = index;
                    return ref values[ni];
                }

                i = ni;
                mask >>= 1;
            }
            else if (navItem.Index > index)
            {
                if (forRead)
                {
                    got = false;
                    _fictive = default;
                    return ref _fictive;
                }

                var oldIndex = navItem.Index;
                var oldValue = values[i];

                navItem.Index = index;
                values[i] = default!;

                if (oldIndex < _pseudoLength)
                    this[(int)oldIndex] = oldValue!;

                got = true;

                if (_navigationData[realSegmentIndex].Length == 0)
                    i = (int)(index & (SegmentSize - 1));

                values = _values[realSegmentIndex];
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
            var r = TryGetInternalForRead((uint)index, out var got);

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
        var coord = findNearest(_pseudoLength - 1, FindNearestMode.NotMore);
        var externalIndex = getExternalIndex(coord);
        _pseudoLength = (uint)externalIndex + 1;
    }

    private long getExternalIndex((int realSegmentIndex, int virtualSegmentIndex, int itemIndex) coord)
    {
        if (coord.itemIndex == -1)
            return -1;

        var navItems = _navigationData[coord.realSegmentIndex];
        if (navItems.Length != 0)
            return navItems[coord.itemIndex].Index;

        var externalIndex = coord.virtualSegmentIndex * SegmentSize + (uint)coord.itemIndex;
        return externalIndex;
    }

    public void Clear()
    {
        _segmentsNavigationData = [];
        _values = [];
        _navigationData = [];
        _used = [];
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

        var coord = findNearest(index, FindNearestMode.NotLess);

        if (coord.realItemIndex == -1)
        {
            value = default;
            return -1;
        }

        value = _values[coord.realSegmentIndex][coord.realItemIndex];
        var externalIndex = getExternalIndex(coord);
        return externalIndex;
    }

    public long NearestNotMore(long index, out TValue value)
    {
        var coord = findNearest(index, FindNearestMode.NotMore);

        if (coord.realItemIndex == -1)
        {
            value = default;
            return -1;
        }

        value = _values[coord.realSegmentIndex][coord.realItemIndex];
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
            for (var i = 0; i < _navigationData.Length; i++)
            {
                var virtualSegmentIndex = _segmentsNavigationData[i].SegmentIndex * SegmentSize;

                if (_navigationData[i].Length != 0)
                {
                    for (var j = 0; j < _used[i]; j++)
                    {
                        yield return new KeyValuePair<int, TValue>(_navigationData[i].Length is 0 ? virtualSegmentIndex + j : (int)_navigationData[i][j].Index, _values[i][j]!);
                    }
                }
                else
                {
                    for (var j = 0; j < _values[i].Length; j++)
                    {
                        if (virtualSegmentIndex + j >= _pseudoLength)
                            break;

                        yield return new KeyValuePair<int, TValue>(virtualSegmentIndex + j, _values[i][j]!);
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
                var coord = findNearest(index, FindNearestMode.NotMore);
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

        result._navigationData = new NavigationItem[_navigationData.Length][];
        for (var i = 0; i < _navigationData.Length; i++)
        {
            if (_navigationData[i].Length == 0)
                result._navigationData[i] = [];
            else
            {
                result._navigationData[i] = new NavigationItem[_navigationData[i].Length];
                Array.Copy(_navigationData[i], result._navigationData[i], result._navigationData[i].Length);
            }
        }

        result._values = new TValue[_values.Length][];
        for (var i = 0; i < _values.Length; i++)
        {
            if (_values[i].Length == 0)
                result._values[i] = [];
            else
                result._values[i] = _values[i]?.Clone() as TValue[];
        }

        result._used = _used.Clone() as ushort[];

        result._pseudoLength = _pseudoLength;

        result._segmentsCount = _segmentsCount;

        result._segmentsNavigationData = _segmentsNavigationData.Clone() as SegmentNavigationItem[];

        return result;
    }
}