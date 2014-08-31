using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Call : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private CodeNode[] arguments;
        public CodeNode[] Arguments { get { return arguments; } }

        internal Call(CodeNode first, CodeNode[] arguments)
            : base(first, null, false)
        {
            this.arguments = arguments;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Evaluate(context);
            newThisBind = context.objectSource;

            Arguments arguments = new Arguments()
            {
                length = this.arguments.Length
            };
            for (int i = 0; i < arguments.length; i++)
            {
                context.objectSource = null;
                var a = this.arguments[i].Evaluate(context);
                if ((a.attributes & JSObjectAttributesInternal.Temporary) != 0)
                {
                    a = a.CloneImpl();
                    a.attributes |= JSObjectAttributesInternal.Cloned;
                }
#if DEBUG
                if (a == null)
                    System.Diagnostics.Debugger.Break();
#endif
                arguments[i] = a;
            }
            context.objectSource = null;

            // Аргументы должны быть вычислены даже если функция не существует.
            if (temp.valueType != JSObjectType.Function && !(temp.valueType == JSObjectType.Object && temp.oValue is Function))
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
            func = temp.oValue as Function;
            func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);

            var oldCaller = func._caller;
            func._caller = context.caller.creator.body.strict ? Function.propertiesDummySM : context.caller;
            try
            {
                return func.Invoke(newThisBind, arguments);
            }
            finally
            {
                func._caller = oldCaller;
            }
        }

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (var i = 0; i < arguments.Length; i++)
                Parser.Optimize(ref arguments[i], depth + 1, vars, strict);
            return base.Optimize(ref _this, depth, vars, strict);
        }

        public override string ToString()
        {
            string res = first + "(";
            for (int i = 0; i < arguments.Length; i++)
            {
                res += arguments[i];
                if (i + 1 < arguments.Length)
                    res += ", ";
            }
            return res + ")";
        }
    }
}