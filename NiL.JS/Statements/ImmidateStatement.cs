using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    internal sealed class ImmidateValueStatement : Statement, IOptimizable
    {
        internal JSObject Value;

        public ImmidateValueStatement(JSObject value)
        {
            Value = value;
        }

        public override JSObject Invoke(Context context)
        {
            return Value;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            var vss = Value.Value as Statement[];
            if (vss != null)
            {
                for (int i = 0; i < vss.Length; i++)
                    Parser.Optimize(ref vss[i], depth + 1, varibles);
            }
            return false;
        }

        public override string ToString()
        {
            return "<Object>";
        }
    }
}