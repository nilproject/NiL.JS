using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class InOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public InOperator(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            bool res;
            if (tempContainer == null)
                tempContainer = new JSValue { attributes = JSValueAttributesInternal.Temporary };
            tempContainer.Assign(first.Evaluate(context));
            var temp = tempContainer;
            tempContainer = null;
            var source = second.Evaluate(context);
            if (source.valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Right-hand value of operator in is not object."));
            if (temp.valueType == JSValueType.Int)
            {
                var array = source.oValue as BaseLibrary.Array;
                if (array != null)
                {
                    res = temp.iValue >= 0 && temp.iValue < array.data.Length && (array.data[temp.iValue] ?? JSValue.notExists).IsExists;
                    tempContainer = temp;
                    return res;
                }
            }
            var t = source.GetMember(temp, false, MemberScope.Сommon);
            tempContainer = temp;
            return t.IsExists;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}