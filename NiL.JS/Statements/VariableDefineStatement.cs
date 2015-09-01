using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class VariableDefineStatement : CodeNode
    {
        internal sealed class AllowWriteCN : Expression
        {
            internal VariableReference variable;
            internal readonly CodeNode source;

            protected internal override bool ResultInTempContainer
            {
                get { return false; }
            }

            internal AllowWriteCN(VariableReference variable, Expression source)
            {
                this.variable = variable;
                this.source = source;
            }

            internal override JSValue Evaluate(Context context)
            {
                var res = source.Evaluate(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSValueAttributesInternal.ReadOnly;
                return res;
            }

            internal override JSValue EvaluateForAssing(Context context)
            {
                var res = source.EvaluateForAssing(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSValueAttributesInternal.ReadOnly;
                return res;
            }

            internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
            {
                var v = variable as CodeNode;
                var res = variable.Build(ref v, depth, variables, state, message, statistic, opts);
                variable = v as VariableReference;
                return res;
            }

            internal override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
            {
                var v = variable as CodeNode;
                variable.Optimize(ref v, owner, message, opts, statistic);
                variable = v as VariableReference;
            }

            public override T Visit<T>(Visitor<T> visitor)
            {
                return visitor.Visit(variable);
            }

            protected override CodeNode[] getChildsImpl()
            {
                return variable.Childs;
            }

            public override string ToString()
            {
                return source.ToString();
            }
        }
        internal int functionDepth;
        internal VariableDescriptor[] variables;
        internal CodeNode[] initializators;
        internal readonly string[] names;
        internal readonly bool isConst;

        public bool Const { get { return isConst; } }
        public CodeNode[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        internal VariableDefineStatement(string name, CodeNode init, bool isConst, int functionDepth)
        {
            names = new[] { name };
            initializators = new[] { init };
            this.isConst = isConst;
            this.functionDepth = functionDepth;
        }

        private VariableDefineStatement(string[] names, CodeNode[] initializators, bool isConst, int functionDepth)
        {
            this.initializators = initializators;
            this.names = names;
            this.isConst = isConst;
            this.functionDepth = functionDepth;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            bool isConst = false;
            if (!Parser.Validate(state.Code, "var ", ref i)
                && !(isConst = Parser.Validate(state.Code, "const ", ref i)))
                return new ParseResult();
            bool isDef = false;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var initializator = new List<CodeNode>();
            var names = new List<string>();
            while ((state.Code[i] != ';') && (state.Code[i] != '}') && !Tools.isLineTerminator(state.Code[i]))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                {
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict.Peek()))
                        throw new JSException((new SyntaxError('\"' + Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek()) + "\" is a reserved word at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                    throw new JSException((new SyntaxError("Invalid variable definition at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }
                string name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (name == "arguments" || name == "eval")
                        throw new JSException((new SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }
                names.Add(name);
                isDef = true;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
                if (i < state.Code.Length && (state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    throw new JSException((new SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 1))));
                if (i >= state.Code.Length)
                {
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                    break;
                }
                if (Tools.isLineTerminator(state.Code[i]))
                {
                    s = i;
                    do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i >= state.Code.Length)
                    {
                        initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                        break;
                    }
                    if (state.Code[i] != '=')
                        i = s;
                }
                if (state.Code[i] == '=')
                {
                    do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i == state.Code.Length)
                        throw new JSException((new SyntaxError("Unexpected end of line in variable defenition.")));
                    VariableReference accm = new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth };
                    Expression source = ExpressionTree.Parse(state, ref i, false).Statement as Expression;
                    if (isConst)
                        source = new AllowWriteCN(accm, source);
                    initializator.Add(
                        new AssignmentOperator(
                            accm,
                            source)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                {
                    //if (isConst)
                    //    throw new JSException(new SyntaxError("Constant must contain value at " + CodeCoordinates.FromTextPosition(state.Code, i)));
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                }
                if (i >= state.Code.Length)
                    break;
                s = i;
                if ((state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    throw new JSException(new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, i, 0)));
                while (s < state.Code.Length && char.IsWhiteSpace(state.Code[s])) s++;
                if (s >= state.Code.Length)
                    break;
                if (state.Code[s] == ',')
                {
                    i = s;
                    do i++; while (char.IsWhiteSpace(state.Code[i]));
                }
                else
                    while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
            }
            if (!isDef)
                throw new ArgumentException("code (" + i + ")");
            var inits = initializator.ToArray();
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new VariableDefineStatement(names.ToArray(), inits, isConst, state.functionsDepth)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSValue Evaluate(Context context)
        {
            for (int i = 0; i < initializators.Length; i++)
            {
                initializators[i].Evaluate(context);
                if (isConst)
                    (this.variables[i].cacheRes ?? this.variables[i].Get(context, false, this.variables[i].defineDepth)).attributes |= JSValueAttributesInternal.ReadOnly;
            }
            return JSValue.notExists;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            res.AddRange(initializators);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            this.variables = new VariableDescriptor[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                VariableDescriptor desc = null;
                if (!variables.TryGetValue(names[i], out desc))
                    variables[names[i]] = desc = new VariableDescriptor(names[i], functionDepth);
                else
                {
                    if (message != null)
                    {
                        if (desc.isDefined)
                            message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Variable redefinition. \"" + names[i] + "\" already defined.");
                        else
                        {
                            if (desc.references != null && desc.references.Count > 0)
                            {
                                var use = desc.references[0];
                                for (var j = desc.references.Count; j-- > 1; )
                                    if (desc.references[j].Position < use.Position)
                                        use = desc.references[j];
                                if (use.Position < Position)
                                    message(MessageLevel.Warning, new CodeCoordinates(0, use.Position, use.Length), "Variable \"" + use.Name + "\" used before definition.");
                            }
                        }
                    }
                }
                this.variables[i] = desc;
                this.variables[i].isDefined = true;
                this.variables[i].isReadOnly = isConst;
            }
            int actualChilds = 0;
            for (int i = 0; i < initializators.Length; i++)
            {
                Parser.Build(ref initializators[i], message != null ? 2 : depth, variables, state, message, statistic, opts);
                if (initializators[i] != null)
                    actualChilds++;
            }
            if (this == _this && actualChilds < initializators.Length)
            {
                if ((opts & Options.SuppressUselessStatementsElimination) == 0 && actualChilds == 0)
                {
                    _this = null;
                    Eliminated = true;
                    return false;
                }
                var newinits = new CodeNode[actualChilds];
                for (int i = 0, j = 0; i < initializators.Length; i++)
                    if (initializators[i] != null)
                        newinits[j++] = initializators[i];
                initializators = newinits;
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            for (int i = 0; i < initializators.Length; i++)
            {
                initializators[i].Optimize(ref initializators[i], owner, message, opts, statistic);
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = isConst ? "const " : "var ";
            for (var i = 0; i < initializators.Length; i++)
            {
                var t = initializators[i].ToString();
                if (string.IsNullOrEmpty(t))
                    continue;
                if (t[0] == '(')
                    t = t.Substring(1, t.Length - 2);
                if (i > 0)
                    res += ", ";
                res += t;
            }
            return res;
        }
    }
}