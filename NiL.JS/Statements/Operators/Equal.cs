using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Equal : Operator
    {
        public Equal(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);
            var lvt = temp.ValueType;
            switch (lvt)
            {
                case ObjectValueType.Bool:
                case ObjectValueType.Int:
                    {
                        var iValue = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = iValue == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {

                                    tempResult.iValue = iValue == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = iValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        tempResult.iValue = 0;
                                    }
                                    else goto default;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        var dValue = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = dValue == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    tempResult.iValue = dValue == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = dValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        tempResult.iValue = 0;
                                    }
                                    else goto default;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.String:
                    {
                        string left = temp.oValue as string;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp.iValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp.dValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    tempResult.iValue = left == temp.oValue as string ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    if (temp.oValue == null)
                                        tempResult.iValue = left == null ? 1 : 0;
                                    else
                                    {
                                        temp = temp.ToPrimitiveValue_Value_String(context);
                                        tempResult.iValue = temp.Value.ToString() == left ? 1 : 0;
                                    }
                                    break;
                                }
                            case ObjectValueType.Undefined:
                            case ObjectValueType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        var stemp = second.Invoke(context);
                        var secondNValue = 0.0;
                        switch (stemp.ValueType)
                        {
                            case ObjectValueType.Double:
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    secondNValue = stemp.ValueType == ObjectValueType.Double ? stemp.dValue : stemp.iValue;
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case ObjectValueType.Bool:
                                        case ObjectValueType.Int:
                                            {
                                                tempResult.iValue = temp.iValue == secondNValue ? 1 : 0;
                                                break;
                                            }
                                        case ObjectValueType.Double:
                                            {
                                                tempResult.iValue = temp.dValue == secondNValue ? 1 : 0;
                                                break;
                                            }
                                        case ObjectValueType.String:
                                            {
                                                double d = 0;
                                                int i = 0;
                                                if (Parser.ParseNumber(temp.oValue as string, ref i, true, out d) && (i == (temp.oValue as string).Length))
                                                    tempResult.iValue = d == secondNValue ? 1 : 0;
                                                else
                                                    tempResult.iValue = 0;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    var str = stemp.oValue as string;
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case ObjectValueType.Double:
                                        case ObjectValueType.Bool:
                                        case ObjectValueType.Int:
                                            {
                                                secondNValue = temp.ValueType == ObjectValueType.Double ? temp.dValue : stemp.iValue;
                                                double d = 0;
                                                int i = 0;
                                                if (Parser.ParseNumber(str, ref i, true, out d) && (i == str.Length))
                                                    tempResult.iValue = d == secondNValue ? 1 : 0;
                                                else
                                                    tempResult.iValue = 0;
                                                break;
                                            }
                                        case ObjectValueType.String:
                                            {
                                                tempResult.iValue = temp.oValue as string == str ? 1 : 0;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    tempResult.iValue = temp.Value == stemp.Value ? 1 : 0;
                                    break;
                                }
                        }
                        
                        break;
                    }
                case ObjectValueType.NotExistInObject:
                case ObjectValueType.Undefined:
                    {
                        temp = second.Invoke(context);
                        tempResult.iValue = temp.ValueType == ObjectValueType.Undefined || temp.ValueType == ObjectValueType.NotExistInObject ? 1 : 0;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            tempResult.ValueType = ObjectValueType.Bool;
            return tempResult;
        }
    }
}