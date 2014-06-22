using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class UnsignedShiftLeft : Operator
    {
        public UnsignedShiftLeft(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt32(first.Invoke(context));
                tempContainer.iValue = (int)((uint)left << Tools.JSObjectToInt32(second.Invoke(context)));
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " <<< " + second + ")";
        }
    }
}