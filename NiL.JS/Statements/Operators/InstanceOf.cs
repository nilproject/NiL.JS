using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class InstanceOf : Operator
    {
        public InstanceOf(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var a = Tools.RaiseIfNotExist(first.Invoke(context));
            var oassc = a.assignCallback;
            a.assignCallback = (sender) => { a = sender.Clone() as JSObject; };
            try
            {
                var c = Tools.RaiseIfNotExist(second.Invoke(context));
                if (c.ValueType != JSObjectType.Function)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function.")));
                c = c.GetField("prototype", true, false);
                tempResult.ValueType = JSObjectType.Bool;
                tempResult.iValue = 0;
                if (c.oValue != null)
                {
                    bool tpmode = c.oValue is TypeProxy;
                    Type type = null;
                    if (tpmode)
                        type = (c.oValue as TypeProxy).hostedType;
                    while (a.ValueType >= JSObjectType.Object && a.oValue != null)
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

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            Parser.Optimize(ref second, depth + 1, vars);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " instanceof " + second + ")";
        }
    }
}