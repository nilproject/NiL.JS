using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JSTest.SyntaxExtensions
{
    [CustomCodeFragment]
    public sealed class UsingStatement : CodeNode
    {
        private string namespaceName;
        private string aliasName;

        public UsingStatement(string namespaceName, string aliasName)
        {
            this.namespaceName = namespaceName;
            this.aliasName = aliasName;
        }

        public static bool Validate(string code, int position)
        {
            return Parser.Validate(code, "using", position);
        }

        public static CodeNode Parse(ParsingState state, ref int position)
        {
            if (!Parser.Validate(state.Code, "using", ref position))
                return null;

            while (char.IsWhiteSpace(state.Code, position))
                position++;

            int start = position;
            while (Parser.ValidateName(state.Code, ref position) && state.Code[position] == '.')
                position++;

            if (state.Code[position] != ' ')
            {
                throw new JSException(new SyntaxError("Unexpected char at " + CodeCoordinates.FromTextPosition(state.Code, position, 0)));
            }

            var namespaceName = state.Code.Substring(start, position - start);

            while (char.IsWhiteSpace(state.Code, position))
                position++;

            if (!Parser.Validate(state.Code, "as", ref position))
            {
                throw new JSException(new SyntaxError("Expected \"as\" at " + CodeCoordinates.FromTextPosition(state.Code, position, 2)));
            }

            while (char.IsWhiteSpace(state.Code, position))
                position++;

            start = position;
            if (!Parser.ValidateName(state.Code, ref position))
            {
                throw new JSException(new SyntaxError("Expected identifier name at " + CodeCoordinates.FromTextPosition(state.Code, position, 0)));
            }

            var aliasName = state.Code.Substring(start, position - start);

            while (char.IsWhiteSpace(state.Code, position))
                position++;

            if (state.Code[position] != ';')
            {
                throw new JSException(new SyntaxError("Expected \";\" at " + CodeCoordinates.FromTextPosition(state.Code, position, 1)));
            }

            return new UsingStatement(namespaceName, aliasName);
        }

        public override JSValue Evaluate(Context context)
        {
            context.DefineVariable(aliasName).Assign(new NamespaceProvider(namespaceName));
            return null;
        }
    }
}
