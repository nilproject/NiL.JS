using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Extensions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
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
#if (PORTABLE || NETCORE)
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
            Tools.SkipSpaces(state.Code, ref i);

            if (!Parser.Validate(state.Code, "for(", ref i)
            && (!Parser.Validate(state.Code, "for (", ref i)))
                return null;

            Tools.SkipSpaces(state.Code, ref i);

            var result = new ForOf()
            {
                _labels = state.Labels.GetRange(state.Labels.Count - state.LabelsCount, state.LabelsCount).ToArray()
            };

            VariableDescriptor[] vars = null;
            var oldVariablesCount = state.Variables.Count;
            state.LexicalScopeLevel++;
            try
            {
                var vStart = i;
                var variableName = "";
                var variableNameStart = 0;
                var variableDef = VariableDefinition.Parse(state, ref i, true);
                if (variableDef == null)
                {
                    if (state.Code[i] == ';')
                        return null;

                    Tools.SkipSpaces(state.Code, ref i);

                    variableNameStart = i;
                    if (!Parser.ValidateName(state.Code, ref i, state.Strict))
                        return null;

                    variableName = Tools.Unescape(state.Code.Substring(variableNameStart, i - variableNameStart), state.Strict);                    

                    variableDef = new Variable(variableName, state.LexicalScopeLevel)
                    {
                        Position = variableNameStart,
                        Length = i - variableNameStart,
                        ScopeLevel = state.LexicalScopeLevel
                    };

                    Tools.SkipSpaces(state.Code, ref i);

                    if (state.Code[i] == '=')
                    {
                        Tools.SkipSpaces(state.Code, ref i);

                        var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, true);
                        if (defVal == null)
                            return defVal;

                        Expression exp = new AssignmentOperatorCache(variableDef as Variable);
                        exp = new Assignment(exp, defVal)
                        {
                            Position = exp.Position,
                            Length = defVal.EndPosition - exp.Position
                        };

                        if (variableDef == exp._left._left)
                            variableDef = exp;

                        Tools.SkipSpaces(state.Code, ref i);
                    }
                }
                else
                {
                    variableName = (variableDef as VariableDefinition)._variables[0].name;
                }

                if (!Parser.Validate(state.Code, "of", ref i))
                {
                    if (oldVariablesCount < state.Variables.Count)
                        state.Variables.RemoveRange(oldVariablesCount, state.Variables.Count - oldVariablesCount);

                    return null;
                }

                if (state.Strict)
                {
                    if (variableName == "arguments" || variableName == "eval")
                        ExceptionHelper.ThrowSyntaxError(
                            "Parameters name may not be \"arguments\" or \"eval\" in strict mode at ", 
                            state.Code, 
                            variableDef.Position, 
                            variableDef.Length);
                }

                if (variableDef is VariableDefinition)
                {
                    if ((variableDef as VariableDefinition)._variables.Length > 1)
                        ExceptionHelper.ThrowSyntaxError("Too many variables in for-of loop", state.Code, i);
                }


                result._variable = variableDef;

                state.LabelsCount = 0;
                Tools.SkipSpaces(state.Code, ref i);

                result._source = Parser.Parse(state, ref i, CodeFragmentType.Expression);
                Tools.SkipSpaces(state.Code, ref i);

                if (state.Code[i] != ')')
                    ExceptionHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));

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
                state.LexicalScopeLevel--;
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
            var frame = ExceptionHelper.GetStackFrame(context, false);

            SuspendData suspendData = null;
            if (context._executionMode >= ExecutionMode.Resume)
            {
                suspendData = context.SuspendData[this] as SuspendData;
            }

            JSValue source = null;
            if (suspendData == null || suspendData.source == null)
            {
                if (context._debugging && !(_source is CodeBlock))
                    context.raiseDebugger(_source);

                frame.CodeNode = _source;

                source = _source.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
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
                if (context._debugging && _variable is not CodeBlock)
                    context.raiseDebugger(_variable);

                frame.CodeNode = _variable;

                var varialeDefStat = _variable as VariableDefinition;
                if (varialeDefStat != null)
                {
                    _variable.Evaluate(context);
                    variable = (varialeDefStat._initializers[0]._left ?? varialeDefStat._initializers[0]).EvaluateForWrite(context);
                }
                else if (_variable is Assignment)
                {
                    _variable.Evaluate(context);
                    variable = (_variable as Assignment)._left.Evaluate(context);
                }
                else
                    variable = _variable.EvaluateForWrite(context);

                if (context._executionMode == ExecutionMode.Suspend)
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

            IIteratorResult iteratorResult = context._executionMode != ExecutionMode.Resume ? iterator.next() : null;
            while (context._executionMode >= ExecutionMode.Resume || !iteratorResult.done)
            {
                if (context._executionMode != ExecutionMode.Resume)
                {
                    var tempAttr = variable._attributes;
                    variable._attributes &= ~JSValueAttributesInternal.ReadOnly;
                    variable.Assign(iteratorResult.value);
                    variable._attributes = tempAttr;
                }

                frame.CodeNode = _body;

                _body.Evaluate(context);

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
                    else if (context._executionMode == ExecutionMode.Suspend)
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

        protected internal override CodeNode[] GetChildrenImpl()
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref _variable, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _source, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _body, System.Math.Max(1, expressionDepth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, stats, opts);

            if (_variable is Comma)
            {
                if ((_variable as Comma).RightOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-of");

                _variable = (_variable as Comma).LeftOperand;
            }

            if (message != null
                && (_source is ObjectDefinition
                || _source is ArrayDefinition
                || _source is Constant))
                message(MessageLevel.Recomendation, Position, Length, "for-of with constant source. This solution has a performance penalty. Rewrite without using for..in.");

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
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