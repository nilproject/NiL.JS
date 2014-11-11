using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class OpAssignCache : CodeNode
    {
        private JSObject result;
        private CodeNode source;

        public CodeNode Source { get { return source; } }

        internal OpAssignCache(CodeNode source)
        {
            this.source = source;
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            var res = source.EvaluateForAssing(context);
            if (res.valueType == JSObjectType.Property)
                result = (res.oValue as PropertyPair).get != null ? (res.oValue as PropertyPair).get.Invoke(context.objectSource, null) : JSObject.notExists;
            else
                result = res;
            return res;
        }

        internal override JSObject Evaluate(Context context)
        {
            var res = result;
            result = null;
            return res;
        }

        public override string ToString()
        {
            return source.ToString();
        }

        public override int EndPosition
        {
            get
            {
                return source.EndPosition;
            }
        }

        public override int Length
        {
            get
            {
                return source.Length;
            }
            internal set
            {
                source.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return source.Position;
            }
            internal set
            {
                source.Position = value;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            return source.Childs;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            var res = source.Build(ref _this, depth, variables, strict);
            if (!res && source is GetVariableStatement)
                (source as GetVariableStatement).forceThrow = true;
            return res;
        }
    }
}
