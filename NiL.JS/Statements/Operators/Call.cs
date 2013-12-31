using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Call : Operator
    {
        public Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var oldutb = context.updateThisBind;
            context.updateThisBind = true;
            var oldThisBind = context.thisBind;
            var temp = first.Invoke(context);
            if (temp.ValueType != ObjectValueType.Statement)
                throw new ArgumentException(temp + " is not callable");

            JSObject res = null;

            var stat = (temp.oValue as Statement);
            var args = second.Invoke(context);
            if (args.oValue is JSObject[])
                res = stat.Invoke(context, args.oValue as JSObject[]);
            else
            {
                var sps = args.oValue as Statement[];
                if (sps != null)
                {
                    var newThisBind = context.thisBind;
                    context.thisBind = oldThisBind;
                    JSObject[] stmnts = sps.Length == 0 ? null as JSObject[] : new JSObject[sps.Length];
                    for (int i = 0; i < sps.Length; i++)
                        stmnts[i] = sps[i].Invoke(context);
                    context.thisBind = newThisBind;
                    res = stat.Invoke(context, stmnts);
                }
            }
            context.thisBind = oldThisBind;
            context.updateThisBind = oldutb;
            return res;
        }
    }
}