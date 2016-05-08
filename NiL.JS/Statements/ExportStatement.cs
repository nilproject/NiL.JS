using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    public sealed class ExportStatement : CodeNode
    {
        private string reexportSourceModuleName;
        private VariableDefinition _internalDefinition;
        private readonly List<KeyValuePair<string, Expression>> _map = new List<KeyValuePair<string, Expression>>();

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            if (!Parser.Validate(state.Code, "export", ref index))
                return null;

            Tools.SkipSpaces(state.Code, ref index);

            var result = new ExportStatement();
            var reexport = 0;

            if (Parser.Validate(state.Code, "*", ref index))
            {
                reexport = 1;
            }
            else if (Parser.Validate(state.Code, "default", ref index))
            {
                reexport = -1;
                result._map.Add(new KeyValuePair<string, Expression>("", (Expression)ExpressionTree.Parse(state, ref index)));
            }
            else if (state.Code[index] == '{')
            {
                parseExportMap(result, state, ref index);
            }
            else
            {
                reexport = -1;
                var variables = (VariableDefinition)VariableDefinition.Parse(state, ref index);
                if (variables == null)
                    ExceptionsHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, index);

                result._internalDefinition = variables;
            }

            Tools.SkipSpaces(state.Code, ref index);

            if (Parser.Validate(state.Code, "from", ref index))
            {
                if (reexport == -1)
                    ExceptionsHelper.ThrowSyntaxError("Reexport is not allowed with this syntax", state.Code, index - 4);

                Tools.SkipSpaces(state.Code, ref index);

                var start = index;
                if (!Parser.ValidateString(state.Code, ref index, false))
                    ExceptionsHelper.ThrowSyntaxError("Expected module name", state.Code, index);

                result.reexportSourceModuleName = Tools.Unescape(state.Code.Substring(start + 1, index - start - 2), false);
            }
            else if (reexport == 1)
                ExceptionsHelper.ThrowSyntaxError("Expected 'from'", state.Code, index);

            return result;
        }

        private static void parseExportMap(ExportStatement export, ParseInfo state, ref int index)
        {
            index++;
            Tools.SkipSpaces(state.Code, ref index);

            if (state.Code[index] == '}')
                ExceptionsHelper.ThrowSyntaxError("Empty export map", state.Code, index);

            while (state.Code[index] != '}')
            {
                var start = index;
                if (!Parser.ValidateName(state.Code, ref index))
                    ExceptionsHelper.ThrowSyntaxError("Invalid export name", state.Code, index);
                var name = state.Code.Substring(start, index - start);
                var alias = name;

                Tools.SkipSpaces(state.Code, ref index);

                alias = parseAlias(state.Code, ref index) ?? name;

                for (var i = 0; i < export._map.Count; i++)
                {
                    if (export._map[i].Key == name)
                        ExceptionsHelper.ThrowSyntaxError("Duplicate import", state.Code, index);
                }

                export._map.Add(
                    new KeyValuePair<string, Expression>(
                        alias,
                        new GetVariable(name, state.lexicalScopeLevel)
                        {
                            Position = start,
                            Length = name.Length
                        }));

                if (Parser.Validate(state.Code, ",", ref index))
                    Tools.SkipSpaces(state.Code, ref index);
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
                if (!Parser.ValidateName(code, ref index) && !Parser.Validate(code, "default", ref index))
                    ExceptionsHelper.ThrowSyntaxError("Invalid export alias", code, index);

                alias = code.Substring(start, index - start);

                if (alias == "default")
                    alias = "";

                Tools.SkipSpaces(code, ref index);
            }

            return alias;
        }

        public override void Decompose(ref CodeNode self)
        {
            throw new InvalidOperationException();
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._module == null)
                ExceptionsHelper.Throw(new BaseLibrary.Error("Module undefined"));

            if (reexportSourceModuleName != null)
            {
                if (string.IsNullOrEmpty(context._module.FilePath))
                    ExceptionsHelper.Throw(new BaseLibrary.Error("Module must has name"));

                var module = context._module.Import(reexportSourceModuleName);

                if (_map.Count == 0)
                {
                    foreach (var item in module.Exports)
                    {
                        context._module.Exports[item.Key] = item.Value;
                    }
                }
                else
                {
                    for (var i = 0; i < _map.Count; i++)
                    {
                        context._module.Exports[_map[i].Key] = module.Exports[_map[i].Value.ToString()];
                    }
                }
            }
            else if (_internalDefinition != null)
            {
                _internalDefinition.Evaluate(context);

                for (var i = 0; i < _internalDefinition.variables.Length; i++)
                {
                    context._module.Exports[_internalDefinition.variables[i].name] = _internalDefinition.variables[i].references[0].Evaluate(context);
                }
            }
            else
            {
                for (var i = 0; i < _map.Count; i++)
                {
                    context._module.Exports[_map[i].Key] = _map[i].Value.Evaluate(context);
                }
            }

            return null;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (reexportSourceModuleName != null)
                return false;

            if (_internalDefinition != null)
            {
                Parser.Build(ref _internalDefinition, expressionDepth, variables, codeContext, message, stats, opts | Options.SuppressUselessStatementsElimination);
            }
            else
            {
                for (var i = 0; i < _map.Count; i++)
                {
                    var v = _map[i].Value;
                    Parser.Build(ref v, expressionDepth + 1, variables, codeContext, message, stats, opts);
                    if (v != _map[i].Value)
                        _map[i] = new KeyValuePair<string, Expression>(_map[i].Key, v);
                }
            }

            return false;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            if (reexportSourceModuleName != null)
                return;

            if (_internalDefinition != null)
            {
                _internalDefinition.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
            else
            {
                for (var i = 0; i < _map.Count; i++)
                {
                    _map[i].Value.RebuildScope(functionInfo, transferedVariables, scopeBias);
                }
            }
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (reexportSourceModuleName != null)
                return;

            if (_internalDefinition != null)
            {
                CodeNode t = _internalDefinition;
                _internalDefinition.Optimize(ref t, owner, message, opts, stats);
                if (t != _internalDefinition)
                    _internalDefinition = t as VariableDefinition;
            }
            else
            {
                for (var i = 0; i < _map.Count; i++)
                {
                    var v = _map[i].Value;
                    _map[i].Value.Optimize(ref v, owner, message, opts, stats);
                    if (v != _map[i].Value)
                        _map[i] = new KeyValuePair<string, Expression>(_map[i].Key, v);
                }
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder("export ");

            if (_map.Count == 1 && _map[0].Key == "")
            {
                result.Append(" default ");
                result.Append(_map[0].Value);
            }

            var i = 0;
            if (i < _map.Count)
            {
                result.Append("{ ");

                for (;;)
                {
                    var item = _map[i];

                    result.Append(item.Key);

                    if (item.Key != item.Value.ToString())
                    {
                        result
                            .Append(" as ")
                            .Append(string.IsNullOrEmpty(item.Value.ToString()) ? "default" : item.Value.ToString());
                    }

                    i++;

                    if (i < _map.Count)
                        result.Append(", ");
                    else
                        break;
                }

                result.Append(" }");
            }
            else
            {
                if (reexportSourceModuleName != null)
                    result.Append(" * ");
                else
                    result.Append(_internalDefinition);
            }

            if (reexportSourceModuleName != null)
            {
                result
                    .Append(" from \"")
                    .Append(reexportSourceModuleName)
                    .Append("\"");
            }

            return result.ToString();
        }
    }
}
