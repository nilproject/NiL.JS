using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Assign : Operator
    {
        private JSObject setterArgs = new JSObject(true) { ValueType = JSObjectType.Object, oValue = new Arguments() };
        private JSObject setterArg = new JSObject();

        public Assign(Statement first, Statement second)
            : base(first, second)
        {
            setterArgs.fields["length"] = new JSObject()
            {
                iValue = 1,
                ValueType = JSObjectType.Int,
                attributes = ObjectAttributes.DontEnum | ObjectAttributes.DontDelete | ObjectAttributes.ReadOnly
            };
            setterArgs.fields["0"] = setterArg;
        }

        public override JSObject Invoke(Context context)
        {
            JSObject field = null;
            field = first.InvokeForAssing(context);
            if (field.ValueType == JSObjectType.Property)
            {
                var fieldSource = context.objectSource;
                setterArg.Assign(Tools.RaiseIfNotExist(second.Invoke(context)));
                var setter = (field.oValue as NiL.JS.Core.BaseTypes.Function[])[0];
                var otb = context.thisBind;
                context.thisBind = fieldSource;
                try
                {
                    if (setter != null)
                        setter.Invoke(context, setterArgs);
                    return setterArg;
                }
                finally
                {
                    context.thisBind = otb;
                    context.objectSource = null;
                }
            }
            if (context.strict)
                Tools.RaiseIfNotExist(field);
            var t = second.Invoke(context);
            if (t.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible is not defined.")));
            field.Assign(t);
            return t;
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            var res = base.Optimize(ref _this, depth, vars);
            var t = first;
            while (t is Operators.None)
                t = (t as Operators.None).Second;
            if (t is Operators.Call)
                throw new InvalidOperationException("Invalid left-hand side in assignment.");
            return res;
        }

        public override string ToString()
        {
            return "(" + first + " = " + second + ")";
        }
    }
}