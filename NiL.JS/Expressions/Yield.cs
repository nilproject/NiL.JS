using System;
using NiL.JS.Core;
using System.Threading;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Yield : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Yield(CodeNode first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.Assign(first.Evaluate(context));
                context.abortInfo = tempContainer;
#pragma warning disable 618
                Thread.CurrentThread.Suspend();
#pragma warning restore
                tempContainer.Assign(context.abortInfo ?? JSObject.notExists);
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "yield " + first;
        }
    }
}