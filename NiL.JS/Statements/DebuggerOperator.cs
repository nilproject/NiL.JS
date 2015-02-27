using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DebuggerOperator : CodeNode
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "debugger", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            i ^= index;
            index ^= i;
            i ^= index;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DebuggerOperator()
                {
                    Position = i,
                    Length = index - i
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
#if DEV
            if (!context.debugging)
                // Без этого условия обработчик остановки вызывается дважды с одним выражением.
                // Первый вызов происходит из цикла CodeBlock, второй из строки ниже.
                context.raiseDebugger(this);
#else
            context.raiseDebugger(this);
#endif
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "debugger";
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            if (statistic != null)
                statistic.ContainsDebugger = true;
            return base.Build(ref _this, depth, variables, strict, message, statistic, opts);
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}