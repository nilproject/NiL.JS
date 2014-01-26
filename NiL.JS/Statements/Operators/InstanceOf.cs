using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class InstanceOf : Operator
    {
        public InstanceOf(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke (Context context)
        {
            var a = first.Invoke(context);
            var c = second.Invoke(context).GetField("prototype", true, false);
            tempResult.ValueType = JSObjectType.Bool;
            tempResult.iValue = 0;
            if (c.oValue != null)
                while (a.ValueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == c.oValue || (c.oValue is TypeProxy && a.oValue.GetType() == (c.oValue as TypeProxy).hostedType))
                    {
                        tempResult.iValue = 1;
                        return tempResult;
                    }
                    a = a.GetField("__proto__", true, false);
                }
            return tempResult;
        }
    }
}