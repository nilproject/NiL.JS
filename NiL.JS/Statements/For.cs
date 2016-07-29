using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class For : CodeNode
    {
        private sealed class PerIterationScopeInitializer : CodeNode
        {
            private VariableDescriptor[] _variables;

            public PerIterationScopeInitializer(VariableDescriptor[] variables)
            {
                _variables = variables;
            }

            public override void Decompose(ref CodeNode self)
            {

            }

            public override JSValue Evaluate(Context context)
            {
                if (_variables != null)
                {
                    for (var i = 0; i < _variables.Length; i++)
                    {
                        if (_variables[i].captured)
                            context.DefineVariable(_variables[i].name).Assign(_variables[i].cacheRes.CloneImpl());
                    }
                }

                return null;
            }

            public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
            {

            }
        }

        private CodeNode _initializer;
        private CodeNode _condition;
        private CodeNode _post;
        private CodeNode _body;
        private string[] labels;

        public CodeNode Initializer { get { return _initializer; } }
        public CodeNode Condition { get { return _condition; } }
        public CodeNode Post { get { return _post; } }
        public CodeNode Body { get { return _body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        private For()
        {
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            CodeNode init = null;
            CodeNode body = null;
            CodeNode condition = null;
            CodeNode post = null;
            CodeNode result = null;

            var labelsCount = state.LabelsCount;
            var oldVariablesCount = state.Variables.Count;
            state.LabelsCount = 0;
            state.lexicalScopeLevel++;
            try
            {
                init = VariableDefinition.Parse(state, ref i, true);
                if (init == null)
                    init = ExpressionTree.Parse(state, ref i, forForLoop: true);
                if ((init is ExpressionTree)
                    && (init as ExpressionTree).Type == OperationType.None
                    && (init as ExpressionTree).second == null)
                    init = (init as ExpressionTree).first;
                if (state.Code[i] != ';')
                    ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
                condition = state.Code[i] == ';' ? null as CodeNode : ExpressionTree.Parse(state, ref i, forForLoop: true);
                if (state.Code[i] != ';')
                    ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
                post = state.Code[i] == ')' ? null as CodeNode : ExpressionTree.Parse(state, ref i, forForLoop: true);
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != ')')
                    ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));

                i++;
                Tools.SkipSpaces(state.Code, ref i);

                state.AllowBreak.Push(true);
                state.AllowContinue.Push(true);
                try
                {
                    body = Parser.Parse(state, ref i, 0);
                    var vds = body as VariableDefinition;
                    if (vds != null)
                    {
                        if (vds.Kind >= VariableKind.ConstantInLexicalScope)
                        {
                            ExceptionsHelper.ThrowSyntaxError("Block scope variables can not be declared in for-loop directly", state.Code, body.Position);
                        }

                        if (state.message != null)
                            state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare variables in for-loop directly");
                    }
                }
                finally
                {
                    state.AllowBreak.Pop();
                    state.AllowContinue.Pop();
                }

                int startPos = index;
                index = i;

                result = new For()
                {
                    _body = body,
                    _condition = condition,
                    _initializer = init,
                    _post = post,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = startPos,
                    Length = index - startPos
                };

                var vars = CodeBlock.extractVariables(state, oldVariablesCount);
                result = new CodeBlock(new[] { result }) { _variables = vars, Position = result.Position, Length = result.Length };
            }
            finally
            {
                state.lexicalScopeLevel--;
            }

            return result;
        }

        public override JSValue Evaluate(Context context)
        {
            if (_initializer != null && (context.executionMode != AbortReason.Resume || context.SuspendData[this] == _initializer))
            {
                if (context.executionMode != AbortReason.Resume && context.debugging)
                    context.raiseDebugger(_initializer);

                _initializer.Evaluate(context);
                if (context.executionMode == AbortReason.Suspend)
                {
                    context.SuspendData[this] = _initializer;
                    return null;
                }
            }

            var be = _body != null;
            var pe = _post != null;
            var @continue = false;

            if (context.executionMode != AbortReason.Resume || context.SuspendData[this] == _condition)
            {
                if (context.executionMode != AbortReason.Resume && context.debugging)
                    context.raiseDebugger(_condition);

                @continue = (bool)_condition.Evaluate(context);
                if (context.executionMode == AbortReason.Suspend)
                {
                    context.SuspendData[this] = _condition;
                    return null;
                }

                if (!@continue)
                    return null;
            }

            do
            {
                if (be && (context.executionMode != AbortReason.Resume || context.SuspendData[this] == _body))
                {
                    if (context.executionMode != AbortReason.Resume && context.debugging && !(_body is CodeBlock))
                        context.raiseDebugger(_body);

                    var temp = _body.Evaluate(context);
                    if (temp != null)
                        context.lastResult = temp;

                    if (context.executionMode != AbortReason.None)
                    {
                        if (context.executionMode < AbortReason.Return)
                        {
                            var me = context.executionInfo == null || System.Array.IndexOf(labels, context.executionInfo.oValue as string) != -1;
                            var _break = (context.executionMode > AbortReason.Continue) || !me;
                            if (me)
                            {
                                context.executionMode = AbortReason.None;
                                context.executionInfo = null;
                            }

                            if (_break)
                                return null;
                        }
                        else if (context.executionMode == AbortReason.Suspend)
                        {
                            context.SuspendData[this] = _body;
                            return null;
                        }
                        else
                            return null;
                    }
                }

                if (pe && (context.executionMode != AbortReason.Resume || context.SuspendData[this] == _post))
                {
                    if (context.executionMode != AbortReason.Resume && context.debugging)
                        context.raiseDebugger(_post);

                    _post.Evaluate(context);
                }

                if (context.executionMode != AbortReason.Resume && context.debugging)
                    context.raiseDebugger(_condition);

                @continue = (bool)_condition.Evaluate(context);
                if (context.executionMode == AbortReason.Suspend)
                {
                    context.SuspendData[this] = _condition;
                    return null;
                }
            }
            while (@continue);

            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                _initializer,
                _condition,
                _post,
                _body
            };

            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref _initializer, 1, variables, codeContext, message, stats, opts);
            var initAsVds = _initializer as VariableDefinition;

            if ((opts & Options.SuppressUselessStatementsElimination) == 0)
            {
                if (initAsVds != null && initAsVds.initializers.Length == 1 && initAsVds.Kind == VariableKind.FunctionScope)
                    _initializer = initAsVds.initializers[0];
            }

            Parser.Build(ref _condition, 2, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, stats, opts);

            if (_post != null)
            {
                Parser.Build(ref _post, 1, variables, codeContext | CodeContext.Conditional | CodeContext.InLoop | CodeContext.InExpression, message, stats, opts);
                if (_post == null && message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Last expression of for-loop was removed. Maybe, it's a mistake.");
            }

            Parser.Build(ref _body, System.Math.Max(1, expressionDepth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, stats, opts);

            if (initAsVds != null && initAsVds.Kind != VariableKind.FunctionScope)
            {
                var bodyAsCodeBlock = _body as CodeBlock;
                if (bodyAsCodeBlock != null)
                {
                    var newLines = new CodeNode[bodyAsCodeBlock._lines.Length + 1];
                    System.Array.Copy(bodyAsCodeBlock._lines, newLines, bodyAsCodeBlock._lines.Length);
                    newLines[newLines.Length - 1] = new PerIterationScopeInitializer(initAsVds.variables);
                    bodyAsCodeBlock._lines = newLines;
                }
                else
                {
                    _body = bodyAsCodeBlock = new CodeBlock(new[] { _body, new PerIterationScopeInitializer(initAsVds.variables) });
                }

                bodyAsCodeBlock.suppressScopeIsolation = SuppressScopeIsolationMode.DoNotSuppress;

                for (var i = 0; i < initAsVds.variables.Length; i++)
                {
                    if (initAsVds.variables[i].captured)
                        initAsVds.variables[i].definitionScopeLevel = -1;
                }
            }

            if (_condition == null)
            {
                _condition = new Constant(BaseLibrary.Boolean.True);
            }
            else if ((_condition is Expression)
                  && (_condition as Expression).ContextIndependent
                  && !(bool)_condition.Evaluate(null))
            {
                _this = _initializer;
                return false;
            }
            else if (_body == null || _body is Empty)
            {
                VariableReference variable = null;
                Constant limit = null;
                if (_condition is Less)
                {
                    variable = (_condition as Less).FirstOperand as VariableReference;
                    limit = (_condition as Less).SecondOperand as Constant;
                }
                else if (_condition is More)
                {
                    variable = (_condition as More).SecondOperand as VariableReference;
                    limit = (_condition as More).FirstOperand as Constant;
                }
                else if (_condition is NotEqual)
                {
                    variable = (_condition as Less).SecondOperand as VariableReference;
                    limit = (_condition as Less).FirstOperand as Constant;
                    if (variable == null && limit == null)
                    {
                        variable = (_condition as Less).FirstOperand as VariableReference;
                        limit = (_condition as Less).SecondOperand as Constant;
                    }
                }
                if (variable != null
                    && limit != null
                    && _post is Increment
                    && ((_post as Increment).FirstOperand as VariableReference)._descriptor == variable._descriptor)
                {
                    if (variable.ScopeLevel >= 0 && variable._descriptor.definitionScopeLevel >= 0)
                    {
                        if (_initializer is Assignment
                            && (_initializer as Assignment).FirstOperand is GetVariable
                            && ((_initializer as Assignment).FirstOperand as GetVariable)._descriptor == variable._descriptor)
                        {
                            var value = (_initializer as Assignment).SecondOperand;
                            if (value is Constant)
                            {
                                var vvalue = value.Evaluate(null);
                                var lvalue = limit.Evaluate(null);
                                if ((vvalue.valueType == JSValueType.Integer
                                    || vvalue.valueType == JSValueType.Boolean
                                    || vvalue.valueType == JSValueType.Double)
                                    && (lvalue.valueType == JSValueType.Integer
                                    || lvalue.valueType == JSValueType.Boolean
                                    || lvalue.valueType == JSValueType.Double))
                                {
                                    _post.Eliminated = true;
                                    _condition.Eliminated = true;

                                    if (!Less.Check(vvalue, lvalue))
                                    {

                                        _this = _initializer;
                                        return false;
                                    }

                                    _this = new CodeBlock(new[] { _initializer, new Assignment(variable, limit) });
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (_initializer != null)
                _initializer.Optimize(ref _initializer, owner, message, opts, stats);
            if (_condition != null)
                _condition.Optimize(ref _condition, owner, message, opts, stats);
            if (_post != null)
                _post.Optimize(ref _post, owner, message, opts, stats);
            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref CodeNode self)
        {
            _initializer?.Decompose(ref _initializer);
            _condition?.Decompose(ref _condition);
            _body?.Decompose(ref _body);
            _post?.Decompose(ref _post);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _initializer?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _condition?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _body?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _post?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            var istring = (_initializer as object ?? "").ToString();
            return "for (" + istring + "; " + _condition + "; " + _post + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}