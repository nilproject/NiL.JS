using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public class StrictEqual : Expression
    {
        public StrictEqual(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal static bool Check(JSObject first, CodeNode second, Context context)
        {
            var temp = first;
            var lvt = temp.valueType;

            switch (lvt)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        var l = temp.iValue;
                        temp = second.Evaluate(context);
                        if (temp.valueType == JSObjectType.Double)
                            return l == temp.dValue;
                        else if (lvt != temp.valueType)
                            return false;
                        else
                            return l == temp.iValue;
                    }
                case JSObjectType.Double:
                    {
                        var l = temp.dValue;
                        temp = second.Evaluate(context);
                        if (temp.valueType == JSObjectType.Int)
                            return l == temp.iValue;
                        else if (lvt != temp.valueType)
                            return false;
                        else
                            return l == temp.dValue;
                    }
                case JSObjectType.Function:
                    {
                        var l = temp.oValue;
                        temp = second.Evaluate(context);
                        if (lvt != temp.valueType)
                            return false;
                        else
                            return l == temp.oValue;
                    }
                case JSObjectType.Object:
                    {
                        var l = temp.oValue;
                        temp = second.Evaluate(context);
                        if (temp.valueType != JSObjectType.Object)
                            return false;
                        else if (l == null || temp.oValue == null)
                            return l == temp.oValue;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.Date:
                    {
                        var l = temp.oValue;
                        temp = second.Evaluate(context);
                        if (temp.valueType != JSObjectType.Date)
                            return false;
                        else if (l == null || temp.oValue == null)
                            return l == temp.oValue;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.String:
                    {
                        var l = temp.oValue;
                        temp = second.Evaluate(context);
                        if (lvt != temp.valueType)
                            return false;
                        else
                            return l.Equals(temp.oValue);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        temp = second.Evaluate(context);
                        return !temp.isDefinded;
                    }
                case JSObjectType.Property:
                    return false;
            }
            if (lvt == JSObjectType.NotExists)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
            throw new NotImplementedException();
        }

        internal override JSObject Evaluate(Context context)
        {
            return Check(first.Evaluate(context), second, context);
        }

        public override string ToString()
        {
            return "(" + first + " === " + second + ")";
        }
    }
}