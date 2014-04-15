using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    public sealed class Call : Operator
    {
        private Statement[] args;

        public Statement[] Arguments { get { return args; } }

        internal Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Invoke(context);
            if (temp.ValueType == JSObjectType.NotExist)
            {
                if (context.thisBind == null)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                else
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(First + " not exist.")));
            }
            if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
            func = temp.oValue as Function;
            newThisBind = context.objectSource;

            JSObject arguments = new JSObject(true)
                {
                    ValueType = JSObjectType.Object,
                    oValue = new Arguments(),
                    attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum
                };
            var field = arguments.GetField("length", false, true);
            field.assignCallback(field);
            field.ValueType = JSObjectType.Int;
            if (args == null)
                args = second.Invoke(null).oValue as Statement[];
            field.iValue = args.Length;
            field.attributes = ObjectAttributes.DontEnum;
            for (int i = 0; i < field.iValue; i++)
            {
                var a = Tools.RaiseIfNotExist(args[i].Invoke(context)).Clone() as JSObject;
                context.objectSource = newThisBind;
                arguments.fields[i.ToString()] = a;
                a.attributes |= ObjectAttributes.Argument;
            }
            arguments.prototype = JSObject.GlobalPrototype;
            arguments.fields["callee"] = field = new JSObject();
            field.ValueType = JSObjectType.Function;
            field.oValue = func;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            return func.Invoke(context, newThisBind, arguments);
        }

        public override string ToString()
        {
            string res = first + "(";
            var args = second.Invoke(null).oValue as Statement[];
            for (int i = 0; i < args.Length; i++)
            {
                res += args[i];
                if (i + 1 < args.Length)
                    res += ", ";
            }
            return res + ")";
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            Parser.Optimize(ref second, depth + 1, vars);
            args = second.Invoke(null).oValue as Statement[];
            return false;
        }
    }
}