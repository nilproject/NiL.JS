using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    public static class Tools
    {
        public static double JSObjectToDouble(JSObject arg)
        {
            if (arg == null)
                return double.NaN;
            var r = arg;
            double x = double.NaN;
            if (r.ValueType == ObjectValueType.Int || r.ValueType == ObjectValueType.Bool)
                x = r.iValue;
            else if (r.ValueType == ObjectValueType.Double)
                x = r.dValue;
            else if ((r.ValueType == ObjectValueType.Statement) || (r.ValueType == ObjectValueType.Undefined))
                return double.NaN;
            else if ((r.ValueType == ObjectValueType.String))
            {
                int ix = 0;
                string s = r.oValue as string;
                Parser.ParseNumber(s, ref ix, false, out x);
            }
            return x;
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
        
        internal static void skipComment(string code, ref int index, bool skipSpaces)
        {
            bool work;
            do
            {
                if (code.Length <= index)
                    return;
                work = false;
                if (code[index] == '/')
                {
                    switch (code[index + 1])
                    {
                        case '/':
                            {
                                index += 2;
                                while (index < code.Length && !Tools.isLineTerminator(code[index])) index++;
                                while (index < code.Length && char.IsWhiteSpace(code[index])) index++;
                                work = true;
                                break;
                            }
                        case '*':
                            {
                                index += 2;
                                while (code[index] != '*' || code[index + 1] != '/')
                                    index++;
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }
            } while (work);
            if (skipSpaces)
                while ((index < code.Length) && (char.IsWhiteSpace(code[index]))) index++;
        }

        internal static string RemoveComments(string code)
        {
            StringBuilder res = new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; )
            {
                while (i < code.Length && char.IsWhiteSpace(code[i])) res.Append(code[i++]);
                var s = i;
                skipComment(code, ref i, false);
                for (; s < i; s++)
                    res.Append(' ');
                if (i >= code.Length)
                    continue;
                if (Parser.ValidateName(code, ref i, true)
                    || Parser.ValidateNumber(code, ref i, true)
                    || Parser.ValidateRegex(code, ref i, true, false)
                    || Parser.ValidateString(code, ref i, true))
                {
                    for (; s < i; s++)
                        res.Append(code[s]);
                }
                else
                    res.Append(code[i++]);
            }
            return res.ToString();
        }
    }
}
