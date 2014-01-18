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
        internal static CallableField eval;
        public static Context GlobalContext { get { return globalContext; } }

        public static void RefreshGlobalContext()
        {
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            
            BaseObject.RegisterTo(globalContext);
            globalContext.AttachModule(typeof(Date));
            globalContext.AttachModule(typeof(BaseTypes.Array));
            globalContext.AttachModule(typeof(BaseTypes.String));
            globalContext.AttachModule(typeof(BaseTypes.Number));
            globalContext.AttachModule(typeof(BaseTypes.Function));
            globalContext.AttachModule(typeof(BaseTypes.Boolean));

            #region Base Function
            globalContext.GetField("eval").Assign(eval = new CallableField((cont, x) =>
            {
                int i = 0;
                string c = "{" + Tools.RemoveComments(x.GetField("0", true, false).ToString()) + "}";
                var cb = CodeBlock.Parse(new ParsingState(c), ref i).Statement;
                if (i != c.Length)
                    throw new System.ArgumentException("Invalid char");
                Parser.Optimize(ref cb, null);
                var res = cb.Invoke(cont);
                return res;
            }));
            globalContext.GetField("isNaN").Assign(new CallableField((t, x) =>
            {
                var r = x.GetField("0", true, false);
                if (r.ValueType == JSObjectType.Double)
                    return double.IsNaN(r.dValue);
                if (r.ValueType == JSObjectType.Bool || r.ValueType == JSObjectType.Int || r.ValueType == JSObjectType.Date)
                    return false;
                if (r.ValueType == JSObjectType.String)
                {
                    double d = 0;
                    int i = 0;
                    if (Tools.ParseNumber(r.oValue as string, ref i, false, out d))
                        return double.IsNaN(d);
                    return true;
                }
                return true;
            }));
            #endregion
            #region Base types
            globalContext.GetField("RegExp").Assign(new CallableField((t, x) =>
            {
                var pattern = x.GetField("0", true, false).Value.ToString();
                var flags = x.GetField("length", false, true).iValue > 1 ? x.GetField("1", true, false).Value.ToString() : "";
                var re = new System.Text.RegularExpressions.Regex(pattern,
                    System.Text.RegularExpressions.RegexOptions.ECMAScript
                    | (flags.IndexOf('i') != -1 ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : 0)
                    | (flags.IndexOf('m') != -1 ? System.Text.RegularExpressions.RegexOptions.Multiline : 0)
                    );
                JSObject res = new JSObject();
                res.prototype = globalContext.GetField("RegExp").GetField("prototype", true, false);
                res.ValueType = JSObjectType.Object;
                res.oValue = re;
                var field = res.GetField("global", false, true);
                field.Protect();
                field.ValueType = JSObjectType.Bool;
                field.iValue = flags.IndexOf('g') != -1 ? 1 : 0;
                field = res.GetField("ignoreCase", false, false);
                field.Protect();
                field.ValueType = JSObjectType.Bool;
                field.iValue = (re.Options & System.Text.RegularExpressions.RegexOptions.IgnoreCase) != 0 ? 1 : 0;
                field = res.GetField("multiline", false, true);
                field.Protect();
                field.ValueType = JSObjectType.Bool;
                field.iValue = (re.Options & System.Text.RegularExpressions.RegexOptions.Multiline) != 0 ? 1 : 0;
                field = res.GetField("source", false, true);
                field.Assign(pattern);
                field.Protect();
                return res;
            }));
            var rep = globalContext.GetField("RegExp").GetField("prototype", false, true);
            rep.Assign(null);
            rep.prototype = BaseObject.Prototype;
            rep.ValueType = JSObjectType.Object;
            rep.oValue = new object();
            rep.GetField("exec", false, true).Assign(new CallableField((cont, args) =>
            {
                if (args.GetField("length", false, true).iValue == 0)
                    return new JSObject() { ValueType = JSObjectType.Object };
                var m = ((cont.thisBind ?? cont.GetField("this")).oValue as System.Text.RegularExpressions.Regex).Match(args.GetField("0", true, false).Value.ToString());
                var mres = new JSObject();
                mres.ValueType = JSObjectType.Object;
                if (m.Groups.Count != 1)
                {
                    mres.oValue = new string[] { m.Groups[1].Value };
                    mres.GetField("index", false, true).Assign(m.Groups[1].Index);
                    mres.GetField("input", false, true).Assign(m.Groups[0].Value);
                }
                return mres;
            }));
            #endregion
            #region Consts
            globalContext.fields["undefined"] = JSObject.undefined;
            globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
            globalContext.fields["NaN"] = Number.NaN;
            globalContext.fields["null"] = JSObject.Null;
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
        internal bool updateThisBind;
        internal JSObject abortInfo;
        internal JSObject thisBind;

        private JSObject define(string name)
        {
            var res = new JSObject() { ValueType = JSObjectType.NotExist };
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
                res.attributes |= ObjectAttributes.DontDelete;
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
                {
                    if (prototype == null || prototype == globalContext)
                        thisBind = new ThisObject(this);
                    else
                        thisBind = prototype.GetField(name);
                }
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
                if (res.ValueType == JSObjectType.NotExistInObject)
                    res.ValueType = JSObjectType.NotExist;
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
            fields.Add(moduleType.Name, new TypeProxy(moduleType));
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