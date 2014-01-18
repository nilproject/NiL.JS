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
                if (temp.ValueType != JSObjectType.Function)
                    throw new ArgumentException(temp + " is not callable");

                JSObject res = null;
                var stat = (temp.oValue as NiL.JS.Core.BaseTypes.Function);
                var args = second.Invoke(context);
                var sps = args.oValue as Statement[];
                var newThisBind = context.thisBind;
                context.thisBind = oldThisBind;
                JSObject arguments = new JSObject(true)
                    {
                        ValueType = JSObjectType.Object,
                        oValue = "[object Arguments]".Clone(),
                        attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum
                    };
                arguments.assignCallback = JSObject.ProtectAssignCallback;
                var length = arguments.GetField("length", false, true);
                length.ValueType = JSObjectType.Int;
                length.iValue = sps == null ? 0 : sps.Length;
                length.Protect();
                length.attributes |= ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
                for (int i = 0; i < length.iValue; i++)
                {
                    var a = arguments.GetField(i.ToString(), false, false);
                    a.Assign(sps[i].Invoke(context));
                    a.attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
                }
                context.thisBind = newThisBind;
                res = stat.Invoke(context, arguments);
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