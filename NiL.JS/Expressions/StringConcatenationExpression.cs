using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class StringConcatenationExpression : Expression
    {
        internal IList<Expression> sources;

        protected internal override bool ContextIndependent
        {
            get
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (!sources[i].ContextIndependent)
                        return false;
                }
                return true;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i].NeedDecompose)
                        return true;
                }
                return false;
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

        private static object prep(JSValue x)
        {
            if (x.valueType == JSValueType.String)
            {
                return x.oValue;
            }
            if (x.valueType == JSValueType.Date)
                x = x.ToPrimitiveValue_String_Value();
            else
                x = x.ToPrimitiveValue_Value_String();
            if (x.valueType == JSValueType.String)
            {
                return x.oValue;
            }
            return x.ToString();
        }

        public override JSValue Evaluate(Context context)
        {
            object res = prep(sources[0].Evaluate(context));
            for (var i = 1; i < sources.Count; i++)
                res = new RopeString(res, prep(sources[i].Evaluate(context)));
            tempContainer.valueType = JSValueType.String;
            tempContainer.oValue = res;
            return tempContainer;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
            if (!res)
                second = sources[sources.Count - 1];
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            for (var i = 0; i < sources.Count; i++)
            {
                Expression s = sources[i];
                sources[i].Decompose(ref s, result);
                sources[i] = s;
                if (sources[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(sources[i] is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(sources[i], false));
                    sources[i] = new ExtractStoredValueExpression(sources[i]);
                }
            }
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