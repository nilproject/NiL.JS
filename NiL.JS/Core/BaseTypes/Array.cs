using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Array : JSObject
    {
        private sealed class _lengthField : JSObject
        {
            private Array array;

            public _lengthField(Array owner)
            {
                attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.NotConfigurable;
                array = owner;
                if ((long)(int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSObjectType.Int;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSObjectType.Double;
                }
            }

            public override void Assign(JSObject value)
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (uint)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    throw new JSException(new RangeError("Invalid array length"));
                if ((attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    return;
                array.setLength(nlen);
                if ((long)(int)array.data.Length == array.data.Length)
                {
                    this.iValue = (int)array.data.Length;
                    this.valueType = JSObjectType.Int;
                }
                else
                {
                    this.dValue = array.data.Length;
                    this.valueType = JSObjectType.Double;
                }
            }
        }

        [Hidden]
        internal SparseArray<JSObject> data;

        [DoNotEnumerate]
        public Array()
        {
            oValue = this;
            valueType = JSObjectType.Object;
            data = new SparseArray<JSObject>();
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(typeof(Array));
            _lengthObj = new _lengthField(this);
        }

        [DoNotEnumerate]
        public Array(int length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>();
            if (length > 0)
                data[length - 1] = null;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [CLSCompliant(false)]
        [Hidden]
        public Array(long length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0 || (uint)length > uint.MaxValue)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>((int)length);
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
        }

        [DoNotEnumerate]
        public Array(double d)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (((long)d != d) || (d < 0) || (d > 0xffffffff))
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new SparseArray<JSObject>();
            if (d > 0)
                data[(int)((uint)d - 1)] = null;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
        }

        [DoNotEnumerate]
        public Array(Arguments args)
            : this(MethodProxy.argumentsToArray<object>(args))
        {

        }

        [Hidden]
        public Array(ICollection collection)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (collection == null)
                throw new ArgumentNullException("collection");
            data = new SparseArray<JSObject>();
            if (collection.Count > 0)
                data[(int)collection.Count - 1] = null;
            var index = 0;
            foreach (var e in collection)
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
        }

        [Hidden]
        public Array(IEnumerable enumerable)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            data = new SparseArray<JSObject>();
            var index = 0;
            foreach (var e in enumerable)
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
        }

        [Hidden]
        internal Array(IEnumerator enumerator)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");
            data = new SparseArray<JSObject>();
            var index = 0;
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current;
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            }
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to array");
        }

        [Hidden]
        public void Add(JSObject obj)
        {
            data.Add(obj);
        }

        private _lengthField _lengthObj;
        [Hidden]
        public JSObject length
        {
            [Hidden]
            get
            {
                if (_lengthObj == null)
                    _lengthObj = new _lengthField(this);
                if (data.Length <= int.MaxValue)
                {
                    _lengthObj.iValue = (int)data.Length;
                    _lengthObj.valueType = JSObjectType.Int;
                }
                else
                {
                    _lengthObj.dValue = data.Length > uint.MaxValue ? 0 : (uint)data.Length;
                    _lengthObj.valueType = JSObjectType.Double;
                }
                return _lengthObj;
            }
        }

        internal bool setLength(long nlen)
        {
            if (data.Length == nlen)
                return true;
            if (nlen < 0)
                throw new JSException(new RangeError("Invalid array length"));
            if (data.Length > nlen)
            {
                var res = true;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                    if (element.Value != null
                        && element.Value.isExist
                        && element.Key >= nlen
                        && (element.Value.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                    {
                        nlen = element.Key;
                        res = false;
                    }
                if (!res)
                {
                    setLength(nlen + 1); // бесконечной рекурсии не может быть.
                    return false;
                }
            }
            if (data.Length > nlen) do
                {
                    data.RemoveAt((int)(data.Length - 1));
                    data.Trim();
                }
                while (data.Length > nlen);
            if (data.Length != nlen)
                data[(int)nlen - 1] = data[(int)nlen - 1];
            return true;
        }

        [DoNotEnumerateAttribute]
        public JSObject concat(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var res = new Array();
            long resLen = data.Length;
            foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
            {
                if (element.Value != null && element.Value.isExist)
                    res.data[element.Key] = element.Value.CloneImpl();
            }
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.valueType == JSObjectType.Object && arg.oValue is Array)
                {
                    Array arr = arg.oValue as Array;
                    foreach (var element in (arr.data as IEnumerable<KeyValuePair<int, JSObject>>))
                        if (element.Value != null && element.Value.isExist)
                            res.data[(int)(element.Key + resLen)] = element.Value.CloneImpl();
                    resLen += arr.data.Length;
                    res.data[(int)(resLen - 1)] = res.data[(int)(resLen - 1)];
                }
                else
                    res.data[(int)(resLen++)] = args[i].CloneImpl();
            }
            return res;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject every(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                var len = data.Length;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (element.Value == null || !element.Value.isExist)
                        continue;
                    if (element.Key >= len)
                        break;
                    var v = element.Value;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    ao[1].Assign(context.wrap(element.Key));
                    if (!(bool)f.Invoke(tb, ao).CloneImpl())
                        return false;
                }
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long len = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (len > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                for (var i = 0L; i < len; i++)
                {
                    if (i <= int.MaxValue)
                    {
                        ao[1].valueType = JSObjectType.Int;
                        ao[1].iValue = (int)i;
                    }
                    else
                    {
                        ao[1].valueType = JSObjectType.Double;
                        ao[1].dValue = i;
                    }
                    var v = this.GetMember(ao[1], false, false);
                    if (!v.isExist)
                        continue;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    if (!(bool)f.Invoke(tb, ao).CloneImpl())
                        return false;
                }
            }
            return true;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject some(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                var len = data.Length;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (element.Value == null || !element.Value.isExist)
                        continue;
                    if (element.Key >= len)
                        break;
                    var v = element.Value;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    ao[1].Assign(context.wrap(element.Key));
                    if ((bool)f.Invoke(tb, ao).CloneImpl())
                        return true;
                }
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long len = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (len > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                for (var i = 0L; i < len; i++)
                {
                    if (i <= int.MaxValue)
                    {
                        ao[1].valueType = JSObjectType.Int;
                        ao[1].iValue = (int)i;
                    }
                    else
                    {
                        ao[1].valueType = JSObjectType.Double;
                        ao[1].dValue = i;
                    }
                    var v = this.GetMember(ao[1], false, false);
                    if (!v.isExist)
                        continue;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    if ((bool)f.Invoke(tb, ao).CloneImpl())
                        return true;
                }
            }
            return false;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject filter(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var res = new Array();
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                var len = data.Length;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (element.Value == null || !element.Value.isExist)
                        continue;
                    if (element.Key >= len)
                        break;
                    var v = element.Value;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    ao[1].Assign(context.wrap(element.Key));
                    if ((bool)f.Invoke(tb, ao).CloneImpl())
                        res.data.Add(v);
                }
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long len = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (len > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                for (var i = 0L; i < len; i++)
                {
                    if (i <= int.MaxValue)
                    {
                        ao[1].valueType = JSObjectType.Int;
                        ao[1].iValue = (int)i;
                    }
                    else
                    {
                        ao[1].valueType = JSObjectType.Double;
                        ao[1].dValue = i;
                    }
                    var v = this.GetMember(ao[1], false, false);
                    if (!v.isExist)
                        continue;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    if ((bool)f.Invoke(tb, ao).CloneImpl())
                        res.data.Add(v);
                }
            }
            return res;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject map(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var res = new Array();
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                var len = data.Length;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (element.Value == null || !element.Value.isExist)
                        continue;
                    if (element.Key >= len)
                        break;
                    var v = element.Value;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    ao[1].Assign(context.wrap(element.Key));
                    res.data[element.Key] = f.Invoke(tb, ao).CloneImpl();
                }
                res.data[(int)len] = res.data[(int)len];
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long len = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (len > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                for (var i = 0L; i < len; i++)
                {
                    if (i <= int.MaxValue)
                    {
                        ao[1].valueType = JSObjectType.Int;
                        ao[1].iValue = (int)i;
                    }
                    else
                    {
                        ao[1].valueType = JSObjectType.Double;
                        ao[1].dValue = i;
                    }
                    var v = this.GetMember(ao[1], false, false);
                    if (!v.isExist)
                        continue;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    res.data[(int)i] = f.Invoke(tb, ao).CloneImpl();
                }
                res.data[(int)len] = res.data[(int)len];
            }
            return res;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject forEach(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = new JSObject();
            ao[1] = new JSObject();
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                var len = data.Length;
                foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (element.Value == null || !element.Value.isExist)
                        continue;
                    if (element.Key >= len)
                        break;
                    var v = element.Value;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    ao[1].Assign(context.wrap(element.Key));
                    f.Invoke(tb, ao);
                }
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long len = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (len > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                for (var i = 0L; i < len; i++)
                {
                    if (i <= int.MaxValue)
                    {
                        ao[1].valueType = JSObjectType.Int;
                        ao[1].iValue = (int)i;
                    }
                    else
                    {
                        ao[1].valueType = JSObjectType.Double;
                        ao[1].dValue = i;
                    }
                    var v = this.GetMember(ao[1], false, false);
                    if (!v.isExist)
                        continue;
                    if (v.valueType == JSObjectType.Property)
                        v = ((v.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    ao[0].Assign(v);
                    f.Invoke(tb, ao);
                }
            }
            return undefined;
        }

        [DoNotEnumerateAttribute]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject indexOf(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var el = args[0];
            JSObject src = this;
            if (!src.isDefinded || (src.valueType >= JSObjectType.Object && src.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.indexOf for null or undefined"));
            long _length = -1;
            HashSet<string> processedKeys = null;
            for (; ; )
            {
                if (src.GetType() == typeof(Array))
                {
                    if (_length == -1)
                        _length = data.Length;
                    var fromIndex = Tools.JSObjectToInt64(args[1], 0, true);
                    if (fromIndex < 0)
                        fromIndex += _length;
                    if (args.Length == 0)
                        return -1;
                    var len = _length;
                    foreach (var element in ((src as Array).data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (element.Key >= len) // эээ...
                            break;
                        var value = element.Value;
                        if (value == null || !value.isExist)
                            continue;
                        if (value.valueType == JSObjectType.Property)
                            value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        if ((uint)element.Key >= fromIndex && Expressions.StrictEqual.Check(value, el, null))
                            return Context.CurrentContext.wrap((uint)element.Key);
                    }
                }
                else
                {
                    if (src.valueType == JSObjectType.String)
                        return (src["indexOf"].oValue as Function).Invoke(src, args);
                    if (_length == -1)
                    {
                        var len = src["length"]; // тут же проверка на null/undefined с падением если надо
                        if (!len.isDefinded)
                            return -1;
                        if (len.valueType == JSObjectType.Property)
                            len = ((len.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                        if (len.valueType >= JSObjectType.Object)
                            len = len.ToPrimitiveValue_Value_String();
                        _length = (uint)Tools.JSObjectToInt64(len);
                    }
                    if (args.Length == 0)
                        return -1;
                    var fromIndex = Tools.JSObjectToInt64(args[1], 0, true);
                    if (fromIndex < 0)
                        fromIndex += _length;
                    var @enum = src.GetEnumeratorImpl(false);
                    while (@enum.MoveNext())
                    {
                        var i = @enum.Current;
                        var pindex = 0;
                        var dindex = 0.0;
                        long lindex = 0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex)
                            && (pindex == i.Length)
                            && dindex < _length
                            && (lindex = (long)dindex) == dindex)
                        {
                            var temp = src[i];
                            if (temp.valueType == JSObjectType.Property)
                                temp = ((temp.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                            if (!temp.isExist)
                                continue;
                            if (processedKeys != null)
                            {
                                var sk = lindex.ToString();
                                if (processedKeys.Contains(sk))
                                    continue;
                                processedKeys.Add(sk);
                            }
                            if (lindex >= fromIndex && Expressions.StrictEqual.Check(temp, el, null))
                            {
                                return lindex;
                            }
                        }
                    }
                }
                var crnt = src;
                if (src.__proto__ == null)
                    src.GetMember("__proto__");
                if (src.__proto__ == null)
                    break;
                src = src.__proto__.oValue as JSObject ?? src.__proto__;
                if (src == null || (src.valueType >= JSObjectType.String && src.oValue == null))
                    break;
                if (processedKeys == null)
                    processedKeys = new HashSet<string>(crnt);
            }
            return -1;
        }

        [DoNotEnumerateAttribute]
        public static JSObject isArray(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            return args[0].Value is Array || args[0].Value == TypeProxy.GetPrototype(typeof(Array));
        }

        [DoNotEnumerate]
        public JSObject join(Arguments separator)
        {
            if (data.Length == 0)
                return "";
            var el = separator == null || separator.length == 0 || !separator[0].isDefinded ? "," : separator[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JSObject t;
            for (var i = 0L; i < (long)data.Length; i++)
            {
                t = data[(int)i];
                if (((t != null && t.isExist) || null != (t = this.GetMember(i.ToString(), false, false)))
                    && t.isDefinded)
                {
                    if (t.valueType < JSObjectType.String || t.oValue != null)
                        sb.Append(t);
                }
                sb.Append(el);
            }
            sb.Length -= el.Length;
            return sb.ToString();
        }

        [DoNotEnumerateAttribute]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject lastIndexOf(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var el = args[0];
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
            }
            JSObject src = this;
            if (!src.isDefinded || (src.valueType >= JSObjectType.Object && src.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.indexOf for null or undefined"));
            HashSet<string> processedKeys = null;
            for (; ; )
            {
                if (src.GetType() == typeof(Array))
                {
                    if (args.Length == 0)
                        return -1;
                    foreach (var element in (src as Array).data.Reversed)
                    {
                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        if (element.Value != null && element.Value.isExist && Expressions.StrictEqual.Check(element.Value, el, null))
                            return Context.CurrentContext.wrap(element.Key);
                    }
                }
                else
                {
                    if (src.valueType == JSObjectType.String)
                        return (src["lastIndexOf"].oValue as Function).Invoke(src, args);
                    var len = src["length"]; // тут же проверка на null/undefined с падением если надо
                    if (!len.isDefinded)
                        return -1;
                    if (len.valueType == JSObjectType.Property)
                        len = ((len.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                    if (len.valueType >= JSObjectType.Object)
                        len = len.ToPrimitiveValue_Value_String();
                    var _length = (uint)Tools.JSObjectToInt64(len);
                    if (args.Length == 0)
                        return -1;
                    long result = -1;
                    foreach (var i in src)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        long lindex = 0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex)
                            && (pindex == i.Length)
                            && dindex < _length
                            && (lindex = (long)dindex) == dindex)
                        {
                            if (processedKeys != null)
                            {
                                var sk = lindex.ToString();
                                if (processedKeys.Contains(sk))
                                    continue;
                                processedKeys.Add(sk);
                            }
                            var temp = this[i];
                            if (temp.isExist && Expressions.StrictEqual.Check(temp, el, null))
                            {
                                result = lindex;
                            }
                        }
                    }
                    if (result != -1)
                        return result;
                }
                var crnt = src;
                if (src.__proto__ == null)
                    src.GetMember("__proto__");
                if (src.__proto__ == null)
                    break;
                src = src.__proto__.oValue as JSObject ?? src.__proto__;
                if (src == null || (src.valueType >= JSObjectType.String && src.oValue == null))
                    break;
                if (processedKeys == null)
                    processedKeys = new HashSet<string>(crnt);
            }
            return -1;
        }

        [DoNotEnumerate]
        public JSObject pop()
        {
            notExists.valueType = JSObjectType.NotExistsInObject;
            if (data.Length == 0)
                return notExists;
            var res = data[(int)(data.Length - 1)] ?? notExists;
            int newLen = (int)(data.Length - 1);
            data.RemoveAt(newLen);
            data[newLen - 1] = data[newLen - 1];
            return res;
        }

        [DoNotEnumerate]
        public JSObject push(Arguments args)
        {
            for (var i = 0; i < args.Length; i++)
                data.Add(args[i].CloneImpl());
            return this.length;
        }

        [DoNotEnumerate]
        public JSObject reverse()
        {
            for (var i = data.Length >> 1; i-- > 0; )
            {
                var v0 = data[(int)(data.Length - 1 - i)];
                var v1 = data[(int)(i)];
                if (v1 != null && v1.isExist)
                    data[(int)(data.Length - 1 - i)] = v1;
                else if (v0 != null)
                    data[(int)(data.Length - 1 - i)] = null;
                if (v0 != null && v0.isExist)
                    data[(int)i] = v0;
                else if (v1 != null)
                    data[(int)i] = null;
            }
            return this;
        }

        [DoNotEnumerate]
        public JSObject reduce(Arguments args)
        {
            var funco = args[0];
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("First argument on reduce mast be a function."));
            var func = funco.oValue as Function;
            var accum = args[1];
            if ((accum.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                accum = accum.CloneImpl();
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (data.Length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                index++;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            if (index >= data.Length)
                return accum;
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args[2] = new JSObject();
            var context = Context.CurrentContext;
            foreach (var element in (data as IEnumerable<KeyValuePair<int, JSObject>>))
            {
                if (element.Value == null || !element.Value.isExist)
                    continue;
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args[1] = (element.Value.oValue as Function[])[1] == null ? undefined : (element.Value.oValue as Function[])[1].Invoke(this, null);
                else
                    args[1] = element.Value;
                args[2].Assign(element.Key);
                args[3] = this;
                accum.Assign(func.Invoke(args));
            }
            return accum;
        }

        [DoNotEnumerate]
        public JSObject reduceRight(Arguments args)
        {
            var funco = args[0];
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("First argument on reduce mast be a function."));
            var func = funco.oValue as Function;
            var accum = args[1];
            if ((accum.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                accum = accum.CloneImpl();
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (data.Length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                index++;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            if (index >= data.Count)
                return accum;
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args[2] = new JSObject();
            var context = Context.CurrentContext;
            foreach (var element in data.Reversed)
            {
                if (element.Value == null || !element.Value.isExist)
                    continue;
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args[1] = (element.Value.oValue as Function[])[1] == null ? undefined : (element.Value.oValue as Function[])[1].Invoke(this, null);
                else
                    args[1] = element.Value;
                args[2].Assign(element.Key);
                args[3] = this;
                accum.Assign(func.Invoke(args));
            }
            return accum;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject shift()
        {
            notExists.valueType = JSObjectType.NotExistsInObject;
            if (this.GetType() == typeof(Array))
            {
                if (data.Length == 0)
                    return notExists;
                var res = data[0] ?? notExists;
                data[0] = null;
                if (res.valueType == JSObjectType.Property)
                    res = ((res.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                foreach (var item in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (item.Key == 0)
                        continue;
                    data[item.Key - 1] = item.Value;
                    data[item.Key] = null;
                }
                data.RemoveAt((int)(data.Length - 1));
                return res;
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long _length = (long)(uint)Tools.JSObjectToDouble(lenObj);
                if (_length > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                if (_length == 0)
                {
                    this["length"] = lenObj = _length;
                    return notExists;
                }
                this["length"] = lenObj = _length - 1;
                var res = this["0"].CloneImpl();
                if (_length == 1)
                {
                    this["0"] = notExists;
                    return res;
                }
                List<long> keysToRelocate = new List<long>();
                foreach (var i in this)
                {
                    var pindex = 0;
                    var dindex = 0.0;
                    long lindex = 0;
                    if (Tools.ParseNumber(i, ref pindex, out dindex)
                        && (pindex == i.Length)
                        && (lindex = (long)dindex) == dindex
                        && lindex > 0)
                    {
                        if (lindex >= _length)
                            break;
                        var temp = this[i];
                        if (!temp.isExist)
                            continue;
                        keysToRelocate.Add(lindex);
                    }
                }
                var ti = new JSObject() { valueType = JSObjectType.String, oValue = "0" };
                notExists.valueType = JSObjectType.NotExistsInObject;
                long prewKey = 0;
                for (int i = 0; i < keysToRelocate.Count; i++)
                {
                    if (keysToRelocate[i] == 1)
                        res = res.CloneImpl();
                    if (prewKey - keysToRelocate[i] < -1)
                        ti.oValue = (keysToRelocate[i] - 1).ToString();
                    prewKey = keysToRelocate[i];
                    var dst = this.GetMember(ti, true, false);
                    ti.oValue = keysToRelocate[i].ToString();
                    var src = this.GetMember(ti, true, false);
                    dst.Assign(src);
                    src.oValue = null;
                    src.valueType = JSObjectType.NotExistsInObject;
                }
                return res;
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject slice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            JSObject src = this;
            if (!src.isDefinded || (src.valueType >= JSObjectType.Object && src.oValue == null))
                throw new JSException(new TypeError("Can not call Array.prototype.slice for null or undefined"));
            HashSet<string> processedKeys = null;
            Array res = new Array();
            for (; ; )
            {
                if (src.GetType() == typeof(Array))
                {
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += data.Length;
                    var endIndex = Tools.JSObjectToInt64(args[1], data.Length, true);
                    if (endIndex < 0)
                        endIndex += data.Length;
                    var len = data.Length;
                    foreach (var element in ((src as Array).data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (element.Key >= len) // эээ...
                            break;
                        var value = element.Value;
                        if (value == null || !value.isExist)
                            continue;
                        if (value.valueType == JSObjectType.Property)
                            value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        if (element.Key >= startIndex && element.Key < endIndex)
                            res.Add(element.Value.CloneImpl());
                    }
                }
                else
                {
                    var lenObj = this["length"]; // тут же проверка на null/undefined с падением если надо
                    if (!lenObj.isDefinded)
                        return new Array();
                    if (lenObj.valueType == JSObjectType.Property)
                        lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                    if (lenObj.valueType >= JSObjectType.Object)
                        lenObj = lenObj.ToPrimitiveValue_Value_String();
                    if (!lenObj.isDefinded)
                        return new Array();
                    long _length = (uint)Tools.JSObjectToInt64(lenObj);
                    var startIndex = Tools.JSObjectToInt64(args[0], 0, true);
                    if (startIndex < 0)
                        startIndex += _length;
                    var endIndex = Tools.JSObjectToInt64(args[1], _length, true);
                    if (endIndex < 0)
                        endIndex += _length;
                    var @enum = src.GetEnumeratorImpl(false);
                    while (@enum.MoveNext())
                    {
                        var i = @enum.Current;
                        var pindex = 0;
                        var dindex = 0.0;
                        long lindex = 0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex)
                            && (pindex == i.Length)
                            && dindex < _length
                            && (lindex = (long)dindex) == dindex)
                        {
                            var temp = src[i];
                            if (temp.valueType == JSObjectType.Property)
                                temp = ((temp.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(src, null);
                            if (!temp.isExist)
                                continue;
                            if (processedKeys != null)
                            {
                                var sk = lindex.ToString();
                                if (processedKeys.Contains(sk))
                                    continue;
                                processedKeys.Add(sk);
                            }
                            if (lindex >= startIndex && lindex < endIndex)
                                res.Add(temp.CloneImpl());
                        }
                    }
                }
                var crnt = src;
                if (src.__proto__ == null)
                    src.GetMember("__proto__");
                if (src.__proto__ == null)
                    break;
                src = src.__proto__.oValue as JSObject ?? src.__proto__;
                if (src == null || (src.valueType >= JSObjectType.String && src.oValue == null))
                    break;
                if (processedKeys == null)
                    processedKeys = new HashSet<string>(crnt);
            }
            return res;
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject splice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if (this.GetType() == typeof(Array)) // Да, Array sealed, но тут и не такое возможно.
            {
                long pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), data.Length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), data.Length);
                }
                else
                    pos1 = data.Length;
                if (pos0 < 0)
                    pos0 = data.Length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = (uint)System.Math.Min(pos0, data.Length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, data.Length);
                var res = new Array((int)(pos1 - pos0));
                var delta = args.length - (pos1 - pos0) - 2;
                List<KeyValuePair<int, JSObject>> relocated = null;
                if (delta > 0)
                    relocated = new List<KeyValuePair<int, JSObject>>();
                foreach (var node in (data as IEnumerable<KeyValuePair<int, JSObject>>))
                {
                    if (node.Value != null && node.Value.isExist && node.Key >= pos0)
                    {
                        if (node.Key < pos1)
                        {
                            res.data[(int)(node.Key - pos0)] = node.Value;
                            data[node.Key] = null;
                        }
                        else
                        {
                            if (delta == 0)
                                break;
                            if (delta > 0)
                                relocated.Add(node);
                            else
                                data[(int)(node.Key + delta)] = node.Value;
                            data[node.Key] = null;
                        }
                    }
                }
                if (delta > 0)
                {
                    for (var i = 0; i < relocated.Count; i++)
                        data[(int)(relocated[i].Key + delta)] = relocated[i].Value;
                }
                else while (delta++ < 0)
                        data.RemoveAt((int)(data.Length - 1));
                for (var i = 2; i < args.length; i++)
                    if (args[i].isExist)
                        data[(int)(pos0 + i - 2)] = args[i].CloneImpl();
                return res;
            }
            else // кто-то отправил объект с полем length
            {
                var lenObj = this["length"]; // тут же проверка на null/undefined с падением если надо
                if (!lenObj.isDefinded)
                    return new Array();
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                if (lenObj.valueType >= JSObjectType.Object)
                    lenObj = lenObj.ToPrimitiveValue_Value_String();
                if (!lenObj.isDefinded)
                    return new Array();
                long _length = (uint)Tools.JSObjectToInt64(lenObj);
                var pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
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
                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var res = new Array();
                List<string> keysToRemove = new List<string>();
                List<KeyValuePair<uint, JSObject>> itemsToReplace = new List<KeyValuePair<uint, JSObject>>();
                long prewKey = -1;
                foreach (var keyS in this)
                {
                    var pindex = 0;
                    var dkey = 0.0;
                    if (Tools.ParseNumber(keyS, ref pindex, out dkey) && (pindex == keyS.Length))
                    {
                        if (prewKey == -1)
                            prewKey = (uint)dkey;
                        if (dkey - prewKey > 1 && dkey < pos1)
                        {
                            for (var i = prewKey + 1; i < dkey; i++)
                                res.data[(int)i] = __proto__[i.ToString()].CloneImpl();
                        }
                        if (dkey >= pos1)
                        {
                            keysToRemove.Add(keyS);
                            itemsToReplace.Add(new KeyValuePair<uint, JSObject>((uint)dkey, this[keyS].CloneImpl()));
                        }
                        else if (pos0 <= dkey)
                        {
                            res.Add(this[keyS].CloneImpl());
                            keysToRemove.Add(keyS);
                        }
                        prewKey = (long)dkey;
                    }
                }
                if (prewKey == -1)
                {
                    for (var i = 0; i < pos1; i++)
                        res.Add(__proto__[i.ToString()].CloneImpl());
                }
                for (var i = keysToRemove.Count; i-- > 0; )
                    this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                _length -= pos1 - pos0 - args.length + 2;
                for (var i = 2; i < args.length; i++)
                    this[(i - 2 + pos0).ToString()] = args[i].CloneImpl();
                for (var i = 0; i < itemsToReplace.Count; i++)
                    this[(itemsToReplace[i].Key - (pos1 - pos0 - args.length + 2)).ToString()] = itemsToReplace[i].Value;
                this["length"] = _length;
                return res;
            }
        }

        private sealed class JSComparer : IComparer<JSObject>
        {
            Arguments args;
            JSObject first;
            JSObject second;
            Function comparer;

            public JSComparer(Arguments args, JSObject first, JSObject second, Function comparer)
            {
                this.args = args;
                this.first = first;
                this.second = second;
                this.comparer = comparer;
            }

            public int Compare(JSObject x, JSObject y)
            {
                first.Assign(x);
                second.Assign(y);
                args[0] = first;
                args[1] = second;
                var res = Tools.JSObjectToInt32(comparer.Invoke(JSObject.undefined, args));
                return res;
            }
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject sort(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0].oValue as Function;
            if (this.GetType() == typeof(Array))
            {
                var len = data.Length;
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    Array src = this;
                    uint length = data.Length;
                    while (src != null)
                    {
                        foreach (var node in (src.data as IEnumerable<KeyValuePair<int, JSObject>>))
                        {
                            if (node.Value != null && node.Value.isDefinded)
                            {
                                if (src != this
                                    && (data[node.Key] ?? notExists).isExist)
                                    continue;
                                List<JSObject> els = null;
                                if (!tt.TryGetValue(node.Value, out els))
                                    tt[node.Value] = els = new List<JSObject>();
                                els.Add(node.Value);
                            }
                        }
                        if (src.__proto__ is TypeProxy
                            && src.__proto__ == TypeProxy.GetPrototype(typeof(Array)))
                            src = (src.__proto__ as TypeProxy).prototypeInstance as Array;
                        else
                            src = src.__proto__.oValue as Array;
                    }
                    var index = 0u;
                    data.Clear();
                    data[(int)length - 1] = null;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            data[(int)(index++)] = node.value[i];
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    Array src = this;
                    uint length = data.Length;
                    while (src != null)
                    {
                        foreach (var node in (src.data as IEnumerable<KeyValuePair<int, JSObject>>))
                        {
                            if (node.Value != null && node.Value.isDefinded)
                            {
                                if (src != this
                                    && (data[node.Key] ?? notExists).isExist)
                                    continue;
                                List<JSObject> els = null;
                                var key = node.Value.ToString();
                                if (!tt.TryGetValue(key, out els))
                                    tt[key] = els = new List<JSObject>();
                                els.Add(node.Value);
                            }
                        }
                        if (src.__proto__ is TypeProxy
                            && src.__proto__ == TypeProxy.GetPrototype(typeof(Array)))
                            src = (src.__proto__ as TypeProxy).prototypeInstance as Array;
                        else
                            src = src.__proto__.oValue as Array;
                    }
                    var index = 0u;
                    data.Clear();
                    data[(int)length - 1] = null;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            data[(int)(index++)] = node.value[i];
                    }
                }
            }
            else
            {
                var len = (uint)Tools.JSObjectToInt64(this["length"]);
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in this)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(key);
                            var value = this[key];
                            if (value.isDefinded)
                            {
                                value = value.CloneImpl();
                                List<JSObject> els = null;
                                if (!tt.TryGetValue(value, out els))
                                    tt[value] = els = new List<JSObject>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0; )
                        this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            this[(index++).ToString()] = node.value[i];
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in this)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(key);
                            var value = this[key];
                            if (value.isDefinded)
                            {
                                value = value.CloneImpl();
                                List<JSObject> els = null;
                                var skey = value.ToString();
                                if (!tt.TryGetValue(skey, out els))
                                    tt[skey] = els = new List<JSObject>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0; )
                        this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            this[(index++).ToString()] = node.value[i];
                    }
                }
            }
            return this;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject unshift(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException();
            if (this.GetType() == typeof(Array))
            {
                if (args.length == 0)
                    return data.Length;
                if (data.Length + args.length > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                var len = data.Length;
                var prewIndex = -1;
                foreach (var node in data.Reversed)
                {
                    if (prewIndex == -1)
                        prewIndex = node.Key;
                    else if (prewIndex - node.Key > 1)
                    {
                        for (var i = node.Key + 1; i < prewIndex; i++)
                        {
                            var t = __proto__[i.ToString()];
                            if (t.isExist)
                                data[(int)(i + args.Length)] = t.CloneImpl();
                        }
                    }
                    data[(int)node.Key] = null;
                    if (node.Value != null && node.Value.isExist)
                        data[(int)(node.Key + args.Length)] = node.Value;
                    else
                    {
                        var t = __proto__[node.Key.ToString()];
                        if (t.isExist)
                            data[(int)(node.Key + args.Length)] = t.CloneImpl();
                    }
                    prewIndex = node.Key;
                }
                data[(int)(args.Length + len - 1)] = data[(int)(args.Length + len - 1)];
                for (var i = 0; i < args.Length; i++)
                    data[i] = args[i].CloneImpl();
                return length;
            }
            else
            {
                var lenObj = this["length"];
                if (lenObj.valueType == JSObjectType.Property)
                    lenObj = ((lenObj.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(this, null);
                long _length = (long)(uint)Tools.JSObjectToDouble(this["length"]);
                if (_length + args.length > uint.MaxValue)
                    throw new JSException(new RangeError("Invalid array length"));
                this["length"] = lenObj = _length;
                if (args.length == 0)
                    return lenObj;
                List<KeyValuePair<KeyValuePair<long, string>, JSObject>> keysToRemove = new List<KeyValuePair<KeyValuePair<long, string>, JSObject>>();
                long prewKey = -1;
                foreach (var key in this)
                {
                    var pindex = 0;
                    var dindex = 0.0;
                    if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                        && dindex < _length
                        && (long)dindex == dindex)
                    {
                        if (prewKey == -1)
                            prewKey = (uint)dindex;
                        if (dindex - prewKey > 1)
                        {
                            for (var i = prewKey + 1; i < dindex; i++)
                            {
                                var t = __proto__[i.ToString()];
                                if (t.isExist)
                                    this[(i + args.Length).ToString()] = t.CloneImpl();
                            }
                        }
                        keysToRemove.Add(new KeyValuePair<KeyValuePair<long, string>, JSObject>(new KeyValuePair<long, string>((long)dindex, key), this[key].CloneImpl()));
                    }
                }
                if (prewKey == -1)
                {
                    for (var i = 0; i < _length; i++)
                    {
                        var t = __proto__[i.ToString()];
                        if (t.isExist)
                            this[(i + args.Length).ToString()] = t.CloneImpl();
                    }
                }
                for (var i = keysToRemove.Count; i-- > 0; )
                    this[keysToRemove[i].Key.Value] = notExists;
                for (var i = keysToRemove.Count; i-- > 0; )
                {
                    this[(keysToRemove[i].Key.Key + args.length).ToString(CultureInfo.InvariantCulture)] = keysToRemove[i].Value;
                }
                for (var i = 0; i < args.Length; i++)
                    this[i.ToString()] = args[i].CloneImpl();
                _length += args.length;
                this["length"] = _length;
                return _length;
            }
        }

        [Hidden]
        public override string ToString()
        {
            return join(null).oValue.ToString();
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public new JSObject toString(Arguments args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                throw new JSException(new TypeError("Try to call Array.toString on not Array object."));
            return this.ToString();
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            foreach (var node in (data as IEnumerable<KeyValuePair<int, JSObject>>))
            {
                if (node.Value != null
                    && node.Value.isExist
                    && (!hideNonEnum || (node.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return node.Key.ToString();
            }
            if (!hideNonEnum)
                yield return "length";
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.isExist && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (name.valueType == JSObjectType.String && "length".Equals(name.oValue))
                return length;
            bool isIndex = false;
            int index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Bool:
                    break;
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
                forWrite &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                if (forWrite)
                {
                    if (_lengthObj != null && (_lengthObj.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && index >= data.Length)
                    {
                        if (own)
                            throw new JSException(new TypeError("Cannot add element in fixed size array"));
                        return notExists;
                    }
                    var res = data[(int)index];
                    if (res == null)
                    {
                        res = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                        data[(int)index] = res;
                    }
                    else if ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                        data[(int)index] = res = res.CloneImpl();
                    return res;
                }
                else
                {
                    notExists.valueType = JSObjectType.NotExistsInObject;
                    var res = data[(int)index] ?? notExists;
                    if (!res.isExist && !own)
                        return __proto__.GetMember(name, forWrite, own);
                    return res;
                }
            }

            //if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
            //    return __proto__.GetMember(name, create, own);
            return DefaultFieldGetter(name, forWrite, own);
        }

        [Hidden]
        public override JSObject valueOf()
        {
            return base.valueOf();
        }
    }
}