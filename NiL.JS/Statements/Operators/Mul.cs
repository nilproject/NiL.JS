using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

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
                                    dr *= temp.iValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Double:
                                {
                                    dr *= temp.dValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Statement:
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == ObjectValueType.Int)
                                        goto case ObjectValueType.Int;
                                    else if (temp.ValueType == ObjectValueType.Double)
                                        goto case ObjectValueType.Double;
                                    else if ((temp.ValueType == ObjectValueType.Object)
                                        || (temp.ValueType == ObjectValueType.String)
                                        || (temp.ValueType == ObjectValueType.Undefined)
                                        || (temp.ValueType == ObjectValueType.Statement))
                                    {
                                        tempResult.ValueType = ObjectValueType.Double;
                                        tempResult.dValue = double.NaN;
                                        return tempResult;
                                    }
                                    break;
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
                                    dr *= temp.iValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Double:
                                {
                                    dr *= temp.dValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == ObjectValueType.Int)
                                        goto case ObjectValueType.Int;
                                    else if (temp.ValueType == ObjectValueType.Double)
                                        goto case ObjectValueType.Double;
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        tempResult.ValueType = ObjectValueType.Double;
                                        tempResult.dValue = double.NaN;
                                        return tempResult;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String();
                        if (temp.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        else if (temp.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        else if (temp.ValueType == ObjectValueType.Object)
                        {
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = double.NaN;
                            return tempResult;
                        }
                        break;
                    }
            }
            throw new NotImplementedException();
        }
    }
}