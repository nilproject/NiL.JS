using System;
using System.Collections.Generic;
using System.Collections;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal class Array : EmbeddedType
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

        [Hidden]
        private List<JSObject> data;

        private Array(List<JSObject> collection)
        {
            data = collection;
        }

        public Array()
        {
            data = new List<JSObject>();
        }

        public Array(int length)
        {
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
        }

        public Array(double d)
        {
            if ((int)d != d)
                throw new ArgumentException("Invalid array length");
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
        }

        public Array(object[] args)
        {
            data = new List<JSObject>(args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                JSObject val;
                if (args[i] == null)
                    val = null;
                else if (args[i] is JSObject)
                    val = args[i] as JSObject;
                else if (args[i] is int)
                    val = (int)args[i];
                else if (args[i] is long)
                    val = (long)args[i];
                else if (args[i] is double)
                    val = (double)args[i];
                else if (args[i] is string)
                    val = (string)args[i];
                else if (args[i] is bool)
                    val = (bool)args[i];
                else
                    val = TypeProxy.Proxy(args[i]);
                data.Add(val);
            }
        }

        public void Add(JSObject obj)
        {
            data.Add(obj);
        }

        private int lastReqIndex;
        private JSObject tempElement;

        public JSObject this[int index]
        {
            get
            {
                if (data.Count <= index || data[index] == null)
                {
                    if (tempElement == null)
                    {
                        tempElement = new JSObject(false) { ValueType = JSObjectType.NotExistInObject };
                        tempElement.assignCallback = () =>
                        {
                            if (data.Count <= lastReqIndex)
                            {
                                data.Capacity = lastReqIndex + 1;
                                while (data.Count <= lastReqIndex)
                                    data.Add(null);
                            }
                            data[lastReqIndex] = tempElement;
                            tempElement.assignCallback = null;
                            tempElement = null;
                        };
                    }
                    lastReqIndex = index;
                    return tempElement;
                }
                else
                    return data[index];
            }
            internal set
            {
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
                else while (data.Count <= value)
                    data.Add(null);
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
                right.Value = data[i];
                if ((bool)opeq.Invoke(null))
                    return i;
            }
            return -1;
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
                right.Value = data[i];
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
            for (int i = 0; i < args.Length; i++)
            {
                var t = new JSObject();
                t.Assign(args[i]);
                data.Add(t);
            }
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

        public JSObject slice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            switch (args[0].ValueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        pos0 = args[0].iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        pos0 = (int)args[0].dValue;
                        break;
                    }
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.String:
                    {
                        double d;
                        Tools.ParseNumber(args[0].ToString(), ref pos0, false, out d);
                        pos0 = (int)d;
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos1 = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos1 = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), ref pos1, false, out d);
                            pos1 = (int)d;
                            break;
                        }
                }
                pos1 = System.Math.Min(pos1, data.Count);
            }
            else
                pos1 = data.Count;
            if (pos0 < 0)
                pos0 = data.Count + pos0;
            if (pos1 < 0)
                pos1 = data.Count + pos1;
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

        public JSObject sort(JSObject args)
        {
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
                data.Sort((JSObject l, JSObject r) =>
                {
                    first.Assign(l);
                    second.Assign(r);
                    return Tools.JSObjectToInt(comparer.Invoke(args));
                });
            }
            else
                data.Sort((JSObject l, JSObject r) => { return string.Compare(l.ToString(), r.ToString(), StringComparison.Ordinal); });
            return this;
        }

        public override string ToString()
        {
            if (data.Count == 0)
                return "";
            var res = (data[0].Value ?? "").ToString();
            for (int i = 1; i < data.Count; i++)
                res += "," + (data[i].Value ?? "").ToString();
            return res;
        }

        public override JSObject toString()
        {
            return ToString();
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return new Enumerator(this);
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
            if (Tools.ParseNumber(name, ref index, false, out dindex) && ((index = (int)dindex) == dindex))
                return this[index];
            else
                return base.GetField(name, fast, own);
        }

        #region functions with callback
        /*
         * В таких функциях необходимо делать callback, 
         * а для этого нам будет нужен контекст. 
         * Контекст можно получить только для CallableField'ов
         */

        [Hidden]
        private static readonly JSObject forEachCF = new CallableField(forEach);

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
            var cbargs = new JSObject[3];
            cbargs[2] = obj;
            cbargs[1] = new JSObject(false) { ValueType = JSObjectType.Int };
            var stat = args.GetField("0", true, false).oValue as Function;
            for (int i = 0; i < count; i++)
            {
                cbargs[0] = obj.GetField(i.ToString(System.Globalization.CultureInfo.InvariantCulture), true, false);
                cbargs[1].iValue = i;
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