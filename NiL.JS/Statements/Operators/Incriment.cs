using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    internal sealed class Incriment : Operator
    {
        public Incriment(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = (first ?? second).Invoke(context);
            if ((val.assignCallback != null) && (!val.assignCallback()))
                return double.NaN;

            JSObject o = null;
            if ((second != null) && (val.ValueType != JSObjectType.Undefined) && (val.ValueType != JSObjectType.NotExistInObject))
            {
                o = tempResult;
                o.Assign(val);
            }
            else
                o = val;
            @switch:
            switch (val.ValueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        val.ValueType = JSObjectType.Int;
                        val.iValue++;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        val.dValue++;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double resd;
                        int i = 0;
                        if (!Tools.ParseNumber(val.oValue as string, ref i, false, out resd))
                            resd = double.NaN;
                        resd++;
                        val.ValueType = JSObjectType.Double;
                        val.dValue = resd;
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        val.Assign(val.ToPrimitiveValue_Value_String(context));
                        goto @switch;
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        val.ValueType = JSObjectType.Double;
                        val.dValue = double.NaN;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.Error("Varible not defined.")));
                default:
                    throw new NotImplementedException();
            }
            return o;
        }

        public override bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            base.Optimize(ref _this, depth, vars);
            if (depth <= 1 && second != null)
            {
                first = second;
                second = null;
            }
            return false;
        }
    }
}