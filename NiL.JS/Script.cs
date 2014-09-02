using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Core.BaseTypes;
using System;
using System.Threading;
using System.Linq.Expressions;
using NiL.JS.Core.JIT;

namespace NiL.JS
{
    /// <summary>
    /// Управляет выполнением скрипта на языке JavaScript.
    /// </summary>
    [Serializable]
    public sealed class Script
    {
        private static readonly Function pseudoCaller = new Function(Context.globalContext, FunctionStatement.Parse("function superCaller(){ }"));

#if !NET35

        private Func<Context, JSObject> compiledScript;
        public bool IsCompiled { get { return compiledScript != null; } }

#endif

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
        /// Инициализирует объект типа Script и преобрзует код скрипта во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript</param>
        public Script(string code)
        {
            if (code == null)
                throw new ArgumentNullException();
            Code = code;
            int i = 0;
            root = CodeBlock.Parse(new ParsingState(Tools.RemoveComments(code, 0), Code), ref i).Statement;
            if (i < code.Length)
                throw new System.ArgumentException("Invalid char");
            Parser.Optimize(ref root, 0, new System.Collections.Generic.Dictionary<string, VariableDescriptor>(), false);
            var body = root as CodeBlock;
            Context = new Context(NiL.JS.Core.Context.globalContext, pseudoCaller);
            Context.thisBind = new ThisBind(Context);
            Context.variables = (root as CodeBlock).variables;
            Context.strict = (root as CodeBlock).strict;
            for (i = body.variables.Length; i-- > 0; )
            {
                if (body.variables[i].Owner == body)
                {
                    var f = Context.DefineVariable(body.variables[i].name);
                    if (body.variables[i].Inititalizator != null)
                        f.Assign(body.variables[i].Inititalizator.Evaluate(Context));
                }
            }
#if !NET35
            if (Context.UseJit)
                compiledScript = JITHelpers.compile(body, false);
#endif
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

#if !NET35
                if (IsCompiled)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Log(0, "NiL.JS JIT", "Run compiled script");
#endif
                    compiledScript(Context);
                }
                else
#endif
                {
#if DEBUG && !NET35
                    System.Diagnostics.Debugger.Log(0, "NiL.JS JIT", "Run non compiled script");
#endif
                    root.Evaluate(Context);
                }
            }
            finally
            {
                System.Runtime.GCSettings.LatencyMode = lm;
                Context.Deactivate();
            }
        }
    }
}