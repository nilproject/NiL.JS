using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Array : JSObject, IIterable
    {
        private static readonly SparseArray<JSValue> emptyData = new SparseArray<JSValue>();
        [Hidden]
        internal SparseArray<JSValue> _data;

        [DoNotEnumerate]
        public Array()
        {
            _oValue = this;
            _valueType = JSValueType.Object;
            _data = new SparseArray<JSValue>();
            _attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(int length)
            : this((double)length)
        {
        }

        internal Array(long length)
        {
            if (length < 0 || length > uint.MaxValue)
                ExceptionHelper.Throw((new RangeError("Invalid array length.")));

            _oValue = this;
            _valueType = JSValueType.Object;
            _data = new SparseArray<JSValue>((int)System.Math.Min(100000, length));
            _attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(double length)
        {
            if (((long)length != length) || (length < 0) || (length > 0xffffffff))
                ExceptionHelper.Throw((new RangeError("Invalid array length.")));

            _oValue = this;
            _valueType = JSValueType.Object;
            _data = new SparseArray<JSValue>();
            _attributes |= JSValueAttributesInternal.SystemObject;

            if (length > 0)
                _data[(int)((uint)length - 1)] = null;
        }

        internal Array(JSValue[] data)
        {
            _oValue = this;
            _valueType = JSValueType.Object;
            _data = new SparseArray<JSValue>(data);
            _attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            _oValue = this;
            _valueType = JSValueType.Object;
            _data = new SparseArray<JSValue>();
            _attributes |= JSValueAttributesInternal.SystemObject;

            for (var i = 0; i < args.length; i++)
                _data[i] = args[i].CloneImpl(false);
        }

        [Hidden]
        public Array(ICollection source)
            : this(source as IEnumerable)
        {

        }

        [Hidden]
        public Array(IEnumerable source)
            : this(source == null ? null : source.GetEnumerator())
        {

        }

        [Hidden]
        internal Array(IEnumerator source)
        {
            _oValue = this;
            _valueType = JSValueType.Object;
            if (source == null)
                throw new ArgumentNullException("enumerator");
            _data = new SparseArray<JSValue>();
            var index = 0;
            while (source.MoveNext())
            {
                var e = source.Current;
                _data[index++] = (e as JSValue ?? Context.CurrentGlobalContext.ProxyValue(e)).CloneImpl(false);
            }
            _attributes |= JSValueAttributesInternal.SystemObject;
        }

        [Hidden]
        public void Add(JSValue obj)
        {
            _data.Add(obj);
        }

        private LengthField _lengthObj;
        [Hidden]
        public JSValue length
        {
            [Hidden]
            get
            {
                if (_lengthObj == null)
                    _lengthObj = new LengthField(this);

                if (_data.Length <= int.MaxValue)
                {
                    _lengthObj._iValue = (int)_data.Length;
                    _lengthObj._valueType = JSValueType.Integer;
                }
                else
                {
                    _lengthObj._dValue = _data.Length;
                    _lengthObj._valueType = JSValueType.Double;
                }
                return _lengthObj;
            }
        }

        [Hidden]
        internal bool SetLenght(long nlen)
        {
            if (_data.Length == nlen)
                return true;
            if (nlen < 0)
                ExceptionHelper.Throw(new RangeError("Invalid array length"));
            if (_data.Length > nlen)
            {
                var res = true;
                foreach (var element in _data.ReversOrder)
                {
                    if ((uint)element.Key < nlen)
                        break;
                    if (element.Value != null
                        && element.Value.Exists
                        && (element.Value._attributes & JSValueAttributesInternal.DoNotDelete) != 0)
                    {
                        nlen = element.Key;
                        res = false;
                    }
                }
                if (!res)
                {
                    SetLenght(nlen + 1);
                    return false;
                }
            }
            while (_data.Length > nlen)
            {
                _data.RemoveAt((int)(_data.Length - 1));
                _data.Trim();
            }
            if (_data.Length != nlen)
                _data[(int)nlen - 1] = _data[(int)nlen - 1];
            return true;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue concat(JSValue self, Arguments args)
        {
            Array res = null;
            var lenObj = self?.GetProperty("length", PropertyScope.Own) ?? undefined;
            if (!lenObj.Defined)
            {
                if (self._valueType < JSValueType.Object)
                    self = self.ToObject();
                res = new Array() { self };
            }
            else
            {
                res = Tools.arraylikeToArray(self, true, true, false, -1);
            }

            if (args != null)
            {
                for (var i = 0; i < args.length; i++)
                {
                    var v = args[i];
                    var varr = v._oValue as Array;
                    if (varr != null)
                    {
                        varr = Tools.arraylikeToArray(varr, true, false, false, -1);
                        for (var ai = 0; ai < varr._data.Length; ai++)
                        {
                            var item = varr._data[ai];
                            res._data.Add(item);
                        }
                    }
                    else
                    {
                        res._data.Add(v.CloneImpl(false));
                    }
                }
            }

            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue copyWithin(JSValue self, Arguments args)
        {
            if (self == null
             || self.IsNull
             || self.IsUndefined())
                ExceptionHelper.ThrowTypeError("this is null or undefined");

            var length = Tools.getLengthOfArraylike(self, false);

            var target = Tools.JSObjectToInt64(args?[0] ?? undefined, 0, true);
            if (target < 0)
                target += length;
            if (target < 0)
                target = 0;
            if (target > length)
                target = length;

            var start = Tools.JSObjectToInt64(args?[1] ?? undefined, 0, true);
            if (start < 0)
                start += length;
            if (start < 0)
                start = 0;
            if (start > length)
                start = length;

            var end = Tools.JSObjectToInt64(args?[2] ?? undefined, length, true);
            if (end < 0)
                end += length;
            if (end < 0)
                end = 0;
            if (end > length)
                end = length;

            if (start == target
                || self._valueType < JSValueType.Object)
                return self.ToObject();

            var direction = System.Math.Sign(start - target);
            var fromIndex = System.Math.Min(start * direction, target * direction) * direction;
            var count = System.Math.Min(end - start, length - target);
            var modifier = (count - 1) * (-(direction - 1) / 2);

            var array = self.Value as Array;
            if (array != null)
            {
                for (long from = start + modifier, to = target + modifier; count != 0; from += direction, to += direction, count--)
                {
                    array._data[(int)to] = array._data[(int)from];
                }
            }
            else
            {
                var key = new JSValue();
                for (long from = start + modifier, to = target + modifier; count != 0; from += direction, to += direction, count--)
                {
                    if ((int)from == from)
                    {
                        key._iValue = (int)from;
                        key._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        key._dValue = (int)from;
                        key._valueType = JSValueType.Double;
                    }

                    var value = Tools.InvokeGetter(self.GetProperty(key, false, PropertyScope.Own), self);

                    if ((int)to == to)
                    {
                        key._iValue = (int)to;
                        key._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        key._dValue = to;
                        key._valueType = JSValueType.Double;
                    }

                    if (value.Exists)
                        self.SetProperty(key, value, true);
                    else
                        self.DeleteProperty(key);
                }
            }

            return self;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(2)]
        public static JSValue fill(JSValue self, Arguments args)
        {
            if (self == null
             || self.IsNull
             || self.IsUndefined())
                ExceptionHelper.ThrowTypeError("this is null or undefined");

            var length = Tools.getLengthOfArraylike(self, false);

            var value = args?[0] ?? undefined;

            var start = Tools.JSObjectToInt64(args[1], 0, true);
            if (start < 0)
                start += length;
            if (start < 0)
                start = 0;
            if (start > length)
                start = length;

            var end = Tools.JSObjectToInt64(args[2], length, true);
            if (end < 0)
                end += length;
            if (end < 0)
                end = 0;
            if (end > length)
                end = length;

            var array = self.Value as Array;
            if (array != null)
            {
                for (var i = start; i < end; i++)
                {
                    array._data[(int)i] = value.CloneImpl(false);
                }
            }
            else
            {
                var key = new JSValue();
                for (var i = start; i < end; i++)
                {
                    if ((int)i == i)
                    {
                        key._iValue = (int)i;
                        key._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        key._dValue = i;
                        key._valueType = JSValueType.Double;
                    }

                    self.SetProperty(key, value, true);
                }
            }

            return self;
        }


        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue find(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = undefined;

            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                if ((bool)jsCallback.Call(thisBind, new Arguments { value, index, self }))
                {
                    result = value.CloneImpl(false);
                    return false;
                }

                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue findIndex(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = -1L;

            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                if ((bool)jsCallback.Call(thisBind, new Arguments { value, index, self }))
                {
                    result = index;
                    return false;
                }

                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue every(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = true;

            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                return result &= (bool)jsCallback.Call(thisBind, new Arguments { value, index, self });
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue some(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = true;
            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                return result &= !(bool)jsCallback.Call(thisBind, new Arguments { value, index, self });
            });

            return !result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue filter(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            Array result = new Array();

            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                if ((bool)jsCallback.Call(thisBind, new Arguments { value, index, self }))
                    result.Add(value.CloneImpl(false));

                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue map(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            Array result = new Array();

            var len = iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                result[(int)index] = jsCallback.Call(thisBind, new Arguments { value, index, self }).CloneImpl(false);

                return true;
            });

            result.SetLenght(len);

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue forEach(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                jsCallback.Call(thisBind, new Arguments { value, index, self });

                return true;
            });

            return null;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue indexOf(JSValue self, Arguments args)
        {
            if (self == null)
                self = undefined;
            var result = -1L;

            iterateImpl(self, null, args?[1] ?? undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                if (Expressions.StrictEqual.Check(args[0], value))
                {
                    result = index;
                    return false;
                }

                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue includes(JSValue self, Arguments args)
        {
            var result = -1L;

            iterateImpl(self, null, args?[1] ?? undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                if (args[0].IsNaN() ?
                        value.IsNaN()
                    :
                        Expressions.StrictEqual.Check(args[0], value))
                {
                    result = index;
                    return false;
                }

                return true;
            });

            return result != -1L;
        }

        private static long iterateImpl(JSValue self, Arguments args, JSValue startIndexSrc, JSValue endIndexSrc, bool processMissing, Func<JSValue, long, JSValue, Function, bool> callback)
        {
            Array arraySrc = self._oValue as Array;
            bool nativeMode = arraySrc != null;
            if (!self.Defined || (self._valueType >= JSValueType.Object && self._oValue == null))
            {
#if (PORTABLE || NETCORE)
                ExceptionHelper.Throw(new TypeError("Trying to call method for for null or undefined"));
#else
                var stackTrace = new System.Diagnostics.StackTrace();
                ExceptionHelper.Throw(new TypeError("Can not call Array.prototype." + stackTrace.GetFrame(stackTrace.FrameCount - 2).GetMethod().Name + " for null or undefined"));
#endif
            }

            var length = nativeMode ? arraySrc._data.Length : Tools.getLengthOfArraylike(self, false);
            long startIndex = 0;
            long endIndex = 0;
            Function jsCallback = null;
            JSValue thisBind = null;

            if (args != null)
            {
                // forEach, map, filter, every, some, reduce
                jsCallback = args[0] == null ? null : args[0]._oValue as Function;
                if (jsCallback == null)
                    ExceptionHelper.Throw(new TypeError("Callback is not a function."));

                thisBind = args.Length > 1 ? args[1] : null;
            }
            else if (startIndexSrc.Exists)
            {
                // indexOf, slice
                startIndex = Tools.JSObjectToInt64(startIndexSrc, 0, true);
                if (startIndex > length)
                    startIndex = length;
                if (startIndex < 0)
                    startIndex += length;
                if (startIndex < 0)
                    startIndex = 0;

                endIndex = Tools.JSObjectToInt64(endIndexSrc, long.MaxValue, true);
                if (endIndex > length)
                    endIndex = length;
                if (endIndex < 0)
                    endIndex += length;
                if (endIndex < 0)
                    endIndex = 0;

                if (length > endIndex)
                    length = endIndex;
            }

            if (length > 0)
            {
                if (!nativeMode)
                {
                    long prevKey = startIndex - 1;
                    var source = self;
                    while (source != null && !source.IsNull && source.Defined)
                    {
                        for (var enumerator = source.GetEnumerator(false, EnumerationMode.RequireValues); enumerator.MoveNext();)
                        {
                            var item = enumerator.Current;
                            long index;
                            if (long.TryParse(item.Key, out index))
                            {
                                if (index >= length)
                                {
                                    prevKey = index;
                                    break;
                                }

                                if (index - prevKey > 1)
                                {
                                    for (var i = prevKey + 1; i < index; i++)
                                    {
                                        var tempKey = new JSValue();
                                        if (i <= int.MaxValue)
                                        {
                                            tempKey._iValue = (int)i;
                                            tempKey._valueType = JSValueType.Integer;
                                        }
                                        else
                                        {
                                            tempKey._dValue = i;
                                            tempKey._valueType = JSValueType.Double;
                                        }
                                        var value = source.GetProperty(tempKey, false, PropertyScope.Common);
                                        if (processMissing || value.Exists)
                                        {
                                            if (!callback(Tools.InvokeGetter(value, self), i, thisBind, jsCallback))
                                                return length;
                                        }
                                    }
                                }
                                else if (index <= prevKey)
                                    continue;

                                if (!callback(Tools.InvokeGetter(item.Value, self), index, thisBind, jsCallback))
                                    return length;

                                prevKey = index;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (length - prevKey <= 1)
                            break;

                        source = source.__proto__;
                    }
                }
                else
                {
                    var tempKey = new JSValue();
                    var prevKey = startIndex - 1;
                    var mainEnum = arraySrc._data.DirectOrder.GetEnumerator();
                    var moved = true;
                    while (moved)
                    {
                        moved = mainEnum.MoveNext();
                        while (moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.Exists))
                            moved = mainEnum.MoveNext();

                        var element = mainEnum.Current;
                        var index = (long)(uint)element.Key;

                        if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.Exists))
                            index = length;

                        var value = element.Value;
                        if (index - prevKey > 1 || (!moved && index < length))
                        {
                            if (!moved)
                                index = length;

                            for (var i = prevKey + 1; i < index; i++)
                            {
                                if (i <= int.MaxValue)
                                {
                                    tempKey._iValue = (int)i;
                                    tempKey._valueType = JSValueType.Integer;
                                }
                                else
                                {
                                    tempKey._dValue = i;
                                    tempKey._valueType = JSValueType.Double;
                                }

                                value = self.GetProperty(tempKey, false, PropertyScope.Common);
                                if (processMissing || value.Exists)
                                {
                                    if (!callback(Tools.InvokeGetter(value, self), i, thisBind, jsCallback))
                                        return length;
                                }
                            }

                            value = element.Value;
                        }

                        if (index <= prevKey)
                            continue;

                        prevKey = index;

                        if (index >= length || (!moved))
                            break;

                        if (value == null || !value.Exists)
                            continue;

                        value = Tools.InvokeGetter(value, self);

                        if (!callback(value, index, thisBind, jsCallback))
                            return length;
                    }
                }
            }

            return length;
        }

        private static long reverseIterateImpl(JSValue self, Arguments args, JSValue startIndexSrc, Func<JSValue, long, JSValue, Function, bool> callback)
        {
            Array arraySrc = self._oValue as Array;
            bool nativeMode = arraySrc != null;
            if (!self.Defined || (self._valueType >= JSValueType.Object && self._oValue == null))
            {
#if (PORTABLE || NETCORE)
                ExceptionHelper.Throw(new TypeError("Trying to call method for for null or undefined"));
#else
                var stackTrace = new System.Diagnostics.StackTrace();
                ExceptionHelper.Throw(new TypeError("Can not call Array.prototype." + stackTrace.GetFrame(stackTrace.FrameCount - 2).GetMethod().Name + " for null or undefined"));
#endif
            }

            var length = nativeMode ? arraySrc._data.Length : Tools.getLengthOfArraylike(self, false);
            long startIndex = length - 1;
            Function jsCallback = null;
            JSValue thisBind = null;

            if (args != null)
            {
                // reduceRight
                jsCallback = args[0] == null ? null : args[0]._oValue as Function;
                if (jsCallback == null)
                    ExceptionHelper.Throw(new TypeError("Callback is not a function."));

                thisBind = args.Length > 1 ? args[1] : null;
            }
            else if (startIndexSrc.Exists)
            {
                // lastIndexOf
                startIndex = Tools.JSObjectToInt64(startIndexSrc, 0, true);
                if (startIndex > length)
                    startIndex = length;
                if (startIndex < 0)
                    startIndex += length;
                if (startIndex < 0)
                    startIndex = -1;
            }

            if (!nativeMode)
            {
                for (var i = startIndex; i >= 0; i--)
                {
                    var tempKey = new JSValue();
                    if (i <= int.MaxValue)
                    {
                        tempKey._iValue = (int)i;
                        tempKey._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        tempKey._dValue = i;
                        tempKey._valueType = JSValueType.Double;
                    }

                    var value = self.GetProperty(tempKey, false, PropertyScope.Common);
                    if (value.Exists)
                    {
                        if (!callback(Tools.InvokeGetter(value, self), i, thisBind, jsCallback))
                            return length;
                    }
                }
            }
            else
            {
                long prevKey = startIndex + 1;
                var mainEnum = arraySrc._data.ReversOrder.GetEnumerator();
                bool moved = true;
                while (moved)
                {
                    moved = mainEnum.MoveNext();

                    while (moved && ((uint)mainEnum.Current.Key > startIndex || mainEnum.Current.Value == null || !mainEnum.Current.Value.Exists))
                        moved = mainEnum.MoveNext();

                    var element = mainEnum.Current;
                    var index = (long)(uint)element.Key;

                    if (!moved && (mainEnum.Current.Value == null || !mainEnum.Current.Value.Exists))
                        index = 0;

                    var value = element.Value;
                    if (prevKey - index > 1 || (!moved && prevKey > 0))
                    {
                        if (!moved)
                            index = -1;

                        for (var i = prevKey - 1; i > index; i--)
                        {
                            var tempKey = new JSValue();
                            if (i <= int.MaxValue)
                            {
                                tempKey._iValue = (int)i;
                                tempKey._valueType = JSValueType.Integer;
                            }
                            else
                            {
                                tempKey._dValue = i;
                                tempKey._valueType = JSValueType.Double;
                            }

                            value = self.GetProperty(tempKey, false, PropertyScope.Common);
                            if (value.Exists)
                            {
                                if (!callback(Tools.InvokeGetter(value, self), i, thisBind, jsCallback))
                                    return length;
                            }
                        }

                        value = element.Value;
                    }

                    prevKey = index;

                    if (index >= length || !moved)
                        break;

                    if (value == null || !value.Exists)
                        continue;

                    value = Tools.InvokeGetter(value, self);

                    if (!callback(value, index, thisBind, jsCallback))
                        return length;
                }
            }

            return length;
        }

        [DoNotEnumerate]
        public static JSValue isArray(Arguments args)
        {
            if (args == null)
            {
                ExceptionHelper.ThrowArgumentNull("args");
                return null;
            }

            return args[0].Value is Array || args[0].Value == Context.CurrentGlobalContext.GetPrototype(typeof(Array));
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue join(JSValue self, Arguments args)
        {
            if (self == null || self._valueType <= JSValueType.Undefined || (self._valueType >= JSValueType.Object && self.Value == null))
                ExceptionHelper.Throw(new TypeError("Array.prototype.join called for null or undefined"));

            return joinImpl(self, args == null || args.length == 0 || !args[0].Defined ? "," : args[0].ToString(), false);
        }

        private static string joinImpl(JSValue self, string separator, bool locale)
        {
            var selfA = self.Value as Array;
            selfA = selfA ?? Tools.arraylikeToArray(self, true, false, false, -1);

            var _data = selfA._data;

            if (_data == null || _data.Length == 0)
                return "";

            if ((_data.Length - 1) * separator.Length > int.MaxValue)
                ExceptionHelper.Throw(new RangeError("The array is too big"));

            selfA._data = emptyData;

            var sb = new System.Text.StringBuilder((int)((_data.Length - 1) * separator.Length));
            JSValue t, temp = 0;

            for (long i = 0; i < _data.Length; i++)
            {
                if (i > 0)
                    sb.Append(separator);

                int index = unchecked((int)i);
                t = _data[index];

                if (t == null || !t.Exists)
                {
                    if (i <= int.MaxValue)
                    {
                        temp._iValue = index;
                        temp._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        temp._dValue = i;
                        temp._valueType = JSValueType.Double;
                    }
                    t = self.GetProperty(temp, false, PropertyScope.Common);
                }

                if (t != null && t.Defined)
                {
                    if (t._valueType == JSValueType.String)
                        sb.Append(t.ToString());
                    else if (t._valueType < JSValueType.String || t._oValue != null)
                        sb.Append(locale ? t.ToPrimitiveValue_LocaleString_Value() : t.ToPrimitiveValue_String_Value());
                }
            }

            selfA._data = _data;

            return sb.ToString();
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue lastIndexOf(JSValue self, Arguments args)
        {
            var result = -1L;

            reverseIterateImpl(self, null, args?[1] ?? undefined, (value, index, thisBind, jsCallback) =>
            {
                if (Expressions.StrictEqual.Check(args[0], value))
                {
                    result = index;
                    return false;
                }

                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        public static JSValue of(Arguments args)
        {
            if (args == null || args.Length == 0)
                return new Array();

            return new Array(args);
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue pop(JSValue self)
        {
            notExists._valueType = JSValueType.NotExistsInObject;
            var selfa = self as Array;
            if (selfa != null)
            {
                if (selfa._data.Length == 0)
                    return notExists;
                int newLen = (int)(selfa._data.Length - 1);
                var res = selfa._data[newLen] ?? self[newLen.ToString()];
                if (res._valueType == JSValueType.Property)
                    res = ((res._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null);
                selfa._data.RemoveAt(newLen);
                selfa._data[newLen - 1] = selfa._data[newLen - 1];
                return res;
            }
            else
            {
                var length = Tools.getLengthOfArraylike(self, true);
                if (length <= 0 || length > uint.MaxValue)
                    return notExists;
                length--;
                var tres = self.GetProperty(length.ToString(), true, PropertyScope.Common);
                JSValue res;
                if (tres._valueType == JSValueType.Property)
                    res = ((tres._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null);
                else
                    res = tres.CloneImpl(false);
                if ((tres._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                {
                    tres._oValue = null;
                    tres._valueType = JSValueType.NotExistsInObject;
                }
                self["length"] = length;
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue push(JSValue self, Arguments args)
        {
            notExists._valueType = JSValueType.NotExistsInObject;
            var selfa = self as Array;
            if (selfa != null)
            {
                if (args != null)
                {
                    for (var i = 0; i < args.length; i++)
                    {
                        if (selfa._data.Length == uint.MaxValue)
                        {
                            if (selfa._fields == null)
                                selfa._fields = getFieldsContainer();
                            selfa._fields[uint.MaxValue.ToString()] = args[0].CloneImpl(false);
                            ExceptionHelper.Throw(new RangeError("Invalid length of array"));
                        }
                        selfa._data.Add(args[i].CloneImpl(false));
                    }
                }

                return selfa.length;
            }
            else
            {
                var length = (long)Tools.getLengthOfArraylike(self, false);
                if (args != null)
                {
                    var index = length;
                    length += args.length;
                    self["length"] = length;
                    for (var j = 0; index < length; index++, j++)
                        self[index.ToString()] = args[j].CloneImpl(false);
                }

                return length;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue reverse(JSValue self)
        {
            Arguments args = null;
            var selfa = self as Array;
            if (selfa != null)
            {
                for (var i = selfa._data.Length >> 1; i-- > 0;)
                {
                    var item0 = selfa._data[(int)(selfa._data.Length - 1 - i)];
                    var item1 = selfa._data[(int)(i)];
                    JSValue value0, value1;
                    if (item0 == null || !item0.Exists)
                        item0 = selfa.__proto__[(selfa._data.Length - 1 - i).ToString()];
                    if (item0._valueType == JSValueType.Property)
                        value0 = ((item0._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value0 = item0;
                    if (item1 == null || !item1.Exists)
                        item1 = selfa.__proto__[i.ToString()];
                    if (item1._valueType == JSValueType.Property)
                        value1 = ((item1._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value1 = item1;
                    if (item0._valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args[0] = item1;
                        ((item0._oValue as PropertyPair).setter ?? Function.Empty).Call(self, args);
                    }
                    else if (value1.Exists)
                        selfa._data[(int)(selfa._data.Length - 1 - i)] = value1;
                    else
                        selfa._data[(int)(selfa._data.Length - 1 - i)] = null;

                    if (item1._valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args[0] = item0;
                        ((item1._oValue as PropertyPair).setter ?? Function.Empty).Call(self, args);
                    }
                    else if (value0.Exists)
                        selfa._data[(int)i] = value0;
                    else
                        selfa._data[(int)i] = null;
                }
                return self;
            }
            else
            {
                var length = Tools.getLengthOfArraylike(self, false);
                for (var i = 0; i < (length >> 1); i++)
                {
                    JSValue i0 = i.ToString();
                    JSValue i1 = (length - 1 - i).ToString();
                    var item0 = self.GetProperty(i0, false, PropertyScope.Common);
                    var item1 = self.GetProperty(i1, false, PropertyScope.Common);
                    var value0 = item0;
                    var value1 = item1;
                    if (value0._valueType == JSValueType.Property)
                        value0 = ((item0._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value0 = value0.CloneImpl(false);
                    if (value1._valueType == JSValueType.Property)
                        value1 = ((item1._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value1 = value1.CloneImpl(false);

                    if (item0._valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args[0] = value1;
                        ((item0._oValue as PropertyPair).setter ?? Function.Empty).Call(self, args);
                    }
                    else if (value1.Exists)
                        self.SetProperty(i0, value1, false);
                    else
                    {
                        var t = self.GetProperty(i0, true, PropertyScope.Own);
                        if ((t._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t._oValue = null;
                            t._valueType = JSValueType.NotExists;
                        }
                    }
                    if (item1._valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args[0] = value0;
                        ((item1._oValue as PropertyPair).setter ?? Function.Empty).Call(self, args);
                    }
                    else if (value0.Exists)
                        self.SetProperty(i1, value0, false);
                    else
                    {
                        var t = self.GetProperty(i1, true, PropertyScope.Own);
                        if ((t._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t._oValue = null;
                            t._valueType = JSValueType.NotExists;
                        }
                    }
                }
                return self;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue reduce(JSValue self, Arguments args)
        {
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = undefined;
            bool skip = true;

            if (args.length > 1)
            {
                skip = false;
                result = args[1];
                args[1] = null;
                args.length = 1;
            }

            var len = (skip ? 0 : 1) + iterateImpl(self, args, undefined, undefined, false, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                if (!skip)
                {
                    if (!result.Exists)
                        result = undefined;

                    result = jsCallback.Call(thisBind, new Arguments { result, value, index, self }).CloneImpl(false);
                }
                else
                {
                    result = value;
                    skip = false;
                }

                return true;
            });

            if (len == 0 || skip)
                ExceptionHelper.ThrowTypeError("Length of array cannot be 0");

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue reduceRight(JSValue self, Arguments args)
        {
            if (self._valueType < JSValueType.Object)
                self = self.ToObject();

            var result = undefined;
            bool skip = true;

            if (args.length > 1)
            {
                skip = false;
                result = args[1];
                args[1] = null;
                args.length = 1;
            }

            var len = (skip ? 0 : 1) + reverseIterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                if (!skip)
                {
                    if (!result.Exists)
                        result = undefined;

                    result = jsCallback.Call(thisBind, new Arguments { result, value, index, self }).CloneImpl(false);
                }
                else
                {
                    result = value;
                    skip = false;
                }

                return true;
            });

            if (len == 0 || skip)
                ExceptionHelper.ThrowTypeError("Length of array cannot be 0");

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(0)]
        public static JSValue shift(JSValue self)
        {
            var src = self._oValue as Array;
            if (src != null)
            {
                var res = src._data[0];
                if (res == null || !res.Exists)
                    res = src.__proto__["0"];
                if (res._valueType == JSValueType.Property)
                    res = Tools.InvokeGetter(res, self);

                JSValue prw = res;
                var length = src._data.Length;
                long prewIndex = 0;
                for (var e = src._data.DirectOrder.GetEnumerator(); ;)
                {
                    KeyValuePair<int, JSValue> element;
                    if (e.MoveNext())
                    {
                        element = e.Current;
                    }
                    else
                    {
                        if (length == 0)
                            break;
                        element = new KeyValuePair<int, JSValue>((int)length, null);
                    }

                    if (element.Key == 0)
                        continue;

                    if ((uint)element.Key < length - 1 && (element.Value == null || !element.Value.Exists))
                        continue;

                    JSValue value = null;
                    int key = 0;
                    for (; prewIndex < length && prewIndex <= (uint)element.Key; prewIndex++)
                    {
                        if (prewIndex == (uint)element.Key && element.Value != null && element.Value.Exists)
                        {
                            value = element.Value;
                            key = element.Key;
                        }
                        else
                        {
                            key = (int)prewIndex;
                            value = src.__proto__[prewIndex.ToString()];
                        }
                        if (value != null && value._valueType == JSValueType.Property)
                            value = Tools.InvokeGetter(value, self);

                        if (prw != null && prw._valueType == JSValueType.Property)
                        {
                            ((prw._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { value });
                        }
                        else
                        {
                            if (value != null)
                            {
                                if (value.Exists)
                                    src._data[key - 1] = value;
                                if (value._valueType != JSValueType.Property)
                                    src._data[key] = null;
                            }
                        }
                        prw = value;
                    }

                    if (prewIndex >= length || prewIndex < 0)
                        break;
                }

                if (length == 1)
                {
                    src._data.Clear();
                }
                else if (length > 0)
                {
                    src._data.RemoveAt((int)length - 1);
                }

                return res;
            }
            else
            {
                var lenObj = self["length"];
                if (lenObj._valueType == JSValueType.Property)
                    lenObj = Tools.InvokeGetter(lenObj, self);

                long _length = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (_length > uint.MaxValue)
                    ExceptionHelper.Throw(new RangeError("Invalid array length"));
                if (_length == 0)
                {
                    self["length"] = lenObj = _length;
                    return notExists;
                }
                var ti = new JSValue() { _valueType = JSValueType.String, _oValue = "0" };
                var t = self.GetProperty(ti, true, PropertyScope.Common);
                var res = t;
                if (res._valueType == JSValueType.Property)
                    res = Tools.InvokeGetter(res, self).CloneImpl(false);
                else
                    res = res.CloneImpl(false);
                if ((t._attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete)) == 0)
                {
                    t._oValue = null;
                    t._valueType = JSValueType.NotExists;
                }
                if (_length == 1)
                {
                    self["length"] = lenObj = _length - 1;
                    return res;
                }
                var protoSource = Tools.arraylikeToArray(self, false, true, false, -1);
                self["length"] = lenObj = _length - 1;

                List<string> keysToRemove = new List<string>();
                foreach (var item in self)
                {
                    var pindex = 0;
                    var dindex = 0.0;
                    long lindex = 0;
                    if (Tools.ParseNumber(item.Key, ref pindex, out dindex)
                        && (pindex == item.Key.Length)
                        && (lindex = (long)dindex) == dindex
                        && lindex < _length)
                    {
                        var temp = item.Value;
                        if (!temp.Exists)
                            continue;
                        if (temp._valueType != JSValueType.Property)
                            keysToRemove.Add(item.Key);
                    }
                }
                var tjo = new JSValue() { _valueType = JSValueType.String };
                for (var i = 0; i < keysToRemove.Count; i++)
                {
                    tjo._oValue = keysToRemove[i];
                    var to = self.GetProperty(tjo, true, PropertyScope.Common);
                    if ((to._attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete)) == 0)
                    {
                        to._oValue = null;
                        to._valueType = JSValueType.NotExists;
                    }
                }
                tjo._valueType = JSValueType.Integer;
                foreach (var item in protoSource._data.DirectOrder)
                {
                    if ((uint)item.Key > int.MaxValue)
                    {
                        tjo._valueType = JSValueType.Double;
                        tjo._dValue = (uint)(item.Key - 1);
                    }
                    else
                        tjo._iValue = (item.Key - 1);
                    if (item.Value != null && item.Value.Exists)
                    {
                        var temp = self.GetProperty(tjo, true, PropertyScope.Common);
                        if (temp._valueType == JSValueType.Property)
                            ((temp._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { item.Value });
                        else
                            temp.Assign(item.Value);
                    }
                }
                return res;
            }
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        [InstanceMember]
        public static JSValue slice(JSValue self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (!self.Defined || (self._valueType >= JSValueType.Object && self._oValue == null))
                ExceptionHelper.Throw(new TypeError("Can not call Array.prototype.slice for null or undefined"));

            var result = new Array();
            var index = 0L;
            iterateImpl(self, null, args[0], args[1], true, (value, itemIndex, thisBind, jsCallback) =>
            {
                if (value.Exists)
                {
                    value = value.CloneImpl(false);

                    if (index < uint.MaxValue - 1)
                        result._data[(int)index] = value;
                    else
                        result[index.ToString()] = value;
                }

                index++;
                return true;
            });

            return result;
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        [InstanceMember]
        public static JSValue splice(JSValue self, Arguments args)
        {
            return spliceImpl(self, args, true);
        }

        private static JSValue spliceImpl(JSValue self, Arguments args, bool needResult)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return needResult ? new Array() : null;
            var selfa = self as Array;
            if (selfa != null)
            {
                var _length = selfa._data.Length;
                long pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length); // double потому, что нужно "с заполнением", а не "с переполнением"
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1]._valueType <= JSValueType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                if (pos1 == 0 && args.length <= 2)
                    return needResult ? new Array() : null;
                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var res = needResult ? new Array((int)(pos1 - pos0)) : null;
                var delta = System.Math.Max(0, args.length - 2) - (pos1 - pos0);
                foreach (var node in (delta > 0 ? selfa._data.ReversOrder : selfa._data.DirectOrder))
                {
                    if (node.Key < pos0)
                        continue;
                    if (node.Key >= pos1 && delta == 0)
                        break;
                    var key = node.Key;
                    var value = node.Value;
                    if (value == null || !value.Exists)
                    {
                        value = selfa.__proto__[((uint)key).ToString()];
                        if (!value.Exists)
                            continue;
                        value = value.CloneImpl(false);
                    }
                    if (value._valueType == JSValueType.Property)
                        value = Tools.InvokeGetter(value, self).CloneImpl(false);

                    if (key < pos1)
                    {
                        if (needResult)
                            res._data[(int)(key - pos0)] = value;
                    }
                    else
                    {
                        var t = selfa._data[(int)(key + delta)];
                        if (t != null && t._valueType == JSValueType.Property)
                        {
                            ((t._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { value });
                        }
                        else
                        {
                            selfa._data[(int)(key + delta)] = value;
                        }

                        selfa._data[key] = null;
                    }
                }
                if (delta < 0)
                {
                    do
                        selfa._data.RemoveAt((int)(selfa._data.Length - 1));
                    while (++delta < 0);
                }
                for (var i = 2; i < args.length; i++)
                {
                    if (args[i].Exists)
                    {
                        var t = selfa._data[(int)(pos0 + i - 2)];
                        if (t != null && t._valueType == JSValueType.Property)
                        {
                            ((t._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { args[i] });
                        }
                        else
                        {
                            selfa._data[(int)(pos0 + i - 2)] = args[i].CloneImpl(false);
                        }
                    }
                }
                return res;
            }
            else
            {
                long _length = Tools.getLengthOfArraylike(self, false);
                var pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1]._valueType <= JSValueType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;

                if (pos1 == 0 && args.length <= 2)
                {
                    var lenobj = self.GetProperty("length", true, PropertyScope.Common);
                    if (lenobj._valueType == JSValueType.Property)
                    {
                        ((lenobj._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { _length });
                    }
                    else
                    {
                        lenobj.Assign(_length);
                    }

                    return new Array();
                }

                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var delta = System.Math.Max(0, args.length - 2) - (pos1 - pos0);
                var res = needResult ? new Array() : null;
                long prewKey = -1;
                foreach (var keyS in Tools.EnumerateArraylike(_length, self))
                {
                    if (prewKey == -1)
                        prewKey = (uint)keyS.Key;
                    if (keyS.Key - prewKey > 1 && keyS.Key < pos1)
                    {
                        for (var i = prewKey + 1; i < keyS.Key; i++)
                        {
                            var value = self.__proto__[i.ToString()];
                            if (value._valueType == JSValueType.Property)
                                value = Tools.InvokeGetter(value, self).CloneImpl(false);
                            else
                                value = value.CloneImpl(false);
                            if (needResult)
                                res._data[(int)i] = value.CloneImpl(false);
                        }
                    }
                    if (keyS.Key >= pos1)
                        break;
                    else if (pos0 <= keyS.Key)
                    {
                        var value = keyS.Value;
                        if (value.ValueType == JSValueType.Property)
                            value = Tools.InvokeGetter(value, self).CloneImpl(false);
                        else
                            value = value.CloneImpl(false);
                        if (needResult)
                            res._data[(int)(keyS.Key - pos0)] = value;
                    }
                    prewKey = keyS.Key;
                }
                if (prewKey == -1 && needResult)
                {
                    for (var i = 0; i < (pos1 - pos0); i++)
                        res.Add(self.__proto__[(i + pos0).ToString()].CloneImpl(false));
                }
                var tjo = new JSValue();
                if (delta > 0)
                {
                    for (var i = _length; i-- > pos1;)
                    {
                        if (i <= int.MaxValue)
                        {
                            tjo._valueType = JSValueType.Integer;
                            tjo._iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo._valueType = JSValueType.Double;
                            tjo._dValue = i + delta;
                        }
                        var dst = self.GetProperty(tjo, true, PropertyScope.Common);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo._valueType = JSValueType.Integer;
                            tjo._iValue = (int)(i);
                        }
                        else
                        {
                            tjo._valueType = JSValueType.Double;
                            tjo._dValue = i;
                        }
                        var src = self.GetProperty(tjo, true, PropertyScope.Common);
                        if (src._valueType == JSValueType.Property)
                            src = Tools.InvokeGetter(src, self);

                        if (dst._valueType == JSValueType.Property)
                        {
                            ((dst._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { src });
                        }
                        else
                        {
                            dst.Assign(src);
                        }
                    }
                }
                else if (delta < 0)
                {
                    for (var i = pos0; i < pos1; i++)
                    {
                        if (i + delta <= int.MaxValue)
                        {
                            tjo._valueType = JSValueType.Integer;
                            tjo._iValue = (int)(i);
                        }
                        else
                        {
                            tjo._valueType = JSValueType.Double;
                            tjo._dValue = i;
                        }
                        var src = self.GetProperty(tjo, true, PropertyScope.Common);
                        if (i >= _length + delta)
                        {
                            if ((src._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                            {
                                src._valueType = JSValueType.NotExists;
                                src._oValue = null;
                            }
                        }
                    }

                    for (var i = pos1; i < _length; i++)
                    {
                        if (i <= int.MaxValue)
                        {
                            tjo._valueType = JSValueType.Integer;
                            tjo._iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo._valueType = JSValueType.Double;
                            tjo._dValue = i + delta;
                        }
                        var dst = self.GetProperty(tjo, true, PropertyScope.Common);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo._valueType = JSValueType.Integer;
                            tjo._iValue = (int)(i);
                        }
                        else
                        {
                            tjo._valueType = JSValueType.Double;
                            tjo._dValue = i;
                        }
                        var srcItem = self.GetProperty(tjo, true, PropertyScope.Common);
                        var src = srcItem;
                        if (src._valueType == JSValueType.Property)
                            src = Tools.InvokeGetter(src, self);

                        if (dst._valueType == JSValueType.Property)
                        {
                            ((dst._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { src });
                        }
                        else
                        {
                            dst.Assign(src);
                        }

                        if (i >= _length + delta)
                        {
                            if ((srcItem._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                            {
                                srcItem._valueType = JSValueType.NotExists;
                                srcItem._oValue = null;
                            }
                        }
                    }
                }
                for (var i = 2; i < args.length; i++)
                {
                    if ((i - 2 + pos0) <= int.MaxValue)
                    {
                        tjo._valueType = JSValueType.Integer;
                        tjo._iValue = (int)(i - 2 + pos0);
                    }
                    else
                    {
                        tjo._valueType = JSValueType.Double;
                        tjo._dValue = (i - 2 + pos0);
                    }
                    var dst = self.GetProperty(tjo, true, PropertyScope.Common);
                    if (dst._valueType == JSValueType.Property)
                    {
                        ((dst._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { args[i] });
                    }
                    else
                    {
                        dst.Assign(args[i]);
                    }
                }

                _length += delta;
                var lenObj = self.GetProperty("length", true, PropertyScope.Common);
                if (lenObj._valueType == JSValueType.Property)
                {
                    ((lenObj._oValue as PropertyPair).setter ?? Function.Empty).Call(self, new Arguments { _length });
                }
                else
                {
                    lenObj.Assign(_length);
                }

                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue sort(JSValue self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0]._oValue as Function;
            var selfa = self as Array;
            if (selfa != null)
            {
                if (comparer != null)
                {
                    var second = new JSValue();
                    var first = new JSValue();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSValue, List<JSValue>>(new JSComparer(args, first, second, comparer));
                    uint length = selfa._data.Length;
                    foreach (var item in selfa._data.DirectOrder)
                    {
                        if (item.Value == null || !item.Value.Defined)
                            continue;
                        var v = item.Value;
                        if (v._valueType == JSValueType.Property)
                            v = ((v._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                        List<JSValue> list = null;
                        if (!tt.TryGetValue(v, out list))
                            tt[v] = list = new List<JSValue>();
                        list.Add(item.Value);
                    }
                    selfa._data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa._data.Add(node.value[i]);
                    }
                    selfa._data[(int)length - 1] = selfa._data[(int)length - 1];
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSValue>>(StringComparer.Ordinal);
                    uint length = selfa._data.Length;
                    foreach (var item in selfa._data.DirectOrder)
                    {
                        if (item.Value == null || !item.Value.Exists)
                            continue;
                        var v = item.Value;
                        if (v._valueType == JSValueType.Property)
                            v = ((v._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null).CloneImpl(false);
                        List<JSValue> list = null;
                        var key = v.ToString();
                        if (!tt.TryGetValue(key, out list))
                            tt[key] = list = new List<JSValue>();
                        list.Add(item.Value);
                    }
                    selfa._data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa._data.Add(node.value[i]);
                    }
                    selfa._data[(int)length - 1] = selfa._data[(int)length - 1];
                }
            }
            else
            {
                var len = Tools.getLengthOfArraylike(self, false);
                if (comparer != null)
                {
                    var second = new JSValue();
                    var first = new JSValue();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSValue, List<JSValue>>(new JSComparer(args, first, second, comparer));
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in Tools.EnumerateArraylike(len, self))
                    {
                        keysToRemove.Add(key.Key.ToString());
                        var item = key.Value;
                        if (item.Defined)
                        {
                            item = item.CloneImpl(false);
                            JSValue value;
                            if (item._valueType == JSValueType.Property)
                                value = ((item._oValue as PropertyPair).getter ?? Function.Empty).Call(self, null);
                            else
                                value = item;
                            List<JSValue> els = null;
                            if (!tt.TryGetValue(value, out els))
                                tt[value] = els = new List<JSValue>();
                            els.Add(item);
                        }
                    }
                    var tjo = new JSValue() { _valueType = JSValueType.String };
                    for (var i = keysToRemove.Count; i-- > 0;)
                    {
                        tjo._oValue = keysToRemove[i];
                        var t = self.GetProperty(tjo, true, PropertyScope.Common);
                        if ((t._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t._oValue = null;
                            t._valueType = JSValueType.NotExists;
                        }
                    }
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0;)
                            self[(index++).ToString()] = node.value[i];
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSValue>>(StringComparer.Ordinal);
                    List<string> keysToRemove = new List<string>();
                    foreach (var item in self)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(item.Key, ref pindex, out dindex) && (pindex == item.Key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(item.Key);
                            var value = item.Value;
                            if (value.Defined)
                            {
                                value = value.CloneImpl(false);
                                List<JSValue> els = null;
                                var skey = value.ToString();
                                if (!tt.TryGetValue(skey, out els))
                                    tt[skey] = els = new List<JSValue>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0;)
                        self[keysToRemove[i]]._valueType = JSValueType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0;)
                            self[(index++).ToString()] = node.value[i];
                    }
                }
            }
            return self;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsCount(1)]
        public static JSValue unshift(JSValue self, Arguments args)
        {
            for (var i = args.length; i-- > 0;)
                args[i + 2] = args[i];
            args.length += 2;
            args[0] = 0;
            args[1] = args[0];
            spliceImpl(self, args, false);
            return Tools.getLengthOfArraylike(self, false);
        }

        [Hidden]
        public override string ToString()
        {
            return joinImpl(this, ",", false);
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        [ArgumentsCount(0)]
        public new JSValue toString(Arguments args)
        {
            return this.ToString();
        }

        [DoNotEnumerate]
        public new JSValue toLocaleString()
        {
            return joinImpl(this, ",", true);
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            foreach (var item in _data.DirectOrder)
            {
                if (item.Value != null
                    && item.Value.Exists
                    && (!hideNonEnum || (item.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                {
                    var value = item.Value;
                    if (enumeratorMode == EnumerationMode.RequireValuesForWrite && (value._attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                        _data[item.Key] = value = value.CloneImpl(true);

                    yield return new KeyValuePair<string, JSValue>(((uint)item.Key).ToString(), value);
                }
            }
            if (!hideNonEnum)
                yield return new KeyValuePair<string, JSValue>("length", length);
            if (_fields != null)
            {
                foreach (var f in _fields)
                {
                    if (f.Value.Exists && (!hideNonEnum || (f.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    {
                        var value = f.Value;
                        if (enumeratorMode == EnumerationMode.RequireValuesForWrite && (value._attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                        {
                            _fields[f.Key] = value = value.CloneImpl(true);
                        }

                        yield return new KeyValuePair<string, JSValue>(f.Key, value);
                    }
                }
            }
        }

        [Hidden]
        public JSValue this[int index]
        {
            [Hidden]
            get
            {
                notExists._valueType = JSValueType.NotExistsInObject;
                var res = _data[(int)index] ?? notExists;
                if (res._valueType < JSValueType.Undefined)
                    return __proto__.GetProperty(index, false, PropertyScope.Common);
                return res;
            }
            [Hidden]
            set
            {
                if (index >= _data.Length
                    && _lengthObj != null
                    && (_lengthObj._attributes & JSValueAttributesInternal.ReadOnly) != 0)
                    return; // fixed size array. Item could not be added

                var res = _data[index];
                if (res == null)
                {
                    res = new JSValue() { _valueType = JSValueType.NotExistsInObject };
                    _data[index] = res;
                }
                else if ((res._attributes & JSValueAttributesInternal.SystemObject) != 0)
                    _data[index] = res = res.CloneImpl(false);
                if (res._valueType == JSValueType.Property)
                {
                    var setter = (res._oValue as PropertyPair).setter;
                    if (setter != null)
                        setter.Call(this, new Arguments { value });
                    return;
                }
                res.Assign(value);
            }
        }

        [Hidden]
        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (key._valueType != JSValueType.Symbol && memberScope < PropertyScope.Super)
            {
                var isIndex = false;
                var index = 0;
                var repeat = false;
                do
                {
                    repeat = false;
                    switch (key._valueType)
                    {
                        case JSValueType.Integer:
                            {
                                isIndex = (key._iValue & int.MinValue) == 0;
                                index = key._iValue;
                                break;
                            }
                        case JSValueType.Double:
                            {
                                isIndex = key._dValue >= 0 && key._dValue < uint.MaxValue && (long)key._dValue == key._dValue;
                                if (isIndex)
                                    index = (int)(uint)key._dValue;
                                break;
                            }
                        case JSValueType.String:
                            {
                                if (string.CompareOrdinal("length", key._oValue.ToString()) == 0)
                                    return length;

                                var skey = key._oValue.ToString();
                                if (skey.Length > 0 && '0' <= skey[0] && '9' >= skey[0])
                                {
                                    var dindex = 0.0;
                                    int si = 0;
                                    if (Tools.ParseNumber(skey, ref si, out dindex)
                                        && (si == skey.Length)
                                        && dindex >= 0
                                        && dindex < uint.MaxValue
                                        && (long)dindex == dindex)
                                    {
                                        isIndex = true;
                                        index = (int)(uint)dindex;
                                    }
                                }

                                break;
                            }
                        default:
                            {
                                if (key._valueType >= JSValueType.Object)
                                {
                                    key = key.ToPrimitiveValue_String_Value();
                                    var keyValue = key.Value;
                                    if (keyValue != null && string.CompareOrdinal("length", keyValue.ToString()) == 0)
                                        return length;

                                    if (key.ValueType < JSValueType.Object)
                                        repeat = true;
                                }

                                break;
                            }
                    }
                }
                while (repeat);

                if (isIndex)
                {
                    forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;
                    if (forWrite)
                    {
                        if (_lengthObj != null && (_lengthObj._attributes & JSValueAttributesInternal.ReadOnly) != 0 && index >= _data.Length)
                        {
                            if (memberScope == PropertyScope.Own)
                                ExceptionHelper.Throw(new TypeError("Can not add item into fixed size array"));
                            return notExists;
                        }

                        var res = _data[index];
                        if (res == null)
                        {
                            res = new JSValue() { _valueType = JSValueType.NotExistsInObject };
                            _data[index] = res;
                        }
                        else if ((res._attributes & JSValueAttributesInternal.SystemObject) != 0)
                        {
                            _data[index] = res = res.CloneImpl(false);
                        }

                        return res;
                    }
                    else
                    {
                        notExists._valueType = JSValueType.NotExistsInObject;
                        var res = _data[index] ?? notExists;
                        if (res._valueType < JSValueType.Undefined && memberScope != PropertyScope.Own)
                        {
                            return __proto__.GetProperty(key, false, memberScope);
                        }

                        return res;
                    }
                }
            }

            return base.GetProperty(key, forWrite, memberScope);
        }

        /*internal override bool DeleteMember(JSObject name)
        {
            if (name.valueType == JSObjectType.String && string.CompareOrdinal("length", name.oValue.ToString()) == 0)
                return false;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = tname.dValue >= 0 && tname.dValue < uint.MaxValue && (long)tname.dValue == tname.dValue;
                        if (isIndex)
                            index = (int)(uint)tname.dValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var fc = tname.oValue.ToString()[0];
                        if ('0' <= fc && '9' >= fc)
                        {
                            var dindex = 0.0;
                            int si = 0;
                            if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                && (si == tname.oValue.ToString().Length)
                                && dindex >= 0
                                && dindex < uint.MaxValue
                                && (long)dindex == dindex)
                            {
                                isIndex = true;
                                index = (int)(uint)dindex;
                            }
                        }
                        break;
                    }
            }
            if (isIndex)
            {
                var t = data[index];
                if (t == null)
                    return true;
                if (t.IsExists
                    && (t.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                    return false;
                data[index] = null;
                return true;
            }
            return base.DeleteMember(name);
        }*/

        /// <summary>
        ///
        /// </summary>
        /// <remarks>Не убирть!</remarks>
        /// <returns></returns>
        [Hidden]
        public override JSValue valueOf()
        {
            return base.valueOf();
        }

        private sealed class JSComparer : IComparer<JSValue>
        {
            private Arguments args;
            private JSValue first;
            private JSValue second;
            private Function comparer;

            public JSComparer(Arguments args, JSValue first, JSValue second, Function comparer)
            {
                this.args = args;
                this.first = first;
                this.second = second;
                this.comparer = comparer;
            }

            public int Compare(JSValue x, JSValue y)
            {
                first.Assign(x);
                second.Assign(y);
                args[0] = first;
                args[1] = second;
                var res = Tools.JSObjectToInt32(comparer.Call(undefined, args));
                return res;
            }
        }

        private sealed class LengthField : JSValue
        {
            private Array array;

            public LengthField(Array owner)
            {
                _attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.Reassign;
                array = owner;
                if ((int)array._data.Length == array._data.Length)
                {
                    this._iValue = (int)array._data.Length;
                    this._valueType = JSValueType.Integer;
                }
                else
                {
                    this._dValue = array._data.Length;
                    this._valueType = JSValueType.Double;
                }
            }

            public override void Assign(JSValue value)
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (uint)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    ExceptionHelper.Throw(new RangeError("Invalid array length"));
                if ((_attributes & JSValueAttributesInternal.ReadOnly) != 0)
                    return;
                array.SetLenght(nlen);
                if ((int)array._data.Length == array._data.Length)
                {
                    this._iValue = (int)array._data.Length;
                    this._valueType = JSValueType.Integer;
                }
                else
                {
                    this._dValue = array._data.Length;
                    this._valueType = JSValueType.Double;
                }
            }
        }

        public IIterator iterator()
        {
            return _data.GetEnumerator().AsIterator();
        }

        [DoNotEnumerate]
        [InstanceMember]
        public static IIterator entries(JSValue self)
        {
            IEnumerable enumerable;
            var array = self.As<Array>();
            if (array != null)
            {
                enumerable = array.getEntriesEnumerator();
            }
            else
            {
                enumerable = getGenericEntriesEnumerator(self);
            }

            return enumerable.AsIterable().iterator();
        }

        private static IEnumerable getGenericEntriesEnumerator(JSValue self)
        {
            var length = Tools.getLengthOfArraylike(self, false);
            for (var i = 0U; i < length; i++)
            {
                JSValue value;
                if (i < int.MaxValue)
                    value = self.GetProperty(Tools.Int32ToString((int)i));
                else
                    value = self.GetProperty(i.ToString());
                yield return new Array { i, value };
            }
        }

        private IEnumerable<Array> getEntriesEnumerator()
        {
            var prev = -1;
            foreach (var item in _data.DirectOrder)
            {
                if (item.Key - prev > 1)
                {
                    while (prev < item.Key - 1)
                    {
                        ++prev;
                        yield return new Array { prev, this[prev] };
                    }
                }

                if (item.Value == null || !item.Value.Exists)
                {
                    yield return new Array { item.Key, this[item.Key] };
                }
                else
                {
                    yield return new Array() { item.Key, item.Value };
                }

                prev = item.Key;
            }
        }
    }
}
