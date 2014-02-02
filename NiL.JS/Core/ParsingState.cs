using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ParsingState
    {
        public int AllowBreak;
        public int AllowContinue;
        public int AllowReturn;
        public bool InExpression;
        public List<string> Labels;
        public readonly Stack<bool> strict;
        public int LabelCount;
        public readonly string Code;
        public bool allowStrict;

        public ParsingState(string code)
        {
            Code = code;
            Labels = new List<string>();
            strict = new Stack<bool>();
            strict.Push(false);
            allowStrict = true;
        }
    }
}
