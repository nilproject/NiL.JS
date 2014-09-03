using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    internal sealed class Incriment : Expression
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

        public Incriment(CodeNode op, Type type)
            : base(type == Type.Preincriment ? op : null, type == Type.Postincriment ? op : null, type == Type.Postincriment)
        {
            if (type > Type.Postincriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
            if (tempContainer != null)
                tempContainer.assignCallback = null;
        }

        internal override JSObject Evaluate(Context context)
        {
            Function setter = null;
            JSObject prev = null;
            lock (this)
            {
                var val = (first ?? second).EvaluateForAssing(context);
                if (val.valueType == JSObjectType.Property)
                {
                    setter = (val.oValue as Function[])[0];
                    if (context.strict && setter == null)
                        throw new JSException(new TypeError("Can not increment property \"" + (first ?? second) + "\" without setter."));
                    val = (val.oValue as Function[])[1].Invoke(context.objectSource, null).CloneImpl();
                    val.attributes = 0;
                }
                else if (context.strict && (val.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    throw new JSException(new TypeError("Can not incriment readonly \"" + (first ?? second) + "\""));
                switch (val.valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Double:
                        break;
                    case JSObjectType.Bool:
                        {
                            val.valueType = JSObjectType.Int;
                            break;
                        }
                    case JSObjectType.String:
                        {
                            double resd;
                            int i = 0;
                            if (!Tools.ParseNumber(val.oValue as string, i, out resd, Tools.ParseNumberOptions.Default))
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
                                        if (!Tools.ParseNumber(val.oValue as string, i, out resd, Tools.ParseNumberOptions.Default))
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
                    default:
                        {
                            Tools.RaiseIfNotExist(val, first ?? second);
                            if (val.assignCallback != null)
                                val.assignCallback(val);
                            break;
                        }
                }
                if (second != null && val.isDefinded)
                {
                    prev = tempContainer;
                    prev.Assign(val);
                }
                else
                    prev = val;
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
                return prev;
            }
        }

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            base.Optimize(ref _this, depth, vars, strict);
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
            return first != null ? "++" + first : second + "++";
        }
    }
}