using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class WhileStatement : Statement
    {
        private Statement condition;
        private Statement body;
        private List<string> labels;

        public Statement Condition { get { return condition; } }
        public Statement Body { get { return body; } }
        public string[] Labels { get { return labels.ToArray(); } }

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
            while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
            if (i >= code.Length)
                throw new JSException(new SyntaxError("Unexpected end of line."));
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (i < code.Length && char.IsWhiteSpace(code[i]));
            if (i >= code.Length)
                throw new JSException(new SyntaxError("Unexpected end of line."));
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            state.AllowBreak--;
            state.AllowContinue--;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new WhileStatement()
                {

                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount),
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
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
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
            }
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                body,
                condition
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            depth = System.Math.Max(1, depth);
            Parser.Optimize(ref body, depth, variables, strict);
            Parser.Optimize(ref condition, 2, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "while (" + condition + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}