using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Array : CustomType
    {
        /*[Serializable]
        private class Element : JSObject
        {
            private Array owner;
            [NonSerialized]
            private int index;

            public Element(Array owner, int index)
            {
                valueType = JSObjectType.NotExistInObject;
                this.owner = owner;
                this.index = index;
                if (owner.data.Count <= index)
                {
                    var pv = (owner.__proto__ ?? owner.GetMember("__proto__")).GetMember(index.ToString(CultureInfo.InvariantCulture));
                    if (pv != undefined)
                        base.Assign(pv);
                    else
                        this.valueType = JSObjectType.NotExistInObject;
                }
                else
                    base.Assign(owner.data[index] ?? undefined);
            }

            public override void Assign(JSObject value)
            {
                if (owner.data.Count < index + 1)
                {
                    while (owner.data.Count < index)
                        owner.Add(null);
                    owner.data.Add(this);
                }
                base.Assign(value);
            }
        }*/

        [Hidden]
        internal List<JSObject> data;

        [DoNotEnumerate]
        public Array()
        {
            data = new List<JSObject>();
            attributes |= JSObjectAttributesInternal.ReadOnly;
        }

        [DoNotEnumerate]
        public Array(int length)
        {
            if (length < 0)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
        }

        [DoNotEnumerate]
        public Array(double d)
        {
            if (((long)d != d) || (d < 0) || (d > 0x7fffffff))
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new List<JSObject>((int)d);
            for (int i = 0; i < data.Capacity; i++)
                data.Add(null);
        }

        [DoNotEnumerate]
        public Array(JSObject args)
            : this(MethodProxy.argumentsToArray<object>(args))
        {

        }

        [Hidden]
        public Array(ICollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            data = new List<JSObject>(collection.Count);
            foreach (var o in collection)
            {
                var t = TypeProxy.Proxy(o).Clone() as JSObject;
                t.assignCallback = null;
                data.Add(t);
            }
        }

        [Hidden]
        public Array(IEnumerable enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            data = new List<JSObject>();
            foreach (var o in enumerable)
            {
                var t = TypeProxy.Proxy(o).Clone() as JSObject;
                t.assignCallback = null;
                data.Add(t);
            }
        }

        [Hidden]
        internal Array(IEnumerator enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");
            data = new List<JSObject>();
            while (enumerator.MoveNext())
            {
                var t = TypeProxy.Proxy(enumerator.Current).Clone() as JSObject;
                t.assignCallback = null;
                data.Add(t);
            }
        }

        [Hidden]
        public void Add(JSObject obj)
        {
            data.Add(obj);
        }

        [Field]
        [DoNotDelete]
        [NotConfigurable]
        [DoNotEnumerateAttribute]
        public JSObject length
        {
            [Hidden]
            get
            {
                return data.Count;
            }
            [Hidden]
            set
            {
                var nlenD = Tools.JSObjectToDouble(value["0"]);
                var nlen = (int)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    throw new JSException(new RangeError("Invalid array length"));
                setLength(nlen);
            }
        }

        internal bool setLength(int nlen)
        {
            if (nlen < 0)
                throw new JSException(new RangeError("Invalid array length"));
            if (data.Count > nlen)
            {
                while (data.Count > nlen)
                {
                    var element = data[data.Count - 1];
                    if (element != null && element.isExist && (element.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                        return false;
                    data.RemoveAt(data.Count - 1);
                }
            }
            else
            {
                if (data.Capacity < nlen)
                    data.Capacity = nlen;
                while (data.Count < nlen)
                    data.Add(null);
            }
            return true;
        }

        [DoNotEnumerateAttribute]
        public JSObject concat(JSObject[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var res = new List<JSObject>(data.Count + args.Length);
            for (int i = 0; i < data.Count; i++)
            {
                res.Add(new JSObject(false));
                res[i].Assign(data[i]);
            }
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.valueType == JSObjectType.Object && arg.oValue is Array)
                {
                    Array arr = arg.oValue as Array;
                    for (int j = 0; j < arr.data.Count; j++)
                    {
                        res.Add(new JSObject(false));
                        res[res.Count - 1].Assign(arr.data[j]);
                    }
                }
                else
                {
                    res.Add(new JSObject(false));
                    res[res.Count - 1].Assign(arg);
                }
            }
            return new Array(res);
        }

        [DoNotEnumerate]
        public JSObject every(JSObject[] args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Callback argument is not a function.")));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new JSObject() { oValue = Arguments.Instance, valueType = JSObjectType.Object };
            ao["length"] = 3;
            ao["1"] = 0;
            ao["2"] = this;
            bool res = true;
            bool isArray = this.GetType() == typeof(Array);
            var len = isArray ? data.Count : Tools.JSObjectToInt32(this["length"]);
            for (var i = 0; i < len && res; i++)
            {
                if (isArray && (data[i] == null || data[i].valueType < JSObjectType.Undefined))
                    continue;
                ao["0"] = isArray ? data[i] : this[i.ToString(CultureInfo.InvariantCulture)];
                ao["1"].iValue = i;
                res &= (bool)f.Invoke(tb, ao);
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject some(JSObject[] args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Callback argument is not a function.")));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new JSObject() { oValue = Arguments.Instance, valueType = JSObjectType.Object };
            ao["length"] = 3;
            ao["1"] = 0;
            ao["2"] = this;
            bool res = false;
            bool isArray = this.GetType() == typeof(Array);
            var len = isArray ? data.Count : Tools.JSObjectToInt32(this["length"]);
            for (var i = 0; i < len && !res; i++)
            {
                if (isArray && (data[i] == null || data[i].valueType < JSObjectType.Undefined))
                    continue;
                ao["0"] = isArray ? data[i] : this[i.ToString(CultureInfo.InvariantCulture)];
                ao["1"].iValue = i;
                res &= (bool)f.Invoke(tb, ao);
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject filter(JSObject[] args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Callback argument is not a function.")));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new JSObject() { oValue = Arguments.Instance, valueType = JSObjectType.Object };
            ao["length"] = 3;
            ao["1"] = 0;
            ao["2"] = this;
            var res = new Array();
            bool isArray = this.GetType() == typeof(Array);
            var len = isArray ? data.Count : Tools.JSObjectToInt32(this["length"]);
            for (var i = 0; i < len; i++)
            {
                if (isArray && (data[i] == null || data[i].valueType < JSObjectType.Undefined))
                    continue;
                ao["0"] = isArray ? data[i] : this[i.ToString(CultureInfo.InvariantCulture)];
                ao["1"].iValue = i;
                if ((bool)f.Invoke(tb, ao))
                    res.data.Add(data[i].Clone() as JSObject);
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject map(JSObject[] args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Callback argument is not a function.")));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new JSObject() { oValue = Arguments.Instance, valueType = JSObjectType.Object };
            ao["length"] = 3;
            ao["1"] = 0;
            ao["2"] = this;
            var res = new Array();
            bool isArray = this.GetType() == typeof(Array);
            var len = isArray ? data.Count : Tools.JSObjectToInt32(this["length"]);
            for (var i = 0; i < len; i++)
            {
                if (isArray && (data[i] == null || data[i].valueType < JSObjectType.Undefined))
                    continue;
                ao["0"] = isArray ? data[i] : this[i.ToString(CultureInfo.InvariantCulture)];
                ao["1"].iValue = i;
                res.data.Add(f.Invoke(tb, ao).Clone() as JSObject);
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject forEach(JSObject[] args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Callback argument is not a function.")));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new JSObject() { oValue = Arguments.Instance, valueType = JSObjectType.Object };
            ao["length"] = 3;
            ao["1"] = 0;
            ao["2"] = this;
            bool isArray = this.GetType() == typeof(Array);
            var len = isArray ? data.Count : Tools.JSObjectToInt32(this["length"]);
            for (var i = 0; i < len; i++)
            {
                if (isArray && (data[i] == null || data[i].valueType < JSObjectType.Undefined))
                    continue;
                ao["0"] = isArray ? data[i] : this[i.ToString(CultureInfo.InvariantCulture)];
                ao["1"].iValue = i;
                f.Invoke(tb, ao);
            }
            return undefined;
        }

        [DoNotEnumerateAttribute]
        public JSObject indexOf(JSObject[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return -1;
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
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            for (int i = 0; i < data.Count; i++)
            {
                if (Expressions.StrictEqual.Check(data[i] ?? undefined, left, null))
                    return i;
            }
            return -1;
        }

        [DoNotEnumerateAttribute]
        public static JSObject isArray(JSObject args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            return args.GetMember("0", false, true).Value is Array;
        }

        [DoNotEnumerate]
        public JSObject join(JSObject[] separator)
        {
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (separator.Length == 0)
                return ToString();
            var el = separator[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var i = 0;
            var max = data.Count - 1;
            for (; i < max; i++)
            {
                sb.Append(data[i] ?? "");
                sb.Append(el);
            }
            sb.Append(data[i] ?? "");
            return sb.ToString();
        }

        [DoNotEnumerate]
        public JSObject lastIndexOf(JSObject[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return -1;
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
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            for (int i = data.Count; i-- > 0; )
            {
                if (Expressions.StrictEqual.Check(data[i] ?? undefined, left, null))
                    return i;
            }
            return -1;
        }

        [DoNotEnumerate]
        public JSObject pop()
        {
            if (data.Count == 0)
                return JSObject.undefined;
            var res = data[data.Count - 1] ?? JSObject.undefined;
            data.RemoveAt(data.Count - 1);
            return res;
        }

        [DoNotEnumerate]
        public JSObject push(JSObject[] args)
        {
            data.Capacity = System.Math.Max(data.Capacity, data.Count + args.Length);
            for (var i = 0; i < args.Length; i++)
                data.Add(args[i].Clone() as JSObject);
            return this;
        }

        [DoNotEnumerate]
        public JSObject reverse()
        {
            data.Reverse();
            return this;
        }

        [DoNotEnumerate]
        public JSObject reduce(JSObject args)
        {
            var funco = args.GetMember("0");
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(TypeProxy.Proxy(new TypeError("First argument on reduce mast be a function.")));
            var func = funco.oValue as Function;
            var accum = args.GetMember("1", false, false);
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (data.Count == 0)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Array is empty.")));
                index++;
                accum.assignCallback = null;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            else
                args.fields.Remove("1");
            if (index >= data.Count)
                return accum;
            args["length"] = 4;
            args.fields["0"] = accum;
            args["3"] = this;
            while (index < data.Count)
            {
                args["1"] = data[index];
                args["2"] = index;
                accum.Assign(func.Invoke(args));
                index++;
            }
            return accum;
        }

        [DoNotEnumerate]
        public JSObject reduceRight(JSObject args)
        {
            var funco = args.GetMember("0");
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(TypeProxy.Proxy(new TypeError("First argument on reduce mast be a function.")));
            var func = funco.oValue as Function;
            var accum = args.GetMember("1", false, false);
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (data.Count == 0)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Array is empty.")));
                index++;
                accum.assignCallback = null;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            else
                args.fields.Remove("1");
            if (index >= data.Count)
                return accum;
            args["length"] = 4;
            args.fields["0"] = accum;
            args["3"] = this;
            while (index < data.Count)
            {
                args["1"] = data[data.Count - index - 1];
                var indexA = args.DefineMember("2");
                indexA.valueType = JSObjectType.Int;
                indexA.attributes |= JSObjectAttributesInternal.Argument;
                indexA.iValue = data.Count - index - 1;
                accum.Assign(func.Invoke(args));
                index++;
            }
            return accum;
        }

        [DoNotEnumerate]
        public JSObject shift()
        {
            if (data.Count == 0)
                return JSObject.undefined;
            var res = data[0] ?? JSObject.undefined;
            data.RemoveAt(0);
            return res;
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public JSObject slice(JSObject[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // Да, Array sealed, но тут и не такое возможно.
            {
                int pos0 = Tools.JSObjectToInt32(args[0], int.MaxValue, true);
                int pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = data.Count;
                    else
                        pos1 = System.Math.Min(Tools.JSObjectToInt32(args[1], true), data.Count);
                }
                else
                    pos1 = data.Count;
                if (pos0 < 0)
                    pos0 = data.Count + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = data.Count + pos1;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, data.Count);
                if (pos0 >= 0 && pos1 >= 0 && pos1 > pos0)
                {
                    var res = new Array();
                    for (int i = pos0; i < pos1; i++)
                    {
                        var t = new JSObject();
                        t.Assign(data[i]);
                        res.data.Add(t);
                    }
                    return res;
                }
                return new Array();
            }
            else // кто-то отправил объект с полем length
            {
                var leno = this.GetMember("length");
                if (leno.valueType < JSObjectType.Undefined)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Array.property.slice call for incompatible object.")));
                int len = Tools.JSObjectToInt32(leno);
                if (len >= 0)
                {
                    var t = new Array(len);
                    for (int i = 0; i < t.data.Count; i++)
                    {
                        var val = this.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
                        if (val.valueType > JSObjectType.Undefined)
                            t.data[i] = val.Clone() as JSObject;
                    }
                    return t.slice(args);
                }
                else
                {
                    long pos0 = (long)Tools.JSObjectToDouble(args[0]);
                    long pos1 = 0;
                    if (args.Length > 1)
                    {
                        if (args[1].valueType <= JSObjectType.Undefined)
                            pos1 = data.Count;
                        else
                            pos1 = (long)Tools.JSObjectToDouble(args[1]);
                    }
                    else
                        pos1 = data.Count;
                    if (pos0 < 0)
                        pos0 = data.Count + pos0;
                    if (pos0 < 0)
                        pos0 = 0;
                    if (pos1 < 0)
                        pos1 = data.Count + pos1;
                    if (pos1 < 0)
                        pos1 = 0;
                    var t = new Array(0);
                    if (this.fields != null)
                    {
                        foreach (var p in this.fields)
                        {
                            int i = 0;
                            double d = 0;
                            if (Tools.ParseNumber(p.Key, ref i, out d) && i == p.Key.Length && (long)d == d && d >= pos0 && d < pos1)
                                t.GetMember((d - pos0).ToString(CultureInfo.InvariantCulture), true, true).Assign(p.Value);
                        }
                    }
                    return t;
                }
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public JSObject splice(JSObject[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // Да, Array sealed, но тут и не такое возможно.
            {
                int pos0 = Tools.JSObjectToInt32(args[0], int.MaxValue, true);
                int pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = System.Math.Min(Tools.JSObjectToInt32(args[1], true), data.Count);
                }
                else
                    pos1 = data.Count;
                if (pos0 < 0)
                    pos0 = data.Count + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, data.Count);
                pos1 += pos0;
                pos1 = System.Math.Min(pos1, this.data.Count);
                var res = new Array();
                for (int i = pos0; i < pos1; i++)
                {
                    res.data.Add(data[i]);
                }
                this.data.RemoveRange(pos0, pos1 - pos0);
                if (args.Length > 2)
                {
                    this.data.InsertRange(pos0, args);
                    this.data.RemoveRange(pos0, 2);
                }
                return res;
            }
            else // кто-то отправил объект с полем length
            {
                var lobj = this.GetMember("length");
                long len = (long)Tools.JSObjectToDouble(lobj);
                len &= 0xffffffff;
                long pos0 = (long)Tools.JSObjectToDouble(args[0]);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = data.Count;
                    else
                        pos1 = (long)Tools.JSObjectToDouble(args[1]);
                }
                else
                    pos1 = len;
                if (pos0 < 0)
                    pos0 = len + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, len);
                pos1 += pos0;
                pos1 = System.Math.Min(pos1, len);
                var t = new Array(pos1 - pos0);
                List<string> keysForDelete = new List<string>(t.data.Count);
                List<KeyValuePair<string, JSObject>> ritems = new List<KeyValuePair<string, JSObject>>();
                var addCount = System.Math.Max(0, args.Length - 2);
                if (this.fields != null)
                {
                    foreach (var p in this.fields)
                    {
                        int i = 0;
                        double d = 0;
                        if (Tools.ParseNumber(p.Key, ref i, out d) && i == p.Key.Length && (long)d == d && d >= pos0)
                        {
                            if (d < pos1)
                            {
                                t.data[(int)(d - pos0)] = p.Value;
                                keysForDelete.Add(p.Key);
                            }
                            else if (d < len)
                            {
                                ritems.Add(new KeyValuePair<string, JSObject>((d - pos1 + pos0 + addCount).ToString(CultureInfo.InvariantCulture), p.Value));
                                keysForDelete.Add(p.Key);
                            }
                        }
                    }
                    for (int i = 0; i < keysForDelete.Count; i++)
                        this.fields.Remove(keysForDelete[i]);
                    for (int i = 0; i < ritems.Count; i++)
                        this.fields[ritems[i].Key] = ritems[i].Value;
                    for (int i = 0; i < addCount; i++)
                        this.fields[(pos0 + i).ToString(CultureInfo.InvariantCulture)] = args[i + 2];
                    pos0 += addCount + ritems.Count;
                    if (pos0 < 0x7fffffff)
                    {
                        lobj.iValue = (int)(pos0);
                        lobj.valueType = JSObjectType.Int;
                    }
                    else
                    {
                        lobj.dValue = pos0;
                        lobj.valueType = JSObjectType.Double;
                    }
                }
                return t;
            }
        }

        private sealed class JSComparer : IComparer<JSObject>
        {
            JSObject args;
            JSObject first;
            JSObject second;
            Function comparer;

            public JSComparer(JSObject args, JSObject first, JSObject second, Function comparer)
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
                args.fields["0"] = first;
                args.fields["1"] = second;
                var res = Tools.JSObjectToInt32(comparer.Invoke(JSObject.undefined, args));
                return res;
            }
        }

        [DoNotEnumerate]
        public JSObject sort(JSObject args)
        {
            if (!(((object)this) is Array)) // Да, Array sealed, но тут и не такое возможно.
            {
                int l = System.Math.Max(0, Tools.JSObjectToInt32(this.GetMember("length")));
                var t = new Array(l);
                for (int i = 0; i < l; i++)
                {
                    var val = this.GetMember(i.ToString(CultureInfo.InvariantCulture));
                    t.data[i] = val;
                }
                t.sort(args);
                for (int i = 0; i < l; i++)
                    this.fields[i.ToString(CultureInfo.InvariantCulture)] = t.data[i];
                return this;
            }
            if (args == null)
                throw new ArgumentNullException("args");
            var length = args.GetMember("length");
            var comparer = args.GetMember("0").oValue as Function;
            var len = Tools.JSObjectToInt32(length);
            if (comparer != null)
            {
                var second = new JSObject();
                var first = new JSObject();
                args.fields.Clear();
                length.iValue = 2;
                length.valueType = JSObjectType.Int;
                args.fields["length"] = length;
                args.fields["0"] = first;
                args.fields["1"] = second;

                /*int undefBlockStart = data.Count;
                for (int i = 0; i < undefBlockStart; i++)
                {
                    if (data[i] == null || data[i].valueType <= JSObjectType.Undefined)
                    {
                        undefBlockStart--;
                        var t = data[i];
                        data[i] = data[undefBlockStart];
                        data[undefBlockStart] = t;
                    }
                }*/

                data.Sort(new JSComparer(args, first, second, comparer));
            }
            else
                data.Sort((l, r) => string.CompareOrdinal((l ?? "undefined").ToString(), (r ?? "undefined").ToString()));
            return this;
        }

        [DoNotEnumerate]
        public JSObject unshift(JSObject[] args)
        {
            data.InsertRange(0, args);
            return length;
        }

        [Hidden]
        public override string ToString()
        {
            if (data.Count == 0)
                return "";
            var res = (data[0] ?? "").ToString();
            for (int i = 1; i < data.Count; i++)
                res += "," + (data[i] ?? "").ToString();
            return res;
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public new JSObject toString(JSObject args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Array.toString on not Array object.")));
            return this.ToString();
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            for (var i = 0; i < data.Count; i++)
            {
                if (data[i] != null && data[i].isExist && !data[i].attributes.HasFlag(JSObjectAttributesInternal.DoNotEnum))
                    yield return i.ToString(CultureInfo.InvariantCulture);
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
        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            int index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsNaN(dindex) && !double.IsInfinity(dindex))
            {
                if (dindex >= 0)
                {
                    if (dindex > 0x7fffffff)
                        throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array index")));
                    if (((index = (int)dindex) == dindex))
                    {
                        create &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                        if (data.Count <= index)
                        {
                            if (create)
                            {
                                if ((this["length"].oValue as Function[])[0] == null)
                                {
                                    if (own)
                                        throw new JSException(new TypeError("Cannot add element in fixed size array"));
                                    notExist.valueType = JSObjectType.NotExistInObject;
                                    return notExist;
                                }
                                setLength(index + 1);
                                return data[index] = new JSObject() { valueType = JSObjectType.NotExistInObject };
                            }
                            else
                                return base.GetMember(name, create, own);
                        }
                        else if (data[index] == null)
                        {
                            if (create)
                                return (data[index] ?? (data[index] = new JSObject() { valueType = JSObjectType.NotExistInObject }));
                            else
                                return base.GetMember(name, create, own);
                        }
                        else
                        {
                            if (data[index].valueType == JSObjectType.NotExist)
                                data[index].valueType = JSObjectType.NotExistInObject;
                            return data[index];
                        }
                    }
                }
            }
            return base.GetMember(name, create, own);
        }
    }
}