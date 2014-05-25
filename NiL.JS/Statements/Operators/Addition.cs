using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Addition : Operator
    {
        public Addition(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            JSObject temp = first.Invoke(context);
            lock (this)
            {
                switch (temp.ValueType)
                {
                    case JSObjectType.Bool:
                    case JSObjectType.Int:
                        {
                            var type = temp.ValueType;
                            int ir = temp.iValue;
                            temp = second.Invoke(context);
                            if (temp.ValueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                            {
                                if (((ir | temp.iValue) & (int)0x40000000) == 0)
                                {
                                    tempResult.ValueType = JSObjectType.Int;
                                    tempResult.iValue = ir + temp.iValue;
                                    return tempResult;
                                }
                                else
                                {
                                    tempResult.ValueType = JSObjectType.Double;
                                    tempResult.dValue = (double)ir + temp.iValue;
                                    return tempResult;
                                }
                            }
                            else if (temp.ValueType == JSObjectType.Double)
                            {
                                tempResult.ValueType = JSObjectType.Double;
                                tempResult.dValue = ir + temp.dValue;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.String)
                            {
                                tempResult.oValue = (type == JSObjectType.Bool ? (ir != 0 ? "true" : "false") : ir.ToString()) + (string)temp.oValue;
                                tempResult.ValueType = JSObjectType.String;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject)
                            {
                                tempResult.dValue = double.NaN;
                                tempResult.ValueType = JSObjectType.Double;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.Object) // x+null
                            {
                                tempResult.dValue = ir;
                                tempResult.ValueType = JSObjectType.Double;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.NotExist)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            double dr = temp.dValue;
                            temp = second.Invoke(context);
                            if (temp.ValueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.ValueType)
                            {
                                case JSObjectType.Int:
                                case JSObjectType.Bool:
                                    {
                                        dr += temp.iValue;
                                        tempResult.ValueType = JSObjectType.Double;
                                        tempResult.dValue = dr;
                                        return tempResult;
                                    }
                                case JSObjectType.Double:
                                    {
                                        dr += temp.dValue;
                                        tempResult.ValueType = JSObjectType.Double;
                                        tempResult.dValue = dr;
                                        return tempResult;
                                    }
                                case JSObjectType.String:
                                    {
                                        tempResult.oValue = Tools.DoubleToString(dr) + (string)temp.oValue;
                                        tempResult.ValueType = JSObjectType.String;
                                        return tempResult;
                                    }
                                case JSObjectType.Object: // null
                                    {
                                        tempResult.dValue = dr;
                                        tempResult.ValueType = JSObjectType.Double;
                                        return tempResult;
                                    }
                                case JSObjectType.NotExistInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempResult.dValue = double.NaN;
                                        tempResult.ValueType = JSObjectType.Double;
                                        return tempResult;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            }
                            break;
                        }
                    case JSObjectType.String:
                        {
                            var val = temp.oValue as string;
                            temp = second.Invoke(context);
                            if (temp.ValueType == JSObjectType.Date)
                                temp = temp.ToPrimitiveValue_String_Value();
                            else if (temp.ValueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.ValueType)
                            {
                                case JSObjectType.Int:
                                    {
                                        val += temp.iValue;
                                        break;
                                    }
                                case JSObjectType.Double:
                                    {
                                        val += Tools.DoubleToString(temp.dValue);
                                        break;
                                    }
                                case JSObjectType.Bool:
                                    {
                                        val += temp.iValue != 0 ? "true" : "false";
                                        break;
                                    }
                                case JSObjectType.String:
                                    {
                                        val += temp.oValue;
                                        break;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistInObject:
                                    {
                                        val += "undefined";
                                        break;
                                    }
                                case JSObjectType.Object:
                                case JSObjectType.Function:
                                case JSObjectType.Date:
                                    {
                                        val += "null";
                                        break;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            }
                            tempResult.oValue = val;
                            tempResult.ValueType = JSObjectType.String;
                            return tempResult;
                        }
                    case JSObjectType.Date:
                        {
                            temp = temp.ToPrimitiveValue_String_Value();
                            goto case JSObjectType.String;
                        }
                    case JSObjectType.NotExistInObject:
                    case JSObjectType.Undefined:
                        {
                            var val = "undefined";
                            temp = second.Invoke(context);
                            if (temp.ValueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.ValueType)
                            {
                                case JSObjectType.String:
                                    {
                                        tempResult.ValueType = JSObjectType.String;
                                        tempResult.oValue = val as string + temp.oValue as string;
                                        return tempResult;
                                    }
                                case JSObjectType.Double:
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        tempResult.ValueType = JSObjectType.Double;
                                        tempResult.dValue = double.NaN;
                                        return tempResult;
                                    }
                                case JSObjectType.Object: // undefined+null
                                case JSObjectType.NotExistInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempResult.ValueType = JSObjectType.Double;
                                        tempResult.dValue = double.NaN;
                                        return tempResult;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            }
                            break;
                        }
                    case JSObjectType.Function:
                    case JSObjectType.Object:
                        {
                            temp = temp.ToPrimitiveValue_Value_String();
                            if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.ValueType == JSObjectType.Object)
                            {
                                temp = second.Invoke(context);
                                if (temp.ValueType >= JSObjectType.Object)
                                    temp = temp.ToPrimitiveValue_Value_String();
                                if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                                {
                                    tempResult.ValueType = JSObjectType.Int;
                                    tempResult.iValue = temp.iValue;
                                    return tempResult;
                                }
                                else if (temp.ValueType == JSObjectType.Double)
                                {
                                    tempResult.ValueType = JSObjectType.Double;
                                    tempResult.dValue = temp.dValue;
                                    return tempResult;
                                }
                                else if (temp.ValueType == JSObjectType.String)
                                {
                                    tempResult.oValue = "null" + (string)temp.oValue;
                                    tempResult.ValueType = JSObjectType.String;
                                    return tempResult;
                                }
                                else if (temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject)
                                {
                                    tempResult.dValue = double.NaN;
                                    tempResult.ValueType = JSObjectType.Double;
                                    return tempResult;
                                }
                                else if (temp.ValueType == JSObjectType.Object) // null+null
                                {
                                    tempResult.iValue = 0;
                                    tempResult.ValueType = JSObjectType.Int;
                                    return tempResult;
                                }
                            }
                            else if (temp.ValueType == JSObjectType.Double)
                                goto case JSObjectType.Double;
                            else if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.ValueType == JSObjectType.String)
                                goto case JSObjectType.String;
                            else if (temp.ValueType == JSObjectType.NotExist)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            break;
                        }
                    case JSObjectType.NotExist:
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                }
                throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
