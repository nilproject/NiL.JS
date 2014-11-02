using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class InstanceOf : Expression
    {
        public InstanceOf(CodeNode first, CodeNode second)
            : base(first, second, true)
        {
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var a = tempContainer;
                a.Assign(first.Evaluate(context));
                var c = second.Evaluate(context);
                if (c.valueType != JSObjectType.Function)
                    throw new JSException(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function."));
                var p = c.GetMember("prototype");
                if (p.valueType == JSObjectType.Property)
                    p = ((p.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(c, null);
                if (p.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Property \"prototype\" of function not represent object."));
                if (p.oValue != null)
                {
                    while (a != null && a.valueType >= JSObjectType.Object && a.oValue != null)
                    {
                        if (a.oValue == p.oValue)
                            return true;
                        a = a.__proto__;
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