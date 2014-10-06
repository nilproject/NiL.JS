using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class StringConcat : Expression
    {
        internal readonly IList<CodeNode> sources;

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

        public StringConcat(IList<CodeNode> sources)
            : base(null, null, true)
        {
            if (sources.Count < 2)
                throw new ArgumentException("sources too short");
            this.sources = sources;
        }

        private static string prep(JSObject x)
        {
            if (x.valueType == JSObjectType.Date)
                return x.ToPrimitiveValue_String_Value().ToString();
            return x.ToPrimitiveValue_Value_String().ToString();
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.valueType = JSObjectType.String;
                if (sources.Count == 2)
                    tempContainer.oValue = string.Concat(
                        prep(sources[0].Evaluate(context)),
                        prep(sources[1].Evaluate(context)));
                else if (sources.Count == 3)
                    tempContainer.oValue = string.Concat(
                        prep(sources[0].Evaluate(context)),
                        prep(sources[1].Evaluate(context)),
                        prep(sources[2].Evaluate(context)));
                else if (sources.Count == 4)
                    tempContainer.oValue = string.Concat(
                        prep(sources[0].Evaluate(context)),
                        prep(sources[1].Evaluate(context)),
                        prep(sources[2].Evaluate(context)),
                        prep(sources[3].Evaluate(context)));
                else
                {
                    var temp = new string[sources.Count];
                    for (var i = 0; i < sources.Count; i++)
                        temp[i] = prep(sources[i].Evaluate(context));
                    tempContainer.oValue = string.Concat(temp);
                }
                return tempContainer;
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