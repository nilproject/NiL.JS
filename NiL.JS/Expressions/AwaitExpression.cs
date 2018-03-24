using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    public sealed class AwaitExpression : Expression
    {
        protected internal override bool NeedDecompose
        {
            get
            {
                return true;
            }
        }

        public AwaitExpression(Expression source)
            : base(source, null, false)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._executionMode == ExecutionMode.ResumeThrow)
            {
                if ((bool)context.SuspendData[this])
                {
                    context._executionMode = ExecutionMode.None;
                    throw new JSException(context._executionInfo);
                }
            }
            else if (context._executionMode == ExecutionMode.Resume)
            {
                if ((bool)context.SuspendData[this])
                {
                    context._executionMode = ExecutionMode.None;
                    return context._executionInfo;
                }
            }

            var result = _left.Evaluate(context);

            if (context._executionMode != ExecutionMode.None)
            {
                if (context._executionMode == ExecutionMode.Suspend)
                    context.SuspendData[this] = false;

                return null;
            }

            if (result != null && (result._valueType < JSValueType.Object || !(result.Value is Promise)))
                return result;

            context._executionMode = ExecutionMode.Suspend;
            context._executionInfo = result;
            context.SuspendData[this] = true;
            return null;
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "await", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;

            if ((state.CodeContext & CodeContext.InAsync) == 0)
                ExceptionHelper.ThrowSyntaxError("await is not allowed in this context", state.Code, index, "await".Length);

            Tools.SkipSpaces(state.Code, ref i);

            var source = ExpressionTree.Parse(state, ref i, false, false, false, true, true);
            if (source == null)
                ExceptionHelper.ThrowSyntaxError("Expression missed", state.Code, i);

            index = i;
            return new AwaitExpression(source);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            stats.NeedDecompose = true;
            return base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            _left.Decompose(ref _left, result);

            if ((_codeContext & CodeContext.InExpression) != 0)
            {
                result.Add(new StoreValue(this, false));
                self = new ExtractStoredValue(this);
            }
        }

        public override string ToString()
        {
            return "await " + _left;
        }
    }
}
