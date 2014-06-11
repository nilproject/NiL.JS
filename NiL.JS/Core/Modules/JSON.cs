using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.BaseTypes;
using System.Globalization;

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
            Array
        }

        private class StackFrame
        {
            public JSObject obj;
            public string fieldName = "";
            public ParseState state;
            public int valuesCount;
        }

        public static JSObject parse(string code)
        {
            return parse(code, null);
        }

        public static JSObject parse(string code, Function reviewer)
        {
            Stack<StackFrame> stack = new Stack<StackFrame>();
            BaseTypes.Array revargs = reviewer != null ? new BaseTypes.Array(2) : null;
            stack.Push(new StackFrame() { obj = JSObject.CreateObject() });
            stack.Push(new StackFrame() { obj = JSObject.CreateObject() });
            int pos = 0;
            code = code.Trim();
            while (pos < code.Length)
            {
                int start = pos;
                bool waitControlChar = true;
                bool waitComma = true;
                if (Parser.ValidateValue(code, ref pos))
                {
                    if (char.IsDigit(code[start]) || (code[start] == '-' && char.IsDigit(code[start + 1])))
                    {
                        if (stack.Peek().state == ParseState.Name)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        double value;
                        if (!Tools.ParseNumber(code, ref start, out value))
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid number definition.")));
                        JSObject val = value;
                        var v = stack.Pop();
                        if (reviewer != null)
                        {
                            revargs.data[0] = v.fieldName;
                            revargs.data[1] = val;
                            val = reviewer.Invoke(revargs);
                            if (val.valueType <= JSObjectType.Undefined)
                                val = null;
                        }
                        if (val != null)
                            stack.Peek().obj.GetMember(v.fieldName, true, true).Assign(val);
                    }
                    else
                    {
                        string value = code.Substring(start, pos - start);
                        if (value[0] != '"')
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        value = Tools.Unescape(value.Substring(1, value.Length - 2), false);
                        if (stack.Peek().state == ParseState.Name)
                        {
                            stack.Peek().fieldName = value;
                            stack.Peek().state = ParseState.Value;
                            while (char.IsWhiteSpace(code[pos])) pos++;
                            if (code[pos] != ':')
                                throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                            pos++;
                            waitControlChar = false;
                            waitComma = false;
                        }
                        else
                        {
                            var v = stack.Pop();
                            JSObject val = value;
                            if (reviewer != null)
                            {
                                revargs.data[0] = v.fieldName;
                                revargs.data[1] = val;
                                val = reviewer.Invoke(revargs);
                                if (val.valueType <= JSObjectType.Undefined)
                                    val = null;
                            }
                            if (val != null)
                                stack.Peek().obj.GetMember(v.fieldName, true, true).Assign(val);
                        }
                    }
                }
                else if (code[pos] == '{')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                    stack.Peek().obj = JSObject.CreateObject();
                    stack.Peek().state = ParseState.Object;
                    waitComma = false;
                    pos++;
                }
                else if (code[pos] == '[')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                    stack.Peek().obj = new BaseTypes.Array();
                    stack.Peek().state = ParseState.Array;
                    waitComma = false;
                    pos++;
                }
                else throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                while (code.Length > pos && char.IsWhiteSpace(code[pos])) pos++;
                while (waitControlChar)
                {
                    if (stack.Peek().state == ParseState.Object || stack.Peek().state == ParseState.Array)
                    {
                        if ((stack.Peek().state == ParseState.Array && code[pos] == ']')
                            || (stack.Peek().state == ParseState.Object && code[pos] == '}'))
                        {
                            var t = stack.Pop();
                            if (reviewer != null)
                            {
                                revargs.data[0] = t.fieldName;
                                revargs.data[1] = t.obj;
                                t.obj = reviewer.Invoke(revargs);
                                if (t.obj.valueType <= JSObjectType.Undefined)
                                    t.obj = null;
                            }
                            if (t.obj != null)
                                stack.Peek().obj.GetMember(t.fieldName, true, true).Assign(t.obj);
                            do pos++; while (code.Length > pos && char.IsWhiteSpace(code[pos]));
                            continue;
                        }
                        else if (code[pos] == ',')
                        {
                            do pos++; while (code.Length > pos && char.IsWhiteSpace(code[pos]));
                            waitComma = false;
                            waitControlChar = false;
                        }
                        else if (waitComma)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        else
                            break;
                    }
                    else break;
                }
                if (stack.Peek().state == ParseState.Array)
                    stack.Push(new StackFrame() { fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), state = ParseState.Value });
                else if (stack.Peek().state == ParseState.Object)
                    stack.Push(new StackFrame() { state = ParseState.Name });
            }
            return stack.Peek().obj.GetMember("");
        }

        public static string stringify(JSObject obj)
        {
            return stringify(obj.GetMember("0"), null, null);
        }

        public static string stringify(JSObject obj, Function replacer)
        {
            return stringify(obj, replacer, null);
        }

        public static string stringify(JSObject obj, Function replacer, string space)
        {
            if (obj.valueType < JSObjectType.Object)
            {
                if (obj.valueType == JSObjectType.String)
                    return "\"" + (obj.oValue as string)
                        .Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\\n")
                        .Replace("\r", "\\\r")
                        .Replace("\n\\\r", "\n\r")
                        .Replace("\r\\\n", "\r\n") + '"';
                return obj.Value.ToString();
            }
            StringBuilder res = new StringBuilder("{");
            var args = new BaseTypes.Array(2);
            args.data[0] = "";
            bool first = true;
            foreach (var f in obj.fields)
            {
                if ((f.Value.valueType < JSObjectType.Undefined) && ((f.Value.attributes & JSObjectAttributes.DoNotEnum) == 0))
                    continue;
                var value = f.Value;
                if (replacer != null)
                {
                    args.data[0].oValue = f.Key;
                    args.data[1] = f.Value;
                    var t = replacer.Invoke(args);
                    if (t.valueType <= JSObjectType.Undefined || (t.valueType >= JSObjectType.Object && t.oValue == null))
                        continue;
                    value = t;
                }
                string strval = stringify(value, replacer, space);
                if (!first)
                    res.Append(", ");
                res.Append('"').Append(f.Key).Append("\": ").Append(strval).Append(space);
                first = false;
            }
            return res.Append("}").ToString();
        }
    }
}
