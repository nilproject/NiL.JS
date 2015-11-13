using System;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;

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

        public YieldOperator(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            //lock (this)
            {
                var r = first.Evaluate(context).CloneImpl(false);
                context.abortInfo = r;
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

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            result.Add(new StoreValueStatement(this));
            self = new ExtractStoredValueExpression(this);
        }

        public override string ToString()
        {
            return "yield " + first;
        }
    }
}