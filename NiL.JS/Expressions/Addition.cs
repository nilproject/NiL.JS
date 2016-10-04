using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
#if !(PORTABLE || NETCORE)
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Addition : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var frt = first.ResultType;
                var srt = second.ResultType;
                if (frt == PredictedType.String || srt == PredictedType.String)
                    return PredictedType.String;
                if (frt == srt)
                {
                    switch (frt)
                    {
                        case PredictedType.Bool:
                        case PredictedType.Int:
                            return PredictedType.Number;
                        case PredictedType.Double:
                            return PredictedType.Double;
                    }
                }
                if (frt == PredictedType.Bool)
                {
                    if (srt == PredictedType.Double)
                        return PredictedType.Double;
                    if (Tools.IsEqual(srt, PredictedType.Number, PredictedType.Group))
                        return PredictedType.Number;
                }
                if (srt == PredictedType.Bool)
                {
                    if (frt == PredictedType.Double)
                        return PredictedType.Double;
                    if (Tools.IsEqual(frt, PredictedType.Number, PredictedType.Group))
                        return PredictedType.Number;
                }
                return PredictedType.Unknown;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Addition(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var f = first.Evaluate(context);

            var temp = tempContainer;
            tempContainer = null;
            if (temp == null)
                temp = new JSValue { _attributes = JSValueAttributesInternal.Temporary };

            temp._valueType = f._valueType;
            temp._iValue = f._iValue;
            temp._dValue = f._dValue;
            temp._oValue = f._oValue;
            Impl(temp, temp, second.Evaluate(context));

            tempContainer = temp;
            return temp;
        }

        internal static void Impl(JSValue resultContainer, JSValue first, JSValue second)
        {
            switch (first._valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        if (second._valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second._valueType)
                        {
                            case JSValueType.Integer:
                            case JSValueType.Boolean:
                                {
                                    long tl = (long)first._iValue + second._iValue;
                                    if ((int)tl == tl)
                                    {
                                        resultContainer._valueType = JSValueType.Integer;
                                        resultContainer._iValue = (int)tl;
                                    }
                                    else
                                    {
                                        resultContainer._valueType = JSValueType.Double;
                                        resultContainer._dValue = (double)tl;
                                    }
                                    return;
                                }
                            case JSValueType.Double:
                                {
                                    resultContainer._valueType = JSValueType.Double;
                                    resultContainer._dValue = first._iValue + second._dValue;
                                    return;
                                }
                            case JSValueType.String:
                                {
                                    resultContainer._oValue = new RopeString((first._valueType == JSValueType.Boolean ? (first._iValue != 0 ? "true" : "false") : first._iValue.ToString(CultureInfo.InvariantCulture)), second._oValue);
                                    resultContainer._valueType = JSValueType.String;
                                    return;
                                }
                            case JSValueType.NotExists:
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer._dValue = double.NaN;
                                    resultContainer._valueType = JSValueType.Double;
                                    return;
                                }
                            case JSValueType.Object: // x+null
                                {
                                    resultContainer._iValue = first._iValue;
                                    resultContainer._valueType = JSValueType.Integer;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.Double:
                    {
                        if (second._valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second._valueType)
                        {
                            case JSValueType.Integer:
                            case JSValueType.Boolean:
                                {
                                    resultContainer._valueType = JSValueType.Double;
                                    resultContainer._dValue = first._dValue + second._iValue;
                                    return;
                                }
                            case JSValueType.Double:
                                {
                                    resultContainer._valueType = JSValueType.Double;
                                    resultContainer._dValue = first._dValue + second._dValue;
                                    return;
                                }
                            case JSValueType.String:
                                {
                                    resultContainer._oValue = new RopeString(Tools.DoubleToString(first._dValue), second._oValue);
                                    resultContainer._valueType = JSValueType.String;
                                    return;
                                }
                            case JSValueType.Object: // null
                                {
                                    resultContainer._dValue = first._dValue;
                                    resultContainer._valueType = JSValueType.Double;
                                    return;
                                }
                            case JSValueType.NotExists:
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer._dValue = double.NaN;
                                    resultContainer._valueType = JSValueType.Double;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.String:
                    {
                        object tstr = first._oValue;
                        switch (second._valueType)
                        {
                            case JSValueType.String:
                                {
                                    tstr = new RopeString(tstr, second._oValue);
                                    break;
                                }
                            case JSValueType.Boolean:
                                {
                                    tstr = new RopeString(tstr, second._iValue != 0 ? "true" : "false");
                                    break;
                                }
                            case JSValueType.Integer:
                                {
                                    tstr = new RopeString(tstr, second._iValue.ToString(CultureInfo.InvariantCulture));
                                    break;
                                }
                            case JSValueType.Double:
                                {
                                    tstr = new RopeString(tstr, Tools.DoubleToString(second._dValue));
                                    break;
                                }
                            case JSValueType.Undefined:
                            case JSValueType.NotExistsInObject:
                                {
                                    tstr = new RopeString(tstr, "undefined");
                                    break;
                                }
                            case JSValueType.Object:
                            case JSValueType.Function:
                            case JSValueType.Date:
                                {
                                    tstr = new RopeString(tstr, second.ToString());
                                    break;
                                }
                        }
                        resultContainer._oValue = tstr;
                        resultContainer._valueType = JSValueType.String;
                        return;
                    }
                case JSValueType.Date:
                    {
                        first = first.ToPrimitiveValue_String_Value();
                        Impl(resultContainer, first, second);
                        return;
                    }
                case JSValueType.NotExistsInObject:
                case JSValueType.Undefined:
                    {
                        if (second._valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second._valueType)
                        {
                            case JSValueType.String:
                                {
                                    resultContainer._valueType = JSValueType.String;
                                    resultContainer._oValue = new RopeString("undefined", second._oValue);
                                    return;
                                }
                            case JSValueType.Double:
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    resultContainer._valueType = JSValueType.Double;
                                    resultContainer._dValue = double.NaN;
                                    return;
                                }
                            case JSValueType.Object: // undefined+null
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer._valueType = JSValueType.Double;
                                    resultContainer._dValue = double.NaN;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first._valueType == JSValueType.Integer || first._valueType == JSValueType.Boolean)
                            goto case JSValueType.Integer;
                        else if (first._valueType == JSValueType.Object) // null
                        {
                            if (second._valueType >= JSValueType.String)
                                second = second.ToPrimitiveValue_Value_String();
                            if (second._valueType == JSValueType.String)
                            {
                                resultContainer._oValue = new RopeString("null", second._oValue);
                                resultContainer._valueType = JSValueType.String;
                                return;
                            }
                            first._iValue = 0;
                            goto case JSValueType.Integer;
                        }
                        else if (first._valueType == JSValueType.Double)
                            goto case JSValueType.Double;
                        else if (first._valueType == JSValueType.String)
                            goto case JSValueType.String;
                        break;
                    }
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (!res && _this == this)
            {
                if (first is StringConcatenation)
                {
                    _this = first;
                    (first as StringConcatenation)._parts.Add(second);
                }
                else if (second is StringConcatenation)
                {
                    _this = second;
                    (second as StringConcatenation)._parts.Insert(0, first);
                }
                else
                {
                    if (first.ContextIndependent && first.Evaluate(null)._valueType == JSValueType.String)
                    {
                        if (first.Evaluate(null).ToString().Length == 0)
                            _this = new ConvertToString(second);
                        else
                            _this = new StringConcatenation(new List<Expression>() { first, second });
                    }
                    else if (second.ContextIndependent && second.Evaluate(null)._valueType == JSValueType.String)
                    {
                        if (second.Evaluate(null).ToString().Length == 0)
                            _this = new ConvertToString(first);
                        else
                            _this = new StringConcatenation(new List<Expression>() { first, second });
                    }
                }
            }
            return res;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);

            if (Tools.IsEqual(first.ResultType, PredictedType.Number, PredictedType.Group)
                && Tools.IsEqual(second.ResultType, PredictedType.Number, PredictedType.Group))
            {
                _this = new NumberAddition(first, second);
                return;
            }
        }
#if !(PORTABLE || NETCORE) && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var ft = first.TryCompile(false, false, null, dynamicValues);
            var st = second.TryCompile(false, false, null, dynamicValues);
            if (ft == st) // null == null
                return null;
            if (ft == null && st != null)
            {
                second = new CompiledNode(second, st, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft != null && st == null)
            {
                first = new CompiledNode(first, ft, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft.Type == st.Type)
                return System.Linq.Expressions.Expression.Add(ft, st);
            return null;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
