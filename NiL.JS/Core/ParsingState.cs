using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    internal sealed class ParsingState
    {
        public int AllowBreak;
        public int AllowContinue;
        public readonly HashSet<string> Labels;
        /// <summary>
        /// Назначать только в LabeledStatement! После взятия присвоить null!
        /// </summary>
        public string Label;
        public readonly string Code;

        public ParsingState(string code)
        {
            Code = code;
            Labels = new HashSet<string>();
        }
    }
}
