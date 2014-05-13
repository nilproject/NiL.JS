using NiL.JS.Core;
using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public abstract class Operator : Statement
    {
        protected internal readonly JSObject tempResult = new JSObject() { attributes = ObjectAttributes.DontDelete, assignCallback = JSObject.ErrorAssignCallback };

        protected internal Statement first;
        protected internal Statement second;

        public Statement First { get { return first; } }
        public Statement Second { get { return second; } }

        protected Operator(Statement first, Statement second)
        {
            this.first = first;
            this.second = second;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            Parser.Optimize(ref second, depth + 1, vars);
            try
            {
                if (first is ImmidateValueStatement && second is ImmidateValueStatement)
                    _this = new ImmidateValueStatement(this.Invoke(null));
            }
            catch (NullReferenceException)
            { }
            return false;
        }
    }
}