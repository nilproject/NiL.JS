using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class StrictEqual : Operator
    {
        public StrictEqual(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Double)
                    return l == temp.dValue;
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.iValue;
            }
            if (lvt == ObjectValueType.Double)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    return l == temp.iValue;
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.dValue;
            }
            if (lvt == ObjectValueType.Statement)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.oValue;
            }
            if (lvt == ObjectValueType.Object)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                return l.Equals(temp.oValue);
            }
            if (lvt == ObjectValueType.String)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                return l.Equals(temp.oValue);
            }
            if (lvt == ObjectValueType.Undefined || lvt == ObjectValueType.NotExistInObject)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                return temp.ValueType == ObjectValueType.Undefined || temp.ValueType == ObjectValueType.NotExistInObject;
            }
            throw new InvalidOperationException();
        }
    }
}