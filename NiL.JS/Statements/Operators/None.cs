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
            temp = Tools.RaiseIfNotExist(first.Invoke(context));
            if (second != null)
            {
                context.thisBind = null;
                temp = Tools.RaiseIfNotExist(second.Invoke(context));
            }
            context.objectSource = null;
            return temp;
        }

        public override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            if (second == null)
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