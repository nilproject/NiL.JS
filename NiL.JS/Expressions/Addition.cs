using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Addition : Expression
    {
        public Addition(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                JSObject temp = first.Evaluate(context);
                object tstr;
                int tint;
                double tdouble;
                JSObjectType ttype;
                switch (temp.valueType)
                {
                    case JSObjectType.Bool:
                    case JSObjectType.Int:
                        {
                            ttype = temp.valueType;
                            tint = temp.iValue;
                            temp = second.Evaluate(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.Int:
                                case JSObjectType.Bool:
                                    {
                                        if (((tint | temp.iValue) & 0x7c000000) == 0
                                            && (tint & temp.iValue & 0x80000000) == 0)
                                        {
                                            tempContainer.valueType = JSObjectType.Int;
                                            tempContainer.iValue = tint + temp.iValue;
                                        }
                                        else
                                        {
                                            tempContainer.valueType = JSObjectType.Double;
                                            tempContainer.dValue = (long)tint + temp.iValue;
                                        }
                                        return tempContainer;
                                    }
                                case JSObjectType.Double:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = tint + temp.dValue;
                                        return tempContainer;
                                    }
                                case JSObjectType.String:
                                    {
                                        tempContainer.oValue = (ttype == JSObjectType.Bool ? (tint != 0 ? "true" : "false") : tint.ToString(CultureInfo.InvariantCulture)) + (string)temp.oValue;
                                        tempContainer.valueType = JSObjectType.String;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExists:
                                case JSObjectType.NotExistsInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempContainer.dValue = double.NaN;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                                case JSObjectType.Object: // x+null
                                    {
                                        tempContainer.dValue = tint;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                            }
                            throw new NotImplementedException();
                        }
                    case JSObjectType.Double:
                        {
                            tdouble = temp.dValue;
                            temp = second.Evaluate(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.Int:
                                case JSObjectType.Bool:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = tdouble + temp.iValue;
                                        return tempContainer;
                                    }
                                case JSObjectType.Double:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = tdouble + temp.dValue;
                                        return tempContainer;
                                    }
                                case JSObjectType.String:
                                    {
                                        tempContainer.oValue = Tools.DoubleToString(tdouble) + (string)temp.oValue;
                                        tempContainer.valueType = JSObjectType.String;
                                        return tempContainer;
                                    }
                                case JSObjectType.Object: // null
                                    {
                                        tempContainer.dValue = tdouble;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExists:
                                case JSObjectType.NotExistsInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempContainer.dValue = double.NaN;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    case JSObjectType.String:
                        {
                            tstr = temp.oValue;
                            temp = second.Evaluate(context);
                            switch (temp.valueType)
                            {
                                case JSObjectType.String:
                                    {
                                        tstr = string.Concat(tstr, temp.oValue);
                                        break;
                                    }
                                case JSObjectType.Bool:
                                    {
                                        tstr += temp.iValue != 0 ? "true" : "false";
                                        break;
                                    }
                                case JSObjectType.Int:
                                    {
                                        tstr = string.Concat(tstr, temp.iValue.ToString(CultureInfo.InvariantCulture));
                                        break;
                                    }
                                case JSObjectType.Double:
                                    {
                                        tstr += Tools.DoubleToString(temp.dValue);
                                        break;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistsInObject:
                                    {
                                        tstr += "undefined";
                                        break;
                                    }
                                case JSObjectType.Object:
                                case JSObjectType.Function:
                                case JSObjectType.Date:
                                    {
                                        tstr += temp.ToString();
                                        break;
                                    }
                            }
                            tempContainer.oValue = tstr;
                            tempContainer.valueType = JSObjectType.String;
                            return tempContainer;
                        }
                    case JSObjectType.Date:
                        {
                            temp = temp.ToPrimitiveValue_String_Value();
                            goto case JSObjectType.String;
                        }
                    case JSObjectType.NotExistsInObject:
                    case JSObjectType.Undefined:
                        {
                            temp = second.Evaluate(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.String:
                                    {
                                        tempContainer.valueType = JSObjectType.String;
                                        tempContainer.oValue = "undefined" + temp.oValue as string;
                                        return tempContainer;
                                    }
                                case JSObjectType.Double:
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = double.NaN;
                                        return tempContainer;
                                    }
                                case JSObjectType.Object: // undefined+null
                                case JSObjectType.NotExistsInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = double.NaN;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExists:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            }
                            break;
                        }
                    case JSObjectType.Function:
                    case JSObjectType.Object:
                        {
                            temp = temp.ToPrimitiveValue_Value_String();
                            if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.valueType == JSObjectType.Object)
                            {
                                temp = second.Evaluate(context);
                                if (temp.valueType >= JSObjectType.Object)
                                    temp = temp.ToPrimitiveValue_Value_String();
                                if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                {
                                    tempContainer.valueType = JSObjectType.Int;
                                    tempContainer.iValue = temp.iValue;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.Double)
                                {
                                    tempContainer.valueType = JSObjectType.Double;
                                    tempContainer.dValue = temp.dValue;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.String)
                                {
                                    tempContainer.oValue = "null" + (string)temp.oValue;
                                    tempContainer.valueType = JSObjectType.String;
                                    return tempContainer;
                                }
                                else if (temp.valueType <= JSObjectType.Undefined)
                                {
                                    tempContainer.dValue = double.NaN;
                                    tempContainer.valueType = JSObjectType.Double;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.Object) // null+null
                                {
                                    tempContainer.iValue = 0;
                                    tempContainer.valueType = JSObjectType.Int;
                                    return tempContainer;
                                }
                            }
                            else if (temp.valueType == JSObjectType.Double)
                                goto case JSObjectType.Double;
                            else if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.valueType == JSObjectType.String)
                                goto case JSObjectType.String;
                            else if (temp.valueType == JSObjectType.NotExists)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            break;
                        }
                }
                throw new NotImplementedException();
            }
        }

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Optimize(ref _this, depth, vars, strict);
            if (!res && _this == this)
            {
                if (first is StringConcat)
                {
                    _this = first;
                    (first as StringConcat).sources.Add(second);
                }
                else if (second is StringConcat)
                {
                    _this = second;
                    (second as StringConcat).sources.Insert(0, first);                    
                }
                else
                {
                    if ((first is ImmidateValueStatement
                        && (first as ImmidateValueStatement).value.valueType == JSObjectType.String)
                        || (second is ImmidateValueStatement
                        && (second as ImmidateValueStatement).value.valueType == JSObjectType.String))
                    {
                        _this = new StringConcat(new List<CodeNode>() { first, second });
                    }
                }
            }
            return res;
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
