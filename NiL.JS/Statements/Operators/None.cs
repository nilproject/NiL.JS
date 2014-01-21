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
            var outb = context.updateThisBind;
            context.updateThisBind = false;
            JSObject temp = null;
            temp = first.Invoke(context);
            if (temp.ValueType == JSObjectType.NotExist)
            {
                context.updateThisBind = outb;
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("varible not defined")));
            }
            if (second != null)
            {
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.NotExist)
                {
                    context.updateThisBind = outb;
                    throw new InvalidOperationException("varible not defined");
                }
            }
            context.updateThisBind = outb;
            return temp;
        }

        public override bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            if (second == null && !(first is GetVaribleStatement))
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