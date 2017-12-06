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
    public sealed class ForIn : CodeNode
    {
        private sealed class SuspendData
        {
            public JSValue source;
            public JSValue variable;
            public HashSet<string> processedKeys;
            public IEnumerator<KeyValuePair<string, JSValue>> keys;
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
                return _labels.AsReadOnly<string>();
#elif NETCORE
                return new ReadOnlyCollection<string>(_labels);
#else
                return System.Array.AsReadOnly(_labels);
#endif
            }
        }

        private ForIn()
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

            var result = new ForIn()
            {
                _labels = state.Labels.GetRange(state.Labels.Count - state.LabelsCount, state.LabelsCount).ToArray()
            };

            VariableDescriptor[] vars = null;
            var oldVariablesCount = state.Variables.Count;
            state.lexicalScopeLevel++;
            try
            {
                var vStart = i;
                result._variable = VariableDefinition.Parse(state, ref i, true);
                if (result._variable == null)
                {
                    if (state.Code[i] == ';')
                        return null;

                    Tools.SkipSpaces(state.Code, ref i);

                    int start = i;
                    string varName;
                    if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    {
                        if (Parser.ValidateValue(state.Code, ref i))
                        {
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            if (Parser.Validate(state.Code, "in", ref i))
                                ExceptionHelper.ThrowSyntaxError("Invalid accumulator name at ", state.Code, start, i - start);
                            else
                                return null;
                        }
                        else
                            return null;
                    }

                    varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                    if (state.strict)
                    {
                        if (varName == "arguments" || varName == "eval")
                            ExceptionHelper.ThrowSyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at ", state.Code, start, i - start);
                    }

                    result._variable = new Variable(varName, state.lexicalScopeLevel) { Position = start, Length = i - start, ScopeLevel = state.lexicalScopeLevel };

                    Tools.SkipSpaces(state.Code, ref i);

                    if (state.Code[i] == '=')
                    {
                        i++;
                        Tools.SkipSpaces(state.Code, ref i);

                        var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, true);
                        if (defVal == null)
                            return defVal;

                        Expression exp = new AssignmentOperatorCache(result._variable as Variable ?? (result._variable as VariableDefinition)._initializers[0] as Variable);
                        exp = new Assignment(exp, defVal)
                        {
                            Position = exp.Position,
                            Length = defVal.EndPosition - exp.Position
                        };

                        if (result._variable == exp._left._left)
                            result._variable = exp;
                        else
                            (result._variable as VariableDefinition)._initializers[0] = exp;

                        Tools.SkipSpaces(state.Code, ref i);
                    }
                }

                if (!Parser.Validate(state.Code, "in", ref i))
                {
                    if (oldVariablesCount < state.Variables.Count)
                        state.Variables.RemoveRange(oldVariablesCount, state.Variables.Count - oldVariablesCount);

                    return null;
                }

                state.LabelsCount = 0;
                Tools.SkipSpaces(state.Code, ref i);

                if (result._variable is VariableDefinition)
                {
                    if ((result._variable as VariableDefinition)._variables.Length > 1)
                        ExceptionHelper.ThrowSyntaxError("Too many variables in for-in loop", state.Code, i);
                }

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
                state.lexicalScopeLevel--;
            }

            return new CodeBlock(new[] { result }) { _variables = vars, Position = result.Position, Length = result.Length };
        }

        public override JSValue Evaluate(Context context)
        {
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
                if (context._debugging && !(_variable is CodeBlock))
                    context.raiseDebugger(_variable);

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

            if (!source.Defined
                || (source._valueType >= JSValueType.Object && source._oValue == null)
                || _body == null)
                return JSValue.undefined;

            var index = 0;
            var processedKeys = (suspendData != null ? suspendData.processedKeys : null) ?? new HashSet<string>(StringComparer.Ordinal);
            while (source != null)
            {
                var keys = (suspendData != null ? suspendData.keys : null) ?? source.GetEnumerator(false, EnumerationMode.RequireValues);
                while (context._executionMode >= ExecutionMode.Resume || keys.MoveNext())
                {
                    if (context._executionMode != ExecutionMode.Resume)
                    {
                        var key = keys.Current.Key;
                        variable._valueType = JSValueType.String;
                        variable._oValue = key;
                        if (processedKeys.Contains(key))
                            continue;
                        processedKeys.Add(key);
                        if ((keys.Current.Value._attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                            continue;

                        if (context._debugging && !(_body is CodeBlock))
                            context.raiseDebugger(_body);
                    }

                    context._lastResult = _body.Evaluate(context) ?? context._lastResult;
                    if (context._executionMode != ExecutionMode.None)
                    {
                        if (context._executionMode < ExecutionMode.Return)
                        {
                            var me = context._executionInfo == null || System.Array.IndexOf(_labels, context._executionInfo._oValue as string) != -1;
                            var _break = (context._executionMode > ExecutionMode.Continue) || !me;
                            if (me)
                            {
                                context._executionMode = ExecutionMode.None;
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
                            suspendData.processedKeys = processedKeys;
                            suspendData.keys = keys;
                            return null;
                        }
                        else
                            return null;
                    }

                    index++;
                }

                source = source.__proto__;
                if (source == JSValue.@null || !source.Defined || (source._valueType >= JSValueType.Object && source._oValue == null))
                    break;
            }
            return null;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>()
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
            if (_variable is Expressions.Comma)
            {
                if ((_variable as Expressions.Comma).RightOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                _variable = (_variable as Expressions.Comma).LeftOperand;
            }

            if (message != null
                && (_source is ObjectDefinition
                || _source is ArrayDefinition
                || _source is Constant))
                message(MessageLevel.Recomendation, Position, Length, "for..in with constant source. This reduce performance. Rewrite without using for..in.");

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
            return "for (" + _variable + " in " + _source + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}