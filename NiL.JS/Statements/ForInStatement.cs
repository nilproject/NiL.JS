using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ForInStatement : CodeNode
    {
        private CodeNode _variable;
        private CodeNode _source;
        private CodeNode _body;
        private string[] labels;

        public CodeNode Variable { get { return _variable; } }
        public CodeNode Source { get { return _source; } }
        public CodeNode Body { get { return _body; } }
        public ReadOnlyCollection<string> Labels
        {
            get
            {
#if PORTABLE
                return labels.AsReadOnly<string>();
#else
                return System.Array.AsReadOnly(labels);
#endif
            }
        }

        private ForInStatement()
        {

        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var res = new ForInStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount).ToArray()
            };
            var vStart = i;
            if (Parser.Validate(state.Code, "var", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    throw new ArgumentException();
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw(new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start)));
                }
                res._variable = new VariableDefineStatement(varName, new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, defineFunctionDepth = state.functionsDepth }, false, state.functionsDepth) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (state.Code[i] == ';')
                    return null;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                {
                    if (Parser.ValidateValue(state.Code, ref i))
                    {
                        while (char.IsWhiteSpace(state.Code[i]))
                            i++;
                        if (Parser.Validate(state.Code, "in", ref i))
                            ExceptionsHelper.Throw(new SyntaxError("Invalid accumulator name at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start)));
                        else
                            return null;
                    }
                    else
                        return null;
                    //ExceptionsHelper.Throw(new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, start)));
                }
                //if (!Parser.ValidateName(state.Code, ref i, state.strict))
                //    return null; // for (1 in {};;); должен вызвать синтаксическую ошибку, но это проверка заставляет перейти в обычный for, для которого такое выражение допустимо
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                }
                res._variable = new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, defineFunctionDepth = state.functionsDepth };
            }
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] == '=')
            {
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, false, true);
                if (defVal == null)
                    return defVal;
                NiL.JS.Expressions.Expression exp = new GetValueForAssignmentOperator(res._variable as GetVariableExpression ?? (res._variable as VariableDefineStatement).initializers[0] as GetVariableExpression);
                exp = new AssignmentOperator(
                    exp,
                    (NiL.JS.Expressions.Expression)defVal)
                    {
                        Position = res._variable.Position,
                        Length = defVal.EndPosition - res._variable.Position
                    };
                if (res._variable == exp.first.first)
                    res._variable = exp;
                else
                    (res._variable as VariableDefineStatement).initializers[0] = exp;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
            }
            if (!Parser.Validate(state.Code, "in", ref i))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            res._source = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            res._body = Parser.Parse(state, ref i, 0);
            if (res._body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res._body.Position, 0), "Do not declare function in nested blocks.");
                res._body = new CodeBlock(new[] { res._body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            res.Position = index;
            res.Length = i - index;
            index = i;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            var s = _source.Evaluate(context);
            JSValue v = null;
            if (_variable is AssignmentOperator)
            {
                _variable.Evaluate(context);
                v = (_variable as AssignmentOperator).first.Evaluate(context);
            }
            else
                v = _variable.EvaluateForWrite(context);
            if (!s.IsDefined
                || (s.valueType >= JSValueType.Object && s.oValue == null)
                || _body == null)
                return JSValue.undefined;
            int index = 0;
            HashSet<string> processedKeys = new HashSet<string>(StringComparer.Ordinal);
            while (s != null)
            {
                if (s.oValue is NiL.JS.BaseLibrary.Array)
                {
                    var src = s.oValue as NiL.JS.BaseLibrary.Array;
                    foreach (var item in src.data.DirectOrder)
                    {
                        if (item.Value == null
                            || !item.Value.IsExists
                            || (item.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                            continue;
                        if (item.Key >= 0)
                        {
                            v.oValue = item.Key.ToString();
                        }
                        else
                        {
                            v.oValue = ((uint)item.Key).ToString();
                        }
                        if (processedKeys.Contains(v.oValue.ToString()))
                            continue;
                        processedKeys.Add(v.oValue.ToString());
                        v.valueType = JSValueType.String;
#if DEV
                        if (context.debugging && !(_body is CodeBlock))
                            context.raiseDebugger(_body);
#endif
                        context.lastResult = _body.Evaluate(context) ?? context.lastResult;
                        if (context.abortType != AbortType.None)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (context.abortType < AbortType.Return && me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = JSValue.notExists;
                            }
                            if (_break)
                                return null;
                        }
                    }
                    if (src.fields != null)
                        foreach (var item in src.fields)
                        {
                            if (item.Value == null
                                || !item.Value.IsExists
                                || (item.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                                continue;
                            v.valueType = JSValueType.String;
                            v.oValue = item.Key;
                            if (processedKeys.Contains(v.oValue.ToString()))
                                continue;
                            processedKeys.Add(v.oValue.ToString());
#if DEV
                            if (context.debugging && !(_body is CodeBlock))
                                context.raiseDebugger(_body);
#endif
                            context.lastResult = _body.Evaluate(context) ?? context.lastResult;
                            if (context.abortType != AbortType.None)
                            {

                                var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                                var _break = (context.abortType > AbortType.Continue) || !me;
                                if (context.abortType < AbortType.Return && me)
                                {
                                    context.abortType = AbortType.None;
                                    context.abortInfo = JSValue.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                        }
                }
                else
                {
                    var keys = s.GetEnumerator(false, EnumerationMode.NeedValues);
                    try
                    {
                        for (; ; )
                        {
                            if (!keys.MoveNext())
                                break;
                            var key = keys.Current.Key;
                            v.valueType = JSValueType.String;
                            v.oValue = key;
                            if (processedKeys.Contains(key))
                                continue;
                            processedKeys.Add(key);
                            if ((keys.Current.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                                continue;
#if DEV
                            if (context.debugging && !(_body is CodeBlock))
                                context.raiseDebugger(_body);
#endif
                            context.lastResult = _body.Evaluate(context) ?? context.lastResult;
                            if (context.abortType != AbortType.None)
                            {

                                var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                                var _break = (context.abortType > AbortType.Continue) || !me;
                                if (context.abortType < AbortType.Return && me)
                                {
                                    context.abortType = AbortType.None;
                                    context.abortInfo = JSValue.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                            index++;
                        }
                    }
                    finally
                    {
                        keys.Dispose();
                    }
                }
                s = s.__proto__;
                if (s == JSValue.Null || !s.IsDefined || (s.valueType >= JSValueType.Object && s.oValue == null))
                    break;
            }
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
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

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref _variable, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            var tvar = _variable as VariableDefineStatement;
            if (tvar != null)
                _variable = tvar.initializers[0];
            if (_variable is AssignmentOperator)
                ((_variable as AssignmentOperator).first.first as GetVariableExpression).forceThrow = false;
            Parser.Build(ref _source, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            Parser.Build(ref _body, System.Math.Max(1, depth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, statistic, opts);
            if (_variable is Expressions.CommaOperator)
            {
                if ((_variable as Expressions.CommaOperator).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                _variable = (_variable as Expressions.CommaOperator).FirstOperand;
            }
            if (message != null
                && (_source is ObjectDefinition
                || _source is ArrayDefinition
                || _source is ConstantDefinition))
                message(MessageLevel.Recomendation, new CodeCoordinates(0, Position, Length), "for..in with constant source. This reduce performance. Rewrite without using for..in.");
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            _source.Optimize(ref _source, owner, message, opts, statistic);
            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref CodeNode self)
        {
            _variable.Decompose(ref _variable);
            _source.Decompose(ref _source);
            _body.Decompose(ref _body);
        }

        public override string ToString()
        {
            return "for (" + _variable + " in " + _source + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}