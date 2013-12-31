using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    internal abstract class Operator : Statement, IOptimizable
    {
        protected static readonly JSObject tempResult = new JSObject();

        protected Statement first;
        protected Statement second;

        protected Operator(Statement first, Statement second)
        {
            this.first = first;
            this.second = second;
        }

        public override JSObject Invoke(Context context, JSObject _this, JSObject[] args)
        {
            throw new InvalidOperationException();
        }

        public virtual bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth + 1, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth + 1, vars);
            return false;
        }
    }
}