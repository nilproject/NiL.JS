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
        private CodeNode init;
        private CodeNode condition;
        private CodeNode post;
        private CodeNode body;
        private string[] labels;

        public CodeNode Initializator { get { return init; } }
        public CodeNode Condition { get { return condition; } }
        public CodeNode Post { get { return post; } }
        public CodeNode Body { get { return body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        private ForStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            CodeNode init = null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            init = state.Code[i] == ';' ? null as CodeNode : Parser.Parse(state, ref i, 3);
            if ((init is ExpressionTree)
                && (init as ExpressionTree).Type == OperationType.None
                && (init as ExpressionTree).second == null)
                init = (init as ExpressionTree).first;
            if (state.Code[i] != ';')
                throw new JSException((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var condition = state.Code[i] == ';' ? null as CodeNode : ExpressionTree.Parse(state, ref i).Statement;
            if (state.Code[i] != ';')
                throw new JSException((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var post = state.Code[i] == ')' ? null as CodeNode : ExpressionTree.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new SyntaxError("Expected \";\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionExpression)
            {
                if (state.strict.Peek())
                    throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            int startPos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ForStatement()
                {
                    body = body,
                    condition = condition,
                    init = init,
                    post = post,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = startPos,
                    Length = index - startPos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            if (init != null)
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(init);
#endif
                init.Evaluate(context);
            }
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if (!(bool)condition.Evaluate(context))
                return null;
            bool be = body != null;
            bool pne = post == null;
            do
            {
                if (be)
                {
#if DEV
                    if (context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    var temp = body.Evaluate(context);
                    if (temp != null)
                        context.lastResult = temp;
                    if (context.abort != AbortType.None)
                    {
                        var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                        var _break = (context.abort > AbortType.Continue) || !me;
                        if (context.abort < AbortType.Return && me)
                        {
                            context.abort = AbortType.None;
                            context.abortInfo = null;
                        }
                        if (_break)
                            return null;
                    }
                }
#if DEV
                if (context.debugging)
                {
                    if (!pne)
                    {
                        context.raiseDebugger(post);
                        post.Evaluate(context);
                    }
                    context.raiseDebugger(condition);
                }
                else if (!pne)
                    post.Evaluate(context);
#else
                if (pne)
                    continue;
                post.Evaluate(context);
#endif
            } while ((bool)condition.Evaluate(context));
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                init, 
                condition,
                post,
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref init, 1, variables, state, message, statistic, opts);
            if ((opts & Options.SuppressUselessStatementsElimination) == 0
                && init is VariableDefineStatement
                && !(init as VariableDefineStatement).isConst
                && (init as VariableDefineStatement).initializators.Length == 1)
                init = (init as VariableDefineStatement).initializators[0];
            Parser.Build(ref condition, 2, variables, state | _BuildState.InLoop, message, statistic, opts);
            if (post != null)
            {
                Parser.Build(ref post, 1, variables, state | _BuildState.Conditional | _BuildState.InLoop, message, statistic, opts);
                if (post == null && message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Last expression of for-loop was removed. Maybe, it's a mistake.");
            }
            Parser.Build(ref body, System.Math.Max(1, depth), variables, state | _BuildState.Conditional | _BuildState.InLoop, message, statistic, opts);
            if (condition == null)
                condition = new Constant(NiL.JS.BaseLibrary.Boolean.True);
            else if ((condition is Expressions.Expression)
                && (condition as Expressions.Expression).IsContextIndependent
                && !(bool)condition.Evaluate(null))
            {
                _this = init;
                return false;
            }
            else if (body == null || body is EmptyStatement) // initial solution. Will extended
            {
                VariableReference variable = null;
                Constant limit = null;
                if (condition is NiL.JS.Expressions.Less)
                {
                    variable = (condition as NiL.JS.Expressions.Less).FirstOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.Less).SecondOperand as Constant;
                }
                else if (condition is NiL.JS.Expressions.More)
                {
                    variable = (condition as NiL.JS.Expressions.Less).SecondOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.Less).FirstOperand as Constant;
                }
                else if (condition is NiL.JS.Expressions.NotEqual)
                {
                    variable = (condition as NiL.JS.Expressions.Less).SecondOperand as VariableReference;
                    limit = (condition as NiL.JS.Expressions.Less).FirstOperand as Constant;
                    if (variable == null && limit == null)
                    {
                        variable = (condition as NiL.JS.Expressions.Less).FirstOperand as VariableReference;
                        limit = (condition as NiL.JS.Expressions.Less).SecondOperand as Constant;
                    }
                }
                if (variable != null
                    && limit != null
                    && post is NiL.JS.Expressions.Incriment
                    && ((post as NiL.JS.Expressions.Incriment).FirstOperand as VariableReference).descriptor == variable.descriptor)
                {
                    if (variable.functionDepth >= 0 && variable.descriptor.defineDepth >= 0)
                    {
                        if (init is NiL.JS.Expressions.Assign
                            && (init as NiL.JS.Expressions.Assign).FirstOperand is GetVariableExpression
                            && ((init as NiL.JS.Expressions.Assign).FirstOperand as GetVariableExpression).descriptor == variable.descriptor)
                        {
                            var value = (init as NiL.JS.Expressions.Assign).SecondOperand;
                            if (value is Constant)
                            {
                                var vvalue = value.Evaluate(null);
                                var lvalue = limit.Evaluate(null);
                                if ((vvalue.valueType == JSObjectType.Int
                                    || vvalue.valueType == JSObjectType.Bool
                                    || vvalue.valueType == JSObjectType.Double)
                                    && (lvalue.valueType == JSObjectType.Int
                                    || lvalue.valueType == JSObjectType.Bool
                                    || lvalue.valueType == JSObjectType.Double))
                                {
                                    if (!(bool)NiL.JS.Expressions.Less.Check(vvalue, lvalue))
                                    {
                                        _this = init;
                                        return false;
                                    }
                                    _this = new CodeBlock(new[] { new NiL.JS.Expressions.Assign(variable, limit), init }, false);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (init != null)
                init.Optimize(ref init, owner, message, opts, statistic);
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

        public override string ToString()
        {
            var istring = (init as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}