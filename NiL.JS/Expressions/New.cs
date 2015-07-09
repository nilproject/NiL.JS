using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class New : Expression
    {
        private sealed class ThisSetter : Expression
        {
            private CodeNode source;
            public JSValue lastThisBind;

            protected internal override bool ResultInTempContainer
            {
                get { return false; }
            }

            public ThisSetter(CodeNode source)
            {
                this.source = source;
            }

            public override bool IsContextIndependent
            {
                get
                {
                    return false;
                }
            }

            protected override CodeNode[] getChildsImpl()
            {
                throw new InvalidOperationException();
            }

            internal override JSValue Evaluate(Context context)
            {
                JSValue ctor = source.Evaluate(context);
                if (ctor.valueType != JSValueType.Function && !(ctor.valueType == JSValueType.Object && ctor.oValue is Function))
                    throw new JSException((new NiL.JS.BaseLibrary.TypeError(ctor + " is not callable")));
                if (ctor.oValue is EvalFunction
                    || ctor.oValue is ExternalFunction
                    || ctor.oValue is MethodProxy)
                    throw new JSException(new TypeError("Function \"" + (ctor.oValue as Function).name + "\" is not a constructor."));

                JSValue _this = new JSObject(false) { valueType = JSValueType.Object, oValue = typeof(New) };
                context.objectSource = _this;
                lastThisBind = _this;
                return ctor;
            }

            public override string ToString()
            {
                return source.ToString();
            }

            public override T Visit<T>(Visitor<T> visitor)
            {
                if (source is GetVariableExpression)
                    return visitor.Visit(source as GetVariableExpression);
                if (source is GetMemberExpression)
                    return visitor.Visit(source as GetMemberExpression);
                if (source is None)
                    return visitor.Visit(source as None);
                return visitor.Visit(source);
            }

            internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
            {
                return source.Build(ref source, depth, variables, state, message, statistic, opts);
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
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        private ThisSetter thisSetter;

        public New(Expression first, Expression[] arguments)
            : base(null, null, false)
        {
            //if (first is Call)
            //    this.first = new Call(thisSetter = new ThisSetter((first as Call).FirstOperand), (first as Call).Arguments);
            //else
            this.first = new Call(thisSetter = new ThisSetter(first), arguments);
        }

        internal override NiL.JS.Core.JSValue Evaluate(NiL.JS.Core.Context context)
        {
            var prevTB = thisSetter.lastThisBind;
            try
            {
                thisSetter.lastThisBind = null;
                var temp = first.Evaluate(context);
                if (temp.valueType >= JSValueType.Object && temp.oValue != null)
                    return temp;
                return thisSetter.lastThisBind;
            }
            finally
            {
                thisSetter.lastThisBind = prevTB;
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (message != null && depth <= 1)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, 0), "Do not use NewOperator for side effect");
            return base.Build(ref _this, depth, variables, state, message, statistic, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "new " + first.ToString();
        }
    }
}