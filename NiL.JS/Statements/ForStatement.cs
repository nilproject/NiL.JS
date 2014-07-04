using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForStatement : CodeNode
    {
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
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "for(", ref i) && (!Parser.Validate(code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            CodeNode init = null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            init = code[i] == ';' ? null as CodeNode : Parser.Parse(state, ref i, 3);
            if (code[i] != ';')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = code[i] == ';' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            if (code[i] != ';')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var post = code[i] == ')' ? null as CodeNode : ExpressionStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
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
            JSObject res = JSObject.notExist;
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Invoke(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExist;
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
                res = body.Invoke(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return res;
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(post);
#endif
                post.Invoke(context);
            }
        }

        private JSObject impl2(Context context)
        {
            JSObject res = JSObject.undefined;
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            while ((bool)condition.Invoke(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Invoke(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
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
            while ((bool)condition.Invoke(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                res = body.Invoke(context) ?? res;
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return res;
                }
#if DEV
                if (context.debugging)
                {
                    context.raiseDebugger(post);
                    post.Invoke(context);
                    context.raiseDebugger(condition);
                }
                else
                    post.Invoke(context);
#else
                post.Invoke(context);
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
            while ((bool)condition.Invoke(context))
            {
#if DEV
                if (context.debugging)
                {
                    context.raiseDebugger(post);
                    post.Invoke(context);
                    context.raiseDebugger(condition);
                }
                else
                    post.Invoke(context);
#else
                post.Invoke(context);
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
            while ((bool)condition.Invoke(context))
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
            ;
            return JSObject.undefined;
        }

        internal override JSObject Invoke(Context context)
        {
            if (init != null)
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(init);
#endif
                init.Invoke(context);
            }
            if (implId == 3)
                return impl3(context);
            else if (implId == 0)
                return impl0(context);
            else if (implId == 1)
                return impl1(context);
            else if (implId == 2)
                return impl2(context);
            else if (implId == 4)
                return impl4(context);
            else if (implId == 5)
                return impl5(context);
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

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref init, 1, fdepth, variables, strict);
            Parser.Optimize(ref condition, 2, fdepth, variables, strict);
            Parser.Optimize(ref post, 1, fdepth, variables, strict);
            Parser.Optimize(ref body, System.Math.Max(1, depth), fdepth, variables, strict);
            return false;
        }

        public override string ToString()
        {
            var istring = (init as object ?? "").ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}