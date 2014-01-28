using NiL.JS.Core;
using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal abstract class Operator : Statement, IOptimizable
    {
        protected readonly JSObject tempResult = new JSObject() { attributes = ObjectAttributes.DontDelete, assignCallback = JSObject.ErrorAssignCallback };

        protected Statement first;
        protected Statement second;

        public Statement First { get { return first; } }
        public Statement Second { get { return second; } }

        protected Operator(Statement first, Statement second)
        {
            this.first = first;
            this.second = second;
        }

        public virtual bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth + 1, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth + 1, vars);
            if (first is ImmidateValueStatement && second is ImmidateValueStatement)
                _this = new ImmidateValueStatement(this.Invoke(null));
            return false;
        }
    }
}