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
    /// Represents and manage JavaScript module
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Module
    {
        private static readonly StringMap<Module> _modulesCache = new StringMap<Module>();
        private static List<ResolveModuleHandler> _resolveModuleHandlers = new List<ResolveModuleHandler>();

        /// <summary>
        /// Occurs when module not found in modules cache
        /// </summary>
        public static event ResolveModuleHandler ResolveModule
        {
            add
            {
                if (value != null)
                    lock (_resolveModuleHandlers)
                        _resolveModuleHandlers.Add(value);
            }
            remove
            {
                lock (_resolveModuleHandlers)
                    _resolveModuleHandlers.Remove(value);
            }
        }

        public JSObject Exports { get; private set; } = JSObject.CreateObject();

        private CodeNode root;
        /// <summary>
        /// Root node of AST
        /// </summary>
        public CodeBlock Root { get { return root as CodeBlock; } }

        /// <summary>
        /// JavaScript code, used for initialization
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Root context of module
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        public Module(string code)
            : this(code, null, Options.None)
        {

        }

        /// <summary>
        /// Initializes a new Module with specified code and callback for output compiler messages.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages</param>
        public Module(string code, CompilerMessageCallback messageCallback)
            : this(code, messageCallback, Options.None)
        {

        }

        /// <summary>
        /// Initializes a new Module with specified code, callback for output compiler messages and compiler options.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        public Module(string code, CompilerMessageCallback messageCallback, Options options)
            : this(null, code, messageCallback, options)
        { }

        /// <summary>
        /// Initializes a new Module with specified code, callback for output compiler messages and compiler options.
        /// </summary>
        /// <param name="name">Name of module that used for import by ImportStatement</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        public Module(string name, string code, CompilerMessageCallback messageCallback, Options options)
        {
            if (code == null)
                throw new ArgumentNullException();

            Code = code;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!_modulesCache.ContainsKey(name))
                    _modulesCache[name] = this;
            }

            if (code == "")
                return;

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
            body.suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;
            Context = new Context(Context.globalContext, true, null);
            Context.thisBind = new GlobalObject(Context);
            Context.strict = body._strict;

            var tv = stat.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stat, tv, body._variables.Length == 0 || !stat.WithLexicalEnvironment ? 1 : 0);
            if (tv != null)
                body._variables = new List<VariableDescriptor>(tv.Values).ToArray();

            var bd = body as CodeNode;
            body.Optimize(ref bd, null, icallback, options, stat);

            if (stat.ContainsYield)
                body.Decompose(ref bd);
        }

        /// <summary>
        /// Run the script
        /// </summary>
        public void Run()
        {
            if (Code == "")
                return;

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

        internal static Module Resolve(string name)
        {
            var e = new ResolveModuleEventArgs(name);

            for (var i = 0; i < _resolveModuleHandlers.Count && e.Module == null; i++)
                _resolveModuleHandlers[i](e);

            if (e.Module != null && e.AddToCache && !_modulesCache.ContainsKey(name))
                _modulesCache[name] = e.Module;

            return e.Module;
        }

        /// <summary>
        /// Returns module, which provides access to clr-namespace
        /// </summary>
        /// <param name="namespace">Namespace</param>
        /// <returns></returns>
        public static Module ClrNamespace(string @namespace)
        {
            return new Module("") { Exports = new NamespaceProvider(@namespace) };
        }
    }
}