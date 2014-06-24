using System;
using System.Collections.Generic;
using NiL.JS.Core;

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

        internal override NiL.JS.Core.JSObject InvokeForAssing(NiL.JS.Core.Context context)
        {
            if (value == JSObject.undefined)
                return value;
            return base.InvokeForAssing(context);
        }

        protected override Statement[] getChildsImpl()
        {
            if (value.Value is Statement[])
                return value.Value as Statement[];
            return null;
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            var vss = value.oValue as Statement[];
            if (vss != null)
            {
                for (int i = 0; i < vss.Length; i++)
                    Parser.Optimize(ref vss[i], depth + 1, fdepth, variables, strict);
            }
            return false;
        }

        public override string ToString()
        {
            if (value.valueType == JSObjectType.String)
                return "\"" + value.oValue + "\"";
            if (value.oValue is Statement[])
            {
                string res = "";
                for (var i = (value.oValue as Statement[]).Length; i-- > 0; )
                    res = (i != 0 ? ", " : "") + (value.oValue as Statement[])[i] + res;
                return res;
            }
            return value.ToString();
        }
    }
}