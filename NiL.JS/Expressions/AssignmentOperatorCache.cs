using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class AssignmentOperatorCache : Expression
    {
        private JSValue secondResult;

        public CodeNode Source { get { return _left; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return _left.ResultType;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal AssignmentOperatorCache(Expression source)
            : base(source, null, false)
        {

        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            var res = _left.EvaluateForWrite(context);
            secondResult = Tools.InvokeGetter(res, context._objectSource);
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            var res = secondResult;
            secondResult = null;
            return res;
        }

        public override string ToString()
        {
            return _left.ToString();
        }

        public override int Length
        {
            get
            {
                return _left.Length;
            }
            internal set
            {
                _left.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return _left.Position;
            }
            internal set
            {
                _left.Position = value;
            }
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return _left.Childs;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            // second будем использовать как флаг isVisited
            if (_right != null)
                return false;

            _right = _left;

            _codeContext = codeContext;

            var res = _left.Build(ref _this, expressionDepth,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            if (!res && _left is Variable)
                (_left as Variable)._ForceThrow = true;
            return res;
        }
    }
}
