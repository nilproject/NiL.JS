using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class DoWhileStatement : Statement
    {
        private Statement condition;
        private Statement body;
        private List<string> labels;

        public Statement Condition { get { return condition; } }
        public Statement Body { get { return body; } }
        public string[] Labels { get { return labels.ToArray(); } }

        private DoWhileStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "do", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(code[i])) i++;
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 4);
            if (body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            state.AllowBreak--;
            state.AllowContinue--;
            if (!(body is CodeBlock) && code[i] == ';')
                i++;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "while", ref i))
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \"while\" at + " + Tools.PositionToTextcord(code, i))));
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '(')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \"(\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(code, i))));
            i++;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DoWhileStatement()
                {
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount),
                    Position = pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject res = null;
            do
            {
                res = body.Invoke(context);
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
            while ((bool)condition.Invoke(context));
            return res;
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            depth = System.Math.Max(1, depth);
            Parser.Optimize(ref body, depth, varibles);
            Parser.Optimize(ref condition, 2, varibles);
            return false;
        }

        public override string ToString()
        {
            return "do" + (body is CodeBlock ? body + " " : Environment.NewLine + "  " + body + ";" + Environment.NewLine) + "while (" + condition + ")";
        }
    }
}