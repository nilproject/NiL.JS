using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ParsingState
    {
        public int AllowBreak;
        public int AllowContinue;
        public int AllowReturn;
        public int InExpression;
        public List<string> Labels;
        public readonly Stack<bool> strict;
        public int LabelCount;
        public readonly string Code;
        public readonly string SourceCode;
        public bool AllowStrict;

        public ParsingState(string code, string sourceCode)
        {
            Code = code;
            SourceCode = sourceCode;
            Labels = new List<string>();
            strict = new Stack<bool>();
            strict.Push(false);
            AllowStrict = true;
        }
    }
}