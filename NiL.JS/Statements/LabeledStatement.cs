using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class LabeledStatement : CodeNode
    {
        private CodeNode statement;
        private string label;

        public CodeNode Statement { get { return statement; } }
        public string Label { get { return label; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            string code = state.Code;
            if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                return new ParseResult();
            int l = i;
            if (i >= code.Length || (!Parser.Validate(code, " :", ref i) && code[i++] != ':'))
                return new ParseResult();
            var label = code.Substring(index, l - index);
            state.Labels.Add(label);
            int oldlc = state.LabelCount;
            state.LabelCount++;
            state.AllowBreak++;
            var stat = Parser.Parse(state, ref i, 0);
            state.AllowBreak--;
            state.Labels.Remove(label);
            state.LabelCount = oldlc;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new LabeledStatement()
                {
                    statement = stat,
                    label = label,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            statement.Invoke(context);
            if ((context.abort == AbortType.Break) && (context.abortInfo != null) && (context.abortInfo.oValue as string == label))
            {
                context.abort = AbortType.None;
                context.abortInfo = null;
            }
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref statement, depth, fdepth, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return label + ": " + statement;
        }
    }
}