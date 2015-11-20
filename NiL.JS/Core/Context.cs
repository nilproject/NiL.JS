using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    internal enum AbortType
    {
        None = 0,
        Continue,
        Break,
        Return,
        TailRecursion,
        Exception,
        Suspend,
        Resume,
        ResumeThrow
    }

    /// <summary>
    /// Контекст выполнения скрипта. Хранит состояние выполнения сценария.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class Context : IEnumerable<string>
    {
#if PORTABLE
        [ThreadStatic]
        internal static Context currentContext;
#else
        internal const int MaxConcurentContexts = 65535;
        internal static readonly Context[] runnedContexts = new Context[MaxConcurentContexts];
#endif
        public static Context CurrentContext
        {
            get
            {
#if PORTABLE
                return currentContext;
#else
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
#endif
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
                    globalContext.fields = new StringMap2<JSValue>();
                JSObject.GlobalPrototype = null;
                TypeProxy.Clear();
                globalContext.fields.Add("Object", TypeProxy.GetConstructor(typeof(JSObject)).CloneImpl(false));
                globalContext.fields["Object"].attributes = JSValueAttributesInternal.DoNotDelete;
                JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
                Core.GlobalObject.refreshGlobalObjectProto();
                globalContext.AttachModule(typeof(BaseLibrary.Math));
                globalContext.AttachModule(typeof(BaseLibrary.Array));
                globalContext.AttachModule(typeof(JSON));
                globalContext.AttachModule(typeof(BaseLibrary.String));
                globalContext.AttachModule(typeof(Function));
                globalContext.AttachModule(typeof(Date));
                globalContext.AttachModule(typeof(Number));
                globalContext.AttachModule(typeof(Symbol));
                globalContext.AttachModule(typeof(BaseLibrary.Boolean));
                globalContext.AttachModule(typeof(Error));
                globalContext.AttachModule(typeof(TypeError));
                globalContext.AttachModule(typeof(ReferenceError));
                globalContext.AttachModule(typeof(EvalError));
                globalContext.AttachModule(typeof(RangeError));
                globalContext.AttachModule(typeof(URIError));
                globalContext.AttachModule(typeof(SyntaxError));
                globalContext.AttachModule(typeof(RegExp));
#if !PORTABLE
                globalContext.AttachModule(typeof(console));
#endif
                globalContext.AttachModule(typeof(ArrayBuffer));
                globalContext.AttachModule(typeof(Int8Array));
                globalContext.AttachModule(typeof(Uint8Array));
                globalContext.AttachModule(typeof(Uint8ClampedArray));
                globalContext.AttachModule(typeof(Int16Array));
                globalContext.AttachModule(typeof(Uint16Array));
                globalContext.AttachModule(typeof(Int32Array));
                globalContext.AttachModule(typeof(Uint32Array));
                globalContext.AttachModule(typeof(Float32Array));
                globalContext.AttachModule(typeof(Float64Array));

                globalContext.AttachModule(typeof(Debug));

                #region Base Function
                globalContext.DefineVariable("eval").Assign(new EvalFunction());
                globalContext.fields["eval"].attributes |= JSValueAttributesInternal.Eval;
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
#if !PORTABLE
                globalContext.DefineVariable("__pinvoke").Assign(new ExternalFunction(GlobalFunctions.__pinvoke));
#endif
                #endregion
                #region Consts
                globalContext.fields["undefined"] = JSValue.undefined;
                globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
                globalContext.fields["NaN"] = Number.NaN;
                globalContext.fields["null"] = JSValue.Null;
                #endregion

                foreach (var v in globalContext.fields.Values)
                    v.attributes |= JSValueAttributesInternal.DoNotEnumerate;
            }
            catch
            {
                throw;
            }
            finally
            {
                globalContext.Deactivate();
            }
        }

#if !PORTABLE
        internal int threadId;
#endif
        internal Context oldContext;
        /// <summary>
        /// Временное хранилище для передачи значений.
        /// <remarks>
        /// Поскольку в каждом потоке может быть только один головной контекст, 
        /// и наоборот каждый контекст может быть запущен только в одном потоке,
        /// отсутствует вероятность конфликта при использовании данного поля.
        /// </remarks>
        /// </summary>
        internal AbortType abortType;
        internal JSValue objectSource;
        internal JSValue tempContainer;
        internal JSValue abortInfo;
        internal JSValue lastResult;
        internal JSValue arguments;
        internal JSValue thisBind;
        internal Function owner;
        internal Context parent;
        internal IDictionary<string, JSValue> fields;
        internal bool strict;
        internal VariableDescriptor[] variables;
        private Dictionary<CodeNode, object> suspendData;

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
        public JSValue ThisBind
        {
            get
            {
                var c = this;
                if (thisBind == null)
                {
                    if (strict)
                        return JSValue.undefined;
                    for (; c.thisBind == null; )
                    {
                        if (c.parent == globalContext)
                        {
                            thisBind = new GlobalObject(c);
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

        public Dictionary<CodeNode, object> SuspendData { get { return suspendData ?? (suspendData = new Dictionary<CodeNode, object>()); } }

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

        internal Context(Context prototype, bool createFields, Function owner)
        {
            this.owner = owner;
            if (prototype != null)
            {
                if (owner == prototype.owner)
                    arguments = prototype.arguments;
                tempContainer = prototype.tempContainer;
                this.parent = prototype;
                this.thisBind = prototype.thisBind;
#if DEV
                this.debugging = prototype.debugging;
#endif
            }
            else
            {
                tempContainer = new JSValue() { attributes = JSValueAttributesInternal.Temporary };
            }
            if (createFields)
                this.fields = JSObject.getFieldsContainer();
            this.abortInfo = JSValue.notExists;
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
#if PORTABLE
            if (currentContext == this)
                return false;
            if (oldContext != null)
                throw new InvalidOperationException("Try to reactivate context");
            if (currentContext != null) // что-то выполняется
            {
                if (!Monitor.IsEntered(currentContext)) // не в этом потоке?
                    throw new InvalidOperationException("Too many concurrent contexts.");
            }
            oldContext = currentContext;
            currentContext = this;
            Monitor.Enter(this);
            return true;
#else
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var i = 0;
            do
            {
                var c = runnedContexts[i];
                if (c == null || c.threadId == threadId)
                {
                    if (c == this)
                        return false;
                    if (oldContext != null)
                        throw new ApplicationException("Try to reactivate context");
                    this.oldContext = c;
                    runnedContexts[i] = this;
                    this.threadId = threadId;
                    return true;
                }
                i++;
            }
            while (i < MaxConcurentContexts);
            throw new InvalidOperationException("Too many concurrent contexts.");
#endif
        }

        /// <summary>
        /// Деактивирует контекст и активирует предыдущий активный контекст.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Возникает, если текущий контекст не активен.</exception>
        /// <returns>Текущий активный контекст</returns>
        internal Context Deactivate()
        {
#if PORTABLE
            if (currentContext != this)
                throw new InvalidOperationException("Context not runned");
            currentContext = oldContext;
            var res = oldContext;
            oldContext = null;
            Monitor.Exit(this);
            return res;
#else
            Context c = null;
            var i = 0;
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
            if (i == -1)
                throw new InvalidOperationException("Context not runned");
            oldContext = null;
            return c;
#endif
        }

        internal Context GetRunnedContextFor(Function function)
        {
            var context = CurrentContext;
            while (context != null && context.owner != function)
            {
                context = context.oldContext;
            }
            return context;
        }

        /// <summary>
        /// Действие аналогично функции GeField с тем отличием, что возвращённое поле всегда определено в указанном контектсе.
        /// </summary>
        /// <param name="name">Имя поля, которое необходимо вернуть.</param>
        /// <returns>Поле, соответствующее указанному имени.</returns>
        public virtual JSValue DefineVariable(string name)
        {
            if (name == "this")
                return thisBind;
            JSValue res = null;
            if (!fields.TryGetValue(name, out res))
            {
                fields[name] = res = new JSValue()
                {
                    attributes = JSValueAttributesInternal.DoNotDelete
                };
            }
            else if ((res.attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                fields[name] = res = res.CloneImpl(false);
            res.valueType |= JSValueType.Undefined;
            return res;
        }

        /// <summary>
        /// Получает переменную, определённую в этом или одном из родительских контекстов. 
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns></returns>
        public JSValue GetVariable(string name)
        {
            return GetVariable(name, false);
        }

        internal protected virtual JSValue GetVariable(string name, bool create)
        {
            if (name.Length == 4 // такое странное решение показало лучшую скорость
                && name[0] == 't'
                && name[1] == 'h'
                && name[2] == 'i'
                && name[3] == 's')
                return ThisBind;

            JSValue res = null;
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
                    {
                        res = new JSValue() { valueType = JSValueType.NotExists };
                        fields[name] = res;
                    }
                    else
                    {
                        res = JSObject.GlobalPrototype.GetMember(wrap(name), false, PropertyScope.Сommon);
                        if (res.valueType == JSValueType.NotExistsInObject)
                            res.valueType = JSValueType.NotExists;
                    }
                }
            }
            else if (fromProto)
                objectSource = parent.objectSource;
            else
            {
                if (create && (res.attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                    fields[name] = res = res.CloneImpl(false);
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
            if (System.Diagnostics.Debugger.IsAttached)
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
                fields = new StringMap2<JSValue>();
            string name;
#if PORTABLE
            if (System.Reflection.IntrospectionExtensions.GetTypeInfo(moduleType).IsGenericType)
#else
            if (moduleType.IsGenericType)
#endif
                name = moduleType.Name.Substring(0, moduleType.Name.LastIndexOf('`'));
            else
                name = moduleType.Name;
            AttachModule(moduleType, name);
        }

        /// <summary>
        /// Добавляет в указанный контекст объект, представляющий переданный тип.
        /// Имя созданного объекта будет совпадать с именем переданного типа. 
        /// Статические члены типа будут доступны как поля созданного объекта. 
        /// Если тип не являлся статическим, то созданный объект будет функцией (с поздним связыванием), представляющей конструкторы указанного типа.
        /// </summary>
        /// <param name="moduleType">Тип, для которого будет создан внутренний объект.</param>
        public void AttachModule(Type moduleType, string name)
        {
            fields.Add(name, TypeProxy.GetConstructor(moduleType).CloneImpl(false));
            fields[name].attributes = JSValueAttributesInternal.DoNotEnumerate;
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSValue Eval(string code)
        {
            return Eval(code, false);
        }

        /// <summary>
        /// Ожидается один аргумент.
        /// Выполняет переданный код скрипта в указанном контексте.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript</param>
        /// <param name="inplace">Если установлен, переменные объявленные в ходе выполнения, не будут доступны для удаления</param>
        /// <returns>Результат выполнения кода (аргумент оператора "return" либо результат выполнения последней выполненной строки кода).</returns>
        public JSValue Eval(string code, bool inplace)
        {
            if (parent == null)
                throw new InvalidOperationException("Cannot execute script in global context");
            if (string.IsNullOrEmpty(code))
                return JSValue.undefined;
#if DEV
            var debugging = this.debugging;
            this.debugging = false;
#endif
            try
            {
                int i = 0;
                string c = Tools.RemoveComments(code, 0);
                var ps = new ParsingState(c, code, null);
                ps.strict = strict;
                var body = CodeBlock.Parse(ps, ref i) as CodeBlock;
                bool leak = !(strict || body.strict);
                if (i < c.Length)
                    throw new System.ArgumentException("Invalid char");
                var vars = new Dictionary<string, VariableDescriptor>();
                CodeNode cb = body;
                Parser.Build(ref cb, 0, vars, (strict ? CodeContext.Strict : CodeContext.None) | CodeContext.InEval, null, null, Options.Default);
                body = cb as CodeBlock;
                Context context = null;
                if (leak)
                    context = this;
                else
                    context = new Context(this, true, this.owner) { strict = true, variables = body.variables };
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
                                    context.variables[i].captured = true;
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
                if (body.localVariables != null)
                {
                    for (i = body.localVariables.Length; i-- > 0; )
                    {
                        var f = context.DefineVariable(body.localVariables[i].name);
                        if (!inplace)
                            f.attributes = JSValueAttributesInternal.None;
                        if (body.localVariables[i].initializer != null)
                            f.Assign(body.localVariables[i].initializer.Evaluate(context));
                        if (body.localVariables[i].isReadOnly)
                            f.attributes |= JSValueAttributesInternal.ReadOnly;
                        body.localVariables[i].captured = true;
                    }
                }

                cb = body;
                body.Optimize(ref cb, null, null, Options.SuppressUselessExpressionsElimination | Options.SuppressConstantPropogation, null);
                body = cb as CodeBlock;

                var run = context.Activate();
                try
                {
                    return body.Evaluate(context) ?? context.lastResult ?? JSValue.notExists;
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
        internal JSValue wrap(long value)
        {
            if (value <= int.MaxValue)
            {
                tempContainer.valueType = JSValueType.Int;
                tempContainer.iValue = (int)value;
                return tempContainer;
            }
            else
            {
                tempContainer.valueType = JSValueType.Double;
                tempContainer.dValue = value;
                return tempContainer;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal JSValue wrap(string value)
        {
            tempContainer.valueType = JSValueType.String;
            tempContainer.oValue = value;
            return tempContainer;
        }

        #endregion
    }
}