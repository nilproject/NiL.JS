using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class In : Operator
    {
        public In(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var fn = Tools.RaiseIfNotExist(first.Invoke(context));
            var oassc = fn.assignCallback;
            fn.assignCallback = () => { fn = fn.Clone() as JSObject; };
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

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth + 1, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth + 1, vars);
            return false;
        }
    }
}