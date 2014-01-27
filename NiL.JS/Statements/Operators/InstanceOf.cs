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
            var a = Tools.RaiseIfNotExist(first.Invoke(context));
            var oassc = a.assignCallback;
            a.assignCallback = () => { a = a.Clone() as JSObject; };
            try
            {
                var c = Tools.RaiseIfNotExist(second.Invoke(context));
                if (c.ValueType != JSObjectType.Function)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function.")));
                c = c.GetField("prototype", true, false);
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
            finally
            {
                a.assignCallback = oassc;
            }
        }
    }
}