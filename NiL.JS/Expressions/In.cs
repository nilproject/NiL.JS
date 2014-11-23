using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class In : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public In(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            bool res;
            if (tempContainer == null)
                tempContainer = new JSObject { attributes = JSObjectAttributesInternal.Temporary };
            tempContainer.Assign(first.Evaluate(context));
            var temp = tempContainer;
            tempContainer = null;
            var source = second.Evaluate(context);
            if (source.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Right-hand value of operator in is not object."));
            if (temp.valueType == JSObjectType.Int)
            {
                var array = source.oValue as Core.BaseTypes.Array;
                if (array != null)
                {
                    res = temp.iValue >= 0 && temp.iValue < array.data.Length && (array.data[temp.iValue] ?? JSObject.notExists).IsExist;
                    tempContainer = temp;
                    return res;
                }
            }
            var t = source.GetMember(temp.ToString());
            tempContainer = temp;
            return t.IsExist ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}