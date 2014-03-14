using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class In : Operator
    {
        public In(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var fn = Tools.RaiseIfNotExist(first.Invoke(context));
            var oassc = fn.assignCallback;
            fn.assignCallback = (sender) => { fn = fn.Clone() as JSObject; };
            try
            {
                var source = Tools.RaiseIfNotExist(second.Invoke(context));
                if (source.ValueType < JSObjectType.Object)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Right-hand value of instanceof is not object.")));
                var t = source.GetField(fn.ToString(), true, false);
                tempResult.iValue = t != JSObject.undefined && t.ValueType >= JSObjectType.Undefined ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            finally
            {
                fn.assignCallback = oassc;
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            Parser.Optimize(ref second, depth + 1, vars);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}