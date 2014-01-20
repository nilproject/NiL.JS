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
            switch (r.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        return r.dValue;
                    }
                case JSObjectType.String:
                    {
                        double x = double.NaN;
                        int ix = 0;
                        string s = (r.oValue as string).Trim();
                        if (Tools.ParseNumber(s, ref ix, true, out x) && ix < s.Length)
                            return double.NaN;
                        return x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return 0;
                        r = r.ToPrimitiveValue_Value_String(Context.globalContext);
                        return JSObjectToDouble(r);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return double.NaN;
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException();
            }
        }

        public static int JSObjectToInt(JSObject arg)
        {
            if (arg == null)
                return 0;
            var r = arg;
            switch (r.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(r.dValue) || double.IsInfinity(r.dValue))
                            return 0;
                        return (int)((long)r.dValue & 0xFFFFFFFF);
                    }
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r.oValue as string).Trim();
                        if (!Tools.ParseNumber(s, ref ix, true, out x) || ix < s.Length)
                            return 0;
                        return (int)x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return 0;
                        r = r.ToPrimitiveValue_Value_String(Context.globalContext);
                        return JSObjectToInt(r);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool ParseNumber(string code, ref int index, bool move, out double value)
        {
            value = double.NaN;
            if (code.Length == 0)
            {
                value = 0.0;
                return true;
            }
            int i = index;
            while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            if (i == code.Length)
            {
                value = 0.0;
                return true;
            }
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            const string infinity = "Infinity";
            for (int j = i; (j - i) < infinity.Length; j++)
            {
                if (code[j] != infinity[j - i])
                    break;
                else if (code[j] == 'y')
                {
                    if (move)
                        index = j + 1;
                    value = sig * double.PositiveInfinity;
                    return true;
                }
            }
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
                if (i >= code.Length)
                {
                    w = false;
                    break;
                }
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
                            r = false;
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
                value = 0.0;
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
                        long temp = 0;
                        int l = 0;
                        for (; (s <= i) && (code[s] != '.'); s++)
                            if (l <= 18)
                            {
                                temp = temp * 10 + (code[s] - '0');
                                l++;
                            }
                            else
                                deg++;
                        if (d)
                        {
                            s++;
                            for (; s <= i; s++, deg--)
                                if (l <= 18)
                                {
                                    temp = temp * 10 + (code[s] - '0');
                                    l++;
                                }
                                else
                                    deg++;
                        }
                        if (deg < 0)
                        {
                            if (deg < -16)
                            {
                                value = (double)((decimal)temp * 0.0000000000000001M);
                                deg += 16;
                            }
                            else
                            {
                                value = (double)((decimal)temp * (decimal)Math.Pow(10.0, deg));
                                deg = 0;
                            }
                        }
                        else if (deg != 0)
                        {
                            if (deg > 10)
                            {
                                value = (double)((decimal)temp * 10000000000M);
                                deg -= 10;
                            }
                            else
                            {
                                value = (double)((decimal)temp * (decimal)Math.Pow(10.0, deg));
                                deg = 0;
                            }
                        }
                        else value = temp;
                    }
                    value *= sig;
                    if (value == 0.0)
                        return true;
                    if (deg != 0)
                    {
                        var exp = Math.Pow(10.0, deg);
                        value *= exp;
                    }
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
                    value *= sig;
                    if (value == 0)
                        return true;
                    for (; deg > 0; deg--)
                        value *= 10;
                    return true;
                }
            }
            return false;
        }

        public static string Unescape(string code)
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

        internal static string DoubleToString(double d)
        {
            if (0.0 == d || double.IsInfinity(d) || double.IsNaN(d))
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Math.Abs(d) < 1.0)
            {
                if (d == (d % 0.000001))
                    return d.ToString("0.####e-0", System.Globalization.CultureInfo.InvariantCulture);
                if (d == (d % 0.001))
                {
                    var t = Math.Sign(d);
                    d = Math.Abs(d);
                    if (t < 0)
                        return d.ToString("-0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (Math.Abs(d) >= 1e+21)
                return d.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            return d.ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
