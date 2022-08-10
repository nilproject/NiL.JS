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
    public sealed class DoWhile : CodeNode
    {
        private bool _allowRemove;
        private CodeNode _condition;
        private CodeNode _body;
        private string[] _labels;

        public CodeNode Condition { get { return _condition; } }
        public CodeNode Body { get { return _body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(_labels); } }

        private DoWhile()
        {

        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;

            if (!Parser.Validate(state.Code, "do", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;

            int labelsCount = state.LabelsCount;
            state.LabelsCount = 0;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
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
            if (!(body is CodeBlock) && state.Code[i] == ';')
                i++;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource);
            if (!Parser.Validate(state.Code, "while", ref i))
                ExceptionHelper.Throw((new SyntaxError("Expected \"while\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != '(')
                ExceptionHelper.Throw((new SyntaxError("Expected \"(\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));
            var condition = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            i++;
            var pos = index;
            index = i;
            return new DoWhile()
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
            var frame = ExceptionHelper.GetStackFrame(context, false);

            JSValue checkResult;
            do
            {
                if (context._executionMode != ExecutionMode.Resume || !context.SuspendData.ContainsKey(this))
                {
                    frame.CodeNode = _body;

                    if (context._debugging && !(_body is CodeBlock))
                        context.raiseDebugger(_body);

                    context._lastResult = _body.Evaluate(context) ?? context._lastResult;
                    if (context._executionMode != ExecutionMode.Regular)
                    {
                        if (context._executionMode < ExecutionMode.Return)
                        {
                            var me = context._executionInfo == null || System.Array.IndexOf(_labels, context._executionInfo._oValue as string) != -1;
                            var _break = (context._executionMode > ExecutionMode.Continue) || !me;
                            if (me)
                            {
                                context._executionMode = ExecutionMode.Regular;
                                context._executionInfo = JSValue.notExists;
                            }
                            if (_break)
                                return null;
                        }
                        else
                            return null;
                    }
                }

                frame.CodeNode = _condition;

                if (context._debugging)
                    context.raiseDebugger(_condition);

                checkResult = _condition.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = null;
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
            Parser.Build(ref _body, expressionDepth, variables, codeContext | CodeContext.InLoop, message, stats, opts);
            Parser.Build(ref _condition, 2, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, stats, opts);
            try
            {
                if (_allowRemove
                    && (opts & Options.SuppressUselessStatementsElimination) == 0
                    && (_condition is Constant || (_condition as Expressions.Expression).ContextIndependent))
                {
                    if ((bool)_condition.Evaluate(null))
                        _this = new InfinityLoop(_body, _labels);
                    else if (_labels.Length == 0)
                        _this = _body;
                    _condition.Eliminated = true;
                }
            }

#if (PORTABLE || NETCORE)
            catch
            {
#else
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(10, "Error", e.Message);
#endif
            }
            if (_this == this && _body == null)
                _body = new Empty();
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            _condition.Optimize(ref _condition, owner, message, opts, stats);
            _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
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

        public override string ToString()
        {
            return "do" + (_body is CodeBlock ? _body + " " : Environment.NewLine + "  " + _body + ";" + Environment.NewLine) + "while (" + _condition + ")";
        }
    }
}