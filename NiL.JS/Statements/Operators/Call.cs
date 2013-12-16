using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Call : Operator
    {
        public Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var temp = first.Invoke(context);
            if (temp.ValueType != ObjectValueType.Statement)
                throw new ArgumentException(temp + " is not callable");
            var stat = (temp.oValue as IContextStatement);
            var args = second.Invoke(context);
            JSObject _this = null;
            var gfs = first as GetFieldStatement;
            if (gfs != null)
                _this = gfs.obj(context);
            else
            {
                _this = context.thisBind;
                if (_this == null)
                    _this = context.GetField("this");
            }
            if (args.oValue is IContextStatement[])
                return stat.Invoke(_this, args.oValue as IContextStatement[]);
            else
            {
                var sps = args.oValue as Statement[];
                if (sps != null)
                {
                    IContextStatement[] stmnts = sps.Length == 0 ? null as ContextStatement[] : new ContextStatement[sps.Length];
                    for (int i = 0; i < sps.Length; i++)
                        stmnts[i] = sps[i].Implement(context);
                    return stat.Invoke(_this, stmnts);
                }
            }
            throw new ArgumentException("Internal error while calling function");
        }
    }
}