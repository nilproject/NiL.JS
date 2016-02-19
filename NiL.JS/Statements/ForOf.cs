using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Extensions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ForOf : CodeNode
    {
        private sealed class SuspendData
        {
            public JSValue source;
            public JSValue variable;
            public IIterator iterator;
        }

        private CodeNode _variable;
        private CodeNode _source;
        private CodeNode _body;
        private string[] _labels;

        public CodeNode Variable { get { return _variable; } }
        public CodeNode Source { get { return _source; } }
        public CodeNode Body { get { return _body; } }
        public ReadOnlyCollection<string> Labels
        {
            get
            {
#if PORTABLE
                return new ReadOnlyCollection<string>(_labels);
#else
                return System.Array.AsReadOnly(_labels);
#endif
            }
        }

        private ForOf()
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

            var result = new ForOf()
            {
                _labels = state.Labels.GetRange(state.Labels.Count - state.LabelsCount, state.LabelsCount).ToArray()
            };

            VariableDescriptor[] vars = null;
            var oldVariablesCount = state.Variables.Count;
            state.lexicalScopeLevel++;
            try
            {
                var vStart = i;
                if ((result._variable = VariableDefinition.Parse(state, ref i, true)) != null)
                {
                }
                else
                {
                    if (state.Code[i] == ';')
                        return null;
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;
                    int start = i;
                    string varName;
                    if (!Parser.ValidateName(state.Code, ref i, state.strict))
                        return null;
                    varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                    if (state.strict)
                    {
                        if (varName == "arguments" || varName == "eval")
                            ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                    }
                    result._variable = new GetVariable(varName, state.lexicalScopeLevel) { Position = start, Length = i - start, ScopeLevel = state.lexicalScopeLevel };
                }
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] == '=')
                {
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));
                    var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, true);
                    if (defVal == null)
                        return defVal;
                    Expression exp = new AssignmentOperatorCache(result._variable as GetVariable ?? (result._variable as VariableDefinition).initializers[0] as GetVariable);
                    exp = new Assignment(
                        exp,
                        (Expression)defVal)
                    {
                        Position = exp.Position,
                        Length = defVal.EndPosition - exp.Position
                    };
                    if (result._variable == exp.first.first)
                        result._variable = exp;
                    else
                        (result._variable as VariableDefinition).initializers[0] = exp;
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;

                }
                if (!Parser.Validate(state.Code, "of", ref i))
                {
                    if (oldVariablesCount < state.Variables.Count)
                        state.Variables.RemoveRange(oldVariablesCount, state.Variables.Count - oldVariablesCount);
                    return null;
                }

                state.LabelsCount = 0;
                Tools.SkipSpaces(state.Code, ref i);

                if (result._variable is VariableDefinition)
                {
                    if ((result._variable as VariableDefinition).variables.Length > 1)
                        ExceptionsHelper.ThrowSyntaxError("Too many variables in for-of loop", state.Code, i);
                }

                result._source = Parser.Parse(state, ref i, CodeFragmentType.Expression);
                Tools.SkipSpaces(state.Code, ref i);

                if (state.Code[i] != ')')
                    ExceptionsHelper.Throw(new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0)));

                i++;
                state.AllowBreak.Push(true);
                state.AllowContinue.Push(true);
                result._body = Parser.Parse(state, ref i, 0);
                state.AllowBreak.Pop();
                state.AllowContinue.Pop();
                result.Position = index;
                result.Length = i - index;
                index = i;
                vars = CodeBlock.extractVariables(state, oldVariablesCount);
            }
            finally
            {
                state.lexicalScopeLevel--;
            }

            return new CodeBlock(new[] { result })
            {
                _variables = vars,
                Position = result.Position,
                Length = result.Length
            };
        }

        public override JSValue Evaluate(Context context)
        {
            SuspendData suspendData = null;
            if (context.executionMode >= AbortReason.Resume)
            {
                suspendData = context.SuspendData[this] as SuspendData;
            }

            JSValue source = null;
            if (suspendData == null || suspendData.source == null)
            {
#if DEV
                if (context.debugging && !(_source is CodeBlock))
                    context.raiseDebugger(_source);
#endif
                source = _source.Evaluate(context);
                if (context.executionMode == AbortReason.Suspend)
                {
                    context.SuspendData[this] = null;
                    return null;
                }
            }
            else
                source = suspendData.source;

            JSValue variable = null;
            if (suspendData == null || suspendData.variable == null)
            {
#if DEV
                if (context.debugging && !(_variable is CodeBlock))
                    context.raiseDebugger(_variable);
#endif
                var varialeDefStat = _variable as VariableDefinition;
                if (varialeDefStat != null)
                {
                    _variable.Evaluate(context);
                    variable = (varialeDefStat.initializers[0].first ?? varialeDefStat.initializers[0]).EvaluateForWrite(context);
                }
                else if (_variable is Assignment)
                {
                    _variable.Evaluate(context);
                    variable = (_variable as Assignment).first.Evaluate(context);
                }
                else
                    variable = _variable.EvaluateForWrite(context);
                if (context.executionMode == AbortReason.Suspend)
                {
                    if (suspendData == null)
                        suspendData = new SuspendData();
                    context.SuspendData[this] = suspendData;
                    suspendData.source = source;
                    return null;
                }
            }
            else
                variable = suspendData.variable;

            if (!source.Defined || source.IsNull || _body == null)
                return null;

            var iterator = (suspendData != null ? suspendData.iterator : null) ?? source.AsIterable().iterator();
            if (iterator == null)
                return null;

            IIteratorResult iteratorResult = context.executionMode != AbortReason.Resume ? iterator.next() : null;
            while (context.executionMode >= AbortReason.Resume || !iteratorResult.done)
            {
                if (context.executionMode != AbortReason.Resume)
                    variable.Assign(iteratorResult.value);
                _body.Evaluate(context);

                if (context.executionMode != AbortReason.None)
                {
                    if (context.executionMode < AbortReason.Return)
                    {
                        var me = context.executionInfo == null || System.Array.IndexOf(_labels, context.executionInfo.oValue as string) != -1;
                        var _break = (context.executionMode > AbortReason.Continue) || !me;
                        if (me)
                        {
                            context.executionMode = AbortReason.None;
                            context.executionInfo = JSValue.notExists;
                        }
                        if (_break)
                            return null;
                    }
                    else if (context.executionMode == AbortReason.Suspend)
                    {
                        if (suspendData == null)
                            suspendData = new SuspendData();
                        context.SuspendData[this] = suspendData;

                        suspendData.source = source;
                        suspendData.variable = variable;
                        suspendData.iterator = iterator;
                        return null;
                    }
                    else
                        return null;
                }

                iteratorResult = iterator.next();
            }

            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>
            {
                _body,
                _variable,
                _source
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref _variable, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _source, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _body, System.Math.Max(1, expressionDepth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, stats, opts);
            if (_variable is Expressions.Comma)
            {
                if ((_variable as Expressions.Comma).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                _variable = (_variable as Expressions.Comma).FirstOperand;
            }
            if (message != null
                && (this._source is Expressions.ObjectDefinition
                || this._source is ArrayDefinition
                || this._source is Constant))
                message(MessageLevel.Recomendation, new CodeCoordinates(0, Position, Length), "for..in with constant source. This reduce performance. Rewrite without using for..in.");
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            _variable.Optimize(ref _variable, owner, message, opts, stats);
            _source.Optimize(ref _source, owner, message, opts, stats);
            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref CodeNode self)
        {
            _variable.Decompose(ref _variable);
            _source.Decompose(ref _source);
            _body?.Decompose(ref _body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _variable.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _source.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _body?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            return "for (" + _variable + " of " + _source + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}