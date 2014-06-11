using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections;
using NiL.JS.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class FunctionStatement : Statement
    {
        [Serializable]
        internal sealed class FunctionReference : VaribleReference
        {
            private FunctionStatement owner;

            public FunctionStatement Owner { get { return owner; } }
            public override VaribleDescriptor Descriptor { get; internal set; }

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
                this.owner = owner;
            }

            public override string ToString()
            {
                return owner.name + ": " + owner;
            }
        }

        [Serializable]
        internal sealed class ParameterReference : VaribleReference
        {
            private string name;

            public override string Name
            {
                get { return name; }
            }

            public override VaribleDescriptor Descriptor
            {
                get;
                internal set;
            }

            internal override JSObject Invoke(Context context)
            {
                return null;
            }

            public ParameterReference(string name)
            {
                this.name = name;
                Descriptor = new VaribleDescriptor(this, true);
            }

            public override string ToString()
            {
                return name;
            }
        }

        private string[] parametersNames;
        private VaribleReference[] parameters;
        private CodeBlock body;
        internal readonly string name;
        internal FunctionType mode;

        public CodeBlock Body { get { return body; } }
        public string Name { get { return name; } }
        public VaribleReference[] Parameters { get { return parameters; } }
        public VaribleReference Reference { get; private set; }

        private FunctionStatement(string name)
        {
            Reference = new FunctionReference(this);
            this.name = name;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionType.Regular);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, FunctionType mode)
        {
            string code = state.Code;
            int i = index;
            switch (mode)
            {
                case FunctionType.Regular:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return new ParseResult();
                        if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
                case FunctionType.Getter:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
                case FunctionType.Setter:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false };
                        break;
                    }
            }
            bool inExp = state.InExpression;
            state.InExpression = false;
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
                    throw new ArgumentException("Invalid char at " + i + ": '" + code[i] + "'");
            }
            else if (mode != FunctionType.Regular)
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
                parameters.Add(new ParameterReference(Tools.Unescape(code.Substring(n, i - n), state.strict.Peek())) { Position = n, Length = i - n });
                while (char.IsWhiteSpace(code[i])) i++;
            }
            switch (mode)
            {
                case FunctionType.Getter:
                    {
                        if (parameters.Count != 0)
                            throw new ArgumentException("getter have many arguments");
                        break;
                    }
                case FunctionType.Setter:
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
            state.AllowReturn++;
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
                state.AllowReturn--;
            }
            if (!inExp)
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
                        Statement = new Operators.Call(new FunctionStatement(name)
                        {
                            parameters = parameters.ToArray(),
                            body = body
                        }, new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = args.ToArray() }))
                    };
                }
                else
                    i = tindex;
            }
            state.InExpression = inExp;
            FunctionStatement res = new FunctionStatement(name)
            {
                parameters = parameters.ToArray(),
                body = body,
                mode = mode,
                Position = index,
                Length = i - index
            };
            if (name != null)
            {
                res.Reference.Position = nameStartPos;
                res.Reference.Length = name.Length;
            }
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = res
            };
        }

        internal override JSObject Invoke(Context context)
        {
            return MakeFunction(context);
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new Statement[1 + parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                res[0] = parameters[i];
            res[parameters.Length] = body;
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
            if (parametersNames == null)
            {
                parametersNames = new string[parameters.Length];
                for (var i = 0; i < parametersNames.Length; i++)
                    parametersNames[i] = parameters[i].Name;
            }
            return new Function(context, body, parametersNames, name, mode);
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            var stat = body as Statement;
            var nvars = new Dictionary<string, VaribleDescriptor>();
            for (var i = 0; i < parameters.Length; i++)
                nvars[parameters[i].Name] = parameters[i].Descriptor;
            body.Optimize(ref stat, 0, nvars);
            body = stat as CodeBlock;
            if (body.varibles != null)
            {
                for (var i = body.varibles.Length; i-->0;)
                {
                    if (!body.varibles[i].Defined)
                    {
                        VaribleDescriptor desc = null;
                        if (varibles.TryGetValue(body.varibles[i].Name, out desc))
                        {
                            foreach (var r in body.varibles[i].References)
                            {
                                desc.references.Add(r);
                                r.Descriptor = desc;
                            }
                            body.varibles[i] = desc;
                        }
                        else varibles.Add(body.varibles[i].Name, body.varibles[i]);
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            var res = mode + " " + name + "(";
            if (parameters != null)
                for (int i = 0; i < parameters.Length; )
                    res += parameters[i] + (++i < parameters.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }
    }
}