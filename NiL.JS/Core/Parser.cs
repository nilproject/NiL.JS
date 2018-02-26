using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Statements;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core
{
    public enum CodeFragmentType
    {
        Statement,
        Expression
    }

    internal class Rule
    {
        public ValidateDelegate Validate { get; private set; }
        public ParseDelegate Parse { get; private set; }

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
                new Rule("var ", VariableDefinition.Parse),
                new Rule("let ", VariableDefinition.Parse),
                new Rule("const ", VariableDefinition.Parse),
                new Rule("if", IfElse.Parse),
                new Rule("for", ForOf.Parse),
                new Rule("for", ForIn.Parse),
                new Rule("for", For.Parse),
                new Rule("while", While.Parse),
                new Rule("return", Return.Parse),
                new Rule("await", AwaitExpression.Parse),
                new Rule("function", FunctionDefinition.ParseFunction),
                new Rule("async function", FunctionDefinition.ParseFunction(FunctionKind.AsyncFunction)),
                new Rule("class", ClassDefinition.Parse),
                new Rule("switch", Switch.Parse),
                new Rule("with", With.Parse),
                new Rule("do", DoWhile.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseFunction(FunctionKind.Arrow)),
                new Rule("(", ExpressionTree.Parse),
                new Rule("+", ExpressionTree.Parse),
                new Rule("-", ExpressionTree.Parse),
                new Rule("!", ExpressionTree.Parse),
                new Rule("~", ExpressionTree.Parse),
                new Rule("`", ExpressionTree.Parse),
                new Rule("true", ExpressionTree.Parse),
                new Rule("false", ExpressionTree.Parse),
                new Rule("null", ExpressionTree.Parse),
                new Rule("this", ExpressionTree.Parse),
                new Rule("super", ExpressionTree.Parse),
                new Rule("typeof", ExpressionTree.Parse),
                new Rule("try", TryCatch.Parse),
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", Yield.Parse),
                new Rule("break", Break.Parse),
                new Rule("continue", Continue.Parse),
                new Rule("throw", Throw.Parse),
                new Rule("import", ImportStatement.Parse),
                new Rule("export", ExportStatement.Parse),
                new Rule(ValidateName, LabeledStatement.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
                new Rule("debugger", Debugger.Parse)
            },
            // 1
            new List<Rule> // Начало выражения
            {
                new Rule("[", ExpressionTree.Parse),
                new Rule("{", ExpressionTree.Parse),
                new Rule("await", AwaitExpression.Parse),
                new Rule("function", ExpressionTree.Parse),
                new Rule("class", ExpressionTree.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseFunction(FunctionKind.Arrow)),
                new Rule("(", ExpressionTree.Parse),
                new Rule("+", ExpressionTree.Parse),
                new Rule("-", ExpressionTree.Parse),
                new Rule("!", ExpressionTree.Parse),
                new Rule("~", ExpressionTree.Parse),
                new Rule("`", ExpressionTree.Parse),
                new Rule("true", ExpressionTree.Parse),
                new Rule("false", ExpressionTree.Parse),
                new Rule("null", ExpressionTree.Parse),
                new Rule("this", ExpressionTree.Parse),
                new Rule("typeof", ExpressionTree.Parse),
                new Rule("new", ExpressionTree.Parse),
                new Rule("delete", ExpressionTree.Parse),
                new Rule("void", ExpressionTree.Parse),
                new Rule("yield", Yield.Parse),
                new Rule(ValidateName, ExpressionTree.Parse),
                new Rule(ValidateValue, ExpressionTree.Parse),
            },
            // 2
            new List<Rule> // Сущности внутри выражения
            {
                new Rule("`", TemplateString.Parse),
                new Rule("[", ArrayDefinition.Parse),
                new Rule("{", ObjectDefinition.Parse),
                new Rule("await", AwaitExpression.Parse),
                new Rule("function", FunctionDefinition.ParseFunction),
                new Rule("class", ClassDefinition.Parse),
                new Rule("new", New.Parse),
                new Rule("yield", Yield.Parse),
                new Rule(ValidateArrow, FunctionDefinition.ParseFunction(FunctionKind.Arrow)),
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

            while (Tools.IsWhiteSpace(code[index]))
            {
                index++;
                if (code.Length == index)
                    return false;
            }

            if (bracket)
            {
                if (code[index] != ')')
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
                        while (Tools.IsWhiteSpace(code[index]));

                        Validate(code, "...", ref index);

                        if (!ValidateName(code, ref index))
                            return false;

                        while (Tools.IsWhiteSpace(code[index]))
                        {
                            index++;
                            if (code.Length == index)
                                return false;
                        }
                    }
                    while (code[index] == ',');
                    if (code[index] != ')')
                        return false;
                }

                do
                {
                    index++;
                    if (code.Length == index)
                        return false;
                }
                while (Tools.IsWhiteSpace(code[index]));
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

        public static bool Validate(string code, string pattern, ref int index)
        {
            if (string.IsNullOrEmpty(pattern))
                return true;
            if (string.IsNullOrEmpty(code))
                return false;
            int i = 0;
            int j = index;
            bool needInc = false;
            while (i < pattern.Length)
            {
                if (j >= code.Length)
                    return false;
                if (Tools.IsWhiteSpace(pattern[i]))
                {
                    while (code.Length > j && Tools.IsWhiteSpace(code[j]))
                    {
                        j++;
                        needInc = true;
                    }
                    if (needInc)
                    {
                        i++;
                        if (i == pattern.Length)
                            break;
                        if (code.Length <= j)
                            return false;
                        needInc = false;
                    }
                    else if (i < pattern.Length - 1 && IsIdentifierTerminator(pattern[i + 1]))
                    {
                        i++;
                    }
                }
                if (code[j] != pattern[i])
                    return false;
                i++;
                j++;
            }

            var result = IsIdentifierTerminator(pattern[pattern.Length - 1]) || j >= code.Length || IsIdentifierTerminator(code[j]);
            if (result)
                index = j;

            return result;
        }

        public static bool ValidateName(string code) => ValidateName(code, 0);

        public static bool ValidateName(string code, int index) => ValidateName(code, ref index, true, true, false);

        [CLSCompliant(false)]
        public static bool ValidateName(string code, ref int index) => ValidateName(code, ref index, true, true, false);

        public static bool ValidateName(string code, ref int index, bool strict) => ValidateName(code, ref index, true, true, strict);

        [CLSCompliant(false)]
        public static bool ValidateName(string code, int index, bool strict) => ValidateName(code, ref index, true, true, strict);

        [CLSCompliant(false)]
        public static bool ValidateName(string name, int index, bool reserveControl, bool allowEscape, bool strict) => ValidateName(name, ref index, reserveControl, allowEscape, strict);

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

            if (allowEscape || checkReservedWords)
            {
                string name = code.Substring(index, j - index);
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
                {
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
                }
            }

            index = j;
            return true;
        }

        public static bool ValidateNumber(string code, ref int index)
        {
            double fictive = 0.0;
            return Tools.ParseNumber(code, ref index, out fictive, 0, ParseNumberOptions.AllowFloat | ParseNumberOptions.AllowAutoRadix);
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
                var escape = false;
                j++;
                while ((j < code.Length) && (escape || code[j] != '/'))
                {
                    if (Tools.IsLineTerminator(code[j]))
                        return false;

                    if (code[j] == '\\')
                    {
                        j++;
                        if (Tools.IsLineTerminator(code[j]))
                            return false;
                    }
                    else
                    {
                        if (code[j] == '[')
                        {
                            escape = true;
                        }
                        if (code[j] == ']')
                        {
                            escape = false;
                        }
                    }

                    j++;
                }

                if (j == code.Length)
                    return false;

                var w = true;
                var g = false;
                var i = false;
                var m = false;
                var u = false;
                var y = false;
                while (w)
                {
                    j++;
                    if (j >= code.Length)
                        break;
                    char c = code[j];
                    switch (c)
                    {
                        case 'g':
                            {
                                if (g)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in RegExp definition");
                                    else
                                        return false;
                                g = true;
                                break;
                            }
                        case 'i':
                            {
                                if (i)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in RegExp definition");
                                    else
                                        return false;
                                i = true;
                                break;
                            }
                        case 'm':
                            {
                                if (m)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in RegExp definition");
                                    else
                                        return false;
                                m = true;
                                break;
                            }
                        case 'u':
                            {
                                if (u)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in RegExp definition");
                                    else
                                        return false;
                                u = true;
                                break;
                            }
                        case 'y':
                            {
                                if (y)
                                    if (throwError)
                                        throw new ArgumentException("Invalid flag in RegExp definition");
                                    else
                                        return false;
                                y = true;
                                break;
                            }
                        default:
                            {
                                if (IsIdentifierTerminator(c))
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
            if (j + 1 < code.Length && ((code[j] == '\'') || (code[j] == '"') || (code[j] == '`')))
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
                    else if (Tools.IsLineTerminator(code[j]) || (j + 1 >= code.Length))
                    {
                        if (!@throw)
                            return false;

                        ExceptionHelper.Throw(new SyntaxError("Unterminated string constant"));
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
                || (c == '.')
                || (c == '|');
        }

        public static bool IsIdentifierTerminator(char c)
        {
            return c == ' '
                || Tools.IsLineTerminator(c)
                || IsOperator(c)
                || Tools.IsWhiteSpace(c)
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
                || (c == '~')
                || (c == '`');
        }

        internal static CodeNode Parse(ParseInfo state, ref int index, CodeFragmentType ruleSet)
        {
            return Parse(state, ref index, ruleSet, true);
        }

        internal static CodeNode Parse(ParseInfo state, ref int index, CodeFragmentType ruleSet, bool throwError)
        {
            while ((index < state.Code.Length) && (Tools.IsWhiteSpace(state.Code[index])))
                index++;

            if (index >= state.Code.Length || state.Code[index] == '}')
                return null;

            int sindex = index;
            if (state.Code[index] == ',' || state.Code[index] == ';')
            {
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
                ExceptionHelper.ThrowUnknownToken(state.Code, sindex);

            return null;
        }

        internal static void Build<T>(ref T self, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts) where T : CodeNode
        {
            var t = (CodeNode)self;
            while (t != null && t.Build(ref t, expressionDepth, variables, codeContext, message, stats, opts))
                self = (T)t;

            self = (T)t;
        }

        internal static void Build(ref Expression s, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            CodeNode t = s;
            Build(ref t, expressionDepth, variables, codeContext, message, stats, opts);
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
                ExceptionHelper.ThrowArgumentNull("type");

            var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(CustomCodeFragment), false).ToArray();
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

            var validateMethod = type.GetRuntimeMethod("Validate", new[] { typeof(string), typeof(int) });
            if (validateMethod == null || validateMethod.ReturnType != typeof(bool))
                throw new ArgumentException("type must contain static method \"Validate\" which get String and Int32 and returns Boolean");

            var parserMethod = type.GetRuntimeMethod("Parse", new[] { typeof(ParseInfo), typeof(int).MakeByRefType() });
            if (parserMethod == null || parserMethod.ReturnType != typeof(CodeNode))
                throw new ArgumentException("type must contain static method \"Parse\" which get " + typeof(ParseInfo).Name + " and Int32 by reference and returns " + typeof(CodeNode).Name);

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
