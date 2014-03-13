using System;
using System.Collections.Generic;
using System.Linq;
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
            IndexedValue
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
                if (Parser.ValidateValue(code, ref pos, true))
                {
                    if (char.IsDigit(code[start]))
                    {
                        if (stack.Peek().state == ParseState.Name)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        double value;
                        if (!Tools.ParseNumber(code, ref start, true, out value))
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid number definition.")));
                        JSObject val = value;
                        if (stack.Peek().state == ParseState.Value)
                        {
                            var v = stack.Pop();
                            if (reviewer != null)
                            {
                                revargs.data[0] = v.fieldName;
                                revargs.data[1] = val;
                                val = reviewer.Invoke(revargs);
                                if (val.ValueType <= JSObjectType.Undefined)
                                    val = null;
                            }
                            if (val != null)
                                stack.Peek().obj.GetField(v.fieldName, false, true).Assign(val);
                        }
                        else
                        {
                            var v = stack.Peek();
                            if (reviewer != null)
                            {
                                revargs.data[0] = v.valuesCount;
                                revargs.data[1] = val;
                                val = reviewer.Invoke(revargs);
                                if (val.ValueType <= JSObjectType.Undefined)
                                    val = null;
                            }
                            if (val != null)
                                stack.Peek().obj.GetField((v.valuesCount++).ToString(), false, true).Assign(val);
                        }
                    }
                    else
                    {
                        string value = code.Substring(start, pos - start);
                        if (value[0] != '"')
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                        value = value.Substring(1, value.Length - 2);
                        if (stack.Peek().state == ParseState.Name)
                        {
                            stack.Push(new StackFrame() { fieldName = value, state = ParseState.Value });
                            while (char.IsWhiteSpace(code[pos])) pos++;
                            if (code[pos] != ':')
                                throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                            pos++;
                            waitControlChar = false;
                            waitComma = false;
                        }
                        else if (stack.Peek().state == ParseState.Value)
                        {
                            var v = stack.Pop();
                            JSObject val = value;
                            if (reviewer != null)
                            {
                                revargs.data[0] = v.fieldName;
                                revargs.data[1] = val;
                                val = reviewer.Invoke(revargs);
                                if (val.ValueType <= JSObjectType.Undefined)
                                    val = null;
                            }
                            if (val != null)
                                stack.Peek().obj.GetField(v.fieldName, false, true).Assign(val);
                        }
                        else if (stack.Peek().state == ParseState.IndexedValue)
                        {
                            JSObject val = value;
                            if (reviewer != null)
                            {
                                revargs.data[0] = stack.Peek().valuesCount;
                                revargs.data[1] = val;
                                val = reviewer.Invoke(revargs);
                                if (val.ValueType <= JSObjectType.Undefined)
                                    val = null;
                            }
                            if (val != null)
                                stack.Peek().obj.GetField((stack.Peek().valuesCount++).ToString(), false, true).Assign(val);
                        }
                    }
                }
                else if (code[pos] == '{')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                    stack.Push(new StackFrame() { state = ParseState.Name, obj = JSObject.CreateObject() });
                    waitComma = false;
                    pos++;
                }
                else if (code[pos] == '[')
                {
                    if (stack.Peek().state == ParseState.Name)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                    stack.Push(new StackFrame() { state = ParseState.IndexedValue, obj = new BaseTypes.Array() });
                    waitComma = false;
                    pos++;
                }
                else throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected token.")));
                while (code.Length > pos && char.IsWhiteSpace(code[pos])) pos++;
                while (waitControlChar)
                {
                    if (stack.Peek().state == ParseState.Name || stack.Peek().state == ParseState.IndexedValue)
                    {
                        if (stack.Peek().state == ParseState.IndexedValue && code[pos] == ']')
                        {
                            var t = stack.Pop();
                            if (reviewer != null)
                            {
                                revargs.data[0] = t.fieldName;
                                revargs.data[1] = t.obj;
                                t.obj = reviewer.Invoke(revargs);
                                if (t.obj.ValueType <= JSObjectType.Undefined)
                                    t.obj = null;
                            }
                            if (t.obj != null)
                                stack.Peek().obj.GetField(t.fieldName, false, true).Assign(t.obj);
                            do pos++; while (code.Length > pos && char.IsWhiteSpace(code[pos]));
                            continue;
                        }
                        else if (stack.Peek().state == ParseState.Name && code[pos] == '}')
                        {
                            var t = stack.Pop();
                            if (reviewer != null)
                            {
                                revargs.data[0] = t.fieldName;
                                revargs.data[1] = t.obj;
                                t.obj = reviewer.Invoke(revargs);
                                if (t.obj.ValueType <= JSObjectType.Undefined)
                                    t.obj = null;
                            }
                            if (t.obj != null)
                                stack.Peek().obj.GetField(t.fieldName, false, true).Assign(t.obj);
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
            }
            return stack.Peek().obj.GetField("");
        }

        public static string stringify(JSObject obj)
        {
            return stringify(obj.GetField("0", true, false), null, null);
        }

        public static string stringify(JSObject obj, Function replacer)
        {
            return stringify(obj, replacer, null);
        }

        public static string stringify(JSObject obj, Function replacer, string space)
        {
            if (obj.ValueType < JSObjectType.Object)
            {
                if (obj.ValueType == JSObjectType.String)
                    return "\"" + obj.Value + '"';
                return obj.Value.ToString();
            }
            StringBuilder res = new StringBuilder("{");
            var args = new BaseTypes.Array(2);
            args.data[0] = "";
            foreach (var f in obj.fields)
            {
                if ((f.Value.ValueType < JSObjectType.Undefined) && ((f.Value.attributes & ObjectAttributes.DontEnum) == 0))
                    continue;
                var value = f.Value;
                if (replacer != null)
                {
                    args.data[0].oValue = f.Key;
                    args.data[1] = f.Value;
                    var t = replacer.Invoke(args);
                    if (t.ValueType <= JSObjectType.Undefined || (t.ValueType >= JSObjectType.Object && t.oValue == null))
                        continue;
                    value = t;
                }
                string strval = stringify(value, replacer, space);
                res.Append('"').Append(f.Key).Append("\": ").Append(strval).Append(space);
            }
            return res.Append("}").ToString();
        }
    }
}
