using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Division : Operator
    {
        public Division(Statement first, Statement second)
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
                case ObjectValueType.Int:
                    {
                        dr = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Int:
                                {
                                    dr /= temp.iValue;
                                    res.ValueType = ObjectValueType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case ObjectValueType.Double:
                                {
                                    dr /= temp.dValue;
                                    res.ValueType = ObjectValueType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case ObjectValueType.Statement:
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == ObjectValueType.Int)
                                        goto case ObjectValueType.Int;
                                    else if (temp.ValueType == ObjectValueType.Double)
                                        goto case ObjectValueType.Double;
                                    else if ((temp.ValueType == ObjectValueType.Object)
                                        || (temp.ValueType == ObjectValueType.String)
                                        || (temp.ValueType == ObjectValueType.Undefined)
                                        || (temp.ValueType == ObjectValueType.Statement))
                                    {
                                        res.ValueType = ObjectValueType.Double;
                                        res.dValue = double.NaN;
                                        return res;
                                    }
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    var d = double.NaN;
                                    int n = 0;
                                    if (Parser.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                                        d = double.NaN;
                                    tempResult.dValue = d;
                                    temp = tempResult;
                                    goto case ObjectValueType.Double;
                                }
                        }
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dr = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Int:
                                {
                                    dr /= temp.iValue;
                                    res.ValueType = ObjectValueType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case ObjectValueType.Double:
                                {
                                    dr /= temp.dValue;
                                    res.ValueType = ObjectValueType.Double;
                                    res.dValue = dr;
                                    return res;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == ObjectValueType.Int)
                                        goto case ObjectValueType.Int;
                                    else if (temp.ValueType == ObjectValueType.Double)
                                        goto case ObjectValueType.Double;
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        res.ValueType = ObjectValueType.Double;
                                        res.dValue = double.NaN;
                                        return res;
                                    }
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    var d = double.NaN;
                                    int n = 0;
                                    if (Parser.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                                        d = double.NaN;
                                    tempResult.dValue = d;
                                    temp = tempResult;
                                    goto case ObjectValueType.Double;
                                }
                        }
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        else if (temp.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        else if (temp.ValueType == ObjectValueType.Object)
                        {
                            res.ValueType = ObjectValueType.Double;
                            res.dValue = double.NaN;
                            return res;
                        }
                        else if (temp.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        break;
                    }
                case ObjectValueType.String:
                    {
                        var d = double.NaN;
                        int n = 0;
                        if (Parser.ParseNumber(temp.oValue as string, ref n, true, out d) && (n < (temp.oValue as string).Length))
                            d = double.NaN;
                        tempResult.dValue = d;
                        temp = tempResult;
                        goto case ObjectValueType.Double;
                    }
            }
            throw new NotImplementedException();
        }
    }
}