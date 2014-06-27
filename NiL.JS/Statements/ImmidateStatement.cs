using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ImmidateValueStatement : CodeNode
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

        protected override CodeNode[] getChildsImpl()
        {
            if (value.Value is CodeNode[])
                return value.Value as CodeNode[];
            return null;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            var vss = value.oValue as CodeNode[];
            if (vss != null)
            {
                throw new InvalidOperationException("It behaviour is deprecated");
                //for (int i = 0; i < vss.Length; i++)
                //    Parser.Optimize(ref vss[i], depth + 1, fdepth, variables, strict);
            }
            return false;
        }

        public override string ToString()
        {
            if (value.valueType == JSObjectType.String)
                return "\"" + value.oValue + "\"";
            if (value.oValue is CodeNode[])
            {
                string res = "";
                for (var i = (value.oValue as CodeNode[]).Length; i-- > 0; )
                    res = (i != 0 ? ", " : "") + (value.oValue as CodeNode[])[i] + res;
                return res;
            }
            return value.ToString();
        }
    }
}