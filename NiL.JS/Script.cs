using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Statements;

namespace NiL.JS
{
    [Flags]
    public enum Options
    {
        Default = 0,
        SuppressUselessExpressionsElimination = 1,
        SuppressUselessStatementsElimination = 2,
        SuppressConstantPropogation = 4,
    }

    /// <summary>
    /// Управляет выполнением скрипта на языке JavaScript.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Script
    {
        internal static readonly Function pseudoCaller = null;//new Function(Context.globalContext, FunctionStatement.Parse("function superCaller(){ }"));

        private CodeNode root;
        public CodeBlock Root { get { return root as CodeBlock; } }
        /// <summary>
        /// Исходный код скрипта, переданный при создании объекта.
        /// </summary>
        public string Code { get; private set; }
        /// <summary>
        /// Корневой контекст выполнения скрипта.
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        public Script(string code)
            : this(code, null, null, Options.Default)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, CompilerMessageCallback messageCallback)
            : this(code, null, messageCallback, Options.Default)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="parentContext">Родительский контекст для контекста выполнения сценария.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, Context parentContext)
            : this(code, parentContext, null, Options.Default)
        { }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="parentContext">Родительский контекст для контекста выполнения сценария.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, Context parentContext, CompilerMessageCallback messageCallback, Options options)
        {
            if (code == null)
                throw new ArgumentNullException();
            Code = code;
            int i = 0;
            root = CodeBlock.Parse(new ParsingState(Tools.RemoveComments(code, 0), Code, messageCallback), ref i).Statement;
            if (i < code.Length)
                throw new System.ArgumentException("Invalid char");
            CompilerMessageCallback icallback = messageCallback != null ? (level, cord, message) =>
                {
                    messageCallback(level, CodeCoordinates.FromTextPosition(code, cord.Column, cord.Length), message);
                } : null as CompilerMessageCallback;
            var stat = new FunctionStatistics();
            Parser.Build(ref root, 0, new System.Collections.Generic.Dictionary<string, VariableDescriptor>(), _BuildState.None, icallback, stat, options);
            var body = root as CodeBlock;
            Context = new Context(parentContext ?? NiL.JS.Core.Context.globalContext, true, pseudoCaller);
            Context.thisBind = new GlobalObject(Context);
            Context.variables = (root as CodeBlock).variables;
            Context.strict = (root as CodeBlock).strict;
            for (i = body.localVariables.Length; i-- > 0; )
            {
                var f = Context.DefineVariable(body.localVariables[i].name);
                body.localVariables[i].cacheRes = f;
                body.localVariables[i].cacheContext = Context;
                if (body.localVariables[i].Inititalizator != null)
                    f.Assign(body.localVariables[i].Inititalizator.Evaluate(Context));
                if (body.localVariables[i].isReadOnly)
                    body.localVariables[i].cacheRes.attributes |= JSObjectAttributesInternal.ReadOnly;
                body.localVariables[i].captured |= stat.ContainsEval;
            }
            var bd = body as CodeNode;
            body.Optimize(ref bd, null, icallback, options, stat);
        }

        /// <summary>
        /// Запускает выполнение скрипта.
        /// </summary>
        public void Invoke()
        {
            var lm = System.Runtime.GCSettings.LatencyMode;
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
            try
            {
                Context.Activate();
                root.Evaluate(Context);
            }
            finally
            {
                System.Runtime.GCSettings.LatencyMode = lm;
                Context.Deactivate();
            }
        }
#if !PORTABLE
        /// <summary>
        /// Пытается провести частичную или полную компилияцию сценария, начиная с листов синтаксического дерева.
        /// Максимальные по высоте поддеревья, для которых допустима компиляция, будут заменены на листы, 
        /// представляющие скомпилированное выражение.
        /// После этой операции возможно нарушение процесса отладки.
        /// </summary>
        public void TryCompile()
        {
            root.TryCompile(true, false, null, new List<CodeNode>());
        }
#endif
    }
}