using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class Json : CodeNode
    {
        private string[] fields;
        private CodeNode[] values;

        public CodeNode[] Body { get { return values; } }
        public string[] Fields { get { return fields; } }

        private Json(Dictionary<string, CodeNode> fields)
        {
            this.fields = new string[fields.Count];
            this.values = new CodeNode[fields.Count];
            int i = 0;
            foreach (var f in fields)
            {
                this.fields[i] = f.Key;
                this.values[i++] = f.Value;
            }
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            if (code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new Dictionary<string, CodeNode>();
            int i = index;
            int pos = 0;
            while (code[i] != '}')
            {
                do
                    i++;
                while (char.IsWhiteSpace(code[i]));
                int s = i;
                if (code[i] == '}')
                    break;
                pos = i;
                if (Parser.Validate(code, "set ", ref i) && !Parser.isIdentificatorTerminator(code[i]))
                {
                    i = pos;
                    var setter = FunctionStatement.Parse(state, ref i, FunctionType.Set).Statement as FunctionStatement;
                    if (!flds.ContainsKey(setter.Name))
                    {
                        var vle = new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = new CodeNode[2] { setter, null } });
                        vle.value.valueType = JSObjectType.Property;
                        flds.Add(setter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[setter.Name];
                        if (!(vle is ImmidateValueStatement)
                            || (vle as ImmidateValueStatement).value.valueType != JSObjectType.Property)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to define setter for defined field at " + Tools.PositionToTextcord(code, pos))));
                        if (((vle as ImmidateValueStatement).value.oValue as CodeNode[])[0] != null)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to redefine setter " + setter.Name + " at " + Tools.PositionToTextcord(code, pos))));
                        ((vle as ImmidateValueStatement).value.oValue as CodeNode[])[0] = setter;
                    }
                }
                else if ((i = pos) >= 0 && Parser.Validate(code, "get ", ref i) && !Parser.isIdentificatorTerminator(code[i]))
                {
                    i = pos;
                    var getter = FunctionStatement.Parse(state, ref i, FunctionType.Get).Statement as FunctionStatement;
                    if (!flds.ContainsKey(getter.Name))
                    {
                        var vle = new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = new CodeNode[2] { null, getter } });
                        vle.value.valueType = JSObjectType.Property;
                        flds.Add(getter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[getter.Name];
                        if (!(vle is ImmidateValueStatement)
                            || (vle as ImmidateValueStatement).value.valueType != JSObjectType.Property)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to define getter for defined field at " + Tools.PositionToTextcord(code, pos))));
                        if (((vle as ImmidateValueStatement).value.oValue as CodeNode[])[1] != null)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to redefine getter " + getter.Name + " at " + Tools.PositionToTextcord(code, pos))));
                        ((vle as ImmidateValueStatement).value.oValue as CodeNode[])[1] = getter;
                    }
                }
                else
                {
                    i = pos;
                    var fieldName = "";
                    if (Parser.ValidateName(code, ref i, false, true, state.strict.Peek()))
                        fieldName = Tools.Unescape(code.Substring(s, i - s), state.strict.Peek());
                    else if (Parser.ValidateValue(code, ref i))
                    {
                        string value = code.Substring(s, i - s);
                        if ((value[0] == '\'') || (value[0] == '"'))
                            fieldName = Tools.Unescape(value.Substring(1, value.Length - 2), state.strict.Peek());
                        else
                        {
                            int n = 0;
                            double d = 0.0;
                            if (int.TryParse(value, out n))
                                fieldName = n < 16 ? Tools.NumString[n] : n.ToString(CultureInfo.InvariantCulture);
                            else if (double.TryParse(value, out d))
                                fieldName = Tools.DoubleToString(d);
                            else if (flds.Count != 0)
                                throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid field name at " + Tools.PositionToTextcord(code, pos))));
                            else
                                return new ParseResult();
                        }
                    }
                    else
                        return new ParseResult();
                    while (char.IsWhiteSpace(code[i]))
                        i++;
                    if (code[i] != ':')
                        return new ParseResult();
                    do
                        i++;
                    while (char.IsWhiteSpace(code[i]));
                    var initializator = ExpressionStatement.Parse(state, ref i, false).Statement;
                    CodeNode aei = null;
                    flds.TryGetValue(fieldName, out aei);
                    if (aei != null
                        && ((state.strict.Peek() && (!(aei is ImmidateValueStatement) || (aei as ImmidateValueStatement).value != JSObject.undefined))
                            || (aei is ImmidateValueStatement && ((aei as ImmidateValueStatement).value.valueType == JSObjectType.Property))))
                        throw new JSException(new SyntaxError("Try to redefine field \"" + fieldName + "\" at " + Tools.PositionToTextcord(code, pos)));
                    flds[fieldName] = initializator;
                }
                while (char.IsWhiteSpace(code[i]))
                    i++;
                if ((code[i] != ',') && (code[i] != '}'))
                    return new ParseResult();
            }
            i++;
            pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new Json(flds)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            var res = new JSObject(true);
            res.valueType = JSObjectType.Object;
            res.oValue = res;
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Invoke(context);
                if (val.valueType == JSObjectType.Property)
                {
                    var gs = val.oValue as CodeNode[];
                    var prop = res.GetMember(fields[i], true, true);
                    prop.oValue = new Function[] { gs[0] != null ? gs[0].Invoke(context) as Function : null, gs[1] != null ? gs[1].Invoke(context) as Function : null };
                    prop.valueType = JSObjectType.Property;
                }
                else
                {
                    val = val.CloneImpl();
                    val.attributes = JSObjectAttributesInternal.None;
                    res.fields[this.fields[i]] = val;
                }
            }
            return res;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] is ImmidateValueStatement) && ((values[i] as ImmidateValueStatement).value.valueType == JSObjectType.Property))
                {
                    var gs = (values[i] as ImmidateValueStatement).value.oValue as CodeNode[];
                    Parser.Optimize(ref gs[0], 1, fdepth, vars, strict);
                    Parser.Optimize(ref gs[1], 1, fdepth, vars, strict);
                }
                else
                    Parser.Optimize(ref values[i], 2, fdepth, vars, strict);
            }
            return false;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return values;
        }

        public override string ToString()
        {
            string res = "{ ";
            for (int i = 0; i < fields.Length; i++)
            {
                if ((values[i] is ImmidateValueStatement) && ((values[i] as ImmidateValueStatement).value.valueType == JSObjectType.Property))
                {
                    var gs = (values[i] as ImmidateValueStatement).value.oValue as CodeNode[];
                    res += gs[0];
                    if (gs[0] != null && gs[1] != null)
                        res += ", ";
                    res += gs[1];
                }
                else
                    res += "\"" + fields[i] + "\"" + " : " + values[i];
                if (i + 1 < fields.Length)
                    res += ", ";
            }
            return res + " }";
        }
    }
}