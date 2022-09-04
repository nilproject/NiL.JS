using System;
using System.Globalization;

namespace NiL.JS.Core
{
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
        private const int _ToStringDigitsLimit = 16;

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
                            if (digits is null)
                                digits = new ulong[2];

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

        public static int TryParse(string source, int start, out double value)
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
                return -1;
            }

            if (intPart == null)
                intPart = new ulong[2];

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
                return -1;
            }

            var eDeg = intPartSize - intDigitsCount - intLeadingCount;
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

            return position - start;
        }

        public static int IntLog(long value)
        {
            var x = (ulong)value;
            unchecked
            {
                int log = (int)((-(long)(x & 0xffff_ffff_0000_0000ul)) >> 62) & 32;
                log |= (int)((-(long)((x >> log) & 0xffff0000ul)) >> 62) & 16;
                log |= (int)((-(long)((x >> log) & 0xff00ul)) >> 62) & 8;
                log |= (int)((-(long)((x >> log) & 0xf0ul)) >> 62) & 4;
                log |= (int)((-(long)((x >> log) & 12ul)) >> 62) & 2;
                log |= (int)((-(long)((x >> log) & 2ul)) >> 62) & 1;

                return log;
            }
        }

        public static string DoubleToString(double d, int digitsLimit = _ToStringDigitsLimit, bool trailingZeros = false)
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
                int neg = (d < 0.0 || (d == -0.0 && Tools.IsNegativeZero(d))) ? 1 : 0;

                if (digitsLimit == _ToStringDigitsLimit && !trailingZeros)
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
                        res = lowLevelFormat(_ToStringDigitsLimit, false, abs);
                }
                else
                    res = lowLevelFormat(digitsLimit, trailingZeros, abs);

                if (neg == 1)
                    res = "-" + res;

                _CachedDoubleString[_CachedDoubleStringsIndex].key = d;
                _CachedDoubleString[_CachedDoubleStringsIndex].value = res;
                _CachedDoubleStringsIndex = (_CachedDoubleStringsIndex + 1) & 7;
            }

            return res;
        }

        private static string lowLevelFormat(int digitsLimit, bool trailingZeros, double abs)
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

            var str = new ulong[] { 1 };
            var temp = new ulong[] { 1 };
            var intBuffer = new ulong[] { 0 };
            var fracBuffer = new ulong[] { 0 };
            var alterFracBuffer = default(ulong[]);

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

                if (fracPart != 0)
                {
                    Array.Clear(str, 0, str.Length);
                    str[str.Length - 1] = 1;
                }
            }

            if (fracPart != 0)
            {
                var curDeg = e > 53 ? 53 - e : 0;
                var curBit = 1;
                if (e < 53)
                {
                    fracPart <<= 53 - e;
                }

                var fracPartSize = Math.Min(53, e);

                for (var b = 0; b <= fracPartSize; b++, curBit++)
                {
                    if ((fracPart & highestBit) != 0 || b == fracPartSize)
                    {
                        while (curDeg < curBit)
                        {
                            numStrSum(str, str, ref temp);
                            numStrSum(temp, temp, ref temp);
                            numStrSum(str, temp, ref str);

                            numStrMul10(fracBuffer, ref fracBuffer);

                            fracSize++;
                            curDeg++;
                        }

                        if (b == fracPartSize)
                        {
                            alterFracBuffer = new ulong[fracBuffer.Length];
                            numStrSum(fracBuffer, str, ref alterFracBuffer);
                        }
                        else
                        {
                            numStrSum(fracBuffer, str, ref fracBuffer);
                        }
                    }

                    fracPart <<= 1;
                }

                var fracLen = 0;
                var alterLen = 0;
                var fracSizeLimit = digitsLimit;

                for (var pi = intBuffer.Length; pi-- > 0;)
                {
                    var count = digitsCount(intBuffer[pi]);
                    if (count != 0)
                        fracSizeLimit -= count + pi * 16;
                }

                for (var di = fracSize; di-- > 0 && fracSize - di < fracSizeLimit;)
                {
                    if (fracBuffer.Length > di / 16 && ((fracBuffer[di / 16] >> ((di % 16) * 4)) & 0xf) != 0)
                        fracLen = fracSize - di;

                    if (alterFracBuffer.Length > di / 16 && ((alterFracBuffer[di / 16] >> ((di % 16) * 4)) & 0xf) != 0)
                        alterLen = fracSize - di;
                }

                if (alterLen < fracLen)
                {
                    fracBuffer = alterFracBuffer;
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
                    write = true;
                    buffer[bufferPos++] = (char)((char)v + '0');
                    nonZeroDigits++;
                }
            }

            if (bufferPos == 0)
                buffer[bufferPos++] = '0';

            var lastRealSign = bufferPos;

            if (fracSize != 0)
            {
                buffer[bufferPos++] = '.';
                for (var i = fracSize; i-- > 0;)
                {
                    var v = fracBuffer.Length <= i / 16 ? 0 : (fracBuffer[i / 16] >> 4 * (i % 16)) & 0xf;
                    buffer[bufferPos] = (char)((char)v + '0');
                    bufferPos++;

                    if (v != 0)
                    {
                        write = true;
                        lastRealSign = bufferPos;
                    }

                    if (write)
                        nonZeroDigits++;

                    if (nonZeroDigits == digitsLimit || bufferPos == buffer.Length)
                        break;
                }
            }

            res = new string(buffer, 0, trailingZeros ? bufferPos : lastRealSign);
            if (res.Length < digitsLimit && trailingZeros)
            {
                if (fracSize == 0)
                    res += '.';

                res = res.PadRight(digitsLimit + 1, '0');
            }

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

                do
                {
                    var el = l & 0x8888_8888_8888_8888;
                    var er = r & 0x8888_8888_8888_8888;
                    l += r;
                    var o = ((el | er) & ~l) >> 3;
                    r = o * 6;
                    go |= (uint)(o >> 60);
                    for (; ; )
                    {
                        o = l & (l << 1 | l << 2) & 0x8888888888888888;
                        if (o == 0)
                            break;
                        o >>= 3;

                        l -= o * 10;
                        l += o << 4;
                        go |= (uint)(o >> 60);
                    }
                }
                while (r != 0);

                output[i] = l;

                if (go != 0 && i + 1 == output.Length)
                    Array.Resize(ref output, output.Length * 2);
            }
        }
    }
}
