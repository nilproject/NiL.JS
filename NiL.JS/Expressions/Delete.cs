using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using System.Collections.Generic;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Delete : Expression
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

        public Delete(Expression first)
            : base(first, null, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var temp = _left.Evaluate(context);
            if (temp._valueType < JSValueType.Undefined)
                return true;

            else if ((temp._attributes & JSValueAttributesInternal.Argument) != 0)
            {
                return false;
            }
            else if ((temp._attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                if ((temp._attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    temp._valueType = JSValueType.NotExists;
                    temp._oValue = null;
                }
                return true;
            }
            else if (context._strict)
            {
                ExceptionHelper.Throw(new TypeError("Can not delete property \"" + _left + "\"."));
            }

            return false;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts))
                return true;
            if (_left is Variable)
            {
                if ((codeContext & CodeContext.Strict) != 0)
                    ExceptionHelper.Throw(new SyntaxError("Can not delete variable in strict mode"));
                (_left as Variable)._SuspendThrow = true;
            }
            var gme = _left as Property;
            if (gme != null)
            {
                _this = new DeleteProperty(gme._left, gme._right);
                return false;
            }
            var f = _left as VariableReference ?? ((_left is AssignmentOperatorCache) ? (_left as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                if (f.Descriptor.IsDefined && message != null)
                    message(MessageLevel.Warning, Position, Length, "Tring to delete defined variable." + ((codeContext & CodeContext.Strict) != 0 ? " In strict mode it cause exception." : " It is not allowed"));
                (f.Descriptor.assignments ??
                    (f.Descriptor.assignments = new System.Collections.Generic.List<Expression>())).Add(this);
            }
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "delete " + _left;
        }
    }
}