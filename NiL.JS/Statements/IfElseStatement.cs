using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class IfStatement : CodeNode
    {
        private CodeNode condition;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Condition { get { return condition; } }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state)), body.CompileToIL(state));
        }

#endif

        internal IfStatement(IfElseStatement parent)
        {
            condition = parent.Condition;
            body = parent.Body;
        }

        internal override JSObject Evaluate(Context context)
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
                return body.Evaluate(context);
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
    }

    [Serializable]
    public sealed class IfElseStatement : CodeNode
    {
        private CodeNode condition;
        private CodeNode body;
        private CodeNode elseBody;

        public CodeNode Body { get { return body; } }
        public CodeNode ElseBody { get { return elseBody; } }
        public CodeNode Condition { get { return condition; } }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return elseBody != null ?
                System.Linq.Expressions.Expression.IfThenElse(System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state)), body.CompileToIL(state), elseBody.CompileToIL(state))
                :
                System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToBooleanMethod, condition.CompileToIL(state)), body.CompileToIL(state));
        }

#endif

        private IfElseStatement()
        {

        }

        public IfElseStatement(CodeNode condition, CodeNode body, CodeNode elseBody)
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
            CodeNode condition = ExpressionStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            CodeNode body = Parser.Parse(state, ref i, 0);
            if (body is FunctionStatement && state.strict.Peek())
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            CodeNode elseBody = null;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            if (i < state.Code.Length && !(body is CodeBlock) && (state.Code[i] == ';'))
                do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
            if (Parser.Validate(state.Code, "else", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i])) i++;
                elseBody = Parser.Parse(state, ref i, 0);
                if (elseBody is FunctionStatement && state.strict.Peek())
                    throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            }
            var pos = index;
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

        internal override JSObject Evaluate(Context context)
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
                return body.Evaluate(context);
            }
            else if (elseBody != null)
            {
#if DEV
                if (context.debugging && !(elseBody is CodeBlock))
                    context.raiseDebugger(elseBody);
#endif
                return elseBody.Evaluate(context);
            }
            return null;
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref condition, 2, variables, strict);
            Parser.Optimize(ref body, depth, variables, strict);
            Parser.Optimize(ref elseBody, depth, variables, strict);
            try
            {
                if (condition is Constant || (condition is Expression && (condition as Expression).IsContextIndependent))
                {
                    if ((bool)condition.Evaluate(null))
                        _this = body;
                    else
                        _this = elseBody;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(10, "Error", e.Message);
            }
            if (_this == this && elseBody == null)
                _this = new IfStatement(this);
            return false;
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