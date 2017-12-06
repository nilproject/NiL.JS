using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Throw : CodeNode
    {
        private Expression _body;
        private Exception _exception;

        public Throw(Exception e)
        {
            _exception = e;
        }

        internal Throw(Expression statement)
        {
            _body = statement;
        }

        public CodeNode Body { get { return _body; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "throw", ref i) || (!Parser.IsIdentifierTerminator(state.Code[i])))
                return null;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]) && !Tools.IsLineTerminator(state.Code[i]))
                i++;
            var b = state.Code[i] == ';' || Tools.IsLineTerminator(state.Code[i]) ? null : (Expression)Parser.Parse(state, ref i, CodeFragmentType.Expression);
            if (b is Empty)
                ExceptionHelper.Throw((new SyntaxError("Can't throw result of EmptyStatement " + CodeCoordinates.FromTextPosition(state.Code, i - 1, 0))));
            var pos = index;
            index = i;
            return new Throw(b)
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var value = 
                _body == null ? 
                    _exception == null ? 
                        JSValue.undefined 
                    : 
                        context.GlobalContext.ProxyValue(_exception) 
                : _body.Evaluate(context);

            if (context._executionMode == ExecutionMode.Suspend)
                return null;

            ExceptionHelper.Throw(value);
            return null;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                _body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref _body, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override void Decompose(ref CodeNode self)
        {
            if (_body != null)
                _body.Decompose(ref _body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _body?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "throw" + (_body != null ? " " + _body : _exception != null ? "\"<native exception>\"" : "");
        }
    }
}