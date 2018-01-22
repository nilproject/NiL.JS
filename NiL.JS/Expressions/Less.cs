using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Less : Expression
    {
        private bool trueLess;

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

        internal Less(Expression first, Expression second)
            : base(first, second, true)
        {
            trueLess = this.GetType() == typeof(Less);
        }

        internal static bool Check(JSValue first, JSValue second)
        {
            return Check(first, second, false);
        }

        internal static bool Check(JSValue first, JSValue second, bool moreOrEqual)
        {
            switch (first._valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        switch (second._valueType)
                        {
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    return first._iValue < second._iValue;
                                }
                            case JSValueType.Double:
                                {
                                    if (double.IsNaN(second._dValue))
                                        return moreOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                    else
                                        return first._iValue < second._dValue;
                                }
                            case JSValueType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(second._oValue.ToString(), ref index, out td) && (index == (second._oValue.ToString()).Length))
                                        return first._iValue < td;
                                    else
                                        return moreOrEqual;
                                }
                            case JSValueType.Date:
                            case JSValueType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    if (second._valueType == JSValueType.Integer)
                                        goto case JSValueType.Integer;
                                    if (second._valueType == JSValueType.Boolean)
                                        goto case JSValueType.Integer;
                                    if (second._valueType == JSValueType.Double)
                                        goto case JSValueType.Double;
                                    if (second._valueType == JSValueType.String)
                                        goto case JSValueType.String;
                                    if (second._valueType >= JSValueType.Object) // null
                                        return first._iValue < 0;
                                    throw new NotImplementedException();
                                }
                            default:
                                return moreOrEqual;
                        }
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(first._dValue))
                            return moreOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else
                            switch (second._valueType)
                            {
                                case JSValueType.Boolean:
                                case JSValueType.Integer:
                                    {
                                        return first._dValue < second._iValue;
                                    }
                                case JSValueType.Double:
                                    {
                                        if (double.IsNaN(first._dValue) || double.IsNaN(second._dValue))
                                            return moreOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                        else
                                            return first._dValue < second._dValue;
                                    }
                                case JSValueType.String:
                                    {
                                        var index = 0;
                                        double td = 0;
                                        if (Tools.ParseNumber(second._oValue.ToString(), ref index, out td) && (index == (second._oValue.ToString()).Length))
                                            return first._dValue < td;
                                        else
                                            return moreOrEqual;
                                    }
                                case JSValueType.Date:
                                case JSValueType.Object:
                                    {
                                        second = second.ToPrimitiveValue_Value_String();
                                        if (second._valueType == JSValueType.Integer)
                                            goto case JSValueType.Integer;
                                        if (second._valueType == JSValueType.Boolean)
                                            goto case JSValueType.Integer;
                                        if (second._valueType == JSValueType.Double)
                                            goto case JSValueType.Double;
                                        if (second._valueType == JSValueType.String)
                                            goto case JSValueType.String;
                                        if (second._valueType >= JSValueType.Object) // null
                                            return first._dValue < 0;
                                        throw new NotImplementedException();
                                    }
                                default:
                                    return moreOrEqual;
                            }
                    }
                case JSValueType.String:
                    {
                        string left = first._oValue.ToString();
                        switch (second._valueType)
                        {
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d < second._iValue;
                                    else
                                        return moreOrEqual;
                                }
                            case JSValueType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d < second._dValue;
                                    else
                                        return moreOrEqual;
                                }
                            case JSValueType.String:
                                {
                                    return string.CompareOrdinal(left, second._oValue.ToString()) < 0;
                                }
                            case JSValueType.Function:
                            case JSValueType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    switch (second._valueType)
                                    {
                                        case JSValueType.Integer:
                                        case JSValueType.Boolean:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t < second._iValue;
                                                else
                                                    goto case JSValueType.String;
                                            }
                                        case JSValueType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t < second._dValue;
                                                else
                                                    goto case JSValueType.String;
                                            }
                                        case JSValueType.String:
                                            {
                                                return string.CompareOrdinal(left, second._oValue.ToString()) < 0;
                                            }
                                        case JSValueType.Object:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t < 0;
                                                else
                                                    return moreOrEqual;
                                            }
                                        default: throw new NotImplementedException();
                                    }
                                }
                            default:
                                return moreOrEqual;
                        }
                    }
                case JSValueType.Function:
                case JSValueType.Date:
                case JSValueType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first._valueType == JSValueType.Integer)
                            goto case JSValueType.Integer;
                        if (first._valueType == JSValueType.Boolean)
                            goto case JSValueType.Integer;
                        if (first._valueType == JSValueType.Double)
                            goto case JSValueType.Double;
                        if (first._valueType == JSValueType.String)
                            goto case JSValueType.String;
                        if (first._valueType >= JSValueType.Object) // null
                        {
                            first._iValue = 0; // такое делать можно, поскольку тип не меняется
                            goto case JSValueType.Integer;
                        }
                        throw new NotImplementedException();
                    }
                default:
                    return moreOrEqual;
            }
        }

        public override JSValue Evaluate(Context context)
        {
            var f = _left.Evaluate(context);
            var temp = _tempContainer;
            _tempContainer = null;
            if (temp == null)
                temp = new JSValue { _attributes = JSValueAttributesInternal.Temporary };
            temp._valueType = f._valueType;
            temp._iValue = f._iValue;
            temp._dValue = f._dValue;
            temp._oValue = f._oValue;
            var s = _right.Evaluate(context);
            _tempContainer = temp;
            if (temp._valueType == JSValueType.Integer && s._valueType == JSValueType.Integer)
            {
                temp._valueType = JSValueType.Boolean;
                temp._iValue = temp._iValue < s._iValue ? 1 : 0;
                return _tempContainer;
            }
            if (_tempContainer._valueType == JSValueType.Double && s._valueType == JSValueType.Double)
            {
                temp._valueType = JSValueType.Boolean;
                if (double.IsNaN(temp._dValue) || double.IsNaN(s._dValue))
                    temp._iValue = trueLess ? 0 : 1;
                else
                    temp._iValue = temp._dValue < s._dValue ? 1 : 0;
                return _tempContainer;
            }
            return Check(_tempContainer, s, !trueLess);
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            baseOptimize(ref _this, owner, message, opts, stats);
            if (_this == this)
                if (_left.ResultType == PredictedType.Number && _right.ResultType == PredictedType.Number)
                {
                    _this = new NumberLess(_left, _right);
                    return;
                }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " < " + _right + ")";
        }
    }
}