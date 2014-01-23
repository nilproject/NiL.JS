using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class New : Operator
    {
        public New(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject temp = first.Invoke(context);
            if (temp.ValueType <= JSObjectType.NotExistInObject)
                throw new ArgumentException("varible is not defined");
            if (temp.ValueType != JSObjectType.Function)
                throw new ArgumentException(temp + " is not callable");

            var call = Operators.Call.Instance;
            (call.First as ImmidateValueStatement).Value = temp;
            if (second != null)
                (call.Second as ImmidateValueStatement).Value = second.Invoke(context);
            else
                (call.Second as ImmidateValueStatement).Value = new JSObject[0];
            JSObject _this = new JSObject() { ValueType = JSObjectType.Object };
            if (temp is TypeProxy)
                _this.prototype = temp.GetField("prototype", true, false);
            else
            {
                (_this.prototype = new JSObject()).Assign(temp.GetField("prototype", true, false));
                _this.oValue = new object();
            }
            var otb = context.thisBind;
            context.thisBind = _this;
            try
            {
                var res = call.Invoke(context);
                if ((bool)res)
                    return res;
            }
            finally
            {
                context.thisBind = otb;
            }
            return _this;
        }
    }
}