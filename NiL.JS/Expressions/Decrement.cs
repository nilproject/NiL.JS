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
#if !(PORTABLE || NETCORE)
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
            get { return _tempContainer != null; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                var pd = _left.ResultType;
                if (_tempContainer == null)
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
            var val = _left.EvaluateForWrite(context);
            if (val._valueType == JSValueType.Property)
            {
                var ppair = val._oValue as Core.PropertyPair;
                setter = ppair.setter;
                if (context._strict && setter == null)
                    raiseErrorProp();
                if (ppair.getter == null)
                    val = JSValue.undefined.CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
                else
                    val = ppair.getter.Call(context._objectSource, null).CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
            }
            else if ((val._attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (context._strict)
                    raiseErrorValue();
                val = val.CloneImpl(false);
            }

            switch (val._valueType)
            {
                case JSValueType.Boolean:
                    {
                        val._valueType = JSValueType.Integer;
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
                        switch (val._valueType)
                        {
                            case JSValueType.Boolean:
                                {
                                    val._valueType = JSValueType.Integer;
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
                                    val._valueType = JSValueType.Integer;
                                    val._iValue = 0;
                                    break;
                                }
                        }
                        break;
                    }
                case JSValueType.NotExists:
                    {
                        ExceptionHelper.ThrowIfNotExists(val, _left);
                        break;
                    }
            }

            if (post && val.Defined)
            {
                res = _tempContainer;
                res.Assign(val);
            }
            else
            {
                res = val;
            }

            switch (val._valueType)
            {
                case JSValueType.Integer:
                    {
                        if (val._iValue == int.MinValue)
                        {
                            val._valueType = JSValueType.Double;
                            val._dValue = val._iValue - 1.0;
                        }
                        else
                            val._iValue--;
                        break;
                    }
                case JSValueType.Double:
                    {
                        val._dValue--;
                        break;
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                case JSValueType.NotExists:
                    {
                        val._valueType = JSValueType.Double;
                        val._dValue = double.NaN;
                        break;
                    }
            }

            if (setter != null)
            {
                var args = new Arguments(context);
                args.length = 1;
                args[0] = val;
                setter.Call(context._objectSource, args);
            }
            else if ((val._attributes & JSValueAttributesInternal.Reassign) != 0)
            {
                val.Assign(val);
            }
            return res;
        }

        private void raiseErrorValue()
        {
            ExceptionHelper.Throw(new TypeError("Can not decrement readonly \"" + (_left) + "\""));
        }

        private void raiseErrorProp()
        {
            ExceptionHelper.Throw(new TypeError("Can not decrement property \"" + (_left) + "\" without setter."));
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            Parser.Build(ref _left, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            if (expressionDepth <= 1 && _type == DecrimentType.Postdecriment)
                _type = DecrimentType.Predecriment;
            var f = _left as VariableReference ?? ((_left is AssignmentOperatorCache) ? (_left as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignments ??
                    (f.Descriptor.assignments = new System.Collections.Generic.List<Expression>())).Add(this);
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var vr = _left as VariableReference;
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
            return _type == DecrimentType.Predecriment ? "--" + _left : _left + "--";
        }
    }
}