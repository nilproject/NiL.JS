using System;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class YieldOperator : Expression
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

        public YieldOperator(Expression first, bool reiterate)
            : base(first, null, true)
        {
            _reiterate = reiterate;
        }

        public static CodeNode Parse(ParsingState state, ref int index)
        {
            if ((state.CodeContext & CodeContext.InGenerator) == 0)
                ExceptionsHelper.Throw(new SyntaxError("Invalid use of yield operator"));

            var i = index;
            if (!Parser.Validate(state.Code, "yield", ref i))
                return null;

            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            bool reiterate = false;
            if (state.Code[i] == '*')
            {
                reiterate = true;
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
            }

            var source = ExpressionTree.Parse(state, ref i, false, false, false, true, false, true);
            if (source == null)
            {
                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
            }

            index = i;

            return new Expressions.YieldOperator(source, reiterate) { Position = index, Length = i - index };
        }

        public override JSValue Evaluate(Context context)
        {
            if (context.abortType == AbortType.ResumeThrow)
            {
                context.SuspendData.Clear();
                context.abortType = AbortType.None;
                var exceptionData = context.abortInfo;
                ExceptionsHelper.Throw(exceptionData);
            }

            if (_reiterate)
            {
                if (context.abortType == AbortType.None)
                {
                    var iterator = first.Evaluate(context).AsIterable().iterator();
                    var iteratorResult = iterator.next();

                    if (iteratorResult.done)
                        return JSValue.undefined;

                    context.SuspendData[this] = iterator;
                    context.abortInfo = iteratorResult.value;
                    context.abortType = AbortType.Suspend;
                    return JSValue.notExists;
                }
                else if (context.abortType == AbortType.Resume)
                {
                    IIterator iterator = context.SuspendData[this] as IIterator;
                    var iteratorResult = iterator.next(context.abortInfo.IsDefined ? new Arguments { context.abortInfo } : null);

                    context.abortInfo = iteratorResult.value;

                    if (iteratorResult.done)
                    {
                        context.abortType = AbortType.None;
                        return iteratorResult.value;
                    }
                    else
                    {
                        context.SuspendData[this] = iterator;
                        context.abortType = AbortType.Suspend;
                        return JSValue.notExists;
                    }
                }
            }
            else
            {
                if (context.abortType == AbortType.None)
                {
                    context.abortInfo = first.Evaluate(context);
                    context.abortType = AbortType.Suspend;
                    return JSValue.notExists;
                }
                else if (context.abortType == AbortType.Resume)
                {
                    context.abortType = AbortType.None;
                    var result = context.abortInfo;
                    context.abortInfo = null;
                    return result;
                }
            }
            throw new InvalidOperationException();
        }

        protected internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            statistic.ContainsYield = true;
            return base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            first.Decompose(ref first, result);

            if ((_codeContext & CodeContext.InExpression) != 0)
            {
                result.Add(new StoreValueStatement(this, false));
                self = new ExtractStoredValueExpression(this);
            }
        }

        public override string ToString()
        {
            return "yield" + (_reiterate ? "* " : " ") + first;
        }
    }
}