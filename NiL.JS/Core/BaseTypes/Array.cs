using System;
using System.Collections.Generic;
using System.Collections;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal class Array : EmbeddedType, IEnumerable<string>
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
            
            public object Current { get { return index.ToString(); } }

            string IEnumerator<string>.Current { get { return index.ToString(); } }

            public bool MoveNext()
            {
                do
                    index++;
                while (index < owner.data.Count && (owner.data[index] == null || owner.data[index].ValueType < ObjectValueType.Undefined));
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
        internal List<JSObject> data;

        private Array(List<JSObject> collection)
        {
            data = collection;
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array()
        {
            data = new List<JSObject>();
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array(int length)
        {
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array(double d)
        {
            if ((int)d != d)
                throw new ArgumentException("Invalid array length");
            data = new List<JSObject>(length);
            for (int i = 0; i < length; i++)
                data.Add(null);
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array(object[] args)
        {
            data = new List<JSObject>(args.Length);
            ValueType = ObjectValueType.Object;
            oValue = this;
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
                else if (args[i] is ContextStatement)
                    val = (JSObject)(ContextStatement)args[i];
                else
                    val = TypeProxy.Proxy(args[i]);
                data.Add(val);
            }
        }

        private static int lastReqIndex;
        private static JSObject tempElement;

        public JSObject this[int index]
        {
            get
            {
                if (data.Count <= index || data[index] == null)
                {
                    if (tempElement == null)
                    {
                        tempElement = new JSObject(false) { ValueType = ObjectValueType.NotExistInObject };
                        tempElement.assignCallback = () =>
                        {
                            while (data.Count <= lastReqIndex)
                                data.Add(null);
                            data[lastReqIndex] = tempElement;
                            tempElement.assignCallback = null;
                            tempElement = null;
                            return true;
                        };
                    }
                    lastReqIndex = index;
                    return tempElement;
                }
                else
                    return data[index];
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
                var arg = args[i].firstContainer ?? args[i];
                if (arg is Array)
                {
                    Array arr = arg as Array;
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
                    case ObjectValueType.Int:
                    case ObjectValueType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case ObjectValueType.Object:
                    case ObjectValueType.Date:
                    case ObjectValueType.Statement:
                    case ObjectValueType.String:
                        {
                            double d;
                            Parser.ParseNumber(args[1].ToString(), ref pos, false, out d);
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
                    case ObjectValueType.Int:
                    case ObjectValueType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case ObjectValueType.Object:
                    case ObjectValueType.Date:
                    case ObjectValueType.Statement:
                    case ObjectValueType.String:
                        {
                            double d;
                            Parser.ParseNumber(args[1].ToString(), ref pos, false, out d);
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
                case ObjectValueType.Int:
                case ObjectValueType.Bool:
                    {
                        pos0 = args[0].iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        pos0 = (int)args[0].dValue;
                        break;
                    }
                case ObjectValueType.Object:
                case ObjectValueType.Date:
                case ObjectValueType.Statement:
                case ObjectValueType.String:
                    {
                        double d;
                        Parser.ParseNumber(args[0].ToString(), ref pos0, false, out d);
                        pos0 = (int)d;
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1].ValueType)
                {
                    case ObjectValueType.Int:
                    case ObjectValueType.Bool:
                        {
                            pos1 = args[1].iValue;
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            pos1 = (int)args[1].dValue;
                            break;
                        }
                    case ObjectValueType.Object:
                    case ObjectValueType.Date:
                    case ObjectValueType.Statement:
                    case ObjectValueType.String:
                        {
                            double d;
                            Parser.ParseNumber(args[1].ToString(), ref pos1, false, out d);
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

        public override string ToString()
        {
            if (data.Count == 0)
                return "";
            var res = (data[0].Value ?? "").ToString();
            for (int i = 1; i < data.Count; i++)
                res += "," + (data[i].Value ?? "").ToString();
            return res;
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
            if (Parser.ParseNumber(name, ref index, false, out dindex) && ((index = (int)dindex) == dindex))
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
        private static JSObject forEach(Context context, JSObject[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Undefined is not function");

            bool res = true;
            var obj = context.thisBind.firstContainer ?? context.thisBind;
            var len = obj.GetField("length", true);
            if (len.ValueType == ObjectValueType.Property)
                len = (len.oValue as Statement[])[1].Invoke(context, null);
            var count = Tools.jsobjectToDouble(len);
            var cbargs = new JSObject[3];
            cbargs[2] = obj;
            cbargs[1] = new JSObject(false) { ValueType = ObjectValueType.Int };
            var stat = args[0].oValue as Statement;
            var cstat = stat as ContextStatement;
            for (int i = 0; i < count; i++)
            {
                cbargs[0] = obj.GetField(i.ToString());
                cbargs[1].iValue = i;
                if (cstat != null)
                {
                    if (args.Length > 1)
                    {
                        var oldtb = cstat.Context.thisBind;
                        cstat.Context.thisBind = args[1];
                        res &= (bool)cstat.Invoke(cbargs);
                        cstat.Context.thisBind = oldtb;
                    }
                    else
                        res &= (bool)cstat.Invoke(cbargs);
                }
                else
                {
                    if (args.Length > 1)
                    {
                        var oldtb = context.thisBind;
                        context.thisBind = args[1];
                        res &= (bool)stat.Invoke(context, cbargs);
                        context.thisBind = oldtb;
                    }
                    else
                        res &= (bool)stat.Invoke(context, cbargs);
                }
            }
            return res;
        }

        #endregion
    }
}