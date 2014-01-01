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
            var temp0 = first.Invoke(context);
            var temp1 = second.Invoke(context);
            var lvt = temp0.ValueType;
            tempResult.ValueType = ObjectValueType.Bool;
            switch (lvt)
            {
                case ObjectValueType.Bool:
                case ObjectValueType.Int:
                    {
                        switch (temp1.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = temp0.iValue == temp1.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    tempResult.iValue = temp0.iValue == temp1.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp1.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = temp0.iValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp1 = temp1.ToPrimitiveValue_Value_String(context);
                                    if (temp1.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Object)
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
                        switch (temp1.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = temp0.dValue == temp1.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    tempResult.iValue = temp0.dValue == temp1.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp1.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = temp0.dValue == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp1 = temp1.ToPrimitiveValue_Value_String(context);
                                    if (temp1.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp1.ValueType == ObjectValueType.Object)
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
                        string left = temp0.oValue as string;
                        switch (temp1.ValueType)
                        {
                            case ObjectValueType.Bool:
                            case ObjectValueType.Int:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp1.iValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d == temp1.dValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    tempResult.iValue = left == temp1.oValue as string ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    if (temp1.oValue == null)
                                        tempResult.iValue = left == null ? 1 : 0;
                                    else
                                    {
                                        temp1 = temp1.ToPrimitiveValue_Value_String(context);
                                        tempResult.iValue = temp1.Value.ToString() == left ? 1 : 0;
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
                        switch (temp0.ValueType)
                        {
                            case ObjectValueType.Int:
                            case ObjectValueType.Double:
                            case ObjectValueType.Bool:
                            case ObjectValueType.String:
                                {
                                    temp0 = temp0.ToPrimitiveValue_Value_String(context);
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    if (temp0.oValue is NiL.JS.Core.BaseTypes.String)
                                        temp0 = (temp0.oValue as NiL.JS.Core.BaseTypes.String);
                                    break;
                                }
                        }
                        lvt = temp0.ValueType;
                        if (lvt != ObjectValueType.Object)
                        {
                            if (lvt == ObjectValueType.Int)
                                goto case ObjectValueType.Int;
                            if (lvt == ObjectValueType.Double)
                                goto case ObjectValueType.Double;
                            if (lvt == ObjectValueType.Bool)
                                goto case ObjectValueType.Bool;
                            if (lvt == ObjectValueType.String)
                                goto case ObjectValueType.String;
                        }
                        switch (temp1.ValueType)
                        {
                            case ObjectValueType.Object:
                                {
                                    tempResult.iValue = temp0.oValue == temp1.oValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Int:
                            case ObjectValueType.Double:
                            case ObjectValueType.Bool:
                            case ObjectValueType.String:
                            case ObjectValueType.Undefined:
                            case ObjectValueType.NotExistInObject:
                                {
                                    tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.NotExist: throw new InvalidOperationException("object not exist");
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.NotExistInObject:
                case ObjectValueType.Undefined:
                    {
                        tempResult.iValue = temp0.ValueType == ObjectValueType.Undefined || temp0.ValueType == ObjectValueType.NotExistInObject ? 1 : 0;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }
    }
}