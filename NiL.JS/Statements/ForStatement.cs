using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ForStatement : CodeNode
    {
        private CodeNode initializer;
        private CodeNode condition;
        private CodeNode post;
        private CodeNode body;
        private string[] labels;

        public CodeNode Initializator { get { return initializer; } }
        public CodeNode Condition { get { return condition; } }
        public CodeNode Post { get { return post; } }
        public CodeNode Body { get { return body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        private ForStatement()
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
            CodeNode init = null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            init = VariableDefineStatement.Parse(state, ref i);
            if (init == null)
                init = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            if ((init is ExpressionTree)
                && (init as ExpressionTree).Type == OperationType.None
                && (init as ExpressionTree).second == null)
                init = (init as ExpressionTree).first;
            if (state.Code[i] != ';')
                ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            var condition = state.Code[i] == ';' ? null as CodeNode : ExpressionTree.Parse(state, ref i);
            if (state.Code[i] != ';')
                ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            var post = state.Code[i] == ')' ? null as CodeNode : ExpressionTree.Parse(state, ref i);
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            int startPos = index;
            index = i;
            return new ForStatement()
                {
                    body = body,
                    condition = condition,
                    initializer = init,
                    post = post,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = startPos,
                    Length = index - startPos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            if (initializer != null
                && (context.abortType != AbortType.Resume
                    || context.SuspendData[this] == initializer))
            {
#if DEV
                if (context.abortType != AbortType.Resume && context.debugging)
                    context.raiseDebugger(initializer);
#endif
                initializer.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = initializer;
                    return null;
                }
            }
            bool be = body != null;
            bool pe = post != null;
            JSValue checkResult;

            if (context.abortType != AbortType.Resume || context.SuspendData[this] == condition)
            {
#if DEV
                if (context.abortType != AbortType.Resume && context.debugging)
                    context.raiseDebugger(condition);
#endif
                checkResult = condition.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = condition;
                    return null;
                }
                if (!(bool)checkResult)
                    return null;
            }

            do
            {
                if (be
                 && (context.abortType != AbortType.Resume
                    || context.SuspendData[this] == body))
                {
#if DEV
                    if (context.abortType != AbortType.Resume && context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    var temp = body.Evaluate(context);
                    if (temp != null)
                        context.lastResult = temp;
                    if (context.abortType != AbortType.None)
                    {
                        if (context.abortType < AbortType.Return)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = null;
                            }
                            if (_break)
                                return null;
                        }
                        else if (context.abortType == AbortType.Suspend)
                        {
                            context.SuspendData[this] = body;
                            return null;
                        }
                        else
                            return null;
                    }
                }

                if (pe
                 && (context.abortType != AbortType.Resume
                    || context.SuspendData[this] == post))
                {
#if DEV
                    if (context.abortType != AbortType.Resume && context.debugging)
                        context.raiseDebugger(post);
#endif
                    post.Evaluate(context);
                }
#if DEV
                if (context.abortType != AbortType.Resume && context.debugging)
                    context.raiseDebugger(condition);
#endif
                checkResult = condition.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = condition;
                    return null;
                }
            }
            while ((bool)checkResult);
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                initializer, 
                condition,
                post,
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref initializer, 1, variables, codeContext, message, statistic, opts);
            if ((opts & Options.SuppressUselessStatementsElimination) == 0
                && initializer is VariableDefineStatement
                && !(initializer as VariableDefineStatement).isConst
                && (initializer as VariableDefineStatement).initializers.Length == 1)
                initializer = (initializer as VariableDefineStatement).initializers[0];
            Parser.Build(ref condition, 2, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, statistic, opts);
            if (post != null)
            {
                Parser.Build(ref post, 1, variables, codeContext | CodeContext.Conditional | CodeContext.InLoop | CodeContext.InExpression, message, statistic, opts);
                if (post == null && message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Last expression of for-loop was removed. Maybe, it's a mistake.");
            }
            Parser.Build(ref body, System.Math.Max(1, depth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, statistic, opts);
            if (condition == null)
                condition = new ConstantDefinition(NiL.JS.BaseLibrary.Boolean.True);
            else if ((condition is Expressions.Expression)
                && (condition as Expressions.Expression).ContextIndependent
                && !(bool)condition.Evaluate(null))
            {
                _this = initializer;
                return false;
            }
            else if (body == null || body is EmptyExpression) // initial solution. Will extended
            {
                VariableReference variable = null;
                ConstantDefinition limit = null;
                if (condition is NiL.JS.Expressions.LessOperator)
                {
                    variable = (condition as NiL.JS.Expressions.LessOperator).FirstOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.LessOperator).SecondOperand as ConstantDefinition;
                }
                else if (condition is NiL.JS.Expressions.MoreOperator)
                {
                    variable = (condition as NiL.JS.Expressions.MoreOperator).SecondOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.MoreOperator).FirstOperand as ConstantDefinition;
                }
                else if (condition is NiL.JS.Expressions.NotEqualOperator)
                {
                    variable = (condition as NiL.JS.Expressions.LessOperator).SecondOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.LessOperator).FirstOperand as ConstantDefinition;
                    if (variable == null && limit == null)
                    {
                        variable = (condition as NiL.JS.Expressions.LessOperator).FirstOperand as VariableReference;
                        limit = (condition as NiL.JS.Expressions.LessOperator).SecondOperand as ConstantDefinition;
                    }
                }
                if (variable != null
                    && limit != null
                    && post is NiL.JS.Expressions.IncrementOperator
                    && ((post as NiL.JS.Expressions.IncrementOperator).FirstOperand as VariableReference).descriptor == variable.descriptor)
                {
                    if (variable.defineFunctionDepth >= 0 && variable.descriptor.defineDepth >= 0)
                    {
                        if (initializer is NiL.JS.Expressions.AssignmentOperator
                            && (initializer as NiL.JS.Expressions.AssignmentOperator).FirstOperand is GetVariableExpression
                            && ((initializer as NiL.JS.Expressions.AssignmentOperator).FirstOperand as GetVariableExpression).descriptor == variable.descriptor)
                        {
                            var value = (initializer as NiL.JS.Expressions.AssignmentOperator).SecondOperand;
                            if (value is ConstantDefinition)
                            {
                                var vvalue = value.Evaluate(null);
                                var lvalue = limit.Evaluate(null);
                                if ((vvalue.valueType == JSValueType.Int
                                    || vvalue.valueType == JSValueType.Bool
                                    || vvalue.valueType == JSValueType.Double)
                                    && (lvalue.valueType == JSValueType.Int
                                    || lvalue.valueType == JSValueType.Bool
                                    || lvalue.valueType == JSValueType.Double))
                                {
                                    post.Eliminated = true;
                                    condition.Eliminated = true;

                                    if (!(bool)NiL.JS.Expressions.LessOperator.Check(vvalue, lvalue))
                                    {

                                        _this = initializer;
                                        return false;
                                    }

                                    _this = new CodeBlock(new[] { initializer, new NiL.JS.Expressions.AssignmentOperator(variable, limit) });
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (initializer != null)
                initializer.Optimize(ref initializer, owner, message, opts, statistic);
            if (condition != null)
                condition.Optimize(ref condition, owner, message, opts, statistic);
            if (post != null)
                post.Optimize(ref post, owner, message, opts, statistic);
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref CodeNode self)
        {
            initializer.Decompose(ref initializer);
            condition.Decompose(ref condition);
            body.Decompose(ref body);
            post.Decompose(ref post);
        }

        public override string ToString()
        {
            var istring = (initializer as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}