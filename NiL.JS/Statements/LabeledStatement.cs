using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
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
            var stat = Parser.Parse(state, ref i, 0);
            state.Labels.Remove(label);
            state.LabelCount = oldlc;
            if (stat is FunctionExpression)
            {
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, stat.Position, stat.Length), "Labeled function. Are you sure?.");
                stat = new CodeBlock(new[] { stat }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
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

        internal override JSObject Evaluate(Context context)
        {
            var res = statement.Evaluate(context);
            if ((context.abort == AbortType.Break) && (context.abortInfo != null) && (context.abortInfo.oValue as string == label))
            {
                context.abort = AbortType.None;
                context.abortInfo = JSObject.notExists;
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref statement, depth, variables, state, message, statistic, opts);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            statement.Optimize(ref statement, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return label + ": " + statement;
        }
    }
}