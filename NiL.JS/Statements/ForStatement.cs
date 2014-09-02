using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForStatement : CodeNode
    {
#if !NET35

        internal override System.Linq.Expressions.Expression BuildTree(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            var continueLabel = Expression.Label("continue" + (DateTime.Now.Ticks % 1000));
            var breakLabel = Expression.Label("break" + (DateTime.Now.Ticks % 1000));
            for (var i = 0; i < labels.Count; i++)
                state.NamedContinueLabels[labels[i]] = continueLabel;
            state.ContinueLabels.Push(continueLabel);
            state.BreakLabels.Push(breakLabel);
            Expression res = null;
            try
            {
                switch (implId)
                {
                    case 0:
                        {
                            if (init == null)
                                res = Expression.Loop(body.BuildTree(state));
                            else
                                res = Expression.Block(init.BuildTree(state), Expression.Loop(body.BuildTree(state), breakLabel, continueLabel));
                            break;
                        }
                    case 1:
                        {
                            if (init == null)
                                res = Expression.Loop(Expression.Block(body.BuildTree(state), post.BuildTree(state)).Reduce());
                            else
                                res = Expression.Block(init.BuildTree(state), Expression.Loop(Expression.Block(body.BuildTree(state), Expression.Label(continueLabel), post.BuildTree(state)).Reduce(), breakLabel));
                            break;
                        }
                    case 2:
                        {
                            res = Expression.Block(
                                init != null ? init.BuildTree(state) : Expression.Empty(),
                                Expression.Loop(
                                    Expression.IfThenElse(Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.BuildTree(state)),
                                        body.BuildTree(state)
                                ,// else
                                        Expression.Break(breakLabel)).Reduce()
                                , breakLabel, continueLabel)
                            ).Reduce();
                            break;
                        }
                    case 3:
                        {
                            res = Expression.Block(
                                init != null ? init.BuildTree(state) : Expression.Empty(),
                                Expression.Loop(
                                    Expression.IfThenElse(Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.BuildTree(state)),
                                        Expression.Block(body.BuildTree(state), Expression.Label(continueLabel), post.BuildTree(state))
                                ,// else
                                        Expression.Break(breakLabel)).Reduce()
                                , breakLabel)
                            ).Reduce();
                            break;
                        }
                    case 4:
                        {
                            res = Expression.Block(
                                init != null ? init.BuildTree(state) : Expression.Empty(),
                                Expression.Loop(
                                    Expression.IfThenElse(Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.BuildTree(state)),
                                        Expression.Block(Expression.Label(continueLabel), post.BuildTree(state))
                                ,// else
                                        Expression.Break(breakLabel)).Reduce()
                                , breakLabel)
                            ).Reduce();
                            break;
                        }
                    case 5:
                        {
                            res = Expression.Block(
                                init != null ? init.BuildTree(state) : Expression.Empty(),
                                Expression.Loop(
                                    Expression.IfThen(Expression.Not(Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.BuildTree(state))),
                                        Expression.Break(breakLabel)).Reduce()
                                , breakLabel, continueLabel)
                            ).Reduce();
                            break;
                        }
                    default:
                        {
                            if (init == null)
                                res = Expression.Loop(JITHelpers.UndefinedConstant);
                            else
                                res = Expression.Block(init.BuildTree(state), Expression.Loop(JITHelpers.UndefinedConstant));
                            break;
                        }
                }
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
                for (var i = 0; i < labels.Count; i++)
                    state.NamedContinueLabels.Remove(labels[i]);
            }
        }
#endif
        private CodeNode init;
        private CodeNode condition;
        private CodeNode post;
        private CodeNode body;
        private List<string> labels;
        private int implId;

        public CodeNode Initializator { get { return init; } }
        public CodeNode Condition { get { return condition; } }
        public CodeNode Post { get { return post; } }
        public CodeNode Body { get { return body; } }
        public string[] Labels { get { return labels.ToArray(); } }

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
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var condition = state.Code[i] == ';' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            if (state.Code[i] != ';')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var post = state.Code[i] == ')' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            state.AllowBreak--;
            state.AllowContinue--;
            int startPos = index;
            index = i;
            int id = 0;
            if (body != null && !(body is EmptyStatement))
            {
                if (condition == null)
                {
                    if (post == null)
                        id = 0;
                    else
                        id = 1;
                }
                else
                {
                    if (post == null)
                        id = 2;
                    else
                        id = 3;
                }
            }
            else
            {
                if (post != null)
                    id = 4;
                else if (condition != null)
                    id = 5;
                else
                    id = 6;
            }
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ForStatement()
                {
                    body = body,
                    condition = condition,
                    init = init,
                    post = post,
                    implId = id,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount),
                    Position = startPos,
                    Length = index - startPos
                }
            };
        }

        private JSObject impl0(Context context)
        {
            JSObject res = JSObject.notExists;
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Evaluate(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExists;
                    }
                    if (_break)
                        return res;
                }
            }
        }

        private JSObject impl1(Context context)
        {
            JSObject res = JSObject.undefined;
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Evaluate(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExists;
                    }
                    if (_break)
                        return res;
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(post);
#endif
                post.Evaluate(context);
            }
        }

        private JSObject impl2(Context context)
        {
            JSObject res = JSObject.undefined;
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            while ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Evaluate(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExists;
                    }
                    if (_break)
                        return res;
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
            }
            return res;
        }

        private JSObject impl3(Context context)
        {
            JSObject res = JSObject.undefined;
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            while ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Evaluate(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExists;
                    }
                    if (_break)
                        return res;
                }
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
            return res;
        }

        private JSObject impl4(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            while ((bool)condition.Evaluate(context))
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
            return JSObject.undefined;
        }

        private JSObject impl5(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            while ((bool)condition.Evaluate(context))
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
                ;
            return JSObject.undefined;
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
            switch (implId)
            {
                case 0: return impl0(context);
                case 1: return impl1(context);
                case 2: return impl2(context);
                case 3: return impl3(context);
                case 4: return impl4(context);
                case 5: return impl5(context);
            }
            for (; ; ) ;
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

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref init, 1, variables, strict);
            Parser.Optimize(ref condition, 2, variables, strict);
            Parser.Optimize(ref post, 1, variables, strict);
            Parser.Optimize(ref body, System.Math.Max(1, depth), variables, strict);
            return false;
        }

        public override string ToString()
        {
            var istring = (init as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}