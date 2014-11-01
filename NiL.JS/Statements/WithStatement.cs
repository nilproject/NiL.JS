using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class WithStatement : CodeNode
    {
        private CodeNode obj;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Scope { get { return obj; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            state.containsWith.Push(state.containsWith.Pop() || true);
            int i = index;
            if (!Parser.Validate(state.Code, "with (", ref i) && !Parser.Validate(state.Code, "with(", ref i))
                return new ParseResult();
            if (state.strict.Peek())
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("WithStatement is not allowed in strict mode.")));
            var obj = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("Invalid syntax WithStatement.")));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var body = Parser.Parse(state, ref i, 0);
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new WithStatement()
                {
                    obj = obj,
                    body = body,
                    Position = pos,
                    Length = index - pos
                }
            };
        }
#if !NET35
        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var intContext = Expression.Parameter(typeof(WithContext));
            var tempContainer = Expression.Parameter(typeof(Context));
            return Expression.Block(new[] { intContext, tempContainer }
                , Expression.Assign(intContext, Expression.Call(JITHelpers.methodof(initContext), JITHelpers.ContextParameter, obj.CompileToIL(state)))
                , Expression.TryFinally(
                    Expression.Block(
                        Expression.Assign(tempContainer, JITHelpers.ContextParameter)
                        , Expression.Assign(JITHelpers.ContextParameter, intContext)
                        , Expression.Call(intContext, typeof(WithContext).GetMethod("Activate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , body.CompileToIL(state)
                    )
                    , Expression.Block(
                        Expression.Assign(JITHelpers.ContextParameter, tempContainer)
                        , Expression.Call(intContext, typeof(WithContext).GetMethod("Deactivate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        , Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abort"), Expression.Field(intContext, "abort"))
                        , Expression.Assign(Expression.Field(JITHelpers.ContextParameter, "abortInfo"), Expression.Field(intContext, "abortInfo"))
                    )
                ));
        }
#endif
        private static WithContext initContext(Context parent, JSObject obj)
        {
            return new WithContext(obj, parent);
        }

        internal override JSObject Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(obj);
#endif
            var intcontext = new WithContext(obj.Evaluate(context), context);
#if DEV
            if (context.debugging && !(body is CodeBlock))
                context.raiseDebugger(body);
#endif
            try
            {
                intcontext.Activate();
                body.Evaluate(intcontext);
                context.abort = intcontext.abort;
                context.abortInfo = intcontext.abortInfo;
            }
            finally
            {
                intcontext.Deactivate();
            }
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                obj
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Build(ref obj, depth + 1, variables, strict);
            Parser.Build(ref body, depth, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "with (" + obj + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}
