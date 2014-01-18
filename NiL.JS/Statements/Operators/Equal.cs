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
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        var iValue = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tempResult.iValue = iValue == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {

                                    tempResult.iValue = iValue == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Tools.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = iValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == JSObjectType.Int)
                                    {
                                        goto case JSObjectType.Int;
                                    }
                                    else if (temp.ValueType == JSObjectType.Double)
                                    {
                                        goto case JSObjectType.Double;
                                    }
                                    else if (temp.ValueType == JSObjectType.Bool)
                                    {
                                        goto case JSObjectType.Bool;
                                    }
                                    else if (temp.ValueType == JSObjectType.String)
                                    {
                                        goto case JSObjectType.String;
                                    }
                                    else if (temp.ValueType == JSObjectType.Object)
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
                case JSObjectType.Double:
                    {
                        var dValue = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tempResult.iValue = dValue == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    tempResult.iValue = dValue == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Tools.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = dValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    if (temp.ValueType == JSObjectType.Int)
                                    {
                                        goto case JSObjectType.Int;
                                    }
                                    else if (temp.ValueType == JSObjectType.Double)
                                    {
                                        goto case JSObjectType.Double;
                                    }
                                    else if (temp.ValueType == JSObjectType.Bool)
                                    {
                                        goto case JSObjectType.Bool;
                                    }
                                    else if (temp.ValueType == JSObjectType.String)
                                    {
                                        goto case JSObjectType.String;
                                    }
                                    else if (temp.ValueType == JSObjectType.Object)
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
                case JSObjectType.String:
                    {
                        string left = temp.oValue as string;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp.iValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp.dValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    tempResult.iValue = left == temp.oValue as string ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Object:
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
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case JSObjectType.Function:
                    {
                        var left = temp.oValue;
                        temp = second.Invoke(context);
                        tempResult.iValue = left == temp.oValue ? 1 : 0;
                        break;
                    }
                case JSObjectType.Proxy:
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        var stemp = second.Invoke(context);
                        var secondNValue = 0.0;
                        switch (stemp.ValueType)
                        {
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    secondNValue = stemp.ValueType == JSObjectType.Double ? stemp.dValue : stemp.iValue;
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                tempResult.iValue = temp.iValue == secondNValue ? 1 : 0;
                                                break;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                tempResult.iValue = temp.dValue == secondNValue ? 1 : 0;
                                                break;
                                            }
                                        case JSObjectType.String:
                                            {
                                                double d = 0;
                                                int i = 0;
                                                if (Tools.ParseNumber(temp.oValue as string, ref i, true, out d) && (i == (temp.oValue as string).Length))
                                                    tempResult.iValue = d == secondNValue ? 1 : 0;
                                                else
                                                    tempResult.iValue = 0;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    var str = stemp.oValue as string;
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case JSObjectType.Double:
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                secondNValue = temp.ValueType == JSObjectType.Double ? temp.dValue : stemp.iValue;
                                                double d = 0;
                                                int i = 0;
                                                if (Tools.ParseNumber(str, ref i, true, out d) && (i == str.Length))
                                                    tempResult.iValue = d == secondNValue ? 1 : 0;
                                                else
                                                    tempResult.iValue = 0;
                                                break;
                                            }
                                        case JSObjectType.String:
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
                case JSObjectType.NotExistInObject:
                case JSObjectType.Undefined:
                    {
                        temp = second.Invoke(context);
                        tempResult.iValue = temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject ? 1 : 0;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            tempResult.ValueType = JSObjectType.Bool;
            return tempResult;
        }
    }
}