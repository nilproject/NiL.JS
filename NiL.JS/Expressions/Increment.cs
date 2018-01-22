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

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Increment : Expression
    {
        private IncrimentType _type;

        public IncrimentType Type
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

        public Increment(Expression op, IncrimentType type)
            : base(op, null, type == IncrimentType.Postincriment)
        {
            if (op == null)
                throw new ArgumentNullException("op");

            _type = type;
        }

        public override JSValue Evaluate(Context context)
        {
            bool post = _type == IncrimentType.Postincriment;
            Function setter = null;
            JSValue res = null;
            var val = _left.EvaluateForWrite(context);
            Arguments args = null;
            if (val._valueType == JSValueType.Property)
            {
                var ppair = val._oValue as Core.PropertyPair;
                setter = ppair.setter;
                if (context._strict && setter == null)
                    ExceptionHelper.ThrowIncrementPropertyWOSetter(_left);
                args = new Arguments();
                if (ppair.getter == null)
                    val = JSValue.undefined.CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
                else
                    val = ppair.getter.Call(context._objectSource, args).CloneImpl(unchecked((JSValueAttributesInternal)(-1)));
            }
            else if ((val._attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (context._strict)
                    ExceptionHelper.ThrowIncrementReadonly(_left);
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
                res = val;
            switch (val._valueType)
            {
                case JSValueType.Integer:
                    {
                        if (val._iValue == 0x7FFFFFFF)
                        {
                            val._valueType = JSValueType.Double;
                            val._dValue = val._iValue + 1.0;
                        }
                        else
                            val._iValue++;
                        break;
                    }
                case JSValueType.Double:
                    {
                        val._dValue++;
                        break;
                    }
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        val._valueType = JSValueType.Double;
                        val._dValue = double.NaN;
                        break;
                    }
            }
            if (setter != null)
            {
                args.length = 1;
                args[0] = val;
                setter.Call(context._objectSource, args);
            }
            else if ((val._attributes & JSValueAttributesInternal.Reassign) != 0)
                val.Assign(val);
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            Parser.Build(ref _left, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            if (expressionDepth <= 1 && _type == IncrimentType.Postincriment)
                _type = IncrimentType.Preincriment;
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
            return _type == IncrimentType.Preincriment ? "++" + _left : _left + "++";
        }
    }
}