using System;

namespace NiL.JS.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomCodeFragment : Attribute
    {
        public CodeFragmentType Type { get; private set; }
        public string[] ReservedWords { get; private set; }

        public CustomCodeFragment()
            : this(CodeFragmentType.Statement)
        { }

        public CustomCodeFragment(CodeFragmentType codeFragmentType)
            : this(codeFragmentType, null)
        {

        }

        public CustomCodeFragment(CodeFragmentType codeFragmentType, params string[] reservedWords)
        {
            Type = codeFragmentType;
            ReservedWords = reservedWords ?? new string[0];
        }
    }
}
