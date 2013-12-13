using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal unsafe class None : Operator
    {
        public None(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject temp = null;
            if (second != null)
            {
                var t = (second as ImmidateValueStatement).Value.oValue as Statement[];
                if (t != null)
                {
                    for (int i = 0; i < t.Length; i++)
                    {
                        temp = t[i].Invoke(context);
                        if (temp.ValueType == ObjectValueType.NoExist)
                            throw new InvalidOperationException("varible not defined");
                    }
                }
                else
                {
                    temp = second.Invoke(context);
                    if (temp.ValueType == ObjectValueType.NoExist)
                        throw new InvalidOperationException("varible not defined");
                }
            }
            temp = first.Invoke(context);
            if (temp.ValueType == ObjectValueType.NoExist)
                throw new InvalidOperationException("varible not defined");
            return temp;
        }

        public override bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            if (second == null && !(first is GetVaribleStatement))
            {
                _this = first;
                return true;
            }
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth, vars);
            return false;
        }
    }
}
