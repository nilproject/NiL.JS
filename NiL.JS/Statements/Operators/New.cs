using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class New : Operator
    {
        private sealed class ThisSetStat : Statement
        {
            public JSObject _this;
            public JSObject value;

            public ThisSetStat()
            {

            }

            protected override Statement[] getChildsImpl()
            {
                throw new InvalidOperationException();
            }

            internal override JSObject Invoke(Context context)
            {
                context.objectSource = _this;
                return value;
            }
        }

        private readonly Call CallInstance = new Call(new ThisSetStat(), new ImmidateValueStatement(null));

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public New(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                JSObject temp = first.Invoke(context);
                if (temp.ValueType <= JSObjectType.NotExistInObject)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(temp + " is not callable")));
                if (temp.oValue is MethodProxy)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(temp + " can't be used as a constructor")));

                JSObject _this = new JSObject() { ValueType = JSObjectType.Object };
                _this.prototype = temp.GetField("prototype", true, false);
                if (_this.prototype.ValueType < JSObjectType.Object)
                    _this.prototype = null;
                if (!(temp.oValue is TypeProxyConstructor))
                {
                    if (_this.prototype != null)
                        _this.prototype = _this.prototype.Clone() as JSObject;
                    _this.oValue = _this;
                }
                else
                    _this.oValue = this;
                (CallInstance.FirstOperand as ThisSetStat).value = temp;
                (CallInstance.FirstOperand as ThisSetStat)._this = _this;
                var res = CallInstance.Invoke(context);
                if (res.ValueType >= JSObjectType.Object && res.oValue != null)
                    return res;
                return _this;
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> vars)
        {
            if (second == null)
                (CallInstance.SecondOperand as ImmidateValueStatement).value = new JSObject() { ValueType = JSObjectType.Object, oValue = new Statement[0] };
            else
                (CallInstance.SecondOperand as ImmidateValueStatement).value = second.Invoke(null);
            return base.Optimize(ref _this, depth, vars);
        }

        public override string ToString()
        {
            string res = "new " + first + "(";
            var args = (CallInstance.SecondOperand as ImmidateValueStatement).value.oValue as Statement[];
            for (int i = 0; i < args.Length; i++)
            {
                res += args[i];
                if (i + 1 < args.Length)
                    res += ", ";
            }
            return res + ")";
        }
    }
}