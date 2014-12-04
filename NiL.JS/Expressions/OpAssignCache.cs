using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class OpAssignCache : Expression
    {
        private JSObject result;
        private CodeNode source;

        public CodeNode Source { get { return source; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

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
            if (!res && source is GetVariableExpression)
                (source as GetVariableExpression).forceThrow = true;
            return res;
        }
    }
}
