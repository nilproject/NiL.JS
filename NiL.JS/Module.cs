using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Core.Interop;

namespace NiL.JS
{
    [Flags]
    public enum Options
    {
        None = 0,
        SuppressUselessExpressionsElimination = 1,
        SuppressUselessStatementsElimination = 2,
        SuppressConstantPropogation = 4,
        DoNotOptimize = 8,
        ForceStrict = 16,
    }

    public enum ModuleEvaluationState
    {
        Default = 0,
        Evaluating,
        Evaluated,
        Fail,
    }

    /// <summary>
    /// Represents and manages JavaScript module
    /// </summary>
    public class Module
    {
        private static readonly char[] _pathSplitChars = new[] { '\\', '/' };

        public ExportTable Exports { get; } = new ExportTable();

        public List<IModuleResolver> ModuleResolversChain { get; } = new List<IModuleResolver>();

        public ModuleEvaluationState EvaluationState { get; private set; }

        /// <summary>
        /// Root node of AST
        /// </summary>
        [Obsolete]
        public CodeBlock Root => Script.Root;

        /// <summary>
        /// JavaScript code, used for initialization
        /// </summary>
        [Obsolete]
        public string Code => Script.Code;

        /// <summary>
        /// The script of the module
        /// </summary>
        public Script Script { get; private set; }

        /// <summary>
        /// Root context of module
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Path to file with script
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        public Module(string code)
            : this(code, null, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="path">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="globalContext">Global context</param>
        public Module(string path, string code, GlobalContext globalContext)
            : this(path, code, null, Options.None, globalContext)
        { }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="path">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        public Module(string path, string code)
            : this(path, code, null, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code and callback for output compiler messages.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages</param>
        public Module(string code, CompilerMessageCallback messageCallback)
            : this(code, messageCallback, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code and callback for output compiler messages.
        /// </summary>
        /// <param name="virtualPath">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages</param>
        public Module(string virtualPath, string code, CompilerMessageCallback messageCallback)
            : this(virtualPath, code, messageCallback, Options.None)
        { }

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
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        /// <param name="globalContext">Global context</param>
        public Module(string code, CompilerMessageCallback messageCallback, Options options, GlobalContext globalContext)
            : this(null, code, messageCallback, options, globalContext)
        { }

        /// <summary>
        /// Initializes a new Module with specified code, callback for output compiler messages and compiler options.
        /// </summary>
        /// <param name="virtualPath">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        public Module(string virtualPath, string code, CompilerMessageCallback messageCallback = null, Options options = Options.None, GlobalContext globalContext = null)
            : this(virtualPath, Script.Parse(code, messageCallback, options), globalContext)
        { }

        /// <summary>
        /// Initializes a new Module with a script.
        /// </summary>
        /// <param name="virtualPath">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="script">The module script</param>
        /// <param name="globalContext">Global context</param>
        public Module(string virtualPath, Script script, GlobalContext globalContext = null)
        {
            if (script == null)
                throw new ArgumentNullException();

            FilePath = virtualPath;

            Context = new Context(globalContext ?? Context.CurrentGlobalContext, true, null);
            Context._module = this;
            Context._thisBind = new GlobalObject(Context);

            Script = script;

            Context._strict = Script.Root._strict;
        }

        public Module()
            : this("")
        {
        }

        /// <summary>
        /// Run the script
        /// </summary>
        public void Run()
        {
            EvaluationState = ModuleEvaluationState.Evaluating;
            Script.Evaluate(Context);
            EvaluationState = ModuleEvaluationState.Evaluated;
        }

        /// <summary>
        /// Run the script with time limit
        /// </summary>
        /// <param name="timeLimitInMilliseconds">Time limit</param>
        public void Run(int timeLimitInMilliseconds)
        {
            var start = Environment.TickCount;
            var oldDebugValue = Context.Debugging;
            Context.Debugging = true;
            DebuggerCallback callback = (context, e) =>
            {
                if (Environment.TickCount - start >= timeLimitInMilliseconds)
                    throw new TimeoutException();
            };
            Context.DebuggerCallback += callback;

            try
            {
                Run();
            }
            catch
            {
                EvaluationState = ModuleEvaluationState.Fail;
                throw;
            }
            finally
            {
                Context.Debugging = oldDebugValue;
                Context.DebuggerCallback -= callback;
            }
        }

        internal Module Import(string importArg)
        {
            var request = new ModuleRequest(this, importArg, makeAbsolutePath(this, importArg));
            Module module = null;
            for (var i = 0; i < ModuleResolversChain.Count; i++)
            {
                if (ModuleResolversChain[i].TryGetModule(request, out module))
                    break;

                module = null;
            }

            if (module == null)
                throw new InvalidOperationException("Unable to load module \"" + request.CmdArgument + "\"");

            if (module.FilePath == null)
                module.FilePath = request.AbsolutePath;

            if (module.EvaluationState == ModuleEvaluationState.Default)
            {
                module.ModuleResolversChain.AddRange(ModuleResolversChain);
                module.Run();
            }

            return module;
        }

        private static string makeAbsolutePath(Module initiator, string path)
        {
            var thisName = initiator.FilePath.Split(_pathSplitChars);
            var requestedName = path.Split(_pathSplitChars);
            var pathTokens = new LinkedList<string>(thisName);

            if (requestedName.Length > 0 && requestedName[0] == "" || requestedName[0].EndsWith(":"))
                pathTokens.Clear();
            else
                pathTokens.RemoveLast();

            for (var i = 0; i < requestedName.Length; i++)
                pathTokens.AddLast(requestedName[i]);

            for (var node = pathTokens.First; node != null;)
            {
                if (node.Value == "." || (node.Value == "" && node.Previous != pathTokens.First))
                {
                    node = node.Next;
                    pathTokens.Remove(node.Previous);
                }
                else if (node.Value == ".." && node.Previous != null)
                {
                    node = node.Next;
                    pathTokens.Remove(node.Previous);
                    pathTokens.Remove(node.Previous);
                }
                else
                    node = node.Next;
            }

            if (pathTokens.Last.Value.IndexOf('.') == -1)
                pathTokens.Last.Value = pathTokens.Last.Value + ".js";

            if (pathTokens.Count == 0 || !pathTokens.First.Value.EndsWith(":"))
                pathTokens.AddFirst("");

            return string.Join("/", pathTokens);
        }

#if !NETCORE
        /// <summary>
        /// Returns module, which provides access to clr-namespace
        /// </summary>
        /// <param name="namespace">Namespace</param>
        /// <returns></returns>
        public static Module ClrNamespace(string @namespace)
        {
            var result = new Module();

            foreach (var type in NamespaceProvider.GetTypesByPrefix(@namespace))
            {
                try
                {
                    if (type.Namespace == @namespace)
                    {
                        result.Exports[type.Name] = Context.CurrentGlobalContext.GetConstructor(type);
                    }
                    else if (type.Namespace.StartsWith(@namespace) && type.Namespace[@namespace.Length] == '.')
                    {
                        var nextSegment = type.Namespace.Substring(@namespace.Length).Split('.')[1];
                        result.Exports[nextSegment] = new NamespaceProvider($"{@namespace}.{nextSegment}");
                    }
                }
                catch
                { }
            }

            return result;
        }
#endif
    }
}
