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

    public class Context
    {
        internal static Context currentRootContext = null;
        internal readonly static Context globalContext = new Context();
        public static Context GlobalContext { get { return globalContext; } }

        public static void RefreshGlobalContext()
        {
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            
            BaseObject.RegisterTo(globalContext);
            globalContext.AttachModule(typeof(BaseTypes.Date));
            globalContext.AttachModule(typeof(BaseTypes.Array));
            globalContext.AttachModule(typeof(BaseTypes.String));
            globalContext.AttachModule(typeof(BaseTypes.Number));
            globalContext.AttachModule(typeof(BaseTypes.Function));
            globalContext.AttachModule(typeof(BaseTypes.Boolean));
            globalContext.AttachModule(typeof(BaseTypes.Error));
            globalContext.AttachModule(typeof(BaseTypes.TypeError));
            globalContext.AttachModule(typeof(BaseTypes.ReferenceError));
            globalContext.AttachModule(typeof(BaseTypes.EvalError));
            globalContext.AttachModule(typeof(BaseTypes.RangeError));
            globalContext.AttachModule(typeof(BaseTypes.URIError));
            globalContext.AttachModule(typeof(BaseTypes.SyntaxError));
            globalContext.AttachModule(typeof(BaseTypes.RegExp));
            globalContext.AttachModule(typeof(Modules.Math));
            globalContext.AttachModule(typeof(Modules.console));

            #region Base Function
            globalContext.GetField("eval").Assign(new CallableField((context, x) =>
            {
                int i = 0;
                string c = "{" + Tools.RemoveComments(x.GetField("0", true, false).ToString()) + "}";
                var cb = CodeBlock.Parse(new ParsingState(c), ref i).Statement;
                if (i != c.Length)
                    throw new System.ArgumentException("Invalid char");
                Parser.Optimize(ref cb, null);
                var res = cb.Invoke(context);
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
            globalContext.GetField("unescape").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpUtility.HtmlDecode(x.GetField("0", true, false).ToString());
            }));
            globalContext.GetField("escape").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpUtility.HtmlEncode(x.GetField("0", true, false).ToString());
            }));
            globalContext.GetField("encodeURI").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.UTF8Encoding.Default.GetBytes(x.GetField("0", true, false).ToString()));
            }));
            globalContext.GetField("encodeURIComponent").Assign(globalContext.GetField("encodeURI"));
            globalContext.GetField("decodeURI").Assign(new CallableField((t, x) =>
            {
                return System.Text.UTF8Encoding.Default.GetString(System.Web.HttpServerUtility.UrlTokenDecode(x.GetField("0", true, false).ToString()));
            }));
            globalContext.GetField("decodeURIComponent").Assign(globalContext.GetField("decodeURI"));
            globalContext.GetField("isFinite").Assign(new CallableField((t, x) =>
            {
                return !double.IsInfinity(Tools.JSObjectToDouble(x.GetField("0", true, false)));
            }));
            globalContext.GetField("parseFloat").Assign(new CallableField((t, x) =>
            {
                return Tools.JSObjectToDouble(x.GetField("0", true, false));
            }));
            globalContext.GetField("parseInt").Assign(new CallableField((t, x) =>
            {
                var r = x.GetField("0", true, false);
                for (; ; )
                    switch (r.ValueType)
                    {
                        case JSObjectType.Bool:
                        case JSObjectType.Int:
                            {
                                return r.iValue;
                            }
                        case JSObjectType.Double:
                            {
                                if (double.IsNaN(r.dValue) || double.IsInfinity(r.dValue))
                                    return 0;
                                return (int)((long)r.dValue & 0xFFFFFFFF);
                            }
                        case JSObjectType.String:
                            {
                                double dres = 0;
                                int ix = 0;
                                string s = (r.oValue as string).Trim();
                                if (!Tools.ParseNumber(s, ref ix, true, out dres, Tools.JSObjectToInt(x.GetField("1", true, false)), true))
                                    return 0;
                                return (int)dres;
                            }
                        case JSObjectType.Date:
                        case JSObjectType.Function:
                        case JSObjectType.Object:
                            {
                                if (r.oValue == null)
                                    return 0;
                                r = r.ToPrimitiveValue_Value_String();
                                break;
                            }
                        case JSObjectType.Undefined:
                        case JSObjectType.NotExistInObject:
                            return 0;
                        default:
                            throw new NotImplementedException();
                    }
            }));
            #endregion
            #region Consts
            globalContext.fields["undefined"] = JSObject.undefined;
            globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
            globalContext.fields["NaN"] = Number.NaN;
            globalContext.fields["null"] = JSObject.Null;
            #endregion

            foreach (var v in globalContext.fields.Values)
                v.attributes |= ObjectAttributes.DontEnum;
        }

        static Context()
        {
            RefreshGlobalContext();
        }

        private readonly Context prototype;
        private const int cacheSize = 5;

        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal bool updateThisBind;
        internal JSObject abortInfo;
        internal JSObject thisBind;
        
        private Context()
        {
        }

        private JSObject define(string name)
        {
            JSObject res = null;
            if (BaseTypes.BaseObject.Prototype != null)
            {
                res = BaseTypes.BaseObject.Prototype.GetField(name, true, true);
                if (res == JSObject.undefined)
                    res = new JSObject() { ValueType = JSObjectType.NotExist };
                else
                    res = res.Clone() as JSObject;
            }
            else
                res = new JSObject() { ValueType = JSObjectType.NotExist };
            res.assignCallback = () =>
            {
                if (fields == null)
                    fields = new Dictionary<string, JSObject>();
                fields[name] = res;
                res.assignCallback = null;
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
            abort = AbortType.None;
            abortInfo = JSObject.undefined;
        }

        public virtual JSObject GetField(string name)
        {
            JSObject res = null;
            var c = this;
            if (name == "this")
            {
                if (thisBind == null)
                {
                    for (; ; )
                    {
                        if (c.prototype == globalContext)
                        {
                            thisBind = new ThisObject(c);
                            c.thisBind = thisBind;
                            break;
                        }
                        else
                            c = c.prototype;
                        if (c.thisBind != null)
                            thisBind = c.thisBind;
                    }
                }
                else if (thisBind.ValueType <= JSObjectType.Undefined) // было "delete this". Просто вернём к жизни существующий объект
                    thisBind.ValueType = JSObjectType.Object;
                return thisBind;
            }
            var scriptRoot = this;
            while (((c.fields == null) || !c.fields.TryGetValue(name, out res)) && (c.prototype != null))
            {
                c = c.prototype;
                if (c != globalContext)
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
            return res;
        }

        public void AttachModule(Type moduleType)
        {
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            fields.Add(moduleType.Name, TypeProxy.GetConstructor(moduleType));
        }

        internal Context(Context prototype)
        {
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.undefined;
        }
    }
}