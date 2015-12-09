using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using System.Collections.Generic;

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
    public sealed class Decrement : Expression
    {
        private DecrimentType _type;

        public DecrimentType Type
        {
            get
            {
                return _type;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return tempContainer != null; }
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

        protected internal override bool LValueModifier
        {
            get
            {
                return true;
            }
        }

        public Decrement(Expression op, DecrimentType type)
            : base(op, null, type == DecrimentType.Postdecriment)
        {
            if (op == null)
                throw new ArgumentNullException("op");

            _type = type;
        }

        public override JSValue Evaluate(Context context)
        {
            bool post = _type == DecrimentType.Postdecriment;
            Function setter = null;
            JSValue res = null;
            var val = first.EvaluateForWrite(context);
            Arguments args = null;
            if (val.valueType == JSValueType.Property)
            {
                var ppair = val.oValue as GsPropertyPair;
                setter = ppair.set;
                if (context.strict && setter == null)
                    raiseErrorProp();
                args = new Arguments();
                if (ppair.get == null)
                    val = JSValue.undefined.CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
                else
                    val = ppair.get.Call(context.objectSource, args).CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
            }
            else if ((val.attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (context.strict)
                    raiseErrorValue();
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
            if (post && val.Defined)
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
                            val.valueType = JSValueType.Double;
                            val.dValue = val.iValue - 1.0;
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

        private void raiseErrorValue()
        {
            ExceptionsHelper.Throw(new TypeError("Can not decrement readonly \"" + (first) + "\""));
        }

        private void raiseErrorProp()
        {
            ExceptionsHelper.Throw(new TypeError("Can not decrement property \"" + (first) + "\" without setter."));
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            Parser.Build(ref first, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            if (expressionDepth <= 1 && _type == DecrimentType.Postdecriment)
                _type = DecrimentType.Predecriment;
            var f = first as VariableReference ?? ((first is AssignmentOperatorCache) ? (first as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignments ??
                    (f.Descriptor.assignments = new System.Collections.Generic.List<Expression>())).Add(this);
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var vr = first as VariableReference;
            if (vr != null && vr._descriptor.IsDefined)
            {
                var pd = vr._descriptor.lastPredictedType;
                switch (pd)
                {
                    case PredictedType.Int:
                    case PredictedType.Unknown:
                        {
                            vr._descriptor.lastPredictedType = PredictedType.Number;
                            break;
                        }
                    case PredictedType.Double:
                        {
                            // кроме как double он ничем больше оказаться не может. Даже NaN это double
                            break;
                        }
                    default:
                        {
                            vr._descriptor.lastPredictedType = PredictedType.Ambiguous;
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
            return _type == DecrimentType.Predecriment ? "--" + first : first + "--";
        }
    }
}