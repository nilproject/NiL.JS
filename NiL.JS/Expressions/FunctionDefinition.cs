using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ParameterDescriptor : VariableDescriptor
    {
        public ObjectDesctructor Destructor { get; internal set; }
        public bool IsRest { get; private set; }

        internal ParameterDescriptor(string name, bool rest, int depth)
            : base(name, depth)
        {
            IsRest = rest;
            lexicalScope = true;
        }

        public override string ToString()
        {
            if (IsRest)
                return "..." + name;
            return name;
        }
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ParameterReference : VariableReference
    {
        public override string Name
        {
            get { return _descriptor.name; }
        }

        internal ParameterReference(string name, bool rest, int depth)
        {
            _descriptor = new ParameterDescriptor(name, rest, depth);
            _descriptor.references.Add(this);
        }

        public override JSValue Evaluate(Context context)
        {
            if (_descriptor.cacheContext != context)
                throw new InvalidOperationException();

            return _descriptor.cacheRes;
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

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class FunctionDefinition : EntityDefinition
    {
        #region Runtime
        internal int parametersStored;
        internal int recursionDepth;
        #endregion

        internal readonly FunctionInfo _functionInfo;
        internal ParameterDescriptor[] parameters;
        internal CodeBlock _body;
        internal FunctionKind kind;
#if DEBUG
        internal bool trace;
#endif

        public CodeBlock Body { get { return _body; } }
        public ReadOnlyCollection<ParameterDescriptor> Parameters { get { return new ReadOnlyCollection<ParameterDescriptor>(parameters); } }

        protected internal override bool NeedDecompose
        {
            get
            {
                return _functionInfo.NeedDecompose;
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

        public override bool Hoist
        {
            get
            {
                return kind != FunctionKind.Arrow
                    && kind != FunctionKind.AsyncArrow;
            }
        }

        public FunctionKind Kind
        {
            get
            {
                return kind;
            }
        }

        public bool Strict
        {
            get
            {
                return _body?.Strict ?? false;
            }
            internal set
            {
                if (_body != null)
                    _body._strict = value;
            }
        }

        private FunctionDefinition(string name)
            : base(name)
        {
            _functionInfo = new FunctionInfo();
        }

        internal FunctionDefinition()
            : this("anonymous")
        {
            parameters = new ParameterDescriptor[0];
            _body = new CodeBlock(new CodeNode[0])
            {
                _strict = true,
                _variables = new VariableDescriptor[0]
            };
        }

        internal static ParseDelegate ParseFunction(FunctionKind kind)
        {
            return new ParseDelegate((ParseInfo info, ref int index) => Parse(info, ref index, kind));
        }

        internal static CodeNode ParseFunction(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, FunctionKind.Function);
        }

        internal static Expression Parse(ParseInfo state, ref int index, FunctionKind kind)
        {
            string code = state.Code;
            int position = index;
            switch (kind)
            {
                case FunctionKind.AnonymousFunction:
                case FunctionKind.AnonymousGenerator:
                    {
                        break;
                    }
                case FunctionKind.Function:
                    {
                        if (!Parser.Validate(code, "function", ref position))
                            return null;

                        if (code[position] == '*')
                        {
                            kind = FunctionKind.Generator;
                            position++;
                        }
                        else if ((code[position] != '(') && (!Tools.IsWhiteSpace(code[position])))
                            return null;

                        break;
                    }
                case FunctionKind.Getter:
                    {
                        if (!Parser.Validate(code, "get ", ref position))
                            return null;

                        break;
                    }
                case FunctionKind.Setter:
                    {
                        if (!Parser.Validate(code, "set ", ref position))
                            return null;

                        break;
                    }
                case FunctionKind.MethodGenerator:
                case FunctionKind.Method:
                    {
                        if (code[position] == '*')
                        {
                            kind = FunctionKind.MethodGenerator;
                            position++;
                        }
                        else if (kind == FunctionKind.MethodGenerator)
                            throw new ArgumentException("mode");

                        break;
                    }
                case FunctionKind.Arrow:
                    {
                        break;
                    }
                case FunctionKind.AsyncFunction:
                    {
                        if (!Parser.Validate(code, "async", ref position))
                            return null;

                        Tools.SkipSpaces(code, ref position);

                        if (!Parser.Validate(code, "function", ref position))
                            return null;

                        break;
                    }
                default:
                    throw new NotImplementedException(kind.ToString());
            }

            Tools.SkipSpaces(state.Code, ref position);

            var parameters = new List<ParameterDescriptor>();
            CodeBlock body = null;
            string name = null;
            bool arrowWithSunglePrm = false;
            int nameStartPos = 0;
            bool containsDestructuringPrms = false;

            if (kind != FunctionKind.Arrow)
            {
                if (code[position] != '(')
                {
                    nameStartPos = position;
                    if (Parser.ValidateName(code, ref position, false, true, state.strict))
                        name = Tools.Unescape(code.Substring(nameStartPos, position - nameStartPos), state.strict);
                    else if ((kind == FunctionKind.Getter || kind == FunctionKind.Setter) && Parser.ValidateString(code, ref position, false))
                        name = Tools.Unescape(code.Substring(nameStartPos + 1, position - nameStartPos - 2), state.strict);
                    else if ((kind == FunctionKind.Getter || kind == FunctionKind.Setter) && Parser.ValidateNumber(code, ref position))
                        name = Tools.Unescape(code.Substring(nameStartPos, position - nameStartPos), state.strict);
                    else
                        ExceptionHelper.ThrowSyntaxError("Invalid function name", code, nameStartPos, position - nameStartPos);

                    Tools.SkipSpaces(code, ref position);

                    if (code[position] != '(')
                        ExceptionHelper.ThrowUnknownToken(code, position);
                }
                else if (kind == FunctionKind.Getter || kind == FunctionKind.Setter)
                    ExceptionHelper.ThrowSyntaxError("Getter and Setter must have name", code, index);
                else if (kind == FunctionKind.Method || kind == FunctionKind.MethodGenerator)
                    ExceptionHelper.ThrowSyntaxError("Method must have name", code, index);

                position++;
            }
            else if (code[position] != '(')
            {
                arrowWithSunglePrm = true;
            }
            else
            {
                position++;
            }

            Tools.SkipSpaces(code, ref position);

            if (code[position] == ',')
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, code, position);

            while (code[position] != ')')
            {
                if (parameters.Count == 255 || (kind == FunctionKind.Setter && parameters.Count == 1) || kind == FunctionKind.Getter)
                    ExceptionHelper.ThrowSyntaxError(string.Format(Strings.TooManyArgumentsForFunction, name), code, index);

                bool rest = Parser.Validate(code, "...", ref position);

                Expression destructor = null;
                int n = position;
                if (!Parser.ValidateName(code, ref position, state.strict))
                {
                    if (code[position] == '{')
                        destructor = (Expression)ObjectDefinition.Parse(state, ref position);
                    else if (code[position] == '[')
                        destructor = (Expression)ArrayDefinition.Parse(state, ref position);

                    if (destructor == null)
                        ExceptionHelper.ThrowUnknownToken(code, nameStartPos);

                    containsDestructuringPrms = true;
                }

                var pname = Tools.Unescape(code.Substring(n, position - n), state.strict);
                var reference = new ParameterReference(pname, rest, state.lexicalScopeLevel + 1)
                                    {
                                        Position = n,
                                        Length = position - n
                                    };
                var desc = reference.Descriptor as ParameterDescriptor;

                if (destructor != null)
                    desc.Destructor = new ObjectDesctructor(destructor);

                parameters.Add(desc);

                Tools.SkipSpaces(state.Code, ref position);
                if (arrowWithSunglePrm)
                {
                    position--;
                    break;
                }

                if (code[position] == '=')
                {
                    if (kind == FunctionKind.Arrow)
                        ExceptionHelper.ThrowSyntaxError("Parameters of arrow-function cannot have an initializer", code, position);

                    if (rest)
                        ExceptionHelper.ThrowSyntaxError("Rest parameters can not have an initializer", code, position);
                    do
                        position++;
                    while (Tools.IsWhiteSpace(code[position]));
                    desc.initializer = ExpressionTree.Parse(state, ref position, false, false) as Expression;
                }

                if (code[position] == ',')
                {
                    if (rest)
                        ExceptionHelper.ThrowSyntaxError("Rest parameters must be the last in parameters list", code, position);
                    do
                        position++;
                    while (Tools.IsWhiteSpace(code[position]));
                }
            }

            if (kind == FunctionKind.Setter)
            {
                if (parameters.Count != 1)
                    ExceptionHelper.ThrowSyntaxError("Setter must have only one argument", code, index);
            }

            position++;
            Tools.SkipSpaces(code, ref position);

            if (kind == FunctionKind.Arrow)
            {
                if (!Parser.Validate(code, "=>", ref position))
                    ExceptionHelper.ThrowSyntaxError("Expected \"=>\"", code, position);
                Tools.SkipSpaces(code, ref position);
            }

            if (code[position] != '{')
            {
                var oldFunctionScopeLevel = state.functionScopeLevel;
                state.functionScopeLevel = ++state.lexicalScopeLevel;

                try
                {
                    if (kind == FunctionKind.Arrow)
                    {
                        body = new CodeBlock(new CodeNode[]
                        {
                            new Return(ExpressionTree.Parse(state, ref position, processComma: false) as Expression)
                        })
                        {
                            _variables = new VariableDescriptor[0]
                        };

                        body.Position = body._lines[0].Position;
                        body.Length = body._lines[0].Length;
                    }
                    else
                        ExceptionHelper.ThrowUnknownToken(code, position);
                }
                finally
                {
                    state.functionScopeLevel = oldFunctionScopeLevel;
                    state.lexicalScopeLevel--;
                }
            }
            else
            {
                var oldCodeContext = state.CodeContext;
                state.CodeContext &= ~(CodeContext.InExpression | CodeContext.Conditional | CodeContext.InEval);
                if (kind == FunctionKind.Generator || kind == FunctionKind.MethodGenerator || kind == FunctionKind.AnonymousGenerator)
                    state.CodeContext |= CodeContext.InGenerator;
                else if (kind == FunctionKind.AsyncFunction)
                    state.CodeContext |= CodeContext.InAsync;
                state.CodeContext |= CodeContext.InFunction;

                var labels = state.Labels;
                state.Labels = new List<string>();
                state.AllowReturn++;
                try
                {
                    state.AllowBreak.Push(false);
                    state.AllowContinue.Push(false);
                    state.AllowDirectives = true;
                    body = CodeBlock.Parse(state, ref position) as CodeBlock;
                    if (containsDestructuringPrms)
                    {
                        var destructuringTargets = new List<VariableDescriptor>();
                        var assignments = new List<Expression>();
                        for (var i = 0; i < parameters.Count; i++)
                        {
                            if (parameters[i].Destructor != null)
                            {
                                var targets = parameters[i].Destructor.GetTargetVariables();
                                for (var j = 0; j < targets.Count; j++)
                                {
                                    destructuringTargets.Add(new VariableDescriptor(targets[j].Name, state.functionScopeLevel));
                                }

                                assignments.Add(new Assignment(parameters[i].Destructor, parameters[i].references[0]));
                            }
                        }

                        var newLines = new CodeNode[body._lines.Length + 1];
                        System.Array.Copy(body._lines, 0, newLines, 1, body._lines.Length);
                        newLines[0] = new VariableDefinition(destructuringTargets.ToArray(), assignments.ToArray(), VariableKind.AutoGeneratedParameters);
                        body._lines = newLines;
                    }
                }
                finally
                {
                    state.CodeContext = oldCodeContext;
                    state.AllowBreak.Pop();
                    state.AllowContinue.Pop();
                    state.AllowDirectives = false;
                    state.Labels = labels;
                    state.AllowReturn--;
                }

                if (kind == FunctionKind.Function && string.IsNullOrEmpty(name))
                    kind = FunctionKind.AnonymousFunction;
            }

            if (body._strict || (parameters.Count > 0 && parameters[parameters.Count - 1].IsRest) || kind == FunctionKind.Arrow)
            {
                for (var j = parameters.Count; j-- > 1;)
                    for (var k = j; k-- > 0;)
                        if (parameters[j].Name == parameters[k].Name)
                            ExceptionHelper.ThrowSyntaxError("Duplicate names of function parameters not allowed in strict mode", code, index);

                if (name == "arguments" || name == "eval")
                    ExceptionHelper.ThrowSyntaxError("Functions name can not be \"arguments\" or \"eval\" in strict mode at", code, index);

                for (int j = parameters.Count; j-- > 0;)
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        ExceptionHelper.ThrowSyntaxError("Parameters name cannot be \"arguments\" or \"eval\" in strict mode at", code, parameters[j].references[0].Position, parameters[j].references[0].Length);
                }
            }

            var func = new FunctionDefinition(name)
            {
                parameters = parameters.ToArray(),
                _body = body,
                kind = kind,
                Position = index,
                Length = position - index,
#if DEBUG
                trace = body.directives != null ? body.directives.Contains("debug trace") : false
#endif
            };

            if (!string.IsNullOrEmpty(name))
            {
                func.Reference.ScopeLevel = state.lexicalScopeLevel;
                func.Reference.Position = nameStartPos;
                func.Reference.Length = name.Length;

                func.reference._descriptor.definitionScopeLevel = func.reference.ScopeLevel;
            }

            if (parameters.Count != 0)
            {
                var newVariablesCount = body._variables.Length + parameters.Count;

                for (var j = 0; j < body._variables.Length; j++)
                {
                    for (var k = 0; k < parameters.Count; k++)
                    {
                        if (body._variables[j].name == parameters[k].name)
                        {
                            newVariablesCount--;
                            break;
                        }
                    }
                }

                var newVariables = new VariableDescriptor[newVariablesCount];
                for (var j = 0; j < parameters.Count; j++)
                {
                    newVariables[j] = parameters[parameters.Count - j - 1]; // порядок определяет приоритет
                    for (var k = 0; k < body._variables.Length; k++)
                    {
                        if (body._variables[k] != null && body._variables[k].name == parameters[j].name)
                        {
                            if (body._variables[k].initializer != null)
                                newVariables[j] = body._variables[k];
                            else
                                body._variables[k].lexicalScope = false;
                            body._variables[k] = null;
                            break;
                        }
                    }
                }

                for (int j = 0, k = parameters.Count; j < body._variables.Length; j++)
                {
                    if (body._variables[j] != null)
                        newVariables[k++] = body._variables[j];
                }

                body._variables = newVariables;
            }

            if ((state.CodeContext & CodeContext.InExpression) == 0 && kind == FunctionKind.Function)
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
                var tindex = position;
                while (position < code.Length && Tools.IsWhiteSpace(code[position]) && !Tools.IsLineTerminator(code[position]))
                    position++;

                if (position < code.Length && code[position] == '(')
                {
                    var args = new List<Expression>();
                    position++;
                    for (;;)
                    {
                        while (Tools.IsWhiteSpace(code[position]))
                            position++;
                        if (code[position] == ')')
                            break;
                        else if (code[position] == ',')
                            do
                                position++;
                            while (Tools.IsWhiteSpace(code[position]));
                        args.Add(ExpressionTree.Parse(state, ref position, false, false));
                    }

                    position++;
                    index = position;
                    while (position < code.Length && Tools.IsWhiteSpace(code[position]))
                        position++;

                    if (position < code.Length && code[position] == ';')
                        ExceptionHelper.Throw((new SyntaxError("Expression can not start with word \"function\"")));

                    return new Call(func, args.ToArray());
                }
                else
                    position = tindex;
            }

            if ((state.CodeContext & CodeContext.InExpression) == 0
                && (kind != FunctionKind.Arrow || (state.CodeContext & CodeContext.InEval) == 0))
            {
                if (string.IsNullOrEmpty(name))
                {
                    ExceptionHelper.ThrowSyntaxError("Function must have name", state.Code, index);
                }

                if (state.strict && state.functionScopeLevel != state.lexicalScopeLevel)
                {
                    ExceptionHelper.ThrowSyntaxError("In strict mode code, functions can only be declared at top level or immediately within other function.", state.Code, index);
                }

                state.Variables.Add(func.reference._descriptor);
            }

            index = position;
            return func;
        }

        public override JSValue Evaluate(Context context)
        {
            return MakeFunction(context);
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new CodeNode[1 + parameters.Length + (Reference != null ? 1 : 0)];
            for (var i = 0; i < parameters.Length; i++)
                res[i] = parameters[i].references[0];
            res[parameters.Length] = _body;

            if (Reference != null)
                res[res.Length - 1] = Reference;

            return res;
        }

        /// <summary>
        /// Создаёт функцию, описанную выбранным выражением в контексте указанного сценария.
        /// </summary>
        /// <param name="script">Сценарий, контекст которого будет родительским для контекста выполнения функции.</param>
        /// <returns></returns>
        public Function MakeFunction(Module script)
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
            if (kind == FunctionKind.Generator || kind == FunctionKind.MethodGenerator || kind == FunctionKind.AnonymousGenerator)
                return new GeneratorFunction(context, this);

            if (kind == FunctionKind.AsyncFunction || kind == FunctionKind.AsyncAnonymousFunction || kind == FunctionKind.AsyncArrow)
                return new AsyncFunction(context, this);

            if (_body != null)
            {
                if (_body._lines.Length == 0)
                {
                    return new ConstantFunction(JSValue.notExists, this);
                }
                else if (_body._lines.Length == 1)
                {
                    var ret = _body._lines[0] as Return;
                    if (ret != null
                    && (ret.Value == null || ret.Value.ContextIndependent))
                    {
                        return new ConstantFunction(ret.Value?.Evaluate(null) ?? JSValue.undefined, this);
                    }
                }
            }

            if (!_functionInfo.ContainsArguments
                && !_functionInfo.ContainsRestParameters
                && !_functionInfo.ContainsEval
                && !_functionInfo.ContainsWith)
            {
                return new SimpleFunction(context, this);
            }

            return new Function(context, this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (_body.built)
                return false;

            if (stats != null)
                stats.ContainsInnerEntities = true;

            _codeContext = codeContext;

            if ((codeContext & CodeContext.InLoop) != 0 && message != null)
                message(MessageLevel.Warning, Position, EndPosition - Position, Strings.FunctionInLoop);

            /*
                Если переменная за время построения функции получит хоть одну ссылку плюсом,
                значит её следует пометить захваченной. Для этого необходимо запомнить количество
                ссылок для всех пеменных
            */
            var numbersOfReferences = new Dictionary<string, int>();
            foreach (var variable in variables)
            {
                numbersOfReferences[variable.Key] = variable.Value.references.Count;
            }

            VariableDescriptor descriptorToRestore = null;
            if (!string.IsNullOrEmpty(_name))
            {
                variables.TryGetValue(_name, out descriptorToRestore);
                variables[_name] = reference._descriptor;
            }

            _functionInfo.ContainsRestParameters = parameters.Length > 0 && parameters[parameters.Length - 1].IsRest;

            var bodyCode = _body as CodeNode;
            bodyCode.Build(
                ref bodyCode,
                0,
                variables,
                codeContext & ~(CodeContext.Conditional
                              | CodeContext.InExpression
                              | CodeContext.InEval)
                            | CodeContext.InFunction,
                message,
                _functionInfo,
                opts);
            _body = bodyCode as CodeBlock;

            if (message != null)
            {
                for (var i = parameters.Length; i-- > 0;)
                {
                    if (parameters[i].ReferenceCount == 1)
                        message(MessageLevel.Recomendation, parameters[i].references[0].Position, 0, "Unused parameter \"" + parameters[i].name + "\"");
                    else
                        break;
                }
            }

            _body._suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;
            checkUsings();
            if (stats != null)
            {
                stats.ContainsDebugger |= _functionInfo.ContainsDebugger;
                stats.ContainsEval |= _functionInfo.ContainsEval;
                stats.ContainsInnerEntities = true;
                stats.ContainsTry |= _functionInfo.ContainsTry;
                stats.ContainsWith |= _functionInfo.ContainsWith;
                stats.NeedDecompose |= _functionInfo.NeedDecompose;
                stats.UseCall |= _functionInfo.UseCall;
                stats.UseGetMember |= _functionInfo.UseGetMember;
                stats.ContainsThis |= _functionInfo.ContainsThis;
            }

            if (descriptorToRestore != null)
            {
                variables[descriptorToRestore.name] = descriptorToRestore;
            }
            else if (!string.IsNullOrEmpty(_name))
            {
                variables.Remove(_name);
            }

            foreach (var variable in variables)
            {
                int count = 0;
                if (!numbersOfReferences.TryGetValue(variable.Key, out count) || count != variable.Value.references.Count)
                {
                    variable.Value.captured = true;
                    if ((codeContext & CodeContext.InWith) != 0)
                    {
                        for (var i = count; i < variable.Value.references.Count; i++)
                            variable.Value.references[i].ScopeLevel = -System.Math.Abs(variable.Value.references[i].ScopeLevel);
                    }
                }
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var bd = _body as CodeNode;
            var oldScopeIsolation = _body._suppressScopeIsolation;
            _body._suppressScopeIsolation = SuppressScopeIsolationMode.DoNotSuppress;
            _body.Optimize(ref bd, this, message, opts, _functionInfo);
            _body._suppressScopeIsolation = oldScopeIsolation;

            if (_functionInfo.Returns.Count > 0)
            {
                _functionInfo.ResultType = _functionInfo.Returns[0].ResultType;
                for (var i = 1; i < _functionInfo.Returns.Count; i++)
                {
                    if (_functionInfo.ResultType != _functionInfo.Returns[i].ResultType)
                    {
                        _functionInfo.ResultType = PredictedType.Ambiguous;
                        if (message != null
                            && _functionInfo.ResultType >= PredictedType.Undefined
                            && _functionInfo.Returns[i].ResultType >= PredictedType.Undefined)
                            message(MessageLevel.Warning, parameters[i].references[0].Position, 0, "Type of return value is ambiguous");
                        break;
                    }
                }
            }
            else
                _functionInfo.ResultType = PredictedType.Undefined;
        }

        private void checkUsings()
        {
            if (_body == null
                || _body._lines == null
                || _body._lines.Length == 0)
                return;
            if (_body._variables != null)
            {
                var containsEntities = _functionInfo.ContainsInnerEntities;
                if (!containsEntities)
                {
                    for (var i = 0; !containsEntities && i < _body._variables.Length; i++)
                        containsEntities |= _body._variables[i].initializer != null;
                    _functionInfo.ContainsInnerEntities = containsEntities;
                }
                for (var i = 0; i < _body._variables.Length; i++)
                {
                    _functionInfo.ContainsArguments |= _body._variables[i].name == "arguments";
                }
            }
        }
#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            _body.TryCompile(true, false, null, new List<CodeNode>());
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            CodeNode cn = _body;
            cn.Decompose(ref cn);
            _body = (CodeBlock)cn;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, null, scopeBias);

            var tempVariables = _functionInfo.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            _body.RebuildScope(_functionInfo, tempVariables, scopeBias + (_body._variables == null || _body._variables.Length == 0 || !_functionInfo.WithLexicalEnvironment ? 1 : 0));
            if (tempVariables != null)
            {
                var block = _body as CodeBlock;
                if (block != null)
                {
                    block._variables = tempVariables.Values.Where(x => !(x is ParameterDescriptor)).ToArray();
                }
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        internal string ToString(bool headerOnly)
        {
            StringBuilder code = new StringBuilder();
            switch (kind)
            {
                case FunctionKind.Generator:
                    {
                        code.Append("functions* ");
                        break;
                    }
                case FunctionKind.Method:
                    {
                        break;
                    }
                case FunctionKind.Getter:
                    {
                        code.Append("get ");
                        break;
                    }
                case FunctionKind.Setter:
                    {
                        code.Append("set ");
                        break;
                    }
                case FunctionKind.Arrow:
                    {
                        break;
                    }
                case FunctionKind.AsyncFunction:
                    {
                        code.Append("async ");
                        goto default;
                    }
                default:
                    {
                        code.Append("function ");
                        break;
                    }
            }

            code.Append(_name)
                .Append("(");

            if (parameters != null)
                for (int i = 0; i < parameters.Length;)
                    code.Append(parameters[i])
                        .Append(++i < parameters.Length ? "," : "");

            code.Append(")");

            if (!headerOnly)
            {
                code.Append(" ");
                if (kind == FunctionKind.Arrow)
                    code.Append("=> ");

                if (kind == FunctionKind.Arrow
                    && _body._lines.Length == 1
                    && _body.Position == _body._lines[0].Position)
                    code.Append(_body._lines[0].Childs[0].ToString());
                else
                    code.Append((object)_body ?? "{ [native code] }");
            }

            return code.ToString();
        }
    }
}
