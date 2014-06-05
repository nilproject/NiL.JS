using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    internal sealed class Incriment : Operator
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

        public Incriment(Statement op, Type type)
            : base(type == Type.Preincriment ? op : null, type == Type.Postincriment ? op : null, type == Type.Postincriment)
        {
            if (type > Type.Postincriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
            if (tempResult != null)
                tempResult.assignCallback = null;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                if (first != null && second != null)
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid incriment operand.")));
                var val = (first ?? second).InvokeForAssing(context);
                if (val.ValueType == JSObjectType.NotExist)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
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
                            if (val.iValue == 0x7FFFFFFF)
                            {
                                val.dValue = val.iValue + 1.0;
                                val.ValueType = JSObjectType.Double;
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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            base.Optimize(ref _this, depth, vars);
            if (depth <= 1 && second != null)
            {
                first = second;
                second = null;
            }
            return false;
        }

        public override string ToString()
        {
            return first != null ? "++" + first : second + "++";
        }
    }
}