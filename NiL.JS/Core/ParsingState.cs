using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ParsingState
    {
        public int AllowBreak;
        public int AllowContinue;
        public readonly List<string> Labels;
        public int LabelCount;
        public readonly string Code;

        public ParsingState(string code)
        {
            Code = code;
            Labels = new List<string>();
        }
    }
}
