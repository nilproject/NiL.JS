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
        private static IDictionary<int, Context> _executedContexts = new BinaryTree<int, Context>();
#else
        private static IDictionary<int, Context> _executedContexts = new System.Collections.Concurrent.ConcurrentDictionary<int, Context>();
#endif

        public static Context CurrentContext
        {
            get
            {
                Context res = null;
                _executedContexts.TryGetValue(Thread.CurrentThread.ManagedThreadId, out res);
                return res;
            }
        }

        /// <summary>
        /// Указывает, присутствует ли контекст в каскаде выполняющихся контекстов непосредственно
        /// или в качестве одного из прототипов
        /// </summary>
        public bool IsExcecuting
        {
            get
            {
                var ccontext = CurrentContext;
                do
                {
                    if (ccontext == this)
                        return true;
                    var pc = ccontext.prototype;
                    while (pc != null)
                    {
                        if (pc == this)
                            return true;
                        pc = pc.prototype;
                    }
                    ccontext = ccontext.oldContext;
                } while (ccontext != null);
                return false;
            }
        }

        private Context oldContext;
        internal bool Run()
        {
            Context ccntxt = null;
            _executedContexts.TryGetValue(Thread.CurrentThread.ManagedThreadId, out ccntxt);
            if (ccntxt != this)
            {
                _executedContexts[Thread.CurrentThread.ManagedThreadId] = this;
                if (oldContext != null)
                    System.Diagnostics.Debug.Fail("Try to rewrite oldContext");
                this.oldContext = ccntxt;
                return true;
            }
            return false;
        }

        internal void Stop()
        {
#if DEBUG
            if (oldContext == globalContext)
                System.Diagnostics.Debug.Print("Return to global context.");
#endif
            if (this != CurrentContext)
                throw new InvalidOperationException("Context not runned");
            _executedContexts[Thread.CurrentThread.ManagedThreadId] = oldContext;
            oldContext = null;
        }

        internal readonly static Context globalContext = new Context() { strict = true };
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
#if DEBUG
            if (CurrentContext != globalContext)
                throw new InvalidOperationException("Try to refresh global context while script executing.");
#endif
            if (globalContext.fields != null)
                globalContext.fields.Clear();
            else
                globalContext.fields = new Dictionary<string, JSObject>();
            ThisObject.thisProto = null;
            JSObject.GlobalPrototype = null;
            TypeProxy.Clear();
            globalContext.fields.Add("Object", TypeProxy.GetConstructor(typeof(JSObject)).Clone() as JSObject);
            globalContext.fields["Object"].attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum;
            JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
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
            globalContext.DefineVariable("eval").Assign(new ExternalFunction((t, x) => { return CurrentContext.Eval(x["0"].ToString()); }));
            globalContext.DefineVariable("isNaN").Assign(new ExternalFunction((t, x) =>
            {
                var r = x.GetMember("0");
                if (r.valueType == JSObjectType.Double)
                    return double.IsNaN(r.dValue);
                if (r.valueType == JSObjectType.Bool || r.valueType == JSObjectType.Int || r.valueType == JSObjectType.Date)
                    return false;
                if (r.valueType == JSObjectType.String)
                {
                    double d = 0;
                    int i = 0;
                    if (Tools.ParseNumber(r.oValue as string, i, out d))
                        return double.IsNaN(d);
                    return true;
                }
                return true;
            }));
            globalContext.DefineVariable("unescape").Assign(new ExternalFunction((t, x) =>
            {
                return System.Web.HttpUtility.HtmlDecode(x.GetMember("0").ToString());
            }));
            globalContext.DefineVariable("escape").Assign(new ExternalFunction(escape));
            globalContext.DefineVariable("encodeURI").Assign(new ExternalFunction((thisBind, x) =>
            {
                return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.UTF8Encoding.Default.GetBytes(x.GetMember("0").ToString()));
            }));
            globalContext.DefineVariable("encodeURIComponent").Assign(globalContext.GetVariable("encodeURI"));
            globalContext.DefineVariable("decodeURI").Assign(new ExternalFunction((thisBind, x) =>
            {
                return System.Text.UTF8Encoding.Default.GetString(System.Web.HttpServerUtility.UrlTokenDecode(x.GetMember("0").ToString()));
            }));
            globalContext.DefineVariable("decodeURIComponent").Assign(globalContext.GetVariable("decodeURI"));
            globalContext.DefineVariable("isFinite").Assign(new ExternalFunction((thisBind, x) =>
            {
                var d = Tools.JSObjectToDouble(x.GetMember("0"));
                return !double.IsNaN(d) && !double.IsInfinity(d);
            }));
            globalContext.DefineVariable("parseFloat").Assign(new ExternalFunction((thisBind, x) =>
            {
                return Tools.JSObjectToDouble(x.GetMember("0"));
            }));
            globalContext.DefineVariable("parseInt").Assign(new ExternalFunction((thisBind, x) =>
            {
                var r = x.GetMember("0");
                for (; ; )
                    switch (r.valueType)
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
                                if (!Tools.ParseNumber(s, ref ix, out dres, Tools.JSObjectToInt(x.GetMember("1")), true))
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
            globalContext.DefineVariable("__pinvoke").Assign(new ExternalFunction(__pinvoke));
            #endregion
            #region Consts
            globalContext.fields["undefined"] = JSObject.undefined;
            globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
            globalContext.fields["NaN"] = Number.NaN;
            globalContext.fields["null"] = JSObject.Null;
            #endregion

            foreach (var v in globalContext.fields.Values)
                v.attributes |= JSObjectAttributes.DoNotEnum;
        }

        private static JSObject escape(JSObject thisBind, JSObject x)
        {
            return System.Web.HttpUtility.HtmlEncode(x.GetMember("0").ToString());
        }

        private static JSObject __pinvoke(JSObject thisBind, JSObject args)
        {
            var argsCount = Tools.JSObjectToInt(args.GetMember("length"));
            var threadsCount = 1;
            if (argsCount == 0)
                return null;
            if (argsCount > 1)
                threadsCount = Tools.JSObjectToInt(args.GetMember("1"));
            var function = args.GetMember("0").oValue as Function;
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
                            valueType = JSObjectType.Object,
                            attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum
                        };
                        targs.fields["length"] = new Number(1) { assignCallback = null, attributes = JSObjectAttributes.DoNotEnum };
                        targs.fields["0"] = new Number((int)o) { assignCallback = null, attributes = JSObjectAttributes.Argument };
                        (targs.fields["callee"] = new JSObject()
                        {
                            valueType = JSObjectType.Function,
                            oValue = function,
                            attributes = JSObjectAttributes.DoNotEnum
                        }).Protect();
                        function.Invoke(null, targs);
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
                    argsCount = Tools.JSObjectToInt(arg.GetMember("length"));
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
                        var threadIndex = Tools.JSObjectToInt(args.GetMember("0"));
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
            globalContext.Run();
            RefreshGlobalContext();
        }

        internal readonly Context prototype;

        private static uint __pinvokeCalled;
        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal JSObject objectSource;
        internal JSObject abortInfo;
        internal JSObject thisBind;
        internal bool strict;
#if DEV
        internal bool debugging;
#endif
        internal VariableDescriptor[] variables;
        public Context Root
        {
            get
            {
                var res = this;
                while (res.prototype != null && res.prototype != globalContext)
                    res = res.prototype;
                return res;
            }
        }
        /// <summary>
        /// Событие, возникающее при попытке выполнения оператора "debugger".
        /// </summary>
        public event DebuggerCallback DebuggerCallback;
#if DEV
        public bool Debugging { get { return debugging; } set { debugging = value; } }
#endif

        internal void raiseDebugger(Statement nextStatement)
        {
            var p = this;
            while (p != null)
            {
#if DEV
                if (p.debugging && p.DebuggerCallback != null)
#else
                if (p.DebuggerCallback != null)
#endif
                    p.DebuggerCallback(this, new DebuggerCallbackEventArgs() { Statement = nextStatement });
                p = p.prototype;
            }
        }

        private Context()
        {
        }

        /// <summary>
        /// Действие аналогично функции GeField с тем отличием, что возвращённое поле всегда определено в указанном контектсе.
        /// </summary>
        /// <param name="name">Имя поля, которое необходимо вернуть.</param>
        /// <returns>Поле, соответствующее указанному имени.</returns>
        public virtual JSObject DefineVariable(string name)
        {
            if (name == "this")
                return GetVariable(name);
            JSObject res = null;
            if (fields == null)
                (fields = new Dictionary<string, JSObject>())[name] = res = new JSObject();
            else if (!fields.TryGetValue(name, out res))
            {
                fields[name] = res = new JSObject();
                res.attributes = JSObjectAttributes.DoNotDelete;
            }
            res.lastRequestedName = name;
#if DEBUG
            res.attributes &= ~JSObjectAttributes.DBGGettedOverGM;
#endif
            return res;
        }

        /// <summary>
        /// Получает переменную, определённую в этом или одном из родительских контекстов. 
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns></returns>
        public JSObject GetVariable(string name)
        {
            return GetVariable(name, false);
        }

        internal protected virtual JSObject GetVariable(string name, bool create)
        {
#if DEBUG
            if (!IsExcecuting)
                System.Diagnostics.Debug.Fail("Try to get varible from stoped context.");
#endif
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
                else if (thisBind.valueType == JSObjectType.NotExist) // было "delete this". Просто вернём к жизни существующий объект
                    thisBind.valueType = JSObjectType.Object;
                return thisBind;
            }
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && (prototype != null);
            if (fromProto)
                res = prototype.GetVariable(name, create);
            if (res == null || res == JSObject.notExist)
            {
                if (this == globalContext)
                    return null;
                if (create)
                    (fields ?? (fields = new Dictionary<string, JSObject>()))[name] = res = new JSObject();
                else
                {
                    JSObject.notExist.valueType = JSObjectType.NotExist;
                    JSObject.notExist.lastRequestedName = name;
                    return JSObject.notExist;
                }
            }
            else
            {
                if (res.valueType == JSObjectType.NotExistInObject)
                    res.valueType = JSObjectType.NotExist;
            }
            res.lastRequestedName = name;
#if DEBUG
            if (create)
                res.attributes &= ~JSObjectAttributes.DBGGettedOverGM;
            else
                res.attributes |= JSObjectAttributes.DBGGettedOverGM;
#endif
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
            fields.Add(moduleType.Name, TypeProxy.GetConstructor(moduleType).Clone() as JSObject);
            fields[moduleType.Name].attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum;
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="args">Код скрипта на языке JavaScript</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSObject Eval(string code)
        {
#if DEV
            var debugging = this.debugging;
            this.debugging = false;
#endif
            try
            {
                int i = 0;
                string c = Tools.RemoveComments(code);
                var ps = new ParsingState(c, code);
                ps.strict.Clear();
                ps.strict.Push(strict);
                var cb = CodeBlock.Parse(ps, ref i).Statement;
                bool leak = !(strict || (cb as CodeBlock).strict);
                if (i < c.Length)
                    throw new System.ArgumentException("Invalid char");
                var vars = new Dictionary<string, VariableDescriptor>();
                Parser.Optimize(ref cb, leak ? -1 : -2, vars);
                var context = leak ? this : new Context(this) { strict = true };
                var cvars = this.variables ??
                    (this.variables = new VariableDescriptor[0]); // если всё правильно, эта ветка не выполнится.
                for (i = cvars.Length; i-- > 0; )
                {
                    VariableDescriptor desc = null;
                    if (vars.TryGetValue(cvars[i].Name, out desc))
                    {
                        foreach (var r in desc.references)
                            r.Descriptor = cvars[i];
                    }
                }
                var run = context.Run();
                try
                {
                    var res = cb.Invoke(context);
                    return res;
                }
                finally
                {
                    if (run)
                        context.Stop();
                }
            }
            finally
            {
#if DEV
                this.debugging = debugging;
#endif
            }
        }

        public virtual IEnumerator<string> GetEnumerator()
        {
            return fields.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        internal Context(Context prototype)
            : this(prototype, true)
        {
        }

        internal Context(Context prototype, bool createFields)
        {
            if (createFields)
                this.fields = new Dictionary<string, JSObject>();
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.undefined;
#if DEV
            this.debugging = prototype.debugging;
#endif
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return this == globalContext ? "Global context" : "Context";
        }
    }
}