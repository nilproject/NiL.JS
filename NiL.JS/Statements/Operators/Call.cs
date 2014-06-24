using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

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

        private Statement[] arguments;
        public Statement[] Arguments { get { return arguments; } }

        internal Call(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Invoke(context);
            newThisBind = context.objectSource;

            JSObject arguments = new JSObject(false)
                {
                    valueType = JSObjectType.Object,
                    oValue = NiL.JS.Core.Arguments.Instance,
                    attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum
                };
            if (this.arguments == null)
                this.arguments = second.Invoke(null).oValue as Statement[];
            JSObject field = new JSObject();
            field.valueType = JSObjectType.Int;
            field.iValue = this.arguments.Length;
            field.attributes = JSObjectAttributes.DoNotEnum;
            arguments.fields = new Dictionary<string, JSObject>(this.arguments.Length + 3);
            arguments.fields["length"] = field;
            for (int i = 0; i < field.iValue; i++)
            {
                context.objectSource = null;
                var a = this.arguments[i].Invoke(context);
                arguments.fields[i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture)] = a;
            }

            // Аргументы должны быть вычислены даже если функция не существует.
            if (temp.valueType == JSObjectType.NotExist)
            {
                if (context.thisBind == null)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                else
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(FirstOperand + " not exist.")));
            }
            if (temp.valueType != JSObjectType.Function && !(temp.valueType == JSObjectType.Object && temp.oValue is Function))
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
            func = temp.oValue as Function;
            func.lastRequestedName = temp.lastRequestedName;

            var oldCaller = func._caller;
            func._caller = context.caller;
            try
            {
                return func.Invoke(newThisBind, arguments);
            }
            finally
            {
                func._caller = oldCaller;
            }
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

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            base.Optimize(ref _this, depth, fdepth, vars, strict);
            arguments = second.Invoke(null).oValue as Statement[];
            return false;
        }
    }
}