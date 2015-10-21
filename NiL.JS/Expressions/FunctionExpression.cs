using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class FunctionExpression : Expression
    {
#if !PORTABLE
        internal sealed class GeneratorInitializator : Function
        {
            public override JSObject prototype
            {
                get
                {
                    return null;
                }
                set
                {

                }
            }

            private Function generator;
            [Hidden]
            public GeneratorInitializator(Function generator)
            {
                this.generator = generator;
            }

            [Hidden]
            public override JSObject Invoke(JSObject thisBind, Arguments args)
            {
                return TypeProxy.Proxy(new Generator(generator, thisBind, args));
            }

            internal override JSObject GetDefaultPrototype()
            {
                return TypeProxy.GetPrototype(typeof(Function));
            }
        }

        internal sealed class Generator : IDisposable
        {
            private Context generatorContext;
            private Arguments initialArgs;
            private Thread thread;
            private Function generator;
            private JSObject self;

            [Hidden]
            public Generator(Function generator, JSObject self, Arguments args)
            {
                this.generator = generator;
                this.initialArgs = args;
                this.self = self;
            }

            ~Generator()
            {
                Dispose();
            }

            public JSObject next(Arguments args)
            {
                if (thread == null)
                {
                    thread = new Thread(() =>
                    {
                        generator.Invoke(self, initialArgs);
                        GC.SuppressFinalize(this);
                    });
                    thread.Start();
                    do
                    {
                        for (var i = 0; i < Context.MaxConcurentContexts; i++)
                        {
                            if (Context.runnedContexts[i] == null)
                                break;
                            if (Context.runnedContexts[i].threadId == thread.ManagedThreadId)
                            {
                                generatorContext = Context.runnedContexts[i];
                                break;
                            }
                        }
                    }
                    while (generatorContext == null);
                    while (generatorContext.abort == AbortType.None)
#if !NET35
                        Thread.Yield();
#else
                        Thread.Sleep(0);
#endif
                    var res = JSObject.CreateObject();
                    res.fields["value"] = generatorContext.abortInfo;
                    res.fields["done"] = generatorContext.abort == AbortType.Return;
                    return res;
                }
                else
                {
                    if (thread.ThreadState == ThreadState.Running
                        || thread.ThreadState == ThreadState.WaitSleepJoin)
                    {
                        generatorContext.abortInfo = args[0];
                        generatorContext.abort = AbortType.None;
                        while (generatorContext.abort == AbortType.None)
#if !NET35
                            Thread.Yield();
#else
                            Thread.Sleep(0);
#endif
                        var res = JSObject.CreateObject();
                        res.fields["value"] = generatorContext.abortInfo;
                        res.fields["done"] = generatorContext.abort == AbortType.Return;
                        return res;
                    }
                    else
                    {
                        var res = JSObject.CreateObject();
                        res.fields["done"] = true;
                        return res;
                    }
                }
            }

            public void @throw()
            {
                if (thread != null)
                {
                    if (thread.ThreadState == ThreadState.Suspended)
                    {
                        generatorContext.abort = AbortType.Exception;
                    }
                }
            }

            public void Dispose()
            {
                try
                {
                    thread.Abort();
                }
                catch
                {

                }
            }
        }
#endif

#if !PORTABLE
        [Serializable]
#endif
        internal sealed class FunctionReference : VariableReference
        {
            private FunctionExpression owner;

            public FunctionExpression Owner { get { return owner; } }

            public override string Name
            {
                get { return owner.name; }
            }

            internal override JSObject Evaluate(Context context)
            {
                return owner.Evaluate(context);
            }

            public FunctionReference(FunctionExpression owner)
            {
                functionDepth = -1;
                this.owner = owner;
            }

            public override T Visit<T>(Visitor<T> visitor)
            {
                return visitor.Visit(this);
            }

            public override string ToString()
            {
                return owner.ToString();
            }
        }

#if !PORTABLE
        [Serializable]
#endif
        internal sealed class ParameterReference : VariableReference
        {
            private string name;

            public override string Name
            {
                get { return name; }
            }

            internal override JSObject Evaluate(Context context)
            {
                return null;
            }

            public ParameterReference(string name, int fdepth)
            {
                functionDepth = fdepth;
                this.name = name;
                descriptor = new VariableDescriptor(this, true, fdepth);
            }

            public override T Visit<T>(Visitor<T> visitor)
            {
                return visitor.Visit(this);
            }

            public override string ToString()
            {
                return name;
            }
        }

        internal VariableReference reference;
        #region Runtime
        internal int parametersStored;
        internal int recursiveDepth;
        #endregion
        internal FunctionStatistics statistic;
        internal VariableDescriptor[] arguments;
        internal CodeBlock body;
        internal string name;
        internal FunctionType type;
#if DEBUG
        internal bool trace;
#endif

        public CodeBlock Body { get { return body; } }
        public string Name { get { return name; } }
        public ReadOnlyCollection<VariableDescriptor> Parameters { get { return new ReadOnlyCollection<VariableDescriptor>(arguments); } }
        public VariableReference Reference { get { return reference; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Function;
            }
        }

        internal FunctionExpression(string name)
        {
            reference = new FunctionReference(this);
            arguments = new VariableDescriptor[0];
            body = new CodeBlock(new CodeNode[0], false);
            body.variables = new VariableDescriptor[0];
            this.name = name;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionType.Function);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, FunctionType mode)
        {
            string code = state.Code;
            int i = index;
            switch (mode)
            {
                case FunctionType.Function:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return new ParseResult();
                        if (code[i] == '*')
                        {
                            mode = FunctionType.Generator;
                            i++;
                        }
                        else if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
                case FunctionType.Get:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
                case FunctionType.Set:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
            }
            var inExp = state.InExpression;
            state.InExpression = 0;
            while (char.IsWhiteSpace(code[i])) i++;
            var parameters = new List<ParameterReference>();
            string name = null;
            int nameStartPos = 0;
            if (code[i] != '(')
            {
                nameStartPos = i;
                if (Parser.ValidateName(code, ref i, false, true, state.strict.Peek()))
                    name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict.Peek());
                else if ((mode == FunctionType.Get || mode == FunctionType.Set) && Parser.ValidateString(code, ref i, false))
                    name = Tools.Unescape(code.Substring(nameStartPos + 1, i - nameStartPos - 2), state.strict.Peek());
                else if ((mode == FunctionType.Get || mode == FunctionType.Set) && Parser.ValidateNumber(code, ref i))
                    name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict.Peek());
                else throw new JSException((new SyntaxError("Invalid function name at " + CodeCoordinates.FromTextPosition(code, nameStartPos, i - nameStartPos))));
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new JSException(new SyntaxError("Unexpected char at " + CodeCoordinates.FromTextPosition(code, i, 0)));
            }
            else if (mode == FunctionType.Get || mode == FunctionType.Set)
                throw new JSException(new SyntaxError("Getters and Setters must have name"));
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new JSException(new SyntaxError("Unexpected char at " + CodeCoordinates.FromTextPosition(code, i, 0)));
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                    throw new JSException((new SyntaxError("Invalid description of function arguments at " + CodeCoordinates.FromTextPosition(code, nameStartPos, 0))));
                var pname = Tools.Unescape(code.Substring(n, i - n), state.strict.Peek());
                parameters.Add(new ParameterReference(pname, state.functionsDepth + 1) { Position = n, Length = i - n });
                while (char.IsWhiteSpace(code[i])) i++;
            }
            switch (mode)
            {
                case FunctionType.Get:
                    {
                        if (parameters.Count != 0)
                            throw new JSException(new SyntaxError("getter have many arguments"));
                        break;
                    }
                case FunctionType.Set:
                    {
                        if (parameters.Count != 1)
                            throw new JSException(new SyntaxError("setter have invalid arguments"));
                        break;
                    }
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new JSException(new SyntaxError("Unexpected char at " + CodeCoordinates.FromTextPosition(code, i, 0)));
            bool needSwitchCWith = state.containsWith.Peek();
            if (needSwitchCWith)
                state.containsWith.Push(false);
            var labels = state.Labels;
            state.Labels = new List<string>();
            state.functionsDepth++;
            state.AllowReturn++;
            CodeBlock body = null;
            try
            {
                if (mode == FunctionType.Generator)
                    state.AllowYield.Push(true);
                state.AllowBreak.Push(false);
                state.AllowContinue.Push(false);
                state.AllowDirectives = true;
                body = CodeBlock.Parse(state, ref i).Statement as CodeBlock;
            }
            finally
            {
                if (mode == FunctionType.Generator)
                    state.AllowYield.Pop();
                state.AllowBreak.Pop();
                state.AllowContinue.Pop();
                state.AllowDirectives = false;
                state.Labels = labels;
                state.functionsDepth--;
                state.AllowReturn--;
            }
            if (body.strict)
            {
                for (var j = parameters.Count; j-- > 1; )
                    for (var k = j; k-- > 0; )
                        if (parameters[j].Name == parameters[k].Name)
                            throw new JSException(new SyntaxError("Duplicate names of function parameters not allowed in strict mode."));
                if (name == "arguments" || name == "eval")
                    throw new JSException((new SyntaxError("Functions name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(code, index, 0))));
                for (int j = parameters.Count; j-- > 0; )
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        throw new JSException((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(code, parameters[j].Position, parameters[j].Length))));
                }
            }
            if (mode == FunctionType.Function && string.IsNullOrEmpty(name))
                mode = FunctionType.AnonymousFunction;
            FunctionExpression func = new FunctionExpression(name)
            {
                arguments = (from prm in parameters select prm.descriptor).ToArray(),
                body = body,
                type = mode,
                Position = index,
                Length = i - index,
#if DEBUG
                trace = body.directives != null ? body.directives.Contains("debug trace") : false
#endif
            };
            if (inExp == 0 && mode == FunctionType.Function)
            // Позволяет делать вызов сразу при объявлении функции 
            // (в таком случае функция не добавляется в контекст).
            // Если убрать проверку, то в тех сулчаях,
            // когда определение и вызов стоят внутри выражения,
            // будет выдано исключение, потому,
            // что тогда это уже не определение и вызов функции,
            // а часть выражения, которые не могут начинаться со слова "function".
            // За красивыми словами "может/не может" кроется другая хрень: если бы это было выражение, 
            // то прямо тут надо было бы разбирать тот оператор, который стоит после определения функции,
            // что не разумно
            {
                var tindex = i;
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if (i < code.Length && code[i] == '(')
                {
                    var args = new List<Expression>();
                    i++;
                    for (; ; )
                    {
                        while (char.IsWhiteSpace(code[i])) i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do i++; while (char.IsWhiteSpace(code[i]));
                        args.Add((Expression)ExpressionTree.Parse(state, ref i, false).Statement);
                    }
                    i++;
                    index = i;
                    while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
                    if (i < code.Length && code[i] == ';')
                        throw new JSException((new SyntaxError("Expression can not start with word \"function\"")));
                    return new ParseResult()
                    {
                        IsParsed = true,
                        Statement = new Expressions.Call(func, args.ToArray())
                    };
                }
                else
                    i = tindex;
            }
            state.InExpression = inExp;
            if (name != null)
            {
                func.Reference.functionDepth = state.functionsDepth;
                func.Reference.Position = nameStartPos;
                func.Reference.Length = name.Length;
            }
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = func
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            return MakeFunction(context);
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new CodeNode[1 + arguments.Length + (Reference != null ? 1 : 0)];
            for (var i = 0; i < arguments.Length; i++)
                res[i] = arguments[i].references[0];
            res[arguments.Length] = body;
            if (Reference != null)
                res[res.Length - 1] = Reference;
            return res;
        }

        /// <summary>
        /// Создаёт функцию, описанную выбранным выражением в контексте указанного сценария.
        /// </summary>
        /// <param name="script">Сценарий, контекст которого будет родительским для контекста выполнения функции.</param>
        /// <returns></returns>
        public Function MakeFunction(Script script)
        {
            return MakeFunction(script.Context);
        }

        /// <summary>
        /// Создаёт функцию, описанную выбранным выражением в контексте указанного сценария.
        /// </summary>
        /// <param name="script">Сценарий, контекст которого будет родительским для контекста выполнения функции.</param>
        /// <returns></returns>
        public Function MakeFunction(Context context)
        {
#if !PORTABLE
            if (type == FunctionType.Generator)
                return new GeneratorInitializator(new Function(context, this));
#endif
            return new Function(context, this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            if (body.builded)
                return false;
            if (stats != null)
                stats.ContainsInnerFunction = true;
            codeContext = state;

            if ((state & _BuildState.InLoop) != 0 && message != null)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, EndPosition - Position), "Do not define function inside a loop");

            var bodyCode = body as CodeNode;
            var nvars = new Dictionary<string, VariableDescriptor>();
            for (var i = 0; i < arguments.Length; i++)
            {
                nvars[arguments[i].name] = arguments[i];
                arguments[i].owner = this;
                arguments[i].isDefined = true;
            }
            var stat = new FunctionStatistics();
            bodyCode.Build(ref bodyCode, 0, nvars, state & ~_BuildState.Conditional, message, stat, opts);
            if (type == FunctionType.Function && !string.IsNullOrEmpty(name))
            {
                VariableDescriptor fdesc = null;
                if (Reference.Descriptor == null)
                    Reference.descriptor = new VariableDescriptor(Reference, true, Reference.functionDepth);// { owner = this };
                if (System.Array.FindIndex(arguments, x => x.Name == Reference.descriptor.name) == -1)
                    if (nvars.TryGetValue(name, out fdesc) && !fdesc.IsDefined)
                    {
                        foreach (var r in fdesc.references)
                            r.descriptor = Reference.Descriptor;
                        Reference.Descriptor.references.AddRange(fdesc.references);
                        for (var i = body.variables.Length; i-- > 0; )
                        {
                            if (body.variables[i] == fdesc)
                            {
                                body.variables[i] = Reference.Descriptor;
                                break;
                            }
                        }
                    }
            }
            /*
            for (var i = 0; i < arguments.Length; i++)
            {
                VariableDescriptor desc = null;
                if (nvars.TryGetValue(arguments[i].Name, out desc) && desc.Inititalizator == null)
                {
                    desc.references.AddRange(arguments[i].references);
                    desc.isDefined = true;
                    desc.defineDepth = arguments[i].defineDepth;
                    arguments[i] = desc;
                    arguments[i].owner = this;
                }
            }
            */
            if (message != null)
            {
                for (var i = arguments.Length; i-- > 0; )
                {
                    if (arguments[i].ReferenceCount == 1)
                        message(MessageLevel.Recomendation, new CodeCoordinates(0, arguments[i].references[0].Position, 0), "Unused variable \"" + arguments[i].name + "\"");
                    else
                        break;
                }
            }
            body = bodyCode as CodeBlock;
            if (body.variables != null)
            {
                for (var i = body.variables.Length; i-- > 0; )
                {
                    if (!body.variables[i].IsDefined
                        && body.variables[i].name != "this"
                        && body.variables[i].name != "arguments")
                    // все необъявленные переменные нужно прокинуть вниз для того,
                    // чтобы во всех местах их использования был один дескриптор и один кеш
                    {
                        VariableDescriptor desc = null;
                        if (variables.TryGetValue(body.variables[i].name, out desc))
                        {
                            desc.captured = true;
                            foreach (var r in body.variables[i].References)
                            {
                                desc.references.Add(r);
                                r.descriptor = desc;
                            }
                            if (body.variables[i].assignations != null)
                                if (desc.assignations == null)
                                    desc.assignations = body.variables[i].assignations;
                                else
                                    desc.assignations.AddRange(body.variables[i].assignations);
                            body.variables[i] = desc;
                        }
                        else
                        {
                            body.variables[i].captured = true;
                            variables.Add(body.variables[i].name, body.variables[i]);
                        }
                    }
                }
            }
            statistic = stat;
            checkUsings();
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var bd = body as CodeNode;
            body.Optimize(ref bd, this, message, opts, this.statistic);
            if (this.statistic.Returns.Count > 0)
            {
                this.statistic.ResultType = this.statistic.Returns[0].ResultType;
                for (var i = 1; i < this.statistic.Returns.Count; i++)
                {
                    if (this.statistic.ResultType != this.statistic.Returns[i].ResultType)
                    {
                        this.statistic.ResultType = PredictedType.Ambiguous;
                        if (message != null
                            && this.statistic.ResultType >= PredictedType.Undefined
                            && this.statistic.Returns[i].ResultType >= PredictedType.Undefined)
                            message(MessageLevel.Warning, new CodeCoordinates(0, arguments[i].references[0].Position, 0), "Type of return value is ambiguous");
                        break;
                    }
                }
            }
            else
                this.statistic.ResultType = PredictedType.Undefined;
            if (statistic != null)
            {
                statistic.ContainsDebugger |= this.statistic.ContainsDebugger;
                statistic.ContainsEval |= this.statistic.ContainsEval;
                statistic.ContainsInnerFunction |= true;
                statistic.ContainsTry |= this.statistic.ContainsTry;
                statistic.ContainsWith |= this.statistic.ContainsWith;
                statistic.UseCall |= this.statistic.UseCall;
                statistic.UseGetMember |= this.statistic.UseGetMember;
                statistic.UseThis |= this.statistic.UseThis;
            }
        }

        private void checkUsings()
        {
            if (body == null
                || body.lines == null
                || body.lines.Length == 0)
                return;
            var containsFunctions = statistic.ContainsInnerFunction;
            if (!containsFunctions)
            {
                for (var i = 0; !containsFunctions && i < body.localVariables.Length; i++)
                    containsFunctions |= body.localVariables[i].Initializer != null;
                statistic.ContainsInnerFunction = containsFunctions;
            }
            for (var i = 0; !statistic.IsRecursive && i < body.variables.Length; i++)
                statistic.IsRecursive |= body.variables[i].name == name;
            if (body.variables != null)
                for (var i = 0; i < body.variables.Length; i++)
                    body.variables[i].captured |= statistic.ContainsEval;
        }
#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            body.TryCompile(true, false, null, new List<CodeNode>());
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = ((FunctionType)((int)type & 3)).ToString().ToLowerInvariant() + " " + name + "(";
            if (arguments != null)
                for (int i = 0; i < arguments.Length; )
                    res += arguments[i] + (++i < arguments.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }
    }
}
