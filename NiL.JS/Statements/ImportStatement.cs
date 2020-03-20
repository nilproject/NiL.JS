using NiL.JS.Backward;
using NiL.JS.Core;
using NiL.JS.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiL.JS.Statements
{
    public sealed class ImportStatement : CodeNode
    {
        private readonly List<KeyValuePair<string, Variable>> _map = new List<KeyValuePair<string, Variable>>();
        private string _moduleName;

        public IList<KeyValuePair<string, Variable>> ImportMap => _map.AsReadOnly();
        public string SourceModuleName => _moduleName;

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            if (!Parser.Validate(state.Code, "import", ref index))
                return null;

            Tools.SkipSpaces(state.Code, ref index);

            var result = new ImportStatement();

            var start = index;
            if (!Parser.ValidateString(state.Code, ref index, true))
            {
                var onlyDefault = false;
                if (Parser.ValidateName(state.Code, ref index))
                {
                    var defaultAlias = state.Code.Substring(start, index - start);
                    result._map.Add(new KeyValuePair<string, Variable>(
                        string.Empty,
                        new Variable(defaultAlias, state.LexicalScopeLevel)
                        {
                            Position = start,
                            Length = defaultAlias.Length
                        }));

                    onlyDefault = true;
                    Tools.SkipSpaces(state.Code, ref index);
                    if (state.Code[index] == ',')
                    {
                        onlyDefault = false;
                        index++;
                        Tools.SkipSpaces(state.Code, ref index);
                    }
                }

                if (!onlyDefault)
                {
                    if (result._map.Count == 0 && state.Code[index] == '*')
                    {
                        index++;
                        Tools.SkipSpaces(state.Code, ref index);

                        var alias = parseAlias(state.Code, ref index);
                        if (alias == null)
                            ExceptionHelper.ThrowSyntaxError("Expected identifier", state.Code, index);

                        var aliasVariable = new Variable(alias, state.LexicalScopeLevel)
                        {
                            Position = index - alias.Length - 1,
                            Length = alias.Length
                        };
                        result._map.Add(new KeyValuePair<string, Variable>("*", aliasVariable));
                    }
                    else if (state.Code[index] == '{')
                    {
                        parseImportMap(result, state.Code, ref index, state);
                    }
                    else
                    {
                        ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, index);
                    }
                }

                for (var i = 0; i < result._map.Count; i++)
                {
                    state.Variables.Add(new VariableDescriptor(
                        result._map[i].Value,
                        state.LexicalScopeLevel)
                    {
                        lexicalScope = true,
                        isReadOnly = true
                    });
                }

                Tools.SkipSpaces(state.Code, ref index);

                if (!Parser.Validate(state.Code, "from", ref index))
                    ExceptionHelper.ThrowSyntaxError("Expected 'from'", state.Code, index);

                Tools.SkipSpaces(state.Code, ref index);

                start = index;

                if (!Parser.ValidateString(state.Code, ref index, true))
                    ExceptionHelper.ThrowSyntaxError("Expected module name", state.Code, index);
            }

            result._moduleName = Tools.Unescape(state.Code.Substring(start + 1, index - start - 2), false);

            return result;
        }

        private static void parseImportMap(ImportStatement import, string code, ref int index, ParseInfo state)
        {
            index++;
            Tools.SkipSpaces(code, ref index);

            if (code[index] == '}')
                ExceptionHelper.ThrowSyntaxError("Empty import map", code, index);

            while (code[index] != '}')
            {
                var start = index;
                if (!Parser.ValidateName(code, ref index))
                    ExceptionHelper.ThrowSyntaxError("Invalid import name", code, index);
                var name = code.Substring(start, index - start);
                Tools.SkipSpaces(code, ref index);

                var alias = parseAlias(code, ref index) ?? name;

                for (var i = 0; i < import._map.Count; i++)
                {
                    if (import._map[i].Key == name)
                        ExceptionHelper.ThrowSyntaxError("Duplicate import", code, index);
                }

                import._map.Add(new KeyValuePair<string, Variable>(name, new Variable(alias, state.LexicalScopeLevel)
                {
                    Position = start,
                    Length = name.Length
                }));

                if (Parser.Validate(code, ",", ref index))
                    Tools.SkipSpaces(code, ref index);
            }

            index++;
        }

        private static string parseAlias(string code, ref int index)
        {
            string alias = null;
            if (Parser.Validate(code, "as", ref index))
            {
                Tools.SkipSpaces(code, ref index);

                var start = index;
                if (!Parser.ValidateName(code, ref index))
                    ExceptionHelper.ThrowSyntaxError("Invalid import alias", code, index);

                alias = code.Substring(start, index - start);

                Tools.SkipSpaces(code, ref index);
            }

            return alias;
        }

        public override void Decompose(ref CodeNode self)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._module == null)
                ExceptionHelper.Throw(new BaseLibrary.Error("Module undefined"));
            if (string.IsNullOrEmpty(context._module.FilePath))
                ExceptionHelper.Throw(new BaseLibrary.Error("Module must has name"));

            Module module = context._module.Import(_moduleName);
            if (module == null)
                return null;

            if (_map.Count > 0)
            {
                for (var i = 0; i < _map.Count; i++)
                {
                    JSValue value = null;

                    switch (_map[i].Key)
                    {
                        case "":
                        {
                            value = module.Exports.Default;
                            break;
                        }
                        case "*":
                        {
                            value = module.Exports.CreateExportList();
                            break;
                        }
                        default:
                        {
                            value = module.Exports[_map[i].Key];
                            break;
                        }
                    }

                    context._variables[_map[i].Value._variableName] = value;
                }
            }
            else
            {
                foreach (var variable in module.Context._variables)
                {
                    context._variables[variable.Key] = variable.Value;
                }
            }

            return null;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
        }

        public override string ToString()
        {
            var result = new StringBuilder("import ");
            var i = 0;

            if (_map[i].Key == "")
            {
                result.Append(_map[i++].Value);
                if (_map.Count > 1)
                    result.Append(", ");
            }
            else if (_map[i].Key == "*")
            {
                result.Append("* as ")
                    .Append(_map[i++].Value);
            }

            if (i < _map.Count)
            {
                result.Append("{ ");

                for (; ; )
                {
                    var item = _map[i];

                    result.Append(item.Key);

                    if (item.Key != item.Value._variableName)
                    {
                        result
                            .Append(" as ")
                            .Append(item.Value);
                    }

                    i++;

                    if (i < _map.Count)
                        result.Append(", ");
                    else
                        break;
                }

                result.Append(" }");
            }

            result.Append(" from \"")
                .Append(_moduleName)
                .Append("\"");

            return result.ToString();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            for (var i = 0; i < _map.Count; i++)
            {
                var v = _map[i].Value;
                Parser.Build(ref v, 1, variables, codeContext, message, stats, opts);
            }
            return false;
        }
    }
}
