using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal class Call : Operator
    {
        private Statement[] args;

        public Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            JSObject oldThisBind = context.thisBind;
            bool oldutb = context.updateThisBind;
            try
            {
                context.updateThisBind = true;
                context.thisBind = null;
                var temp = first.Invoke(context);
                if (temp.ValueType == JSObjectType.NotExist)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(temp + " is not callable")));
                func = temp.oValue as Function ?? (Function)((TypeProxy)temp.oValue);
                newThisBind = context.thisBind;
            }
            finally
            {
                context.thisBind = oldThisBind;
                context.updateThisBind = oldutb;
            }

            JSObject arguments = new JSObject(true)
                {
                    ValueType = JSObjectType.Object,
                    oValue = "[object Arguments]".Clone(),
                    attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum,
                    prototype = BaseObject.Prototype
                };
            var field = arguments.GetField("length", false, true);
            field.ValueType = JSObjectType.Int;
            if (args == null)
                args = second.Invoke(null).oValue as Statement[];
            field.iValue = args.Length;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            for (int i = 0; i < field.iValue; i++)
            {
                var a = arguments.GetField(i.ToString(), false, false);
                a.Assign(Tools.RaiseIfNotExist(args[i].Invoke(context)));
            }
            field = arguments.GetField("callee", false, true);
            field.ValueType = JSObjectType.Function;
            field.oValue = func;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            if (func is ExternalFunction)
                return func.Invoke(context, newThisBind, arguments);
            else
                return func.Invoke(newThisBind, arguments);
        }

        public override string ToString()
        {
            return first + "(" + second + ")";
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth + 1, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth + 1, vars);
            args = second.Invoke(null).oValue as Statement[];
            return false;
        }
    }
}