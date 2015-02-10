using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Incriment : Expression
    {
        public enum Type
        {
            Preincriment,
            Postincriment
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
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

        public Incriment(Expression op, Type type)
            : base(op, type == Type.Postincriment ? op : null, type == Type.Postincriment)
        {
            if (type > Type.Postincriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        internal override JSObject Evaluate(Context context)
        {
            // Если это постинкремент, то second не будут равен нулю.
            // first всегда содержит узел, из которого нужно получать пременную
            // Определяем тип операции по вторичным признакам.
            Function setter = null;
            JSObject res = null;
            var val = first.EvaluateForAssing(context);
            if (val.valueType == JSObjectType.Property)
            {
                setter = (val.oValue as PropertyPair).set;
                if (context.strict && setter == null)
                    throw new JSException(new TypeError("Can not increment property \"" + (first) + "\" without setter."));
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
                case JSObjectType.Bool:
                    {
                        val.valueType = JSObjectType.Int;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double resd;
                        int i = 0;
                        if (!Tools.ParseNumber(val.oValue.ToString(), i, out resd, Tools.ParseNumberOptions.Default))
                            resd = double.NaN;
                        val.valueType = JSObjectType.Double;
                        val.dValue = resd;
                        break;
                    }
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                    {
                        val.Assign(val.ToPrimitiveValue_Value_String());
                        switch (val.valueType)
                        {
                            case JSObjectType.Bool:
                                {
                                    val.valueType = JSObjectType.Int;
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    double resd;
                                    int i = 0;
                                    if (!Tools.ParseNumber(val.oValue.ToString(), i, out resd, Tools.ParseNumberOptions.Default))
                                        resd = double.NaN;
                                    val.valueType = JSObjectType.Double;
                                    val.dValue = resd;
                                    break;
                                }
                            case JSObjectType.Date:
                            case JSObjectType.Function:
                            case JSObjectType.Object: // null
                                {
                                    val.iValue = 0;
                                    val.valueType = JSObjectType.Int;
                                    break;
                                }
                        }
                        break;
                    }
                case JSObjectType.NotExists:
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
                case JSObjectType.Int:
                    {
                        if (val.iValue == 0x7FFFFFFF)
                        {
                            val.dValue = val.iValue + 1.0;
                            val.valueType = JSObjectType.Double;
                        }
                        else
                            val.iValue++;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        val.dValue++;
                        break;
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        val.valueType = JSObjectType.Double;
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            Parser.Build(ref first, depth + 1, vars, strict, message, statistic, opts);
            if (depth <= 1 && second != null)
                second = null;
            var f = first as VariableReference ?? ((first is OpAssignCache) ? (first as OpAssignCache).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
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