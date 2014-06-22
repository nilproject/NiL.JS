using System;
using NiL.JS.Core;

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
                tempContainer.iValue = Tools.JSObjectToInt32(first.Invoke(context)) ^ -1;
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "~" + first;
        }
    }
}