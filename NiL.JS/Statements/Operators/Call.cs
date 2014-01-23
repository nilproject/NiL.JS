using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;

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
            JSObject res = null;
            JSObject newThisBind = null;
            Function func = null;
            JSObject oldThisBind = context.thisBind;
            bool oldutb = context.updateThisBind;
            try
            {
                context.updateThisBind = true;
                context.thisBind = null;
                var temp = first.Invoke(context);
                if (temp.ValueType != JSObjectType.Function)
                    throw new ArgumentException(temp + " is not callable");
                func = (temp.oValue as Function);
                newThisBind = context.thisBind;
            }
            finally
            {
                context.thisBind = oldThisBind;
                context.updateThisBind = oldutb;
            }

            var sps = second.Invoke(context).oValue as Statement[];
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
            length.assignCallback = JSObject.ProtectAssignCallback;
            length.attributes |= ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
            for (int i = 0; i < length.iValue; i++)
            {
                var a = arguments.GetField(i.ToString(), false, false);
                a.Assign(sps[i].Invoke(context));
            }
            if (newThisBind != null)
            {
                context.thisBind = newThisBind;
                try
                {
                    res = func.Invoke(context, arguments);
                    return res;
                }
                finally
                {
                    context.thisBind = oldThisBind;
                }
            }
            else
            {
                res = func.Invoke(arguments);
                return res;
            }
        }
    }
}