using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
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
            public JSValue container;
            public JSValue value;
            public JSValue fieldName;
            public int valuesCount;
            public ParseState state;
        }

        [DoNotEnumerate]
        [ArgumentsCount(2)]
        public static JSValue parse(Arguments args)
        {
            var length = Tools.JSObjectToInt32(args.length);
            var code = args[0].ToString();
            Function reviewer = length > 1 ? args[1]._oValue as Function : null;
            return parse(code, reviewer);
        }

        [Hidden]
        public static JSValue parse(string code)
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
        public static JSValue parse(string code, Function reviewer)
        {
            var stack = new Stack<StackFrame>();
            var pos = 0;
            var revargs = reviewer != null ? new Arguments() { length = 2 } : null;
            stack.Push(new StackFrame() { container = null, value = null, state = ParseState.Value });

            while (code.Length > pos && isSpace(code[pos]))
                pos++;

            while (pos < code.Length)
            {
                var newObject = false;
                var start = pos;
                if (Tools.IsDigit(code[start]) || (code[start] == '-' && Tools.IsDigit(code[start + 1])))
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    double value;
                    if (!Tools.ParseNumber(code, ref pos, out value))
                        ExceptionHelper.ThrowSyntaxError("Invalid number definition.");

                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = value;
                }
                else if (code[start] == '"')
                {
                    if (!Parser.ValidateString(code, ref pos, true))
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var value = code.Substring(start + 1, pos - start - 2);
                    for (var i = value.Length; i-- > 0;)
                    {
                        if ((value[i] >= 0 && value[i] <= 0x1f))
                            ExceptionHelper.ThrowSyntaxError("Invalid string char '\\u000" + (int)value[i] + "'.");
                    }

                    if (stack.Peek().state == ParseState.Name)
                    {
                        stack.Peek().fieldName = value;
                        stack.Peek().state = ParseState.Value;

                        while (isSpace(code[pos]))
                            pos++;

                        if (code[pos] != ':')
                            ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                        pos++;
                    }
                    else
                    {
                        if (stack.Peek().state != ParseState.Value)
                            ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                        value = Tools.Unescape(value, false);

                        var v = stack.Peek();
                        v.state = ParseState.End;
                        v.value = value;
                    }
                }
                else if (Parser.Validate(code, "null", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = JSValue.@null;
                }
                else if (Parser.Validate(code, "true", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = true;
                }
                else if (Parser.Validate(code, "false", ref pos))
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var v = stack.Peek();
                    v.state = ParseState.End;
                    v.value = false;
                }
                else if (code[pos] == '{')
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var v = stack.Peek();
                    v.value = JSObject.CreateObject();
                    v.state = ParseState.Object;
                    newObject = true;
                    pos++;
                }
                else if (code[pos] == '[')
                {
                    if (stack.Peek().state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                    var v = stack.Peek();
                    v.value = new Array();
                    v.state = ParseState.Array;
                    newObject = true;
                    pos++;
                }
                else if (stack.Peek().state == ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                while (code.Length > pos && isSpace(code[pos]))
                    pos++;

                if (stack.Peek().state == ParseState.End)
                {
                    var t = stack.Pop();
                    if (reviewer != null)
                    {
                        revargs[0] = t.fieldName;
                        revargs[1] = t.value;
                        var value = reviewer.Call(revargs);
                        if (value.Defined)
                        {
                            if (t.container != null)
                                t.container.GetProperty(t.fieldName, true, PropertyScope.Own).Assign(value);
                            else
                            {
                                t.value = value;
                                stack.Push(t);
                            }
                        }
                    }
                    else if (t.container != null)
                        t.container.GetProperty(t.fieldName, true, PropertyScope.Own).Assign(t.value);
                    else
                        stack.Push(t);
                }

                if (code.Length <= pos)
                {
                    if (stack.Peek().state != ParseState.End)
                        ExceptionHelper.Throw(new SyntaxError("Unexpected end of string."));
                    else
                        break;
                }

                switch (code[pos])
                {
                    case ',':
                        {
                            if (newObject)
                                ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                            if (stack.Peek().state == ParseState.Array)
                                stack.Push(new StackFrame() { state = ParseState.Value, fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), container = stack.Peek().value });
                            else if (stack.Peek().state == ParseState.Object)
                                stack.Push(new StackFrame() { state = ParseState.Name, container = stack.Peek().value });
                            else
                                ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                            pos++;
                            break;
                        }
                    case ']':
                        {
                            if (stack.Peek().state != ParseState.Array)
                                ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                            stack.Peek().state = ParseState.End;
                            pos++;
                            break;
                        }
                    case '}':
                        {
                            if (stack.Peek().state != ParseState.Object)
                                ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                            stack.Peek().state = ParseState.End;
                            pos++;
                            break;
                        }
                    default:
                        {
                            if (newObject)
                            {
                                pos--;
                                newObject = false;
                                goto case ',';
                            }

                            if (stack.Peek().state != ParseState.Value)
                                ExceptionHelper.ThrowSyntaxError("Unexpected token.");

                            break;
                        }
                }

                while (code.Length > pos && isSpace(code[pos]))
                    pos++;

                if (code.Length <= pos && stack.Peek().state != ParseState.End)
                    ExceptionHelper.ThrowSyntaxError("Unexpected end of string.");
            }

            if ((stack.Count != 1) 
                || (code.Length > pos) 
                || (stack.Peek().state != ParseState.End))
                ExceptionHelper.ThrowSyntaxError("Unexpected end of string.");

            return stack.Pop().value;
        }

        [DoNotEnumerate]
        [ArgumentsCount(3)]
        public static JSValue stringify(Arguments args)
        {
            var length = args.length;
            var replacer = length > 1 ? args[1]._oValue as Function : null;
            var keys = length > 1 ? args[1]._oValue as Array : null;
            string space = null;
            if (args.length > 2)
            {
                var sa = args[2];
                if (sa._valueType >= JSValueType.Object)
                    sa = sa._oValue as JSValue ?? sa;
                if (sa is ObjectWrapper)
                    sa = sa.Value as JSValue ?? sa;
                if (sa._valueType == JSValueType.Integer
                    || sa._valueType == JSValueType.Double
                    || sa._valueType == JSValueType.String)
                {
                    if (sa._valueType == JSValueType.Integer)
                    {
                        if (sa._iValue > 0)
                            space = "          ".Substring(10 - System.Math.Max(0, System.Math.Min(10, sa._iValue)));
                    }
                    else if (sa._valueType == JSValueType.Double)
                    {
                        if ((int)sa._dValue > 0)
                            space = "          ".Substring(10 - System.Math.Max(0, System.Math.Min(10, (int)sa._dValue)));
                    }
                    else
                    {
                        space = sa.ToString();
                        if (space.Length > 10)
                            space = space.Substring(0, 10);
                        if (space.Length == 0)
                            space = null;
                    }
                }
            }
            var target = args[0];
            return stringify(target, replacer, keys, space) ?? JSValue.undefined;
        }

        [Hidden]
        public static string stringify(JSValue obj, Function replacer, Array keys, string space)
        {
            if (obj._valueType >= JSValueType.Object && obj.Value == null)
                return "null";

            var keysSet = keys == null ? null : new HashSet<string>();
            if (keysSet != null)
            {
                foreach (var key in keys._data)
                    keysSet.Add(key.ToString());
            }

            return stringifyImpl("", obj, replacer, keysSet, space, new List<JSValue>(), new Arguments());
        }

        internal static void escapeIfNeed(StringBuilder sb, char c)
        {
            if ((c >= 0 && c <= 0x1f)
                || (c == '\\')
                || (c == '"'))
            {
                switch (c)
                {
                    case (char)8:
                    {
                        sb.Append("\\b");
                        break;
                    }
                    case (char)9:
                    {
                        sb.Append("\\t");
                        break;
                    }
                    case (char)10:
                    {
                        sb.Append("\\n");
                        break;
                    }
                    case (char)12:
                    {
                        sb.Append("\\f");
                        break;
                    }
                    case (char)13:
                    {
                        sb.Append("\\r");
                        break;
                    }
                    case '\\':
                    {
                        sb.Append("\\\\");
                        break;
                    }
                    case '"':
                    {
                        sb.Append("\\\"");
                        break;
                    }
                    default:
                    {
                        sb.Append("\\u").Append(((int)c).ToString("x4"));
                        break;
                    }
                }
            }
            else
                sb.Append(c);
        }

        private static string stringifyImpl(string key, JSValue obj, Function replacer, HashSet<string> keys, string space, List<JSValue> processed, Arguments args)
        {
            if (replacer != null)
            {
                args[0] = "";
                args[0]._oValue = key;
                args[1] = obj;
                args.length = 2;
                var t = replacer.Call(args);
                if (t._valueType >= JSValueType.Object && t._oValue == null)
                    return "null";
                if (t._valueType <= JSValueType.Undefined)
                    return null;
                obj = t;
            }

            obj = obj.Value as JSValue ?? obj;

            if (processed.IndexOf(obj) != -1)
                ExceptionHelper.Throw(new TypeError("Unable to convert circular structure to JSON."));

            processed.Add(obj);
            try
            {
                StringBuilder res = null;
                string stringValue = null;
                if (obj._valueType < JSValueType.Object)
                {
                    if (obj._valueType <= JSValueType.Undefined)
                        return null;

                    if (obj._valueType == JSValueType.String)
                    {
                        res = new StringBuilder("\"");
                        stringValue = obj.ToString();
                        for (var i = 0; i < stringValue.Length; i++)
                            escapeIfNeed(res, stringValue[i]);
                        res.Append('"');
                        return res.ToString();
                    }

                    if (obj.ValueType == JSValueType.Double && double.IsNaN(obj._dValue) || double.IsInfinity(obj._dValue))
                        return "null";

                    return obj.ToString();
                }

                if (obj.Value == null)
                    return null;
                if (obj._valueType == JSValueType.Function)
                    return null;

                var toJSONmemb = obj["toJSON"];
                toJSONmemb = toJSONmemb.Value as JSValue ?? toJSONmemb;
                if (toJSONmemb._valueType == JSValueType.Function)
                    return stringifyImpl("", (toJSONmemb._oValue as Function).Call(obj, null), null, null, space, processed, null);

                res = new StringBuilder(obj is Array ? "[" : "{");

                string prevKey = null;
                foreach (var member in obj)
                {
                    if (keys != null && !keys.Contains(member.Key))
                        continue;

                    var value = member.Value;
                    value = value.Value as JSValue ?? value;
                    if (value._valueType < JSValueType.Undefined)
                        continue;

                    value = Tools.InvokeGetter(value, obj);
                    stringValue = stringifyImpl(member.Key, value, replacer, null, space, processed, args);

                    if (stringValue == null)
                    {
                        if (obj is Array)
                            stringValue = "null";
                        else
                            continue;
                    }

                    if (prevKey != null)
                        res.Append(",");

                    if (space != null)
                        res.Append(Environment.NewLine);
                    if (space != null)
                        res.Append(space);

                    if (res[0] == '[')
                    {
                        int curentIndex;
                        if (int.TryParse(member.Key, out curentIndex))
                        {
                            var prevIndex = int.Parse(prevKey ?? "-1");

                            var capacity = res.Length + ((space?.Length ?? 0) + "null,".Length) * (curentIndex - prevIndex);
                            if (capacity > res.Length) // Может произойти переполнение
                                res.EnsureCapacity(capacity);

                            for (var i = curentIndex - 1; i-- > prevIndex;)
                            {
                                res.Append(space)
                                   .Append("null,");
                            }

                            res.Append(space)
                               .Append(stringValue);

                            prevKey = member.Key;
                        }
                    }
                    else
                    {
                        res.Append('"');
                        for (var i = 0; i < member.Key.Length; i++)
                            escapeIfNeed(res, member.Key[i]);

                        res.Append("\":")
                           .Append(space ?? "");

                        for (var i = 0; i < stringValue.Length; i++)
                        {
                            res.Append(stringValue[i]);
                            if (i >= Environment.NewLine.Length && stringValue.IndexOf(Environment.NewLine, i - 1, Environment.NewLine.Length) != -1)
                                res.Append(space);
                        }

                        prevKey = member.Key;
                    }
                }

                if (prevKey != null && space != null)
                    res.Append(Environment.NewLine);

                return res.Append(obj is Array ? "]" : "}").ToString();
            }
            finally
            {
                processed.RemoveAt(processed.Count - 1);
            }
        }
    }
}
