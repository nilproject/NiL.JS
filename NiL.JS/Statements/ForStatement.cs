using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForStatement : CodeNode
    {
#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            var continueLabel = Expression.Label("continue" + (DateTime.Now.Ticks % 1000));
            var breakLabel = Expression.Label("break" + (DateTime.Now.Ticks % 1000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueLabel;
            state.ContinueLabels.Push(continueLabel);
            state.BreakLabels.Push(breakLabel);
            Expression res = null;
            try
            {
                Expression loopBody = null;
                if (body == null)
                {
                    if (post == null)
                        loopBody = Expression.Label(continueLabel);
                    else
                        loopBody = Expression.Block(Expression.Label(continueLabel), post.CompileToIL(state));
                }
                else
                {
                    if (post == null)
                        loopBody = Expression.Block(body.CompileToIL(state), Expression.Label(continueLabel));
                    else
                        loopBody = Expression.Block(body.CompileToIL(state), Expression.Label(continueLabel), post.CompileToIL(state));
                }
                Expression loop = condition == null ? Expression.Loop(loopBody, breakLabel) :
                    Expression.Loop(
                    Expression.IfThenElse(Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state)) as Expression,
                        loopBody
                    ,// else
                        Expression.Break(breakLabel)).Reduce()
                    , breakLabel);
                if (init != null)
                    res = Expression.Block(init.CompileToIL(state), loop);
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
            var condition = state.Code[i] == ';' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            if (state.Code[i] != ';')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var post = state.Code[i] == ')' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.continiesCount;
            int cbs = state.breaksCount;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionStatement && state.strict.Peek())
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
            JSObject res = JSObject.undefined;
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if (condition == null || (bool)condition.Evaluate(context))
                do
                {
#if DEV
                    if (context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    if (body != null)
                    {
                        res = body.Evaluate(context) ?? res;
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
                                return res;
                        }
                    }
                    if (post != null)
                    {
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
                    }
                } while (condition == null || (bool)condition.Evaluate(context));
            return res;
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
            Parser.Build(ref condition, 2, variables, strict);
            Parser.Build(ref post, 1, variables, strict);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, strict);
            return false;
        }

        public override string ToString()
        {
            var istring = (init as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}