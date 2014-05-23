using System;
using System.Collections.Generic;
using System.Collections;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Array : EmbeddedType
    {
        private class Enumerator : IEnumerator<string>
        {
            private Array owner;
            private int index;

            public Enumerator(Array owner)
            {
                this.owner = owner;
                index = -1;
            }

            public object Current { get { return index.ToString(System.Globalization.CultureInfo.InvariantCulture); } }
            string IEnumerator<string>.Current { get { return index.ToString(System.Globalization.CultureInfo.InvariantCulture); } }

            public bool MoveNext()
            {
                do
                    index++;
                while (index < owner.data.Count && (owner.data[index] == null || owner.data[index].ValueType < JSObjectType.Undefined));
                return index < owner.data.Count;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {

            }
        }

        [Serializable]
        private class Element : JSObject
        {
            private Array owner;
            [NonSerialized]
            private int index;

            public Element(Array owner, int index)
            {
                this.owner = owner;
                this.index = index;
                if (owner.data.Count <= index)
                {
                    var pv = (owner.prototype ?? owner.GetField("__proto__", true, false)).GetField(index.ToString(), true, false);
                    if (pv != undefined)
                        base.Assign(pv);
                    else
                        this.ValueType = JSObjectType.NotExistInObject;
                }
                else
                    base.Assign(owner[index]);
            }

            public override void Assign(JSObject value)
            {
                if (owner.data.Count < index + 1)
                {
                    while (owner.data.Count < index)
                        owner.Add(new JSObject { ValueType = JSObjectType.Undefined });
                    owner.data.Add(this);
                }
                base.Assign(value);
            }
        }

        [Hidden]
        internal List<JSObject> data;

        public Array()
        {
            data = new List<JSObject>();
        }

        public Array(int length)
        {
            if (length < 0)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
        }

        public Array(double d)
        {
            if (((long)d != d) || (d < 0) || (d > 0x7fffffff))
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new List<JSObject>((int)d);
            for (int i = 0; i < data.Capacity; i++)
                data.Add(null);
        }

        public Array(ICollection collection)
        {
            data = new List<JSObject>(collection.Count);
            foreach (var o in collection)
            {
                var t = TypeProxy.Proxy(o);
                t.assignCallback = null;
                data.Add(t);
            }
        }

        [Hidden]
        public Array(IEnumerable enumerable)
        {
            data = new List<JSObject>();
            foreach (var o in enumerable)
            {
                var t = TypeProxy.Proxy(o);
                t.assignCallback = null;
                data.Add(t);
            }
        }

        [Hidden]
        public void Add(JSObject obj)
        {
            data.Add(obj);
        }

        [Modules.Hidden]
        public JSObject this[int index]
        {
            get
            {
                if (data.Count <= index || data[index] == null)
                    return new Element(this, index);
                else
                    return data[index];
            }
            internal set
            {
                if (data.Capacity < index)
                    data.Capacity = index + 1;
                while (data.Count <= index)
                    data.Add(null);
                data[index] = value;
            }
        }

        public int length
        {
            get
            {
                return data.Count;
            }
            set
            {
                if (data.Count > value)
                    data.RemoveRange(value, data.Count - value);
                else
                {
                    if (data.Capacity < value)
                        data.Capacity = value;
                    while (data.Count < value)
                        data.Add(null);
                }
            }
        }

        public JSObject concat(JSObject[] args)
        {
            var res = new List<JSObject>(data.Count + args.Length);
            for (int i = 0; i < data.Count; i++)
            {
                res.Add(new JSObject(false));
                res[i].Assign(data[i]);
            }
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.ValueType == JSObjectType.Object && arg.oValue is Array)
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

        public JSObject indexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            var el = args[0];
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
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
                            Tools.ParseNumber(args[1].ToString(), ref pos, false, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            var right = new NiL.JS.Statements.ImmidateValueStatement(null);
            var opeq = new NiL.JS.Statements.Operators.Equal(left, right);
            for (int i = 0; i < data.Count; i++)
            {
                right.value = data[i];
                if ((bool)opeq.Invoke(null))
                    return i;
            }
            return -1;
        }

        public static JSObject isArray(JSObject args)
        {
            return args.GetField("0", false, true).oValue is Array;
        }

        public JSObject join(JSObject[] separator)
        {
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

        public JSObject lastIndexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            var el = args[0];
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
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
                            Tools.ParseNumber(args[1].ToString(), ref pos, false, out d);
                            pos = (int)d;
                            break;
                        }
                }
            }
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            var right = new NiL.JS.Statements.ImmidateValueStatement(null);
            var opeq = new NiL.JS.Statements.Operators.Equal(left, right);
            for (int i = data.Count; i-- > 0; )
            {
                right.value = data[i];
                if ((bool)opeq.Invoke(null))
                    return i;
            }
            return -1;
        }

        public JSObject pop()
        {
            if (data.Count == 0)
                return JSObject.undefined;
            var res = data[data.Count - 1] ?? JSObject.undefined;
            data.RemoveAt(data.Count - 1);
            return res;
        }

        public JSObject push(JSObject[] args)
        {
            data.AddRange(args);
            return this;
        }

        public JSObject reverse()
        {
            data.Reverse();
            return this;
        }

        public JSObject shift()
        {
            if (data.Count == 0)
                return JSObject.undefined;
            var res = data[0] ?? JSObject.undefined;
            data.RemoveAt(0);
            return res;
        }

        [ParametersCount(2)]
        public JSObject slice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // Да, Array sealed, но тут и не такое возможно.
            {
                int pos0 = Tools.JSObjectToInt(args[0], int.MaxValue, true);
                int pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].ValueType <= JSObjectType.Undefined)
                        pos1 = data.Count;
                    else
                        pos1 = System.Math.Min(Tools.JSObjectToInt(args[1], true), data.Count);
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
                int len = Tools.JSObjectToInt(this.GetField("length", true, false));
                if (len >= 0)
                {
                    var t = new Array(len);
                    for (int i = 0; i < t.data.Count; i++)
                    {
                        var val = this.GetField(i.ToString(), true, false);
                        if (val.ValueType > JSObjectType.Undefined)
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
                        if (args[1].ValueType <= JSObjectType.Undefined)
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
                            if (Tools.ParseNumber(p.Key, ref i, true, out d) && i == p.Key.Length && (long)d == d && d >= pos0 && d < pos1)
                                t.GetField((d - pos0).ToString(), false, true).Assign(p.Value);
                        }
                    }
                    return t;
                }
            }
        }

        [ParametersCount(2)]
        public JSObject splice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // Да, Array sealed, но тут и не такое возможно.
            {
                int pos0 = Tools.JSObjectToInt(args[0], int.MaxValue, true);
                int pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].ValueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = System.Math.Min(Tools.JSObjectToInt(args[1], true), data.Count);
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
                var lobj = this.GetField("length", true, false);
                long len = (long)Tools.JSObjectToDouble(lobj);
                len &= 0xffffffff;
                long pos0 = (long)Tools.JSObjectToDouble(args[0]);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].ValueType <= JSObjectType.Undefined)
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
                        if (Tools.ParseNumber(p.Key, ref i, true, out d) && i == p.Key.Length && (long)d == d && d >= pos0)
                        {
                            if (d < pos1)
                            {
                                t.data[(int)(d - pos0)] = p.Value;
                                keysForDelete.Add(p.Key);
                            }
                            else if (d < len)
                            {
                                ritems.Add(new KeyValuePair<string, JSObject>((d - pos1 + pos0 + addCount).ToString(), p.Value));
                                keysForDelete.Add(p.Key);
                            }
                        }
                    }
                    for (int i = 0; i < keysForDelete.Count; i++)
                        this.fields.Remove(keysForDelete[i]);
                    for (int i = 0; i < ritems.Count; i++)
                        this.fields[ritems[i].Key] = ritems[i].Value;
                    for (int i = 0; i < addCount; i++)
                        this.fields[(pos0 + i).ToString()] = args[i + 2];
                    pos0 += addCount + ritems.Count;
                    if (pos0 < 0x7fffffff)
                    {
                        lobj.iValue = (int)(pos0);
                        lobj.ValueType = JSObjectType.Int;
                    }
                    else
                    {
                        lobj.dValue = pos0;
                        lobj.ValueType = JSObjectType.Double;
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
                var res = Tools.JSObjectToInt(comparer.Invoke(JSObject.undefined, args));
                return res;
            }
        }

        public JSObject sort(JSObject args)
        {
            if (!(((object)this) is Array)) // Да, Array sealed, но тут и не такое возможно.
            {
                int l = System.Math.Max(0, Tools.JSObjectToInt(this.GetField("length", true, false)));
                var t = new Array(l);
                for (int i = 0; i < l; i++)
                {
                    var val = this.GetField(i.ToString(), true, false);
                    t.data[i] = val;
                }
                t.sort(args);
                for (int i = 0; i < l; i++)
                    this.fields[i.ToString()] = t.data[i];
                return this;
            }
            var length = args.GetField("length", false, true);
            var first = args.GetField("0", false, true);
            var len = Tools.JSObjectToInt(length);
            Function comparer = null;
            if (len > 0)
                comparer = args.GetField("0", true, false).Value as Function;
            if (comparer != null)
            {
                var second = args.GetField("1", false, true);
                second.assignCallback = null;
                first.assignCallback = null;
                length.assignCallback = null;
                args.fields.Clear();
                length.iValue = 2;
                length.ValueType = JSObjectType.Int;
                args.fields["length"] = length;
                args.fields["0"] = first;
                args.fields["1"] = second;

                int undefBlockStart = data.Count;
                for (int i = 0; i < undefBlockStart; i++)
                {
                    if (data[i] == null || data[i].ValueType <= JSObjectType.Undefined)
                    {
                        undefBlockStart--;
                        var t = data[i];
                        data[i] = data[undefBlockStart];
                        data[undefBlockStart] = t;
                    }
                }

                data.Sort(0, undefBlockStart, new JSComparer(args, first, second, comparer));
            }
            else
                data.Sort((JSObject l, JSObject r) =>
                {
                    return string.CompareOrdinal((l ?? "undefined").ToString(), (r ?? "undefined").ToString());
                });
            return this;
        }

        public JSObject unshift(JSObject[] args)
        {
            data.InsertRange(0, args);
            return length;
        }

        public override string ToString()
        {
            if (data.Count == 0)
                return "";
            var res = (data[0] ?? "").ToString();
            for (int i = 1; i < data.Count; i++)
                res += "," + (data[i] ?? "").ToString();
            return res;
        }

        public override JSObject toString(JSObject args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                throw new JSException(TypeProxy.Proxy(new TypeError("Try to call Array.toString on not Array object.")));
            return this.ToString();
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Modules.Hidden]
        public override JSObject valueOf()
        {
            return base.valueOf();
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            switch (name)
            {
                case "forEach":
                    {
                        return forEachCF;
                    }
            }
            int index = 0;
            double dindex = 0.0;
            if (name != "NaN" && name != "Infinity" && name != "-Infinity" &&
                Tools.ParseNumber(name, ref index, false, out dindex))
            {
                if (dindex >= 0)
                {
                    if (dindex > 0x7fffffff)
                        throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array index")));
                    if (((index = (int)dindex) == dindex))
                        return this[index];
                }
            }
            return base.GetField(name, fast, own);
        }

        #region functions with callback
        /*
         * В таких функциях необходимо делать callback, 
         * а для этого нам будет нужен контекст. 
         * Контекст можно получить только для CallableField'ов
         */

        [Hidden]
        private static readonly JSObject forEachCF = new ExternalFunction(forEach);

        [Hidden]
        private static JSObject forEach(Context context, JSObject args)
        {
            var alen = args.GetField("length", true, false).iValue;
            if (alen == 0)
                throw new ArgumentException("Undefined is not function");

            bool res = true;
            var obj = context.thisBind;
            var len = obj.GetField("length", true, false);
            if (len.ValueType == JSObjectType.Property)
                len = (len.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(obj, null);
            var count = Tools.JSObjectToDouble(len);
            var cbargs = JSObject.CreateObject();
            cbargs.GetField("length", false, true).Assign(3);
			var index = new JSObject(false) { ValueType = JSObjectType.Int };
            cbargs.GetField("1", false, true).Assign(index);
            cbargs.GetField("2", false, true).Assign(obj);
            var stat = args.GetField("0", true, false).oValue as Function;
            for (int i = 0; i < count; i++)
            {
                cbargs.GetField("0", false, true).Assign(obj.GetField(i < 10 ? Tools.NumString[i] : i.ToString(System.Globalization.CultureInfo.InvariantCulture), true, false));
                index.iValue = i;
                if (alen > 1)
                    res &= (bool)stat.Invoke(args.GetField("1", true, false), cbargs);
                else
                    res &= (bool)stat.Invoke(cbargs);
            }
            return res;
        }

        #endregion
    }
}