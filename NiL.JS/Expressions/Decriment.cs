using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Decriment : Expression
    {
        public enum Type
        {
            Predecriment,
            Postdecriment
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Decriment(CodeNode op, Type type)
            : base(op, type == Type.Postdecriment ? op : null, type == Type.Postdecriment)
        {
            if (type > Type.Postdecriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        internal override JSObject Evaluate(Context context)
        {
            Function setter = null;
            JSObject res = null;
            var val = first.EvaluateForAssing(context);
            if (val.valueType == JSObjectType.Property)
            {
                setter = (val.oValue as PropertyPair).set;
                if (context.strict && setter == null)
                    throw new JSException(new TypeError("Can not decrement property \"" + (first) + "\" without setter."));
                val = (val.oValue as PropertyPair).get.Invoke(context.objectSource, null).CloneImpl();
                val.attributes = 0;
            }
            else if (context.strict && (val.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                throw new JSException(new TypeError("Can not deccriment readonly \"" + (first) + "\""));
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
            if (second != null && val.isDefinded)
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
                        if (val.iValue == int.MinValue)
                        {
                            val.dValue = val.iValue - 1.0;
                            val.valueType = JSObjectType.Double;
                        }
                        else
                            val.iValue--;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        val.dValue--;
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

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            Parser.Build(ref first, depth + 1, vars, strict);
            if (depth <= 1 && second != null)
            {
                first = second;
                second = null;
            }
            if (first is VariableReference)
                ((first as VariableReference).Descriptor.assignations ??
                    ((first as VariableReference).Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            return false;
        }

        public override string ToString()
        {
            return first != null ? "--" + first : second + "--";
        }
    }
}