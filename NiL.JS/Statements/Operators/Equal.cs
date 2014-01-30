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
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    tempResult.iValue = left == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref index, true, out td) && (index == (temp.oValue as string).Length))
                                        tempResult.iValue = left == td ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (temp.ValueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (temp.ValueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (temp.ValueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (temp.ValueType >= JSObjectType.Object) // null
                                    {
                                        tempResult.iValue = 0;
                                        break;
                                    }
                                    throw new NotImplementedException();
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.NotExist:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    }
                case JSObjectType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    tempResult.iValue = left == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref index, true, out td) && (index == (temp.oValue as string).Length))
                                        tempResult.iValue = left == td ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (temp.ValueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (temp.ValueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (temp.ValueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (temp.ValueType >= JSObjectType.Object) // null
                                    {
                                        temp.iValue = 0;
                                        goto case JSObjectType.Int;
                                    }
                                    throw new NotImplementedException();
                                }
                            case JSObjectType.NotExist:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            default:
                                throw new NotImplementedException();
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
                                    tempResult.iValue = string.Compare(left, temp.oValue as string, StringComparison.Ordinal) == 0 ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Function:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.ValueType)
                                    {
                                        case JSObjectType.Int:
                                        case JSObjectType.Bool:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, true, out t) && (i == left.Length))
                                                    tempResult.iValue = t == temp.iValue ? 1 : 0;
                                                else goto
                                                    case JSObjectType.String;
                                                break;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, true, out t) && (i == left.Length))
                                                    tempResult.iValue = t == temp.dValue ? 1 : 0;
                                                else
                                                    goto case JSObjectType.String;
                                                break;
                                            }
                                        case JSObjectType.String:
                                            {
                                                tempResult.iValue = string.Compare(left, temp.Value.ToString(), StringComparison.Ordinal) == 0 ? 1 : 0;
                                                break;
                                            }
                                        case JSObjectType.Object:
                                            {
                                                tempResult.iValue = 0;
                                                break;
                                            }
                                        case JSObjectType.NotExist:
                                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                                        default: throw new NotImplementedException();
                                    }
                                    break;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.NotExist:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case JSObjectType.Function:
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
                                    temp = temp.ToPrimitiveValue_Value_String();
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
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.ValueType)
                                    {
                                        case JSObjectType.Double:
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                secondNValue = temp.ValueType == JSObjectType.Double ? temp.dValue : temp.iValue;
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
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Object:
                                {
                                    tempResult.iValue = temp.oValue == null ? 1 : 0;
                                    break;
                                }
                            default:
                                {
                                    tempResult.iValue = temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject ? 1 : 0;
                                    break;
                                }
                        }
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default: throw new NotImplementedException();
            }
            tempResult.ValueType = JSObjectType.Bool;
            return tempResult;
        }
    }
}