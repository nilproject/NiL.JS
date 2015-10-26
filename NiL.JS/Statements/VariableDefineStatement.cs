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

            internal override bool ResultInTempContainer
            {
                get { return false; }
            }

            internal AllowWriteCN(VariableReference variable, Expression source)
            {
                this.variable = variable;
                this.source = source;
            }

            public override JSValue Evaluate(Context context)
            {
                var res = source.Evaluate(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSValueAttributesInternal.ReadOnly;
                return res;
            }

            internal protected override JSValue EvaluateForWrite(Context context)
            {
                var res = source.EvaluateForWrite(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSValueAttributesInternal.ReadOnly;
                return res;
            }

            internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
            {
                var v = variable as CodeNode;
                var res = variable.Build(ref v, depth, variables, state, message, statistic, opts);
                variable = v as VariableReference;
                return res;
            }

            internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            bool isConst = false;
            if (!Parser.Validate(state.Code, "var ", ref i)
                && !(isConst = Parser.Validate(state.Code, "const ", ref i)))
                return null;
            bool isDef = false;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var initializator = new List<CodeNode>();
            var names = new List<string>();
            while ((state.Code[i] != ';') && (state.Code[i] != '}') && !Tools.isLineTerminator(state.Code[i]))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                {
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                        ExceptionsHelper.Throw((new SyntaxError('\"' + Tools.Unescape(state.Code.Substring(s, i - s), state.strict) + "\" is a reserved word at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                    ExceptionsHelper.Throw((new SyntaxError("Invalid variable definition at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }
                string name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                if (state.strict)
                {
                    if (name == "arguments" || name == "eval")
                        ExceptionsHelper.ThrowSyntaxError("Varible name cannot be \"arguments\" or \"eval\" in strict mode", state.Code, s, i - s);
                }
                names.Add(name);
                isDef = true;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i]))
                    i++;
                if (i < state.Code.Length && (state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    ExceptionsHelper.Throw((new SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 1))));
                if (i >= state.Code.Length)
                {
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, defineDepth = state.functionsDepth });
                    break;
                }
                if (Tools.isLineTerminator(state.Code[i]))
                {
                    s = i;
                    do
                        i++;
                    while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i >= state.Code.Length)
                    {
                        initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, defineDepth = state.functionsDepth });
                        break;
                    }
                    if (state.Code[i] != '=')
                        i = s;
                }
                if (state.Code[i] == '=')
                {
                    do
                        i++;
                    while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i == state.Code.Length)
                        ExceptionsHelper.ThrowSyntaxError("Unexpected end of line in variable definition.", state.Code, i);
                    VariableReference accm = new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, defineDepth = state.functionsDepth };
                    Expression source = ExpressionTree.Parse(state, ref i, false, false) as Expression;
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
                    //    ExceptionsHelper.Throw(new SyntaxError("Constant must contain value at " + CodeCoordinates.FromTextPosition(state.Code, i)));
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, defineDepth = state.functionsDepth });
                }
                if (i >= state.Code.Length)
                    break;
                s = i;
                if ((state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    ExceptionsHelper.Throw(new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, i, 0)));
                while (s < state.Code.Length && char.IsWhiteSpace(state.Code[s]))
                    s++;
                if (s >= state.Code.Length)
                    break;
                if (state.Code[s] == ',')
                {
                    i = s;
                    do
                        i++;
                    while (char.IsWhiteSpace(state.Code[i]));
                }
                else
                    while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i]))
                        i++;
            }
            if (!isDef)
                throw new ArgumentException("code (" + i + ")");
            var inits = initializator.ToArray();
            var pos = index;
            index = i;
            return new VariableDefineStatement(names.ToArray(), inits, isConst, state.functionsDepth)
                {
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
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

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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