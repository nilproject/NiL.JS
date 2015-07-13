using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
    public enum DecrimentType
    {
        Predecriment,
        Postdecriment
    }
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DecrementOperator : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return second != null; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                var pd = first.ResultType;
                if (tempContainer == null)
                {
                    switch (pd)
                    {
                        case PredictedType.Double:
                            {
                                return PredictedType.Double;
                            }
                        default:
                            {
                                return PredictedType.Number;
                            }
                    }
                }
                return pd;
            }
        }

        public DecrementOperator(Expression op, DecrimentType type)
            : base(op, type == DecrimentType.Postdecriment ? op : null, type == DecrimentType.Postdecriment)
        {
            if (type > DecrimentType.Postdecriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        internal override JSValue Evaluate(Context context)
        {
            Function setter = null;
            JSValue res = null;
            var val = first.EvaluateForAssing(context);
            if (val.valueType == JSValueType.Property)
            {
                setter = (val.oValue as PropertyPair).set;
                if (context.strict && setter == null)
                    throw new JSException(new TypeError("Can not decrement property \"" + (first) + "\" without setter."));
                val = (val.oValue as PropertyPair).get.Invoke(context.objectSource, null).CloneImpl();
                val.attributes = 0;
            }
            else if ((val.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
            {
                if (context.strict)
                    throw new JSException(new TypeError("Can not deccriment readonly \"" + (first) + "\""));
                val = val.CloneImpl();
            }
            switch (val.valueType)
            {
                case JSValueType.Bool:
                    {
                        val.valueType = JSValueType.Int;
                        break;
                    }
                case JSValueType.String:
                    {
                        double resd;
                        int i = 0;
                        if (!Tools.ParseNumber(val.oValue.ToString(), i, out resd, Tools.ParseNumberOptions.Default))
                            resd = double.NaN;
                        val.valueType = JSValueType.Double;
                        val.dValue = resd;
                        break;
                    }
                case JSValueType.Object:
                case JSValueType.Date:
                case JSValueType.Function:
                    {
                        val.Assign(val.ToPrimitiveValue_Value_String());
                        switch (val.valueType)
                        {
                            case JSValueType.Bool:
                                {
                                    val.valueType = JSValueType.Int;
                                    break;
                                }
                            case JSValueType.String:
                                {
                                    double resd;
                                    int i = 0;
                                    if (!Tools.ParseNumber(val.oValue.ToString(), i, out resd, Tools.ParseNumberOptions.Default))
                                        resd = double.NaN;
                                    val.valueType = JSValueType.Double;
                                    val.dValue = resd;
                                    break;
                                }
                            case JSValueType.Date:
                            case JSValueType.Function:
                            case JSValueType.Object: // null
                                {
                                    val.iValue = 0;
                                    val.valueType = JSValueType.Int;
                                    break;
                                }
                        }
                        break;
                    }
                case JSValueType.NotExists:
                    {
                        Tools.RaiseIfNotExist(val, first);
                        break;
                    }
            }
            if (second != null && val.IsDefinded)
            {
                res = tempContainer;
                res.Assign(val);
            }
            else
                res = val;
            switch (val.valueType)
            {
                case JSValueType.Int:
                    {
                        if (val.iValue == int.MinValue)
                        {
                            val.dValue = val.iValue - 1.0;
                            val.valueType = JSValueType.Double;
                        }
                        else
                            val.iValue--;
                        break;
                    }
                case JSValueType.Double:
                    {
                        val.dValue--;
                        break;
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        val.valueType = JSValueType.Double;
                        val.dValue = double.NaN;
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
            if (setter != null)
            {
                var args = new Arguments();
                args.length = 1;
                args[0] = val;
                setter.Invoke(context.objectSource, args);
            }
            else if ((val.attributes & JSObjectAttributesInternal.Reassign) != 0)
                val.Assign(val);
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            Parser.Build(ref first, depth + 1, variables, state, message, statistic, opts);
            if (depth <= 1 && second != null)
            {
                first = second;
                second = null;
            }
            var f = first as VariableReference ?? ((first is GetValueForAssignmentOperator) ? (first as GetValueForAssignmentOperator).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var vr = first as VariableReference;
            if (vr != null && vr.descriptor.isDefined)
            {
                var pd = vr.descriptor.lastPredictedType;
                switch (pd)
                {
                    case PredictedType.Int:
                    case PredictedType.Unknown:
                        {
                            vr.descriptor.lastPredictedType = PredictedType.Number;
                            break;
                        }
                    case PredictedType.Double:
                        {
                            // кроме как double он ничем больше оказаться не может. Даже NaN это double
                            break;
                        }
                    default:
                        {
                            vr.descriptor.lastPredictedType = PredictedType.Ambiguous;
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
            return first != null ? "--" + first : second + "--";
        }
    }
}