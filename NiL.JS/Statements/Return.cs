using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Return : CodeNode
    {
        private Expression value;

        public Expression Value { get { return value; } }

        internal Return()
        {

        }

        internal Return(Expression value)
        {
            this.value = value;
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "return", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;

            if (state.AllowReturn == 0)
                ExceptionHelper.Throw(new SyntaxError("Invalid use of return statement."));

            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]) && !Tools.IsLineTerminator(state.Code[i]))
                i++;

            var body = state.Code[i] == ';' || Tools.IsLineTerminator(state.Code[i]) ? null : Parser.Parse(state, ref i, CodeFragmentType.Expression);
            var pos = index;
            index = i;
            return new Return()
            {
                value = (Expression)body,
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var result = value != null ? value.Evaluate(context) : null;
            if (context._executionMode == ExecutionMode.None)
            {
                context._executionInfo = result;
                if (context._executionMode < ExecutionMode.Return)
                    context._executionMode = ExecutionMode.Return;
            }

            return JSValue.notExists;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            if (value != null)
                return new CodeNode[] { value };
            return new CodeNode[0];
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref value, expressionDepth + 1, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            // Улучшает работу оптимизатора хвостовой рекурсии
            if (message == null && value is Conditional)
            {
                var bat = value as NiL.JS.Expressions.Conditional;
                var bts = bat.Threads;
                _this = new IfElse(bat.LeftOperand, new Return(bts[0]), new Return(bts[1])) { Position = bat.Position, Length = bat.Length };
                return true;
            }
            else if (value is Call)
            {
                (value as Call).allowTCO = true;
            }

            stats.Returns.Add(value ?? Empty.Instance);

            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (value != null)
            {
                var t = value as CodeNode;
                value.Optimize(ref t, owner, message, opts, stats);
                value = (Expression)t;

                if (value is Empty || ((value is Constant) && value.Evaluate(null) == JSValue.undefined))
                    value = null;
            }
        }

        public override void Decompose(ref CodeNode self)
        {
            if (value != null)
                value.Decompose(ref value);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            value?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

#if !PORTABLE && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var b = value.TryCompile(false, false, null, dynamicValues);
            if (b != null)
                value = new CompiledNode(value, b, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "return" + (value != null ? " " + value : "");
        }
    }
}