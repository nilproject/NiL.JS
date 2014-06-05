using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public class StrictEqual : Operator
    {
        public StrictEqual(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal static bool Check(JSObject first, Statement second, Context context)
        {
            var temp = first;
            var lvt = temp.ValueType;

            switch (lvt)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        var l = temp.iValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (temp.ValueType == JSObjectType.Double)
                            return l == temp.dValue;
                        else if (lvt != temp.ValueType)
                            return false;
                        else
                            return l == temp.iValue;
                    }
                case JSObjectType.Double:
                    {
                        var l = temp.dValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (temp.ValueType == JSObjectType.Int)
                            return l == temp.iValue;
                        else if (lvt != temp.ValueType)
                            return false;
                        else
                            return l == temp.dValue;
                    }
                case JSObjectType.Function:
                    {
                        var l = temp.oValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (lvt != temp.ValueType)
                            return false;
                        else
                            return l == temp.oValue;
                    }
                case JSObjectType.Object:
                    {
                        var l = temp.oValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (temp.ValueType != JSObjectType.Object)
                            return false;
                        else if (l == null || temp.oValue == null)
                            return l == temp.oValue;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.Date:
                    {
                        var l = temp.oValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (temp.ValueType != JSObjectType.Date)
                            return false;
                        else if (l == null || temp.oValue == null)
                            return l == temp.oValue;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.String:
                    {
                        var l = temp.oValue;
                        temp = Tools.RaiseIfNotExist(second.Invoke(context));
                        if (lvt != temp.ValueType)
                            return false;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        var l = temp.dValue;
                        temp = second.Invoke(context);
                        return temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject;
                    }
            }
            if (lvt == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
            throw new NotImplementedException();
        }

        private static readonly ImmidateValueStatement ivsInstance = new ImmidateValueStatement(null);
        internal static bool Check(JSObject first, JSObject second)
        {
            lock (ivsInstance)
            {
                ivsInstance.value = second;
                return Check(first, ivsInstance, null);
            }
        }

        internal override JSObject Invoke(Context context)
        {
            return Check(first.Invoke(context), second, context);
        }

        public override string ToString()
        {
            return "(" + first + " === " + second + ")";
        }
    }
}