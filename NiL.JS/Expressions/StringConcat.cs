using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class StringConcat : Expression
    {
        internal IList<Expression> sources;

        public override bool IsContextIndependent
        {
            get
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (!(sources[i] is Expression)
                        || !(sources[i] as Expression).IsContextIndependent)
                        return false;
                }
                return true;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        public StringConcat(IList<Expression> sources)
            : base(null, null, true)
        {
            if (sources.Count < 2)
                throw new ArgumentException("sources too short");
            this.sources = sources;
        }

        private static object prep(JSObject x, ref bool metString)
        {
            if (x.valueType == JSObjectType.String && (metString |= true))
                return x.oValue;
            if (x.valueType == JSObjectType.Date)
                x = x.ToPrimitiveValue_String_Value();
            else
                x = x.ToPrimitiveValue_Value_String();
            if (x.valueType == JSObjectType.String && (metString |= true))
                return x.oValue;
            return x.ToString();
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                bool metString = false;
                object res = prep(sources[0].Evaluate(context), ref metString);
                for (var i = 1; i < sources.Count; i++)
                    res = new RopeString(res, prep(sources[i].Evaluate(context), ref metString));
                if (!metString)
                    throw new InvalidOperationException("metString == false");
                tempContainer.valueType = JSObjectType.String;
                tempContainer.oValue = res;
                return tempContainer;
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, OptimizationOptions opts)
        {
            var res = base.Build(ref _this, depth, vars, strict, message, statistic, opts);
            if (!res)
                second = sources[sources.Count - 1];
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("(", sources.Count * 10).Append(sources[0]);
            for (var i = 1; i < sources.Count; i++)
                res.Append(" + ").Append(sources[i]);
            return res.Append(")").ToString();
        }
    }
}