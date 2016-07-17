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
            get { return false; }
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
                ExceptionsHelper.Throw(new SyntaxError("Invalid use of yield operator"));

            var i = index;
            if (!Parser.Validate(state.Code, "yield", ref i))
                return null;

            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
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
                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
            }

            index = i;

            return new Expressions.Yield(source, reiterate) { Position = index, Length = i - index };
        }

        public override JSValue Evaluate(Context context)
        {
            if (context.executionMode == AbortReason.ResumeThrow)
            {
                context.SuspendData.Clear();
                context.executionMode = AbortReason.None;
                var exceptionData = context.executionInfo;
                ExceptionsHelper.Throw(exceptionData);
            }

            if (_reiterate)
            {
                if (context.executionMode == AbortReason.None)
                {
                    var iterator = first.Evaluate(context).AsIterable().iterator();
                    var iteratorResult = iterator.next();

                    if (iteratorResult.done)
                        return JSValue.undefined;

                    context.SuspendData[this] = iterator;
                    context.executionInfo = iteratorResult.value;
                    context.executionMode = AbortReason.Suspend;
                    return JSValue.notExists;
                }
                else if (context.executionMode == AbortReason.Resume)
                {
                    IIterator iterator = context.SuspendData[this] as IIterator;
                    var iteratorResult = iterator.next(context.executionInfo.Defined ? new Arguments { context.executionInfo } : null);

                    context.executionInfo = iteratorResult.value;

                    if (iteratorResult.done)
                    {
                        context.executionMode = AbortReason.None;
                        return iteratorResult.value;
                    }
                    else
                    {
                        context.SuspendData[this] = iterator;
                        context.executionMode = AbortReason.Suspend;
                        return JSValue.notExists;
                    }
                }
            }
            else
            {
                if (context.executionMode == AbortReason.None)
                {
                    context.executionInfo = first.Evaluate(context);
                    context.executionMode = AbortReason.Suspend;
                    return JSValue.notExists;
                }
                else if (context.executionMode == AbortReason.Resume)
                {
                    context.executionMode = AbortReason.None;
                    var result = context.executionInfo;
                    context.executionInfo = null;
                    return result;
                }
            }
            throw new InvalidOperationException();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            stats.ContainsYield = true;
            return base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            first.Decompose(ref first, result);

            if ((_codeContext & CodeContext.InExpression) != 0)
            {
                result.Add(new StoreValue(this, false));
                self = new ExtractStoredValue(this);
            }
        }

        public override string ToString()
        {
            return "yield" + (_reiterate ? "* " : " ") + first;
        }
    }
}