using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
    public enum IncrimentType
    {
        Preincriment,
        Postincriment
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class IncrementOperator : Expression
    {
        public override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
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

        public IncrementOperator(Expression op, IncrimentType type)
            : base(op, type == IncrimentType.Postincriment ? op : null, type == IncrimentType.Postincriment)
        {
            if (type > IncrimentType.Postincriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        public override JSValue Evaluate(Context context)
        {
            // Если это постинкремент, то second не будут равен нулю.
            // first всегда содержит узел, из которого нужно получать пременную
            // Определяем тип операции по вторичным признакам.
            Function setter = null;
            JSValue res = null;
            var val = first.EvaluateForWrite(context);
            Arguments args = null;
            if (val.valueType == JSValueType.Property)
            {
                var ppair = val.oValue as PropertyPair;
                setter = ppair.set;
                if (context.strict && setter == null)
                    ExceptionsHelper.ThrowIncrementPropertyWOSetter(first);
                args = new Arguments();
                if (ppair.get == null)
                    val = JSValue.undefined.CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
                else
                    val = ppair.get.Call(context.objectSource, args).CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
            }
            else if ((val.attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (context.strict)
                    ExceptionsHelper.ThrowIncrementReadonly(first);
                val = val.CloneImpl(false);
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
                        Tools.JSObjectToNumber(val, val);
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
                                    Tools.JSObjectToNumber(val, val);
                                    break;
                                }
                            case JSValueType.Date:
                            case JSValueType.Function:
                            case JSValueType.Object: // null
                                {
                                    val.valueType = JSValueType.Int;
                                    val.iValue = 0;
                                    break;
                                }
                        }
                        break;
                    }
                case JSValueType.NotExists:
                    {
                        ExceptionsHelper.ThrowIfNotExists(val, first);
                        break;
                    }
            }
            if (second != null && val.IsDefined)
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
                        if (val.iValue == 0x7FFFFFFF)
                        {
                            val.valueType = JSValueType.Double;
                            val.dValue = val.iValue + 1.0;
                        }
                        else
                            val.iValue++;
                        break;
                    }
                case JSValueType.Double:
                    {
                        val.dValue++;
                        break;
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        val.valueType = JSValueType.Double;
                        val.dValue = double.NaN;
                        break;
                    }
            }
            if (setter != null)
            {
                args.length = 1;
                args[0] = val;
                setter.Call(context.objectSource, args);
            }
            else if ((val.attributes & JSValueAttributesInternal.Reassign) != 0)
                val.Assign(val);
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            _codeContext = codeContext;

            Parser.Build(ref first, depth + 1, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            if (depth <= 1 && second != null)
                second = null;
            var f = first as VariableReference ?? ((first is GetValueForAssignmentOperator) ? (first as GetValueForAssignmentOperator).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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
            return second == null ? "++" + first : first + "++";
        }
    }
}