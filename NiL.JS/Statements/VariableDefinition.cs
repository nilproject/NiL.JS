using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum VariableKind
    {
        FunctionScope,
        LexicalScope,
        ConstantInLexicalScope
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class VariableDefinition : CodeNode
    {
        private readonly int scopeLevel;
        private VariableKind mode;

        internal readonly VariableDescriptor[] variables;
        internal Expression[] initializers;

        public CodeNode[] Initializers { get { return initializers; } }
        public VariableDescriptor[] Variables { get { return variables; } }
        public VariableKind Kind { get { return mode; } }

        private VariableDefinition(VariableDescriptor[] variables, Expression[] initializers, int scopeDepth, VariableKind mode)
        {
            this.initializers = initializers;
            this.variables = variables;
            this.scopeLevel = scopeDepth;
            this.mode = mode;
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, false);
        }

        internal static CodeNode Parse(ParseInfo state, ref int index, bool forForLoop)
        {
            int position = index;
            Tools.SkipSpaces(state.Code, ref position);

            var mode = VariableKind.FunctionScope;
            if (Parser.Validate(state.Code, "var ", ref position))
                mode = VariableKind.FunctionScope;
            else if (Parser.Validate(state.Code, "let ", ref position))
                mode = VariableKind.LexicalScope;
            else if (Parser.Validate(state.Code, "const ", ref position))
                mode = VariableKind.ConstantInLexicalScope;
            else
                return null;

            var level = mode == 0 ? state.functionScopeLevel : state.lexicalScopeLevel;
            var initializers = new List<Expression>();
            var names = new List<string>();
            int s = position;
            while ((state.Code[position] != ';') && (state.Code[position] != '}') && !Tools.IsLineTerminator(state.Code[position]))
            {
                Tools.SkipSpaces(state.Code, ref position);

                s = position;
                if (!Parser.ValidateName(state.Code, ref position, state.strict))
                {
                    if (Parser.ValidateName(state.Code, ref position, false, true, state.strict))
                        ExceptionHelper.Throw((new SyntaxError('\"' + Tools.Unescape(state.Code.Substring(s, position - s), state.strict) + "\" is a reserved word at " + CodeCoordinates.FromTextPosition(state.Code, s, position - s))));
                    ExceptionHelper.Throw((new SyntaxError("Invalid variable definition at " + CodeCoordinates.FromTextPosition(state.Code, s, position - s))));
                }

                string name = Tools.Unescape(state.Code.Substring(s, position - s), state.strict);
                if (state.strict)
                {
                    if (name == "arguments" || name == "eval")
                        ExceptionHelper.ThrowSyntaxError("Varible name cannot be \"arguments\" or \"eval\" in strict mode", state.Code, s, position - s);
                }
                names.Add(name);

                position = s;
                var expression = ExpressionTree.Parse(state, ref position, processComma: false, forForLoop: forForLoop);

                if (!(expression is VariableReference))
                {
                    bool valid = false;
                    var et = expression as ExpressionTree;
                    if (et != null)
                    {
                        if (et.Type == OperationType.None && et.second == null)
                            et = et.first as ExpressionTree;
                        valid |= et != null && et.Type == OperationType.Assignment;
                    }
                    else
                    {
                        var cnst = expression as Constant;
                        valid = cnst != null && cnst.value == JSValue.undefined;
                    }

                    if (!valid)
                        ExceptionHelper.ThrowSyntaxError("Invalid variable initializer", state.Code, position);
                }

                initializers.Add(expression);

                s = position;

                if (position >= state.Code.Length)
                    break;

                Tools.SkipSpaces(state.Code, ref s);
                if (s >= state.Code.Length)
                    break;
                if (state.Code[s] == ',')
                {
                    position = s + 1;
                    Tools.SkipSpaces(state.Code, ref position);
                }
                else
                    break;
            }

            if (names.Count == 0)
                throw new InvalidOperationException("code (" + position + ")");

            if (!forForLoop && position < state.Code.Length && state.Code[position] == ';')
                position++;
            else
                position = s;

            var variables = new VariableDescriptor[names.Count];
            for (int i = 0, skiped = 0; i < names.Count; i++)
            {
                bool skip = false;
                for (var j = 0; j < state.Variables.Count - i + skiped; j++)
                {
                    if (state.Variables[j].name == names[i] && state.Variables[j].definitionScopeLevel >= level)
                    {
                        if (state.Variables[j].lexicalScope && mode > VariableKind.FunctionScope)
                            ExceptionHelper.ThrowSyntaxError(string.Format(Strings.IdentifierAlreadyDeclared, names[i]), state.Code, index);

                        skip = true;
                        variables[i] = state.Variables[j];
                        skiped++;
                        break;
                    }
                }

                if (skip)
                    continue;

                variables[i] = new VariableDescriptor(names[i], level)
                {
                    lexicalScope = mode > VariableKind.FunctionScope,
                    isReadOnly = mode == VariableKind.ConstantInLexicalScope
                };

                state.Variables.Add(variables[i]);
            }

            var inits = initializers.ToArray();
            var pos = index;
            index = position;
            return new VariableDefinition(variables, inits, level, mode)
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            int i = 0;
            if (context._executionMode >= ExecutionMode.Resume)
            {
                i = (int)context.SuspendData[this];
            }

            for (; i < initializers.Length; i++)
            {
                if (context._executionMode == ExecutionMode.None && mode > VariableKind.FunctionScope && variables[i].lexicalScope)
                {
                    JSValue f = context.DefineVariable(variables[i].name, false);

                    variables[i].cacheRes = f;
                    variables[i].cacheContext = context;

                    if (mode == VariableKind.ConstantInLexicalScope)
                        f._attributes |= JSValueAttributesInternal.ReadOnly;
                }

                initializers[i].Evaluate(context);

                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = i;
                    return null;
                }
            }
            return JSValue.notExists;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>();
            res.AddRange(initializers);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (mode > VariableKind.FunctionScope)
                stats.WithLexicalEnvironment = true;

            int actualChilds = 0;
            for (int i = 0; i < initializers.Length; i++)
            {
                Parser.Build(ref initializers[i], message != null ? 2 : expressionDepth, variables, codeContext, message, stats, opts);
                if (initializers[i] != null)
                {
                    actualChilds++;

                    if (mode == VariableKind.ConstantInLexicalScope && initializers[i] is Assignment)
                    {
                        initializers[i] = new ForceAssignmentOperator(initializers[i].first, initializers[i].second)
                        {
                            Position = initializers[i].Position,
                            Length = initializers[i].Length
                        };
                    }
                }
            }

            if (actualChilds < initializers.Length)
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

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            for (int i = 0; i < initializers.Length; i++)
            {
                initializers[i].Optimize(ref initializers[i], owner, message, opts, stats);
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = "var ";
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

        public override void Decompose(ref CodeNode self)
        {
            for (var i = 0; i < initializers.Length; i++)
            {
                initializers[i].Decompose(ref initializers[i]);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            for (var i = 0; i < initializers.Length; i++)
            {
                initializers[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }
    }
}