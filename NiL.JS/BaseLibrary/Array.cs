using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Array : JSObject, IIterable
    {
        private static readonly SparseArray<JSValue> emptyData = new SparseArray<JSValue>();
        [Hidden]
        internal SparseArray<JSValue> data;

        [DoNotEnumerate]
        public Array()
        {
            oValue = this;
            valueType = JSValueType.Object;
            data = new SparseArray<JSValue>();
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(int length)
        {
            oValue = this;
            valueType = JSValueType.Object;
            if (length < 0)
                ExceptionsHelper.Throw((new RangeError("Invalid array length.")));
            data = new SparseArray<JSValue>((int)System.Math.Min(100000, (uint)length));

            if (length > 0)
                data[length - 1] = null;

            attributes |= JSValueAttributesInternal.SystemObject;
        }

        internal Array(long length)
        {
            oValue = this;
            valueType = JSValueType.Object;
            if (length < 0 || length > uint.MaxValue)
                ExceptionsHelper.Throw((new RangeError("Invalid array length.")));
            data = new SparseArray<JSValue>((int)System.Math.Min(100000, length));
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(double length)
        {
            oValue = this;
            valueType = JSValueType.Object;
            if (((long)length != length) || (length < 0) || (length > 0xffffffff))
                ExceptionsHelper.Throw((new RangeError("Invalid array length.")));
            data = new SparseArray<JSValue>();

            if (length > 0)
                data[(int)((uint)length - 1)] = null;

            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Array(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            oValue = this;
            valueType = JSValueType.Object;
            data = new SparseArray<JSValue>();
            for (var i = 0; i < args.length; i++)
                data[i] = args[i].CloneImpl(false);
            attributes |= JSValueAttributesInternal.SystemObject;
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
            oValue = this;
            valueType = JSValueType.Object;
            if (source == null)
                throw new ArgumentNullException("enumerator");
            data = new SparseArray<JSValue>();
            var index = 0;
            while (source.MoveNext())
            {
                var e = source.Current;
                data[index++] = (e as JSValue ?? TypeProxy.Proxy(e)).CloneImpl(false);
            }
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [Hidden]
        public void Add(JSValue obj)
        {
            data.Add(obj);
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
                if (data.Length <= int.MaxValue)
                {
                    _lengthObj.iValue = (int)data.Length;
                    _lengthObj.valueType = JSValueType.Integer;
                }
                else
                {
                    _lengthObj.dValue = data.Length;
                    _lengthObj.valueType = JSValueType.Double;
                }
                return _lengthObj;
            }
        }

        [Hidden]
        internal bool SetLenght(long nlen)
        {
            if (data.Length == nlen)
                return true;
            if (nlen < 0)
                ExceptionsHelper.Throw(new RangeError("Invalid array length"));
            if (data.Length > nlen)
            {
                var res = true;
                foreach (var element in data.ReversOrder)
                {
                    if ((uint)element.Key < nlen)
                        break;
                    if (element.Value != null
                        && element.Value.Exists
                        && (element.Value.attributes & JSValueAttributesInternal.DoNotDelete) != 0)
                    {
                        nlen = element.Key;
                        res = false;
                    }
                }
                if (!res)
                {
                    SetLenght(nlen + 1); // áåñêîíå÷íîé ðåêóðñèè íå ìîæåò áûòü.
                    return false;
                }
            }
            while (data.Length > nlen)
            {
                data.RemoveAt((int)(data.Length - 1));
                data.Trim();
            }
            if (data.Length != nlen)
                data[(int)nlen - 1] = data[(int)nlen - 1];
            return true;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue concat(JSValue self, Arguments args)
        {
            Array res = null;
            var lenObj = self.GetProperty("length", PropertyScope.Own);
            if (!lenObj.Defined)
            {
                if (self.valueType < JSValueType.Object)
                    self = self.ToObject();
                res = new Array() { self };
            }
            else
                res = Tools.arraylikeToArray(self, true, true, false, -1);
            for (var i = 0; i < args.length; i++)
            {
                var v = args[i];
                var varr = v.oValue as Array;
                if (varr != null)
                {
                    varr = Tools.arraylikeToArray(varr, true, false, false, -1);
                    for (var ai = 0; ai < varr.data.Length; ai++)
                    {
                        var item = varr.data[ai];
                        res.data.Add(item);
                    }
                }
                else
                {
                    res.data.Add(v.CloneImpl(false));
                }
            }
            return res;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue every(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
                self = self.ToObject();

            var result = true;

            iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                return result &= (bool)jsCallback.Call(thisBind, new Arguments { value, index, self });
            });

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue some(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
                self = self.ToObject();

            var result = true;
            iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                return result &= !(bool)jsCallback.Call(thisBind, new Arguments { value, index, self });
            });

            return !result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue filter(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
                self = self.ToObject();

            Array result = new Array();

            iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
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
        [ArgumentsLength(1)]
        public static JSValue map(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
                self = self.ToObject();

            Array result = new Array();

            var len = iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
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
        [ArgumentsLength(1)]
        public static JSValue forEach(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
                self = self.ToObject();

            iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
            {
                value = value.CloneImpl(false);

                jsCallback.Call(thisBind, new Arguments { value, index, self });

                return true;
            });

            return null;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue indexOf(JSValue self, Arguments args)
        {
            var result = -1L;

            iterateImpl(self, null, args[1], (value, index, thisBind, jsCallback) =>
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

        private static long iterateImpl(JSValue self, Arguments args, JSValue startIndexSrc, Func<JSValue, long, JSValue, Function, bool> callback)
        {
            Array arraySrc = self.oValue as Array;
            bool nativeMode = arraySrc != null;
            if (!self.Defined || (self.valueType >= JSValueType.Object && self.oValue == null))
            {
#if PORTABLE
                ExceptionsHelper.Throw(new TypeError("Trying to call method for for null or undefined"));
#else
                var stackTrace = new System.Diagnostics.StackTrace();
                ExceptionsHelper.Throw(new TypeError("Can not call Array.prototype." + stackTrace.GetFrame(stackTrace.FrameCount - 2).GetMethod().Name + " for null or undefined"));
#endif
            }

            var length = nativeMode ? arraySrc.data.Length : Tools.getLengthOfArraylike(self, false);
            long startIndex = 0;
            Function jsCallback = null;
            JSValue thisBind = null;

            if (args != null)
            {
                // forEach, map, filter, every, some, reduce
                jsCallback = args[0] == null ? null : args[0].oValue as Function;
                if (jsCallback == null)
                    ExceptionsHelper.Throw(new TypeError("Callback is not a function."));

                thisBind = args.Length > 1 ? args[1] : null;
            }
            else if (startIndexSrc.Exists)
            {
                // indexOf
                startIndex = Tools.JSObjectToInt64(startIndexSrc, 0, true);
                if (startIndex > length)
                    startIndex = length;
                if (startIndex < 0)
                    startIndex += length;
                if (startIndex < 0)
                    startIndex = 0;
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
                                            tempKey.iValue = (int)i;
                                            tempKey.valueType = JSValueType.Integer;
                                        }
                                        else
                                        {
                                            tempKey.dValue = i;
                                            tempKey.valueType = JSValueType.Double;
                                        }
                                        var value = source.GetProperty(tempKey, false, PropertyScope.Сommon);
                                        if (value.Exists)
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
                    long prevKey = startIndex - 1;
                    var mainEnum = arraySrc.data.DirectOrder.GetEnumerator();
                    bool moved = true;
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
                                var tempKey = new JSValue();
                                if (i <= int.MaxValue)
                                {
                                    tempKey.iValue = (int)i;
                                    tempKey.valueType = JSValueType.Integer;
                                }
                                else
                                {
                                    tempKey.dValue = i;
                                    tempKey.valueType = JSValueType.Double;
                                }
                                value = self.GetProperty(tempKey, false, PropertyScope.Сommon);
                                if (value.Exists)
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
            Array arraySrc = self.oValue as Array;
            bool nativeMode = arraySrc != null;
            if (!self.Defined || (self.valueType >= JSValueType.Object && self.oValue == null))
            {
#if PORTABLE
                ExceptionsHelper.Throw(new TypeError("Trying to call method for for null or undefined"));
#else
                var stackTrace = new System.Diagnostics.StackTrace();
                ExceptionsHelper.Throw(new TypeError("Can not call Array.prototype." + stackTrace.GetFrame(stackTrace.FrameCount - 2).GetMethod().Name + " for null or undefined"));
#endif
            }

            var length = nativeMode ? arraySrc.data.Length : Tools.getLengthOfArraylike(self, false);
            long startIndex = length - 1;
            Function jsCallback = null;
            JSValue thisBind = null;

            if (args != null)
            {
                // reduceRight
                jsCallback = args[0] == null ? null : args[0].oValue as Function;
                if (jsCallback == null)
                    ExceptionsHelper.Throw(new TypeError("Callback is not a function."));

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
                        tempKey.iValue = (int)i;
                        tempKey.valueType = JSValueType.Integer;
                    }
                    else
                    {
                        tempKey.dValue = i;
                        tempKey.valueType = JSValueType.Double;
                    }

                    var value = self.GetProperty(tempKey, false, PropertyScope.Сommon);
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
                var mainEnum = arraySrc.data.ReversOrder.GetEnumerator();
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
                                tempKey.iValue = (int)i;
                                tempKey.valueType = JSValueType.Integer;
                            }
                            else
                            {
                                tempKey.dValue = i;
                                tempKey.valueType = JSValueType.Double;
                            }

                            value = self.GetProperty(tempKey, false, PropertyScope.Сommon);
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
                throw new ArgumentNullException("args");
            return args[0].Value is Array || args[0].Value == TypeProxy.GetPrototype(typeof(Array));
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue join(JSValue self, Arguments separator)
        {
            return joinImpl(self, separator == null || separator.length == 0 || !separator[0].Defined ? "," : separator[0].ToString(), false);
        }

        private static string joinImpl(JSValue self, string separator, bool locale)
        {
            if ((self.oValue == null && self.valueType >= JSValueType.Object) || self.valueType <= JSValueType.Undefined)
                ExceptionsHelper.Throw(new TypeError("Array.prototype.join called for null or undefined"));
            if (self.valueType >= JSValueType.Object && self.oValue.GetType() == typeof(Array))
            {
                var selfa = self as Array;
                if (selfa.data.Length == 0)
                    return "";
                var _data = selfa.data;
                try
                {
                    selfa.data = emptyData;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    JSValue t;
                    JSValue temp = 0;
                    for (var i = 0L; i < (long)_data.Length; i++)
                    {
                        t = _data[(int)i];
                        if (i <= int.MaxValue)
                            temp.iValue = (int)i;
                        else
                        {
                            temp.dValue = i;
                            temp.valueType = JSValueType.Double;
                        }
                        if (((t != null && t.Exists) || null != (t = self.GetProperty(temp, false, PropertyScope.Сommon)))
                            && t.Defined)
                        {
                            if (t.valueType < JSValueType.String || t.oValue != null)
                                sb.Append(locale ? t.ToPrimitiveValue_LocaleString_Value() : t.ToPrimitiveValue_String_Value());
                        }
                        sb.Append(separator);
                    }
                    sb.Length -= separator.Length;
                    return sb.ToString();
                }
                finally
                {
                    selfa.data = _data;
                }
            }
            else
            {
                return joinImpl(Tools.arraylikeToArray(self, true, false, false, -1), separator, locale);
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue lastIndexOf(JSValue self, Arguments args)
        {
            var result = -1L;

            reverseIterateImpl(self, null, args[1], (value, index, thisBind, jsCallback) =>
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
        [ArgumentsLength(0)]
        public static JSValue pop(JSValue self)
        {
            notExists.valueType = JSValueType.NotExistsInObject;
            var selfa = self as Array;
            if (selfa != null)
            {
                if (selfa.data.Length == 0)
                    return notExists;
                int newLen = (int)(selfa.data.Length - 1);
                var res = selfa.data[newLen] ?? self[newLen.ToString()];
                if (res.valueType == JSValueType.Property)
                    res = ((res.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null);
                selfa.data.RemoveAt(newLen);
                selfa.data[newLen - 1] = selfa.data[newLen - 1];
                return res;
            }
            else
            {
                var length = Tools.getLengthOfArraylike(self, true);
                if (length <= 0 || length > uint.MaxValue)
                    return notExists;
                length--;
                var tres = self.GetProperty(Context.CurrentContext.wrap(length.ToString()), true, PropertyScope.Сommon);
                JSValue res;
                if (tres.valueType == JSValueType.Property)
                    res = ((tres.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null);
                else
                    res = tres.CloneImpl(false);
                if ((tres.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                {
                    tres.oValue = null;
                    tres.valueType = JSValueType.NotExistsInObject;
                }
                self["length"] = length;
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue push(JSValue self, Arguments args)
        {
            notExists.valueType = JSValueType.NotExistsInObject;
            var selfa = self as Array;
            if (selfa != null)
            {
                for (var i = 0; i < args.length; i++)
                {
                    if (selfa.data.Length == uint.MaxValue)
                    {
                        if (selfa.fields == null)
                            selfa.fields = getFieldsContainer();
                        selfa.fields[uint.MaxValue.ToString()] = args.a0.CloneImpl(false);
                        ExceptionsHelper.Throw(new RangeError("Invalid length of array"));
                    }
                    selfa.data.Add(args[i].CloneImpl(false));
                }
                return selfa.length;
            }
            else
            {
                var length = (long)Tools.getLengthOfArraylike(self, false);
                var i = length;
                length += args.length;
                self["length"] = length;
                for (var j = 0; i < length; i++, j++)
                    self[i.ToString()] = args[j].CloneImpl(false);
                return length;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSValue reverse(JSValue self)
        {
            Arguments args = null;
            var selfa = self as Array;
            if (selfa != null)
            {
                for (var i = selfa.data.Length >> 1; i-- > 0;)
                {
                    var item0 = selfa.data[(int)(selfa.data.Length - 1 - i)];
                    var item1 = selfa.data[(int)(i)];
                    JSValue value0, value1;
                    if (item0 == null || !item0.Exists)
                        item0 = selfa.__proto__[(selfa.data.Length - 1 - i).ToString()];
                    if (item0.valueType == JSValueType.Property)
                        value0 = ((item0.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value0 = item0;
                    if (item1 == null || !item1.Exists)
                        item1 = selfa.__proto__[i.ToString()];
                    if (item1.valueType == JSValueType.Property)
                        value1 = ((item1.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value1 = item1;
                    if (item0.valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = item1;
                        ((item0.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, args);
                    }
                    else if (value1.Exists)
                        selfa.data[(int)(selfa.data.Length - 1 - i)] = value1;
                    else
                        selfa.data[(int)(selfa.data.Length - 1 - i)] = null;

                    if (item1.valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = item0;
                        ((item1.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, args);
                    }
                    else if (value0.Exists)
                        selfa.data[(int)i] = value0;
                    else
                        selfa.data[(int)i] = null;
                }
                return self;
            }
            else
            {
                var length = Tools.getLengthOfArraylike(self, false);
                for (var i = 0; i < length >> 1; i++)
                {
                    JSValue i0 = i.ToString();
                    JSValue i1 = (length - 1 - i).ToString();
                    var item0 = self.GetProperty(i0, false, PropertyScope.Сommon);
                    var item1 = self.GetProperty(i1, false, PropertyScope.Сommon);
                    var value0 = item0;
                    var value1 = item1;
                    if (value0.valueType == JSValueType.Property)
                        value0 = ((item0.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value0 = value0.CloneImpl(false);
                    if (value1.valueType == JSValueType.Property)
                        value1 = ((item1.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                    else
                        value1 = value1.CloneImpl(false);

                    if (item0.valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = value1;
                        ((item0.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, args);
                    }
                    else if (value1.Exists)
                        self.SetProperty(i0, value1, false);
                    else
                    {
                        var t = self.GetProperty(i0, true, PropertyScope.Own);
                        if ((t.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t.oValue = null;
                            t.valueType = JSValueType.NotExists;
                        }
                    }
                    if (item1.valueType == JSValueType.Property)
                    {
                        if (args == null)
                            args = new Arguments();
                        args.length = 1;
                        args.a0 = value0;
                        ((item1.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, args);
                    }
                    else if (value0.Exists)
                        self.SetProperty(i1, value0, false);
                    else
                    {
                        var t = self.GetProperty(i1, true, PropertyScope.Own);
                        if ((t.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t.oValue = null;
                            t.valueType = JSValueType.NotExists;
                        }
                    }
                }
                return self;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue reduce(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
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

            var len = (skip ? 0 : 1) + iterateImpl(self, args, undefined, (value, index, thisBind, jsCallback) =>
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
                ExceptionsHelper.ThrowTypeError("Length of array cannot be 0");

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue reduceRight(JSValue self, Arguments args)
        {
            if (self.valueType < JSValueType.Object)
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
                ExceptionsHelper.ThrowTypeError("Length of array cannot be 0");

            return result;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSValue shift(JSValue self)
        {
            var src = self.oValue as Array;
            if (src != null)
            {
                var res = src.data[0];
                if (res == null || !res.Exists)
                    res = src.__proto__["0"];
                if (res.valueType == JSValueType.Property)
                    res = Tools.InvokeGetter(res, self);

                JSValue prw = res;
                var length = src.data.Length;
                long prewIndex = 0;
                for (var e = src.data.DirectOrder.GetEnumerator(); ;)
                {
                    KeyValuePair<int, JSValue> element;
                    if (e.MoveNext())
                        element = e.Current;
                    else
                    {
                        if (length == 0)
                            break;
                        element = new KeyValuePair<int, JSValue>((int)length, null);
                    }

                    if (element.Key == 0)
                        continue;

                    JSValue value = null;
                    int key = 0;
                    if ((uint)element.Key < length - 1 && (element.Value == null || !element.Value.Exists))
                        continue;
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
                            //if (value == null || !value.IsExists)
                            //    continue;
                        }
                        if (value != null && value.valueType == JSValueType.Property)
                            value = Tools.InvokeGetter(value, self);

                        if (prw != null && prw.valueType == JSValueType.Property)
                        {
                            ((prw.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = value, length = 1 });
                        }
                        else
                        {
                            if (value != null)
                            {
                                if (value.Exists)
                                    src.data[key - 1] = value;
                                if (value.valueType != JSValueType.Property)
                                    src.data[key] = null;
                            }
                        }
                        prw = value;
                    }
                    if (prewIndex >= length || prewIndex < 0)
                        break;
                }
                if (length == 1)
                    src.data.Clear();
                else if (length > 0)
                {
                    src.data.RemoveAt((int)length - 1);
                    //selfa.data.Trim();
                    //if (len - 1 > selfa.data.Length)
                    //    selfa.data[(int)len - 2] = selfa.data[(int)len - 2];
                }
                return res;
            }
            else
            {
                var lenObj = self["length"];
                if (lenObj.valueType == JSValueType.Property)
                    lenObj = Tools.InvokeGetter(lenObj, self);

                long _length = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (_length > uint.MaxValue)
                    ExceptionsHelper.Throw(new RangeError("Invalid array length"));
                if (_length == 0)
                {
                    self["length"] = lenObj = _length;
                    return notExists;
                }
                var ti = new JSValue() { valueType = JSValueType.String, oValue = "0" };
                var t = self.GetProperty(ti, true, PropertyScope.Сommon);
                var res = t;
                if (res.valueType == JSValueType.Property)
                    res = Tools.InvokeGetter(res, self).CloneImpl(false);
                else
                    res = res.CloneImpl(false);
                if ((t.attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete)) == 0)
                {
                    t.oValue = null;
                    t.valueType = JSValueType.NotExists;
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
                        if (temp.valueType != JSValueType.Property)
                            keysToRemove.Add(item.Key);
                    }
                }
                var tjo = new JSValue() { valueType = JSValueType.String };
                for (var i = 0; i < keysToRemove.Count; i++)
                {
                    tjo.oValue = keysToRemove[i];
                    var to = self.GetProperty(tjo, true, PropertyScope.Сommon);
                    if ((to.attributes & (JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete)) == 0)
                    {
                        to.oValue = null;
                        to.valueType = JSValueType.NotExists;
                    }
                }
                tjo.valueType = JSValueType.Integer;
                foreach (var item in protoSource.data.DirectOrder)
                {
                    if ((uint)item.Key > int.MaxValue)
                    {
                        tjo.valueType = JSValueType.Double;
                        tjo.dValue = (uint)(item.Key - 1);
                    }
                    else
                        tjo.iValue = (item.Key - 1);
                    if (item.Value != null && item.Value.Exists)
                    {
                        var temp = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if (temp.valueType == JSValueType.Property)
                            ((temp.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments { item.Value });
                        else
                            temp.Assign(item.Value);
                    }
                }
                return res;
            }
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [InstanceMember]
        public static JSValue slice(JSValue self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (!self.Defined || (self.valueType >= JSValueType.Object && self.oValue == null))
                ExceptionsHelper.Throw(new TypeError("Can not call Array.prototype.slice for null or undefined"));
            HashSet<string> processedKeys = new HashSet<string>();
            Array res = new Array();
            for (;;)
            {
                var selfa = self as Array;
                if (selfa != null)
                {
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += selfa.data.Length;
                    if (startIndex < 0)
                        startIndex = 0;
                    var endIndex = Tools.JSObjectToInt64(args[1], selfa.data.Length, true);
                    if (endIndex < 0)
                        endIndex += selfa.data.Length;
                    var len = selfa.data.Length;
                    foreach (var element in selfa.data.DirectOrder)
                    {
                        if (element.Key >= len) // ýýý...
                            break;
                        var value = element.Value;
                        if (value == null || !value.Exists)
                            continue;
                        if (value.valueType == JSValueType.Property)
                            value = Tools.InvokeGetter(value, self);

                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        if (element.Key >= startIndex && element.Key < endIndex)
                            res.data[element.Key - (int)startIndex] = element.Value.CloneImpl(false);
                    }
                }
                else
                {
                    var lenObj = self["length"]; // проверка на null/undefined с бросанием исключения
                    if (!lenObj.Defined)
                        return res;
                    if (lenObj.valueType == JSValueType.Property)
                        lenObj = Tools.InvokeGetter(lenObj, self);

                    if (lenObj.valueType >= JSValueType.Object)
                        lenObj = lenObj.ToPrimitiveValue_Value_String();
                    if (!lenObj.Defined)
                        return new Array();
                    long _length = (uint)Tools.JSObjectToInt64(lenObj);
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += _length;
                    if (startIndex < 0)
                        startIndex = 0;
                    var endIndex = Tools.JSObjectToInt64(args[1], _length, true);
                    if (endIndex < 0)
                        endIndex += _length;
                    var @enum = self.GetEnumerator(false, EnumerationMode.RequireValues);
                    while (@enum.MoveNext())
                    {
                        var i = @enum.Current.Key;
                        var pindex = 0;
                        var dindex = 0.0;
                        long lindex = 0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex)
                            && (pindex == i.Length)
                            && dindex < _length
                            && (lindex = (long)dindex) == dindex)
                        {
                            var temp = @enum.Current.Value;
                            if (temp.valueType == JSValueType.Property)
                                temp = Tools.InvokeGetter(temp, self);

                            if (!temp.Exists)
                                continue;
                            if (processedKeys != null)
                            {
                                var sk = lindex.ToString();
                                if (processedKeys.Contains(sk))
                                    continue;
                                processedKeys.Add(sk);
                            }
                            if (lindex >= startIndex && lindex < endIndex)
                                res.data[(int)lindex - (int)startIndex] = temp.CloneImpl(false);
                        }
                    }
                }
                var crnt = self;
                if (self.__proto__ == @null)
                    break;
                self = self.__proto__;
                if (self == null || (self.valueType >= JSValueType.String && self.oValue == null))
                    break;
            }
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
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
                var _length = selfa.data.Length;
                long pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length); // double потому, что нужно "с заполнением", а не "с переполнением"
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSValueType.Undefined)
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
                foreach (var node in (delta > 0 ? selfa.data.ReversOrder : selfa.data.DirectOrder))
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
                    if (value.valueType == JSValueType.Property)
                        value = Tools.InvokeGetter(value, self).CloneImpl(false);

                    if (key < pos1)
                    {
                        if (needResult)
                            res.data[(int)(key - pos0)] = value;
                    }
                    else
                    {
                        var t = selfa.data[(int)(key + delta)];
                        if (t != null && t.valueType == JSValueType.Property)
                            ((t.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = value, length = 1 });
                        else
                            selfa.data[(int)(key + delta)] = value;
                        selfa.data[(int)(key)] = null;
                    }
                }
                if (delta < 0)
                {
                    do
                        selfa.data.RemoveAt((int)(selfa.data.Length - 1));
                    while (++delta < 0);
                }
                for (var i = 2; i < args.length; i++)
                {
                    if (args[i].Exists)
                    {
                        var t = selfa.data[(int)(pos0 + i - 2)];
                        if (t != null && t.valueType == JSValueType.Property)
                            ((t.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = args[i], length = 1 });
                        else
                            selfa.data[(int)(pos0 + i - 2)] = args[i].CloneImpl(false);
                    }
                }
                return res;
            }
            else // êòî-òî îòïðàâèë îáúåêò ñ ïîëåì length
            {
                long _length = Tools.getLengthOfArraylike(self, false);
                var pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSValueType.Undefined)
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
                    var lenobj = self.GetProperty("length", true, PropertyScope.Сommon);
                    if (lenobj.valueType == JSValueType.Property)
                        ((lenobj.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = _length, length = 1 });
                    else
                        lenobj.Assign(_length);
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
                            if (value.valueType == JSValueType.Property)
                                value = Tools.InvokeGetter(value, self).CloneImpl(false);
                            else
                                value = value.CloneImpl(false);
                            if (needResult)
                                res.data[(int)i] = value.CloneImpl(false);
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
                            res.data[(int)(keyS.Key - pos0)] = value;
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
                            tjo.valueType = JSValueType.Integer;
                            tjo.iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo.valueType = JSValueType.Double;
                            tjo.dValue = i + delta;
                        }
                        var dst = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSValueType.Integer;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSValueType.Double;
                            tjo.dValue = i;
                        }
                        var src = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if (src.valueType == JSValueType.Property)
                            src = Tools.InvokeGetter(src, self);

                        if (dst.valueType == JSValueType.Property)
                            ((dst.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = src, length = 1 });
                        else
                            dst.Assign(src);
                    }
                }
                else if (delta < 0)
                {
                    for (var i = pos0; i < pos1; i++)
                    {
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSValueType.Integer;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSValueType.Double;
                            tjo.dValue = i;
                        }
                        var src = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if (i >= _length + delta)
                        {
                            if ((src.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                            {
                                src.valueType = JSValueType.NotExists;
                                src.oValue = null;
                            }
                        }
                    }

                    for (var i = pos1; i < _length; i++)
                    {
                        if (i <= int.MaxValue)
                        {
                            tjo.valueType = JSValueType.Integer;
                            tjo.iValue = (int)(i + delta);
                        }
                        else
                        {
                            tjo.valueType = JSValueType.Double;
                            tjo.dValue = i + delta;
                        }
                        var dst = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if (i + delta <= int.MaxValue)
                        {
                            tjo.valueType = JSValueType.Integer;
                            tjo.iValue = (int)(i);
                        }
                        else
                        {
                            tjo.valueType = JSValueType.Double;
                            tjo.dValue = i;
                        }
                        var srcItem = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        var src = srcItem;
                        if (src.valueType == JSValueType.Property)
                            src = Tools.InvokeGetter(src, self);

                        if (dst.valueType == JSValueType.Property)
                            ((dst.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = src, length = 1 });
                        else
                            dst.Assign(src);
                        if (i >= _length + delta)
                        {
                            if ((srcItem.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                            {
                                srcItem.valueType = JSValueType.NotExists;
                                srcItem.oValue = null;
                            }
                        }
                    }
                }
                for (var i = 2; i < args.length; i++)
                {
                    if ((i - 2 + pos0) <= int.MaxValue)
                    {
                        tjo.valueType = JSValueType.Integer;
                        tjo.iValue = (int)(i - 2 + pos0);
                    }
                    else
                    {
                        tjo.valueType = JSValueType.Double;
                        tjo.dValue = (i - 2 + pos0);
                    }
                    var dst = self.GetProperty(tjo, true, PropertyScope.Сommon);
                    if (dst.valueType == JSValueType.Property)
                        ((dst.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = args[i], length = 1 });
                    else
                        dst.Assign(args[i]);
                }

                _length += delta;
                var lenObj = self.GetProperty("length", true, PropertyScope.Сommon);
                if (lenObj.valueType == JSValueType.Property)
                    ((lenObj.oValue as GsPropertyPair).set ?? Function.Empty).Call(self, new Arguments() { a0 = _length, length = 1 });
                else
                    lenObj.Assign(_length);
                return res;
            }
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(1)]
        public static JSValue sort(JSValue self, Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0].oValue as Function;
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
                    uint length = selfa.data.Length;
                    foreach (var item in selfa.data.DirectOrder)
                    {
                        if (item.Value == null || !item.Value.Defined)
                            continue;
                        var v = item.Value;
                        if (v.valueType == JSValueType.Property)
                            v = ((v.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                        List<JSValue> list = null;
                        if (!tt.TryGetValue(v, out list))
                            tt[v] = list = new List<JSValue>();
                        list.Add(item.Value);
                    }
                    selfa.data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa.data.Add(node.value[i]);
                    }
                    selfa.data[(int)length - 1] = selfa.data[(int)length - 1];
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSValue>>(StringComparer.Ordinal);
                    uint length = selfa.data.Length;
                    foreach (var item in selfa.data.DirectOrder)
                    {
                        if (item.Value == null || !item.Value.Exists)
                            continue;
                        var v = item.Value;
                        if (v.valueType == JSValueType.Property)
                            v = ((v.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null).CloneImpl(false);
                        List<JSValue> list = null;
                        var key = v.ToString();
                        if (!tt.TryGetValue(key, out list))
                            tt[key] = list = new List<JSValue>();
                        list.Add(item.Value);
                    }
                    selfa.data.Clear();
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = 0; i < node.value.Count; i++)
                            selfa.data.Add(node.value[i]);
                    }
                    selfa.data[(int)length - 1] = selfa.data[(int)length - 1];
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
                            if (item.valueType == JSValueType.Property)
                                value = ((item.oValue as GsPropertyPair).get ?? Function.Empty).Call(self, null);
                            else
                                value = item;
                            List<JSValue> els = null;
                            if (!tt.TryGetValue(value, out els))
                                tt[value] = els = new List<JSValue>();
                            els.Add(item);
                        }
                    }
                    var tjo = new JSValue() { valueType = JSValueType.String };
                    for (var i = keysToRemove.Count; i-- > 0;)
                    {
                        tjo.oValue = keysToRemove[i];
                        var t = self.GetProperty(tjo, true, PropertyScope.Сommon);
                        if ((t.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
                        {
                            t.oValue = null;
                            t.valueType = JSValueType.NotExists;
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
                        self[keysToRemove[i]].valueType = JSValueType.NotExists;
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
        [ArgumentsLength(1)]
        public static JSValue unshift(JSValue self, Arguments args)
        {
            for (var i = args.length; i-- > 0;)
                args[i + 2] = args[i];
            args.length += 2;
            args.a0 = 0;
            args.a1 = args.a0;
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
        [ArgumentsLength(0)]
        public new JSValue toString(Arguments args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                ExceptionsHelper.Throw(new TypeError("Try to call Array.toString on not Array object."));
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
            foreach (var item in data.DirectOrder)
            {
                if (item.Value != null
                    && item.Value.Exists
                    && (!hideNonEnum || (item.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                {
                    var value = item.Value;
                    if (enumeratorMode == EnumerationMode.RequireValuesForWrite && (value.attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                        data[item.Key] = value = value.CloneImpl(true);

                    yield return new KeyValuePair<string, JSValue>(((uint)item.Key).ToString(), value);
                }
            }
            if (!hideNonEnum)
                yield return new KeyValuePair<string, JSValue>("length", length);
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.Exists && (!hideNonEnum || (f.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    {
                        var value = f.Value;
                        if (enumeratorMode == EnumerationMode.RequireValuesForWrite && (value.attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                        {
                            fields[f.Key] = value = value.CloneImpl(true);
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
                notExists.valueType = JSValueType.NotExistsInObject;
                var res = data[(int)index] ?? notExists;
                if (res.valueType < JSValueType.Undefined)
                    return __proto__.GetProperty(index, false, PropertyScope.Сommon);
                return res;
            }
            [Hidden]
            set
            {
                if (index >= data.Length
                    && _lengthObj != null
                    && (_lengthObj.attributes & JSValueAttributesInternal.ReadOnly) != 0)
                    return; // fixed size array. Item could not be added

                var res = data[index];
                if (res == null)
                {
                    res = new JSValue() { valueType = JSValueType.NotExistsInObject };
                    data[index] = res;
                }
                else if ((res.attributes & JSValueAttributesInternal.SystemObject) != 0)
                    data[index] = res = res.CloneImpl(false);
                if (res.valueType == JSValueType.Property)
                {
                    var setter = (res.oValue as GsPropertyPair).set;
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
            if (key.valueType != JSValueType.Symbol && memberScope < PropertyScope.Super)
            {
                if (key.valueType == JSValueType.String && string.CompareOrdinal("length", (string)key.oValue.ToString()) == 0)
                    return this.length;
                bool isIndex = false;
                int index = 0;
                if (key.valueType >= JSValueType.Object)
                    key = key.ToPrimitiveValue_String_Value();

                switch (key.valueType)
                {
                    case JSValueType.Integer:
                        {
                            isIndex = (key.iValue & int.MinValue) == 0;
                            index = key.iValue;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            isIndex = key.dValue >= 0 && key.dValue < uint.MaxValue && (long)key.dValue == key.dValue;
                            if (isIndex)
                                index = (int)(uint)key.dValue;
                            break;
                        }
                    case JSValueType.String:
                        {
                            var skey = key.oValue.ToString();
                            if ('0' <= skey[0] && '9' >= skey[0])
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
                }
                if (isIndex)
                {
                    forWrite &= (attributes & JSValueAttributesInternal.Immutable) == 0;
                    if (forWrite)
                    {
                        if (_lengthObj != null && (_lengthObj.attributes & JSValueAttributesInternal.ReadOnly) != 0 && index >= data.Length)
                        {
                            if (memberScope == PropertyScope.Own)
                                ExceptionsHelper.Throw(new TypeError("Can not add item to fixed size array"));
                            return notExists;
                        }
                        var res = data[index];
                        if (res == null)
                        {
                            res = new JSValue() { valueType = JSValueType.NotExistsInObject };
                            data[index] = res;
                        }
                        else if ((res.attributes & JSValueAttributesInternal.SystemObject) != 0)
                            data[index] = res = res.CloneImpl(false);
                        return res;
                    }
                    else
                    {
                        notExists.valueType = JSValueType.NotExistsInObject;
                        var res = data[index] ?? notExists;
                        if (res.valueType < JSValueType.Undefined && memberScope != PropertyScope.Own)
                            return __proto__.GetProperty(key, false, memberScope);
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
                attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.Reassign;
                array = owner;
                if ((int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSValueType.Integer;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSValueType.Double;
                }
            }

            public override void Assign(JSValue value)
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (uint)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    ExceptionsHelper.Throw(new RangeError("Invalid array length"));
                if ((attributes & JSValueAttributesInternal.ReadOnly) != 0)
                    return;
                array.SetLenght(nlen);
                if ((int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSValueType.Integer;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSValueType.Double;
                }
            }
        }

        public IIterator iterator()
        {
            return data.GetEnumerator().AsIterator();
        }
    }
}