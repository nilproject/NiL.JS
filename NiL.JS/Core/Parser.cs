using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    internal class Rule
    {
        public ValidateDelegate Validate;
        public ParseDelegate Parse;

        public Rule(string token, ParseDelegate parseDel)
        {
            this.Validate = (string code, int pos) => Parser.Validate(code, token, pos);
            this.Parse = parseDel;
        }

        public Rule(ValidateDelegate valDel, ParseDelegate parseDel)
        {
            this.Validate = valDel;
            this.Parse = parseDel;
        }
    }

    public static class Parser
    {
        private static List<Rule>[] rules = new List<Rule>[]
        {
            // 0
            new List<Rule> // Общий
            {                
                new Rule("[", ExpressionTree.Parse),
                new Rule("{", CodeBlock.Parse),
                new Rule("var ", VariableDefineStatement.Parse),
                new Rule("const ", VariableDefineStatement.Parse),
                new Rule("if", IfElseStatement.Parse),
                new Rule("for", ForOfStatement.Parse),
                new Rule("for", ForInStatement.Parse),
                new Rule("for", ForStatement.Parse),
                new Rule("while", WhileStatement.Parse),
                new Rule("return", ReturnStatement.Parse),
                new Rule("function", FunctionNotation.Parse),
                new Rule("class", ClassNotation.Parse),
                new Rule("switch", SwitchStatement.Parse),
                new Rule("with", WithStatement.Parse),
                new Rule("do", DoWhileStatement.Parse),
                new Rule("(", ExpressionTree.Parse),
                new Rule("+", ExpressionTree.Parse),
                new Rule("-", ExpressionTree.Parse),
                new Rule("!", ExpressionTree.Parse),
                new Rule("~", ExpressionTree.Parse),
                new Rule("true", ExpressionTree.Parse),
                new Rule("false", ExpressionTree.Parse),
                new Rule("null", ExpressionTree.Parse),
                new Rule("this", ExpressionTree.Parse),
                new Rule("typeof", ExpressionTree.Parse),
                new Rule("try", TryCatchStatement.Parse),
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", ExpressionTree.Parse),
                new Rule("break", BreakStatement.Parse),
                new Rule("continue", ContinueStatement.Parse),
                new Rule("throw", ThrowStatement.Parse),
                new Rule(ValidateName, LabeledStatement.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
                new Rule("debugger", DebuggerStatement.Parse)
            },
            // 1
            new List<Rule> // Для операторов
            {
                new Rule("[", ExpressionTree.Parse),
                new Rule("{", ExpressionTree.Parse),
                new Rule("function", ExpressionTree.Parse),
                new Rule("class", ExpressionTree.Parse),
                new Rule("(", ExpressionTree.Parse),
                new Rule("+", ExpressionTree.Parse),
                new Rule("-", ExpressionTree.Parse),
                new Rule("!", ExpressionTree.Parse),
                new Rule("~", ExpressionTree.Parse),
                new Rule("(", ExpressionTree.Parse),
                new Rule("true", ExpressionTree.Parse),
                new Rule("false", ExpressionTree.Parse),
                new Rule("null", ExpressionTree.Parse),
                new Rule("this", ExpressionTree.Parse),
                new Rule("typeof", ExpressionTree.Parse),
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", ExpressionTree.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
            },
            // 2
            new List<Rule> // Для операторов №2
            {
                new Rule("[", ArrayNotation.Parse),
                new Rule("{", ObjectNotation.Parse),
                new Rule("function", FunctionNotation.Parse),
                new Rule("class", ClassNotation.Parse),
            },
            // 3
            new List<Rule> // Для for
            {
                new Rule("const ", VariableDefineStatement.Parse),
                new Rule("var ", VariableDefineStatement.Parse),
                new Rule("(", ExpressionTree.Parse),
                new Rule("+", ExpressionTree.Parse),
                new Rule("-", ExpressionTree.Parse),
                new Rule("!", ExpressionTree.Parse),
                new Rule("~", ExpressionTree.Parse),
                new Rule("function", ExpressionTree.Parse),
                new Rule("class", ClassNotation.Parse),
                new Rule("(", ExpressionTree.Parse),
                new Rule("true", ExpressionTree.Parse),
                new Rule("false", ExpressionTree.Parse),
                new Rule("null", ExpressionTree.Parse),
                new Rule("this", ExpressionTree.Parse),
                new Rule("typeof", ExpressionTree.Parse),
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", ExpressionTree.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
            }
        };

        public static bool Validate(string code, string patern, int index)
        {
            return Validate(code, patern, ref index);
        }

        public static bool Validate(string code, string patern, ref int index)
        {
            int i = 0;
            int j = index;
            bool needInc = false;
            while (i < patern.Length)
            {
                if (j >= code.Length)
                    return false;
                while (char.IsWhiteSpace(patern[i])
                    && code.Length > j
                    && char.IsWhiteSpace(code[j]))
                {
                    j++;
                    needInc = true;
                }
                if (needInc)
                {
                    i++;
                    if (i == patern.Length)
                        break;
                    if (code.Length <= j)
                        return false;
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

        public static bool ValidateName(string code, int index)
        {
            return ValidateName(code, ref index, true, true, false);
        }

        public static bool ValidateName(string code, ref int index)
        {
            return ValidateName(code, ref index, true, true, false);
        }

        public static bool ValidateName(string code, ref int index, bool strict)
        {
            return ValidateName(code, ref index, true, true, strict);
        }

        public static bool ValidateName(string name, int index, bool reserveControl, bool allowEscape, bool strict)
        {
            return ValidateName(name, ref index, reserveControl, allowEscape, strict);
        }

        public static bool ValidateName(string code, ref int index, bool reserveControl, bool allowEscape, bool strict)
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
            string name = allowEscape || reserveControl ? code.Substring(index, j - index) : null;
            if (allowEscape)
            {
                int i = 0;
                var nname = Tools.Unescape(name, strict, false, false);
                if (nname != name)
                {
                    var res = ValidateName(nname, ref i, reserveControl, false, strict) && i == nname.Length;
                    if (res)
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
                    case "yield":
                        return false;
                    case "implements":
                    case "interface":
                    case "package":
                    case "private":
                    case "protected":
                    case "public":
                    case "static":
                    case "let":
                        {
                            if (strict)
                                return false;
                            break;
                        }
                }
            index = j;
            return true;
        }

        public static bool ValidateNumber(string code, ref int index)
        {
            double fictive = 0.0;
            return Tools.ParseNumber(code, ref index, out fictive, 0, Tools.ParseNumberOptions.AllowFloat | Tools.ParseNumberOptions.AllowAutoRadix);
        }

        public static bool ValidateRegex(string code, ref int index, bool except)
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
                        c = Tools.Unescape(code.Substring(j, len + 1), false)[0];
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
                index = j;
                return true;
            }
            return false;
        }

        public static bool ValidateString(string code, ref int index, bool @throw)
        {
            int j = index;
            if (j + 1 < code.Length && ((code[j] == '\'') || (code[j] == '"')))
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
                    {
                        if (!@throw)
                            return false;
                        ExceptionsHelper.Throw((new SyntaxError("Unterminated string constant")));
                    }
                    j++;
                    if (j >= code.Length)
                        return false;
                }
                index = ++j;
                return true;
            }
            return false;
        }

        public static bool ValidateValue(string code, int index)
        {
            return ValidateValue(code, ref index);
        }

        public static bool ValidateValue(string code, ref int index)
        {
            int j = index;
            if (code[j] == '/')
                return ValidateRegex(code, ref index, true);
            if ((code[j] == '\'') || (code[j] == '"'))
                return ValidateString(code, ref index, false);
            if ((code.Length - j >= 4) && (code[j] == 'n' || code[j] == 't' || code[j] == 'f'))
            {
                string codeSs4 = code.Substring(j, 4);
                if ((codeSs4 == "null") || (codeSs4 == "true") || ((code.Length >= 5) && (codeSs4 == "fals") && (code[j + 4] == 'e')))
                {
                    index += codeSs4 == "fals" ? 5 : 4;
                    return true;
                }
            }
            return ValidateNumber(code, ref index);
        }

        public static bool isOperator(char c)
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

        public static bool isIdentificatorTerminator(char c)
        {
            return c == ' '
                || Tools.isLineTerminator(c)
                || isOperator(c)
                || char.IsWhiteSpace(c)
                || (c == '{')
                || (c == '\v')
                || (c == '}')
                || (c == '(')
                || (c == ')')
                || (c == ';')
                || (c == '[')
                || (c == ']')
                || (c == '\'')
                || (c == '"')
                || (c == '~');
        }

        internal static CodeNode Parse(ParsingState state, ref int index, int ruleset)
        {
            while ((index < state.Code.Length) && (char.IsWhiteSpace(state.Code[index])))
                index++;
            if (index >= state.Code.Length || state.Code[index] == '}')
                return null;
            int sindex = index;
            if (state.Code[index] == ',' || state.Code[index] == ';')
            {
                index++;
                return null;
            }
            if (index >= state.Code.Length)
                return null;
            for (int i = 0; i < rules[ruleset].Count; i++)
            {
                if (rules[ruleset][i].Validate(state.Code, index))
                {
                    var pr = rules[ruleset][i].Parse(state, ref index);
                    if (pr.isParsed)
                        return pr.node;
                }
            }
            var cord = CodeCoordinates.FromTextPosition(state.Code, sindex, 0);
            ExceptionsHelper.Throw((new SyntaxError("Unexpected token at " + cord + " : "
                + state.Code.Substring(index, System.Math.Min(20, state.Code.Length - index)).Split(new[] { ' ', '\n', '\r' })[0])));
            return null;
        }

        internal static void Build(ref CodeNode s, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            while (s != null && s.Build(ref s, depth, variables, state, message, statistic, opts))
                ;
        }

        internal static void Build(ref Expressions.Expression s, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            CodeNode t = s;
            Build(ref t, depth, variables, state, message, statistic, opts);
            if (t == null)
            {
                s = null;
                return;
            }
            s = t as Expression ?? new ExpressionWrapper(t);
        }

        public static void DefineCustomCodeFragment(Type type)
        {
            if (type == null)
                ExceptionsHelper.ThrowArgumentNull("type");
            if (!typeof(CodeNode).IsAssignableFrom(type))
                throw new ArgumentException("type must be sub-class of " + typeof(CodeNode).Name);

            var validateDelegate = type.GetMethod("Validate", new[] { typeof(string), typeof(int) });
            if (validateDelegate == null || validateDelegate.ReturnType != typeof(bool))
                throw new ArgumentException("type must contain static method \"Validate\" which get String and Int32 and returns Boolean");
            var parserDelegate = type.GetMethod("Parse", new[] { typeof(ParsingState), typeof(int).MakeByRefType() });
            if (parserDelegate == null || parserDelegate.ReturnType != typeof(ParseResult))
                throw new ArgumentException("type must contain static method \"Parse\" which get " + typeof(ParsingState).Name + " and Int32 by reference and returns " + typeof(ParseResult).Name);

            rules[0].Insert(rules[0].Count - 4, new Rule(
                validateDelegate.CreateDelegate(typeof(ValidateDelegate)) as ValidateDelegate,
                parserDelegate.CreateDelegate(typeof(ParseDelegate)) as ParseDelegate));
        }
    }
}
