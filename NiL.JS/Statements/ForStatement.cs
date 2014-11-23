using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForStatement : CodeNode
    {
#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            var continueLabel = System.Linq.Expressions.Expression.Label("continue" + (DateTime.Now.Ticks % 1000));
            var breakLabel = System.Linq.Expressions.Expression.Label("break" + (DateTime.Now.Ticks % 1000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueLabel;
            state.ContinueLabels.Push(continueLabel);
            state.BreakLabels.Push(breakLabel);
            System.Linq.Expressions.Expression res = null;
            try
            {
                System.Linq.Expressions.Expression loopBody = null;
                if (body == null)
                {
                    if (post == null)
                        loopBody = System.Linq.Expressions.Expression.Label(continueLabel);
                    else
                        loopBody = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Label(continueLabel), post.CompileToIL(state));
                }
                else
                {
                    if (post == null)
                        loopBody = System.Linq.Expressions.Expression.Block(body.CompileToIL(state), System.Linq.Expressions.Expression.Label(continueLabel));
                    else
                        loopBody = System.Linq.Expressions.Expression.Block(body.CompileToIL(state), System.Linq.Expressions.Expression.Label(continueLabel), post.CompileToIL(state));
                }
                System.Linq.Expressions.Expression loop = condition == null ? System.Linq.Expressions.Expression.Loop(loopBody, breakLabel) :
                    System.Linq.Expressions.Expression.Loop(
                    System.Linq.Expressions.Expression.IfThenElse(System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state)) as System.Linq.Expressions.Expression,
                        loopBody
                    ,// else
                        System.Linq.Expressions.Expression.Break(breakLabel)).Reduce()
                    , breakLabel);
                if (init != null)
                    res = System.Linq.Expressions.Expression.Block(init.CompileToIL(state), loop);
                else
                    res = loop;
                return res;
            }
            finally
            {
                if (state.BreakLabels.Peek() != breakLabel)
                    throw new InvalidOperationException();
                state.BreakLabels.Pop();
                if (state.ContinueLabels.Peek() != continueLabel)
                    throw new InvalidOperationException();
                state.ContinueLabels.Pop();
                for (var i = 0; i < labels.Length; i++)
                    state.NamedContinueLabels.Remove(labels[i]);
            }
        }
#endif
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
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            CodeNode init = null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            init = state.Code[i] == ';' ? null as CodeNode : Parser.Parse(state, ref i, 3);
            if (state.Code[i] != ';')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var condition = state.Code[i] == ';' ? null as CodeNode : ExpressionTree.Parse(state, ref i).Statement;
            if (state.Code[i] != ';')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var post = state.Code[i] == ')' ? null as CodeNode : ExpressionTree.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.continiesCount;
            int cbs = state.breaksCount;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionExpression && state.strict.Peek())
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
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
            do
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                if (body != null)
                {
                    context.lastResult = body.Evaluate(context) ?? context.lastResult;
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
                if (post == null)
                    continue;
#if DEV
                if (context.debugging)
                {
                    context.raiseDebugger(post);
                    post.Evaluate(context);
                    context.raiseDebugger(condition);
                }
                else
                    post.Evaluate(context);
#else
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Build(ref init, 1, variables, strict);
            if (init is VariableDefineStatement
                && (init as VariableDefineStatement).initializators.Length == 1)
                init = (init as VariableDefineStatement).initializators[0];
            Parser.Build(ref condition, 2, variables, strict);
            Parser.Build(ref post, 1, variables, strict);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, strict);
            if (condition == null)
                condition = new Constant(Core.BaseTypes.Boolean.True);
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

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            if (init != null)
                init.Optimize(ref init, owner);
            if (condition != null)
                condition.Optimize(ref condition, owner);
            if (post != null)
                post.Optimize(ref post, owner);
            if (body != null)
                body.Optimize(ref body, owner);
        }

        public override string ToString()
        {
            var istring = (init as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}