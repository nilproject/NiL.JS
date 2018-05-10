using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
#if !PORTABLE
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
                var frt = _left.ResultType;
                var srt = _right.ResultType;
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
            var f = _left.Evaluate(context);

            var temp = _tempContainer;
            _tempContainer = null;
            if (temp == null)
                temp = new JSValue { _attributes = JSValueAttributesInternal.Temporary };

            temp._valueType = f._valueType;
            temp._iValue = f._iValue;
            temp._dValue = f._dValue;
            temp._oValue = f._oValue;
            Impl(temp, temp, _right.Evaluate(context));

            _tempContainer = temp;
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
                            case JSValueType.NotExists:
                                {
                                    tstr = new RopeString(tstr, "undefined");
                                    break;
                                }
                            case JSValueType.Object:
                            case JSValueType.Function:
                                {
                                    tstr = new RopeString(tstr, second.ToPrimitiveValue_Value_String().BaseToString());
                                    break;
                                }
                            case JSValueType.Date:
                                {
                                    tstr = new RopeString(tstr, second.ToPrimitiveValue_String_Value().BaseToString());
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
                case JSValueType.NotExists:
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
                            case JSValueType.NotExists:
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (!res && _this == this)
            {
                if (_left is StringConcatenation)
                {
                    _this = _left;
                    var temp = (_left as StringConcatenation)._parts;
                    Array.Resize(ref temp, temp.Length + 1);
                    temp[temp.Length - 1] = _right;
                    (_left as StringConcatenation)._parts = temp;
                }
                else if (_right is StringConcatenation)
                {
                    _this = _right;
                    var temp = (_right as StringConcatenation)._parts;
                    Array.Resize(ref temp, temp.Length + 1);
                    Array.Copy(temp, 0, temp, 1, temp.Length - 1);
                    temp[0] = _left;
                    (_right as StringConcatenation)._parts = temp;
                }
                else
                {
                    if (_left.ContextIndependent && _left.ResultType == PredictedType.String)
                    {
                        if (_left.Evaluate(null).ToString().Length == 0)
                            _this = new ConvertToString(_right);
                        else
                            _this = new StringConcatenation(new[] { _left, _right });
                    }
                    else if (_right.ContextIndependent && _right.ResultType == PredictedType.String)
                    {
                        if (_right.Evaluate(null).ToString().Length == 0)
                            _this = new ConvertToString(_left);
                        else
                            _this = new StringConcatenation(new[] { _left, _right });
                    }
                    else if (_left.ResultType == PredictedType.String
                         || _right.ResultType == PredictedType.String)
                    {
                        _this = new StringConcatenation(new[] { _left, _right });
                    }
                }
            }

            return res;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);

            if (Tools.IsEqual(_left.ResultType, PredictedType.Number, PredictedType.Group)
                && Tools.IsEqual(_right.ResultType, PredictedType.Number, PredictedType.Group))
            {
                _this = new NumberAddition(_left, _right);
                return;
            }
        }
#if !PORTABLE && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var ft = _left.TryCompile(false, false, null, dynamicValues);
            var st = _right.TryCompile(false, false, null, dynamicValues);
            if (ft == st) // null == null
                return null;
            if (ft == null && st != null)
            {
                _right = new CompiledNode(_right, st, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft != null && st == null)
            {
                _left = new CompiledNode(_left, ft, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
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
            return "(" + _left + " + " + _right + ")";
        }
    }
}
