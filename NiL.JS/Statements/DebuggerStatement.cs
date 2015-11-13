using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DebuggerStatement : CodeNode
    {
        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "debugger", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            i ^= index;
            index ^= i;
            i ^= index;
            return new DebuggerStatement()
                {
                    Position = i,
                    Length = index - i
                };
        }

        public override JSValue Evaluate(Context context)
        {
#if DEV
            if (!context.debugging)
                // Без этого условия обработчик остановки вызывается дважды с одним выражением.
                // Первый вызов происходит из цикла CodeBlock, второй из строки ниже.
                context.raiseDebugger(this);
#else
            context.raiseDebugger(this);
#endif
            return JSValue.undefined;
        }

        public override string ToString()
        {
            return "debugger";
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (statistic != null)
                statistic.ContainsDebugger = true;
            return base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}