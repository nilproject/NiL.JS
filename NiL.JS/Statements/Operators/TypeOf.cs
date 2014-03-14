using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class TypeOf : Operator
    {
        public TypeOf(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var val = first.Invoke(context);
            var vt = val.ValueType;
            switch (vt)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return "number";
                    }
                case JSObjectType.NotExist:
                case JSObjectType.NotExistInObject:
                case JSObjectType.Undefined:
                    {
                        return "undefined";
                    }
                case JSObjectType.String:
                    {
                        return "string";
                    }
                case JSObjectType.Bool:
                    {
                        return "boolean";
                    }
                case JSObjectType.Function:
                    {
                        return "function";
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        return "object";
                    }
                default: throw new NotImplementedException();
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            return false;
        }

        public override string ToString()
        {
            return "typeof " + first;
        }
    }
}