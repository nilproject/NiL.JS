using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    internal enum AbortType
    {
        None = 0,
        Continue,
        Break,
        Return,
        TailRecursion,
        Exception,
        Yield
    }

    /// <summary>
    /// Контекст выполнения скрипта. Хранит состояние выполнения сценария.
    /// </summary>
    [Serializable]
    public class Context : IEnumerable<string>
    {
        internal const int MaxConcurentContexts = 65535;
        internal static readonly Context[] runnedContexts = new Context[MaxConcurentContexts];

        public static Context CurrentContext
        {
            get
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                for (var i = 0; i < MaxConcurentContexts; i++)
                {
                    var c = runnedContexts[i];
                    if (c == null)
                        break;
                    if (c.threadId == threadId)
                        return c;
                }
                return null;
            }
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
            globalContext.Activate();
            try
            {
                if (globalContext.fields != null)
                    globalContext.fields.Clear();
                else
                    globalContext.fields = new Dictionary<string, JSObject>();
                JSObject.GlobalPrototype = null;
                TypeProxy.Clear();
                globalContext.fields.Add("Object", TypeProxy.GetConstructor(typeof(JSObject)).CloneImpl());
                globalContext.fields["Object"].attributes = JSObjectAttributesInternal.DoNotDelete;
                JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
                Core.ThisBind.refreshThisBindProto();
                globalContext.AttachModule(typeof(Modules.Math));
                globalContext.AttachModule(typeof(BaseTypes.Array));
                globalContext.AttachModule(typeof(Modules.JSON));
                globalContext.AttachModule(typeof(BaseTypes.String));
                globalContext.AttachModule(typeof(BaseTypes.Function));
                globalContext.AttachModule(typeof(BaseTypes.Date));
                globalContext.AttachModule(typeof(BaseTypes.Number));
                globalContext.AttachModule(typeof(BaseTypes.Boolean));
                globalContext.AttachModule(typeof(BaseTypes.Error));
                globalContext.AttachModule(typeof(BaseTypes.TypeError));
                globalContext.AttachModule(typeof(BaseTypes.ReferenceError));
                globalContext.AttachModule(typeof(BaseTypes.EvalError));
                globalContext.AttachModule(typeof(BaseTypes.RangeError));
                globalContext.AttachModule(typeof(BaseTypes.URIError));
                globalContext.AttachModule(typeof(BaseTypes.SyntaxError));
                globalContext.AttachModule(typeof(BaseTypes.RegExp));
                globalContext.AttachModule(typeof(Modules.console));

                globalContext.AttachModule(typeof(BaseTypes.ArrayBuffer));
                globalContext.AttachModule(typeof(BaseTypes.Int8Array));
                globalContext.AttachModule(typeof(BaseTypes.Uint8Array));
                globalContext.AttachModule(typeof(BaseTypes.Uint8ClampedArray));
                globalContext.AttachModule(typeof(BaseTypes.Int16Array));
                globalContext.AttachModule(typeof(BaseTypes.Uint16Array));
                globalContext.AttachModule(typeof(BaseTypes.Int32Array));
                globalContext.AttachModule(typeof(BaseTypes.Uint32Array));
                globalContext.AttachModule(typeof(BaseTypes.Float32Array));
                globalContext.AttachModule(typeof(BaseTypes.Float64Array));

                globalContext.AttachModule(typeof(Modules.Debug));

                #region Base Function
                globalContext.DefineVariable("eval").Assign(new EvalFunction());
                globalContext.fields["eval"].attributes |= JSObjectAttributesInternal.Eval;
                globalContext.DefineVariable("isNaN").Assign(new ExternalFunction(GlobalFunctions.isNaN));
                globalContext.DefineVariable("unescape").Assign(new ExternalFunction(GlobalFunctions.unescape));
                globalContext.DefineVariable("escape").Assign(new ExternalFunction(GlobalFunctions.escape));
                globalContext.DefineVariable("encodeURI").Assign(new ExternalFunction(GlobalFunctions.encodeURI));
                globalContext.DefineVariable("encodeURIComponent").Assign(new ExternalFunction(GlobalFunctions.encodeURIComponent));
                globalContext.DefineVariable("decodeURI").Assign(new ExternalFunction(GlobalFunctions.decodeURI));
                globalContext.DefineVariable("decodeURIComponent").Assign(new ExternalFunction(GlobalFunctions.decodeURIComponent));
                globalContext.DefineVariable("isFinite").Assign(new ExternalFunction(GlobalFunctions.isFinite));
                globalContext.DefineVariable("parseFloat").Assign(new ExternalFunction(GlobalFunctions.parseFloat));
                globalContext.DefineVariable("parseInt").Assign(new ExternalFunction(GlobalFunctions.parseInt));
                globalContext.DefineVariable("__pinvoke").Assign(new ExternalFunction(GlobalFunctions.__pinvoke));
                #endregion
                #region Consts
                globalContext.fields["undefined"] = JSObject.undefined;
                globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
                globalContext.fields["NaN"] = Number.NaN;
                globalContext.fields["null"] = JSObject.Null;
                #endregion

                foreach (var v in globalContext.fields.Values)
                    v.attributes |= JSObjectAttributesInternal.DoNotEnum;
            }
            finally
            {
                globalContext.Deactivate();
            }
        }

        internal int threadId;
        internal Context oldContext;
        /// <summary>
        /// Временное хранилище для передачи значений.
        /// <remarks>
        /// Поскольку в каждом потоке может быть только один головной контекст, 
        /// а каждый контекст может быть запущен только в одном потоке,
        /// отсутствует вероятность конфликта при использовании данного поля.
        /// </remarks>
        /// </summary>
        internal JSObject tempContainer;
        internal readonly Context parent;
        internal Dictionary<string, JSObject> fields;
        internal AbortType abort;
        internal JSObject objectSource;
        internal JSObject abortInfo;
        internal JSObject lastResult;
        internal JSObject thisBind;
        internal Function caller;
        internal bool strict;
        internal VariableDescriptor[] variables;
        public Context Root
        {
            get
            {
                var res = this;
                if (res.parent != null && res.parent != globalContext)
                    do
                        res = res.parent;
                    while (res.parent != null && res.parent != globalContext);
                return res;
            }
        }
        public JSObject ThisBind
        {
            get
            {
                var c = this;
                if (thisBind == null)
                {
                    if (strict)
                        return JSObject.undefined;
                    for (; c.thisBind == null; )
                    {
                        if (c.parent == globalContext)
                        {
                            thisBind = new ThisBind(c);
                            c.thisBind = thisBind;
                            break;
                        }
                        else
                            c = c.parent;
                    }
                    thisBind = c.thisBind;
                }
                return thisBind;
            }
        }

#if DEV
        internal bool debugging;
        public bool Debugging { get { return debugging; } set { debugging = value; } }
#endif
        /// <summary>
        /// Событие, возникающее при попытке выполнения оператора "debugger".
        /// </summary>
        public event DebuggerCallback DebuggerCallback;

        /// <summary>
        /// Указывает, присутствует ли контекст в каскаде выполняющихся контекстов непосредственно
        /// или в качестве одного из прототипов
        /// </summary>
        public bool IsExcecuting
        {
            get
            {
                var ccontext = CurrentContext;
                if (ccontext == null)
                    return false;
                do
                {
                    if (ccontext == this)
                        return true;
                    var pc = ccontext.parent;
                    while (pc != null)
                    {
                        if (pc == this)
                            return true;
                        pc = pc.parent;
                    }
                    ccontext = ccontext.oldContext;
                } while (ccontext != null);
                return false;
            }
        }

        static Context()
        {
            RefreshGlobalContext();
        }

        public Context()
            : this(globalContext, true, Function.emptyFunction)
        {
        }

        public Context(Context prototype)
            : this(prototype, true, Function.emptyFunction)
        {
        }

        internal Context(Context prototype, bool createFields, Function caller)
        {
            if (prototype != null)
            {
                tempContainer = prototype.tempContainer;
                this.parent = prototype;
                this.thisBind = prototype.thisBind;
#if DEV
                this.debugging = prototype.debugging;
#endif
            }
            else
            {
                tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
            }
            this.caller = caller;
            if (createFields)
                this.fields = new Dictionary<string, JSObject>();
            this.abortInfo = JSObject.notExists;
        }

        /// <summary>
        /// Делает контекст активным в текущем потоке выполнения.
        /// </summary>
        /// <exception cref="System.ApplicationException">Возникает, если контекст не является активным, 
        /// но хранит ссылку на предыдущий активный контекст. Обычно это возникает в том случае, 
        /// когда указанный контекст находится в цепочке вложенности активированных контекстов.</exception>
        /// <returns>Истина если текущий контекст был активирован данным вызовом. Ложь если контекст уже активен.</returns>
        internal bool Activate()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            //lock (runnedContexts)
            {
                var i = 0;
                do
                {
                    var c = runnedContexts[i];
                    if (c == null || c.threadId == threadId)
                    {
                        if (c == this)
                            return false;
                        if (oldContext != null)
                        {
                            throw new ApplicationException("Try to reactivate context");
                        }
                        this.oldContext = c;
                        runnedContexts[i] = this;
                        this.threadId = threadId;
                        return true;
                    }
                    i++;
                }
                while (i < MaxConcurentContexts);
            }
            throw new InvalidOperationException("Too many concurrent contexts.");
        }

        /// <summary>
        /// Деактивирует контекст и активирует предыдущий активный контекст.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Возникает, если текущий контекст не активен.</exception>
        /// <returns>Текущий активный контекст</returns>
        internal Context Deactivate()
        {
            Context c = null;
            var i = 0;
            //lock (runnedContexts)
            {
                for (; i < runnedContexts.Length; i++)
                {
                    c = runnedContexts[i];
                    if (c != null && c.threadId == threadId)
                    {
                        if (c != this)
                            throw new InvalidOperationException("Context not runned");
                        runnedContexts[i] = c = oldContext;
                        break;
                    }
                }
            }
            if (i == -1)
                throw new InvalidOperationException("Context not runned");
            oldContext = null;
            return c;
        }

        /// <summary>
        /// Действие аналогично функции GeField с тем отличием, что возвращённое поле всегда определено в указанном контектсе.
        /// </summary>
        /// <param name="name">Имя поля, которое необходимо вернуть.</param>
        /// <returns>Поле, соответствующее указанному имени.</returns>
        public virtual JSObject DefineVariable(string name)
        {
            if (name == "this")
                return thisBind;
            JSObject res = null;
            if (!fields.TryGetValue(name, out res))
            {
                fields[name] = res = new JSObject()
                {
                    attributes = JSObjectAttributesInternal.DoNotDelete
                };
            }
            res.valueType |= JSObjectType.Undefined;
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
            //if (!IsExcecuting)
            //    System.Diagnostics.Debug.Fail("Try to get varible from stoped context.");
#endif
            if (name.Length == 4
                && name[0] == 't'
                && name[1] == 'h'
                && name[2] == 'i'
                && name[3] == 's')
                return ThisBind;
            JSObject res = null;
            bool fromProto = fields == null || (!fields.TryGetValue(name, out res) && (parent != null));
            if (fromProto)
                res = parent.GetVariable(name, create);
            if (res == null) // значит вышли из глобального контекста
            {
                if (this == globalContext)
                    return null;
                else
                {
                    if (create)
                        fields[name] = res = new JSObject() { valueType = JSObjectType.NotExists };
                    else
                    {
                        res = JSObject.GlobalPrototype.GetMember(wrap(name), create, false);
                        if (res.valueType == JSObjectType.NotExistsInObject)
                            res.valueType = JSObjectType.NotExists;
                    }
                }
            }
            else if (fromProto)
                objectSource = parent.objectSource;
            else
            {
                if (create && (res.attributes & (JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly)) == JSObjectAttributesInternal.SystemObject)
                    fields[name] = res = res.CloneImpl();
            }
            return res;
        }

        internal void raiseDebugger(CodeNode nextStatement)
        {
            var p = this;
            while (p != null)
            {
                if (p.DebuggerCallback != null)
                {
                    p.DebuggerCallback(this, new DebuggerCallbackEventArgs() { Statement = nextStatement });
                    return;
                }
                p = p.parent;
            }
            System.Diagnostics.Debugger.Break();
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
            fields.Add(moduleType.Name, TypeProxy.GetConstructor(moduleType).CloneImpl());
            fields[moduleType.Name].attributes = JSObjectAttributesInternal.DoNotEnum;
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSObject Eval(string code)
        {
            return Eval(code, false);
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript</param>
        /// <param name="inplace">Если истина, переменные объявленные в ходе выполнения, не будут доступны для удаления</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSObject Eval(string code, bool inplace)
        {
#if DEV
            var debugging = this.debugging;
            this.debugging = false;
#endif
            try
            {
                int i = 0;
                string c = Tools.RemoveComments(code, 0);
                var ps = new ParsingState(c, code);
                ps.strict.Clear();
                ps.strict.Push(strict);
                var cb = CodeBlock.Parse(ps, ref i).Statement;
                bool leak = !(strict || (cb as CodeBlock).strict);
                if (i < c.Length)
                    throw new System.ArgumentException("Invalid char");
                var vars = new Dictionary<string, VariableDescriptor>();
                Parser.Build(ref cb, leak ? -1 : -2, vars, strict);
                Context context = null;
                var body = cb as CodeBlock;
                if (leak)
                    context = this;
                else
                    context = new Context(this, true, this.caller) { strict = true, variables = body.variables };
                if (leak)
                {
                    if (context.variables != null)
                        for (i = context.variables.Length; i-- > 0; )
                        {
                            VariableDescriptor desc = null;
                            if (vars.TryGetValue(context.variables[i].name, out desc))
                            {
                                if (desc.IsDefined)
                                {
                                    context.variables[i].defineDepth = -1; // Кеш будет игнорироваться.
                                    // чистить кэш тут не достаточно. 
                                    // Мы не знаем, где объявлена одноимённая переменная 
                                    // и в тех случаях, когда она пришла из функции выше
                                    // или даже глобального контекста, её кэш может быть 
                                    // не сброшен вовремя и значение будет браться из контекста
                                    // eval'а, а не того контекста, в котором её позовут.
                                    /*
                                     * function a(){
                                     *  var c = 1;
                                     *  function b(){
                                     *      eval("var c = 2");
                                     *      // переменная объявлена в контексте b, значит и значение должно быть из
                                     *      // контекста b, но если по выходу из b кэш этой переменной сброшен не будет, 
                                     *      // то в a её значение будет 2
                                     *  }
                                     * }
                                     */
                                }
                                else
                                {
                                    for (var r = 0; r < desc.references.Count; r++)
                                        desc.references[r].descriptor = context.variables[i];
                                }
                            }
                        }
                }
                for (i = body.localVariables.Length; i-- > 0; )
                {
                    var f = context.DefineVariable(body.localVariables[i].name);
                    if (!inplace)
                        f.attributes = JSObjectAttributesInternal.None;
                    if (body.localVariables[i].Inititalizator != null)
                        f.Assign(body.localVariables[i].Inititalizator.Evaluate(context));
                    if (body.localVariables[i].readOnly)
                        f.attributes |= JSObjectAttributesInternal.ReadOnly;
                }

                var run = context.Activate();
                try
                {
                    return cb.Evaluate(context) ?? context.lastResult ?? JSObject.notExists;
                }
                finally
                {
                    if (run)
                        context.Deactivate();
                }
            }
            finally
            {
#if DEV
                this.debugging = debugging;
#endif
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return fields.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        public override string ToString()
        {
            return this == globalContext ? "Global context" : "Context";
        }

        #region Temporal Wrapping

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal JSObject wrap(long value)
        {
            if (value <= int.MaxValue)
            {
                tempContainer.valueType = JSObjectType.Int;
                tempContainer.iValue = (int)value;
                return tempContainer;
            }
            else
            {
                tempContainer.valueType = JSObjectType.Double;
                tempContainer.dValue = value;
                return tempContainer;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal JSObject wrap(string value)
        {
            tempContainer.valueType = JSObjectType.String;
            tempContainer.oValue = value;
            return tempContainer;
        }

        #endregion
    }
}