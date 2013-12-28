using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal sealed class WhileStatement : Statement
    {
        private Statement condition;
        private Statement body;
        private List<string> labels;

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "while (", ref i) && !Parser.Validate(code, "while(", ref i))
                return new ParseResult();
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(code[i])) i++;
            var condition = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 0);
            state.AllowBreak--;
            state.AllowContinue--;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new WhileStatement()
                {
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount)
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            while ((bool)condition.Invoke(context))
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return null;
                }
            }
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}