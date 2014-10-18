using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NiL.JS.Core
{
    public sealed class RopeString : IComparable, ICloneable, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
    {
        private object _firstSource;
        private object _secondSource;
        private string firstSource
        {
            get
            {
                return _firstSource as string ?? (_firstSource = _firstSource.ToString()) as string;
            }
        }
        private string secondSource
        {
            get
            {
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
            _firstSource = source ?? "";
            _secondSource = "";
        }

        public RopeString(object firstSource, object secondSource)
        {
            this._firstSource = firstSource ?? "";
            this._secondSource = secondSource ?? "";
        }

        public char this[int index]
        {
            get
            {
                if (firstSource.ToString().Length < index)
                    return firstSource[index];
                return secondSource[index];
            }
        }

        public int Length
        {
            get
            {
                return firstSource.Length + secondSource.Length;
            }
        }

        public object Clone()
        {
            return new RopeString(firstSource.Clone(), secondSource.Clone());
        }
        public int CompareTo(object value)
        {
            return ToString().CompareTo(value);
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
        public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
        {
            return ToString().EndsWith(value, ignoreCase, culture);
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
        public CharEnumerator GetEnumerator()
        {
            return ToString().GetEnumerator();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public TypeCode GetTypeCode()
        {
            return ToString().GetTypeCode();
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
        public bool IsNormalized()
        {
            return ToString().IsNormalized();
        }
        public bool IsNormalized(NormalizationForm normalizationForm)
        {
            return ToString().IsNormalized(normalizationForm);
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
        public string Normalize()
        {
            return ToString().Normalize();
        }
        public string Normalize(NormalizationForm normalizationForm)
        {
            return ToString().Normalize(normalizationForm);
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
        public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
        {
            return ToString().StartsWith(value, ignoreCase, culture);
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
        public string ToLower(CultureInfo culture)
        {
            return ToString().ToLower(culture);
        }
        public string ToLowerInvariant()
        {
            return ToString().ToLowerInvariant();
        }

        internal StringBuilder toString(bool forceCreate)
        {
            if (_firstSource is RopeString)
                return (_firstSource as RopeString).toString(true).Append(secondSource);
            else if (_secondSource is RopeString)
                return (_secondSource as RopeString).toString(true).Insert(0, firstSource);
            else
                return forceCreate ? new StringBuilder().Append(firstSource).Append(secondSource) : null;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(secondSource))
            {
                if (_firstSource is RopeString
                    || _secondSource is RopeString)
                    _firstSource = (toString(false) as object ?? (firstSource + secondSource)).ToString();
                else
                    _firstSource = firstSource + secondSource;
                _secondSource = "";
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
        public string ToUpper(CultureInfo culture)
        {
            return ToString().ToUpper(culture);
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
            return GetEnumerator();
        }

        #endregion
    }
}
