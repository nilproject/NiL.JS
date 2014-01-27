using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    internal class None : Operator
    {
        public None(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject temp = null;
            var otb = context.thisBind;
            temp = first.Invoke(context);
            if (temp.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("varible not defined")));
            if (second != null)
            {
                context.thisBind = otb;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.NotExist)
                    throw new InvalidOperationException("varible not defined");
            }
            return temp;
        }

        public override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            if (second == null && (depth > 1 || !(first is GetVaribleStatement)))
            {
                _this = first;
                return true;
            }
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth, vars);
            return false;
        }
    }
}