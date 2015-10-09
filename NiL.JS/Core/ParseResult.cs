
namespace NiL.JS.Core
{
    public struct ParseResult
    {
        internal bool isParsed;
        internal CodeNode node;

        public bool IsParsed { get { return isParsed; } }
        public CodeNode Node { get { return node; } }

        public ParseResult(bool parsed, CodeNode codeNode)
        {
            node = codeNode;
            isParsed = parsed;
        }
    }
}
