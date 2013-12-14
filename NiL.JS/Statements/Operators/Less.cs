using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal class Less : Operator
    {
        public Less(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Bool;

            var lvt = temp.ValueType;
            switch (lvt)
            {
                case ObjectValueType.Bool:
                case ObjectValueType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Bool)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.iValue = left < temp.iValue ? 1 : 0;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.iValue = left < temp.dValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }
    }
}
