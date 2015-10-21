using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NewOperator : Expression
    {
        private sealed class ThisSetter : Expression
        {
            private CodeNode source;
            public JSValue lastThisBind;

            internal override bool ResultInTempContainer
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

            public override JSValue Evaluate(Context context)
            {
                JSValue ctor = source.Evaluate(context);
                if (ctor.valueType != JSValueType.Function && !(ctor.valueType == JSValueType.Object && ctor.oValue is Function))
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.TypeError(ctor + " is not callable")));
                if (ctor.oValue is EvalFunction
                    || ctor.oValue is ExternalFunction
                    || ctor.oValue is MethodProxy)
                    ExceptionsHelper.Throw(new TypeError("Function \"" + (ctor.oValue as Function).name + "\" is not a constructor."));

                JSValue _this = new JSObject(false) { valueType = JSValueType.Object, oValue = typeof(NewOperator) };
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
                if (source is GetMemberOperator)
                    return visitor.Visit(source as GetMemberOperator);
                if (source is CommaOperator)
                    return visitor.Visit(source as CommaOperator);
                return visitor.Visit(source);
            }

            internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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

        internal override bool ResultInTempContainer
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

        public NewOperator(Expression first, Expression[] arguments)
            : base(null, null, false)
        {
            this.first = new CallOperator(thisSetter = new ThisSetter(first), arguments);
        }

        public static CodeNode Parse(ParsingState state, ref int index)
        {
            var i = index;
            if (!Parser.Validate(state.Code, "new", ref i))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var result = (Expression)ExpressionTree.Parse(state, ref i, true, false, true, true, false, false);
            if (result == null)
            {
                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
            }
            if (result is CallOperator)
                result = new NewOperator((result as Expression).FirstOperand, (result as CallOperator).Arguments) { Position = index, Length = i - index };
            else
            {
                if (state.message != null)
                    state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, index, 0), "Missed brackets in a constructor invocation.");
                result = new Expressions.NewOperator(result, new Expression[0]) { Position = index, Length = i - index };
            }
            index = i;
            return result;
        }

        public override JSValue Evaluate(NiL.JS.Core.Context context)
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

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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