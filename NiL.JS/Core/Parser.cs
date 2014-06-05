using NiL.JS.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiL.JS.Core
{
    internal static class Parser
    {
        private class _Rule
        {
            public ValidateDelegate Validate;
            public ParseDelegate Parse;

            public _Rule(string token, ParseDelegate parseDel)
            {
                this.Validate = (string code, ref int pos) =>
                {
                    return Parser.Validate(code, token, pos);
                };
                this.Parse = parseDel;
            }

            public _Rule(ValidateDelegate valDel, ParseDelegate parseDel)
            {
                this.Validate = valDel;
                this.Parse = parseDel;
            }
        }

        private static _Rule[][] rules = new _Rule[][]
        {
            // 0
            new _Rule[] // Общий
            {                
                new _Rule("[", OperatorStatement.Parse),
                new _Rule("{", CodeBlock.Parse),
                new _Rule("var ", VaribleDefineStatement.Parse),
                new _Rule("if", IfElseStatement.Parse),
                new _Rule("for", ForInStatement.Parse),
                new _Rule("for", ForStatement.Parse),
                new _Rule("while", WhileStatement.Parse),
                new _Rule("return", ReturnStatement.Parse),
                new _Rule("function", FunctionStatement.Parse),
                new _Rule("switch", SwitchStatement.Parse),
                new _Rule("with", WithStatement.Parse),
                new _Rule("do", DoWhileStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("+", OperatorStatement.Parse),
                new _Rule("-", OperatorStatement.Parse),
                new _Rule("!", OperatorStatement.Parse),
                new _Rule("~", OperatorStatement.Parse),
                new _Rule("true", OperatorStatement.Parse),
                new _Rule("false", OperatorStatement.Parse),
                new _Rule("null", OperatorStatement.Parse),
                new _Rule("this", OperatorStatement.Parse),
                new _Rule("typeof", OperatorStatement.Parse),
                new _Rule("try", TryCatchStatement.Parse),
                new _Rule("new", OperatorStatement.Parse),
                new _Rule("delete", OperatorStatement.Parse),
                new _Rule("void", OperatorStatement.Parse),
                new _Rule("break", BreakStatement.Parse),
                new _Rule("continue", ContinueStatement.Parse),
                new _Rule("throw", ThrowStatement.Parse),
                new _Rule(ValidateName, LabeledStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
                new _Rule("debugger", DebuggerOperator.Parse)
            },
            // 1
            new _Rule[] // Для операторов
            {
                new _Rule("[", OperatorStatement.Parse),
                new _Rule("{", OperatorStatement.Parse),
                new _Rule("function", OperatorStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("+", OperatorStatement.Parse),
                new _Rule("-", OperatorStatement.Parse),
                new _Rule("!", OperatorStatement.Parse),
                new _Rule("~", OperatorStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("true", OperatorStatement.Parse),
                new _Rule("false", OperatorStatement.Parse),
                new _Rule("null", OperatorStatement.Parse),
                new _Rule("this", OperatorStatement.Parse),
                new _Rule("typeof", OperatorStatement.Parse),
                new _Rule("new", OperatorStatement.Parse),
                new _Rule("delete", OperatorStatement.Parse),
                new _Rule("void", OperatorStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
            },
            // 2
            new _Rule[] // Для операторов №2
            {
                new _Rule("[", ArrayStatement.Parse),
                new _Rule("{", Json.Parse),
                new _Rule("function", FunctionStatement.Parse),
            },
            // 3
            new _Rule[] // Для for
            {
                new _Rule("var ", VaribleDefineStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("+", OperatorStatement.Parse),
                new _Rule("-", OperatorStatement.Parse),
                new _Rule("!", OperatorStatement.Parse),
                new _Rule("~", OperatorStatement.Parse),
                new _Rule("function", OperatorStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("true", OperatorStatement.Parse),
                new _Rule("false", OperatorStatement.Parse),
                new _Rule("null", OperatorStatement.Parse),
                new _Rule("this", OperatorStatement.Parse),
                new _Rule("typeof", OperatorStatement.Parse),
                new _Rule("new", OperatorStatement.Parse),
                new _Rule("delete", OperatorStatement.Parse),
                new _Rule("void", OperatorStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
            },
            // 4
            new _Rule[] // Общий без JSON
            {
                new _Rule("//", SinglelineComment.Parse),
                new _Rule("/*", MultilineComment.Parse),
                new _Rule("[", OperatorStatement.Parse),
                new _Rule("{", CodeBlock.Parse),
                new _Rule("var ", VaribleDefineStatement.Parse),
                new _Rule("if", IfElseStatement.Parse),
                new _Rule("for", ForInStatement.Parse),
                new _Rule("for", ForStatement.Parse),
                new _Rule("while", WhileStatement.Parse),
                new _Rule("return", ReturnStatement.Parse),
                new _Rule("function", FunctionStatement.Parse),
                new _Rule("switch", SwitchStatement.Parse),
                new _Rule("with", WithStatement.Parse),
                new _Rule("do", DoWhileStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("+", OperatorStatement.Parse),
                new _Rule("-", OperatorStatement.Parse),
                new _Rule("!", OperatorStatement.Parse),
                new _Rule("~", OperatorStatement.Parse),
                new _Rule("true", OperatorStatement.Parse),
                new _Rule("false", OperatorStatement.Parse),
                new _Rule("null", OperatorStatement.Parse),
                new _Rule("this", OperatorStatement.Parse),
                new _Rule("typeof", OperatorStatement.Parse),
                new _Rule("try", TryCatchStatement.Parse),
                new _Rule("new", OperatorStatement.Parse),
                new _Rule("delete", OperatorStatement.Parse),
                new _Rule("void", OperatorStatement.Parse),
                new _Rule("break", BreakStatement.Parse),
                new _Rule("continue", ContinueStatement.Parse),
                new _Rule("throw", ThrowStatement.Parse),
                new _Rule(ValidateName, LabeledStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
                new _Rule("debugger", DebuggerOperator.Parse)
            }
        };

        internal static bool Validate(string code, string patern, int index)
        {
            int i = 0;
            int j = index;
            while (i < patern.Length)
            {
                if (j == code.Length)
                    return false;
                if ((code[j] != patern[i]) && (!char.IsWhiteSpace(patern[i]) || (!char.IsWhiteSpace(code[j]))))
                    return false;
                i++;
                j++;
            }
            return true;
        }

        internal static bool Validate(string code, string patern, ref int index)
        {
            int i = 0;
            int j = index;
            bool needInc = false;
            while (i < patern.Length)
            {
                if (j >= code.Length)
                    return false;
                if (char.IsWhiteSpace(patern[i]) && char.IsWhiteSpace(code[j]))
                {
                    j++;
                    needInc = true;
                    continue;
                }
                if (needInc)
                {
                    i++;
                    if (i == patern.Length)
                        break;
                    needInc = false;
                }
                if (code[j] != patern[i])
                    return false;
                i++;
                j++;
            }
            index = j;
            return true;
        }

        internal static bool ValidateName(string code, ref int index)
        {
            return ValidateName(code, ref index, false, true, true, false);
        }

        internal static bool ValidateName(string code, ref int index, bool move, bool strict)
        {
            return ValidateName(code, ref index, move, true, true, strict);
        }

        internal static bool ValidateName(string code, ref int index, bool move, bool reserveControl, bool allowEscape, bool strict)
        {
            int j = index;
            if ((!allowEscape || code[j] != '\\') && (code[j] != '$') && (code[j] != '_') && (!char.IsLetter(code[j])))
                return false;
            j++;
            while (j < code.Length)
            {
                if ((!allowEscape || code[j] != '\\') && (code[j] != '$') && (code[j] != '_') && (!char.IsLetterOrDigit(code[j])))
                    break;
                j++;
            }
            if (index == j)
                return false;
            string name = code.Substring(index, j - index);
            if (allowEscape)
            {
                int i = 0;
                var nname = Tools.Unescape(name, false);
                if (nname != name)
                {
                    var res = ValidateName(nname, ref i, true, reserveControl, false, strict) && i == nname.Length;
                    if (res && move)
                        index = j;
                    return res;
                }
                else if (nname.IndexOf('\\') != -1)
                    return false;
            }
            if (reserveControl)
                switch (name)
                {
                    case "break":
                    case "case":
                    case "catch":
                    case "continue":
                    case "delete":
                    case "default":
                    case "do":
                    case "else":
                    case "finally":
                    case "for":
                    case "function":
                    case "if":
                    case "in":
                    case "instanceof":
                    case "new":
                    case "return":
                    case "switch":
                    case "this":
                    case "throw":
                    case "try":
                    case "typeof":
                    case "var":
                    case "void":
                    case "while":
                    case "with":
                    case "true":
                    case "false":
                    case "null":
                    case "export":
                    case "extends":
                    case "import":
                    case "super":
                    case "class":
                    case "const":
                    case "debugger":
                    case "enum":
                        return false;
                    case "implements":
                    case "interface":
                    case "package":
                    case "private":
                    case "protected":
                    case "public":
                    case "static":
                        {
                            if (strict)
                                return false;
                            break;
                        }
                }
            if (move)
                index = j;
            return true;
        }

        internal static bool IsReserevedWord(string code, int index, bool allowEscape, bool strict)
        {
            int j = index;
            if ((!allowEscape || code[j] != '\\') && (code[j] != '$') && (code[j] != '_') && (!char.IsLetter(code[j])))
                return false;
            j++;
            while (j < code.Length)
            {
                if ((!allowEscape || code[j] != '\\') && (code[j] != '$') && (code[j] != '_') && (!char.IsLetterOrDigit(code[j])))
                    break;
                j++;
            }
            if (index == j)
                return false;
            string name = code.Substring(index, j - index);
            if (allowEscape)
            {
                int i = 0;
                name = Tools.Unescape(name, false);
                return IsReserevedWord(name, i, false, strict) && i == name.Length;
            }
            switch (name)
            {
                case "break":
                case "case":
                case "catch":
                case "continue":
                case "delete":
                case "default":
                case "do":
                case "else":
                case "finally":
                case "for":
                case "function":
                case "if":
                case "in":
                case "instanceof":
                case "new":
                case "return":
                case "switch":
                case "this":
                case "throw":
                case "try":
                case "typeof":
                case "var":
                case "void":
                case "while":
                case "with":
                case "true":
                case "false":
                case "null":
                case "export":
                case "extends":
                case "import":
                case "super":
                case "class":
                case "const":
                case "debugger":
                case "enum":
                    return true;
                case "implements":
                case "interface":
                case "package":
                case "private":
                case "protected":
                case "public":
                case "static":
                    {
                        if (strict)
                            return true;
                        break;
                    }
            }
            return false;
        }

        internal static bool ValidateNumber(string code, ref int index, bool move)
        {
            double fictive = 0.0;
            return Tools.ParseNumber(code, ref index, move, out fictive);
        }

        internal static bool ValidateRegex(string code, ref int index, bool move, bool except)
        {
            int j = index;
            if (code[j] == '/')
            {
                j++;
                while ((j < code.Length) && (code[j] != '/'))
                {
                    if (Tools.isLineTerminator(code[j]))
                        return false;
                    if (code[j] == '\\')
                    {
                        j++;
                        if (Tools.isLineTerminator(code[j]))
                            return false;
                    }
                    j++;
                }
                if (j == code.Length)
                    return false;
                try
                {
                    new System.Text.RegularExpressions.Regex(code.Substring(index + 1, j - index - 1)
                        .Replace("\\P", "P")
                        , System.Text.RegularExpressions.RegexOptions.ECMAScript);
                }
                catch
                {
                    return false;
                }
                bool w = true;
                bool g = false, i = false, m = false;
                while (w)
                {
                    j++;
                    if (j >= code.Length)
                        break;
                    char c = code[j];
                    if (c == '\\')
                    {
                        int len = 1;
                        if (code[j + 1] == 'u')
                            len = 5;
                        else if (code[j + 1] == 'x')
                            len = 3;
                        c = Tools.Unescape(code.Substring(j, len + 1))[0];
                        j += len;
                    }
                    switch (c)
                    {
                        case 'g':
                            {
                                if (g)
                                    if (except)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                g = true;
                                break;
                            }
                        case 'i':
                            {
                                if (i)
                                    if (except)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                i = true;
                                break;
                            }
                        case 'm':
                            {
                                if (m)
                                    if (except)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                m = true;
                                break;
                            }
                        default:
                            {
                                if (isIdentificatorTerminator(c))
                                {
                                    w = false;
                                    break;
                                }
                                if (except)
                                    throw new ArgumentException("Invalid flag in regexp definition");
                                else
                                    return false;
                            }
                    }
                }
                if (move)
                    index = j;
                return true;
            }
            return false;
        }

        internal static bool ValidateString(string code, ref int index, bool move)
        {
            int j = index;
            if ((code[j] == '\'') || (code[j] == '"'))
            {
                char fchar = code[j];
                j++;
                while (code[j] != fchar)
                {
                    if (code[j] == '\\')
                    {
                        j++;
                        if ((code[j] == '\r') && (code[j + 1] == '\n'))
                            j++;
                        else if ((code[j] == '\n') && (code[j + 1] == '\r'))
                            j++;
                    }
                    else if (Tools.isLineTerminator(code[j]) || (j + 1 >= code.Length))
                        throw new JSException(TypeProxy.Proxy(new BaseTypes.SyntaxError("Unterminated string constant")));
                    j++;
                }
                if (move)
                {
                    index = ++j;
                    return true;
                }
                else
                    return true;
            }
            return false;
        }

        internal static bool ValidateValue(string code, ref int index)
        {
            return ValidateValue(code, ref index, false);
        }

        internal static bool ValidateValue(string code, ref int index, bool move)
        {
            int j = index;
            if (code[j] == '/')
                return ValidateRegex(code, ref index, move, true);
            if ((code[j] == '\'') || (code[j] == '"'))
                return ValidateString(code, ref index, move);
            if ((code.Length - j >= 4) && (code[j] == 'n' || code[j] == 't' || code[j] == 'f'))
            {
                string codeSs4 = code.Substring(j, 4);
                if ((codeSs4 == "null") || (codeSs4 == "true") || ((code.Length >= 5) && (codeSs4 == "fals") && (code[j + 4] == 'e')))
                {
                    if (move)
                        index += codeSs4 == "fals" ? 5 : 4;
                    return true;
                }
            }
            return ValidateNumber(code, ref index, move);
        }

        private static bool isOperator(char c)
        {
            return (c == '+')
                || (c == '-')
                || (c == '*')
                || (c == '/')
                || (c == '%')
                || (c == '^')
                || (c == '&')
                || (c == '!')
                || (c == '<')
                || (c == '>')
                || (c == '=')
                || (c == '?')
                || (c == ':')
                || (c == ',')
                || (c == '.');
        }

        internal static bool isIdentificatorTerminator(char c)
        {
            return c == ' '
                || Tools.isLineTerminator(c)
                || isOperator(c)
                || (c == '{')
                || (c == '\v')
                || (c == '}')
                || (c == '(')
                || (c == ')')
                || (c == ';')
                || (c == '[')
                || (c == ']')
                || (c == '\'')
                || (c == '"');
        }

        internal static Statement Parse(ParsingState state, ref int index, int ruleset)
        {
            return Parse(state, ref index, ruleset, false);
        }

        internal static Statement Parse(ParsingState state, ref int index, int ruleset, bool lineAutoComplite)
        {
            string code = state.Code;
            while ((index < code.Length) && (char.IsWhiteSpace(code[index])) && (!lineAutoComplite || !Tools.isLineTerminator(code[index]))) index++;
            if (index >= code.Length || code[index] == '}')
                return null;
            int sindex = index;
            if (code[index] == ';' || (lineAutoComplite && Tools.isLineTerminator(code[index])))
            {
                index++;
                return EmptyStatement.Instance;
            }
            if (index >= code.Length)
                return null;
            for (int i = 0; i < rules[ruleset].Length; i++)
            {
                if (rules[ruleset][i].Validate(code, ref index))
                {
                    var pr = rules[ruleset][i].Parse(state, ref index);
                    if (pr.IsParsed)
                        return pr.Statement;
                }
            }
            var cord = Tools.PositionToTextcord(code, sindex);
            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Unexpected token at " + cord + " : "
                + code.Substring(index, Math.Min(20, code.Length - index)).Split(new[] { ' ', '\n', '\r' })[0])));
        }

        internal static void Optimize(ref Statement s, int depth, Dictionary<string, Statement> varibles)
        {
            while (s != null && s.Optimize(ref s, depth, varibles)) { }
        }

        internal static void Optimize(ref Statement s, Dictionary<string, Statement> varibles)
        {
            while (s != null && s.Optimize(ref s, 0, varibles)) { }
        }
    }
}
