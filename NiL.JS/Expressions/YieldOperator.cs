using System;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class YieldOperator : Expression
    {
        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public YieldOperator(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.Assign(first.Evaluate(context));
                context.abortInfo = tempContainer;
                context.abortType = AbortType.Yield;
                context.Deactivate();
                while (context.abortType == AbortType.Yield)
#if !NET35
                    Thread.Yield();
#else
                    Thread.Sleep(0);
#endif
                if (context.abortType == AbortType.Exception)
                    ExceptionsHelper.Throw(new Error("Execution aborted"));
                context.abortType = AbortType.None;
                context.Activate();
                tempContainer.Assign(context.abortInfo ?? JSValue.notExists);
                context.abortInfo = null;
                return tempContainer;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "yield " + first;
        }
    }
}