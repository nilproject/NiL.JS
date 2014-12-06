using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class InstanceOf : Expression
    {
        private static readonly JSObject prototype = "prototype";

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public InstanceOf(Expression first, Expression second)
            : base(first, second, true)
        {
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var a = tempContainer ?? new JSObject { attributes = JSObjectAttributesInternal.Temporary };
                a.Assign(first.Evaluate(context));
                tempContainer = null;
                var c = second.Evaluate(context);
                tempContainer = a;
                if (c.valueType != JSObjectType.Function)
                    throw new JSException(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function."));
                var p = c.GetMember(prototype, false, false);
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