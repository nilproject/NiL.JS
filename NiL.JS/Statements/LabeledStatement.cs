using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class LabeledStatement : Statement, IOptimizable
    {
        private Statement statement;
        private string label;

        public static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            string code = state.Code;
            if (!Parser.ValidateName(code, ref i))
                return new ParseResult();
            int l = i;
            if (!Parser.Validate(code, " :", ref i) && code[i++] != ':')
                return new ParseResult();
            var label = code.Substring(index, l - index);
            state.Labels.Add(label);
            int oldlc = state.LabelCount;
            state.LabelCount++;
            state.AllowBreak++;
            var stat = Parser.Parse(state, ref i, 0);
            state.AllowBreak--;
            state.LabelCount = oldlc;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new LabeledStatement()
                {
                    statement = stat,
                    label = label
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            statement.Invoke(context);
            if ((context.abort == AbortType.Break) && (context.abortInfo != null) && (context.abortInfo.oValue as string == label))
            {
                context.abort = AbortType.None;
                context.abortInfo = null;
            }
            return JSObject.undefined;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref statement, depth, varibles);
            return false;
        }
    }
}