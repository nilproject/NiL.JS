using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public abstract class Expression : CodeNode
    {
        internal Expression _left;
        internal Expression _right;
        internal JSValue _tempContainer;
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

        public Expression LeftOperand { get { return _left; } }
        public Expression RightOperand { get { return _right; } }

        public override bool Eliminated
        {
            get
            {
                return base.Eliminated;
            }
            internal set
            {
                if (_left != null)
                    _left.Eliminated = true;
                if (_right != null)
                    _right.Eliminated = true;
                base.Eliminated = value;
            }
        }

        protected internal virtual bool LValueModifier { get { return false; } }

        protected internal virtual bool ContextIndependent
        {
            get
            {
                return (_left == null || _left.ContextIndependent)
                    && (_right == null || _right.ContextIndependent);
            }
        }

        protected internal virtual bool NeedDecompose
        {
            get
            {
                return (_left != null && _left.NeedDecompose)
                    || (_right != null && _right.NeedDecompose);
            }
        }

        protected Expression()
        {

        }

        protected Expression(Expression first, Expression second, bool createTempContainer)
        {
            this._left = first;
            this._right = second;
            if (createTempContainer)
                _tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;
            codeContext = codeContext | CodeContext.InExpression;

            Parser.Build(ref _left, expressionDepth + 1, variables, codeContext, message, stats, opts);
            Parser.Build(ref _right, expressionDepth + 1, variables, codeContext, message, stats, opts);
            if (this.ContextIndependent)
            {
                if (message != null && !(this is RegExpExpression))
                    message(MessageLevel.Warning, Position, Length, "Constant expression. Maybe, it's a mistake.");

                try
                {
                    var res = this.Evaluate(null);
                    if (res._valueType == JSValueType.Double
                        && !double.IsNegativeInfinity(1.0 / res._dValue)
                        && res._dValue == (double)(int)res._dValue)
                    {
                        res._iValue = (int)res._dValue;
                        res._valueType = JSValueType.Integer;
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

        internal void Optimize(ref Expression self, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            CodeNode cn = self;
            Optimize(ref cn, owner, message, opts, stats);
            self = (Expression)cn;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            baseOptimize(ref _this, owner, message, opts, stats);
        }

        internal void baseOptimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var f = _left as CodeNode;
            var s = _right as CodeNode;
            if (f != null)
            {
                f.Optimize(ref f, owner, message, opts, stats);
                _left = f as Expression;
            }
            if (s != null)
            {
                s.Optimize(ref s, owner, message, opts, stats);
                _right = s as Expression;
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

        private void expressionWillThrow(InternalCompilerMessageCallback message)
        {
            if (message != null && !(this is RegExpExpression))
                message(MessageLevel.Warning, Position, Length, "Expression will throw an exception");
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            if (_left != null && _right != null)
                return new CodeNode[]{
                    _left,
                    _right
                };
            if (_left != null)
                return new CodeNode[]{
                    _left
                };
            if (_right != null)
                return new CodeNode[]{
                    _right
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
            if (_left != null)
            {
                _left.Decompose(ref _left, result);
            }

            if (_right != null)
            {
                if (_right.NeedDecompose && !(_left is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_left, LValueModifier));
                    _left = new ExtractStoredValue(_left);
                }

                _right.Decompose(ref _right, result);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _left?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            _right?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }
    }
}