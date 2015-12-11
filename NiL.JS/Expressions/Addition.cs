using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Expressions
{
#if !PORTABLE
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
                temp = new JSValue { attributes = JSValueAttributesInternal.Temporary };
            temp.valueType = f.valueType;
            temp.iValue = f.iValue;
            temp.dValue = f.dValue;
            temp.oValue = f.oValue;
            Impl(temp, temp, second.Evaluate(context));
            tempContainer = temp;
            return temp;
        }

        internal static void Impl(JSValue resultContainer, JSValue first, JSValue second)
        {
            switch (first.valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        if (second.valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSValueType.Integer:
                            case JSValueType.Boolean:
                                {
                                    long tl = (long)first.iValue + second.iValue;
                                    if ((int)tl == tl)
                                    {
                                        resultContainer.valueType = JSValueType.Integer;
                                        resultContainer.iValue = (int)tl;
                                    }
                                    else
                                    {
                                        resultContainer.valueType = JSValueType.Double;
                                        resultContainer.dValue = (double)tl;
                                    }
                                    return;
                                }
                            case JSValueType.Double:
                                {
                                    resultContainer.valueType = JSValueType.Double;
                                    resultContainer.dValue = first.iValue + second.dValue;
                                    return;
                                }
                            case JSValueType.String:
                                {
                                    resultContainer.oValue = new RopeString((first.valueType == JSValueType.Boolean ? (first.iValue != 0 ? "true" : "false") : first.iValue.ToString(CultureInfo.InvariantCulture)), second.oValue);
                                    resultContainer.valueType = JSValueType.String;
                                    return;
                                }
                            case JSValueType.NotExists:
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSValueType.Double;
                                    return;
                                }
                            case JSValueType.Object: // x+null
                                {
                                    resultContainer.iValue = first.iValue;
                                    resultContainer.valueType = JSValueType.Integer;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.Double:
                    {
                        if (second.valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSValueType.Integer:
                            case JSValueType.Boolean:
                                {
                                    resultContainer.valueType = JSValueType.Double;
                                    resultContainer.dValue = first.dValue + second.iValue;
                                    return;
                                }
                            case JSValueType.Double:
                                {
                                    resultContainer.valueType = JSValueType.Double;
                                    resultContainer.dValue = first.dValue + second.dValue;
                                    return;
                                }
                            case JSValueType.String:
                                {
                                    resultContainer.oValue = new RopeString(Tools.DoubleToString(first.dValue), second.oValue);
                                    resultContainer.valueType = JSValueType.String;
                                    return;
                                }
                            case JSValueType.Object: // null
                                {
                                    resultContainer.dValue = first.dValue;
                                    resultContainer.valueType = JSValueType.Double;
                                    return;
                                }
                            case JSValueType.NotExists:
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer.dValue = double.NaN;
                                    resultContainer.valueType = JSValueType.Double;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.String:
                    {
                        object tstr = first.oValue;
                        switch (second.valueType)
                        {
                            case JSValueType.String:
                                {
                                    tstr = new RopeString(tstr, second.oValue);
                                    break;
                                }
                            case JSValueType.Boolean:
                                {
                                    tstr = new RopeString(tstr, second.iValue != 0 ? "true" : "false");
                                    break;
                                }
                            case JSValueType.Integer:
                                {
                                    tstr = new RopeString(tstr, second.iValue.ToString(CultureInfo.InvariantCulture));
                                    break;
                                }
                            case JSValueType.Double:
                                {
                                    tstr = new RopeString(tstr, Tools.DoubleToString(second.dValue));
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
                        resultContainer.oValue = tstr;
                        resultContainer.valueType = JSValueType.String;
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
                        if (second.valueType >= JSValueType.Object)
                            second = second.ToPrimitiveValue_Value_String();
                        switch (second.valueType)
                        {
                            case JSValueType.String:
                                {
                                    resultContainer.valueType = JSValueType.String;
                                    resultContainer.oValue = new RopeString("undefined", second.oValue);
                                    return;
                                }
                            case JSValueType.Double:
                            case JSValueType.Boolean:
                            case JSValueType.Integer:
                                {
                                    resultContainer.valueType = JSValueType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                            case JSValueType.Object: // undefined+null
                            case JSValueType.NotExistsInObject:
                            case JSValueType.Undefined:
                                {
                                    resultContainer.valueType = JSValueType.Double;
                                    resultContainer.dValue = double.NaN;
                                    return;
                                }
                        }
                        break;
                    }
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first.valueType == JSValueType.Integer || first.valueType == JSValueType.Boolean)
                            goto case JSValueType.Integer;
                        else if (first.valueType == JSValueType.Object) // null
                        {
                            if (second.valueType >= JSValueType.String)
                                second = second.ToPrimitiveValue_Value_String();
                            if (second.valueType == JSValueType.String)
                            {
                                resultContainer.oValue = new RopeString("null", second.oValue);
                                resultContainer.valueType = JSValueType.String;
                                return;
                            }
                            first.iValue = 0;
                            goto case JSValueType.Integer;
                        }
                        else if (first.valueType == JSValueType.Double)
                            goto case JSValueType.Double;
                        else if (first.valueType == JSValueType.String)
                            goto case JSValueType.String;
                        break;
                    }
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
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
                    if (first.ContextIndependent && first.Evaluate(null).valueType == JSValueType.String)
                    {
                        if (first.Evaluate(null).ToString().Length == 0)
                            _this = new ConvertToString(second);
                        else
                            _this = new StringConcatenation(new List<Expression>() { first, second });
                    }
                    else if (second.ContextIndependent && second.Evaluate(null).valueType == JSValueType.String)
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
#if !PORTABLE && !NET35
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

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            throw new NotImplementedException();
        }
    }
}
