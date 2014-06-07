using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public class More : Operator
    {
        public More(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
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
                                    return left > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    if (double.IsNaN(temp.dValue))
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                    else
                                        return left > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(temp.oValue as string, ref index, true, out td) && (index == (temp.oValue as string).Length))
                                        return left > td ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
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
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.NotExist:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case JSObjectType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (double.IsNaN(left))
                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else
                            switch (temp.ValueType)
                            {
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        return left > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.Double:
                                    {
                                        if (double.IsNaN(left) || double.IsNaN(temp.dValue))
                                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                        else
                                            return left > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.String:
                                    {
                                        var index = 0;
                                        double td = 0;
                                        if (Tools.ParseNumber(temp.oValue as string, ref index, true, out td) && (index == (temp.oValue as string).Length))
                                            return left > td ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                        else
                                            return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistInObject:
                                    {
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
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
                                        return d > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                        return d > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                    else
                                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.String:
                                {
                                    return string.CompareOrdinal(left, temp.oValue as string) > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
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
                                                    return t > temp.iValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else goto
                                                    case JSObjectType.String;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, true, out t) && (i == left.Length))
                                                    return t > temp.dValue ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return string.CompareOrdinal(left, temp.Value.ToString()) > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.Object:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, true, out t) && (i == left.Length))
                                                    return t > 0 ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                                else
                                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                            }
                                        case JSObjectType.NotExist:
                                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                                        default: throw new NotImplementedException();
                                    }
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                                }
                            case JSObjectType.NotExist:
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                            default: throw new NotImplementedException();
                        }
                    }
                case JSObjectType.Function:
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
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        second.Invoke(context);
                        return this is LessOrEqual ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
                    }
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default: throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "(" + first + " > " + second + ")";
        }
    }
}