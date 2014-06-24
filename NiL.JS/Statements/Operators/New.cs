using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

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

        private static readonly JSObject newMarker = new JSObject() { valueType = JSObjectType.Object };
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
                JSObject ctor = first.Invoke(context);
                if (ctor.valueType <= JSObjectType.NotExistInObject)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                if (ctor.valueType != JSObjectType.Function && !(ctor.valueType == JSObjectType.Object && ctor.oValue is Function))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(ctor + " is not callable")));
                if (ctor.oValue is MethodProxy)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(ctor + " can't be used as a constructor")));
                if (ctor.oValue is EvalFunction
                    || ctor.oValue is ExternalFunction
                    || ctor.oValue is MethodProxy)
                    throw new JSException(new TypeError("Function \"" + (ctor.oValue as Function).Name + "\" is not a constructor."));

                JSObject _this = null;
                if (!(ctor.oValue is ProxyConstructor))
                {
                    _this = new JSObject(true) { valueType = JSObjectType.Object };
                    _this.__proto__ = ctor.GetMember("prototype");
                    if (_this.__proto__.valueType < JSObjectType.Object)
                        _this.__proto__ = null;
                    else
                        _this.__proto__ = _this.__proto__.Clone() as JSObject;
                    _this.oValue = _this;
                }
                else
                    _this = newMarker;

                (CallInstance.FirstOperand as ThisSetStat).value = ctor;
                (CallInstance.FirstOperand as ThisSetStat)._this = _this;
                var res = CallInstance.Invoke(context);
                if (res.valueType >= JSObjectType.Object && res.oValue != null)
                    return res;
#if DEBUG
                System.Diagnostics.Debug.Assert(_this != newMarker, "_this == newMarker");
#endif
                return _this;
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            newMarker.oValue = this;    // Достаточно один раз туда поставить какой-нибудь
            // экземпляр оператора New, так как дальше проверка идёт только по "is"
            if (second == null)
                (CallInstance.SecondOperand as ImmidateValueStatement).value = new JSObject() { valueType = JSObjectType.Object, oValue = new Statement[0] };
            else
                (CallInstance.SecondOperand as ImmidateValueStatement).value = second.Invoke(null);
            return base.Optimize(ref _this, depth, fdepth, vars, strict);
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