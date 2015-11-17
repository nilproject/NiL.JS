using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ParameterDescriptor : VariableDescriptor
    {
        public bool IsRest { get; private set; }

        internal ParameterDescriptor(string name, bool rest, int depth)
            : base(name, depth)
        {
            this.IsRest = rest;
        }

        public override string ToString()
        {
            if (IsRest)
                return "..." + name;
            return name;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class ParameterReference : VariableReference
    {
        public override string Name
        {
            get { return descriptor.name; }
        }

        internal ParameterReference(string name, bool rest, int depth)
        {
            descriptor = new ParameterDescriptor(name, rest, depth);
            descriptor.references.Add(this);
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return Descriptor.ToString();
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class FunctionDefinition : EntityDefinition
    {
        #region Runtime
        internal int parametersStored;
        internal int recursionDepth;
        #endregion

        internal readonly FunctionStatistics statistic;
        internal ParameterDescriptor[] parameters;
        internal CodeBlock body;
        internal FunctionType type;
        internal bool strict;
#if DEBUG
        internal bool trace;
#endif

        public CodeBlock Body { get { return body; } }
        public ReadOnlyCollection<ParameterDescriptor> Parameters { get { return new ReadOnlyCollection<ParameterDescriptor>(parameters); } }

        public override bool Hoist
        {
            get { return true; }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return statistic.ContainsYield;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
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

        internal FunctionDefinition(string name)
        {
            parameters = new ParameterDescriptor[0];
            body = new CodeBlock(new CodeNode[0]);
            body.variables = new VariableDescriptor[0];
            statistic = new FunctionStatistics();
            this.name = name;
        }

        internal static CodeNode ParseFunction(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionType.Function);
        }

        internal static CodeNode ParseArrow(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionType.Arrow);
        }

        internal static CodeNode Parse(ParsingState state, ref int index, FunctionType mode)
        {
            string code = state.Code;
            int i = index;
            switch (mode)
            {
                case FunctionType.Function:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return null;
                        if (code[i] == '*')
                        {
                            mode = FunctionType.Generator;
                            i++;
                        }
                        else if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionType.Getter:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return null;
                        if ((!char.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionType.Setter:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return null;
                        if ((!char.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionType.MethodGenerator:
                case FunctionType.Method:
                    {
                        if (code[i] == '*')
                        {
                            mode = FunctionType.MethodGenerator;
                            i++;
                        }
                        else if (mode == FunctionType.MethodGenerator)
                            throw new ArgumentException("mode");
                        break;
                    }
                case FunctionType.Arrow:
                    {
                        break;
                    }
                default:
                    throw new NotImplementedException(mode.ToString());
            }
            var inExp = state.InExpression;
            state.InExpression = 0;
            while (char.IsWhiteSpace(code[i]))
                i++;
            var parameters = new List<ParameterDescriptor>();
            string name = null;
            bool arrowWithSunglePrm = false;
            int nameStartPos = 0;
            if (mode != FunctionType.Arrow)
            {
                if (code[i] != '(')
                {
                    nameStartPos = i;
                    if (Parser.ValidateName(code, ref i, false, true, state.strict))
                        name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict);
                    else if ((mode == FunctionType.Getter || mode == FunctionType.Setter) && Parser.ValidateString(code, ref i, false))
                        name = Tools.Unescape(code.Substring(nameStartPos + 1, i - nameStartPos - 2), state.strict);
                    else if ((mode == FunctionType.Getter || mode == FunctionType.Setter) && Parser.ValidateNumber(code, ref i))
                        name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict);
                    else
                        ExceptionsHelper.ThrowSyntaxError("Invalid function name", code, nameStartPos, i - nameStartPos);
                    while (char.IsWhiteSpace(code[i]))
                        i++;
                    if (code[i] != '(')
                        ExceptionsHelper.ThrowUnknownToken(code, i);
                }
                else if (mode == FunctionType.Getter || mode == FunctionType.Setter)
                    ExceptionsHelper.ThrowSyntaxError("Getters and Setters must have name", code, index);
                else if (mode == FunctionType.Method)
                    ExceptionsHelper.ThrowSyntaxError("Methods must have name", code, index);
            }
            else if (code[i] != '(')
            {
                arrowWithSunglePrm = true;
                i--;
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                ExceptionsHelper.ThrowSyntaxError("Unexpected char at ", code, i);
            while (code[i] != ')')
            {
                if (parameters.Count == 255 || (mode == FunctionType.Setter && parameters.Count == 1) || mode == FunctionType.Getter)
                    ExceptionsHelper.ThrowSyntaxError(string.Format("Too many arguments for function \"{0}\"", name), code, index);

                bool rest = Parser.Validate(code, "...", ref i);

                int n = i;
                if (!Parser.ValidateName(code, ref i, state.strict))
                    ExceptionsHelper.ThrowUnknownToken(code, nameStartPos);
                var pname = Tools.Unescape(code.Substring(n, i - n), state.strict);
                var desc = new ParameterReference(pname, rest, state.functionsDepth + 1)
                {
                    Position = n,
                    Length = i - n
                }.Descriptor as ParameterDescriptor;
                parameters.Add(desc);
                while (char.IsWhiteSpace(code[i]))
                    i++;
                if (arrowWithSunglePrm)
                {
                    i--;
                    break;
                }
                if (code[i] == '=')
                {
                    if (mode == FunctionType.Arrow)
                        ExceptionsHelper.ThrowSyntaxError("Parameters of arrow-function cannot have initializer", code, i);

                    if (rest)
                        ExceptionsHelper.ThrowSyntaxError("Rest parameters can not have an initializer", code, i);
                    do
                        i++;
                    while (char.IsWhiteSpace(code[i]));
                    desc.initializer = ExpressionTree.Parse(state, ref i, false, false) as Expression;
                }
                if (code[i] == ',')
                {
                    if (rest)
                        ExceptionsHelper.ThrowSyntaxError("Rest parameters must be the last in parameters list", code, i);
                    do
                        i++;
                    while (char.IsWhiteSpace(code[i]));
                }
            }
            switch (mode)
            {
                case FunctionType.Setter:
                    {
                        if (parameters.Count != 1)
                            ExceptionsHelper.ThrowSyntaxError("Setter must have only one argument", code, index);
                        break;
                    }
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            CodeBlock body = null;
            if (mode == FunctionType.Arrow)
            {
                if (!Parser.Validate(code, "=>", ref i))
                    ExceptionsHelper.ThrowSyntaxError("Expected \"=>\"", code, i);
                while (char.IsWhiteSpace(code[i]))
                    i++;
            }
            if (code[i] != '{')
            {
                if (mode == FunctionType.Arrow)
                    body = new CodeBlock(new CodeNode[] { new ReturnStatement(ExpressionTree.Parse(state, ref i) as Expression) });
                else
                    ExceptionsHelper.ThrowUnknownToken(code, i);
            }
            else
            {
                var oldCodeContext = state.CodeContext;
                if (mode == FunctionType.Generator || mode == FunctionType.MethodGenerator)
                    state.CodeContext |= CodeContext.InGenerator;
                var labels = state.Labels;
                state.Labels = new List<string>();
                state.functionsDepth++;
                state.AllowReturn++;
                try
                {
                    state.AllowBreak.Push(false);
                    state.AllowContinue.Push(false);
                    state.AllowDirectives = true;
                    body = CodeBlock.Parse(state, ref i) as CodeBlock;
                }
                finally
                {
                    state.CodeContext = oldCodeContext;
                    state.AllowBreak.Pop();
                    state.AllowContinue.Pop();
                    state.AllowDirectives = false;
                    state.Labels = labels;
                    state.functionsDepth--;
                    state.AllowReturn--;
                }
                if (mode == FunctionType.Function && string.IsNullOrEmpty(name))
                    mode = FunctionType.AnonymousFunction;
            }
            if (body.strict || (parameters.Count > 0 && parameters[parameters.Count - 1].IsRest) || mode == FunctionType.Arrow)
            {
                for (var j = parameters.Count; j-- > 1; )
                    for (var k = j; k-- > 0; )
                        if (parameters[j].Name == parameters[k].Name)
                            ExceptionsHelper.ThrowSyntaxError("Duplicate names of function parameters not allowed in strict mode", code, index);
                if (name == "arguments" || name == "eval")
                    ExceptionsHelper.ThrowSyntaxError("Functions name cannot be \"arguments\" or \"eval\" in strict mode at", code, index);
                for (int j = parameters.Count; j-- > 0; )
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        ExceptionsHelper.ThrowSyntaxError("Parameters name cannot be \"arguments\" or \"eval\" in strict mode at", code, parameters[j].references[0].Position, parameters[j].references[0].Length);
                }
            }
            FunctionDefinition func = new FunctionDefinition(name)
            {
                parameters = parameters.ToArray(),
                body = body,
                strict = body.strict,
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
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i]))
                    i++;
                if (i < code.Length && code[i] == '(')
                {
                    var args = new List<Expression>();
                    i++;
                    for (; ; )
                    {
                        while (char.IsWhiteSpace(code[i]))
                            i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do
                                i++;
                            while (char.IsWhiteSpace(code[i]));
                        args.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                    }
                    i++;
                    index = i;
                    while (i < code.Length && char.IsWhiteSpace(code[i]))
                        i++;
                    if (i < code.Length && code[i] == ';')
                        ExceptionsHelper.Throw((new SyntaxError("Expression can not start with word \"function\"")));
                    return new CallOperator(func, args.ToArray());
                }
                else
                    i = tindex;
            }
            state.InExpression = inExp;
            if (name != null)
            {
                func.Reference.defineFunctionDepth = state.functionsDepth;
                func.Reference.Position = nameStartPos;
                func.Reference.Length = name.Length;
            }
            index = i;
            return func;
        }

        public override JSValue Evaluate(Context context)
        {
            return MakeFunction(context);
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new CodeNode[1 + parameters.Length + (Reference != null ? 1 : 0)];
            for (var i = 0; i < parameters.Length; i++)
                res[i] = parameters[i].references[0];
            res[parameters.Length] = body;
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
            if (type == FunctionType.Generator || type == FunctionType.MethodGenerator)
                return new GeneratorFunction(new Function(context, this));
#endif
            return new Function(context, this);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            if (body.builded)
                return false;
            if (stats != null)
                stats.ContainsInnerFunction = true;
            _codeContext = codeContext;

            if ((codeContext & CodeContext.InLoop) != 0 && message != null)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, EndPosition - Position), Strings.FunctionInLoop);

            var bodyCode = body as CodeNode;
            var nvars = new Dictionary<string, VariableDescriptor>();
            for (var i = 0; i < parameters.Length; i++)
            {
                nvars[parameters[i].name] = parameters[i];
                parameters[i].owner = this;
                parameters[i].isDefined = true;

                if (parameters[i].initializer != null)
                {
                    CodeNode initializer = parameters[i].initializer;
                    parameters[i].initializer.Build(ref initializer, depth, variables, codeContext, message, statistic, opts);
                    parameters[i].initializer = (Expression)initializer;
                }
            }
            statistic.ContainsRestParameters = parameters.Length > 0 && parameters[parameters.Length - 1].IsRest;
            bodyCode.Build(ref bodyCode, 0, nvars, codeContext & ~(CodeContext.Conditional | CodeContext.InExpression | CodeContext.InEval), message, statistic, opts);
            if (type == FunctionType.Function && !string.IsNullOrEmpty(name))
            {
                VariableDescriptor fdesc = null;
                if (Reference.descriptor == null)
                    Reference.descriptor = new VariableDescriptor(Reference, true, Reference.defineFunctionDepth);
                if (System.Array.FindIndex(parameters, x => x.Name == Reference.descriptor.name) == -1)
                {
                    if (nvars.TryGetValue(name, out fdesc) && !fdesc.IsDefined)
                    {
                        foreach (var r in fdesc.references)
                            r.descriptor = Reference.Descriptor;
                        Reference.descriptor.references.AddRange(fdesc.references);
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
            }
            if (message != null)
            {
                for (var i = parameters.Length; i-- > 0; )
                {
                    if (parameters[i].ReferenceCount == 1)
                        message(MessageLevel.Recomendation, new CodeCoordinates(0, parameters[i].references[0].Position, 0), "Unused variable \"" + parameters[i].name + "\"");
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
                        && (type == FunctionType.Arrow
                            || (body.variables[i].name != "this"
                                && body.variables[i].name != "arguments")))
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

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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
                            message(MessageLevel.Warning, new CodeCoordinates(0, parameters[i].references[0].Position, 0), "Type of return value is ambiguous");
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
                statistic.ContainsInnerFunction = true;
                statistic.ContainsTry |= this.statistic.ContainsTry;
                statistic.ContainsWith |= this.statistic.ContainsWith;
                statistic.ContainsYield |= this.statistic.ContainsYield;
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
                    containsFunctions |= body.localVariables[i].initializer != null;
                statistic.ContainsInnerFunction = containsFunctions;
            }
            if (body.variables != null)
            {
                for (var i = 0; i < body.variables.Length; i++)
                {
                    statistic.IsRecursive |= body.variables[i].name == name;
                    body.variables[i].captured |= statistic.ContainsEval;
                    statistic.ContainsArguments |= body.variables[i].name == "arguments";
                }
            }
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

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            CodeNode cn = body;
            cn.Decompose(ref cn);
            body = (CodeBlock)cn;
        }

        public override string ToString()
        {
            string res;
            switch (type)
            {
                case FunctionType.Generator:
                    {
                        res = "functions* ";
                        break;
                    }
                case FunctionType.Method:
                    {
                        res = "";
                        break;
                    }
                case FunctionType.Getter:
                    {
                        res = "get ";
                        break;
                    }
                case FunctionType.Setter:
                    {
                        res = "set ";
                        break;
                    }
                default:
                    {
                        res = "function ";
                        break;
                    }
            }
            res += name + "(";
            if (parameters != null)
                for (int i = 0; i < parameters.Length; )
                    res += parameters[i] + (++i < parameters.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }
    }
}
