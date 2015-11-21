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
    public sealed class ForOfStatement : CodeNode
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
                return new ReadOnlyCollection<string>(labels);
#else
                return System.Array.AsReadOnly(_labels);
#endif
            }
        }

        private ForOfStatement()
        {

        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            var res = new ForOfStatement()
            {
                _labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount).ToArray()
            };
            var vStart = i;
            if (Parser.Validate(state.Code, "var", ref i))
            {
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    ExceptionsHelper.Throw(new SyntaxError("Invalid variable name at " + CodeCoordinates.FromTextPosition(state.Code, start, 0)));
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                }
                res._variable = new VariableDefineStatement(varName, new GetVariableExpression(varName, state.scopeDepth) { Position = start, Length = i - start, defineScopeDepth = state.scopeDepth }, false, state.scopeDepth) { Position = vStart, Length = i - vStart };
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
                res._variable = new GetVariableExpression(varName, state.scopeDepth) { Position = start, Length = i - start, defineScopeDepth = state.scopeDepth };
            }
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] == '=')
            {
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
                var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, false, true);
                if (defVal == null)
                    return defVal;
                Expression exp = new AssignmentOperatorCache(res._variable as GetVariableExpression ?? (res._variable as VariableDefineStatement).initializers[0] as GetVariableExpression);
                exp = new AssignmentOperator(
                    exp,
                    (Expression)defVal)
                    {
                        Position = exp.Position,
                        Length = defVal.EndPosition - exp.Position
                    };
                if (res._variable == exp.first.first)
                    res._variable = exp;
                else
                    (res._variable as VariableDefineStatement).initializers[0] = exp;
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
            }
            if (!Parser.Validate(state.Code, "of", ref i))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            res._source = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (Tools.IsWhiteSpace(state.Code[i]))
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
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res._body.Position, res._body.Length), "Do not declare function in nested blocks.");
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
            SuspendData suspendData = null;
            if (context.abortType >= AbortType.Resume)
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
                if (context.abortType == AbortType.Suspend)
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
                if (_variable is AssignmentOperator)
                {
                    _variable.Evaluate(context);
                    variable = (_variable as AssignmentOperator).first.Evaluate(context);
                }
                else
                    variable = _variable.EvaluateForWrite(context);
                if (context.abortType == AbortType.Suspend)
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

            IIteratorResult iteratorResult = context.abortType != AbortType.Resume ? iterator.next() : null;
            while (context.abortType >= AbortType.Resume || !iteratorResult.done)
            {
                if (context.abortType != AbortType.Resume)
                    variable.Assign(iteratorResult.value);
                _body.Evaluate(context);

                if (context.abortType != AbortType.None)
                {
                    if (context.abortType < AbortType.Return)
                    {
                        var me = context.abortInfo == null || System.Array.IndexOf(_labels, context.abortInfo.oValue as string) != -1;
                        var _break = (context.abortType > AbortType.Continue) || !me;
                        if (me)
                        {
                            context.abortType = AbortType.None;
                            context.abortInfo = JSValue.notExists;
                        }
                        if (_break)
                            return null;
                    }
                    else if (context.abortType == AbortType.Suspend)
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
            _variable.Optimize(ref _variable, owner, message, opts, statistic);
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
            return "for (" + _variable + " of " + _source + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}