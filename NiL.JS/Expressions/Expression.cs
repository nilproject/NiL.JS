using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class Expression : CodeNode
    {
        internal JSObject tempContainer;

        internal protected virtual PredictedType ResultType
        {
            get
            {
                return PredictedType.Unknown;
            }
        }
        internal protected abstract bool ResultInTempContainer
        {
            get;
        }
        internal _BuildState codeContext;

        protected internal Expression first;
        protected internal Expression second;

        public Expression FirstOperand { get { return first; } }
        public Expression SecondOperand { get { return second; } }

        public override bool Eliminated
        {
            get
            {
                return base.Eliminated;
            }
            internal set
            {
                if (first != null)
                    first.Eliminated = true;
                if (second != null)
                    second.Eliminated = true;
                base.Eliminated = value;
            }
        }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is Constant || (first is Expression && ((Expression)first).IsContextIndependent))
                    && (second == null || second is Constant || (second is Expression && ((Expression)second).IsContextIndependent));
            }
        }

        protected Expression()
        {

        }

        protected Expression(Expression first, Expression second, bool createTempContainer)
        {
            if (createTempContainer)
                tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
            this.first = first;
            this.second = second;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            Parser.Build(ref first, depth + 1, variables, state, message, statistic, opts);
            Parser.Build(ref second, depth + 1, variables, state, message, statistic, opts);
            if (this.IsContextIndependent)
            {
                if (message != null && !(this is RegExpExpression))
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Constant expression. Maybe, it's a mistake.");
                try
                {
                    var res = this.Evaluate(null);
                    if (res.valueType == JSObjectType.Double
                        && !double.IsNegativeInfinity(1.0 / res.dValue)
                        && res.dValue == (double)(int)res.dValue)
                    {
                        res.iValue = (int)res.dValue;
                        res.valueType = JSObjectType.Int;
                    }
                    _this = new Constant(res);
                    return true;
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(new Constant(e.Avatar)));
                    expressionWillThrow(message);
                    return true;
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(e));
                    expressionWillThrow(message);
                    return true;
                }
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            baseOptimize(ref _this, owner, message, opts, statistic);
        }

        protected void baseOptimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var f = first as CodeNode;
            var s = second as CodeNode;
            if (f != null)
            {
                f.Optimize(ref f, owner, message, opts, statistic);
                first = f as Expression;
            }
            if (s != null)
            {
                s.Optimize(ref s, owner, message, opts, statistic);
                second = s as Expression;
            }
            if (IsContextIndependent && !(this is Constant))
            {
                try
                {
                    _this = new Constant(Evaluate(null));
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(new Constant(e.Avatar)));
                    expressionWillThrow(message);
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(e));
                    expressionWillThrow(message);
                }
            }
        }

        private void expressionWillThrow(CompilerMessageCallback message)
        {
            if (message != null && !(this is RegExpExpression))
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Expression will throw an exception");
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (first != null && second != null)
                return new[]{
                    first,
                    second
                };
            if (first != null)
                return new[]{
                    first
                };
            if (second != null)
                return new[]{
                    second
                };
            return null;
        }
    }
}