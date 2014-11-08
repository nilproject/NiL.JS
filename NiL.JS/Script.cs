using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS
{
    /// <summary>
    /// Управляет выполнением скрипта на языке JavaScript.
    /// </summary>
    [Serializable]
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
            : this(code, null)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="parentContext">Родительский контекст для контекста выполнения сценария.</param>
        public Script(string code, Context parentContext)
        {
            if (code == null)
                throw new ArgumentNullException();
            Code = code;
            int i = 0;
            root = CodeBlock.Parse(new ParsingState(Tools.RemoveComments(code, 0), Code), ref i).Statement;
            if (i < code.Length)
                throw new System.ArgumentException("Invalid char");
            Parser.Build(ref root, 0, new System.Collections.Generic.Dictionary<string, VariableDescriptor>(), false);
            var body = root as CodeBlock;
            Context = new Context(parentContext ?? NiL.JS.Core.Context.globalContext, true, pseudoCaller);
            Context.thisBind = new ThisBind(Context);
            Context.variables = (root as CodeBlock).variables;
            Context.strict = (root as CodeBlock).strict;
            for (i = body.localVariables.Length; i-- > 0; )
            {
                var f = Context.DefineVariable(body.localVariables[i].name);
                body.localVariables[i].cacheRes = f;
                body.localVariables[i].cacheContext = Context;
                if (body.localVariables[i].Inititalizator != null)
                    f.Assign(body.localVariables[i].Inititalizator.Evaluate(Context));
                if (body.localVariables[i].readOnly)
                    body.localVariables[i].cacheRes.attributes |= JSObjectAttributesInternal.ReadOnly;
            }
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