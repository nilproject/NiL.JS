using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class None : Operator
    {
        public None(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            if (second == null)
            {
                _this = first;
                return true;
            }
            Parser.Optimize(ref first, depth, vars);
            Parser.Optimize(ref second, depth, vars);
            return false;
        }
    }
}