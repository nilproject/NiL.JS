using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class StringConcatenationExpression : Expression
    {
        internal IList<Expression> sources;

        public override bool IsContextIndependent
        {
            get
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i].IsContextIndependent)
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

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public StringConcatenationExpression(IList<Expression> sources)
            : base(null, null, true)
        {
            if (sources.Count < 2)
                throw new ArgumentException("sources too short");
            this.sources = sources;
        }

        private static object prep(JSValue x, ref bool metString)
        {
            if (x.valueType == JSValueType.String)
            {
                metString = true;
                return x.oValue;
            }
            if (x.valueType == JSValueType.Date)
                x = x.ToPrimitiveValue_String_Value();
            else
                x = x.ToPrimitiveValue_Value_String();
            if (x.valueType == JSValueType.String)
            {
                metString = true;
                return x.oValue;
            }
            return x.ToString();
        }

        public override JSValue Evaluate(Context context)
        {
            //lock (this)
            {
                bool metString = false;
                object res = prep(sources[0].Evaluate(context), ref metString);
                for (var i = 1; i < sources.Count; i++)
                    res = new RopeString(res, prep(sources[i].Evaluate(context), ref metString));
                if (!metString)
                    throw new InvalidOperationException("metString == false");
                tempContainer.valueType = JSValueType.String;
                tempContainer.oValue = res;
                return tempContainer;
            }
        }

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, state, message, statistic, opts);
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