using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
            this.lexicalScope = true;
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
            get { return _descriptor.name; }
        }

        internal ParameterReference(string name, bool rest, int depth)
        {
            _descriptor = new ParameterDescriptor(name, rest, depth);
            _descriptor.references.Add(this);
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

        internal readonly FunctionInfo _functionInfo;
        internal ParameterDescriptor[] parameters;
        internal CodeBlock body;
        internal FunctionKind kind;
#if DEBUG
        internal bool trace;
#endif

        public CodeBlock Body { get { return body; } }
        public ReadOnlyCollection<ParameterDescriptor> Parameters { get { return new ReadOnlyCollection<ParameterDescriptor>(parameters); } }

        protected internal override bool NeedDecompose
        {
            get
            {
                return _functionInfo.ContainsYield;
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
                return true;
            }
        }

        public FunctionKind Kind
        {
            get
            {
                return kind;
            }
        }

        internal FunctionDefinition(string name)
            : base(name)
        {
            parameters = new ParameterDescriptor[0];
            body = new CodeBlock(new CodeNode[0]);
            body._variables = new VariableDescriptor[0];
            _functionInfo = new FunctionInfo();
        }

        internal static CodeNode ParseFunction(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, FunctionKind.Function);
        }

        internal static CodeNode ParseArrow(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, FunctionKind.Arrow);
        }

        internal static Expression Parse(ParseInfo state, ref int index, FunctionKind kind)
        {
            string code = state.Code;
            int i = index;
            switch (kind)
            {
                case FunctionKind.AnonymousFunction:
                case FunctionKind.AnonymousGenerator:
                    {
                        break;
                    }
                case FunctionKind.Function:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return null;
                        if (code[i] == '*')
                        {
                            kind = FunctionKind.Generator;
                            i++;
                        }
                        else if ((code[i] != '(') && (!Tools.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionKind.Getter:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return null;
                        if ((!Tools.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionKind.Setter:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return null;
                        if ((!Tools.IsWhiteSpace(code[i])))
                            return null;
                        break;
                    }
                case FunctionKind.MethodGenerator:
                case FunctionKind.Method:
                    {
                        if (code[i] == '*')
                        {
                            kind = FunctionKind.MethodGenerator;
                            i++;
                        }
                        else if (kind == FunctionKind.MethodGenerator)
                            throw new ArgumentException("mode");
                        break;
                    }
                case FunctionKind.Arrow:
                    {
                        break;
                    }
                default:
                    throw new NotImplementedException(kind.ToString());
            }

            Tools.SkipSpaces(state.Code, ref i);

            var parameters = new List<ParameterDescriptor>();
            CodeBlock body = null;
            string name = null;
            bool arrowWithSunglePrm = false;
            int nameStartPos = 0;

            if (kind != FunctionKind.Arrow)
            {
                if (code[i] != '(')
                {
                    nameStartPos = i;
                    if (Parser.ValidateName(code, ref i, false, true, state.strict))
                        name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict);
                    else if ((kind == FunctionKind.Getter || kind == FunctionKind.Setter) && Parser.ValidateString(code, ref i, false))
                        name = Tools.Unescape(code.Substring(nameStartPos + 1, i - nameStartPos - 2), state.strict);
                    else if ((kind == FunctionKind.Getter || kind == FunctionKind.Setter) && Parser.ValidateNumber(code, ref i))
                        name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict);
                    else
                        ExceptionsHelper.ThrowSyntaxError("Invalid function name", code, nameStartPos, i - nameStartPos);

                    Tools.SkipSpaces(code, ref i);

                    if (code[i] != '(')
                        ExceptionsHelper.ThrowUnknownToken(code, i);
                }
                else if (kind == FunctionKind.Getter || kind == FunctionKind.Setter)
                    ExceptionsHelper.ThrowSyntaxError("Getter and Setter must have name", code, index);
                else if (kind == FunctionKind.Method || kind == FunctionKind.MethodGenerator)
                    ExceptionsHelper.ThrowSyntaxError("Method must have name", code, index);
            }
            else if (code[i] != '(')
            {
                arrowWithSunglePrm = true;
                i--;
            }
            do
                i++;
            while (Tools.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                ExceptionsHelper.ThrowSyntaxError(Strings.UnexpectedToken, code, i);
            while (code[i] != ')')
            {
                if (parameters.Count == 255 || (kind == FunctionKind.Setter && parameters.Count == 1) || kind == FunctionKind.Getter)
                    ExceptionsHelper.ThrowSyntaxError(string.Format(Strings.TooManyArgumentsForFunction, name), code, index);

                bool rest = Parser.Validate(code, "...", ref i);

                int n = i;
                if (!Parser.ValidateName(code, ref i, state.strict))
                    ExceptionsHelper.ThrowUnknownToken(code, nameStartPos);

                var pname = Tools.Unescape(code.Substring(n, i - n), state.strict);
                var desc = new ParameterReference(pname, rest, state.lexicalScopeLevel + 1)
                {
                    Position = n,
                    Length = i - n
                }.Descriptor as ParameterDescriptor;
                parameters.Add(desc);

                Tools.SkipSpaces(state.Code, ref i);
                if (arrowWithSunglePrm)
                {
                    i--;
                    break;
                }
                if (code[i] == '=')
                {
                    if (kind == FunctionKind.Arrow)
                        ExceptionsHelper.ThrowSyntaxError("Parameters of arrow-function cannot have an initializer", code, i);

                    if (rest)
                        ExceptionsHelper.ThrowSyntaxError("Rest parameters can not have an initializer", code, i);
                    do
                        i++;
                    while (Tools.IsWhiteSpace(code[i]));
                    desc.initializer = ExpressionTree.Parse(state, ref i, false, false) as Expression;
                }
                if (code[i] == ',')
                {
                    if (rest)
                        ExceptionsHelper.ThrowSyntaxError("Rest parameters must be the last in parameters list", code, i);
                    do
                        i++;
                    while (Tools.IsWhiteSpace(code[i]));
                }
            }

            switch (kind)
            {
                case FunctionKind.Setter:
                    {
                        if (parameters.Count != 1)
                            ExceptionsHelper.ThrowSyntaxError("Setter must have only one argument", code, index);
                        break;
                    }
            }

            i++;
            Tools.SkipSpaces(code, ref i);

            if (kind == FunctionKind.Arrow)
            {
                if (!Parser.Validate(code, "=>", ref i))
                    ExceptionsHelper.ThrowSyntaxError("Expected \"=>\"", code, i);
                Tools.SkipSpaces(code, ref i);
            }

            if (code[i] != '{')
            {
                var oldFunctionScopeLevel = state.functionScopeLevel;
                state.functionScopeLevel = ++state.lexicalScopeLevel;

                try
                {
                    if (kind == FunctionKind.Arrow)
                    {
                        body = new CodeBlock(new CodeNode[]
                        {
                            new Return(ExpressionTree.Parse(state, ref i, processComma: false) as Expression)
                        })
                        {
                            _variables = new VariableDescriptor[0]
                        };

                        body.Position = body._lines[0].Position;
                        body.Length = body._lines[0].Length;
                    }
                    else
                        ExceptionsHelper.ThrowUnknownToken(code, i);
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
                if (kind == FunctionKind.Generator || kind == FunctionKind.MethodGenerator || kind == FunctionKind.AnonymousGenerator)
                    state.CodeContext |= CodeContext.InGenerator;
                state.CodeContext |= CodeContext.InFunction;
                state.CodeContext &= ~(CodeContext.InExpression | CodeContext.Conditional | CodeContext.InEval | CodeContext.InWith);
                var labels = state.Labels;
                state.Labels = new List<string>();
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
                            ExceptionsHelper.ThrowSyntaxError("Duplicate names of function parameters not allowed in strict mode", code, index);
                if (name == "arguments" || name == "eval")
                    ExceptionsHelper.ThrowSyntaxError("Functions name can not be \"arguments\" or \"eval\" in strict mode at", code, index);
                for (int j = parameters.Count; j-- > 0;)
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        ExceptionsHelper.ThrowSyntaxError("Parameters name cannot be \"arguments\" or \"eval\" in strict mode at", code, parameters[j].references[0].Position, parameters[j].references[0].Length);
                }
            }
            FunctionDefinition func = new FunctionDefinition(name)
            {
                parameters = parameters.ToArray(),
                body = body,
                kind = kind,
                Position = index,
                Length = i - index,
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
                var tindex = i;
                while (i < code.Length && Tools.IsWhiteSpace(code[i]) && !Tools.IsLineTerminator(code[i]))
                    i++;
                if (i < code.Length && code[i] == '(')
                {
                    var args = new List<Expression>();
                    i++;
                    for (;;)
                    {
                        while (Tools.IsWhiteSpace(code[i]))
                            i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do
                                i++;
                            while (Tools.IsWhiteSpace(code[i]));
                        args.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                    }
                    i++;
                    index = i;
                    while (i < code.Length && Tools.IsWhiteSpace(code[i]))
                        i++;
                    if (i < code.Length && code[i] == ';')
                        ExceptionsHelper.Throw((new SyntaxError("Expression can not start with word \"function\"")));
                    return new Call(func, args.ToArray());
                }
                else
                    i = tindex;
            }
            if ((state.CodeContext & CodeContext.InExpression) == 0)
            {
                if (string.IsNullOrEmpty(name))
                {
                    ExceptionsHelper.ThrowSyntaxError("Function must have name", state.Code, index);
                }
                if (state.strict && state.functionScopeLevel != state.lexicalScopeLevel)
                {
                    ExceptionsHelper.ThrowSyntaxError("In strict mode code, functions can only be declared at top level or immediately within other function.", state.Code, index);
                }

                state.Variables.Add(func.reference._descriptor);
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

            return new Function(context, this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (body.built)
                return false;

            if (stats != null)
                stats.ContainsInnerEntities = true;

            _codeContext = codeContext;

            if ((codeContext & CodeContext.InLoop) != 0 && message != null)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, EndPosition - Position), Strings.FunctionInLoop);

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

            var bodyCode = body as CodeNode;
            _functionInfo.ContainsRestParameters = parameters.Length > 0 && parameters[parameters.Length - 1].IsRest;
            bodyCode.Build(ref bodyCode, 0, variables, codeContext & ~(CodeContext.Conditional | CodeContext.InExpression | CodeContext.InEval | CodeContext.InWith) | CodeContext.InFunction, message, _functionInfo, opts);
            if (message != null)
            {
                for (var i = parameters.Length; i-- > 0;)
                {
                    if (parameters[i].ReferenceCount == 1)
                        message(MessageLevel.Recomendation, new CodeCoordinates(0, parameters[i].references[0].Position, 0), "Unused variable \"" + parameters[i].name + "\"");
                    else
                        break;
                }
            }
            body = bodyCode as CodeBlock;
            body.suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;
            checkUsings();
            if (stats != null)
            {
                stats.ContainsDebugger |= this._functionInfo.ContainsDebugger;
                stats.ContainsEval |= this._functionInfo.ContainsEval;
                stats.ContainsInnerEntities = true;
                stats.ContainsTry |= this._functionInfo.ContainsTry;
                stats.ContainsWith |= this._functionInfo.ContainsWith;
                stats.ContainsYield |= this._functionInfo.ContainsYield;
                stats.UseCall |= this._functionInfo.UseCall;
                stats.UseGetMember |= this._functionInfo.UseGetMember;
                stats.ContainsThis |= this._functionInfo.ContainsThis;
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

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var bd = body as CodeNode;
            body.Optimize(ref bd, this, message, opts, this._functionInfo);
            if (this._functionInfo.Returns.Count > 0)
            {
                this._functionInfo.ResultType = this._functionInfo.Returns[0].ResultType;
                for (var i = 1; i < this._functionInfo.Returns.Count; i++)
                {
                    if (this._functionInfo.ResultType != this._functionInfo.Returns[i].ResultType)
                    {
                        this._functionInfo.ResultType = PredictedType.Ambiguous;
                        if (message != null
                            && this._functionInfo.ResultType >= PredictedType.Undefined
                            && this._functionInfo.Returns[i].ResultType >= PredictedType.Undefined)
                            message(MessageLevel.Warning, new CodeCoordinates(0, parameters[i].references[0].Position, 0), "Type of return value is ambiguous");
                        break;
                    }
                }
            }
            else
                this._functionInfo.ResultType = PredictedType.Undefined;
        }

        private void checkUsings()
        {
            if (body == null
                || body._lines == null
                || body._lines.Length == 0)
                return;
            if (body._variables != null)
            {
                var containsEntities = _functionInfo.ContainsInnerEntities;
                if (!containsEntities)
                {
                    for (var i = 0; !containsEntities && i < body._variables.Length; i++)
                        containsEntities |= body._variables[i].initializer != null;
                    _functionInfo.ContainsInnerEntities = containsEntities;
                }
                for (var i = 0; i < body._variables.Length; i++)
                {
                    _functionInfo.ContainsArguments |= body._variables[i].name == "arguments";
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

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            CodeNode cn = body;
            cn.Decompose(ref cn);
            body = (CodeBlock)cn;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, null, scopeBias);

            var tv = _functionInfo.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(_functionInfo, tv, scopeBias + (body._variables == null || body._variables.Length == 0 || !_functionInfo.WithLexicalEnvironment ? 1 : 0));
            if (tv != null)
            {
                var vars = new List<VariableDescriptor>(tv.Values);
                vars.RemoveAll(x => x is ParameterDescriptor);
                body._variables = vars.ToArray();
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
                if (kind == FunctionKind.Arrow)
                    code.Append(" => ");

                if (kind == FunctionKind.Arrow
                    && body._lines.Length == 1
                    && body.Position == body._lines[0].Position)
                    code.Append(body._lines[0].Childs[0].ToString());
                else
                    code.Append((object)body ?? "{ [native code] }");
            }

            return code.ToString();
        }
    }
}
