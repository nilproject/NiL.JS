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
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "do", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (Tools.IsWhiteSpace(state.Code[i]))
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
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            if (!(body is CodeBlock) && state.Code[i] == ';')
                i++;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));
            if (!Parser.Validate(state.Code, "while", ref i))
                ExceptionsHelper.Throw((new SyntaxError("Expected \"while\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != '(')
                ExceptionsHelper.Throw((new SyntaxError("Expected \"(\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));
            var condition = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (Tools.IsWhiteSpace(state.Code[i]))
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
            JSValue checkResult;
            do
            {
                if (context.abortType != AbortType.Resume || !context.SuspendData.ContainsKey(this))
                {
#if DEV
                    if (context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    context.lastResult = body.Evaluate(context) ?? context.lastResult;
                    if (context.abortType != AbortType.None)
                    {
                        if (context.abortType < AbortType.Return)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = JSValue.notExists;
                            }
                            if (_break)
                                return null;
                        }
                        else
                            return null;
                    }
                }
#if DEV
                if (context.debugging)
                    context.raiseDebugger(condition);
#endif
                checkResult = condition.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = null;
                    return null;
                }
            }
            while ((bool)checkResult);
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                condition
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            expressionDepth = System.Math.Max(1, expressionDepth);
            Parser.Build(ref body, expressionDepth, scopeVariables, variables, codeContext | CodeContext.InLoop, message, stats, opts);
            Parser.Build(ref condition, 2, scopeVariables, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, stats, opts);
            try
            {
                if (allowRemove
                    && (opts & Options.SuppressUselessExpressionsElimination) == 0
                    && (condition is ConstantDefinition || (condition as Expressions.Expression).ContextIndependent))
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

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            condition.Optimize(ref condition, owner, message, opts, stats);
            body.Optimize(ref body, owner, message, opts, stats);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref CodeNode self)
        {
            if (condition != null)
                condition.Decompose(ref condition);
            if (body != null)
                body.Decompose(ref body);
        }

        public override string ToString()
        {
            return "do" + (body is CodeBlock ? body + " " : Environment.NewLine + "  " + body + ";" + Environment.NewLine) + "while (" + condition + ")";
        }
    }
}