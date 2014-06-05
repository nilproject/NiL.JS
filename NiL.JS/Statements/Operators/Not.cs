using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Not : Operator
    {
        public Not(Statement first)
            : base(first, null, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                tempResult.iValue = Tools.JSObjectToInt(first.Invoke(context)) ^ -1;
                tempResult.ValueType = JSObjectType.Int;
                return tempResult;
            }
        }

        public override string ToString()
        {
            return "~" + first;
        }
    }
}