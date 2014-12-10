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
            : this(code, null, null)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, CompilerMessageCallback messageCallback)
            : this(code, null, messageCallback)
        {

        }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="parentContext">Родительский контекст для контекста выполнения сценария.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, Context parentContext)
            : this(code, parentContext, null)
        { }

        /// <summary>
        /// Инициализирует объект типа Script и преобрзует код сценария во внутреннее представление.
        /// </summary>
        /// <param name="code">Код скрипта на языке JavaScript.</param>
        /// <param name="parentContext">Родительский контекст для контекста выполнения сценария.</param>
        /// <param name="messageCallback">Делегат обратного вызова, используемый для вывода сообщений компилятора</param>
        public Script(string code, Context parentContext, CompilerMessageCallback messageCallback)
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
                    messageCallback(level, CodeCoordinates.FromTextPosition(code, cord.Column), message);
                } : null as CompilerMessageCallback;
            Parser.Build(ref root, 0, new System.Collections.Generic.Dictionary<string, VariableDescriptor>(), false, icallback);
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
            var bd = body as CodeNode;
            body.Optimize(ref bd, null, icallback);
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

        /// <summary>
        /// Пытается провести частичную или полную компилияцию сценария, начиная с листов синтаксического дерева.
        /// Максимальные по высоте поддеревья, для которых допустима компиляция, будут заменены на листы, 
        /// представляющие скомпилированное выражение.
        /// После этой операции возможно нарушение процесса отладки.
        /// </summary>
        public void TryCompile()
        {
            root.TryCompile(ref root);
        }
    }
}