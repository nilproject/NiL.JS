using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public class More : Expression
    {
        public More(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);
            string tstr;
            int tint;
            int index;
            double tdouble;
            double td;
            var lvt = temp.valueType;
            switch (lvt)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        tint = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return tint > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    if (double.IsNaN(temp.dValue))
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                    else
                                        return tint > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    index = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref index, out tdouble) && (index == (temp.oValue as string).Length))
                                        return tint > tdouble ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (temp.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (temp.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (temp.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (temp.valueType >= JSObjectType.Object) // null
                                    {
                                        temp.iValue = 0;
                                        goto case JSObjectType.Int;
                                    }
                                    throw new NotImplementedException();
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistsInObject:
                                {
                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.NotExists:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case JSObjectType.Double:
                    {
                        tdouble = temp.dValue;
                        temp = second.Invoke(context);
                        if (double.IsNaN(tdouble))
                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else
                            switch (temp.valueType)
                            {
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        return tdouble > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.Double:
                                    {
                                        if (double.IsNaN(tdouble) || double.IsNaN(temp.dValue))
                                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                        else
                                            return tdouble > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.String:
                                    {
                                        index = 0;
                                        if (Tools.ParseNumber(temp.oValue as string, ref index, out td) && (index == (temp.oValue as string).Length))
                                            return tdouble > td ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                        else
                                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistsInObject:
                                    {
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.Date:
                                case JSObjectType.Object:
                                    {
                                        temp = temp.ToPrimitiveValue_Value_String();
                                        if (temp.valueType == JSObjectType.Int)
                                            goto case JSObjectType.Int;
                                        if (temp.valueType == JSObjectType.Bool)
                                            goto case JSObjectType.Int;
                                        if (temp.valueType == JSObjectType.Double)
                                            goto case JSObjectType.Double;
                                        if (temp.valueType == JSObjectType.String)
                                            goto case JSObjectType.String;
                                        if (temp.valueType >= JSObjectType.Object) // null
                                        {
                                            temp.iValue = 0;
                                            goto case JSObjectType.Int;
                                        }
                                        throw new NotImplementedException();
                                    }
                                case JSObjectType.NotExists:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                                default:
                                    throw new NotImplementedException();
                            }
                    }
                case JSObjectType.String:
                    {
                        tstr = temp.oValue as string;
                        temp = second.Invoke(context);
                        switch (temp.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    index = 0;
                                    if (Tools.ParseNumber(tstr, ref index, out td) && (index == tstr.Length))
                                        return td > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    index = 0;
                                    if (Tools.ParseNumber(tstr, ref index, out td) && (index == tstr.Length))
                                        return td > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    return string.CompareOrdinal(tstr, temp.oValue as string) > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
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
                                                index = 0;
                                                if (Tools.ParseNumber(tstr, ref index, out td) && (index == tstr.Length))
                                                    return td > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else goto
                                                    case JSObjectType.String;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                index = 0;
                                                if (Tools.ParseNumber(tstr, ref index, out td) && (index == tstr.Length))
                                                    return td > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return string.CompareOrdinal(tstr, temp.Value.ToString()) > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.Object:
                                            {
                                                index = 0;
                                                if (Tools.ParseNumber(tstr, ref index, out td) && (index == tstr.Length))
                                                    return td > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.NotExists:
                                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                                        default: throw new NotImplementedException();
                                    }
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistsInObject:
                                {
                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.NotExists:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            default: throw new NotImplementedException();
                        }
                    }
                case JSObjectType.Function:
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String();
                        if (temp.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (temp.valueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        if (temp.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        if (temp.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (temp.valueType >= JSObjectType.Object) // null
                        {
                            temp.iValue = 0;
                            goto case JSObjectType.Int;
                        }
                        throw new NotImplementedException();
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        second.Invoke(context);
                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                    }
                case JSObjectType.NotExists:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default: throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "(" + first + " > " + second + ")";
        }
    }
}