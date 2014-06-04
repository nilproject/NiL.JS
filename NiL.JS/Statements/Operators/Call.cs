using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Call : Operator
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

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
                    oValue = NiL.JS.Core.Arguments.Instance,
                    attributes = JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum
                };
            if (args == null)
                args = second.Invoke(null).oValue as Statement[];
            JSObject field = args.Length;
            field.assignCallback = null;
            field.attributes = JSObjectAttributes.DontEnum;
            arguments.fields["length"] = field;
            for (int i = 0; i < field.iValue; i++)
            {
                var a = Tools.RaiseIfNotExist(args[i].Invoke(context)).Clone() as JSObject;
                arguments.fields[i < 16 ? Tools.NumString[i] : i.ToString()] = a;
                a.attributes |= JSObjectAttributes.Argument;
                context.objectSource = null;
            }
            arguments.prototype = JSObject.GlobalPrototype;
            arguments.fields["callee"] = field = new JSObject();
            field.ValueType = JSObjectType.Function;
            field.oValue = func;
            field.Protect();
            field.attributes = JSObjectAttributes.DontEnum;
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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            base.Optimize(ref _this, depth, vars);
            args = second.Invoke(null).oValue as Statement[];
            return false;
        }
    }
}