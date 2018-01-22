using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Debugger : CodeNode
    {
        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "debugger", ref i))
                return null;
            i ^= index;
            index ^= i;
            i ^= index;
            return new Debugger()
            {
                Position = i,
                Length = index - i
            };
        }

        public override JSValue Evaluate(Context context)
        {
            if (!context._debugging)
                // Без этого условия обработчик остановки вызывается дважды с одним выражением.
                // Первый вызов происходит из цикла CodeBlock, второй из строки ниже.
                context.raiseDebugger(this);
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.ContainsDebugger = true;
            return base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return null;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {

        }

        public override void Decompose(ref CodeNode self)
        {

        }
    }
}