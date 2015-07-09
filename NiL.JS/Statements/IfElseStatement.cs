using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class IfStatement : CodeNode
    {
        private Expression condition;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Condition { get { return condition; } }

        internal IfStatement(Expression condition, CodeNode body)
        {
            this.condition = condition;
            this.body = body;
        }

        internal override JSValue Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                var temp = body.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
            }
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return new[] { body, condition };
        }

        public override string ToString()
        {
            string rp = Environment.NewLine;
            string rs = Environment.NewLine + "  ";
            var sbody = body.ToString();
            return "if (" + condition + ")" + (body is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs));
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var cc = condition as CodeNode;
            condition.Optimize(ref cc, owner, message, opts, statistic);
            condition = (Expression)cc;
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
            else
                _this = condition;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class IfElseStatement : CodeNode
    {
        private Expression condition;
        private CodeNode body;
        private CodeNode elseBody;

        public CodeNode Body { get { return body; } }
        public CodeNode ElseBody { get { return elseBody; } }
        public Expression Condition { get { return condition; } }

        private IfElseStatement()
        {

        }

        public IfElseStatement(Expression condition, CodeNode body, CodeNode elseBody)
        {
            this.condition = condition;
            this.body = body;
            this.elseBody = elseBody;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "if (", ref i) && !Parser.Validate(state.Code, "if(", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var condition = (Expression)ExpressionTree.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            CodeNode body = Parser.Parse(state, ref i, 0);
            if (body is FunctionExpression)
            {
                if (state.strict.Peek())
                    throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            CodeNode elseBody = null;
            var pos = i;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            if (i < state.Code.Length && !(body is CodeBlock) && (state.Code[i] == ';'))
                do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
            if (Parser.Validate(state.Code, "else", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i])) i++;
                elseBody = Parser.Parse(state, ref i, 0);
                if (elseBody is FunctionExpression)
                {
                    if (state.strict.Peek())
                        throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (state.message != null)
                        state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, elseBody.Position, elseBody.Length), "Do not declare function in nested blocks.");
                    elseBody = new CodeBlock(new[] { elseBody }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                    // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
                }
            }
            else
                i = pos;
            pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new IfElseStatement()
                {
                    body = body,
                    condition = condition,
                    elseBody = elseBody,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSValue Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                var temp = body.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
                return null;
            }
            else
            {
#if DEV
                if (context.debugging && !(elseBody is CodeBlock))
                    context.raiseDebugger(elseBody);
#endif
                var temp = elseBody.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
                return null;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                condition,
                elseBody
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref condition, 2, variables, state, message, statistic, opts);
            Parser.Build(ref body, depth, variables, state | _BuildState.Conditional, message, statistic, opts);
            Parser.Build(ref elseBody, depth, variables, state | _BuildState.Conditional, message, statistic, opts);

            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && condition is ToBool)
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, condition.Position, 2), "Useless conversion. Remove double negation in condition");
                condition = (condition as Expression).first;
            }
            try
            {
                if ((opts & Options.SuppressUselessExpressionsElimination) == 0
                    && (condition is Constant || (condition is Expression && ((Expression)condition).IsContextIndependent)))
                {
                    if ((bool)condition.Evaluate(null))
                        _this = body;
                    else
                        _this = elseBody;
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
            if (_this == this && elseBody == null)
                _this = new IfStatement(this.condition, this.body) { Position = Position, Length = Length };
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var cc = condition as CodeNode;
            condition.Optimize(ref cc, owner, message, opts, statistic);
            condition = (Expression)cc;
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
            if (elseBody != null)
                elseBody.Optimize(ref elseBody, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string rp = Environment.NewLine;
            string rs = Environment.NewLine + "  ";
            var sbody = body.ToString();
            var sebody = elseBody == null ? "" : elseBody.ToString();
            return "if (" + condition + ")" + (body is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs)) +
                (elseBody != null ?
                Environment.NewLine + "else" + Environment.NewLine +
                (elseBody is CodeBlock ? sebody.Replace(rp, rs) : "  " + sebody) : "");
        }
    }
}