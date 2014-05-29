using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public class StrictEqual : Operator
    {
        public StrictEqual(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var temp = first.Invoke(context);
                var lvt = temp.ValueType;

                switch (lvt)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            var l = temp.iValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (temp.ValueType == JSObjectType.Double)
                                tempResult.iValue = l == temp.dValue ? 1 : 0;
                            else if (lvt != temp.ValueType)
                                tempResult.iValue = 0;
                            else
                                tempResult.iValue = l == temp.iValue ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.Double:
                        {
                            var l = temp.dValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (temp.ValueType == JSObjectType.Int)
                                tempResult.iValue = l == temp.iValue ? 1 : 0;
                            else if (lvt != temp.ValueType)
                                tempResult.iValue = 0;
                            else
                                tempResult.iValue = l == temp.dValue ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.Function:
                        {
                            var l = temp.oValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (lvt != temp.ValueType)
                                tempResult.iValue = 0;
                            else
                                tempResult.iValue = l == temp.oValue ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.Object:
                        {
                            var l = temp.oValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (temp.ValueType != JSObjectType.Object)
                                tempResult.iValue = 0;
                            else if (l == null || temp.oValue == null)
                                tempResult.iValue = l == temp.oValue ? 1 : 0;
                            else
                                tempResult.iValue = l.Equals(temp.oValue) ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.Date:
                        {
                            var l = temp.oValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (temp.ValueType != JSObjectType.Date)
                                tempResult.iValue = 0;
                            else if (l == null || temp.oValue == null)
                                tempResult.iValue = l == temp.oValue ? 1 : 0;
                            else
                                tempResult.iValue = l.Equals(temp.oValue) ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.String:
                        {
                            var l = temp.oValue;
                            temp = Tools.RaiseIfNotExist(second.Invoke(context));
                            if (lvt != temp.ValueType)
                                tempResult.iValue = 0;
                            else
                                tempResult.iValue = l.Equals(temp.oValue) ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                        {
                            var l = temp.dValue;
                            temp = second.Invoke(context);
                            tempResult.iValue = temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject ? 1 : 0;
                            tempResult.ValueType = JSObjectType.Bool;
                            return tempResult;
                        }
                }
                if (lvt == JSObjectType.NotExist)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "(" + first + " === " + second + ")";
        }
    }
}