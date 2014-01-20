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
            if (lvt == JSObjectType.Int || lvt == JSObjectType.Bool)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.Double)
                    return l == temp.dValue;
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.iValue;
            }
            if (lvt == JSObjectType.Double)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (double.IsNaN(l))
                    tempResult.iValue = this is StrictNotEqual ? 1 : 0; // Костыль. Для его устранения нужно делать полноценную реализацию оператора StrictNotEqual.
                if (temp.ValueType == JSObjectType.Int)
                    return l == temp.iValue;
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.dValue;
            }
            if (lvt == JSObjectType.Function)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                return l == temp.oValue;
            }
            if (lvt == JSObjectType.Object)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                if (l == null || temp.oValue == null)
                    return l == temp.oValue;
                return l.Equals(temp.oValue);
            }
            if (lvt == JSObjectType.String)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                return l.Equals(temp.oValue);
            }
            if (lvt == JSObjectType.Undefined || lvt == JSObjectType.NotExistInObject)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                return temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject;
            }
            throw new InvalidOperationException();
        }
    }
}