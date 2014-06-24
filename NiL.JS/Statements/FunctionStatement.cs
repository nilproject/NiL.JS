using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class FunctionStatement : Statement
    {
        [Serializable]
        internal sealed class FunctionReference : VariableReference
        {
            private FunctionStatement owner;

            public FunctionStatement Owner { get { return owner; } }
            public override VariableDescriptor Descriptor { get; internal set; }

            public override string Name
            {
                get { return owner.name; }
            }

            internal override JSObject Invoke(Context context)
            {
                return owner.Invoke(context);
            }

            public FunctionReference(FunctionStatement owner)
            {
                functionDepth = -1;
                this.owner = owner;
            }

            public override string ToString()
            {
                return owner.name + ": " + owner;
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

            public override VariableDescriptor Descriptor
            {
                get;
                internal set;
            }

            internal override JSObject Invoke(Context context)
            {
                return null;
            }

            public ParameterReference(string name, int fdepth)
            {
                functionDepth = fdepth;
                this.name = name;
                Descriptor = new VariableDescriptor(this, true, fdepth);
            }

            public override string ToString()
            {
                return name;
            }
        }

        internal VariableReference[] parameters;
        internal CodeBlock body;
        internal bool recursive;
        internal readonly string name;
        internal FunctionType type;

        public CodeBlock Body { get { return body; } }
        public string Name { get { return name; } }
        public VariableReference[] Parameters { get { return parameters; } }
        public VariableReference Reference { get; private set; }

        internal FunctionStatement(string name)
        {
            Reference = new FunctionReference(this);
            parameters = new VariableReference[0];
            body = new CodeBlock(new Statement[0], false);
            body.variables = new VariableDescriptor[0];
            this.name = name;
        }

        internal static FunctionStatement Parse(string code)
        {
            int index = 0;
            return Parse(new ParsingState(Tools.RemoveComments(code), code), ref index).Statement as FunctionStatement;
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
                        if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
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
                if (!Parser.ValidateName(code, ref i, false, true, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid function name at " + Tools.PositionToTextcord(code, nameStartPos))));
                name = Tools.Unescape(code.Substring(nameStartPos, i - nameStartPos), state.strict.Peek());
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new JSException(new SyntaxError("Unexpected char at " + Tools.PositionToTextcord(code, i)));
            }
            else if (mode != FunctionType.Function)
                throw new ArgumentException("Getters and Setters must have name");
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new ArgumentException("code (" + i + ")");
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid description of function arguments at " + Tools.PositionToTextcord(code, nameStartPos))));
                var pname = Tools.Unescape(code.Substring(n, i - n), state.strict.Peek());
                parameters.Add(new ParameterReference(pname, state.functionsDepth + 1) { Position = n, Length = i - n });
                while (char.IsWhiteSpace(code[i])) i++;
            }
            switch (mode)
            {
                case FunctionType.Get:
                    {
                        if (parameters.Count != 0)
                            throw new ArgumentException("getter have many arguments");
                        break;
                    }
                case FunctionType.Set:
                    {
                        if (parameters.Count != 1)
                            throw new ArgumentException("setter have invalid arguments");
                        break;
                    }
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            var labels = state.Labels;
            state.Labels = new List<string>();
            state.functionsDepth++;
            CodeBlock body = null;
            try
            {
                state.AllowStrict = true;
                body = CodeBlock.Parse(state, ref i).Statement as CodeBlock;
            }
            finally
            {
                state.AllowStrict = false;
                state.Labels = labels;
                state.functionsDepth--;
            }
            if (body.strict)
            {
                for (var j = parameters.Count; j-- > 1; )
                    for (var k = j; k-- > 0; )
                        if (parameters[j].Name == parameters[k].Name)
                            throw new JSException(new SyntaxError("Duplicate names of function parameters not allowed in strict mode."));
                if (name == "arguments" || name == "eval")
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Functions name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, index))));
                for (int j = parameters.Count; j-- > 0; )
                {
                    if (parameters[j].Name == "arguments" || parameters[j].Name == "eval")
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, index))));
                }
            }
            FunctionStatement func = new FunctionStatement(name)
            {
                parameters = parameters.ToArray(),
                body = body,
                type = mode,
                Position = index,
                Length = i - index
            };
            if (inExp == 0 && mode == FunctionType.Function) // Позволяет делать вызов сразу при объявлении функции 
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
                    List<Statement> args = new List<Statement>();
                    i++;
                    for (; ; )
                    {
                        while (char.IsWhiteSpace(code[i])) i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do i++; while (char.IsWhiteSpace(code[i]));
                        args.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                    }
                    i++;
                    index = i;
                    while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
                    if (i < code.Length && code[i] == ';')
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Expression can not start with word \"function\"")));
                    return new ParseResult()
                    {
                        IsParsed = true,
                        Statement = new Operators.Call(func, new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = args.ToArray() }))
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

        internal override JSObject Invoke(Context context)
        {
            return MakeFunction(context);
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new Statement[1 + parameters.Length + (Reference != null ? 1 : 0)];
            for (var i = 0; i < parameters.Length; i++)
                res[i] = parameters[i];
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
            return new Function(context, this);
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            var stat = body as Statement;
            var nvars = new Dictionary<string, VariableDescriptor>();
            for (var i = 0; i < parameters.Length; i++)
            {
                nvars[parameters[i].Name] = parameters[i].Descriptor;
                parameters[i].Descriptor.Owner = this;
                parameters[i].Descriptor.defineDepth = fdepth + 1;
            }
            VariableDescriptor fdesc = null;
            if (type == FunctionType.Function && !string.IsNullOrEmpty(name))
            {
                if (!variables.TryGetValue(name, out fdesc) // Если там не определена
                    || !fdesc.references.Contains(Reference)) // или определена, но не эта. Для примера: var f = function f1(){ return 1 }; function f1(){ return 2 };
                    fdesc = new VariableDescriptor(Reference, true, fdepth + 1); // то создаём новый дескриптор
                nvars[name] = fdesc;
            }
            nvars["arguments"] = new VariableDescriptor("arguments", fdepth + 1) { Owner = this };
            stat.Optimize(ref stat, 0, fdepth + 1, nvars, strict);
            if (fdesc != null)
            {
                fdesc.Owner = null; /* Тело функции, увидев здесь null, говорит, что оно хозяйко этого определения. 
                                     * При таком раскладе функция появится только когда она сама вызовется, что в корне не верно.
                                     */
            }
            body = stat as CodeBlock;
            if (body.variables != null)
            {
                for (var i = body.variables.Length; i-- > 0; )
                {
                    if (!body.variables[i].Defined)
                    // все необъявленные переменные нужно прокинуть вниз для того,
                    // чтобы во всех местах их использования был один дескриптор и один кеш
                    {
                        VariableDescriptor desc = null;
                        if (variables.TryGetValue(body.variables[i].Name, out desc))
                        {
                            foreach (var r in body.variables[i].References)
                            {
                                desc.references.Add(r);
                                r.Descriptor = desc;
                            }
                            body.variables[i] = desc;
                        }
                        else variables.Add(body.variables[i].Name, body.variables[i]);
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            var res = ((FunctionType)((int)type & 3)).ToString().ToLowerInvariant() + " " + name + "(";
            if (parameters != null)
                for (int i = 0; i < parameters.Length; )
                    res += parameters[i] + (++i < parameters.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }
    }
}