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
                    tempResult.iValue = l == temp.dValue ? 1 : 0;
                else if (lvt != temp.ValueType)
                    tempResult.iValue = 0;
                else
                    tempResult.iValue = l == temp.iValue ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.Double)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (double.IsNaN(l))
                    tempResult.iValue = this is StrictNotEqual ? 1 : 0; // Костыль. Для его устранения нужно делать полноценную реализацию оператора StrictNotEqual.
                if (temp.ValueType == JSObjectType.Int)
                    tempResult.iValue = l == temp.iValue ? 1 : 0;
                else if (lvt != temp.ValueType)
                    tempResult.iValue = 0;
                else
                    tempResult.iValue = l == temp.dValue ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.Function)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    tempResult.iValue = 0;
                else
                    tempResult.iValue = l == temp.oValue ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.Object || lvt == JSObjectType.Proxy)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (temp.ValueType != JSObjectType.Proxy && temp.ValueType != JSObjectType.Object)
                    tempResult.iValue = 0;
                else if (l == null || temp.oValue == null)
                    tempResult.iValue = l == temp.oValue ? 1 : 0;
                else
                    tempResult.iValue = l.Equals(temp.oValue) ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.String)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return false;
                else
                    tempResult.iValue = l.Equals(temp.oValue) ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.Undefined || lvt == JSObjectType.NotExistInObject)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                tempResult.iValue = temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject ? 1 : 0;
                tempResult.ValueType = JSObjectType.Bool;
                return tempResult;
            }
            if (lvt == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
            throw new InvalidOperationException();
        }
    }
}