using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Decriment : Operator
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

        public Decriment(Statement op, Type type)
            : base(type == Type.Predecriment ? op : null, type == Type.Postdecriment ? op : null)
        {
            if (type > Type.Postdecriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
            tempResult.assignCallback = null;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                if (first != null && second != null)
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid decriment operand.")));
                var val = Tools.RaiseIfNotExist((first ?? second).Invoke(context));
                if (val.assignCallback != null)
                    val.assignCallback(val);
                if ((val.attributes & JSObjectAttributes.ReadOnly) != 0)
                    return double.NaN;
                switch (val.ValueType)
                {
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                        {
                            val.Assign(val.ToPrimitiveValue_Value_String());
                            break;
                        }
                }
                switch (val.ValueType)
                {
                    case JSObjectType.Bool:
                        {
                            val.ValueType = JSObjectType.Int;
                            break;
                        }
                    case JSObjectType.String:
                        {
                            double resd;
                            int i = 0;
                            if (!Tools.ParseNumber(val.oValue as string, ref i, false, out resd))
                                resd = double.NaN;
                            val.ValueType = JSObjectType.Double;
                            val.dValue = resd;
                            break;
                        }
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.Object: // null
                        {
                            val.iValue = 0;
                            val.ValueType = JSObjectType.Int;
                            break;
                        }
                }
                JSObject o = null;
                if ((second != null) && (val.ValueType != JSObjectType.Undefined) && (val.ValueType != JSObjectType.NotExistInObject))
                {
                    o = tempResult;
                    o.Assign(val);
                }
                else
                    o = val;
                switch (val.ValueType)
                {
                    case JSObjectType.Int:
                        {
                            if (val.iValue == -0x80000000)
                            {
                                val.dValue = val.iValue - 1.0;
                                val.ValueType = JSObjectType.Double;
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
                    case JSObjectType.NotExistInObject:
                        {
                            val.ValueType = JSObjectType.Double;
                            val.dValue = double.NaN;
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                return o;
            }
        }

        public override string ToString()
        {
            return first != null ? "--" + first : second + "--";
        }
    }
}