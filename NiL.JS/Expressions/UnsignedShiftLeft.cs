using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class UnsignedShiftLeft : Expression
    {
        public UnsignedShiftLeft(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt32(first.Evaluate(context));
                tempContainer.iValue = (int)((uint)left << Tools.JSObjectToInt32(second.Evaluate(context)));
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Build(ref _this, depth, vars, strict);
            if (!res && _this == this)
            {
                try
                {
                    if ((first is Expression)
                        && (first as Expression).IsContextIndependent
                        && Tools.JSObjectToInt32((first as Expression).Evaluate(null)) == 0)
                        _this = new Constant(0);
                    else if ((second is Expression)
                            && (second as Expression).IsContextIndependent
                            && Tools.JSObjectToInt32((second as Expression).Evaluate(null)) == 0)
                        _this = new ToInt(first);
                }
                catch
                {

                }
            }
            return res;
        }

        public override string ToString()
        {
            return "(" + first + " <<< " + second + ")";
        }
    }
}