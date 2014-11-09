using System;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class FunctionStatement : CodeNode
    {
        internal sealed class GeneratorInitializator : Function
        {
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
                    thread.TrySetApartmentState(ApartmentState.STA);
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
                    while (thread.ThreadState != ThreadState.Suspended)
                        Thread.Sleep(0);
                    thread.Resume();
                    while (generatorContext.abort == AbortType.None)
                        Thread.Sleep(0);
                    var res = JSObject.CreateObject();
                    res["value"] = generatorContext.abortInfo;
                    res["done"] = generatorContext.abort == AbortType.Return;
                    return res;
                }
                else
                {
                    if (thread.ThreadState == ThreadState.Suspended)
                    {
                        generatorContext.abortInfo = args[0];
                        generatorContext.abort = AbortType.None;
                        thread.Resume();
                        while (generatorContext.abort == AbortType.None)
                            Thread.Sleep(0);
                        var res = JSObject.CreateObject();
                        res["value"] = generatorContext.abortInfo;
                        res["done"] = generatorContext.abort == AbortType.Return;
                        return res;
                    }
                    else
                    {
                        var res = JSObject.CreateObject();
                        res["done"] = true;
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
                        thread.Resume();
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

        [Serializable]
        internal sealed class FunctionReference : VariableReference
        {
            private FunctionStatement owner;

            public FunctionStatement Owner { get { return owner; } }

            public override string Name
            {
                get { return owner.name; }
            }

            internal override JSObject Evaluate(Context context)
            {
                return owner.Evaluate(context);
            }

            public FunctionReference(FunctionStatement owner)
            {
                functionDepth = -1;
                this.owner = owner;
            }

            public override string ToString()
            {
                return owner.ToString();
            }
        }

        [Serializable]
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

            public override string ToString()
            {
                return name;
            }
        }

        internal bool assignToArguments;
        internal bool isClear;
        internal bool containsEval;
        internal bool containsArguments;
        internal bool isRecursive;
        internal bool containsWith;
        internal VariableDescriptor[] arguments;
        internal CodeBlock body;
        internal string name;
        internal FunctionType type;

        public CodeBlock Body { get { return body; } }
        public string Name { get { return name; } }
        public ReadOnlyCollection<VariableDescriptor> Parameters { get { return new ReadOnlyCollection<VariableDescriptor>(arguments); } }
        public VariableReference Reference { get; private set; }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       JITHelpers.methodof(Evaluate),
                       JITHelpers.ContextParameter
                       );
        }

#endif

        internal FunctionStatement(string name)
        {
            Reference = new FunctionReference(this);
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
                else throw new JSException((new SyntaxError("Invalid function name at " + Tools.PositionToTextcord(code, nameStartPos))));
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new JSException(new SyntaxError("Unexpected char at " + Tools.PositionToTextcord(code, i)));
            }
            else if (mode == FunctionType.Get || mode == FunctionType.Set)
                throw new JSException(new SyntaxError("Getters and Setters must have name"));
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new JSException(new SyntaxError("Unexpected char at " + Tools.PositionToTextcord(code, i)));
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                    throw new JSException((new SyntaxError("Invalid description of function arguments at " + Tools.PositionToTextcord(code, nameStartPos))));
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
                throw new JSException(new SyntaxError("Unexpected char at " + Tools.PositionToTextcord(code, i)));
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
                state.AllowStrict = true;
                body = CodeBlock.Parse(state, ref i).Statement as CodeBlock;
            }
            finally
            {
                if (mode == FunctionType.Generator)
                    state.AllowYield.Pop();
                state.AllowBreak.Pop();
                state.AllowContinue.Pop();
                state.AllowStrict = false;
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
                    throw new JSException((new Core.BaseTypes.SyntaxError("Functions name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, index))));
                for (int j = parameters.Count; j-- > 0; )
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        throw new JSException((new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, index))));
                }
            }
            if (mode == FunctionType.Function && string.IsNullOrEmpty(name))
                mode = FunctionType.AnonymousFunction;
            FunctionStatement func = new FunctionStatement(name)
            {
                arguments = (from prm in parameters select prm.descriptor).ToArray(),
                body = body,
                type = mode,
                Position = index,
                Length = i - index,
                containsWith = state.containsWith.Peek() || (needSwitchCWith && state.containsWith.Pop())
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
                    List<CodeNode> args = new List<CodeNode>();
                    i++;
                    for (; ; )
                    {
                        while (char.IsWhiteSpace(code[i])) i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do i++; while (char.IsWhiteSpace(code[i]));
                        args.Add(ExpressionStatement.Parse(state, ref i, false).Statement);
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
            if (type == FunctionType.Generator)
                return new GeneratorInitializator(new Function(context, this));
            return new Function(context, this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            if (body.localVariables != null)
                return false;
            var bodyCode = body as CodeNode;
            var nvars = new Dictionary<string, VariableDescriptor>();
            bodyCode.Build(ref bodyCode, 0, nvars, strict);
            if (type == FunctionType.Function && !string.IsNullOrEmpty(name))
            {
                VariableDescriptor fdesc = null;
                if (Reference.Descriptor == null)
                    Reference.descriptor = new VariableDescriptor(Reference, true, Reference.functionDepth + 1) { owner = this };
                if (System.Array.FindIndex(arguments, x => x.Name == Reference.descriptor.name) == -1)
                    if (nvars.TryGetValue(name, out fdesc) && !fdesc.Defined)
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
            for (var i = 0; i < arguments.Length; i++)
            {
                VariableDescriptor desc = null;
                if (nvars.TryGetValue(arguments[i].Name, out desc) && desc.Inititalizator == null)
                {
                    desc.references.AddRange(arguments[i].references);
                    desc.defined = true;
                    arguments[i] = desc;
                    arguments[i].owner = this;
                }
            }
            body = bodyCode as CodeBlock;
            if (body.variables != null)
            {
                for (var i = body.variables.Length; i-- > 0; )
                {
                    if (!body.variables[i].Defined
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
            checkUsings();
            return false;
        }

        private void checkUsings()
        {
            isClear = true;
            if (body == null
                || body.body == null
                || body.body.Length == 0)
                return;
            for (var i = 0; i < body.variables.Length; i++)
            {
                containsArguments |= body.variables[i].name == "arguments" && body.variables[i].Inititalizator == null;
                containsEval |= body.variables[i].name == "eval";
                isRecursive |= body.variables[i].name == name;
                isClear &= !containsEval;
            }
            if (body.localVariables != null)
                for (var i = 0; i < body.localVariables.Length; i++)
                    isClear &= body.localVariables[i].Inititalizator == null;
            isClear &= body.variables.Length == (body.localVariables == null ? 0 : body.localVariables.Length) + arguments.Length;
            ICollection t = null;
            assignToArguments = containsArguments && (t = body.variables.First(x => x.name == "arguments").assignations) != null && t.Count != 0;
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
