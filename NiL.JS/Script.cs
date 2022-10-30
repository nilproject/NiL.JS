using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS
{
    public sealed class Script
    {
        private static readonly object _scriptStackLock = new();

        [ThreadStatic]
        private static Stack<Script> _scriptsStack;

        internal static Script CurrentScript => _scriptsStack.Count > 0 ? _scriptsStack.Peek() : null;

        public string Code { get; private set; }
        public CodeBlock Root { get; private set; }

        private Script()
        {

        }

        public static Script Parse(string code, CompilerMessageCallback messageCallback = null, Options options = Options.None)
        {
            if (code == null)
                throw new ArgumentNullException();

            if (code == "")
            {
                return new Script
                {
                    Root = new CodeBlock(new CodeNode[0]),
                    Code = ""
                };
            }

            var internalCallback = messageCallback != null ?
                (level, position, length, message) => messageCallback(level, CodeCoordinates.FromTextPosition(code, position, length), message)
                : null as InternalCompilerMessageCallback;

            var codeContext = CodeContext.None;
            if ((options & Options.ForceStrict) != 0)
                codeContext |= CodeContext.Strict;

            int i = 0;
            var root = (CodeBlock)CodeBlock.Parse(new ParseInfo(code, internalCallback) { CodeContext = codeContext | CodeContext.AllowDirectives }, ref i);

            var stat = new FunctionInfo();
            Parser.Build(ref root, 0, new Dictionary<string, VariableDescriptor>(), codeContext, internalCallback, stat, options);

            var body = root as CodeBlock;
            body._suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;

            for (var vi = 0; vi < body._variables.Length; vi++)
                body._variables[vi].captured = true;

            var tv = stat.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stat, tv, body._variables.Length == 0 || !stat.WithLexicalEnvironment ? 1 : 0);

            if (tv != null)
                body._variables = tv.Values.ToArray();

            var bd = body as CodeNode;

            if ((options & Options.DoNotOptimize) == 0)
                body.Optimize(ref bd, null, internalCallback, options, stat);

            if (stat.NeedDecompose)
                body.Decompose(ref bd);

            return new Script() { Code = code, Root = root };
        }

        [ExceptionHelper.StackFrameOverride]
        public JSValue Evaluate(Context context)
        {
            if (Code == "")
                return JSValue.Undefined;

            lock (_scriptStackLock)
            {
                if (_scriptsStack == null)
                    _scriptsStack = new();

                _scriptsStack.Push(this);
            }

            try
            {
                context.Activate();
                return Root.Evaluate(context) ?? context._lastResult ?? JSValue.notExists;
            }
            finally
            {
                for (var i = 0; i < Root._variables.Length; i++)
                    Root._variables[i].cacheContext = null;
                context.Deactivate();

                lock (_scriptStackLock)
                    _scriptsStack.Pop();
            }
        }
    }
}
