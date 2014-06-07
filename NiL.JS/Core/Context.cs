using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Threading;

namespace NiL.JS.Core
{
    [Serializable]
    internal enum AbortType
    {
        None = 0,
        Continue,
        Break,
        Return,
        Exception,
    }

    /// <summary>
    /// Контекст выполнения скрипта. Хранит состояние выполнения сценария.
    /// </summary>
    [Serializable]
    public class Context : IEnumerable<string>
    {
#if NET35
        private static IDictionary<int, WeakReference> _executedContexts = new BinaryTree<int, WeakReference>();
#else
        private static IDictionary<int, WeakReference> _executedContexts = new System.Collections.Concurrent.ConcurrentDictionary<int, WeakReference>();
#endif

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
        /// <summary>
        /// Глобальный контекст выполнения. Хранит глобальные объекты, определённые спецификацией.
        /// Создание переменных в этом контексте невозможно во время выполнения скрипта.
        /// </summary>
        public static Context GlobalContext { get { return globalContext; } }

        /// <summary>
        /// Очищает глобальный контекст, после чего создаёт в нём глобальные объекты, определённые спецификацией, 
        /// </summary>
        public static void RefreshGlobalContext()
        {
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            else
                globalContext.fields = new Dictionary<string, JSObject>();
            ThisObject.thisProto = null;
            JSObject.GlobalPrototype = null;
            TypeProxy.Clear();
            globalContext.fields.Add("Object", TypeProxy.GetConstructor(typeof(JSObject)));
            globalContext.fields["Object"].attributes |= JSObjectAttributes.DontDelete;
            JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
            JSObject.GlobalPrototype.attributes |= JSObjectAttributes.ReadOnly;
            globalContext.AttachModule(typeof(BaseTypes.Date));
            globalContext.AttachModule(typeof(BaseTypes.Array));
            globalContext.AttachModule(typeof(BaseTypes.ArrayBuffer));
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
            globalContext.AttachModule(typeof(Modules.JSON));
            globalContext.AttachModule(typeof(Modules.console));

            #region Base Function
            globalContext.InitField("eval").Assign(new ExternalFunction((context, x) => context.eval(x)));
            globalContext.InitField("isNaN").Assign(new ExternalFunction((t, x) =>
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
            globalContext.InitField("unescape").Assign(new ExternalFunction((t, x) =>
            {
                return System.Web.HttpUtility.HtmlDecode(x.GetField("0", true, false).ToString());
            }));
            globalContext.InitField("escape").Assign(new ExternalFunction((t, x) =>
            {
                return System.Web.HttpUtility.HtmlEncode(x.GetField("0", true, false).ToString());
            }));
            globalContext.InitField("encodeURI").Assign(new ExternalFunction((t, x) =>
            {
                return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.UTF8Encoding.Default.GetBytes(x.GetField("0", true, false).ToString()));
            }));
            globalContext.InitField("encodeURIComponent").Assign(globalContext.GetField("encodeURI"));
            globalContext.InitField("decodeURI").Assign(new ExternalFunction((t, x) =>
            {
                return System.Text.UTF8Encoding.Default.GetString(System.Web.HttpServerUtility.UrlTokenDecode(x.GetField("0", true, false).ToString()));
            }));
            globalContext.InitField("decodeURIComponent").Assign(globalContext.GetField("decodeURI"));
            globalContext.InitField("isFinite").Assign(new ExternalFunction((t, x) =>
            {
                var d = Tools.JSObjectToDouble(x.GetField("0", true, false));
                return !double.IsNaN(d) && !double.IsInfinity(d);
            }));
            globalContext.InitField("parseFloat").Assign(new ExternalFunction((t, x) =>
            {
                return Tools.JSObjectToDouble(x.GetField("0", true, false));
            }));
            globalContext.InitField("parseInt").Assign(new ExternalFunction((t, x) =>
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
            globalContext.InitField("__pinvoke").Assign(new ExternalFunction(__pinvoke));
            #endregion
            #region Consts
            globalContext.fields["undefined"] = JSObject.undefined;
            globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
            globalContext.fields["NaN"] = Number.NaN;
            globalContext.fields["null"] = JSObject.Null;
            #endregion

            foreach (var v in globalContext.fields.Values)
                v.attributes |= JSObjectAttributes.DontEnum;
        }

        private static JSObject __pinvoke(Context context, JSObject args)
        {
            var argsCount = Tools.JSObjectToInt(args.GetField("length", true, false));
            var threadsCount = 1;
            if (argsCount == 0)
                return null;
            if (argsCount > 1)
                threadsCount = Tools.JSObjectToInt(args.GetField("1", true, false));
            var function = args.GetField("0", true, false).oValue as Function;
            Thread[] threads = null;
            if (function != null && threadsCount > 0)
            {
                threads = new Thread[threadsCount];
                for (var i = 0; i < threadsCount; i++)
                {
                    (threads[i] = new Thread((o) =>
                    {
                        var targs = new JSObject(true)
                        {
                            oValue = Arguments.Instance,
                            ValueType = JSObjectType.Object,
                            attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum
                        };
                        targs.fields["length"] = new Number(1) { assignCallback = null, attributes = JSObjectAttributes.DontEnum };
                        targs.fields["0"] = new Number((int)o) { assignCallback = null, attributes = JSObjectAttributes.Argument };
                        (targs.fields["callee"] = new JSObject()
                        {
                            ValueType = JSObjectType.Function,
                            oValue = function,
                            attributes = JSObjectAttributes.DontEnum
                        }).Protect();
                        function.Invoke(context, targs);
                    }) { Name = "NiL.JS __pinvoke thread (" + __pinvokeCalled + ":" + i + ")" }).Start(i);
                }
                __pinvokeCalled++;
            }
            return TypeProxy.Proxy(new
            {
                isAlive = new Func<JSObject, bool>((arg) =>
                {
                    if (threads == null)
                        return false;
                    argsCount = Tools.JSObjectToInt(arg.GetField("length", true, false));
                    if (argsCount == 0)
                    {
                        for (int i = 0; i < threads.Length; i++)
                        {
                            if (threads[i].IsAlive)
                                return true;
                        }
                    }
                    else
                    {
                        var threadIndex = Tools.JSObjectToInt(args.GetField("0", true, false));
                        if (threadIndex < threads.Length && threadIndex >= 0)
                            return threads[threadIndex].IsAlive;
                    }
                    return false;
                }),
                waitEnd = new Action(() =>
                {
                    if (threads == null)
                        return;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (threads[i].IsAlive)
                        {
                            Thread.Sleep(1);
                            i = -1;
                        }
                    }
                })
            });
        }

        static Context()
        {
            RefreshGlobalContext();
        }

        protected readonly Context prototype;

        private int threadid = 0;
        private static uint __pinvokeCalled;

        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal JSObject objectSource;
        internal JSObject abortInfo;
        internal JSObject thisBind;
        internal bool strict;
        internal bool debugging;
        internal Statement owner;

        /// <summary>
        /// Событие, возникающее при попытке выполнения оператора "debugger".
        /// </summary>
        public event DebuggerCallback DebuggerCallback;

        internal void raiseDebugger(Statement nextStatement)
        {
            var p = this;
            while (p != null)
            {
                if (p.debugging && p.DebuggerCallback != null)
                    p.DebuggerCallback(this, nextStatement);
                p = p.prototype;
            }
        }

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
            res.lastRequestedName = name;
            res.assignCallback = (sender) =>
            {
                if (fields == null)
                    fields = new Dictionary<string, JSObject>();
                fields[sender.lastRequestedName] = sender;
                sender.assignCallback = null;
            };
            return res;
        }

        /// <summary>
        /// Действие аналогично функции GeField с тем отличием, что возвращённое поле всегда определено в указанном контектсе.
        /// </summary>
        /// <param name="name">Имя поля, которое необходимо вернуть.</param>
        /// <returns>Поле, соответствующее указанному имени.</returns>
        public virtual JSObject InitField(string name)
        {
            if (name == "this")
                return GetField(name);
            JSObject res = null;
            if (fields == null)
                (fields = new Dictionary<string, JSObject>())[name] = res = new JSObject();
            else if (!fields.TryGetValue(name, out res))
            {
                fields[name] = res = new JSObject();
                res.attributes |= JSObjectAttributes.DontDelete;
            }
            res.lastRequestedName = name;
            return res;
        }

        /// <summary>
        /// Получает поле, определённое в этом или одном из родительских контекстов. 
        /// Если переменная не существовала, вернётся поле, после присваивания значения которому,
        /// будет создано соответствующее поле в базовом контексте выполнения (не в GlobalContext).
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
            }
            res.lastRequestedName = name;
            return res;
        }

        /// <summary>
        /// Добавляет в указанный контекст объект, представляющий переданный тип.
        /// Имя созданного объекта будет совпадать с именем переданного типа. 
        /// Статические члены типа будут доступны как поля созданного объекта. 
        /// Если тип не являлся статическим, то созданный объект будет функцией (с поздним связыванием), представляющей конструкторы указанного типа.
        /// </summary>
        /// <param name="moduleType">Тип, для которого будет создан внутренний объект.</param>
        public void AttachModule(Type moduleType)
        {
            if (fields == null)
                fields = new Dictionary<string, JSObject>();
            fields.Add(moduleType.Name, TypeProxy.GetConstructor(moduleType));
            fields[moduleType.Name].attributes |= JSObjectAttributes.DontDelete;
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="args">Код скрипта на языке JavaScript</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSObject Eval(string code)
        {
            var debugging = this.debugging;
            this.debugging = false;
            try
            {
                int i = 0;
                string c = Tools.RemoveComments(code);
                var cb = CodeBlock.Parse(new ParsingState(c, code), ref i).Statement;
                if (i < c.Length)
                    throw new System.ArgumentException("Invalid char");
                var vars = new Dictionary<string, VaribleDescriptor>();
                Parser.Optimize(ref cb, -1, vars);
                foreach (var v in vars)
                {
                    if (v.Value.Defined)
                    {
                        var f = this.InitField(v.Key);
                        if (v.Value.Inititalizator != null)
                            f.Assign(v.Value.Inititalizator.Invoke(this));
                    }
                }
                var res = cb.Invoke(this);
                return res;
            }
            finally
            {
                this.debugging = debugging;
            }
        }

        private JSObject eval(JSObject args)
        {
            return Eval(args.GetField("0", true, false).ToString());
        }

        internal void ValidateThreadID()
        {
            var cntxt = this;
            while (cntxt.prototype != null && cntxt.prototype != globalContext) cntxt = cntxt.prototype;
            WeakReference wr = null;
            if (_executedContexts.TryGetValue(System.Threading.Thread.CurrentThread.ManagedThreadId, out wr))
            {
                var t = wr.Target;
                if (t == cntxt)
                    return;
                GC.SuppressFinalize(t);
            }
            _executedContexts[cntxt.threadid = System.Threading.Thread.CurrentThread.ManagedThreadId] = new WeakReference(cntxt);
            GC.ReRegisterForFinalize(cntxt);
        }

        public virtual IEnumerator<string> GetEnumerator()
        {
            return fields.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        internal Context(Context prototype, Statement owner)
            : this(prototype, owner, true)
        {
        }

        internal Context(Context prototype, Statement owner, bool createFields)
        {
            this.owner = owner;
            if (createFields)
                this.fields = new Dictionary<string, JSObject>();
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.undefined;
            this.debugging = prototype.debugging;
            GC.SuppressFinalize(this);
        }

        ~Context()
        {
            _executedContexts.Remove(threadid);
        }
    }
}