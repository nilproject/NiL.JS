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
                var f = first.Evaluate(context);
                tempContainer.valueType = f.valueType;
                tempContainer.oValue = f.oValue;
                tempContainer.iValue = f.iValue;
                tempContainer.dValue = f.dValue;
                tempContainer.__proto__ = f.__proto__;
                Impl(tempContainer, tempContainer, second.Evaluate(context));
                return tempContainer;
            }
        }

        private static void Impl(JSObject resultContainer, JSObject first, JSObject second)
        {
            switch (first.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.Int:
                            case JSObjectType.Bool:
                                {
                                    if (((first.iValue | second.iValue) & 0x7c000000) == 0
                                        && (first.iValue & second.iValue & 0x80000000) == 0)
                                    {
                                        resultContainer.valueType = JSObjectType.Int;
                                        resultContainer.iValue = first.iValue + second.iValue;
                                    }
                                    else
                                    {
                                        resultContainer.valueType = JSObjectType.Double;
                                        resultContainer.dValue = (long)first.iValue + second.iValue;
                                    }
                                    return;
                                }
                            case JSObjectType.Double:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.iValue + second.dValue;
                                    return;
                                }
                            case JSObjectType.String:
                                {
                                    resultContainer.oValue = (first.valueType == JSObjectType.Bool ? (first.iValue != 0 ? "true" : "false") : first.iValue.ToString(CultureInfo.InvariantCulture)) + second.oValue.ToString();
                                    resultContainer.valueType = JSObjectType.String;
                                    return;
                                }
                            case JSObjectType.NotExists:
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            case JSObjectType.Object: // x+null
                                {
                                    resultContainer.dValue = first.iValue;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                        }
                        throw new NotImplementedException();
                    }
                case JSObjectType.Double:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.Int:
                            case JSObjectType.Bool:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.dValue + second.iValue;
                                    return;
                                }
                            case JSObjectType.Double:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = first.dValue + second.dValue;
                                    return;
                                }
                            case JSObjectType.String:
                                {
                                    resultContainer.oValue = Tools.DoubleToString(first.dValue) + second.oValue.ToString();
                                    resultContainer.valueType = JSObjectType.String;
                                    return;
                                }
                            case JSObjectType.Object: // null
                                {
                                    resultContainer.dValue = first.dValue;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            case JSObjectType.NotExists:
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSObjectType.Double;
                                    return;
                                }
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case JSObjectType.String:
                    {
                        object tstr = first.oValue;
                        switch (second.valueType)
                        {
                            case JSObjectType.String:
                                {
                                    tstr = new RopeString(tstr, second.oValue);
                                    break;
                                }
                            case JSObjectType.Bool:
                                {
                                    tstr += second.iValue != 0 ? "true" : "false";
                                    break;
                                }
                            case JSObjectType.Int:
                                {
                                    tstr = string.Concat(tstr, second.iValue.ToString(CultureInfo.InvariantCulture));
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    tstr += Tools.DoubleToString(second.dValue);
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
                                    tstr += second.ToString();
                                    break;
                                }
                        }
                        resultContainer.oValue = tstr;
                        resultContainer.valueType = JSObjectType.String;
                        return;
                    }
                case JSObjectType.Date:
                    {
                        first = first.ToPrimitiveValue_String_Value();
                        Impl(resultContainer, first, second);
                        return;
                    }
                case JSObjectType.NotExistsInObject:
                case JSObjectType.Undefined:
                    {
                        if (second.valueType >= JSObjectType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSObjectType.String:
                                {
                                    resultContainer.valueType = JSObjectType.String;
                                    resultContainer.oValue = string.Concat("undefined", second.oValue.ToString());
                                    return;
                                }
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                            case JSObjectType.Object: // undefined+null
                            case JSObjectType.NotExistsInObject:
                            case JSObjectType.Undefined:
                                {
                                    resultContainer.valueType = JSObjectType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                        }
                        break;
                    }
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first.valueType == JSObjectType.Int || first.valueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        else if (first.valueType == JSObjectType.Object) // null
                        {
                            if (second.valueType >= JSObjectType.String)
                                second = second.ToPrimitiveValue_Value_String();
                            if (second.valueType == JSObjectType.String)
                            {
                                resultContainer.oValue = "null" + second.oValue.ToString();
                                resultContainer.valueType = JSObjectType.String;
                                return;
                            }
                            first.iValue = 0;
                            goto case JSObjectType.Int;
                        }
                        else if (first.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (first.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
            }
            throw new NotImplementedException();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Build(ref _this, depth, vars, strict);
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
