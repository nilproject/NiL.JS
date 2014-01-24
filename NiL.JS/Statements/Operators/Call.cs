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
                if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(temp + " is not callable")));
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
                    attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum,
                    prototype = BaseObject.Prototype
                };
            var field = arguments.GetField("length", false, true);
            field.ValueType = JSObjectType.Int;
            field.iValue = sps == null ? 0 : sps.Length;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            for (int i = 0; i < field.iValue; i++)
            {
                var a = arguments.GetField(i.ToString(), false, false);
                a.Assign(sps[i].Invoke(context));
            }
            field = arguments.GetField("callee", false, true);
            field.ValueType = JSObjectType.Function;
            field.oValue = func;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            if (newThisBind != null || func is ExternalFunction)
            {
                context.thisBind = newThisBind ?? oldThisBind;
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
                res = func.Invoke(oldThisBind, arguments);
                return res;
            }
        }
    }
}