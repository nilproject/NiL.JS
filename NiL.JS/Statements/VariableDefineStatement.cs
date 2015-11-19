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
        internal int functionDepth;
        internal VariableDescriptor[] variables;
        internal Expression[] initializers;
        internal readonly string[] names;
        internal readonly bool isConst;

        public bool Const { get { return isConst; } }
        public CodeNode[] Initializers { get { return initializers; } }
        public string[] Names { get { return names; } }

        internal VariableDefineStatement(string name, Expression init, bool isConst, int functionDepth)
        {
            names = new[] { name };
            initializers = new Expression[] { init };
            this.isConst = isConst;
            this.functionDepth = functionDepth;
        }

        private VariableDefineStatement(string[] names, Expression[] initializers, bool isConst, int functionDepth)
        {
            this.initializers = initializers;
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
            var initializers = new List<Expression>();
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

                if (i >= state.Code.Length)
                {
                    initializers.Add(new GetVariableExpression(name, state.scopeDepth) { Position = s, Length = name.Length, defineScopeDepth = state.scopeDepth });
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
                        initializers.Add(new GetVariableExpression(name, state.scopeDepth) { Position = s, Length = name.Length, defineScopeDepth = state.scopeDepth });
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
                    Expression accm = new GetVariableExpression(name, state.scopeDepth) { Position = s, Length = name.Length, defineScopeDepth = state.scopeDepth };
                    Expression source = ExpressionTree.Parse(state, ref i, false, false) as Expression;
                    initializers.Add(new ForceAssignmentOperator(accm, source)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                {
                    initializers.Add(new GetVariableExpression(name, state.scopeDepth) { Position = s, Length = name.Length, defineScopeDepth = state.scopeDepth });
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
            var inits = initializers.ToArray();
            var pos = index;
            index = i;
            return new VariableDefineStatement(names.ToArray(), inits, isConst, state.scopeDepth)
                {
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            int i = 0;
            if (context.abortType >= AbortType.Resume)
            {
                i = (int)context.SuspendData[this];
            }

            for (; i < initializers.Length; i++)
            {
                initializers[i].Evaluate(context);

                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = i;
                    return null;
                }
            }
            return JSValue.notExists;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            res.AddRange(initializers);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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
            for (int i = 0; i < initializers.Length; i++)
            {
                Parser.Build(ref initializers[i], message != null ? 2 : depth, variables, codeContext, message, statistic, opts);
                if (initializers[i] != null)
                    actualChilds++;
            }
            if (this == _this && actualChilds < initializers.Length)
            {
                if ((opts & Options.SuppressUselessStatementsElimination) == 0 && actualChilds == 0)
                {
                    _this = null;
                    Eliminated = true;
                    return false;
                }
                var newinits = new Expression[actualChilds];
                for (int i = 0, j = 0; i < initializers.Length; i++)
                    if (initializers[i] != null)
                        newinits[j++] = initializers[i];
                initializers = newinits;
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            for (int i = 0; i < initializers.Length; i++)
            {
                initializers[i].Optimize(ref initializers[i], owner, message, opts, statistic);
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = isConst ? "const " : "var ";
            for (var i = 0; i < initializers.Length; i++)
            {
                var t = initializers[i].ToString();
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

        internal protected override void Decompose(ref CodeNode self)
        {
            for (var i = 0; i < initializers.Length; i++)
            {
                initializers[i].Decompose(ref initializers[i]);
            }
        }
    }
}