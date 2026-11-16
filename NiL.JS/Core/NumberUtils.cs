using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NiL.JS.Core;

public static class NumberUtils
{
    private struct DoubleStringCacheItem
    {
        public double key;
        public string value;
    }

    private const int _MaxSafeDigitsInTwoLongs = 31;
    private const int _MinExponentValue = -1022;
    private const int _MaxExponentValue = 1023;
    private const int _MantisaSize = 53;
    private const int _DefaultFloatDigitsLimit = 16;

    private static readonly DoubleStringCacheItem[] _CachedDoubleString = new DoubleStringCacheItem[8];
    private static int _CachedDoubleStringsIndex = 0;

    /// <summary>
    /// Проверяет символ на принадлежность диапазону цифр
    /// </summary>
    /// <param name="c"></param>
    /// <remarks>Использовать вместо этой функции char.IsDigit не получится. Версия char учитывает региональные особенности, что не нужно</remarks>
    /// <returns></returns>
    public static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private static int selectDigits(string source, ref ulong[] digits, ref int position, out int leadingCount)
    {
        leadingCount = 0;
        var digitsCount = 0;
        var parsed = false;
        var pos = position;
        while (pos < source.Length)
        {
            if (IsDigit(source[pos]))
            {
                var d = source[pos] - '0';
                if (d != 0 || digitsCount != 0)
                {
                    if (digitsCount < _MaxSafeDigitsInTwoLongs)
                    {
                        digits ??= new ulong[2];

                        digits[1] = (digits[1] << 4) | (digits[0] >> 60);
                        digits[0] <<= 4;
                        digits[0] = (ulong)((long)digits[0] | (long)d);
                        digitsCount++;
                    }
                }
                else
                {
                    leadingCount++;
                }

                pos++;
                parsed = true;
            }
            else if (parsed && source[pos] == '_' && source.Length > pos + 1 && IsDigit(source[pos + 1]))
            {
                pos++;
            }
            else
            {
                break;
            }
        }

        if (parsed)
        {
            position = pos;
            return digitsCount;
        }

        return 0;
    }

    public static bool TryParse(string source, int start, out double value)
        => TryParse(source, start, false, out value, out _);

    public static bool TryParse(string source, int start, bool allowSeparator, out double value, out int length)
    {
        var position = start;

        int sign = 1;
        if (position < source.Length && (source[position] == '-' || source[position] == '+'))
            sign = 44 - source[position++];

        var intPartStart = position;
        var intPart = null as ulong[];
        var intDigitsCount = selectDigits(source, ref intPart, ref position, out var intLeadingCount);

        if (intDigitsCount == 0
            && intLeadingCount == 0
            && source.Length > position
            && source[position] != '.')
        {
            value = double.NaN;
            length = -1;
            return false;
        }

        intPart ??= new ulong[2];

        var intPartSize = position - intPartStart;

        int fracDigitsCount = 0;
        var fracPart = new ulong[2];
        int fracLeadingCount;
        if (position < source.Length && source[position] == '.')
        {
            position++;
            fracDigitsCount = selectDigits(source, ref fracPart, ref position, out fracLeadingCount);
        }
        else
        {
            fracLeadingCount = 0;
        }

        if (intDigitsCount == _MaxSafeDigitsInTwoLongs)
        {
            fracDigitsCount = 0;
            fracPart[0] = 0ul;
            fracPart[1] = 0ul;
        }

        if (intPartSize <= 0 && (fracDigitsCount + fracLeadingCount) <= 0)
        {
            value = double.NaN;
            length = -1;
            return false;
        }

        var eDeg = 0;
        if (position < source.Length && (source[position] == 'e' || source[position] == 'E'))
        {
            position++;
            int td = 0;
            var eDigits = 0;
            int eSign = source[position] == '+' || source[position] == '-' ? 44 - source[position++] : 1;
            if (!IsDigit(source[position]))
            {
                position--;
                if (source[position] == 'e' || source[position] == 'E')
                    position--;
            }
            else
            {
                while (position < source.Length)
                {
                    if (!IsDigit(source[position]))
                        break;
                    else
                    {
                        if ((source[position] != '0' || eDigits != 0) && eDigits <= 6)
                        {
                            td = td * 10 + (source[position] - '0');
                            eDigits++;
                        }

                        position++;
                    }
                }

                eDeg += td * eSign;
            }
        }

        if (eDeg > 0)
        {
            while (intDigitsCount < _MaxSafeDigitsInTwoLongs && eDeg != 0)
            {
                ulong d = 0;

                if (fracLeadingCount > 0)
                {
                    fracLeadingCount--;
                }
                else
                {
                    if (fracDigitsCount > 0)
                    {
                        fracDigitsCount--;

                        int shift = fracDigitsCount % 16 * 4;
                        d = (fracPart[fracDigitsCount / 16] >> shift) & 0xf;
                        fracPart[fracDigitsCount / 16] ^= d << shift;
                    }
                }

                intPart[1] = (intPart[1] << 4) | (intPart[0] >> 60);
                intPart[0] <<= 4;
                intPart[0] = (ulong)((long)intPart[0] | (long)d);
                intDigitsCount++;

                eDeg--;
            }
        }
        else if (eDeg < 0)
        {
            if (intPart[0] == 0 && intPart[1] == 0)
            {
                fracLeadingCount += -eDeg;
                eDeg = 0;
            }
            else
            {
                while (fracLeadingCount > 0)
                {
                    fracLeadingCount--;
                    fracPart[0] = (fracPart[0] >> 4) | ((fracPart[1] & 0xf) << 60);
                    fracPart[1] >>= 4;
                }

                while (eDeg != 0)
                {
                    if (intPart[0] == 0 && intPart[1] == 0)
                        break;

                    var d = intPart[0] & 0xf;
                    intPart[0] = (intPart[0] >> 4) | ((intPart[1] & 0xf) << 60);
                    intPart[1] >>= 4;
                    intDigitsCount--;

                    if (fracDigitsCount == _MaxSafeDigitsInTwoLongs)
                    {
                        fracPart[0] = (fracPart[0] >> 4) | ((fracPart[1] & 0xf) << 60);
                        fracPart[1] >>= 4;
                        fracDigitsCount--;
                    }

                    fracPart[fracDigitsCount / 16] |= d << (fracDigitsCount % 16 * 4);

                    if (fracDigitsCount < _MaxSafeDigitsInTwoLongs)
                        fracDigitsCount++;

                    eDeg++;
                }

                if (intDigitsCount < 0)
                    intDigitsCount = 0;

                fracLeadingCount += -eDeg;
            }
        }

        var exponent = 0;

        var longBuffer = 0ul;
        var full = false;
        const ulong limit = 1ul << 60;
        for (var i = intDigitsCount; i-- > 0;)
        {
            longBuffer *= 10;
            if (!full)
                longBuffer += (intPart[i / 16] >> (i % 16 * 4)) & 0xf;

            if (longBuffer >= limit)
            {
                full = true;
                var tail = longBuffer >> 2;
                longBuffer >>= 3;
                exponent -= 3;
                if (longBuffer > limit)
                {
                    tail >>= 1;
                    longBuffer >>= 1;
                    exponent -= 1;
                }

                longBuffer += tail & 1;
            }
        }

        var logDelta = IntLog((long)longBuffer) - 60;
        if (logDelta < 0)
        {
            longBuffer <<= -logDelta;
            exponent += -logDelta;

            var fracPartIndex = (fracLeadingCount + fracDigitsCount) / 16;
            var fracDigitShift = (fracLeadingCount + fracDigitsCount) % 16 * 4;
            for (var i = 0; i < -logDelta; i++)
            {
                if (fracPart[0] == 0 && fracPart[1] == 0)
                    break;

                if (longBuffer == 0)
                {
                    logDelta--;
                    exponent++;
                }

                numStrSum(fracPart, fracPart, ref fracPart);
                if (fracPartIndex >= fracPart.Length)
                {
                    if ((fracPart[1] >> 60) >= 4)
                    {
                        fracPart[0] = (fracPart[0] >> 4) | ((fracPart[1] & 0xf) << 60);
                        fracPart[1] >>= 4;
                        fracLeadingCount--;
                        fracPartIndex = (fracLeadingCount + fracDigitsCount) / 16;
                        fracDigitShift = (fracLeadingCount + fracDigitsCount) % 16 * 4;
                    }
                }
                else if ((fracPart[fracPartIndex] >> fracDigitShift) != 0)
                {
                    fracPart[fracPartIndex] &= ~(0xful << fracDigitShift);
                    longBuffer |= 1ul << (-logDelta - i - 1);
                }
            }
        }
        else
        {
            longBuffer >>= logDelta;
            exponent -= logDelta;
        }

        for (var i = 0; i < eDeg; i++)
        {
            longBuffer *= 5;

            var tail = longBuffer >> 1;
            longBuffer >>= 2;
            exponent -= 3;
            if (longBuffer > limit * 2)
            {
                tail >>= 1;
                longBuffer >>= 1;
                exponent -= 1;
            }

            longBuffer += tail & 1;
        }

        var intLog = IntLog((long)longBuffer);
        logDelta = intLog - 52;
        var denormal = false;

        if ((exponent - logDelta) > -_MinExponentValue + _MantisaSize)
        {
            logDelta = exponent - -_MinExponentValue - _MantisaSize;
            denormal = true;
        }
        else if ((exponent - logDelta) < -_MaxExponentValue - _MantisaSize)
        {
            logDelta = exponent + -_MaxExponentValue - _MantisaSize;
            denormal = true;
        }

        exponent -= logDelta;

        if (logDelta <= -63 || logDelta >= 63)
        {
            longBuffer = 1ul << _MantisaSize;
        }
        else
        {
            if (logDelta < 0)
            {
                longBuffer <<= -logDelta;
            }
            else
            {
                if (denormal)
                    logDelta++;

                longBuffer = (longBuffer >> logDelta) + ((longBuffer >> (logDelta - 1)) & 1);
            }

            if (longBuffer >= 1ul << _MantisaSize)
            {
                exponent--;
                longBuffer >>= 1;
            }
        }

        if (longBuffer == 0)
        {
            value = 0.0 * sign;
        }
        else
        {
            longBuffer = (longBuffer & ((1UL << 52) - 1));
            exponent = 1023 - exponent + 52;
            exponent &= (1 << 11) - 1;
            longBuffer |= ((ulong)exponent << 52);
            longBuffer |= (ulong)sign & (1ul << 63);
            value = BitConverter.Int64BitsToDouble((long)longBuffer);
        }

        length = position - start;
        return true;
    }

    public static int IntLog(long value)
    {
        if (((ulong)value & 0xffff_ffff_0000_0000ul) != 0)
        {
            return IntLog((uint)(value >> 32)) + 32;
        }

        return IntLog((uint)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [CLSCompliant(false)]
    public static int IntLog(uint x)
    {
        var log = 0;

        if (x > 65535)
        {
            x >>= 16;
            log |= 16;
        }

        if (x > 255)
        {
            x >>= 8;
            log |= 8;
        }

        if (x > 15)
        {
            x >>= 4;
            log |= 4;
        }

        if (x > 3)
        {
            x >>= 2;
            log |= 2;
        }

        if (x > 1)
        {
            log |= 1;
        }

        return log;
    }

    public static string DoubleToString(double d, int digitsLimit = _DefaultFloatDigitsLimit, int fractionalPartDigitsLimit = int.MaxValue, bool trailingZeros = false, bool recomputeLimits = true)
    {
        if (d == 0.0)
            return "0";
        if (double.IsPositiveInfinity(d))
            return "Infinity";
        if (double.IsNegativeInfinity(d))
            return "-Infinity";
        if (double.IsNaN(d))
            return "NaN";

        string res;
        lock (_CachedDoubleString)
        {
            var abs = Math.Abs(d);
            int neg = (d < 0.0 || (d == -0.0 && NumberUtils.IsNegativeZero(d))) ? 1 : 0;

            if (digitsLimit == _DefaultFloatDigitsLimit && fractionalPartDigitsLimit == int.MaxValue && !trailingZeros)
            {
                for (var i = 8; i-- > 0;)
                {
                    if (_CachedDoubleString[i].key == d)
                        return _CachedDoubleString[i].value;
                }

                if (abs < 0.000001)
                    res = abs.ToString("0.################e-0", CultureInfo.InvariantCulture);
                else if (abs >= 1e+21)
                    res = abs.ToString("0.################e+0", CultureInfo.InvariantCulture);
                else if (abs >= 1e+18)
                    res = ((ulong)(abs / 1000.0)).ToString(CultureInfo.InvariantCulture) + "000";
                else
                    res = lowLevelFormat(_DefaultFloatDigitsLimit, int.MaxValue, false, abs, recomputeLimits);
            }
            else
                res = lowLevelFormat(digitsLimit, fractionalPartDigitsLimit, trailingZeros, abs, recomputeLimits);

            if (neg == 1)
                res = "-" + res;

            if (digitsLimit == _DefaultFloatDigitsLimit && fractionalPartDigitsLimit == int.MaxValue && !trailingZeros)
            {
                _CachedDoubleString[_CachedDoubleStringsIndex].key = d;
                _CachedDoubleString[_CachedDoubleStringsIndex].value = res;
                _CachedDoubleStringsIndex = (_CachedDoubleStringsIndex + 1) & 7;
            }
        }

        return res;
    }

    private static string lowLevelFormat(
        int digitsLimit,
        int fractionalPartDigitsLimit,
        bool trailingZeros,
        double abs,
        bool recomputeLimits)
    {
        string res;
        var highestBit = 1UL << 52;
        var raw = BitConverter.DoubleToInt64Bits(abs);
        ulong m = ((ulong)raw & ((1UL << 52) - 1)) | highestBit;
        int e = (int)((raw & long.MaxValue) >> 52);
        e = 52 - e + 1023;

        var intPart = e > 63 ? 0 : e <= 0 ? m : m >> e;
        var fracPart = e < 0 ? 0 : e >= 63 ? m : (m & ((1ul << e) - 1));
        var fracSize = 0;
        var intPartSize = 0;

        var str = new ulong[] { 1 };
        var temp = new ulong[] { 1 };
        var intBuffer = new ulong[] { 0 };
        var fracBuffer = new ulong[] { 0 };
        var lowerFracBuffer = Array.Empty<ulong>();
        var upperFracBuffer = Array.Empty<ulong>();
        var frac254 = Array.Empty<ulong>();

        if (fractionalPartDigitsLimit == 0)
            trailingZeros = false;

        if (intPart != 0)
        {
            var curDeg = e < 0 ? e : 0;
            var curBit = 0;
            for (var b = 0; b < 53 && intPart != 0; b++, curBit++)
            {
                if ((intPart & 1) != 0)
                {
                    while (curDeg < curBit)
                    {
                        numStrSum(str, str, ref str);
                        curDeg++;
                    }

                    numStrSum(intBuffer, str, ref intBuffer);
                }

                intPart >>= 1;
            }

            for (var pi = intBuffer.Length; pi-- > 0;)
            {
                intPartSize = digitsCount(intBuffer[pi]);
                if (intPartSize != 0)
                {
                    break;
                }
            }
        }

        if (fracPart != 0)
        {
            var curDeg = e > 53 ? 53 - e : 0;
            if (e < 53)
            {
                fracPart <<= 53 - e;
            }

            Array.Clear(str, 0, str.Length);
            str[str.Length - 1] = 1;

            var curBit = 1;
            //var nearestChangedDigit = 0;
            var lowerFracPart = fracPart - (1ul << Math.Max(0, 53 - e));

            for (var b = Math.Min(53, e); b-- > 0; curBit++)
            {
                while (curDeg < curBit)
                {
                    numStrSum(str, str, ref temp);
                    numStrSum(temp, temp, ref temp);
                    numStrSum(str, temp, ref str);

                    numStrMul10(lowerFracBuffer, ref lowerFracBuffer);

#if DEBUG
                    numStrMul10(fracBuffer, ref fracBuffer);
#endif

                    fracSize++;
                    curDeg++;
                }

                if ((lowerFracPart & highestBit) != 0)
                {
                    numStrSum(lowerFracBuffer, str, ref lowerFracBuffer);
                }

#if DEBUG
                if ((fracPart & highestBit) != 0)
                {
                    numStrSum(fracBuffer, str, ref fracBuffer);
                }
#endif

                if (b == 0)
                {
#if DEBUG
                    numStrSum(lowerFracBuffer, str, ref temp);
                    System.Diagnostics.Debug.Assert(fracBuffer.Select((x, i) => temp[i] == x).All(x => x));
#else
                    numStrSum(lowerFracBuffer, str, ref fracBuffer);
#endif
                    numStrSum(fracBuffer, str, ref upperFracBuffer);
                }

                lowerFracPart <<= 1;
                fracPart <<= 1;
            }

            if (recomputeLimits)
            {
                var lowerDifferentDigit = -1;
                var upperDifferentDigit = -1;
                var lowerZeroDigit = -1;
                var upperZeroDigit = -1;
                var firstMeaningZeroDigit = -1;
                var firstMeaningDigit = 0;
                for (var i = 0; i < fracSize; i++)
                {
                    if (fracBuffer.Length <= (fracSize - 1 - i) / 16)
                        continue;

                    var digit = (fracBuffer[(fracSize - 1 - i) / 16] >> ((fracSize - 1 - i) % 16 * 4)) & 0xf;
                    if (digit == 0)
                    {
                        if (firstMeaningZeroDigit == -2)
                            firstMeaningZeroDigit = i;
                    }
                    else if (firstMeaningZeroDigit == -1)
                    {
                        firstMeaningDigit = i;
                        firstMeaningZeroDigit = -2;
                    }

                    var lowerDigit = (lowerFracBuffer[(fracSize - 1 - i) / 16] >> ((fracSize - 1 - i) % 16 * 4)) & 0xf;
                    if (digit != lowerDigit)
                    {
                        if (lowerDifferentDigit == -1)
                            lowerDifferentDigit = i;

                        if (lowerDigit != 0 && lowerZeroDigit == -1)
                            lowerZeroDigit = i;
                    }

                    var upperDigit = (upperFracBuffer[(fracSize - 1 - i) / 16] >> ((fracSize - 1 - i) % 16 * 4)) & 0xf;
                    if (digit != upperDigit)
                    {
                        if (upperDifferentDigit == -1)
                            upperDifferentDigit = i;

                        if (upperDigit == 0 && upperZeroDigit == -1)
                            upperZeroDigit = i;
                    }
                }

                if (firstMeaningZeroDigit < 0)
                {
                    firstMeaningZeroDigit = fracSize;
                }

                if (lowerZeroDigit >= 0 && lowerZeroDigit > firstMeaningZeroDigit)
                {
                    fractionalPartDigitsLimit = upperDifferentDigit + 1;
                    digitsLimit = intPartSize + fractionalPartDigitsLimit;
                }
                else if (upperFracBuffer.Length <= fracSize / 16 || ((upperFracBuffer[fracSize / 16] >> (fracSize % 16 * 4)) & 0xf) == 0)
                {
                    if (upperZeroDigit >= 0 && upperZeroDigit < firstMeaningZeroDigit)
                    {
                        fracBuffer = upperFracBuffer;
                        fractionalPartDigitsLimit = upperZeroDigit;
                        digitsLimit = intPartSize + fractionalPartDigitsLimit;
                    }
                    else
                    {
                        fractionalPartDigitsLimit = Math.Max(upperDifferentDigit + 1 + (intPartSize is 0 ? firstMeaningDigit : -intPartSize), 0);
                        digitsLimit = intPartSize + fractionalPartDigitsLimit;
                    }
                }
                else
                {
                    fractionalPartDigitsLimit = Math.Max(17 + (intPartSize is 0 ? firstMeaningDigit : -intPartSize), 0);
                    digitsLimit = intPartSize + fractionalPartDigitsLimit;
                }
            }

            var lastFracDigit = (fracBuffer[(fracSize - fractionalPartDigitsLimit) / 16] >> ((fracSize - fractionalPartDigitsLimit) % 16 * 4)) & 0xf;
            var nextFracDigit = (fracBuffer[(fracSize - 1 - fractionalPartDigitsLimit) / 16] >> ((fracSize - 1 - fractionalPartDigitsLimit) % 16 * 4)) & 0xf;
            if (nextFracDigit >= 5 && (!recomputeLimits || fractionalPartDigitsLimit == 0 || lastFracDigit > 0))
            {
                fracBuffer[(fracSize - 1 - fractionalPartDigitsLimit) / 16] += (10 - nextFracDigit) << ((fracSize - 1 - fractionalPartDigitsLimit) % 16 * 4);
                numStrSum(fracBuffer, [], ref fracBuffer);
            }

            if (!trailingZeros && recomputeLimits)
            {
                do
                {
                    fractionalPartDigitsLimit--;
                    nextFracDigit = (fracSize - 1 - fractionalPartDigitsLimit) / 16 < fracBuffer.Length
                        ? (fracBuffer[(fracSize - 1 - fractionalPartDigitsLimit) / 16] >> ((fracSize - 1 - fractionalPartDigitsLimit) % 16 * 4)) & 0xf
                        : 0ul;
                }
                while (nextFracDigit == 0 && fractionalPartDigitsLimit > 0);
                fractionalPartDigitsLimit++;
            }

            if (fracBuffer.Length > fracSize / 16 && ((fracBuffer[fracSize / 16] >> (fracSize % 16 * 4)) & 0xf) != 0)
            {
                intBuffer[0] += 1;
                numStrSum(intBuffer, [], ref intBuffer);
                fracBuffer[fracSize / 16] -= 1ul << (fracSize % 16 * 4);
            }

            for (var pi = intBuffer.Length; pi-- > 0;)
            {
                intPartSize = digitsCount(intBuffer[pi]);
                if (intPartSize != 0)
                {
                    break;
                }
            }
        }

        var buffer = new char[2 + intBuffer.Length * 16 + fracBuffer.Length * 16];
        var bufferPos = 0;
        var nonZeroDigits = 0;

        var write = false;
        for (var i = intBuffer.Length * 16; i-- > 0;)
        {
            var v = (intBuffer[i / 16] >> 4 * (i % 16)) & 0xf;
            if (v != 0 || write)
            {
                buffer[bufferPos++] = (char)((char)v + '0');
                write = true;
                nonZeroDigits++;
            }
        }

        if (bufferPos == 0)
            buffer[bufferPos++] = '0';

        var lastRealSign = bufferPos;

        if (fracSize > 0 && nonZeroDigits < digitsLimit && fractionalPartDigitsLimit > 0)
        {
            buffer[bufferPos++] = '.';
            for (var i = fracSize; i-- > 0;)
            {
                var v = fracBuffer.Length <= i / 16 ? 0 : (fracBuffer[i / 16] >> 4 * (i % 16)) & 0xf;
                buffer[bufferPos++] = (char)((char)v + '0');

                if (v != 0)
                {
                    write = true;
                    lastRealSign = bufferPos;
                }

                if (write)
                {
                    nonZeroDigits++;
                }

                if (--fractionalPartDigitsLimit == 0 || nonZeroDigits == digitsLimit || bufferPos == buffer.Length)
                    break;
            }

            if (trailingZeros)
            {
                while (nonZeroDigits < digitsLimit && fractionalPartDigitsLimit-- > 0)
                {
                    buffer[bufferPos++] = '0';
                    nonZeroDigits++;
                }
            }
        }

        res = new string(buffer, 0, bufferPos);

        return res;
    }

    private static int digitsCount(ulong x)
    {
        var res = 0;

        if ((x >> 32) != 0)
        {
            res += 8;
            x >>= 32;
        }

        if ((x >> 16) != 0)
        {
            res += 4;
            x >>= 16;
        }

        if ((x >> 8) != 0)
        {
            res += 2;
            x >>= 8;
        }

        if ((x >> 4) != 0)
        {
            res += 1;
            x >>= 4;
        }

        return res + ((x & 0xf) != 0 ? 1 : 0);
    }

    private static void numStrMul10(ulong[] x, ref ulong[] output)
    {
        if (x.Length == 0)
        {
            Array.Clear(output, 0, output.Length);
            return;
        }

        var size = 0;
        for (var i = x.Length; i-- > 0;)
        {
            var vx = x[i];
            if (vx != 0)
            {
                size = i + 1;

                if (((vx >> 60) & 0xf) != 0)
                {
                    size++;
                }

                break;
            }
        }

        if (size > output.Length)
        {
            output = new ulong[size];
        }

        var o = 0ul;
        var xLen = x.Length;
        for (var i = 0; i < output.Length; i++)
        {
            var v = i >= xLen ? 0 : x[i];
            output[i] = (v << 4) | o;
            o = (v >> 60) & 0xf;
        }
    }
    private static void numStrSum(ulong[] left, ulong[] rigth, ref ulong[] output)
    {
        const ulong Mask8 = 0x8888_8888_8888_8888;

        var leftLen = left.Length;
        int rightLen = rigth.Length;
        var len = Math.Max(leftLen, rightLen);
        if (output.Length < len)
            output = new ulong[len];

        var go = 0u;
        for (var i = 0; i < output.Length; i++)
        {
            var l = i < leftLen ? left[i] : 0;
            var r = i < rightLen ? rigth[i] : 0;

            l += go;
            go = 0;

            var el = l;
            l += r;
            var o = ((el | r) & ~l & Mask8) >> 3;
            l += o * 6;
            r = l;
            go |= (uint)(o >> 60);
            for (; ; )
            {
                o = ((l & Mask8) >> 2) & (l >> 1 | l);
                if (o == 0)
                    break;

                l += o * 3;
            }

            go |= (uint)((r & ~l) >> 63);

            output[i] = l;

            if (go != 0 && i + 1 == output.Length)
                Array.Resize(ref output, output.Length * 2);
        }
    }

#if !NET40
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool IsNegativeZero(double d)
    {
        return (((ulong)BitConverter.DoubleToInt64Bits(d)) & 0x800F_FFFF_FFFF_FFFF) == 0x8000_0000_0000_0000;
    }
}
