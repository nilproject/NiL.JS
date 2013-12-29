using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class TypeOf : Operator
    {
        public TypeOf(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = first.Invoke(context);
            var vt = val.ValueType;
            switch (vt)
            {
                case ObjectValueType.Int:
                case ObjectValueType.Double:
                    {
                        return "number";
                    }
                case ObjectValueType.NotExist:
                case ObjectValueType.NotExistInObject:
                case ObjectValueType.Undefined:
                    {
                        return "undefined";
                    }
                case ObjectValueType.String:
                    {
                        return "string";
                    }
                case ObjectValueType.Bool:
                    {
                        return "boolean";
                    }
                case ObjectValueType.Statement:
                    {
                        return "function";
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        if (val.oValue is NiL.JS.Core.BaseTypes.String)
                            return "string";
                        else
                            return "object";
                    }
                default: throw new NotImplementedException();
            }
        }
    }
}