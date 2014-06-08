using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class InstanceOf : Operator
    {
        public InstanceOf(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var a = Tools.RaiseIfNotExist(first.Invoke(context));
                var oassc = a.assignCallback;
                a.assignCallback = (sender) => { a = sender.Clone() as JSObject; };
                try
                {
                    var c = Tools.RaiseIfNotExist(second.Invoke(context));
                    if (c.valueType != JSObjectType.Function)
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function.")));
                    c = c.GetField("prototype", true, false);
                    tempResult.valueType = JSObjectType.Bool;
                    tempResult.iValue = 0;
                    if (c.oValue != null)
                    {
                        bool tpmode = c.oValue is TypeProxy;
                        Type type = null;
                        if (tpmode)
                            type = (c.oValue as TypeProxy).hostedType;
                        while (a.valueType >= JSObjectType.Object && a.oValue != null)
                        {
                            if (a.oValue == c.oValue || (tpmode && a.oValue is TypeProxy && (a.oValue as TypeProxy).hostedType == type))
                            {
                                tempResult.iValue = 1;
                                return tempResult;
                            }
                            a = a.GetField("__proto__", true, false);
                        }
                    }
                    return tempResult;
                }
                finally
                {
                    a.assignCallback = oassc;
                }
            }
        }

        public override string ToString()
        {
            return "(" + first + " instanceof " + second + ")";
        }
    }
}