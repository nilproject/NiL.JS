using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
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

        protected internal Expression first;
        protected internal Expression second;

        public Expression FirstOperand { get { return first; } }
        public Expression SecondOperand { get { return second; } }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is Constant || (first is Expression && ((Expression)first).IsContextIndependent))
                    && (second == null || second is Constant || (second is Expression && ((Expression)second).IsContextIndependent));
            }
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       JITHelpers.methodof(Evaluate),
                       JITHelpers.ContextParameter
                       );
        }

#endif

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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref first, depth + 1, vars, strict, message);
            Parser.Build(ref second, depth + 1, vars, strict, message);
            try
            {
                if (this.IsContextIndependent)
                {
                    if (message != null && !(this is RegExpExpression))
                        message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Constant expression. Maybe, it's a mistake.");
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
            }
            catch
            { }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            baseOptimize(owner, message);
        }

        protected void baseOptimize(FunctionExpression owner, CompilerMessageCallback message)
        {
            var f = first as CodeNode;
            var s = second as CodeNode;
            if (f != null)
            {
                f.Optimize(ref f, owner, message);
                first = f as Expression;
            }
            if (s != null)
            {
                s.Optimize(ref s, owner, message);
                second = s as Expression;
            }
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