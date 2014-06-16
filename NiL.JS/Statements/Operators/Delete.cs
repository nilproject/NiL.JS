using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Delete : Operator
    {
        public sealed class SafeMemberGetter : Statement
        {
            private GetMemberStatement proto;

            internal SafeMemberGetter(GetMemberStatement gms)
            {
                proto = gms;
                Position = gms.Position;
                Length = gms.Length;
            }

            protected override Statement[] getChildsImpl()
            {
                return new[] { proto };
            }

            internal override JSObject Invoke(Context context)
            {
                return proto.InvokeForAssing(context);
            }

            public override string ToString()
            {
                return proto.ToString();
            }
        }

        public Delete(Statement first)
            : base(first, null, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var temp = first.Invoke(context);
                tempResult.valueType = JSObjectType.Bool;
                if (temp.valueType <= JSObjectType.NotExistInObject
                    || (temp.attributes & JSObjectAttributes.SystemConstant) != 0)
                    tempResult.iValue = 1;
                else if ((temp.attributes & JSObjectAttributes.Argument) != 0)
                {
                    if (first is SafeMemberGetter)
                    {
                        tempResult.iValue = 1;
                        var args = context.objectSource;
                        foreach (var a in args.fields)
                        {
                            if (a.Value == temp)
                            {
                                args.fields.Remove(a.Key);
                                return tempResult;
                            }
                        }
                    }
                    else
                    {
                        tempResult.iValue = 0;
                        return tempResult;
                    }
                }
                else if ((temp.attributes & JSObjectAttributes.DoNotDelete) == 0)
                {
                    tempResult.iValue = 1;
                    temp.valueType = JSObjectType.NotExist;
                    temp.oValue = null;
                }
                else if (context.strict)
                    throw new JSException(new TypeError("Can not delete property \"" + first + "\"."));
                else
                    tempResult.iValue = 0;
                return tempResult;
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            if (base.Optimize(ref _this, depth, vars, strict))
                return true;
            if (first is GetVariableStatement)
            {
                if (strict)
                    throw new JSException(new SyntaxError("Can not evalute delete on variable in strict mode"));
                first = new SafeVariableGetter(first as GetVariableStatement);
            }
            if (first is GetMemberStatement)
                first = new SafeMemberGetter(first as GetMemberStatement);
            return false;
        }

        public override string ToString()
        {
            return "delete " + first;
        }
    }
}