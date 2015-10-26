using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    public enum CodeFragmentType
    {
        Statement,
        Expression
    }

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
        private static HashSet<string> customReservedWords;
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
                new Rule("function", FunctionDefinition.ParseFunction),
                new Rule("class", ClassDefinition.Parse),
                new Rule("switch", SwitchStatement.Parse),
                new Rule("with", WithStatement.Parse),
                new Rule("do", DoWhileStatement.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseArrow),
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
            new List<Rule> // Начало выражения
            {
                new Rule("[", ExpressionTree.Parse),
                new Rule("{", ExpressionTree.Parse),
                new Rule("function", ExpressionTree.Parse),
                new Rule("class", ExpressionTree.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseArrow),
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
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", ExpressionTree.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
            },
            // 2
            new List<Rule> // Сущности внутри выражения
            {
                new Rule("[", ArrayDefinition.Parse),
                new Rule("{", ObjectNotation.Parse),
                new Rule("function", FunctionDefinition.ParseFunction),
                new Rule("class", ClassDefinition.Parse),
                new Rule("new", NewOperator.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseArrow),
                new Rule(ValidateRegex, RegExpExpression.Parse),
            }
        };

        private static bool ValidateArrow(string code, int index)
        {
            bool bracket = code[index] == '(';

            if (bracket)
                index++;
            else if (!ValidateName(code, ref index))
                return false;
            if (code.Length == index)
                return false;

            while (char.IsWhiteSpace(code[index]))
            {
                index++;
                if (code.Length == index)
                    return false;
            }

            if (bracket)
            {
                index--;
                do
                {
                    do
                    {
                        index++;
                        if (code.Length == index)
                            return false;
                    }
                    while (char.IsWhiteSpace(code[index]));

                    Validate(code, "...", ref index);

                    if (!ValidateName(code, ref index))
                        return false;

                    while (char.IsWhiteSpace(code[index]))
                    {
                        index++;
                        if (code.Length == index)
                            return false;
                    }
                }
                while (code[index] == ',');
                if (code[index] != ')')
                    return false;

                do
                {
                    index++;
                    if (code.Length == index)
                        return false;
                }
                while (char.IsWhiteSpace(code[index]));
            }

            if (!Validate(code, "=>", index))
                return false;

            return true;
        }

        [CLSCompliant(false)]
        public static bool Validate(string code, string patern, int index)
        {
            return Validate(code, patern, ref index);
        }

        public static bool Validate(string code, string patern, ref int index)
        {
            if (string.IsNullOrEmpty(patern))
                return true;
            if (string.IsNullOrEmpty(code))
                return false;
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
            return IsIdentificatorTerminator(patern[patern.Length - 1]) || code.Length <= index || IsIdentificatorTerminator(code[index]);
        }

        public static bool ValidateName(string code, int index)
        {
            return ValidateName(code, ref index, true, true, false);
        }

        [CLSCompliant(false)]
        public static bool ValidateName(string code, ref int index)
        {
            return ValidateName(code, ref index, true, true, false);
        }

        public static bool ValidateName(string code, ref int index, bool strict)
        {
            return ValidateName(code, ref index, true, true, strict);
        }

        [CLSCompliant(false)]
        public static bool ValidateName(string name, int index, bool reserveControl, bool allowEscape, bool strict)
        {
            return ValidateName(name, ref index, reserveControl, allowEscape, strict);
        }

        public static bool ValidateName(string code, ref int index, bool checkReservedWords, bool allowEscape, bool strict)
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
            string name = allowEscape || checkReservedWords ? code.Substring(index, j - index) : null;
            if (allowEscape)
            {
                int i = 0;
                var nname = Tools.Unescape(name, strict, false, false);
                if (nname != name)
                {
                    var res = ValidateName(nname, ref i, checkReservedWords, false, strict) && i == nname.Length;
                    if (res)
                        index = j;
                    return res;
                }
                else if (nname.IndexOf('\\') != -1)
                    return false;
            }
            if (checkReservedWords)
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
                    default:
                        {
                            if (customReservedWords != null && customReservedWords.Contains(name))
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

        public static bool ValidateRegex(string code, int index)
        {
            return ValidateRegex(code, ref index, false);
        }

        public static bool ValidateRegex(string code, ref int index, bool throwError)
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
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                g = true;
                                break;
                            }
                        case 'i':
                            {
                                if (i)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                i = true;
                                break;
                            }
                        case 'm':
                            {
                                if (m)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in regexp definition");
                                    else
                                        return false;
                                m = true;
                                break;
                            }
                        default:
                            {
                                if (IsIdentificatorTerminator(c))
                                {
                                    w = false;
                                    break;
                                }
                                if (throwError)
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

        [CLSCompliant(false)]
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
                if (code.Length - j >= 4)
                {
                    if (code.IndexOf("null", j, 4) != -1 || code.IndexOf("true", j, 4) != -1)
                    {
                        index += 4;
                        return true;
                    }
                    if (code.Length - j >= 5)
                    {
                        if (code.IndexOf("false", j, 5) != -1)
                        {
                            index += 5;
                            return true;
                        }
                    }
                }
            }
            return ValidateNumber(code, ref index);
        }

        public static bool IsOperator(char c)
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

        public static bool IsIdentificatorTerminator(char c)
        {
            return c == ' '
                || Tools.isLineTerminator(c)
                || IsOperator(c)
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

        internal static CodeNode Parse(ParsingState state, ref int index, CodeFragmentType ruleSet)
        {
            return Parse(state, ref index, ruleSet, true);
        }

        internal static CodeNode Parse(ParsingState state, ref int index, CodeFragmentType ruleSet, bool throwError)
        {
            while ((index < state.Code.Length) && (char.IsWhiteSpace(state.Code[index])))
                index++;
            if (index >= state.Code.Length || state.Code[index] == '}')
                return null;
            int sindex = index;
            if (state.Code[index] == ',' || state.Code[index] == ';')
            {
                //index++;
                return null;
            }
            if (index >= state.Code.Length)
                return null;
            for (int i = 0; i < rules[(int)ruleSet].Count; i++)
            {
                if (rules[(int)ruleSet][i].Validate(state.Code, index))
                {
                    var result = rules[(int)ruleSet][i].Parse(state, ref index);
                    if (result != null)
                        return result;
                }
            }
            if (throwError)
                ExceptionsHelper.ThrowUnknownToken(state.Code, sindex);
            return null;
        }

        internal static void Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            while (_this != null && _this.Build(ref _this, depth, variables, state, message, statistic, opts))
                ;
        }

        internal static void Build(ref Expressions.Expression s, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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

            var attributes = type.GetCustomAttributes(typeof(CustomCodeFragment), false);
            if (attributes.Length == 0)
                throw new ArgumentException("type must be marked with attribute \"" + typeof(CustomCodeFragment).Name + "\"");
            var attribute = attributes[0] as CustomCodeFragment;

            if (attribute.Type == CodeFragmentType.Statement)
            {
                if (!typeof(CodeNode).IsAssignableFrom(type))
                    throw new ArgumentException("type must be sub-class of " + typeof(CodeNode).Name);
            }
            else if (attribute.Type == CodeFragmentType.Expression)
            {
                if (!typeof(Expression).IsAssignableFrom(type))
                    throw new ArgumentException("type must be sub-class of " + typeof(Expression).Name);
            }
            else
                throw new ArgumentException();

            var validateMethod = type.GetMethod("Validate", new[] { typeof(string), typeof(int) });
            if (validateMethod == null || validateMethod.ReturnType != typeof(bool))
                throw new ArgumentException("type must contain static method \"Validate\" which get String and Int32 and returns Boolean");

            var parserMethod = type.GetMethod("Parse", new[] { typeof(ParsingState), typeof(int).MakeByRefType() });
            if (parserMethod == null || parserMethod.ReturnType != typeof(CodeNode))
                throw new ArgumentException("type must contain static method \"Parse\" which get " + typeof(ParsingState).Name + " and Int32 by reference and returns " + typeof(CodeNode).Name);

            var validateDelegate = validateMethod.CreateDelegate(typeof(ValidateDelegate)) as ValidateDelegate;
            var parseDelegate = parserMethod.CreateDelegate(typeof(ParseDelegate)) as ParseDelegate;

            rules[0].Insert(rules[0].Count - 4, new Rule(validateDelegate, parseDelegate));

            if (attribute.Type == CodeFragmentType.Expression)
            {
                rules[1].Insert(rules[1].Count - 2, new Rule(validateDelegate, parseDelegate));
                rules[2].Add(new Rule(validateDelegate, parseDelegate));
            }

            if (attribute.ReservedWords.Length != 0)
            {
                if (customReservedWords == null)
                    customReservedWords = new HashSet<string>();
                for (var i = 0; i < attribute.ReservedWords.Length; i++)
                {
                    if (ValidateName(attribute.ReservedWords[0], 0))
                        customReservedWords.Add(attribute.ReservedWords[0]);
                }
            }
        }
    }
}
