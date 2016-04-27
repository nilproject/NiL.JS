using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Statements
{
    public sealed class ImportStatement : CodeNode
    {
        private readonly List<KeyValuePair<string, string>> _map = new List<KeyValuePair<string, string>>();
        private string _moduleName;

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            if (!Parser.Validate(state.Code, "import", ref index))
                return null;

            Tools.SkipSpaces(state.Code, ref index);

            var result = new ImportStatement();

            if (state.Code[index] == '{')
            {
                parseImportMap(result, state.Code, ref index);
            }
            else
            {
                ExceptionsHelper.Throw(new NotImplementedException());
            }

            Tools.SkipSpaces(state.Code, ref index);

            if (!Parser.Validate(state.Code, "from", ref index))
                ExceptionsHelper.ThrowSyntaxError("Expected 'from'", state.Code, index);

            Tools.SkipSpaces(state.Code, ref index);

            var start = index;
            if (!Parser.ValidateString(state.Code, ref index, true))
                ExceptionsHelper.ThrowSyntaxError("Expected module name", state.Code, index);

            result._moduleName = Tools.Unescape(state.Code.Substring(start + 1, index - start - 2), false);

            return result;
        }

        private static void parseImportMap(ImportStatement import, string code, ref int index)
        {
            index++;
            Tools.SkipSpaces(code, ref index);

            while (code[index] != '}')
            {
                var start = index;
                if (!Parser.ValidateName(code, ref index))
                    ExceptionsHelper.ThrowSyntaxError("Invalid import name", code, index);
                var name = code.Substring(start, index - start);
                var alias = name;

                Tools.SkipSpaces(code, ref index);

                if (Parser.Validate(code, "as", ref index))
                {
                    Tools.SkipSpaces(code, ref index);

                    start = index;
                    if (!Parser.ValidateName(code, ref index))
                        ExceptionsHelper.ThrowSyntaxError("Invalid import alias", code, index);
                    alias = code.Substring(start, index - start);

                    Tools.SkipSpaces(code, ref index);
                }

                import._map.Add(new KeyValuePair<string, string>(name, alias));

                Parser.Validate(code, ",", ref index);
            }

            index++;
        }

        public override void Decompose(ref CodeNode self)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            var module = Module.Resolve(_moduleName);
            for (var i = 0; i < _map.Count; i++)
            {
                var value = module.Exports[_map[i].Key];
                context.DefineVariable(_map[i].Value).Assign(value);
            }

            return null;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
        }

        public override string ToString()
        {
            var result = new StringBuilder("import { ");

            for (var i = 0; i < _map.Count; i++)
            {
                if (i > 0)
                    result.Append(", ");

                var item = _map[i];

                if (string.IsNullOrEmpty(item.Key))
                    result.Append("*");
                else
                    result.Append(item.Key);

                if (item.Key != item.Value)
                {
                    result
                        .Append(" as ")
                        .Append(item.Value);
                }
            }

            result
                .Append(" } from \"")
                .Append(_moduleName)
                .Append("\"");

            return result.ToString();
        }
    }
}
