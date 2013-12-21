using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class StrictNotEqual : Operator
    {
        private StrictEqual proto;

        public StrictNotEqual(Statement first, Statement second)
            : base(first, second)
        {
            proto = new StrictEqual(first, second);
        }

        public override JSObject Invoke(Context context)
        {
            var t = proto.Invoke(context);
            t.iValue ^= 1;
            return t;
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> vars)
        {
            proto.Optimize(ref _this, depth + 1, vars);
            return base.Optimize(ref _this, depth, vars);
        }
    }
}