using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class DoWhileStatement : CodeNode
    {
        private CodeNode condition;
        private CodeNode body;
        private List<string> labels;

        public CodeNode Condition { get { return condition; } }
        public CodeNode Body { get { return body; } }
        public ICollection<string> Labels { get { return labels.AsReadOnly(); } }

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
            while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
            if (i >= code.Length)
                throw new JSException(new SyntaxError("Unexpected end of source."));
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
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject res = null;
            do
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
                        context.abortInfo = JSObject.notExist;
                    }
                    if (_break)
                        return res;
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
            }
            while ((bool)condition.Invoke(context));
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                condition
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            depth = System.Math.Max(1, depth);
            Parser.Optimize(ref body, depth, fdepth, variables, strict);
            Parser.Optimize(ref condition, 2, fdepth, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "do" + (body is CodeBlock ? body + " " : Environment.NewLine + "  " + body + ";" + Environment.NewLine) + "while (" + condition + ")";
        }
    }
}