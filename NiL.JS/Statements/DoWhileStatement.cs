using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DoWhileStatement : CodeNode
    {
        private bool allowRemove;
        private CodeNode condition;
        private CodeNode body;
        private string[] labels;

        public CodeNode Condition { get { return condition; } }
        public CodeNode Body { get { return body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        private DoWhileStatement()
        {

        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "do", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.continiesCount;
            int cbs = state.breaksCount;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }, state.strict); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            if (!(body is CodeBlock) && state.Code[i] == ';')
                i++;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));
            if (!Parser.Validate(state.Code, "while", ref i))
                ExceptionsHelper.Throw((new SyntaxError("Expected \"while\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != '(')
                ExceptionsHelper.Throw((new SyntaxError("Expected \"(\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            var condition = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            i++;
            var pos = index;
            index = i;
            return new DoWhileStatement()
                {
                    allowRemove = ccs == state.continiesCount && cbs == state.breaksCount,
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            do
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.abortType != AbortType.None)
                {
                    var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                    var _break = (context.abortType > AbortType.Continue) || !me;
                    if (context.abortType < AbortType.Return && me)
                    {
                        context.abortType = AbortType.None;
                        context.abortInfo = JSValue.notExists;
                    }
                    if (_break)
                        return null;
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
            }
            while ((bool)condition.Evaluate(context));
            return null;
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

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            depth = System.Math.Max(1, depth);
            Parser.Build(ref body, depth, variables, state | CodeContext.InLoop, message, statistic, opts);
            Parser.Build(ref condition, 2, variables, state | CodeContext.InLoop | CodeContext.InExpression, message, statistic, opts);
            try
            {
                if (allowRemove
                    && (opts & Options.SuppressUselessExpressionsElimination) == 0
                    && (condition is ConstantDefinition || (condition as Expressions.Expression).IsContextIndependent))
                {
                    if ((bool)condition.Evaluate(null))
                        _this = new InfinityLoopStatement(body, labels);
                    else if (labels.Length == 0)
                        _this = body;
                    condition.Eliminated = true;
                }
            }

#if PORTABLE
            catch
            {
#else
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(10, "Error", e.Message);
#endif
            }
            if (_this == this && body == null)
                body = new EmptyExpression();
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            condition.Optimize(ref condition, owner, message, opts, statistic);
            body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "do" + (body is CodeBlock ? body + " " : Environment.NewLine + "  " + body + ";" + Environment.NewLine) + "while (" + condition + ")";
        }
    }
}