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

            tempResult.ValueType = JSObjectType.Bool;
            var lvt = temp.ValueType;
            switch (lvt)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == JSObjectType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Bool)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                case JSObjectType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (double.IsNaN(left))
                            tempResult.iValue = this is MoreOrEqual ? 1 : 0; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else if (temp.ValueType == JSObjectType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else throw new NotImplementedException();
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
                                        tempResult.iValue = d < temp.iValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        tempResult.iValue = d < temp.dValue ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    tempResult.iValue = string.Compare(left, temp.oValue as string) < 0 ? 1 : 0;
                                    break;
                                }
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String(context);
                                    switch (temp.ValueType)
                                    {
                                        case JSObjectType.Int:
                                        case JSObjectType.Bool:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, false, out t))
                                                    tempResult.iValue = t < temp.iValue ? 1 : 0;
                                                else goto
                                                    case JSObjectType.String;
                                                break;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, false, out t))
                                                    tempResult.iValue = t < temp.dValue ? 1 : 0;
                                                else goto
                                                    case JSObjectType.String;
                                                break;
                                            }
                                        case JSObjectType.String:
                                            {
                                                tempResult.iValue = string.Compare(left, temp.Value.ToString()) < 0 ? 1 : 0;
                                                break;
                                            }
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
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        else if (temp.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (temp.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }
    }
}