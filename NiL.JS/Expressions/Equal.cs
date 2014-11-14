using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public class Equal : Expression
    {
        public Equal(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSObject tjso;
            int tint;
            double tdouble;
            string tstr;
            var index = 0;
            switch (temp.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        tint = temp.iValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return tint == tjso.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    return tint == tjso.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tint == tdouble ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (tjso.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (tjso.valueType >= JSObjectType.Object) // null
                                        return false;
                                    throw new NotImplementedException();
                                }
                        }
                        return NiL.JS.Core.BaseTypes.Boolean.False;
                    }
                case JSObjectType.Double:
                    {
                        tdouble = temp.dValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return tdouble == tjso.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    return tdouble == tjso.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tjso.dValue) && (index == tstr.Length))
                                        return tdouble == tjso.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (tjso.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (tjso.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (tjso.valueType >= JSObjectType.Object) // null
                                    {
                                        tjso.iValue = 0;
                                        goto case JSObjectType.Int;
                                    }
                                    throw new NotImplementedException();
                                }
                        }
                        return false;
                    }
                case JSObjectType.String:
                    {
                        tstr = temp.oValue.ToString();
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.Double:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSObjectType.String:
                                {
                                    return string.CompareOrdinal(tstr, temp.oValue.ToString()) == 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Function:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Int:
                                        case JSObjectType.Bool:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else goto
                                                    case JSObjectType.String;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return string.CompareOrdinal(tstr, temp.Value.ToString()) == 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                        }
                        return false;
                    }
                case JSObjectType.Function:
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tdouble = tjso.valueType == JSObjectType.Double ? tjso.dValue : tjso.iValue;
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                return temp.iValue == tdouble ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                return temp.dValue == tdouble ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.String:
                                            {
                                                tstr = temp.oValue.ToString();
                                                if (Tools.ParseNumber(tstr, ref index, out temp.dValue) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    return false;
                                            }
                                    }
                                    return false;
                                }
                            case JSObjectType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSObjectType.Double:
                                        case JSObjectType.Bool:
                                        case JSObjectType.Int:
                                            {
                                                temp.dValue = temp.valueType == JSObjectType.Double ? temp.dValue : temp.iValue;
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    return false;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return temp.oValue.ToString() == tstr ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    return temp.oValue == tjso.oValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                        }
                        break;
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Object:
                                {
                                    return temp.oValue == null ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            default:
                                {
                                    return !temp.isDefinded ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                        }
                    }
                default: throw new NotImplementedException();
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " == " + second + ")";
        }
    }
}