using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Core.BaseTypes;
using System;
using System.Threading;

namespace NiL.JS
{
    /// <summary>
    /// Управляет выполнением скрипта на языке JavaScript.
    /// </summary>
    [Serializable]
    public sealed class Script
    {
        private Statement root;
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
            Code = code;
            int i = 0;
            root = CodeBlock.Parse(new ParsingState(Tools.RemoveComments(code), Code), ref i).Statement;
            if (i < code.Length)
                throw new System.ArgumentException("Invalid char");
            Parser.Optimize(ref root, new System.Collections.Generic.Dictionary<string, VariableDescriptor>(), false);
            var body = root as CodeBlock;
            Context = new Context(NiL.JS.Core.Context.globalContext);
            Context.thisBind = new ThisBind(Context);
            Context.variables = (root as CodeBlock).variables;
            Context.strict = (root as CodeBlock).strict;
            for (i = body.variables.Length; i-- > 0; )
            {
                if (body.variables[i].Owner == body)
                {
                    body.variables[i].ClearCache();
                    var f = Context.DefineVariable(body.variables[i].Name);
                    if (body.variables[i].Inititalizator != null)
                        f.Assign(body.variables[i].Inititalizator.Invoke(Context));
                }
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
                root.Invoke(Context);
            }
            finally
            {
                System.Runtime.GCSettings.LatencyMode = lm;
                Context.Deactivate();
            }
        }
    }
}