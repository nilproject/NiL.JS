using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class LogicalNot : Operator
    {
        public LogicalNot(Statement first)
            : base(first, null)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var val = first.Invoke(context);

                tempResult.iValue = (bool)val ? 0 : 1;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}