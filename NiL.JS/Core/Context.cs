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
        private static Dictionary<int, WeakReference> _executedContexts = new Dictionary<int, WeakReference>();
        internal static Context currentRootContext
        {
            get
            {
                WeakReference res = null;
                if (_executedContexts.TryGetValue(System.Threading.Thread.CurrentThread.ManagedThreadId, out res))
                    return res.Target as Context;
                else
                    return null;
            }
            set
            {
                if (value != null)
                    throw new InvalidOperationException();
                var cc = currentRootContext;
                GC.SuppressFinalize(cc);
                cc.threadid = 0;
                _executedContexts.Remove(System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        internal readonly static Context globalContext = new Context();
        public static Context GlobalContext { get { return globalContext; } }
        public static CallableField Eval { get; private set; }

        public static void RefreshGlobalContext()
        {
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            ThisObject.thisProto = null;
            JSObject.GlobalPrototype = null;
            TypeProxy.Clear();
            var Object = new ExternalFunction(JSObject.Object);
            JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
            var ctor = JSObject.GlobalPrototype.GetField("constructor", false, true);
            var oa = ctor.attributes;
            ctor.attributes = 0;
            ctor.Assign(Object);
            ctor.attributes = oa;
            Object.GetField("prototype", false, true).Assign(JSObject.GlobalPrototype);
            Object.GetField("prototype", false, true).attributes |= ObjectAttributes.ReadOnly | ObjectAttributes.DontDelete;
            globalContext.InitField("Object").Assign(Object);
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
            globalContext.InitField("eval").Assign(Eval = new CallableField((context, x) =>
            {
                int i = 0;
                string c = "{" + Tools.RemoveComments(x.GetField("0", true, false).ToString()) + "}";
                var cb = CodeBlock.Parse(new ParsingState(c), ref i).Statement;
                if (i != c.Length)
                    throw new System.ArgumentException("Invalid char");
                Parser.Optimize(ref cb, -1, null);
                var res = cb.Invoke(context);
                return res;
            }));
            globalContext.InitField("isNaN").Assign(new CallableField((t, x) =>
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
            globalContext.InitField("unescape").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpUtility.HtmlDecode(x.GetField("0", true, false).ToString());
            }));
            globalContext.InitField("escape").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpUtility.HtmlEncode(x.GetField("0", true, false).ToString());
            }));
            globalContext.InitField("encodeURI").Assign(new CallableField((t, x) =>
            {
                return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.UTF8Encoding.Default.GetBytes(x.GetField("0", true, false).ToString()));
            }));
            globalContext.InitField("encodeURIComponent").Assign(globalContext.GetField("encodeURI"));
            globalContext.InitField("decodeURI").Assign(new CallableField((t, x) =>
            {
                return System.Text.UTF8Encoding.Default.GetString(System.Web.HttpServerUtility.UrlTokenDecode(x.GetField("0", true, false).ToString()));
            }));
            globalContext.InitField("decodeURIComponent").Assign(globalContext.GetField("decodeURI"));
            globalContext.InitField("isFinite").Assign(new CallableField((t, x) =>
            {
                var d = Tools.JSObjectToDouble(x.GetField("0", true, false));
                return !double.IsNaN(d) && !double.IsInfinity(d);
            }));
            globalContext.InitField("parseFloat").Assign(new CallableField((t, x) =>
            {
                return Tools.JSObjectToDouble(x.GetField("0", true, false));
            }));
            globalContext.InitField("parseInt").Assign(new CallableField((t, x) =>
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

        protected readonly Context prototype;

        private int threadid = 0;

        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal bool updateThisBind;
        internal JSObject abortInfo;
        internal JSObject thisBind;
        internal bool strict;
        
        private Context()
        {
        }

        private JSObject define(string name)
        {
            JSObject res = null;
            var baseProto = JSObject.GlobalPrototype;
            if (baseProto != null)
            {
                res = baseProto.GetField(name, true, true);
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
        
        public virtual JSObject InitField(string name)
        {
            if (name == "this")
                return GetField(name);
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            JSObject res = null;
            if (!fields.TryGetValue(name, out res))
                fields[name] = res = new JSObject();
            Statements.GetVaribleStatement.ResetCache(name);
            res.attributes |= ObjectAttributes.DontDelete;
            return res;
        }

        /// <summary>
        /// Получает переменную, определённую в этом или одном из родительских объектов. 
        /// Если переменная не существовала, вернётся объект, после присваивания значения которому,
        /// будет создана переменная в базовом контексте выполнения (не в GlobalContext).
        /// </summary>
        /// <remarks>Делать fast версию этого метода не имеет смысла. 
        /// Если переменная была получена с целью прочитать значение, 
        /// то будет брошено исключение, трудоёмкость которого несравнимо больше трудоёмкости создания объекта.</remarks>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual JSObject GetField(string name)
        {
            JSObject res = null;
            if (name == "this")
            {
                var c = this;
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
                else if (thisBind.ValueType < JSObjectType.Undefined) // было "delete this". Просто вернём к жизни существующий объект
                    thisBind.ValueType = JSObjectType.Object;
                return thisBind;
            }
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && (prototype != null);
            if (fromProto)
                res = prototype.GetField(name);
            if (res == null)
            {
                if (this == globalContext)
                    return null;
                return define(name);
            }
            else
            {
                if (res.ValueType == JSObjectType.NotExistInObject)
                    res.ValueType = JSObjectType.NotExist;
                if (fromProto && fields != null)
                    fields[name] = res;
            }
            return res;
        }

        public void AttachModule(Type moduleType)
        {
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            fields.Add(moduleType.Name, TypeProxy.GetConstructor(moduleType));
            Statements.GetVaribleStatement.ResetCache(moduleType.Name);
            fields[moduleType.Name].attributes |= ObjectAttributes.DontDelete;
        }

        internal void ValidateThreadID()
        {
            if (threadid != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                if (prototype != null && prototype != globalContext)
                {
                    prototype.ValidateThreadID();
                }
                else
                {
                    if (threadid != 0)
                        _executedContexts.Remove(threadid);
                    _executedContexts[threadid = System.Threading.Thread.CurrentThread.ManagedThreadId] = new WeakReference(this);
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        internal Context(Context prototype)
        {
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.undefined;
            GC.SuppressFinalize(this);
        }

        ~Context()
        {
            _executedContexts.Remove(threadid);
        }
    }
}