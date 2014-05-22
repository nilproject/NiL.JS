using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForStatement : Statement
    {
        private Statement init;
        private Statement condition;
        private Statement post;
        private Statement body;
        private List<string> labels;
        private int implId;

        public Statement Initializator { get { return init; } }
        public Statement Condition { get { return condition; } }
        public Statement Post { get { return post; } }
        public Statement Body { get { return body; } }
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
            Statement init = null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            init = code[i] == ';' ? null as Statement : Parser.Parse(state, ref i, 3);
            if (code[i] != ';')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = code[i] == ';' ? null as Statement : OperatorStatement.Parse(state, ref i).Statement;
            if (code[i] != ';')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var post = code[i] == ')' ? null as Statement : OperatorStatement.Parse(state, ref i).Statement;
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
            index = i;
            int id = 0;
            if (body != null)
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
                id = 4;
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
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount)
                }
            };
        }

        private JSObject impl0(Context context)
        {
            JSObject res = JSObject.undefined;
            for (; ; )
            {
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
            }
        }

        private JSObject impl1(Context context)
        {
            JSObject res = JSObject.undefined;
            for (; ; )
            {
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
                post.Invoke(context);
            }
        }

        private JSObject impl2(Context context)
        {
            JSObject res = JSObject.undefined;
            while ((bool)condition.Invoke(context))
            {
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
            }
            return res;
        }

        private JSObject impl3(Context context)
        {
            JSObject res = JSObject.undefined;
            while ((bool)condition.Invoke(context))
            {
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
                post.Invoke(context);
            }
            return res;
        }

        private JSObject impl4(Context context)
        {
            while ((bool)condition.Invoke(context))
                post.Invoke(context);
            return JSObject.undefined;
        }

        internal override JSObject Invoke(Context context)
        {
            if (init != null)
                init.Invoke(context);
            if (implId == 0)
                return impl0(context);
            else if (implId == 1)
                return impl1(context);
            else if (implId == 2)
                return impl2(context);
            else if (implId == 3)
                return impl3(context);
            else
                return impl4(context);
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref init, 1, varibles);
            Parser.Optimize(ref condition, 2, varibles);
            Parser.Optimize(ref post, 1, varibles);
            Parser.Optimize(ref body, System.Math.Max(1, depth), varibles);
            return false;
        }

        public override string ToString()
        {
            var istring = init.ToString();
            return "for (" + istring + "; " + condition + "; " + post + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}