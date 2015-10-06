using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Delete : Expression
    {
        internal sealed class SafeMemberGetter : Expression
        {
            private GetMemberExpression proto;

            public override bool IsContextIndependent
            {
                get
                {
                    return false;
                }
            }

            protected internal override bool ResultInTempContainer
            {
                get { return false; }
            }

            internal SafeMemberGetter(GetMemberExpression gms)
            {
                proto = gms;
                Position = gms.Position;
                Length = gms.Length;
            }

            protected override CodeNode[] getChildsImpl()
            {
                return new[] { proto };
            }

            internal override JSObject Evaluate(Context context)
            {
                var source = proto.Source.Evaluate(context);
                var memberName = proto.FieldName.Evaluate(context);
                context.objectSource = source;
                var res = context.objectSource.GetMember(memberName, false, true);
                if (res.IsExists && (res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                    res = context.objectSource.GetMember(memberName, true, true);
                return res;
            }

            public override T Visit<T>(Visitor<T> visitor)
            {
                return visitor.Visit(proto);
            }

            public override string ToString()
            {
                return proto.ToString();
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Delete(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var temp = first.Evaluate(context);
                tempContainer.valueType = JSObjectType.Bool;
                if (temp.valueType < JSObjectType.Undefined)
                    tempContainer.iValue = 1;
                else if ((temp.attributes & JSObjectAttributesInternal.Argument) != 0)
                {
                    if (first is SafeMemberGetter && (temp.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                    {
                        tempContainer.iValue = 1;
                        var args = context.objectSource;
                        if (args.fields != null)
                        {
                            foreach (var a in args.fields)
                            {
                                if (a.Value == temp)
                                {
                                    args.fields.Remove(a.Key);
                                    return tempContainer;
                                }
                            }
                        }
                        var oaa = args.oValue as Arguments;
                        if (oaa != null)
                        {
                            for (var i = 0; i < oaa.length; i++)
                                if (oaa[i] == temp)
                                {
                                    oaa[i] = null;
                                    return tempContainer;
                                }
                        }
                    }
                    else
                    {
                        tempContainer.iValue = 0;
                        return tempContainer;
                    }
                }
                else if ((temp.attributes & JSObjectAttributesInternal.DoNotDelete) == 0)
                {
                    tempContainer.iValue = 1;
                    if ((temp.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    {
                        temp.valueType = JSObjectType.NotExists;
                        temp.oValue = null;
                    }
                }
                else if (context.strict)
                    throw new JSException(new TypeError("Can not delete property \"" + first + "\"."));
                else
                    tempContainer.iValue = 0;
                return tempContainer;
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (base.Build(ref _this, depth,variables, state, message, statistic, opts))
                return true;
            if (first is GetVariableExpression)
            {
                if ((state & _BuildState.Strict) != 0)
                    throw new JSException(new SyntaxError("Can not evalute delete on variable in strict mode"));
                (first as GetVariableExpression).suspendThrow = true;
            }
            var gme = first as GetMemberExpression;
            if (gme != null)
            {
                //first = new SafeMemberGetter(gme);
                _this = new DeleteMemberExpression(gme.first, gme.second);
                return false;
            }
            var f = first as VariableReference ?? ((first is OpAssignCache) ? (first as OpAssignCache).Source as VariableReference : null);
            if (f != null)
            {
                if (f.Descriptor.isDefined && message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Try to delete defined variable." + ((state & _BuildState.Strict) != 0 ? " In strict mode it cause exception." : " This is not allowed"));
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            }
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "delete " + first;
        }
    }
}