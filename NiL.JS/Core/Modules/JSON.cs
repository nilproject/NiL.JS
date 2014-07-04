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
            Array
        }

        private class StackFrame
        {
            public JSObject obj;
            public string fieldName = "";
            public ParseState state;
            public int valuesCount;
        }

        [DoNotEnumerate]
        public static JSObject parse(JSObject args)
        {
            var length = Tools.JSObjectToInt32(args["length"]);
            var code = args["0"].ToString();
            Function reviewer = length > 1 ? args["1"].oValue as Function : null;
            return parse(code, reviewer);
        }

        [Hidden]
        public static JSObject parse(string code)
        {
            return parse(code, null);
        }

        [Hidden]
        public static JSObject parse(string code, Function reviewer)
        {
            Stack<StackFrame> stack = new Stack<StackFrame>();
            Arguments revargs = reviewer != null ? new Arguments() { length = 2 } : null;
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
                            revargs[0] = v.fieldName;
                            revargs[1] = val;
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
                            while (char.IsWhiteSpace(code[pos]))
                                pos++;
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
                                revargs[0] = v.fieldName;
                                revargs[1] = val;
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
                else
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                while (code.Length > pos && char.IsWhiteSpace(code[pos]))
                    pos++;
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
                                revargs[0] = t.fieldName;
                                revargs[1] = t.obj;
                                t.obj = reviewer.Invoke(revargs);
                                if (t.obj.valueType <= JSObjectType.Undefined)
                                    t.obj = null;
                            }
                            if (t.obj != null)
                                stack.Peek().obj.GetMember(t.fieldName, true, true).Assign(t.obj);
                            do
                                pos++;
                            while (code.Length > pos && char.IsWhiteSpace(code[pos]));
                            continue;
                        }
                        else if (code[pos] == ',')
                        {
                            do
                                pos++;
                            while (code.Length > pos && char.IsWhiteSpace(code[pos]));
                            waitComma = false;
                            waitControlChar = false;
                        }
                        else if (waitComma)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        else
                            break;
                    }
                    else
                        break;
                }
                if (stack.Peek().state == ParseState.Array)
                    stack.Push(new StackFrame() { fieldName = (stack.Peek().valuesCount++).ToString(CultureInfo.InvariantCulture), state = ParseState.Value });
                else if (stack.Peek().state == ParseState.Object)
                    stack.Push(new StackFrame() { state = ParseState.Name });
            }
            return stack.Peek().obj.GetMember("");
        }

        [DoNotEnumerate]
        public static string stringify(Arguments args)
        {
            var length = Tools.JSObjectToInt32(args.length);
            Function replacer = length > 1 ? args[1].oValue as Function : null;
            string space = length > 1 ? args[2].ToString() : null;
            var target = args[0];
            return stringify(args.oValue as JSObject ?? args, replacer, space);
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
                    args["0"] = "";
                    if (replacer != null)
                    {
                        args["0"].oValue = key;
                        args["1"] = obj;
                        args["length"] = 2;
                        var t = replacer.Invoke(args);
                        if (t.valueType <= JSObjectType.Undefined || (t.valueType >= JSObjectType.Object && t.oValue == null))
                            return null;
                        obj = t;
                    }
                }
                if (obj.valueType < JSObjectType.Undefined
                    || obj.valueType == JSObjectType.Function)
                    return null;
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
                    return obj.ToString();
                }
                StringBuilder res = new StringBuilder(obj is Array ? "[" : "{");
                bool first = true;
                foreach (var member in obj)
                {
                    var value = obj[member];
                    value = value.oValue as JSObject ?? value;
                    if (value.valueType < JSObjectType.Undefined)
                        continue;
                    string strval = stringifyImpl(member, value, replacer, space, processed, args);
                    if (strval == null)
                        continue;
                    if (!first)
                        res.Append(",").Append(space);
                    if (res[0] == '[')
                        res.Append(strval);
                    else
                        res.Append('"').Append(member).Append("\": ").Append(strval);
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
