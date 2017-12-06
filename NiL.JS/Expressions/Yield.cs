using System;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Yield : Expression
    {
        private bool _reiterate;

        internal override bool ResultInTempContainer
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return true;
            }
        }

        public bool Reiterate
        {
            get
            {
                return _reiterate;
            }
        }

        public Yield(Expression first, bool reiterate)
            : base(first, null, true)
        {
            _reiterate = reiterate;
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            if ((state.CodeContext & CodeContext.InGenerator) == 0)
                ExceptionHelper.Throw(new SyntaxError("Invalid use of yield operator"));

            var i = index;
            if (!Parser.Validate(state.Code, "yield", ref i))
                return null;

            Tools.SkipSpaces(state.Code, ref i);

            bool reiterate = false;
            if (state.Code[i] == '*')
            {
                reiterate = true;
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
            }

            var source = ExpressionTree.Parse(state, ref i, false, false, false, true, true);
            if (source == null)
            {
                ExceptionHelper.ThrowSyntaxError("Invalid prefix operation", state.Code, i);
            }

            index = i;

            return new Yield(source, reiterate) { Position = index, Length = i - index };
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._executionMode == ExecutionMode.ResumeThrow)
            {
                context.SuspendData.Clear();
                context._executionMode = ExecutionMode.None;
                var exceptionData = context._executionInfo;
                ExceptionHelper.Throw(exceptionData);
            }

            if (_reiterate)
            {
                if (context._executionMode == ExecutionMode.None)
                {
                    var iterator = _left.Evaluate(context).AsIterable().iterator();
                    var iteratorResult = iterator.next();

                    if (iteratorResult.done)
                        return JSValue.undefined;

                    context.SuspendData[this] = iterator;
                    context._executionInfo = iteratorResult.value;
                    context._executionMode = ExecutionMode.Suspend;
                    return JSValue.notExists;
                }
                else if (context._executionMode == ExecutionMode.Resume)
                {
                    IIterator iterator = context.SuspendData[this] as IIterator;
                    var iteratorResult = iterator.next(context._executionInfo.Defined ? new Arguments { context._executionInfo } : null);

                    context._executionInfo = iteratorResult.value;

                    if (iteratorResult.done)
                    {
                        context._executionMode = ExecutionMode.None;
                        return iteratorResult.value;
                    }
                    else
                    {
                        context.SuspendData[this] = iterator;
                        context._executionMode = ExecutionMode.Suspend;
                        return JSValue.notExists;
                    }
                }
            }
            else
            {
                if (context._executionMode == ExecutionMode.None)
                {
                    context._executionInfo = _left.Evaluate(context);
                    context._executionMode = ExecutionMode.Suspend;
                    return JSValue.notExists;
                }
                else if (context._executionMode == ExecutionMode.Resume)
                {
                    context._executionMode = ExecutionMode.None;
                    var result = context._executionInfo;
                    context._executionInfo = null;
                    return result;
                }
            }
            throw new InvalidOperationException();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            stats.NeedDecompose = true;
            return base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
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
            return "yield" + (_reiterate ? "* " : " ") + _left;
        }
    }
}