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
                this.Validate = (string code, ref int pos, bool move) =>
                {
                    if (move)
                        return Parser.Validate(code, token, ref pos);
                    else
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

        private static _Rule[][] rules;

        static Parser()
        {
            rules = new _Rule[4][];
            rules[0] = new _Rule[] // Общий
            {
                new _Rule("[", ArrayStatement.Parse),
                new _Rule("{", Json.Parse),
                new _Rule("{", CodeBlock.Parse),
                new _Rule("var ", VaribleDefineStatement.Parse),
                new _Rule("if", IfElseStatement.Parse),
                new _Rule("for", ForInStatement.Parse),
                new _Rule("for", ForStatement.Parse),
                new _Rule("while", WhileStatement.Parse),
                new _Rule("return", ReturnStatement.Parse),
                new _Rule("function", Function.Parse),
                new _Rule("switch", SwitchStatement.Parse),
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
                new _Rule("void", OperatorStatement.Parse),
                new _Rule("break", BreakStatement.Parse),
                new _Rule("continue", ContinueStatement.Parse),
                new _Rule("throw", ThrowStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
            };
            rules[1] = new _Rule[] // Для операторов
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
                new _Rule("void", OperatorStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
            };
            rules[2] = new _Rule[] // Для операторов №2
            {
                new _Rule("[", ArrayStatement.Parse),
                new _Rule("{", Json.Parse),
                new _Rule("function", Function.Parse),
            };
            rules[3] = new _Rule[] // Для for
            {
                new _Rule("var ", VaribleDefineStatement.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("+", OperatorStatement.Parse),
                new _Rule("-", OperatorStatement.Parse),
                new _Rule("!", OperatorStatement.Parse),
                new _Rule("~", OperatorStatement.Parse),
                new _Rule("function", Function.Parse),
                new _Rule("(", OperatorStatement.Parse),
                new _Rule("true", OperatorStatement.Parse),
                new _Rule("false", OperatorStatement.Parse),
                new _Rule("null", OperatorStatement.Parse),
                new _Rule("this", OperatorStatement.Parse),
                new _Rule("typeof", OperatorStatement.Parse),
                new _Rule("new", OperatorStatement.Parse),
                new _Rule("void", OperatorStatement.Parse),
                new _Rule(ValidateName, OperatorStatement.Parse),
                new _Rule(ValidateValue, OperatorStatement.Parse),
            };
        }

        internal static bool Validate(string code, string patern, int index)
        {
            int i = 0;
            int j = index;
            while (i < patern.Length)
            {
                if ((code[j] != patern[i]) && (!char.IsWhiteSpace(patern[i]) || (!char.IsWhiteSpace(code[j]))))
                    return false;
                if (i + 1 == code.Length)
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
                if (i + 1 == code.Length)
                    return false;
                i++;
                j++;
            }
            index = j;
            return true;
        }

        internal static bool ValidateName(string code, ref int index)
        {
            return ValidateName(code, ref index, true, true);
        }

        internal static bool ValidateName(string code, ref int index, bool move)
        {
            return ValidateName(code, ref index, move, true);
        }

        internal static bool ValidateName(string code, ref int index, bool move, bool reserveControl)
        {
            if ((code[index] != '$') && (code[index] != '_') && (!char.IsLetter(code[index])))
                return false;
            int j = index + 1;
            while (j < code.Length)
            {
                if ((code[j] != '$') && (code[j] != '_') && (!char.IsLetterOrDigit(code[j])))
                    break;
                j++;
            }
            string name = code.Substring(index, j - index);
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
                    case "finaly":
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
                        return false;
                }
            if (move)
                index = j;
            return true;
        }

        internal static bool ValidateNumber(string code, ref int index, bool move)
        {
            int i = index;
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            bool h = false;
            bool e = false;
            bool d = false;
            bool r = false;
            bool n = false;
            bool ch = true;
            int s = i;
            bool w = true;
            while (w)
            {
                switch (code[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            r = true;
                            n = true;
                            break;
                        }
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'f':
                        {
                            if (!h || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            e = false;
                            n = true;
                            r = true;
                            break;
                        }
                    case 'x':
                    case 'X':
                        {
                            if (h || !n || e || d || i - s != 1 || code[i - 1] != '0')
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            h = true;
                            break;
                        }
                    case '.':
                        {
                            if (h || d || e)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            d = true;
                            break;
                        }
                    case 'E':
                    case 'e':
                        {
                            if (e || !n)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            e = !h;
                            n = h;
                            break;
                        }
                    case '+':
                    case '-':
                        {
                            if (!e || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            ch = false;
                            break;
                        }
                    default:
                        {
                            i--;
                            w = false;
                            break;
                        }
                }
                w &= ++i < code.Length;
            }
            if (r && move)
                index = i;
            return r;
        }

        internal static bool ValidateRegex(string code, ref int index, bool move)
        {
            int j = index;
            if (code[j] == '/')
            {
                char fchar = code[j];
                j++;
                while ((j < code.Length) && (code[j] != '/'))
                {
                    if ((code[j] == '\n') || (code[j] == '\r'))
                        return false;
                    if (code[j] == '\\')
                        j++;
                    j++;
                }
                if (j == code.Length)
                    return false;
                try
                {
                    new System.Text.RegularExpressions.Regex(code.Substring(index + 1, j - index - 1), System.Text.RegularExpressions.RegexOptions.ECMAScript);
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
                    switch (code[j])
                    {
                        case 'g':
                            {
                                if (g)
                                    throw new ArgumentException("Invalid flag in regexp definition");
                                g = true;
                                break;
                            }
                        case 'i':
                            {
                                if (i)
                                    throw new ArgumentException("Invalid flag in regexp definition");
                                i = true;
                                break;
                            }
                        case 'm':
                            {
                                if (m)
                                    throw new ArgumentException("Invalid flag in regexp definition");
                                m = true;
                                break;
                            }
                        case '\n':
                        case '\r':
                        case ';':
                        case ')':
                        case ']':
                        case '}':
                        case ':':
                        case '.':
                        case ' ':
                            {
                                w = false;
                                break;
                            }
                        default:
                            {
                                if (isOperator(code[j]))
                                {
                                    w = false;
                                    break;
                                }
                                throw new ArgumentException("Invalid flag in regexp definition");
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
                while ((j < code.Length) && (code[j] != fchar))
                {
                    if (code[j] == '\\')
                    {
                        j++;
                        if ((code[j] == '\r') && (code[j + 1] == '\n'))
                            j++;
                        else if ((code[j] == '\n') && (code[j + 1] == '\r'))
                            j++;
                    }
                    else if (code[j] == '\r' || code[j] == '\n')
                        throw new ArgumentException("Unterminated string constant");
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

        internal static bool ValidateValue(string code, ref int index, bool move)
        {
            int j = index;
            if (code[j] == '/')
                return ValidateRegex(code, ref index, move);
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

        internal static bool ParseNumber(string code, ref int index, bool move, out double value)
        {
            value = 0;
            int i = index;
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            bool h = false;
            bool e = false;
            bool d = false;
            bool r = false;
            bool n = false;
            bool ch = true;
            int s = i;
            bool w = true;
            while (w)
            {
                switch (code[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            r = true;
                            n = true;
                            break;
                        }
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'f':
                        {
                            if (!h || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            e = false;
                            n = true;
                            r = true;
                            break;
                        }
                    case 'x':
                    case 'X':
                        {
                            if (h || !n || e || d || i - s != 1 || code[i - 1] != '0')
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            h = true;
                            break;
                        }
                    case '.':
                        {
                            if (h || d || e)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            d = true;
                            break;
                        }
                    case 'E':
                    case 'e':
                        {
                            if (e || !n)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            e = !h;
                            n = h;
                            break;
                        }
                    case '-':
                    case '+':
                        {
                            if (!e || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            ch = false;
                            break;
                        }
                    default:
                        {
                            i--;
                            w = false;
                            break;
                        }
                }
                w &= ++i < code.Length;
            }
            if (r)
            {
                if (move)
                    index = i;
                i--;
                int deg = 0;
                if (e)
                {
                    int t = i;
                    while (code[t] != 'e' && code[t] != 'E' && code[t] != '-' && code[t] != '+')
                        t--;
                    ch |= code[t] == '+';
                    while (++t <= i)
                    {
                        deg *= 10;
                        deg += code[t] - '0';
                    }
                    if (!ch)
                        deg = -deg;
                    while (code[i] != 'e' & code[i--] != 'E') ;
                }
                if (d || deg > 16 || i - s > 8 || deg < 0)
                {
                    if (h)
                    {
                        s += 2;
                        for (; s <= i; s++)
                            value = value * 16 + ((code[s] % 97 % 65 + 10) % 58);
                    }
                    else
                    {
                        for (; (s <= i) && (code[s] != '.'); s++)
                            value = value * 10 + code[s] - '0';
                        if (code[s] == '.')
                        {
                            s++;
                            for (; s <= i; s++, deg--)
                                value = value * 10 + code[s] - '0';
                        }
                    }
                    if (value == 0.0)
                        return true;
                    for (; deg > 0; deg--)
                        value *= 10;
                    for (; deg < 0; deg++)
                        value /= 10;
                    value *= sig;
                    return true;
                }
                else
                {
                    if (h)
                    {
                        s += 2;
                        for (; s <= i; s++)
                            value = value * 16 + ((code[s] % 97 % 65 + 10) % 58);
                    }
                    else
                    {
                        for (; s <= i; s++)
                            value = value * 10 + code[s] - '0';
                    }
                    if (value == 0)
                        return true;
                    for (; deg > 0; deg--)
                        value *= 10;
                    value *= sig;
                    return true;
                }
            }
            return false;
        }

        internal static bool ParseNumber(string code, ref int index, bool move, out int value)
        {
            value = 0;
            int i = index;
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            bool h = false;
            bool e = false;
            bool d = false;
            bool r = false;
            bool n = false;
            bool ch = true;
            int s = i;
            bool w = true;
            while (w)
            {
                switch (code[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            r = true;
                            n = true;
                            break;
                        }
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'f':
                        {
                            if (!h || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            e = false;
                            n = true;
                            r = true;
                            break;
                        }
                    case 'x':
                    case 'X':
                        {
                            if (h || !n || e || d || i - s != 1 || code[i - 1] != '0')
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            h = true;
                            break;
                        }
                    case '.':
                        {
                            if (h || d || e)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            d = true;
                            break;
                        }
                    case 'E':
                    case 'e':
                        {
                            if (e || !n)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            r = true;
                            e = !h;
                            n = h;
                            break;
                        }
                    case '-':
                    case '+':
                        {
                            if (!e || !ch)
                            {
                                i--;
                                w = false;
                                break;
                            }
                            ch = false;
                            break;
                        }
                    default:
                        {
                            i--;
                            w = false;
                            break;
                        }
                }
                w &= ++i < code.Length;
            }
            if (r)
            {
                i--;
                int deg = 0;
                if (e)
                {
                    int t = i;
                    while (code[t] != 'e' && code[t] != 'E' && code[t] != '-' && code[t] != '+')
                        t--;
                    ch |= code[t] == '+';
                    while (++t <= i)
                    {
                        deg *= 10;
                        deg += code[t] - '0';
                    }
                    if (!ch)
                        deg = -deg;
                    while (code[i] != 'e' & code[i--] != 'E') ;
                }
                if (d || deg > 16 || i - s > 8 || deg < 0)
                {
                    return false;
                }
                else
                {
                    if (move)
                        index = i;
                    if (h)
                    {
                        s += 2;
                        for (; s <= i; s++)
                            value = value * 16 + ((code[s] % 97 % 65 + 10) % 58);
                    }
                    else
                    {
                        for (; s <= i; s++)
                            value = value * 10 + code[s] - '0';
                    }
                    if (value == 0)
                        return true;
                    for (; deg > 0; deg--)
                        value *= 10;
                    value *= sig;
                    return true;
                }
            }
            return false;
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
                || (c == ',');
        }

        internal static Statement Parse(ParsingState state, ref int index, int ruleset, bool lineAutoComplite = false)
        {
            string code = state.Code;
            while ((index < code.Length) && (char.IsWhiteSpace(code[index])) && (!lineAutoComplite || !isLineTerminator(code[index]))) index++;
            if (code[index] == '}')
                return null;
            if (code[index] == ';' || (lineAutoComplite && isLineTerminator(code[index])))
            {
                index++;
                return new EmptyStatement();
            }
            for (int i = 0; i < rules[ruleset].Length; i++)
            {
                if (rules[ruleset][i].Validate(code, ref index, false))
                {
                    var pr = rules[ruleset][i].Parse(state, ref index);
                    if (pr.IsParsed)
                        return pr.Statement;
                }
            }
            throw new ArgumentException("Unknown token at index " + index + ": " + code.Substring(index, Math.Min(20, code.Length - index)).Split(' ')[0]);
        }

        internal static void Optimize(ref Statement s, int depth, HashSet<string> varibles)
        {
            while ((s is IOptimizable) && (s as IOptimizable).Optimize(ref s, depth, varibles)) { }
        }

        internal static void Optimize(ref Statement s, HashSet<string> varibles)
        {
            while ((s is IOptimizable) && (s as IOptimizable).Optimize(ref s, 0, varibles)) { }
        }

        internal static string Unescape(string code)
        {
            StringBuilder res = new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\')
                {
                    i++;
                    switch (code[i])
                    {
                        case 'x':
                        case 'u':
                            {
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out chc))
                                {
                                    char ch = (char)chc;
                                    res.Append(ch);
                                    i += c.Length;
                                }
                                else
                                {
                                    throw new ArgumentException("Invalid escape sequence '\\" + code[i] + c + "'");
                                    //res.Append(code[i - 1]);
                                    //res.Append(code[i]);
                                }
                                break;
                            }
                        case 't':
                            {
                                res.Append('\t');
                                break;
                            }
                        case 'f':
                            {
                                res.Append('\f');
                                break;
                            }
                        case 'v':
                            {
                                res.Append('\v');
                                break;
                            }
                        case 'b':
                            {
                                res.Append('\b');
                                break;
                            }
                        case 'n':
                            {
                                res.Append('\n');
                                break;
                            }
                        case 'r':
                            {
                                res.Append('\r');
                                break;
                            }
                        default:
                            {
                                if (char.IsDigit(code[i]))
                                    res.Append((char)(code[i] - '0'));
                                else
                                    res.Append(code[i]);
                                break;
                            }
                    }
                }
                else
                    res.Append(code[i]);
            }
            return res.ToString();
        }

        internal static bool isLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

        internal static bool isIdentificatorTerminator(char c)
        {
            return c == ' '
                || isLineTerminator(c) 
                || isOperator(c) 
                || (c == '{') 
                || (c == '\v') 
                || (c == '}') 
                || (c == '(') 
                || (c == ')') 
                || (c == ';') 
                || (c == '[') 
                || (c == ']');
        }

        internal static string RemoveComments(string code)
        {
            StringBuilder res = new StringBuilder(code.Length);
            int commentType = 0;
            int i = 0;
            for (; i < code.Length - 1; i++)
            {
                if ((commentType == 0) && (code[i] == '/') && (code[i + 1] == '/' || code[i + 1] == '*'))
                    commentType = code[i + 1] == '/' ? 1 : 2;
                if (commentType == 0)
                {
                    var t = i;
                    if (ValidateString(code, ref i, true))
                    {
                        res.Append(code.Substring(t, i - t));
                        if (i == code.Length)
                            return res.ToString();
                    }
                    else if (ValidateRegex(code, ref i, true))
                    {
                        res.Append(code.Substring(t, i - t));
                        if (i == code.Length)
                            return res.ToString();
                    }
                }
                if ((commentType == 1) && isLineTerminator(code[i]))
                    commentType = 0;
                if ((commentType == 2) && (code[i] == '*') && (code[i + 1] == '/'))
                {
                    commentType = 0;
                    i++;
                    if (i + 1 == code.Length)
                        return res.ToString();
                    continue;
                }
                if (commentType == 0)
                    res.Append(code[i]);
            }
            if (i < code.Length)
                res.Append(code[code.Length - 1]);
            return res.ToString();
        }
    }
}