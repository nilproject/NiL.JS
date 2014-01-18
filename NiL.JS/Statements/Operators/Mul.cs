using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal sealed class Mul : Operator
    {
        public Mul(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject temp = first.Invoke(context);
            var res = tempResult;
            double dr;
            switch (temp.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        dr = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    dr *= temp.iValue;
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case JSObjectType.Double:
                                {
                                    dr *= temp.dValue;
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case JSObjectType.Function:
                            case JSObjectType.Object:
                                {
                                    if (temp.oValue != null)
                                        temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    else if (temp.ValueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    else if ((temp.ValueType == JSObjectType.Object)
                                        || (temp.ValueType == JSObjectType.String)
                                        || (temp.ValueType == JSObjectType.Undefined)
                                        || (temp.ValueType == JSObjectType.Function))
                                    {
                                        res.ValueType = JSObjectType.Double;
                                        if (temp.oValue == null)
                                            res.dValue = dr * 0.0;
                                        else
                                            res.dValue = double.NaN;
                                        return res;
                                    }
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    var d = double.NaN;
                                    int n = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                                        d = double.NaN;
                                    tempResult.dValue = d;
                                    temp = tempResult;
                                    goto case JSObjectType.Double;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = double.NaN;
                                    return res;
                                }
                        }
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dr = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Int:
                                {
                                    dr *= temp.iValue;
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case JSObjectType.Double:
                                {
                                    dr *= temp.dValue;
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    else if (temp.ValueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    else if (temp.ValueType == JSObjectType.Object)
                                    {
                                        res.ValueType = JSObjectType.Double;
                                        res.dValue = double.NaN;
                                        return res;
                                    }
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    var d = double.NaN;
                                    int n = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                                        d = double.NaN;
                                    tempResult.dValue = d;
                                    temp = tempResult;
                                    goto case JSObjectType.Double;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    res.ValueType = JSObjectType.Double;
                                    res.dValue = double.NaN;
                                    return res;
                                }
                        }
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        else if (temp.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (temp.ValueType == JSObjectType.Object 
                            || temp.ValueType == JSObjectType.NotExistInObject 
                            || temp.ValueType == JSObjectType.Undefined)
                        {
                            res.ValueType = JSObjectType.Double;
                            res.dValue = double.NaN;
                            return res;
                        }
                        else if (temp.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var d = double.NaN;
                        int n = 0;
                        if (Tools.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                            d = double.NaN;
                        tempResult.dValue = d;
                        temp = tempResult;
                        goto case JSObjectType.Double;
                    }
            }
            throw new NotImplementedException();
        }
    }
}