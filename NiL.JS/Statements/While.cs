using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class While : CodeNode
    {
        private bool _allowRemove;
        private CodeNode _condition;
        private CodeNode _body;
        private string[] _labels;

        public CodeNode Condition { get { return _condition; } }
        public CodeNode Body { get { return _body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(_labels); } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "while (", ref i) && !Parser.Validate(state.Code, "while(", ref i))
                return null;
            int labelsCount = state.LabelsCount;
            state.LabelsCount = 0;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            var condition = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionHelper.Throw(new SyntaxError(Strings.UnexpectedEndOfSource));
            if (state.Code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do
                i++;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]));
            if (i >= state.Code.Length)
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource);
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.ContiniesCount;
            int cbs = state.BreaksCount;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionDefinition)
            {
                if (state.Message != null)
                    state.Message(MessageLevel.CriticalWarning, body.Position, body.Length, Strings.DoNotDeclareFunctionInNestedBlocks);
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции,
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            var pos = index;
            index = i;
            return new While()
            {
                _allowRemove = ccs == state.ContiniesCount && cbs == state.BreaksCount,
                _body = body,
                _condition = condition,
                _labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            bool be = _body != null;
            JSValue checkResult;

            var frame = ExceptionHelper.GetStackFrame(context, false);

            if (context._executionMode != ExecutionMode.Resume || context.SuspendData[this] == _condition)
            {
                frame.CodeNode = _condition;

                if (context._executionMode != ExecutionMode.Resume && context._debugging)
                    context.raiseDebugger(_condition);

                checkResult = _condition.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = _condition;
                    return null;
                }
                if (!(bool)checkResult)
                    return null;
            }

            do
            {
                if (be
                 && (context._executionMode != ExecutionMode.Resume
                    || context.SuspendData[this] == _body))
                {
                    frame.CodeNode = _body;

                    if (context._executionMode != ExecutionMode.Resume && context._debugging && !(_body is CodeBlock))
                        context.raiseDebugger(_body);

                    var temp = _body.Evaluate(context);
                    if (temp != null)
                        context._lastResult = temp;
                    if (context._executionMode != ExecutionMode.Regular)
                    {
                        if (context._executionMode < ExecutionMode.Return)
                        {
                            var me = context._executionInfo == null || System.Array.IndexOf(_labels, context._executionInfo._oValue as string) != -1;
                            var _break = (context._executionMode > ExecutionMode.Continue) || !me;
                            if (me)
                            {
                                context._executionMode = ExecutionMode.Regular;
                                context._executionInfo = null;
                            }
                            if (_break)
                                return null;
                        }
                        else if (context._executionMode == ExecutionMode.Suspend)
                        {
                            context.SuspendData[this] = _body;
                            return null;
                        }
                        else
                            return null;
                    }
                }

                if (context._executionMode != ExecutionMode.Resume && context._debugging)
                    context.raiseDebugger(_condition);

                checkResult = _condition.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = _condition;
                    return null;
                }
            }
            while ((bool)checkResult);
            return null;
        }

        protected internal override CodeNode[] GetChildrenImpl()
        {
            var res = new List<CodeNode>()
            {
                _body,
                _condition
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            expressionDepth = System.Math.Max(1, expressionDepth);
            Parser.Build(ref _body, expressionDepth, variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, stats, opts);
            Parser.Build(ref _condition, 2, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, stats, opts);
            if ((opts & Options.SuppressUselessStatementsElimination) == 0 && _condition is ConvertToBoolean)
            {
                if (message != null)
                    message(MessageLevel.Warning, _condition.Position, 2, "Useless conversion. Remove double negation in condition");
                _condition = (_condition as Expression)._left;
            }

            if (_allowRemove && (_condition is Constant || (_condition is Expression && (_condition as Expression).ContextIndependent)))
            {
                Eliminated = true;
                if ((bool)_condition.Evaluate(null))
                {
                    if ((opts & Options.SuppressUselessStatementsElimination) == 0 && _body != null)
                        _this = new InfinityLoop(_body, _labels);
                }
                else if ((opts & Options.SuppressUselessStatementsElimination) == 0)
                {
                    _this = null;
                    if (_body != null)
                        _body.Eliminated = true;
                }
                _condition.Eliminated = true;
            }
            else
            {
                if ((opts & Options.SuppressUselessStatementsElimination) == 0
                    && ((_condition is ObjectDefinition && (_condition as ObjectDefinition).Properties.Length == 0)
                        || (_condition is ArrayDefinition)))
                {
                    _this = new InfinityLoop(_body, _labels);
                    _condition.Eliminated = true;
                }
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (_condition != null)
                _condition.Optimize(ref _condition, owner, message, opts, stats);
            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override void Decompose(ref CodeNode self)
        {
            if (_condition != null)
                _condition.Decompose(ref _condition);
            if (_body != null)
                _body.Decompose(ref _body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _condition?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _body?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "while (" + _condition + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}