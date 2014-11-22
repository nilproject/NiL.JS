using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Представляет реализация встроенного объекта JSON. Позволяет производить сериализацию и десериализацию объектов JavaScript.
    /// </summary>
    public static class JSON
    {
        private enum ParseState
        {
            Value,
            Name,
            Object,
            Array,
            End
        }

        private class StackFrame
        {
            public JSObject container;
            public JSObject value;
            public JSObject fieldName;
            public int valuesCount;
            public ParseState state;
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public static JSObject parse(Arguments args)
        {
            var length = Tools.JSObjectToInt32(args.length);
            var code = args[0].ToString();
            Function reviewer = length > 1 ? args[1].oValue as Function : null;
            return parse(code, reviewer);
        }

        [Hidden]
        public static JSObject parse(string code)
        {
            return parse(code, null);
        }

        private static bool isSpace(char c)
        {
            return c != '\u000b'
                && c != '\u000c'
                && c != '\u00a0'
                && c != '\u1680'
                && c != '\u180e'
                && c != '\u2000'
                && c != '\u2001'
                && c != '\u2002'
                && c != '\u2003'
                && c != '\u2004'
                && c != '\u2005'
                && c != '\u2006'
                && c != '\u2007'
                && c != '\u2008'
                && c != '\u2009'
                && c != '\u200a'
                && c != '\u2028'
                && c != '\u2029'
                && c != '\u202f'
                && c != '\u205f'
                && c != '\u3000'
                && char.IsWhiteSpace(c);
        }

        [Hidden]
        public static JSObject parse(string code, Function reviewer)
        {
            Stack<StackFrame> stack = new Stack<StackFrame>();
            Arguments revargs = reviewer != null ? new Arguments() { length = 2 } : null;
            stack.Push(new StackFrame() { container = null, value = null, state = ParseState.Value });
            int pos = 0;
            while (code.Length > pos && isSpace(code[pos]))
                pos++;
            while (pos < code.Length)
            {
                int start = pos;
                if (char.IsDigit(code[start]) || (code[start] == '-' && char.IsDigit(code[start + 1])))
                {
                    if (stack.Peek().state != ParseState.Value)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    double value;
                    if (!Tools.ParseNumber(code, ref pos, out value))
                        throw new JSException((new SyntaxError("Invalid number definition.")));
                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = value;
                }
                else if (code[start] == '"')
                {
                    Parser.ValidateString(code, ref pos, true);
                    string value = code.Substring(start + 1, pos - start - 2);
                    for (var i = value.Length; i-- > 0; )
                    {
                        if ((value[i] >= 0 && value[i] <= 0x1f))
                            throw new JSException(new SyntaxError("Invalid string char '\\u000" + (int)value[i] + "'"));
                    }
                    if (stack.Peek().state == ParseState.Name)
                    {
                        stack.Peek().fieldName = value;
                        stack.Peek().state = ParseState.Value;
                        while (isSpace(code[pos]))
                            pos++;
                        if (code[pos] != ':')
                            throw new JSException((new SyntaxError("Unexpected token.")));
                        pos++;
                    }
                    else
                    {
                        value = Tools.Unescape(value, false);
                        if (stack.Peek().state != ParseState.Value)
                            throw new JSException((new SyntaxError("Unexpected token.")));
                        var v = stack.Peek();
                        v.state = ParseState.End;
                        v.value = value;
                    }
                }
                else if (Parser.Validate(code, "null", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = JSObject.Null;
                }
                else if (Parser.Validate(code, "true", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = true;
                }
                else if (Parser.Validate(code, "false", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = true;
                }
                else if (code[pos] == '{')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    stack.Peek().value = JSObject.CreateObject();
                    stack.Peek().state = ParseState.Object;
                    //stack.Push(new StackFrame() { state = ParseState.Name, container = stack.Peek().value });
                    pos++;
                }
                else if (code[pos] == '[')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException((new SyntaxError("Unexpected token.")));
                    stack.Peek().value = new BaseTypes.Array();
                    stack.Peek().state = ParseState.Array;
                    //stack.Push(new StackFrame() { state = ParseState.Value, fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), container = stack.Peek().value });
                    pos++;
                }
                else if (stack.Peek().state != ParseState.End)
                    throw new JSException((new SyntaxError("Unexpected token.")));
                if (stack.Peek().state == ParseState.End)
                {
                    var t = stack.Pop();
                    if (reviewer != null)
                    {
                        revargs[0] = t.fieldName;
                        revargs[1] = t.value;
                        var val = reviewer.Invoke(revargs);
                        if (val.IsDefinded)
                        {
                            if (t.container != null)
                                t.container.GetMember(t.fieldName, true, true).Assign(val);
                            else
                            {
                                t.value = val;
                                stack.Push(t);
                            }
                        }
                    }
                    else if (t.container != null)
                        t.container.GetMember(t.fieldName, true, true).Assign(t.value);
                    else
                        stack.Push(t);
                }
                while (code.Length > pos && isSpace(code[pos]))
                    pos++;
                if (code.Length <= pos)
                {
                    if (stack.Peek().state != ParseState.End)
                        throw new JSException(new SyntaxError("Unexpected end of string."));
                    else
                        break;
                }
                switch (code[pos])
                {
                    case ',':
                        {
                            if (stack.Peek().state == ParseState.Array)
                                stack.Push(new StackFrame() { state = ParseState.Value, fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), container = stack.Peek().value });
                            else if (stack.Peek().state == ParseState.Object)
                                stack.Push(new StackFrame() { state = ParseState.Name, container = stack.Peek().value });
                            else
                                throw new JSException((new SyntaxError("Unexpected token.")));
                            pos++;
                            break;
                        }
                    case ']':
                        {
                            if (stack.Peek().state != ParseState.Array)
                                throw new JSException((new SyntaxError("Unexpected token.")));
                            stack.Peek().state = ParseState.End;
                            pos++;
                            break;
                        }
                    case '}':
                        {
                            if (stack.Peek().state != ParseState.Object)
                                throw new JSException((new SyntaxError("Unexpected token.")));
                            stack.Peek().state = ParseState.End;
                            pos++;
                            break;
                        }
                    default:
                        {
                            if (stack.Peek().state == ParseState.Array)
                                stack.Push(new StackFrame() { state = ParseState.Value, fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), container = stack.Peek().value });
                            else if (stack.Peek().state == ParseState.Object)
                                stack.Push(new StackFrame() { state = ParseState.Name, container = stack.Peek().value });
                            continue;
                        }
                }
                while (code.Length > pos && isSpace(code[pos]))
                    pos++;
                if (code.Length <= pos && stack.Peek().state != ParseState.End)
                    throw new JSException(new SyntaxError("Unexpected end of string."));
            }
            return stack.Pop().value;
        }

        [DoNotEnumerate]
        [ParametersCount(3)]
        public static JSObject stringify(Arguments args)
        {
            var length = Tools.JSObjectToInt32(args.length);
            Function replacer = length > 1 ? args[1].oValue as Function : null;
            string space = "";
            if (args.length > 2)
            {
                var sa = args[2];
                sa = sa.oValue as JSObject ?? sa;
                if (sa.valueType == JSObjectType.Int
                    || sa.valueType == JSObjectType.Double
                    || sa.valueType == JSObjectType.String)
                {
                    if (sa.valueType == JSObjectType.Int)
                        space = "          ".Substring(10 - System.Math.Max(0, System.Math.Min(10, sa.iValue)));
                    else if (sa.valueType == JSObjectType.Double)
                        space = "          ".Substring(10 - System.Math.Max(0, System.Math.Min(10, (int)sa.dValue)));
                    else
                    {
                        space = sa.ToString();
                        if (space.Length > 10)
                            space = space.Substring(0, 10);
                    }
                }
            }
            var target = args[0];
            return stringify(target.oValue as JSObject ?? target, replacer, space) ?? JSObject.undefined;
        }

        [Hidden]
        public static string stringify(JSObject obj, Function replacer, string space)
        {
            return stringifyImpl("", obj, replacer, space, new List<JSObject>(), new Arguments());
        }

        private static string stringifyImpl(string key, JSObject obj, Function replacer, string space, List<JSObject> processed, Arguments args)
        {
            if (processed.IndexOf(obj) != -1)
                throw new JSException(new TypeError("Can not convert circular structure to JSON."));
            processed.Add(obj);
            try
            {
                {
                    if (replacer != null)
                    {
                        args[0] = "";
                        args[0].oValue = key;
                        args[1] = obj;
                        args.length = 2;
                        var t = replacer.Invoke(args);
                        if (t.valueType <= JSObjectType.Undefined || (t.valueType >= JSObjectType.Object && t.oValue == null))
                            return null;
                        obj = t;
                    }
                }
                if (obj.valueType <= JSObjectType.Undefined
                    || obj.valueType == JSObjectType.Function)
                    return null;
                obj = obj.oValue as JSObject ?? obj;
                if (obj.valueType < JSObjectType.Object)
                {
                    if (obj.valueType == JSObjectType.String)
                        return "\"" + (obj.oValue.ToString())
                            .Replace("\\", "\\\\")
                            .Replace("\"", "\\\"")
                            .Replace("\n", "\\\n")
                            .Replace("\r", "\\\r")
                            .Replace("\n\\\r", "\n\r")
                            .Replace("\r\\\n", "\r\n") + '"';
                    return obj.ToString();
                }
                if (obj.oValue == null)
                    return "null";
                var toJSONmemb = obj["toJSON"];
                toJSONmemb = toJSONmemb.oValue as JSObject ?? toJSONmemb;
                if (toJSONmemb.valueType == JSObjectType.Function)
                    return stringifyImpl("", (toJSONmemb.oValue as Function).Invoke(obj, null), null, space, processed, null);
                StringBuilder res = new StringBuilder(obj is Array ? "[" : "{");
                bool first = true;
                foreach (var member in obj)
                {
                    var value = obj[member];
                    value = value.oValue as JSObject ?? value;
                    if (value.valueType < JSObjectType.Undefined)
                        continue;
                    if (value.valueType == JSObjectType.Property)
                        value = ((value.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(obj, null);
                    string strval = stringifyImpl(member, value, replacer, space, processed, args);
                    if (strval == null)
                        continue;
                    if (!first)
                        res.Append(",").Append(space);
                    if (res[0] == '[')
                    {
                        for (var i = 0; i < strval.Length; i++)
                        {
                            if (strval[i] >= 0 && strval[i] <= 0x1f)
                            {
                                switch (strval[i])
                                {
                                    case (char)8:
                                        {
                                            res.Append("\\b");
                                            break;
                                        }
                                    case (char)9:
                                        {
                                            res.Append("\\t");
                                            break;
                                        }
                                    case (char)10:
                                        {
                                            res.Append("\\n");
                                            break;
                                        }
                                    case (char)12:
                                        {
                                            res.Append("\\f");
                                            break;
                                        }
                                    case (char)13:
                                        {
                                            res.Append("\\r");
                                            break;
                                        }
                                    default:
                                        {
                                            res.Append("\\u").Append(((int)strval[i]).ToString("x4"));
                                            break;
                                        }
                                }
                            }
                            else
                                res.Append(strval[i]);
                        }
                    }
                    else
                    {
                        res.Append('"');//.Append(member).Append("\":").Append(strval);
                        for (var i = 0; i < member.Length; i++)
                        {
                            if (member[i] >= 0 && member[i] <= 0x1f)
                            {
                                switch (member[i])
                                {
                                    case (char)8:
                                        {
                                            res.Append("\\b");
                                            break;
                                        }
                                    case (char)9:
                                        {
                                            res.Append("\\t");
                                            break;
                                        }
                                    case (char)10:
                                        {
                                            res.Append("\\n");
                                            break;
                                        }
                                    case (char)12:
                                        {
                                            res.Append("\\f");
                                            break;
                                        }
                                    case (char)13:
                                        {
                                            res.Append("\\r");
                                            break;
                                        }
                                    default:
                                        {
                                            res.Append("\\u").Append(((int)member[i]).ToString("x4"));
                                            break;
                                        }
                                }
                            }
                            else
                                res.Append(member[i]);
                        }
                        res.Append("\":");
                        for (var i = 0; i < strval.Length; i++)
                        {
                            if (strval[i] >= 0 && strval[i] <= 0x1f)
                            {
                                switch (strval[i])
                                {
                                    case (char)8:
                                        {
                                            res.Append("\\b");
                                            break;
                                        }
                                    case (char)9:
                                        {
                                            res.Append("\\t");
                                            break;
                                        }
                                    case (char)10:
                                        {
                                            res.Append("\\n");
                                            break;
                                        }
                                    case (char)12:
                                        {
                                            res.Append("\\f");
                                            break;
                                        }
                                    case (char)13:
                                        {
                                            res.Append("\\r");
                                            break;
                                        }
                                    default:
                                        {
                                            res.Append("\\u").Append(((int)strval[i]).ToString("x4"));
                                            break;
                                        }
                                }
                            }
                            else
                                res.Append(strval[i]);
                        }
                    }
                    first = false;
                }
                return res.Append(obj is Array ? "]" : "}").ToString();
            }
            finally
            {
                processed.RemoveAt(processed.Count - 1);
            }
        }
    }
}
