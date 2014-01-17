using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Less : Operator
    {
        public Less(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Bool;
            var lvt = temp.ValueType;
            switch (lvt)
            {
                case ObjectValueType.Bool:
                case ObjectValueType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Bool)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (double.IsNaN(left))
                            tempResult.iValue = this is MoreOrEqual ? 1 : 0; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else if (temp.ValueType == ObjectValueType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else throw new NotImplementedException();
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
                                        tempResult.iValue = d < temp.iValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d < temp.dValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    tempResult.iValue = string.Compare(left, temp.oValue as string) < 0 ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case ObjectValueType.Int:
                                        case ObjectValueType.Bool:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Parser.ParseNumber(left, ref i, false, out t))
                                                    tempResult.iValue = t < temp.iValue ? 1 : 0;
                                                else goto
                                                    case ObjectValueType.String;
                                                break;
                                            }
                                        case ObjectValueType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Parser.ParseNumber(left, ref i, false, out t))
                                                    tempResult.iValue = t < temp.dValue ? 1 : 0;
                                                else goto
                                                    case ObjectValueType.String;
                                                break;
                                            }
                                        case ObjectValueType.String:
                                            {
                                                tempResult.iValue = string.Compare(left, temp.Value.ToString()) < 0 ? 1 : 0;
                                                break;
                                            }
                                        default: throw new NotImplementedException();
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
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        else if (temp.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        else if (temp.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }
    }
}