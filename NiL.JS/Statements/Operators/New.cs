using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class New : Operator
    {
        private class ThisSetStat : Statement
        {
            public JSObject _this;
            public JSObject value;

            public ThisSetStat()
            {

            }

            public override JSObject Invoke(Context context)
            {
                context.thisBind = _this;
                return value;
            }
        }

        public readonly static Call CallInstance = new Call(new ThisSetStat(), new ImmidateValueStatement(null));

        public New(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject temp = first.Invoke(context);
            if (temp.ValueType <= JSObjectType.NotExistInObject)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
            if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(temp + " is not callable")));

            if (second != null)
                (CallInstance.Second as ImmidateValueStatement).Value = second.Invoke(context);
            else
                (CallInstance.Second as ImmidateValueStatement).Value = new JSObject[0];
            JSObject _this = new JSObject() { ValueType = JSObjectType.Object };
            if (temp.oValue is TypeProxyConstructor)
                _this.prototype = temp.GetField("prototype", true, false);
            else
            {
                (_this.prototype = new JSObject()).Assign(temp.GetField("prototype", true, false));
                _this.oValue = new object();
            }
            (CallInstance.First as ThisSetStat).value = temp;
            (CallInstance.First as ThisSetStat)._this = _this;
            var res = CallInstance.Invoke(context);
            if ((bool)res)
                return res;
            return _this;
        }
    }
}