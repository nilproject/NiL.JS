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
        internal Expression first;
        internal Expression second;
        internal JSValue tempContainer;
        internal CodeContext _codeContext;

        internal protected virtual PredictedType ResultType
        {
            get
            {
                return PredictedType.Unknown;
            }
        }

        internal virtual bool ResultInTempContainer
        {
            get { return false; }
        }

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

        protected internal virtual bool LValueModifier { get { return false; } }

        protected internal virtual bool ContextIndependent
        {
            get
            {
                return (first == null || first.ContextIndependent)
                    && (second == null || second.ContextIndependent);
            }
        }

        protected internal virtual bool NeedDecompose
        {
            get
            {
                return (first != null && first.NeedDecompose)
                    || (second != null && second.NeedDecompose);
            }
        }

        protected Expression()
        {

        }

        protected Expression(Expression first, Expression second, bool createTempContainer)
        {
            this.first = first;
            this.second = second;
            if (createTempContainer)
                tempContainer = new JSValue() { attributes = JSValueAttributesInternal.Temporary };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;
            codeContext = codeContext | CodeContext.InExpression;

            Parser.Build(ref first, expressionDepth + 1, variables, codeContext, message, stats, opts);
            Parser.Build(ref second, expressionDepth + 1, variables, codeContext, message, stats, opts);
            if (this.ContextIndependent)
            {
                if (message != null && !(this is RegExpExpression))
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Constant expression. Maybe, it's a mistake.");
                try
                {
                    var res = this.Evaluate(null);
                    if (res.valueType == JSValueType.Double
                        && !double.IsNegativeInfinity(1.0 / res.dValue)
                        && res.dValue == (double)(int)res.dValue)
                    {
                        res.iValue = (int)res.dValue;
                        res.valueType = JSValueType.Integer;
                    }
                    _this = new Constant(res) as CodeNode;
                    return true;
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new Throw(new Constant(e.Error)));
                    expressionWillThrow(message);
                    return true;
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new Throw(e));
                    expressionWillThrow(message);
                    return true;
                }
            }
            return false;
        }

        internal void Optimize(ref Expression self, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            CodeNode cn = self;
            Optimize(ref cn, owner, message, opts, stats);
            self = (Expression)cn;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            baseOptimize(ref _this, owner, message, opts, stats);
        }

        internal void baseOptimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var f = first as CodeNode;
            var s = second as CodeNode;
            if (f != null)
            {
                f.Optimize(ref f, owner, message, opts, stats);
                first = f as Expression;
            }
            if (s != null)
            {
                s.Optimize(ref s, owner, message, opts, stats);
                second = s as Expression;
            }
            if (ContextIndependent && !(this is Constant))
            {
                try
                {
                    _this = new Constant(Evaluate(null));
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new Throw(new Constant(e.Error)));
                    expressionWillThrow(message);
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new Throw(e));
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

        protected internal override CodeNode[] getChildsImpl()
        {
            if (first != null && second != null)
                return new CodeNode[]{
                    first,
                    second
                };
            if (first != null)
                return new CodeNode[]{
                    first
                };
            if (second != null)
                return new CodeNode[]{
                    second
                };
            return null;
        }

        protected internal void Decompose(ref Expression self)
        {
            CodeNode cn = self;
            cn.Decompose(ref cn);
            self = (Expression)cn;
        }

        public sealed override void Decompose(ref CodeNode self)
        {
            if (NeedDecompose)
            {
                var result = new List<CodeNode>();
                var s = this;
                s.Decompose(ref s, result);
                if (result.Count > 0)
                {
                    self = new SuspendableExpression(this, result.ToArray());
                }
            }
        }

        public virtual void Decompose(ref Expression self, IList<CodeNode> result)
        {
            if (first != null)
            {
                first.Decompose(ref first, result);
            }

            if (second != null)
            {
                if (second.NeedDecompose && !(first is ExtractStoredValue))
                {
                    result.Add(new StoreValue(first, LValueModifier));
                    first = new ExtractStoredValue(first);
                }

                second.Decompose(ref second, result);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            first?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            second?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }
    }
}