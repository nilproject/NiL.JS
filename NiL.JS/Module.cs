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
        None = 0,
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
    public sealed class Module
    {
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
        public Module(string code)
            : this(code, null, Options.None)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Module(string code, CompilerMessageCallback messageCallback)
            : this(code, messageCallback, Options.None)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Module(string code, CompilerMessageCallback messageCallback, Options options)
        {
            if (code == null)
                throw new ArgumentNullException();
            Code = code;
            int i = 0;
            root = CodeBlock.Parse(new ParseInfo(Tools.RemoveComments(code, 0), Code, messageCallback), ref i);

            CompilerMessageCallback icallback = messageCallback != null ? (level, cord, message) =>
                {
                    messageCallback(level, CodeCoordinates.FromTextPosition(code, cord.Column, cord.Length), message);
                }
            : null as CompilerMessageCallback;

            var stat = new FunctionInfo();
            Parser.Build(ref root, 0, new Dictionary<string, VariableDescriptor>(), CodeContext.None, icallback, stat, options);
            var body = root as CodeBlock;
            body.suppressScopeIsolation = true;
            Context = new Context(Context.globalContext, true, null);
            Context.thisBind = new GlobalObject(Context);
            Context.strict = body.strict;

            var bd = body as CodeNode;
            body.Optimize(ref bd, null, icallback, options, stat);

            var tv = stat.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stat, tv, body._variables.Length == 0 || !stat.WithLexicalEnvironment ? 1 : 0);
            if (tv != null)
                body._variables = new List<VariableDescriptor>(tv.Values).ToArray();

            if (stat.ContainsYield)
                body.Decompose(ref bd);
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
    }
}