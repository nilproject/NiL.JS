using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.Core.BaseTypes;
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
        Exception,
    }

    /// <summary>
    /// Контекст выполнения скрипта. Хранит состояние выполнения сценария.
    /// </summary>
    [Serializable]
    public class Context : IEnumerable<string>
    {
        private static readonly Context[] runnedContexts = new Context[65535];

        public static Context CurrentContext
        {
            get
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
#if DEBUG
                for (var i = 0; i < runnedContexts.Length; i++)
#else
                for (var i = runnedContexts.Length; i-- > 0; )
#endif
                    if (runnedContexts[i] != null && runnedContexts[i].threadId == threadId)
                        return runnedContexts[i];
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
                globalContext.DefineVariable("eval").Assign(new EvalFunction());
                globalContext.fields["eval"].attributes |= JSObjectAttributesInternal.Eval;
                globalContext.DefineVariable("isNaN").Assign(new ExternalFunction(GlobalFunctions.isNaN));
                globalContext.DefineVariable("unescape").Assign(new ExternalFunction(GlobalFunctions.unescape));
                globalContext.DefineVariable("escape").Assign(new ExternalFunction(GlobalFunctions.escape));
                globalContext.DefineVariable("encodeURI").Assign(new ExternalFunction((thisBind, x) =>
                {
                    return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.UTF8Encoding.Default.GetBytes(x[0].ToString()));
                }));
                globalContext.DefineVariable("encodeURIComponent").Assign(globalContext.GetVariable("encodeURI"));
                globalContext.DefineVariable("decodeURI").Assign(new ExternalFunction(GlobalFunctions.decodeURI));
                globalContext.DefineVariable("decodeURIComponent").Assign(globalContext.GetVariable("decodeURI"));
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

        private int threadId;
        private Context oldContext;
        /// <summary>
        /// Временное хранилище для передачи значений.
        /// <remarks>
        /// Поскольку в каждом потоке может быть только один головной контекст, 
        /// а каждый контекст может быть запущен только в одном потоке,
        /// отсутствует вероятность конфликта при использовании данного поля.
        /// </remarks>
        /// </summary>
        internal readonly JSObject tempContainer;
        internal readonly Context prototype;
        internal IDictionary<string, JSObject> fields;
        internal AbortType abort;
        internal JSObject objectSource;
        internal JSObject abortInfo;
        internal JSObject thisBind;
        internal Function caller;
        internal bool strict;
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
                        if (c.prototype == globalContext)
                        {
                            thisBind = new ThisBind(c);
                            c.thisBind = thisBind;
                            break;
                        }
                        else
                            c = c.prototype;
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

        static Context()
        {
            RefreshGlobalContext();
        }

        private Context() { tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary }; }

        public Context(Context prototype)
            : this(prototype, true, new Function())
        {
        }

        internal Context(Context prototype, Function caller)
            : this(prototype, true, caller)
        {
        }

        protected Context(Context prototype, bool createFields, Function caller)
        {
            tempContainer = prototype.tempContainer;
            this.caller = caller;
            if (createFields)
                this.fields = new Dictionary<string, JSObject>();
            this.prototype = prototype;
            this.thisBind = prototype.thisBind;
            this.abortInfo = JSObject.notExists;
#if DEV
            this.debugging = prototype.debugging;
#endif
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
            lock (runnedContexts)
            {
#if DEBUG
                for (var i = 0; i < runnedContexts.Length; i++)
#else
                for (var i = runnedContexts.Length; i-- > 0; )
#endif
                {
                    if (runnedContexts[i] == null || runnedContexts[i].threadId == threadId)
                    {
                        if (runnedContexts[i] == this)
                            return false;
                        else if (oldContext != null)
                        {
#if DEBUG
                            System.Diagnostics.Debugger.Break();
#endif
                            throw new ApplicationException("Try to reactivate context");
                        }
                        this.oldContext = runnedContexts[i];
                        runnedContexts[i] = this;
                        this.threadId = threadId;
                        return true;
                    }
                }
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
            if (this != CurrentContext)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Context not runned");
            }
#if DEBUG
            var i = 0;
            lock (runnedContexts)
            {
                for (; i < runnedContexts.Length; i++)
                {
#else
            var i = runnedContexts.Length;
            lock (runnedContexts)
            {
                for (; i-- > 0; )
                {
#endif
                    if (runnedContexts[i] != null && runnedContexts[i].threadId == threadId)
                    {
                        runnedContexts[i] = oldContext;
                        break;
                    }
                }
            }
            oldContext = null;
            return runnedContexts[i];
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
                res.attributes = JSObjectAttributesInternal.DoNotDelete;
            }
            else
            {
                if (res.valueType < JSObjectType.Undefined)
                    res.valueType = JSObjectType.Undefined;
            }
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
                return ThisBind;
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res)) && (prototype != null);
            if (fromProto)
                res = prototype.GetVariable(name, create);
            if (res == null) // значит вышли из глобального контекста
            {
                if (this == globalContext)
                    return null;
                else
                {
                    if (create)
                        fields[name] = res = new JSObject() { valueType = JSObjectType.NotExists };
                    else
                        res = JSObject.GlobalPrototype.GetMember(name);
                }
            }
            return res;
        }

        internal void raiseDebugger(CodeNode nextStatement)
        {
            var p = this;
            while (p != null)
            {
#if DEV
                if (p.debugging && p.DebuggerCallback != null)
#else
                if (p.DebuggerCallback != null)
#endif
                {
                    p.DebuggerCallback(this, new DebuggerCallbackEventArgs() { Statement = nextStatement });
                    break;
                }
                p = p.prototype;
            }
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
                Parser.Optimize(ref cb, leak ? -1 : -2, 0, vars, strict);
                Context context = null;
                var body = cb as CodeBlock;
                if (leak)
                    context = this;
                else
                    context = new Context(this, this.caller) { strict = true, variables = body.variables };
                if (leak)
                {
                    if (context.variables != null)
                        for (i = context.variables.Length; i-- > 0; )
                        {
                            VariableDescriptor desc = null;
                            if (vars.TryGetValue(context.variables[i].name, out desc) && desc.Defined)
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
                                 *      // то в a её значение будет, как бы, 2, но на самом деле 1.
                                 *  }
                                 * }
                                 */
                            }
                        }
                }
                for (i = body.variables.Length; i-- > 0; )
                {
                    if (body.variables[i].Defined)
                    {
                        var f = context.DefineVariable(body.variables[i].name);
                        if (body.variables[i].Inititalizator != null)
                            f.Assign(body.variables[i].Inititalizator.Invoke(context));
                    }
                }

                var run = context.Activate();
                try
                {
                    return cb.Invoke(context);
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
        public JSObject wrap(bool value)
        {
            tempContainer.valueType = JSObjectType.Bool;
            tempContainer.iValue = value ? 1 : 0;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public JSObject wrap(sbyte value)
        {
            tempContainer.valueType = JSObjectType.Int;
            tempContainer.iValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public JSObject wrap(byte value)
        {
            tempContainer.valueType = JSObjectType.Int;
            tempContainer.iValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public JSObject wrap(short value)
        {
            tempContainer.valueType = JSObjectType.Int;
            tempContainer.iValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public JSObject wrap(ushort value)
        {
            tempContainer.valueType = JSObjectType.Int;
            tempContainer.iValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public JSObject wrap(int value)
        {
            tempContainer.valueType = JSObjectType.Int;
            tempContainer.iValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public JSObject wrap(uint value)
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
        public JSObject wrap(long value)
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
        [CLSCompliant(false)]
        public JSObject wrap(ulong value)
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
        public JSObject wrap(float value)
        {
            tempContainer.valueType = JSObjectType.Double;
            tempContainer.dValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public JSObject wrap(double value)
        {
            tempContainer.valueType = JSObjectType.Double;
            tempContainer.dValue = value;
            return tempContainer;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public JSObject wrap(string value)
        {
            tempContainer.valueType = JSObjectType.String;
            tempContainer.oValue = value;
            return tempContainer;
        }

        #endregion
    }
}