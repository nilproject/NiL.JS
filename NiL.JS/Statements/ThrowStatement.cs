using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class ThrowStatement : CodeNode
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
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "throw", ref i) || (!char.IsWhiteSpace(state.Code[i]) && (state.Code[i] != '(')))
                return new ParseResult();
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new JSException((new Core.BaseTypes.SyntaxError("Can't throw result of EmptyStatement " + Tools.PositionToTextcord(state.Code, i - 1))));
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
            return System.Linq.Expressions.Expression.Throw(Expression.New(typeof(JSException).GetConstructor(new[] { typeof(JSObject) }), body.CompileToIL(state)));
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Build(ref body, 2, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}