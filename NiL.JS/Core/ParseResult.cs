using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    internal struct ParseResult
    {
        public bool IsParsed;
        public string Message;
        public Statement Statement;
    }
}
