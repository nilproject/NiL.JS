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

        public readonly Call CallInstance = new Call(new ThisSetStat(), new ImmidateValueStatement(null));

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
                (CallInstance.Second as ImmidateValueStatement).Value = new Statement[0];
            JSObject _this = new JSObject() { ValueType = JSObjectType.Object };
            _this.prototype = temp.GetField("prototype", true, false);
            if (_this.prototype.ValueType > JSObjectType.Undefined && _this.prototype.ValueType < JSObjectType.Object)
                _this.prototype = JSObject.Prototype;
            else
                if (_this.prototype.oValue == null)
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.TypeError("Can't create object.")));
            if (!(temp.oValue is TypeProxyConstructor))
            {
                _this.prototype = _this.prototype.Clone() as JSObject;
                _this.oValue = new object();
            }
            else
                _this.oValue = this;
            (CallInstance.First as ThisSetStat).value = temp;
            (CallInstance.First as ThisSetStat)._this = _this;
            var res = CallInstance.Invoke(context);
            if (res.ValueType >= JSObjectType.Object && res.oValue != null)
                return res;
            return _this;
        }
    }
}