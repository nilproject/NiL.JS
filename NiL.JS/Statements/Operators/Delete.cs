using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Delete : Operator
    {
        public Delete(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.InvokeForAssing(context);
            tempResult.ValueType = JSObjectType.Bool;
            if (temp.ValueType <= JSObjectType.NotExistInObject)
                tempResult.iValue = 1;
            else if ((temp.attributes & ObjectAttributes.DontDelete) == 0)
            {
                tempResult.iValue = 1;
                temp.ValueType = JSObjectType.NotExist;
            }
            else
                tempResult.iValue = 0;
            return tempResult;
        }
    }
}