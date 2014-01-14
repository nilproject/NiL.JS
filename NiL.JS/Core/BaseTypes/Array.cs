using System;
using System.Collections.Generic;
using System.Collections;

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
                while (index < owner.data.Length && (owner.data[index] == null || owner.data[index].ValueType < ObjectValueType.Undefined));
                return index < owner.data.Length;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {

            }
        }

        [Modules.Hidden]
        private JSObject[] data;

        public Array()
        {
            data = new JSObject[0];
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array(int length)
        {
            data = new JSObject[length];
            for (int i = 0; i < data.Length; i++)
                data[i] = null;
            ValueType = ObjectValueType.Object;
            oValue = this;
        }

        public Array(double d)
        {
            throw new ArgumentException("Invalid array length");
        }

        public Array(object[] args)
        {
            data = new JSObject[args.Length];
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
                    val = Modules.TypeProxy.Proxy(args[i]);
                data[i] = val;
            }
        }

        public JSObject this[int index]
        {
            get
            {
                return data[index] ?? JSObject.undefined;
            }
            set
            {
                if (data.Length <= index)
                {
                    var t = new JSObject[index + 1];
                    for (int i = 0; i < data.Length; i++)
                        t[i] = data[i];
                    for (int i = data.Length; i < t.Length; i++)
                        t[i] = null;
                    data = t;
                }
                (data[index] ?? (data[index] = new JSObject())).Assign(value);
            }
        }

        public int length
        {
            get
            {
                return data.Length;
            }
            set
            {
                if (data.Length != value)
                {
                    var t = new JSObject[value];
                    for (int i = 0; i < data.Length; i++)
                        t[i] = data[i];
                    for (int i = data.Length; i < t.Length; i++)
                        t[i] = null;
                    data = t;
                }
            }
        }

        public override string ToString()
        {
            if (data.Length == 0)
                return "";
            var res = (data[0].Value ?? "").ToString();
            for (int i = 1; i < data.Length; i++)
                res += "," + (data[i].Value ?? "").ToString();
            return res;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}