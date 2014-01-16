using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Call : Operator
    {
        public readonly static Call Instance = new Call(new ImmidateValueStatement(null), new ImmidateValueStatement(null));

        public Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var oldutb = context.updateThisBind;
            context.updateThisBind = true;
            var oldThisBind = context.thisBind;
            try
            {
                var temp = first.Invoke(context);
                if (temp.ValueType != ObjectValueType.Statement)
                    throw new ArgumentException(temp + " is not callable");

                JSObject res = null;
                var stat = (temp.oValue as Statement);
                var args = second.Invoke(context);
                var sps = args.oValue as Statement[];
                if (sps != null)
                {
                    var newThisBind = context.thisBind;
                    context.thisBind = oldThisBind;
                    JSObject stmnts = new JSObject(true)
                        {
                            ValueType = ObjectValueType.Object,
                            oValue = "[object Arguments]".Clone(),
                            attributes = ObjectAttributes.DontDelete
                        };
                    var length = stmnts.GetField("length", false, true);
                    length.ValueType = ObjectValueType.Int;
                    length.iValue = sps.Length;
                    length.Protect();
                    length.attributes |= ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
                    for (int i = 0; i < sps.Length; i++)
                    {
                        var a = stmnts.GetField(i.ToString());
                        a.Assign(sps[i].Invoke(context));
                        a.attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
                    }
                    context.thisBind = newThisBind;
                    res = stat.Invoke(context, stmnts);
                }
                return res;
            }
            finally
            {
                context.thisBind = oldThisBind;
                context.updateThisBind = oldutb;
            }
        }
    }
}