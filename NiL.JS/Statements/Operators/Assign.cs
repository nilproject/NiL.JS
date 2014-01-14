using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Assign : Operator
    {
        private static JSObject[] setterArgs = new JSObject[1];

        public Assign(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = second.Invoke(context);
            var field = first.InvokeForAssing(context);
            if (field.ValueType == ObjectValueType.Property)
            {
                setterArgs[0] = val;
                (field.oValue as Statement[])[0].Invoke(context, setterArgs);
            }
            else
                field.Assign(val);
            return val;
        }
    }
}
