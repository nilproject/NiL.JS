using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS
{
    public sealed class Script
    {
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

            int i = 0;
            var root = (CodeBlock)CodeBlock.Parse(new ParseInfo(Parser.RemoveComments(code, 0), code, internalCallback), ref i);

            var stat = new FunctionInfo();
            Parser.Build(ref root, 0, new Dictionary<string, VariableDescriptor>(), CodeContext.None, internalCallback, stat, options);
            var body = root as CodeBlock;
            body._suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;

            var tv = stat.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stat, tv, body._variables.Length == 0 || !stat.WithLexicalEnvironment ? 1 : 0);
            var bd = body as CodeNode;
            body.Optimize(ref bd, null, internalCallback, options, stat);
            if (tv != null)
                body._variables = new List<VariableDescriptor>(tv.Values).ToArray();

            if (stat.NeedDecompose)
                body.Decompose(ref bd);

            return new Script() { Code = code, Root = root };
        }

        public JSValue Evaluate(Context context)
        {
            if (Code == "")
                return JSValue.Undefined;

            try
            {
                context.Activate();
                return Root.Evaluate(context);
            }
            finally
            {
                for (var i = 0; i < Root._variables.Length; i++)
                    Root._variables[i].cacheContext = null;
                context.Deactivate();
            }
        }
    }
}
