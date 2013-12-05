using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal unsafe class Less : Operator
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
                case ObjectValueType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.bValue = left < temp.iValue;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.bValue = left < temp.dValue;
                        else if (temp.ValueType == ObjectValueType.Bool)
                            tempResult.bValue = left < (temp.bValue ? 1 : 0);
                        else throw new NotImplementedException();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.bValue = left < temp.iValue;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.bValue = left < temp.dValue;
                        else throw new NotImplementedException();
                        break;
                    }
                case ObjectValueType.Bool:
                    {
                        bool left = temp.bValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                            tempResult.bValue = *(int*)(&left) < temp.iValue;
                        else if (temp.ValueType == ObjectValueType.Double)
                            tempResult.bValue = *(int*)(&left) < temp.dValue;
                        else throw new NotImplementedException();
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }
    }
}
