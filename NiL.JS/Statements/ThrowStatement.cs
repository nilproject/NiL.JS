using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ThrowStatement : CodeNode
    {
        private CodeNode body;

        public ThrowStatement(Exception e)
        {
            body = new Constant(TypeProxy.Proxy(e));
        }

        private ThrowStatement(CodeNode statement)
        {
            body = statement;
        }

        public CodeNode Body { get { return body; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "throw", ref i) || (!char.IsWhiteSpace(state.Code[i]) && (state.Code[i] != '(')))
                return new ParseResult();
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new JSException((new Core.BaseTypes.SyntaxError("Can't throw result of EmptyStatement " + CodeCoordinates.FromTextPosition(state.Code, i - 1, 0))));
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ThrowStatement(b)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }
#if !NET35
        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.New(typeof(JSException).GetConstructor(new[] { typeof(JSObject) }), body.CompileToIL(state)));
        }
#endif
        internal override JSObject Evaluate(Context context)
        {
#if DEBUG // Экономим на переменных в релизе
            var message = body.Evaluate(context);
            throw new JSException(message);
#else
            throw new JSException(body.Evaluate(context));
#endif
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref body, 2, variables, strict, message);
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            body.Optimize(ref body, owner, message);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}