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
            public JSObject lastThisBind;

            public ThisSetter(CodeNode source)
            {
                this.source = source;
            }

            protected override CodeNode[] getChildsImpl()
            {
                throw new InvalidOperationException();
            }

            internal override JSObject Evaluate(Context context)
            {
                JSObject ctor = source.Evaluate(context);
                if (ctor.valueType != JSObjectType.Function && !(ctor.valueType == JSObjectType.Object && ctor.oValue is Function))
                    throw new JSException((new NiL.JS.BaseLibrary.TypeError(ctor + " is not callable")));
                if (ctor.oValue is EvalFunction
                    || ctor.oValue is ExternalFunction
                    || ctor.oValue is MethodProxy)
                    throw new JSException(new TypeError("Function \"" + (ctor.oValue as Function).name + "\" is not a constructor."));

                JSObject _this = new JSObject(false) { valueType = JSObjectType.Object, oValue = typeof(New) };
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

            internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
            {
                return source.Build(ref source, depth, variables, strict, message, statistic, opts);
            }
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
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

        internal override NiL.JS.Core.JSObject Evaluate(NiL.JS.Core.Context context)
        {
            var prevTB = thisSetter.lastThisBind;
            try
            {
                thisSetter.lastThisBind = null;
                var temp = first.Evaluate(context);
                if (temp.valueType >= JSObjectType.Object && temp.oValue != null)
                    return temp;
                return thisSetter.lastThisBind;
            }
            finally
            {
                thisSetter.lastThisBind = prevTB;
            }
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