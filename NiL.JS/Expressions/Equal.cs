using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Equal : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Equal(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSValue tjso;
            int tint;
            double tdouble;
            string tstr;
            var index = 0;
            switch (temp.valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        tint = temp.iValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    return tint == tjso.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSValueType.Double:
                                {
                                    return tint == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSValueType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tint == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSValueType.Date:
                            case JSValueType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSValueType.Integer)
                                        goto case JSValueType.Integer;
                                    if (tjso.valueType == JSValueType.Boolean)
                                        goto case JSValueType.Integer;
                                    if (tjso.valueType == JSValueType.Double)
                                        goto case JSValueType.Double;
                                    if (tjso.valueType == JSValueType.String)
                                        goto case JSValueType.String;
                                    if (tjso.valueType >= JSValueType.Object) // null
                                        return false;
                                    throw new NotImplementedException();
                                }
                        }
                        return NiL.JS.BaseLibrary.Boolean.False;
                    }
                case JSValueType.Double:
                    {
                        tdouble = temp.dValue;
                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    return tdouble == tjso.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSValueType.Double:
                                {
                                    return tdouble == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSValueType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    if (Tools.ParseNumber(tstr, ref index, out tjso.dValue) && (index == tstr.Length))
                                        return tdouble == tjso.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSValueType.Date:
                            case JSValueType.Object:
                                {
                                    tjso = tjso.ToPrimitiveValue_Value_String();
                                    if (tjso.valueType == JSValueType.Integer)
                                        goto case JSValueType.Integer;
                                    if (tjso.valueType == JSValueType.Boolean)
                                        goto case JSValueType.Integer;
                                    if (tjso.valueType == JSValueType.Double)
                                        goto case JSValueType.Double;
                                    if (tjso.valueType == JSValueType.String)
                                        goto case JSValueType.String;
                                    if (tjso.valueType >= JSValueType.Object) // null
                                    {
                                        return tdouble == 0 && double.IsPositiveInfinity(1.0 / tdouble);
                                    }
                                    throw new NotImplementedException();
                                }
                        }
                        return false;
                    }
                case JSValueType.String:
                    {
                        tstr = temp.oValue.ToString();
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSValueType.Double:
                                {
                                    if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                        return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                    else
                                        return false;
                                }
                            case JSValueType.String:
                                {
                                    return string.CompareOrdinal(tstr, temp.oValue.ToString()) == 0 ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            case JSValueType.Function:
                            case JSValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSValueType.Integer:
                                        case JSValueType.Boolean:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.iValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    goto
                                                        case JSValueType.String;
                                            }
                                        case JSValueType.Double:
                                            {
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    goto case JSValueType.String;
                                            }
                                        case JSValueType.String:
                                            {
                                                return string.CompareOrdinal(tstr, temp.oValue.ToString()) == 0 ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                        }
                        return false;
                    }
                case JSValueType.Function:
                case JSValueType.Date:
                case JSValueType.Symbol:
                case JSValueType.Object:
                    {
                        if (tempContainer == null)
                            tempContainer = new JSValue() { attributes = JSValueAttributesInternal.Temporary };
                        tempContainer.Assign(temp);
                        temp = tempContainer;

                        tjso = second.Evaluate(context);
                        switch (tjso.valueType)
                        {
                            case JSValueType.Double:
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    tdouble = tjso.valueType == JSValueType.Double ? tjso.dValue : tjso.iValue;
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSValueType.Boolean:
                                        case JSValueType.Integer:
                                            {
                                                return temp.iValue == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                        case JSValueType.Double:
                                            {
                                                return temp.dValue == tdouble ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                        case JSValueType.String:
                                            {
                                                tstr = temp.oValue.ToString();
                                                if (Tools.ParseNumber(tstr, ref index, out temp.dValue) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    return false;
                                            }
                                    }
                                    return false;
                                }
                            case JSValueType.String:
                                {
                                    tstr = tjso.oValue.ToString();
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    switch (temp.valueType)
                                    {
                                        case JSValueType.Double:
                                        case JSValueType.Boolean:
                                        case JSValueType.Integer:
                                            {
                                                temp.dValue = temp.valueType == JSValueType.Double ? temp.dValue : temp.iValue;
                                                if (Tools.ParseNumber(tstr, ref index, out tdouble) && (index == tstr.Length))
                                                    return tdouble == temp.dValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                                else
                                                    return false;
                                            }
                                        case JSValueType.String:
                                            {
                                                return temp.oValue.ToString() == tstr ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    return temp.oValue == tjso.oValue ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                        }
                        break;
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        temp = second.Evaluate(context);
                        switch (temp.valueType)
                        {
                            case JSValueType.Object:
                                {
                                    return temp.oValue == null ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                            default:
                                {
                                    return !temp.Defined ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False;
                                }
                        }
                    }
                default:
                    throw new NotImplementedException();
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
            if (message != null)
            {
                var fc = first as Constant ?? second as Constant;
                if (fc != null)
                {
                    switch (fc.value.valueType)
                    {
                        case JSValueType.Undefined:
                            message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "To compare with undefined use '===' or '!==' instead of '==' or '!='.");
                            break;
                        case JSValueType.Object:
                            if (fc.value.oValue == null)
                                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "To compare with null use '===' or '!==' instead of '==' or '!='.");
                            break;
                    }
                }
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " == " + second + ")";
        }
    }
}