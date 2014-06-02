using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;
using System.Collections.Generic;

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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
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
            if (value.oValue is Statement[])
            {
                string res = "";
                for (var i = (value.oValue as Statement[]).Length; i-- > 0; )
                    res += (value.oValue as Statement[])[i] + (i != 0 ? ", " : "");
                return res;
            }
            return value.ToString();
        }
    }
}