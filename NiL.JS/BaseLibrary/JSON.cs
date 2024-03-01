﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary;

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
        var length = args._iValue;
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
        var revargs = reviewer != null ? new Arguments() { _iValue = 2 } : null;
        stack.Push(new StackFrame() { container = null, value = null, state = ParseState.Value });

        while (code.Length > pos && isSpace(code[pos]))
            pos++;

        while (pos < code.Length)
        {
            var newObject = false;
            var start = pos;
            var frame = stack.Peek();
            if (NumberUtils.IsDigit(code[start]) || (code[start] == '-' && NumberUtils.IsDigit(code[start + 1])))
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                double value;
                if (!Tools.ParseJsNumber(code, ref pos, out value))
                    ExceptionHelper.ThrowSyntaxError("Invalid number definition.");

                var intValue = (int)value;

                frame.state = ParseState.End;
                if (intValue == value)
                    frame.value = intValue;
                else
                    frame.value = value;
            }
            else if (code[start] == '"')
            {
                if (!Parser.ValidateString(code, ref pos, true))
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                var value = code.Substring(start + 1, pos - start - 2);
                for (var i = value.Length; i-- > 0;)
                {
                    if ((value[i] >= 0 && value[i] <= 0x1f))
                        ExceptionHelper.ThrowSyntaxError("Invalid string char '\\u000" + (int)value[i] + "'.");
                }

                if (frame.state == ParseState.Name)
                {
                    frame.fieldName = value;
                    frame.state = ParseState.Value;

                    while (isSpace(code[pos]))
                        pos++;

                    if (code[pos] != ':')
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    pos++;
                }
                else
                {
                    if (frame.state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    value = Tools.Unescape(value, false);

                    var v = frame;
                    v.state = ParseState.End;
                    v.value = value;
                }
            }
            else if (Parser.Validate(code, "null", ref pos))
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                var v = frame;
                v.state = ParseState.End;
                v.value = JSValue.@null;
            }
            else if (Parser.Validate(code, "true", ref pos))
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                var v = frame;
                v.state = ParseState.End;
                v.value = true;
            }
            else if (Parser.Validate(code, "false", ref pos))
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                frame.state = ParseState.End;
                frame.value = false;
            }
            else if (code[pos] == '{')
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                frame.value = JSObject.CreateObject();
                frame.state = ParseState.Object;
                newObject = true;
                pos++;
            }
            else if (code[pos] == '[')
            {
                if (frame.state != ParseState.Value)
                    ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                frame.value = new Array();
                frame.state = ParseState.Array;
                newObject = true;
                pos++;
            }
            else if (frame.state == ParseState.Value)
                ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

            while (code.Length > pos && isSpace(code[pos]))
                pos++;

            if (frame.state == ParseState.End)
            {
                stack.Pop();
                if (reviewer != null)
                {
                    revargs[0] = frame.fieldName;
                    revargs[1] = frame.value;
                    var value = reviewer.Call(revargs);
                    if (value.Defined)
                    {
                        if (frame.container != null)
                        {
                            frame.container.GetProperty(frame.fieldName, true, PropertyScope.Own).Assign(value);
                        }
                        else
                        {
                            frame.value = value;
                            stack.Push(frame);
                        }
                    }
                }
                else if (frame.container != null)
                {
                    frame.container.GetProperty(frame.fieldName, true, PropertyScope.Own).Assign(frame.value);
                }
                else
                {
                    stack.Push(frame);
                }

                frame = stack.Peek();
            }

            if (code.Length <= pos)
            {
                if (frame.state != ParseState.End)
                    ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource);
                else
                    break;
            }

            switch (code[pos])
            {
                case ',':
                {
                    if (newObject)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    if (frame.state == ParseState.Array)
                        frame = new StackFrame() { state = ParseState.Value, fieldName = (frame.valuesCount++).ToString(CultureInfo.InvariantCulture), container = frame.value };
                    else if (frame.state == ParseState.Object)
                        frame = new StackFrame() { state = ParseState.Name, container = frame.value };
                    else
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    stack.Push(frame);
                    pos++;
                    break;
                }
                case ']':
                {
                    if (frame.state != ParseState.Array)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    frame.state = ParseState.End;
                    pos++;
                    break;
                }
                case '}':
                {
                    if (frame.state != ParseState.Object)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    frame.state = ParseState.End;
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

                    if (frame.state != ParseState.Value)
                        ExceptionHelper.ThrowSyntaxError("Unexpected token at position " + pos);

                    break;
                }
            }

            while (code.Length > pos && isSpace(code[pos]))
                pos++;

            if (code.Length <= pos && frame.state != ParseState.End)
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource);
        }

        if ((stack.Count != 1)
            || (code.Length > pos)
            || (stack.Peek().state != ParseState.End))
            ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource);

        return stack.Pop().value;
    }

    [Hidden]
    public static string stringify(JSValue value)
    {
        return stringify(new Arguments { value }).ToString();
    }

    [DoNotEnumerate]
    [ArgumentsCount(3)]
    public static JSValue stringify(Arguments args)
    {
        var length = args._iValue;
        var replacer = length > 1 ? args[1]._oValue as Function : null;
        var keys = length > 1 ? args[1]._oValue as Array : null;
        string space = null;
        if (args._iValue > 2)
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
                    else if (space.Length == 0)
                        space = null;
                }
            }
        }

        var target = args[0];

        var keysSet = keys == null ? null : new HashSet<string>();
        if (keysSet != null)
        {
            foreach (var key in keys._data)
                keysSet.Add(key.ToString());
        }

        return stringify(target, replacer, keysSet, space) ?? JSValue.undefined;
    }

    [Hidden]
    public static string stringify(JSValue obj, Function replacer, HashSet<string> keys, string space)
    {
        if (obj._valueType >= JSValueType.Object && obj.Value == null)
            return "null";

        return stringifyImpl("", obj, replacer, keys, space, null, replacer != null ? new Arguments() : null);
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

    private static string stringifyImpl(string key, JSValue obj, Function replacer, HashSet<string> keys, string space, HashSet<JSValue> processed, Arguments args)
    {
        if (replacer != null)
        {
            args[0] = "";
            args[0]._oValue = key;
            args[1] = obj;
            args._iValue = 2;
            var t = replacer.Call(args);
            if (t._valueType >= JSValueType.Object && t._oValue == null)
                return "null";
            if (t._valueType <= JSValueType.Undefined)
                return null;
            obj = t;
        }

        obj = obj.Value as JSValue ?? obj;

        try
        {
            StringBuilder result = null;
            string stringValue = null;
            if (obj._valueType < JSValueType.Object)
            {
                if (obj._valueType <= JSValueType.Undefined)
                    return null;

                if (obj._valueType == JSValueType.String)
                {
                    result = new StringBuilder("\"");
                    stringValue = obj.ToString();
                    for (var i = 0; i < stringValue.Length; i++)
                        escapeIfNeed(result, stringValue[i]);
                    result.Append('"');
                    return result.ToString();
                }

                if (obj.ValueType == JSValueType.Double && double.IsNaN(obj._dValue) || double.IsInfinity(obj._dValue))
                    return "null";

                return obj.ToString();
            }

            if (processed == null)
                processed = new HashSet<JSValue>();

            if (processed.Contains(obj))
                ExceptionHelper.Throw(new TypeError("Unable to convert circular structure to JSON."));

            processed.Add(obj);

            if (obj.Value == null)
                return "null";

            if (obj._valueType == JSValueType.Function)
                return null;

            var toJSONmemb = obj["toJSON"];
            toJSONmemb = toJSONmemb.Value as JSValue ?? toJSONmemb;
            if (toJSONmemb._valueType == JSValueType.Function)
                return stringifyImpl("", (toJSONmemb._oValue as Function).Call(obj, null), null, null, space, processed, null);

            if (obj._valueType >= JSValueType.Object && !typeof(JSValue).GetTypeInfo().IsAssignableFrom(obj.Value.GetType().GetTypeInfo()))
            {
                var currentContext = Context.CurrentGlobalContext;
                if (currentContext != null)
                {
                    var value = obj.Value;
                    var serializer = currentContext.JsonSerializersRegistry?.GetSuitableJsonSerializer(value);
                    if (serializer != null)
                    {
                        return serializer.Serialize(key, value, replacer, keys, space, processed);
                    }
                }
            }

            result = new StringBuilder(obj is Array ? "[" : "{");

            string prevKey = null;
            foreach (var member in obj)
            {
                if (keys != null && !keys.Contains(member.Key))
                    continue;

                var value = member.Value;
                value = value.Value as JSValue ?? value;
                if (value._valueType < JSValueType.Undefined)
                    continue;

                value = Tools.GetPropertyOrValue(value, obj);
                stringValue = stringifyImpl(member.Key, value, replacer, null, space, processed, args);

                if (stringValue == null)
                {
                    if (obj is Array)
                        stringValue = "null";
                    else
                        continue;
                }

                if (prevKey != null)
                    result.Append(",");

                if (space != null)
                {
                    result.Append(Environment.NewLine)
                       .Append(space);
                }

                if (result[0] == '[')
                {
                    int curentIndex;
                    if (int.TryParse(member.Key, out curentIndex))
                    {
                        var prevIndex = int.Parse(prevKey ?? "-1");

                        var capacity = result.Length + "null,".Length * (curentIndex - prevIndex);
                        if (capacity > result.Length) // Может произойти переполнение
                            result.EnsureCapacity(capacity);

                        for (var i = curentIndex - 1; i-- > prevIndex;)
                        {
                            result.Append("null,");
                        }
                    }
                }
                else
                {
                    result.Append('"');
                    for (var i = 0; i < member.Key.Length; i++)
                        escapeIfNeed(result, member.Key[i]);

                    result.Append("\":").Append(space == null ? string.Empty : " ");

                    result.EnsureCapacity(result.Length + stringValue.Length);
                }

                var newLineIndex = 0;
                for (var i = 0; i < stringValue.Length; i++)
                {
                    if (newLineIndex < Environment.NewLine.Length
                        && stringValue[i] == Environment.NewLine[newLineIndex])
                    {
                        newLineIndex++;
                    }
                    else if (newLineIndex != 0)
                    {
                        result.Append(space);
                        newLineIndex = 0;
                    }

                    result.Append(stringValue[i]);
                }

                prevKey = member.Key;
            }

            if (space != null)
            {
                if (prevKey != null)
                    result.Append(Environment.NewLine);
            }

            return result.Append(obj is Array ? "]" : "}").ToString();
        }
        finally
        {
            processed?.Remove(obj);
        }
    }
}
