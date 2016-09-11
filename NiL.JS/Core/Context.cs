using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum AbortReason
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
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Context : IEnumerable<string>
    {
#if (PORTABLE || NETCORE)
        [ThreadStatic]
        internal static List<Context> currentContextStack;

        internal static List<Context> GetCurrectContextStack()
        {
            return currentContextStack;
        }
#else
        internal const int MaxConcurentContexts = 65535;
        internal static readonly List<Context>[] RunningContexts = new List<Context>[MaxConcurentContexts];
        internal static readonly int[] ThreadIds = new int[MaxConcurentContexts];

        internal static List<Context> GetCurrectContextStack()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            for (var i = 0; i < MaxConcurentContexts; i++)
            {
                if (ThreadIds[i] == 0)
                    break;

                if (ThreadIds[i] == threadId)
                    return RunningContexts[i];
            }

            return null;
        }
#endif
        public static Context CurrentContext
        {
            get
            {
#if (PORTABLE || NETCORE)
                return currentContextStack.Count > 0 ? currentContextStack[currentContextStack.Count - 1] : null;
#else
                var stack = GetCurrectContextStack();
                if (stack == null || stack.Count == 0)
                    return null;
                return stack[stack.Count - 1];
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
                    globalContext.fields = new StringMap<JSValue>();
                JSObject.GlobalPrototype = null;
                TypeProxy.Clear();
                globalContext.fields.Add("Object", TypeProxy.GetConstructor(typeof(JSObject)).CloneImpl(false));
                globalContext.fields["Object"]._attributes = JSValueAttributesInternal.DoNotDelete;
                JSObject.GlobalPrototype = TypeProxy.GetPrototype(typeof(JSObject));
                Core.GlobalObject.refreshGlobalObjectProto();
                globalContext.DefineConstructor(typeof(BaseLibrary.Math));
                globalContext.DefineConstructor(typeof(BaseLibrary.Array));
                globalContext.DefineConstructor(typeof(JSON));
                globalContext.DefineConstructor(typeof(BaseLibrary.String));
                globalContext.DefineConstructor(typeof(Function));
                globalContext.DefineConstructor(typeof(Date));
                globalContext.DefineConstructor(typeof(Number));
                globalContext.DefineConstructor(typeof(Symbol));
                globalContext.DefineConstructor(typeof(BaseLibrary.Boolean));
                globalContext.DefineConstructor(typeof(Error));
                globalContext.DefineConstructor(typeof(TypeError));
                globalContext.DefineConstructor(typeof(ReferenceError));
                globalContext.DefineConstructor(typeof(EvalError));
                globalContext.DefineConstructor(typeof(RangeError));
                globalContext.DefineConstructor(typeof(URIError));
                globalContext.DefineConstructor(typeof(SyntaxError));
                globalContext.DefineConstructor(typeof(RegExp));
#if !(PORTABLE || NETCORE)
                globalContext.DefineConstructor(typeof(console));
#endif
                globalContext.DefineConstructor(typeof(ArrayBuffer));
                globalContext.DefineConstructor(typeof(Int8Array));
                globalContext.DefineConstructor(typeof(Uint8Array));
                globalContext.DefineConstructor(typeof(Uint8ClampedArray));
                globalContext.DefineConstructor(typeof(Int16Array));
                globalContext.DefineConstructor(typeof(Uint16Array));
                globalContext.DefineConstructor(typeof(Int32Array));
                globalContext.DefineConstructor(typeof(Uint32Array));
                globalContext.DefineConstructor(typeof(Float32Array));
                globalContext.DefineConstructor(typeof(Float64Array));
                GlobalContext.DefineConstructor(typeof(Promise));

                globalContext.DefineConstructor(typeof(Debug));

                #region Base Function
                globalContext.DefineVariable("eval").Assign(new EvalFunction());
                globalContext.fields["eval"]._attributes |= JSValueAttributesInternal.Eval;
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
#if !(PORTABLE || NETCORE)
                globalContext.DefineVariable("__pinvoke").Assign(new ExternalFunction(GlobalFunctions.__pinvoke));
#endif
                #endregion
                #region Consts
                globalContext.fields["undefined"] = JSValue.undefined;
                globalContext.fields["Infinity"] = Number.POSITIVE_INFINITY;
                globalContext.fields["NaN"] = Number.NaN;
                globalContext.fields["null"] = JSValue.@null;
                #endregion

                foreach (var v in globalContext.fields.Values)
                    v._attributes |= JSValueAttributesInternal.DoNotEnumerate;
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
        
        internal AbortReason executionMode;
        internal JSValue objectSource;
        internal JSValue executionInfo;
        internal JSValue lastResult;
        internal JSValue arguments;
        internal JSValue thisBind;
        internal Function owner;
        internal Context parent;
        internal IDictionary<string, JSValue> fields;
        internal bool strict;
        internal Dictionary<CodeNode, object> suspendData;
        internal VariableDescriptor[] variables;
        internal Module _module;

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
                    for (; c.thisBind == null;)
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

        internal bool debugging;
        public bool Debugging { get { return debugging; } set { debugging = value; } }
        public event DebuggerCallback DebuggerCallback;

        public bool Running
        {
            get
            {
                return GetCurrectContextStack().Contains(this);
            }
        }

        public AbortReason AbortReason
        {
            get
            {
                return executionMode;
            }
        }

        public JSValue AbortInfo
        {
            get
            {
                return executionInfo;
            }
        }

        public Dictionary<CodeNode, object> SuspendData { get { return suspendData ?? (suspendData = new Dictionary<CodeNode, object>()); } }

        static Context()
        {
            RefreshGlobalContext();
        }

        public Context()
            : this(globalContext, true, Function.Empty)
        {
        }

        public Context(Context prototype)
            : this(prototype, true, Function.Empty)
        {
        }

        internal Context(Context prototype, bool createFields, Function owner)
        {
            this.owner = owner;
            if (prototype != null)
            {
                if (owner == prototype.owner)
                    arguments = prototype.arguments;
                this.parent = prototype;
                this.thisBind = prototype.thisBind;
                this.debugging = prototype.debugging;
            }

            if (createFields)
                this.fields = JSObject.getFieldsContainer();
            this.executionInfo = JSValue.notExists;
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
#if (PORTABLE || NETCORE)
            if (currentContextStack == null)
                currentContextStack = new List<Context>();

            if (currentContextStack.Count > 0 && currentContextStack[currentContextStack.Count - 1] == this)
                return false;

            currentContextStack.Add(this);
            return true;
#else
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var i = 0;
            bool entered = false;
            do
            {
                if (ThreadIds[i] == threadId)
                {
                    if (RunningContexts[i].Count > 0 && RunningContexts[i][RunningContexts[i].Count - 1] == this)
                    {
                        if (entered)
                            Monitor.Exit(RunningContexts);
                        return false;
                    }

                    // Бьёт по производительности
                    //if (RunnedContexts[i].Contains(this))
                    //    ExceptionsHelper.Throw(new ApplicationException("Try to reactivate context"));

                    RunningContexts[i].Add(this);

                    if (entered)
                        Monitor.Exit(RunningContexts);
                    return true;
                }

                if (!entered)
                {
                    Monitor.Enter(RunningContexts);
                    entered = true;
                }

                if (ThreadIds[i] <= 0)
                {
                    if (RunningContexts[i] == null)
                        RunningContexts[i] = new List<Context>();

                    ThreadIds[i] = threadId;
                    RunningContexts[i].Add(this);

                    Monitor.Exit(RunningContexts);
                    return true;
                }

                i++;
            }
            while (i < MaxConcurentContexts);

            Monitor.Exit(RunningContexts);

            ExceptionsHelper.Throw(new InvalidOperationException("Too many concurrent contexts."));

            return false;
#endif
        }

        /// <summary>
        /// Деактивирует контекст и активирует предыдущий активный контекст.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Возникает, если текущий контекст не активен.</exception>
        /// <returns>Текущий активный контекст</returns>
        internal Context Deactivate()
        {
#if (PORTABLE || NETCORE)
            if (currentContextStack[currentContextStack.Count - 1] != this)
                throw new InvalidOperationException("Context is not running");

            currentContextStack.RemoveAt(currentContextStack.Count - 1);
            return CurrentContext;
#else
            int threadId = Thread.CurrentThread.ManagedThreadId;

            var i = 0;
            for (; i < MaxConcurentContexts; i++)
            {
                if (ThreadIds[i] == 0)
                    throw new InvalidOperationException("Context is not running");

                if (ThreadIds[i] == threadId)
                {
                    if (RunningContexts[i][RunningContexts[i].Count - 1] != this)
                        throw new InvalidOperationException("Context is not running");

                    _module = null;
                    RunningContexts[i].RemoveAt(RunningContexts[i].Count - 1);
                    if (RunningContexts[i].Count == 0)
                        ThreadIds[i] = -1;

                    break;
                }
            }
            
            return RunningContexts[i].Count > 0 ? RunningContexts[i][RunningContexts[i].Count - 1] : null;
#endif
        }

        internal Context GetRunningContextFor(Function function)
        {
            Context context = null;
            return GetRunningContextFor(function, out context);
        }

        internal Context GetRunningContextFor(Function function, out Context prevContext)
        {
            prevContext = null;

            if (function == null)
                return null;

            var stack = GetCurrectContextStack();

            for (var i = stack.Count; i-- > 0;)
            {
                if (stack[i].owner == function)
                {
                    if (i > 0)
                        prevContext = stack[i - 1];
                    return stack[i];
                }
            }

            return null;
        }

        internal virtual void ReplaceVariableInstance(string name, JSValue instance)
        {
            if (fields != null && fields.ContainsKey(name))
                fields[name] = instance;
            else
                parent?.ReplaceVariableInstance(name, instance);
        }

        public virtual JSValue DefineVariable(string name, bool deletable = false)
        {
            JSValue res = null;
            if (fields == null || !fields.TryGetValue(name, out res))
            {
                if (fields == null)
                    fields = JSObject.getFieldsContainer();

                fields[name] = res = new JSValue();
                if (!deletable)
                    res._attributes = JSValueAttributesInternal.DoNotDelete;
            }
            else if ((res._attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
                fields[name] = res = res.CloneImpl(false);
            res._valueType |= JSValueType.Undefined;
            return res;
        }

        /// <summary>
        /// Creates new property with Getter and Setter in the object
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="getter">Function called when there is an attempt to get a value. Can be null</param>
        /// <param name="setter">Function called when there is an attempt to set a value. Can be null</param>
        /// <exception cref="System.ArgumentException">if property already exists</exception>
        /// <exception cref="System.InvalidOperationException">if unable to create property</exception>
        public void DefineGetSetVariable(string name, Func<object> getter, Action<object> setter)
        {
            var property = GetVariable(name);
            if (property.ValueType >= JSValueType.Undefined)
                throw new ArgumentException();

            property = DefineVariable(name);
            if (property.ValueType < JSValueType.Undefined)
                throw new InvalidOperationException();

            property._valueType = JSValueType.Property;

            Function jsGetter = null;
            if (getter != null)
            {
#if NET40
                jsGetter = new MethodProxy(getter.Method, getter.Target);
#else
                jsGetter = new MethodProxy(getter.GetMethodInfo(), getter.Target);
#endif
            }

            Function jsSetter = null;
            if (setter != null)
            {
#if NET40
                jsSetter = new MethodProxy(setter.Method, setter.Target);
#else
                jsSetter = new MethodProxy(setter.GetMethodInfo(), setter.Target);
#endif
            }

            property._oValue = new GsPropertyPair(jsGetter, jsSetter);
        }

        public JSValue GetVariable(string name)
        {
            return GetVariable(name, false);
        }

        internal protected virtual JSValue GetVariable(string name, bool create)
        {
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
                        res = new JSValue() { _valueType = JSValueType.NotExists };
                        fields[name] = res;
                    }
                    else
                    {
                        res = JSObject.GlobalPrototype.GetProperty(name, false, PropertyScope.Сommon);
                        if (res._valueType == JSValueType.NotExistsInObject)
                            res._valueType = JSValueType.NotExists;
                    }
                }
            }
            else if (fromProto)
                objectSource = parent.objectSource;
            else
            {
                if (create && (res._attributes & (JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly)) == JSValueAttributesInternal.SystemObject)
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

        public void DefineConstructor(Type moduleType)
        {
            if (fields == null)
                fields = new StringMap<JSValue>();
            string name;
#if (PORTABLE || NETCORE)
            if (System.Reflection.IntrospectionExtensions.GetTypeInfo(moduleType).IsGenericType)
#else
            if (moduleType.IsGenericType)
#endif
                name = moduleType.Name.Substring(0, moduleType.Name.LastIndexOf('`'));
            else
                name = moduleType.Name;
            DefineConstructor(moduleType, name);
        }

        public void DefineConstructor(Type type, string name)
        {
            fields.Add(name, TypeProxy.GetConstructor(type).CloneImpl(false));
            fields[name]._attributes = JSValueAttributesInternal.DoNotEnumerate;
        }

        public void SetAbortState(AbortReason abortReason, JSValue abortInfo)
        {
            this.executionMode = abortReason;
            this.executionInfo = abortInfo;
        }

        /// <summary>
        /// Evaluate script
        /// </summary>
        /// <param name="code">Code in JavaScript</param>
        /// <returns>Result of last evaluated operation</returns>
        public JSValue Eval(string code)
        {
            return Eval(code, false);
        }

        /// <summary>
        /// Evaluate script
        /// </summary>
        /// <param name="code">Code in JavaScript</param>
        /// <param name="suppressScopeCreation">If true, scope will not be created. All variables, which will be defined via let, const or class will not be destructed after evalution</param>
        /// <returns>Result of last evaluated operation</returns>
        public JSValue Eval(string code, bool suppressScopeCreation)
        {
            if (parent == null)
                throw new InvalidOperationException("Cannot execute script in global context");
            if (string.IsNullOrEmpty(code))
                return JSValue.undefined;

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

            var mainFunctionContext = this;
            var stack = GetCurrectContextStack();
            while (stack != null
                && stack.Count > 1
                && stack[stack.Count - 2] == mainFunctionContext.parent
                && stack[stack.Count - 2].owner == mainFunctionContext.owner)
            {
                mainFunctionContext = mainFunctionContext.parent;
            }

            int index = 0;
            string c = Tools.RemoveComments(code, 0);
            var ps = new ParseInfo(c, code, null)
            {
                strict = strict,
                AllowDirectives = true,
                CodeContext = CodeContext.InEval
            };

            var body = CodeBlock.Parse(ps, ref index) as CodeBlock;
            if (index < c.Length)
                throw new ArgumentException("Invalid char");
            var variables = new Dictionary<string, VariableDescriptor>();
            var stats = new FunctionInfo();

            CodeNode cb = body;
            Parser.Build(ref cb, 0, variables, (strict ? CodeContext.Strict : CodeContext.None) | CodeContext.InEval, null, stats, Options.None);

            var tv = stats.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stats, tv, body._variables.Length == 0 || !stats.WithLexicalEnvironment ? 1 : 0);
            if (tv != null)
            {
                var newVarDescs = new VariableDescriptor[tv.Values.Count];
                tv.Values.CopyTo(newVarDescs, 0);
                body._variables = newVarDescs;
            }

            body.Optimize(ref cb, null, null, Options.SuppressUselessExpressionsElimination | Options.SuppressConstantPropogation, null);
            body = cb as CodeBlock ?? body;

            if (stats.ContainsYield)
                body.Decompose(ref cb);

            body.suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;

            var debugging = this.debugging;
            this.debugging = false;
            var runned = this.Activate();

            try
            {
                var context = (suppressScopeCreation || !stats.WithLexicalEnvironment) && !body._strict && !strict ? this : new Context(this, false, owner)
                {
                    strict = strict || body._strict
                };

                if (!strict && !body._strict)
                {
                    for (var i = 0; i < body._variables.Length; i++)
                    {
                        if (!body._variables[i].lexicalScope)
                        {
                            JSValue variable;
                            var cc = mainFunctionContext;
                            while (cc.parent != globalContext
                               && (cc.fields == null || !cc.fields.TryGetValue(body._variables[i].name, out variable)))
                            {
                                cc = cc.parent;
                            }

                            if (cc.variables != null)
                            {
                                for (var j = 0; j < cc.variables.Length; j++)
                                {
                                    if (cc.variables[j].name == body._variables[i].name)
                                    {
                                        cc.variables[j].definitionScopeLevel = -1;
                                    }
                                }
                            }

                            variable = mainFunctionContext.DefineVariable(body._variables[i].name, !suppressScopeCreation);

                            if (body._variables[i].initializer != null)
                            {
                                variable.Assign(body._variables[i].initializer.Evaluate(context));
                            }

                            // блокирует создание переменной в конктексте eval
                            body._variables[i].lexicalScope = true;

                            // блокирует кеширование
                            body._variables[i].definitionScopeLevel = -1;
                        }
                    }
                }

                if (body._lines.Length == 0)
                    return JSValue.undefined;

                var runContextOfEval = context.Activate();
                try
                {
                    return body.Evaluate(context) ?? context.lastResult ?? JSValue.notExists;
                }
                finally
                {
                    if (runContextOfEval)
                        context.Deactivate();
                }
            }
            finally
            {
                if (runned)
                    this.Deactivate();
                this.debugging = debugging;
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
    }
}
