using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ImmidateValueStatement : Statement
    {
        internal JSObject value;

        public JSObject Value { get { return value; } }

        public ImmidateValueStatement(JSObject value)
        {
            this.value = value;
        }

        internal override JSObject Invoke(Context context)
        {
            return value;
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            var vss = value.Value as Statement[];
            if (vss != null)
            {
                for (int i = 0; i < vss.Length; i++)
                    Parser.Optimize(ref vss[i], depth + 1, varibles);
            }
            return false;
        }

        public override string ToString()
        {
            if (value.ValueType == JSObjectType.String)
                return "\"" + value.oValue + "\"";
            return value.ToString();
        }
    }
}