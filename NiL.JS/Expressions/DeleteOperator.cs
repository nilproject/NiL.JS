using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DeleteOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public DeleteOperator(Expression first)
            : base(first, null, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            if (temp.valueType < JSValueType.Undefined)
                return true;
            else if ((temp.attributes & JSValueAttributesInternal.Argument) != 0)
            {
                return false;
            }
            else if ((temp.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                if ((temp.attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    temp.valueType = JSValueType.NotExists;
                    temp.oValue = null;
                }
                return true;
            }
            else if (context.strict)
            {
                ExceptionsHelper.Throw(new TypeError("Can not delete property \"" + first + "\"."));
            }
            return false;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (base.Build(ref _this, depth, variables, codeContext, message, statistic, opts))
                return true;
            if (first is GetVariableExpression)
            {
                if ((codeContext & CodeContext.Strict) != 0)
                    ExceptionsHelper.Throw(new SyntaxError("Can not evalute delete on variable in strict mode"));
                (first as GetVariableExpression).suspendThrow = true;
            }
            var gme = first as GetMemberOperator;
            if (gme != null)
            {
                //first = new SafeMemberGetter(gme);
                _this = new DeleteMemberExpression(gme.first, gme.second);
                return false;
            }
            var f = first as VariableReference ?? ((first is AssignmentOperatorCache) ? (first as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                if (f.Descriptor.isDefined && message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Try to delete defined variable." + ((codeContext & CodeContext.Strict) != 0 ? " In strict mode it cause exception." : " This is not allowed"));
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<Expression>())).Add(this);
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