using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class InstanceOf : Expression
    {
        public InstanceOf(CodeNode first, CodeNode second)
            : base(first, second, true)
        {
            tempContainer.assignCallback = null;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var a = tempContainer;
                a.Assign(first.Invoke(context));
                var c = second.Invoke(context);
                if (c.valueType != JSObjectType.Function)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function.")));
                c = c.GetMember("prototype");
                if (c.oValue != null)
                {
                    while (a.valueType >= JSObjectType.Object && a.oValue != null)
                    {
                        if (a.oValue == c.oValue)
                            return true;
                        a = a.GetMember("__proto__");
                    }
                }
                return false;
            }
        }

        public override string ToString()
        {
            return "(" + first + " instanceof " + second + ")";
        }
    }
}