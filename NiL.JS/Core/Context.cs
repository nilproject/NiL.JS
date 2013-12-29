using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;
using System.Collections.Generic;
using System;

namespace NiL.JS.Core
{
    internal enum AbortType
    {
        None = 0,
        Continue,
        Break,
        Return,
        Exception,
    }

    public sealed class Context
    {
        private struct _cacheItem
        {
            public string name;
            public JSObject value;
        }

        internal readonly static Context globalContext = new Context();
        public static Context GlobalContext { get { return globalContext; } }

        public static void RefreshGlobalContext()
        {
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            #region Base Function
            globalContext.GetField("eval").Assign(new CallableField((t, x) =>
            {
                int i = 0;
                string c = "{" + Parser.RemoveComments(x[0].Invoke().oValue.ToString()) + "}";
                var cb = CodeBlock.Parse(new ParsingState(c), ref i).Statement;
                Parser.Optimize(ref cb, null);
                var res = cb.Invoke((x[0] as ContextStatement).Context);
                if (i != c.Length)
                    throw new System.ArgumentException("Invalid char");
                return res;
            }));
            globalContext.GetField("isNaN").Assign(new CallableField((t, x) =>
            {
                var r = x[0].Invoke();
                if (r.ValueType == ObjectValueType.Double)
                    return double.IsNaN(r.dValue);
                if (r.ValueType == ObjectValueType.Bool || r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Date)
                    return false;
                if (r.ValueType == ObjectValueType.String)
                {
                    double d = 0;
                    int i = 0;
                    if (Parser.ParseNumber(r.oValue as string, ref i, false, out d))
                        return double.IsNaN(d);
                    return true;
                }
                return true;
            }));
            globalContext.GetField("Number").Assign(new CallableField((t, x) =>
            {
                if (x.Length > 0)
                {
                    var r = x[0].Invoke();
                    if (r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Bool)
                        return r.iValue;
                    else if (r.ValueType == ObjectValueType.Double)
                        return (int)r.dValue;
                    else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                        return 0;
                    else if ((r.ValueType == ObjectValueType.String) && (r.oValue is string))
                    {
                        int cc = 0;
                        string s = r.oValue as string;
                        if (!int.TryParse(s, out cc) && (s.Length > 2) && (s[0] == '0') && (s[1] == 'x'))
                            int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out cc);
                        return cc;
                    }
                    else throw new System.InvalidCastException("Cannot convert object to primitive value");
                }
                return 0;
            }));
            #endregion
            #region Base types
            globalContext.GetField("Boolean").Assign(new CallableField((t, x) =>
            {
                var temp = x[0].Invoke();
                if (temp.ValueType == ObjectValueType.Bool)
                    return temp;
                return (bool)temp;
            }));
            globalContext.GetField("RegExp").Assign(new CallableField((t, x) =>
            {
                var pattern = x[0].Invoke().Value.ToString();
                var flags = x.Length > 1 ? x[1].Invoke().Value.ToString() : "";
                var re = new System.Text.RegularExpressions.Regex(pattern,
                    System.Text.RegularExpressions.RegexOptions.ECMAScript
                    | (flags.IndexOf('i') != -1 ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : 0)
                    | (flags.IndexOf('m') != -1 ? System.Text.RegularExpressions.RegexOptions.Multiline : 0)
                    );
                JSObject res = new JSObject();
                res.prototype = globalContext.GetField("RegExp").GetField("prototype", true);
                res.ValueType = ObjectValueType.Object;
                res.oValue = re;
                var field = res.GetField("global");
                field.Protect();
                field.ValueType = ObjectValueType.Bool;
                field.iValue = flags.IndexOf('g') != -1 ? 1 : 0;
                field = res.GetField("ignoreCase");
                field.Protect();
                field.ValueType = ObjectValueType.Bool;
                field.iValue = (re.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? 1 : 0;
                field = res.GetField("multiline");
                field.Protect();
                field.ValueType = ObjectValueType.Bool;
                field.iValue = (re.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? 1 : 0;
                field = res.GetField("source");
                field.Protect();
                field.Assign(pattern);
                return res;
            }));
            var rep = globalContext.GetField("RegExp").GetField("prototype");
            rep.Assign(null);
            rep.ValueType = ObjectValueType.Object;
            rep.oValue = new object();
            rep.GetField("exec").Assign(new CallableField((_this, args) =>
            {
                if (args.Length == 0)
                    return new JSObject() { ValueType = ObjectValueType.Object };
                var m = (_this.oValue as System.Text.RegularExpressions.Regex).Match(args[0].Invoke().Value.ToString());
                var mres = new JSObject();
                mres.ValueType = ObjectValueType.Object;
                if (m.Groups.Count != 1)
                {
                    mres.oValue = new string[] { m.Groups[1].Value };
                    mres.GetField("index").Assign(m.Groups[1].Index);
                    mres.GetField("input").Assign(m.Groups[0].Value);
                }
                return mres;
            }));

            BaseObject.RegisterTo(globalContext);
            globalContext.AttachModule(typeof(Date));
            globalContext.AttachModule(typeof(BaseTypes.Array));
            globalContext.AttachModule(typeof(BaseTypes.String));
            #endregion
            #region Consts
            var nan = globalContext.GetField("NaN");
            nan.Protect();
            nan.ValueType = ObjectValueType.Double;
            nan.dValue = double.NaN;
            globalContext.Define("undefined").Protect();
            #endregion

            globalContext.AttachModule(typeof(Modules.Math));
            globalContext.AttachModule(typeof(Modules.console));
        }

        static Context()
        {
            RefreshGlobalContext();
        }

        internal readonly Context prototype;
        private const int cacheSize = 5;
        [NonSerialized]
        private int cacheIndex;
        [NonSerialized]
        private _cacheItem[] cache;

        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal JSObject abortInfo;
        internal JSObject thisBind;

        private JSObject define(string name)
        {
            var res = new JSObject() { ValueType = ObjectValueType.NotExist };
            res.assignCallback = () =>
            {
                if (fields == null)
                    fields = new Dictionary<string, JSObject>();
                fields[name] = res;
                res.assignCallback = null;
                return true;
            };
            return res;
        }

        internal JSObject Define(string name)
        {
            JSObject res;
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            if (!fields.TryGetValue(name, out res))
            {
                res = new JSObject();
                fields[name] = res;
                cache[cacheIndex++] = new _cacheItem() { name = name, value = res };
                cacheIndex %= cacheSize;
            }
            return res;
        }

        internal JSObject Assign(string name, JSObject value)
        {
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            fields[name] = value;
            return value;
        }

        internal void Clear()
        {
            if (fields != null)
                fields.Clear();
            for (int i = 0; i < cacheSize; i++)
                cache[i] = new _cacheItem();
            abort = AbortType.None;
            abortInfo = JSObject.undefined;
        }

        public JSObject GetField(string name)
        {
            JSObject res = null;
            if (name == "this")
            {
                if (thisBind == null)
                    thisBind = new ThisObject(this);
                return thisBind;
            }
            for (int i = 0; i < cacheSize; i++)
            {
                if (cache[i].name == name)
                    return cache[i].value;
            }
            var scriptRoot = this;
            var c = this;
            while (((c.fields == null) || !c.fields.TryGetValue(name, out res)) && (c.prototype != null))
            {
                c = c.prototype;
                if (c != Context.globalContext)
                    scriptRoot = c;
            }
            if (res == null)
                return scriptRoot.define(name);
            else
            {
                if (res.ValueType == ObjectValueType.NotExistInObject)
                    res.ValueType = ObjectValueType.NotExist;
                if ((c != this) && (fields != null))
                    fields[name] = res;
            }
            cache[cacheIndex++] = new _cacheItem() { name = name, value = res };
            cacheIndex %= cacheSize;
            return res;
        }

        public void AttachModule(Type moduleType)
        {
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            fields.Add(moduleType.Name, new Modules.ClassProxy(moduleType));
        }

        private Context()
        {
            cache = new _cacheItem[cacheSize];
        }

        internal Context(Context prototype)
        {
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.undefined;
            cache = new _cacheItem[cacheSize];
        }
    }
}