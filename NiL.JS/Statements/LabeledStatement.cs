using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            //string code = state.Code;
            if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                return new ParseResult();
            int l = i;
            if (i >= state.Code.Length || (!Parser.Validate(state.Code, " :", ref i) && state.Code[i++] != ':'))
                return new ParseResult();
            var label = state.Code.Substring(index, l - index);
            state.Labels.Add(label);
            int oldlc = state.LabelCount;
            state.LabelCount++;
            state.AllowBreak.Push(true);
            var stat = Parser.Parse(state, ref i, 0);
            state.AllowBreak.Pop();
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

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var labelTarget = Expression.Label(label);
            state.NamedBreakLabels[label] = labelTarget;
            return Expression.Block(statement.CompileToIL(state), Expression.Label(labelTarget));
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            statement.Evaluate(context);
            if ((context.abort == AbortType.Break) && (context.abortInfo != null) && (context.abortInfo.oValue as string == label))
            {
                context.abort = AbortType.None;
                context.abortInfo = JSObject.notExists;
            }
            return JSObject.notExists;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref statement, depth, variables, strict, message);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message)
        {
            statement.Optimize(ref statement, owner, message);
        }

        public override string ToString()
        {
            return label + ": " + statement;
        }
    }
}