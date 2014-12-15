using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public class More : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public More(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal static bool Check(JSObject first, JSObject second, bool lessOrEqual)
        {
            switch (first.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        switch (second.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    return first.iValue > second.iValue;
                                }
                            case JSObjectType.Double:
                                {
                                    if (double.IsNaN(second.dValue))
                                        return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                    else
                                        return first.iValue > second.dValue;
                                }
                            case JSObjectType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(second.oValue.ToString(), ref index, out td) && (index == (second.oValue.ToString()).Length))
                                        return first.iValue > td;
                                    else
                                        return lessOrEqual;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    if (second.valueType == JSObjectType.Int)
                                        goto case JSObjectType.Int;
                                    if (second.valueType == JSObjectType.Bool)
                                        goto case JSObjectType.Int;
                                    if (second.valueType == JSObjectType.Double)
                                        goto case JSObjectType.Double;
                                    if (second.valueType == JSObjectType.String)
                                        goto case JSObjectType.String;
                                    if (second.valueType >= JSObjectType.Object) // null
                                        return first.iValue > 0;
                                    throw new NotImplementedException();
                                }
                            default:
                                return lessOrEqual;
                        }
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(first.dValue))
                            return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else
                            switch (second.valueType)
                            {
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        return first.dValue > second.iValue;
                                    }
                                case JSObjectType.Double:
                                    {
                                        if (double.IsNaN(first.dValue) || double.IsNaN(second.dValue))
                                            return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                        else
                                            return first.dValue > second.dValue;
                                    }
                                case JSObjectType.String:
                                    {
                                        var index = 0;
                                        double td = 0;
                                        if (Tools.ParseNumber(second.oValue.ToString(), ref index, out td) && (index == (second.oValue.ToString()).Length))
                                            return first.dValue > td;
                                        else
                                            return lessOrEqual;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistsInObject:
                                    {
                                        return lessOrEqual;
                                    }
                                case JSObjectType.Date:
                                case JSObjectType.Object:
                                    {
                                        second = second.ToPrimitiveValue_Value_String();
                                        if (second.valueType == JSObjectType.Int)
                                            goto case JSObjectType.Int;
                                        if (second.valueType == JSObjectType.Bool)
                                            goto case JSObjectType.Int;
                                        if (second.valueType == JSObjectType.Double)
                                            goto case JSObjectType.Double;
                                        if (second.valueType == JSObjectType.String)
                                            goto case JSObjectType.String;
                                        if (second.valueType >= JSObjectType.Object) // null
                                            return first.dValue > 0;
                                        throw new NotImplementedException();
                                    }
                                default:
                                    return lessOrEqual;
                            }
                    }
                case JSObjectType.String:
                    {
                        string left = first.oValue.ToString();
                        switch (second.valueType)
                        {
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d > second.iValue;
                                    else
                                        return lessOrEqual;
                                }
                            case JSObjectType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d > second.dValue;
                                    else
                                        return lessOrEqual;
                                }
                            case JSObjectType.String:
                                {
                                    return string.CompareOrdinal(left, second.oValue.ToString()) > 0;
                                }
                            case JSObjectType.Function:
                            case JSObjectType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    switch (second.valueType)
                                    {
                                        case JSObjectType.Int:
                                        case JSObjectType.Bool:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > second.iValue;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > second.dValue;
                                                else
                                                    goto case JSObjectType.String;
                                            }
                                        case JSObjectType.String:
                                            {
                                                return string.CompareOrdinal(left, second.Value.ToString()) > 0;
                                            }
                                        case JSObjectType.Object:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > 0;
                                                else
                                                    return lessOrEqual;
                                            }
                                        case JSObjectType.NotExists:
                                            throw new JSException((new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                                        default: throw new NotImplementedException();
                                    }
                                }
                            default:
                                return lessOrEqual;
                        }
                    }
                case JSObjectType.Function:
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first.valueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        if (first.valueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        if (first.valueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        if (first.valueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        if (first.valueType >= JSObjectType.Object) // null
                        {
                            first.iValue = 0; // такое делать можно, поскольку тип не меняется
                            goto case JSObjectType.Int;
                        }
                        throw new NotImplementedException();
                    }
                default:
                    return lessOrEqual;
            }
        }

        internal override JSObject Evaluate(Context context)
        {
            var f = first.Evaluate(context);
            var temp = tempContainer ?? new JSObject { attributes = JSObjectAttributesInternal.Temporary };
            temp.valueType = f.valueType;
            temp.iValue = f.iValue;
            temp.dValue = f.dValue;
            temp.oValue = f.oValue;
            tempContainer = null;
            var s = second.Evaluate(context);
            tempContainer = temp;
            if (tempContainer.valueType == s.valueType
                && tempContainer.valueType == JSObjectType.Int)
                return tempContainer.iValue > s.iValue;
            return Check(tempContainer, s, this is LessOrEqual);
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            baseOptimize(owner, message);
            if (first.ResultType == PredictedType.Number
                && second.ResultType == PredictedType.Number)
            {
                _this = new NumberMore(first, second);
                return;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " > " + second + ")";
        }
    }
}