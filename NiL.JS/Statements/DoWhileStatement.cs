using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
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

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "do", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.continiesCount;
            int cbs = state.breaksCount;
            var body = Parser.Parse(state, ref i, 4);
            if (body is FunctionExpression && state.strict.Peek())
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            if (!(body is CodeBlock) && state.Code[i] == ';')
                i++;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            if (i >= state.Code.Length)
                throw new JSException(new SyntaxError("Unexpected end of source."));
            if (!Parser.Validate(state.Code, "while", ref i))
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \"while\" at + " + CodeCoordinates.FromTextPosition(state.Code, i))));
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != '(')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \"(\" at + " + CodeCoordinates.FromTextPosition(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var condition = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i))));
            i++;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DoWhileStatement()
                {
                    allowRemove = ccs == state.continiesCount && cbs == state.breaksCount,
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = pos,
                    Length = index - pos
                }
            };
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            var continueTarget = System.Linq.Expressions.Expression.Label("continue" + (DateTime.Now.Ticks % 1000));
            var breakTarget = System.Linq.Expressions.Expression.Label("break" + (DateTime.Now.Ticks % 1000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueTarget;
            state.BreakLabels.Push(breakTarget);
            state.ContinueLabels.Push(continueTarget);
            try
            {
                return System.Linq.Expressions.Expression.Loop(
                    System.Linq.Expressions.Expression.Block(
                        body.CompileToIL(state),
                        System.Linq.Expressions.Expression.Label(continueTarget),
                        System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(null, JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state))),
                                                                  System.Linq.Expressions.Expression.Goto(breakTarget))
                    ),
                    breakTarget
                );
            }
            finally
            {
                if (state.BreakLabels.Peek() != breakTarget)
                    throw new InvalidOperationException();
                state.BreakLabels.Pop();
                if (state.ContinueLabels.Peek() != continueTarget)
                    throw new InvalidOperationException();
                state.ContinueLabels.Pop();
                for (var i = 0; i < labels.Length; i++)
                    state.NamedContinueLabels.Remove(labels[i]);
            }
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            do
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.abort != AbortType.None)
                {
                    var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                    var _break = (context.abort > AbortType.Continue) || !me;
                    if (context.abort < AbortType.Return && me)
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = JSObject.notExists;
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            depth = System.Math.Max(1, depth);
            Parser.Build(ref body, depth, variables, strict, message);
            Parser.Build(ref condition, 2, variables, strict, message);
            try
            {
                if (allowRemove && (condition is Constant || (condition as Expressions.Expression).IsContextIndependent))
                {
                    if ((bool)condition.Evaluate(null))
                        _this = new InfinityLoop(body, labels);
                    else if (labels.Length == 0)
                        _this = body;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(10, "Error", e.Message);
            }
            if (_this == this && body == null)
                body = new EmptyStatement();
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            condition.Optimize(ref condition, owner, message);
            body.Optimize(ref body, owner, message);
        }

        public override string ToString()
        {
            return "do" + (body is CodeBlock ? body + " " : Environment.NewLine + "  " + body + ";" + Environment.NewLine) + "while (" + condition + ")";
        }
    }
}