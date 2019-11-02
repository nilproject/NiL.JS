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
        private object _firstPart;
        private object _secondPart;
        private string firstPart
        {
            get
            {
                if (_firstPart == null)
                    return null;

                return _firstPart as string ?? (_firstPart = _firstPart.ToString()) as string;
            }
        }
        private string secondPart
        {
            get
            {
                if (_secondPart == null)
                    return null;

                return _secondPart as string ?? (_secondPart = _secondPart.ToString()) as string;
            }
        }

        public RopeString()
        {
            _firstPart = "";
            _secondPart = "";
        }

        public RopeString(object source)
        {
            _firstPart = source ?? "" as object;
            _secondPart = "";
        }

        public RopeString(object firstSource, object secondSource)
        {
            _firstPart = firstSource ?? "" as object;
            _secondPart = secondSource ?? "" as object;

            _length = calcLength();

            if (_length < 0)
                ExceptionHelper.Throw(new RangeError("String is too large"));
        }

        public char this[int index]
        {
            get
            {
                if (_firstPart != null)
                {
                    if ((_firstPart is RopeString) && (_firstPart as RopeString).Length < index)
                        return (_firstPart as RopeString)[index];
                    if ((_firstPart is StringBuilder) && (_firstPart as StringBuilder).Length < index)
                        return (_firstPart as StringBuilder)[index];
                    if (firstPart.Length < index)
                        return firstPart[index];
                }

                if (_secondPart != null)
                {
                    if (_secondPart is RopeString)
                        return (_secondPart as RopeString)[index];
                    if (_secondPart is StringBuilder)
                        return (_secondPart as StringBuilder)[index];
                    return secondPart[index];
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
            return new RopeString(_firstPart, _secondPart);
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
            var r = firstPart.IndexOf(value);
            if (r == -1)
                return secondPart.IndexOf(value);
            return r;
        }
        public int IndexOf(string value)
        {
            var r = firstPart.IndexOf(value);
            if (r == -1)
                return secondPart.IndexOf(value);
            return r;
        }
        public int IndexOf(char value, int startIndex)
        {
            var r = firstPart.IndexOf(value, startIndex);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex);
            return r;
        }
        public int IndexOf(string value, int startIndex)
        {
            var r = firstPart.IndexOf(value, startIndex);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex);
            return r;
        }
        public int IndexOf(string value, StringComparison comparisonType)
        {
            var r = firstPart.IndexOf(value, comparisonType);
            if (r == -1)
                return secondPart.IndexOf(value, comparisonType);
            return r;
        }
        public int IndexOf(char value, int startIndex, int count)
        {
            var r = firstPart.IndexOf(value, startIndex, count);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex, count);
            return r;
        }
        public int IndexOf(string value, int startIndex, int count)
        {
            var r = firstPart.IndexOf(value, startIndex, count);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex, count);
            return r;
        }
        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            var r = firstPart.IndexOf(value, startIndex, comparisonType);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex, comparisonType);
            return r;
        }
        public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            var r = firstPart.IndexOf(value, startIndex, count, comparisonType);
            if (r == -1)
                return secondPart.IndexOf(value, startIndex, count, comparisonType);
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
            var r = firstPart.LastIndexOf(value);
            if (r == -1)
                return secondPart.LastIndexOf(value);
            return r;
        }
        public int LastIndexOf(string value)
        {
            var r = firstPart.LastIndexOf(value);
            if (r == -1)
                return secondPart.LastIndexOf(value);
            return r;
        }
        public int LastIndexOf(char value, int startIndex)
        {
            var r = firstPart.LastIndexOf(value, startIndex);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex);
            return r;
        }
        public int LastIndexOf(string value, int startIndex)
        {
            var r = firstPart.LastIndexOf(value, startIndex);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex);
            return r;
        }
        public int LastIndexOf(string value, StringComparison comparisonType)
        {
            var r = firstPart.LastIndexOf(value, comparisonType);
            if (r == -1)
                return secondPart.LastIndexOf(value, comparisonType);
            return r;
        }
        public int LastIndexOf(char value, int startIndex, int count)
        {
            var r = firstPart.LastIndexOf(value, startIndex, count);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex, count);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, int count)
        {
            var r = firstPart.LastIndexOf(value, startIndex, count);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex, count);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            var r = firstPart.LastIndexOf(value, startIndex, comparisonType);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex, comparisonType);
            return r;
        }
        public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            var r = firstPart.LastIndexOf(value, startIndex, count, comparisonType);
            if (r == -1)
                return secondPart.LastIndexOf(value, startIndex, count, comparisonType);
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
            if (_secondPart != null)
            {
                if (!(_firstPart is RopeString)
                    && !(_secondPart is RopeString))
                {
                    _firstPart = firstPart + secondPart;
                    _secondPart = null;
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
                            if (stack.Peek()._firstPart is RopeString)
                            {
                                var child = stack.Peek()._firstPart as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(1);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().firstPart ?? "");
                                step.Pop();
                                step.Push(1);
                            }
                        }

                        if (step.Peek() < 2)
                        {
                            if (stack.Peek()._secondPart is RopeString)
                            {
                                var child = stack.Peek()._secondPart as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(2);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().secondPart ?? "");
                                step.Pop();
                                step.Push(2);
                            }
                        }

                        stack.Pop();
                        step.Pop();
                    }

                    _firstPart = res.ToString();
                    _secondPart = null;
                }
            }
            return firstPart;
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
            if (_firstPart != null)
            {
                var rs = _firstPart as RopeString;
                if (rs != null)
                {
                    res = rs.Length;
                }
                else
                {
                    var sb = _firstPart as StringBuilder;
                    if (sb != null)
                        res = sb.Length;
                    else
                        res = firstPart.Length;
                }
            }

            if (_secondPart != null)
            {
                var rs = _secondPart as RopeString;
                if (rs != null)
                {
                    res += rs.Length;
                }
                else
                {
                    var sb = _secondPart as StringBuilder;
                    if (sb != null)
                        res += sb.Length;
                    else
                        res += secondPart.Length;
                }
            }

            return res;
        }
    }
}
