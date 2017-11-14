using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
    public sealed class RopeString : IEnumerable<char>, IEnumerable, IEquatable<string>
    {
        private int _length;
        private object _firstSource;
        private object _secondSource;
        private string firstSource
        {
            get
            {
                if (_firstSource == null)
                    return null;

                return _firstSource as string ?? (_firstSource = _firstSource.ToString()) as string;
            }
        }
        private string secondSource
        {
            get
            {
                if (_secondSource == null)
                    return null;

                return _secondSource as string ?? (_secondSource = _secondSource.ToString()) as string;
            }
        }

        public RopeString()
        {
            _firstSource = "";
            _secondSource = "";
        }

        public RopeString(object source)
        {
            _firstSource = source ?? "" as object;
            _secondSource = "";
        }

        public RopeString(object firstSource, object secondSource)
        {
            _firstSource = firstSource ?? "" as object;
            _secondSource = secondSource ?? "" as object;

            _length = calcLength();

            if (_length < 0)
                ExceptionHelper.Throw(new RangeError("String is too large"));
        }

        public char this[int index]
        {
            get
            {
                if (_firstSource != null)
                {
                    if ((_firstSource is RopeString) && (_firstSource as RopeString).Length < index)
                        return (_firstSource as RopeString)[index];
                    if ((_firstSource is StringBuilder) && (_firstSource as StringBuilder).Length < index)
                        return (_firstSource as StringBuilder)[index];
                    if (firstSource.Length < index)
                        return firstSource[index];
                }

                if (_secondSource != null)
                {
                    if (_secondSource is RopeString)
                        return (_secondSource as RopeString)[index];
                    if (_secondSource is StringBuilder)
                        return (_secondSource as StringBuilder)[index];
                    return secondSource[index];
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public object Clone()
        {
            return new RopeString(_firstSource, _secondSource);
        }
        public int CompareTo(string strB)
        {
            return ToString().CompareTo(strB);
        }
        public bool Contains(string value)
        {
            return ToString().Contains(value);
        }
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            ToString().CopyTo(sourceIndex, destination, destinationIndex, count);
        }
        public bool EndsWith(string value)
        {
            return ToString().EndsWith(value);
        }
        public bool EndsWith(string value, StringComparison comparisonType)
        {
            return ToString().EndsWith(value, comparisonType);
        }
        public override bool Equals(object obj)
        {
            return ToString().Equals(obj);
        }
        public bool Equals(string value)
        {
            return ToString().Equals(value);
        }
        public bool Equals(string value, StringComparison comparisonType)
        {
            return ToString().Equals(value, comparisonType);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public int IndexOf(char value)
        {
            var r = firstSource.IndexOf(value);
            if (r == -1)
                return secondSource.IndexOf(value);
            return r;
        }
        public int IndexOf(string value)
        {
            var r = firstSource.IndexOf(value);
            if (r == -1)
                return secondSource.IndexOf(value);
            return r;
        }
        public int IndexOf(char value, int startIndex)
        {
            var r = firstSource.IndexOf(value, startIndex);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex);
            return r;
        }
        public int IndexOf(string value, int startIndex)
        {
            var r = firstSource.IndexOf(value, startIndex);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex);
            return r;
        }
        public int IndexOf(string value, StringComparison comparisonType)
        {
            var r = firstSource.IndexOf(value, comparisonType);
            if (r == -1)
                return secondSource.IndexOf(value, comparisonType);
            return r;
        }
        public int IndexOf(char value, int startIndex, int count)
        {
            var r = firstSource.IndexOf(value, startIndex, count);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex, count);
            return r;
        }
        public int IndexOf(string value, int startIndex, int count)
        {
            var r = firstSource.IndexOf(value, startIndex, count);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex, count);
            return r;
        }
        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            var r = firstSource.IndexOf(value, startIndex, comparisonType);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex, comparisonType);
            return r;
        }
        public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            var r = firstSource.IndexOf(value, startIndex, count, comparisonType);
            if (r == -1)
                return secondSource.IndexOf(value, startIndex, count, comparisonType);
            return r;
        }
        public int IndexOfAny(char[] anyOf)
        {
            return ToString().IndexOfAny(anyOf);
        }
        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return ToString().IndexOfAny(anyOf, startIndex);
        }
        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return ToString().IndexOfAny(anyOf, startIndex, count);
        }
        public string Insert(int startIndex, string value)
        {
            return ToString().Insert(startIndex, value);
        }
        public int LastIndexOf(char value)
        {
            var r = firstSource.LastIndexOf(value);
            if (r == -1)
                return secondSource.LastIndexOf(value);
            return r;
        }
        public int LastIndexOf(string value)
        {
            var r = firstSource.LastIndexOf(value);
            if (r == -1)
                return secondSource.LastIndexOf(value);
            return r;
        }
        public int LastIndexOf(char value, int startIndex)
        {
            var r = firstSource.LastIndexOf(value, startIndex);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex);
            return r;
        }
        public int LastIndexOf(string value, int startIndex)
        {
            var r = firstSource.LastIndexOf(value, startIndex);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex);
            return r;
        }
        public int LastIndexOf(string value, StringComparison comparisonType)
        {
            var r = firstSource.LastIndexOf(value, comparisonType);
            if (r == -1)
                return secondSource.LastIndexOf(value, comparisonType);
            return r;
        }
        public int LastIndexOf(char value, int startIndex, int count)
        {
            var r = firstSource.LastIndexOf(value, startIndex, count);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex, count);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, int count)
        {
            var r = firstSource.LastIndexOf(value, startIndex, count);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex, count);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            var r = firstSource.LastIndexOf(value, startIndex, comparisonType);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex, comparisonType);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            var r = firstSource.LastIndexOf(value, startIndex, count, comparisonType);
            if (r == -1)
                return secondSource.LastIndexOf(value, startIndex, count, comparisonType);
            return r;
        }
        public int LastIndexOfAny(char[] anyOf)
        {
            return ToString().LastIndexOfAny(anyOf);
        }
        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            return ToString().LastIndexOfAny(anyOf, startIndex);
        }
        public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return ToString().LastIndexOfAny(anyOf, startIndex, count);
        }
        public string PadLeft(int totalWidth)
        {
            return ToString().PadLeft(totalWidth);
        }
        public string PadLeft(int totalWidth, char paddingChar)
        {
            return ToString().PadLeft(totalWidth, paddingChar);
        }
        public string PadRight(int totalWidth)
        {
            return ToString().PadRight(totalWidth);
        }
        public string PadRight(int totalWidth, char paddingChar)
        {
            return ToString().PadRight(totalWidth, paddingChar);
        }
        public string Remove(int startIndex)
        {
            return ToString().Remove(startIndex);
        }
        public string Remove(int startIndex, int count)
        {
            return ToString().Remove(startIndex, count);
        }
        public string Replace(char oldChar, char newChar)
        {
            return ToString().Replace(oldChar, newChar);
        }
        public string Replace(string oldValue, string newValue)
        {
            return ToString().Replace(oldValue, newValue);
        }
        public string[] Split(params char[] separator)
        {
            return ToString().Split(separator);
        }
        public string[] Split(char[] separator, int count)
        {
            return ToString().Split(separator, count);
        }
        public string[] Split(char[] separator, StringSplitOptions options)
        {
            return ToString().Split(separator, options);
        }
        public string[] Split(string[] separator, StringSplitOptions options)
        {
            return ToString().Split(separator, options);
        }
        public string[] Split(char[] separator, int count, StringSplitOptions options)
        {
            return ToString().Split(separator, count, options);
        }
        public string[] Split(string[] separator, int count, StringSplitOptions options)
        {
            return ToString().Split(separator, count, options);
        }
        public bool StartsWith(string value)
        {
            return ToString().StartsWith(value);
        }
        public bool StartsWith(string value, StringComparison comparisonType)
        {
            return ToString().StartsWith(value, comparisonType);
        }
        public string Substring(int startIndex)
        {
            return ToString().Substring(startIndex);
        }
        public string Substring(int startIndex, int length)
        {
            return ToString().Substring(startIndex, length);
        }
        public char[] ToCharArray()
        {
            return ToString().ToCharArray();
        }
        public char[] ToCharArray(int startIndex, int length)
        {
            return ToString().ToCharArray(startIndex, length);
        }
        public string ToLower()
        {
            return ToString().ToLower();
        }
        public string ToLowerInvariant()
        {
            return ToString().ToLowerInvariant();
        }

        private static void _append(StringBuilder sb, object arg)
        {
            var str = arg.ToString();
            var start = sb.Length;
            if (sb.Capacity < start + str.Length)
                sb.EnsureCapacity(System.Math.Max(sb.Capacity << 1, start + str.Length));
            sb.Length += str.Length;
            for (var i = 0; i < str.Length; i++)
            {
                sb[start + i] = str[i];
            }
        }

        public override string ToString()
        {
            if (_secondSource != null)
            {
                if (!(_firstSource is RopeString)
                    && !(_secondSource is RopeString))
                {
                    _firstSource = firstSource + secondSource;
                    _secondSource = null;
                }
                else
                {
                    var stack = new Stack<RopeString>();
                    var step = new Stack<int>();
                    var res = new StringBuilder(Length);
                    stack.Push(this);
                    step.Push(0);
                    while (stack.Count != 0)
                    {
                        if (step.Peek() < 1)
                        {
                            if (stack.Peek()._firstSource is RopeString)
                            {
                                var child = stack.Peek()._firstSource as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(1);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().firstSource ?? "");
                                step.Pop();
                                step.Push(1);
                            }
                        }

                        if (step.Peek() < 2)
                        {
                            if (stack.Peek()._secondSource is RopeString)
                            {
                                var child = stack.Peek()._secondSource as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(2);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().secondSource ?? "");
                                step.Pop();
                                step.Push(2);
                            }
                        }

                        stack.Pop();
                        step.Pop();
                    }

                    _firstSource = res.ToString();
                    _secondSource = null;
                }
            }
            return firstSource;
        }
        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }
        public string ToUpper()
        {
            return ToString().ToUpper();
        }
        public string ToUpperInvariant()
        {
            return ToString().ToUpperInvariant();
        }
        public string Trim()
        {
            return ToString().Trim();
        }
        public string Trim(params char[] trimChars)
        {
            return ToString().Trim(trimChars);
        }
        public string TrimEnd(params char[] trimChars)
        {
            return ToString().TrimEnd(trimChars);
        }
        public string TrimStart(params char[] trimChars)
        {
            return ToString().TrimStart(trimChars);
        }

        public static bool operator ==(RopeString left, RopeString right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null)
                || ReferenceEquals(right, null))
                return false;

            return left.ToString() == right.ToString();
        }

        public static bool operator !=(RopeString left, RopeString rigth)
        {
            return !(left == rigth);
        }

        public static RopeString operator +(RopeString left, RopeString rigth)
        {
            return new RopeString(left, rigth);
        }

        public static RopeString operator +(RopeString left, string rigth)
        {
            return new RopeString(left, rigth);
        }

        public static RopeString operator +(string left, RopeString rigth)
        {
            return new RopeString(left, rigth);
        }

        #region Члены IEnumerable<char>

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<char>).GetEnumerator();
        }

        #endregion

        private int calcLength()
        {
            //return ToString().Length;

            int res = 0;
            if (_firstSource != null)
            {
                var rs = _firstSource as RopeString;
                if (rs != null)
                {
                    res = rs.Length;
                }
                else
                {
                    var sb = _firstSource as StringBuilder;
                    if (sb != null)
                        res = sb.Length;
                    else
                        res = firstSource.Length;
                }
            }

            if (_secondSource != null)
            {
                var rs = _secondSource as RopeString;
                if (rs != null)
                {
                    res += rs.Length;
                }
                else
                {
                    var sb = _secondSource as StringBuilder;
                    if (sb != null)
                        res += sb.Length;
                    else
                        res += secondSource.Length;
                }
            }

            return res;
        }
    }
}
