using System;

namespace NiL.JS.Core.BaseTypes
{
    internal class Array
    {
        [Modules.Hidden]
        private JSObject[] data;

        public Array()
        {
            data = new JSObject[0];
        }

        public Array(int length)
        {
            data = new JSObject[length];
            for (int i = 0; i < data.Length; i++)
                data[i] = new JSObject();
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
                else val = new NiL.JS.Modules.ClassProxy(args[i]);
                data[i] = val;
            }
        }

        public JSObject this[int index]
        {
            get
            {
                return data[index];
            }
            set
            {
                if (data.Length <= index)
                {
                    var t = new JSObject[index + 1];
                    for (int i = 0; i < data.Length; i++)
                        t[i] = data[i];
                    for (int i = data.Length; i < t.Length; i++)
                        t[i] = new JSObject();
                    data = t;
                }
                data[index].Assign(value);
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
                        t[i] = new JSObject();
                    data = t;
                }
            }
        }

        public string toString()
        {
            return ToString();
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
    }
}
