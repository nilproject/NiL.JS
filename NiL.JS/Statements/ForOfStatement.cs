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
        private CodeNode _variable;
        private CodeNode _source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return _variable; } }
        public CodeNode Source { get { return _source; } }
        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels
        {
            get
            {
#if PORTABLE
                return new ReadOnlyCollection<string>(labels);
#else
                return System.Array.AsReadOnly<string>(labels);
#endif
            }
        }

        private ForOfStatement()
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
            var res = new ForOfStatement()
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
                    ExceptionsHelper.Throw(new SyntaxError("Invalid variable name at " + CodeCoordinates.FromTextPosition(state.Code, start, 0)));
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
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
                    return null;
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
                NiL.JS.Expressions.Expression exp = new GetValueForAssignmentOperator(res._variable as GetVariableExpression ?? (res._variable as VariableDefineStatement).initializators[0] as GetVariableExpression);
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
                    (res._variable as VariableDefineStatement).initializators[0] = exp;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
            }
            if (!Parser.Validate(state.Code, "of", ref i))
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
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res.body.Position, res.body.Length), "Do not declare function in nested blocks.");
                res.body = new CodeBlock(new[] { res.body }, state.strict); // для того, чтобы не дублировать код по декларации функции, 
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

            JSValue variable = null;
            var source = _source.Evaluate(context);

            if (_variable is AssignmentOperator)
            {
                _variable.Evaluate(context);
                variable = (_variable as AssignmentOperator).first.Evaluate(context);
            }
            else
                variable = _variable.EvaluateForWrite(context);

            if (!source.IsDefined || source.IsNull || body == null)
                return null;

            var iterator = source.AsIterable().iterator();
            IIteratorResult iteratorResult = iterator.next();

            while (!iteratorResult.done)
            {
                variable.Assign(iteratorResult.value);
                body.Evaluate(context);
                iteratorResult = iterator.next();
            }

            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>
            {
                body,
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
                _variable = tvar.initializators[0];
            if (_variable is AssignmentOperator)
                ((_variable as AssignmentOperator).first.first as GetVariableExpression).forceThrow = false;
            Parser.Build(ref _source, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, statistic, opts);
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
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "for (" + _variable + " in " + _source + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}