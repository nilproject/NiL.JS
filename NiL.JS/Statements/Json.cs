using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class Json : Statement
    {
        private string[] fields;
        private Statement[] values;

        public Statement[] Body { get { return values; } }
        public string[] Fields { get { return fields; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            if (code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new List<string>();
            var vls = new List<Statement>();
            int i = index;
            while (code[i] != '}')
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                int s = i;
                if (code[i] == '}')
                    break;
                if (Parser.Validate(code, "set", i) && (Tools.isLineTerminator(code[i + 3]) || char.IsWhiteSpace(code[i + 3])))
                {
                    int pos = i;
                    var setter = FunctionStatement.Parse(state, ref i, FunctionStatement.FunctionParseMode.set).Statement as FunctionStatement;
                    if (flds.IndexOf(setter.name) == -1)
                    {
                        flds.Add(setter.name);
						var vle = new ImmidateValueStatement(new JSObject() { ValueType = JSObjectType.Object, oValue = new Statement[2] { setter, null } });
                        vle.value.ValueType = JSObjectType.Property;
                        vls.Add(vle);
                    }
                    else
                    {
                        var vle = vls[flds.IndexOf(setter.name)];
                        if (!(vle is ImmidateValueStatement))
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to define setter for defined field at " + Tools.PositionToTextcord(code, pos))));
                        if (((vle as ImmidateValueStatement).value.oValue as Statement[])[0] != null)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to redefine setter " + setter.name + " at " + Tools.PositionToTextcord(code, pos))));
                        ((vle as ImmidateValueStatement).value.oValue as Statement[])[0] = setter;
                    }
                }
                else if (Parser.Validate(code, "get", i) && (Tools.isLineTerminator(code[i + 3]) || char.IsWhiteSpace(code[i + 3])))
                {
                    int pos = i;
                    var getter = FunctionStatement.Parse(state, ref i, FunctionStatement.FunctionParseMode.get).Statement as FunctionStatement;
                    if (flds.IndexOf(getter.name) == -1)
                    {
                        flds.Add(getter.name);
						var vle = new ImmidateValueStatement(new JSObject() { ValueType = JSObjectType.Object, oValue = new Statement[2] { null, getter } });
                        vle.value.ValueType = JSObjectType.Property;
                        vls.Add(vle);
                    }
                    else
                    {
                        var vle = vls[flds.IndexOf(getter.name)];
                        if (!(vle is ImmidateValueStatement))
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to define getter for defined field at " + Tools.PositionToTextcord(code, pos))));
                        if (((vle as ImmidateValueStatement).value.oValue as Statement[])[1] != null)
                            throw new JSException(TypeProxy.Proxy(new SyntaxError("Try to redefine getter " + getter.name + " at " + Tools.PositionToTextcord(code, pos))));
                        ((vle as ImmidateValueStatement).value.oValue as Statement[])[1] = getter;
                    }
                }
                else
                {
                    if (Parser.ValidateName(code, ref i, true, state.strict.Peek()))
                        flds.Add(Tools.Unescape(code.Substring(s, i - s)));
                    else if (Parser.ValidateValue(code, ref i, true))
                    {
                        string value = code.Substring(s, i - s);
                        if ((value[0] == '\'') || (value[0] == '"'))
                            flds.Add(value.Substring(1, value.Length - 2));
                        else
                        {
                            int n = 0;
                            double d = 0.0;
                            if (int.TryParse(value, out n))
                                flds.Add(n.ToString());
                            else if (double.TryParse(value, out d))
                                flds.Add(Tools.DoubleToString(d));
                            else
                                return new ParseResult();
                        }
                    }
                    else
                        return new ParseResult();
                    while (char.IsWhiteSpace(code[i])) i++;
                    if (code[i] != ':')
                        return new ParseResult();
                    do i++; while (char.IsWhiteSpace(code[i]));
                    vls.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                }
                while (char.IsWhiteSpace(code[i])) i++;
                if ((code[i] != ',') && (code[i] != '}'))
                    return new ParseResult();
            }
            i++;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new Json()
                {
                    fields = flds.ToArray(),
                    values = vls.ToArray()
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            var res = new JSObject(true);
            res.ValueType = JSObjectType.Object;
            res.oValue = res;
            res.prototype = JSObject.GlobalPrototype.Clone() as JSObject;
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Invoke(context);
                if (val.ValueType == JSObjectType.Property)
                {
                    var gs = val.oValue as Statement[];
                    var prop = res.GetField(fields[i], false, true);
                    prop.assignCallback(prop);
                    prop.oValue = new Function[] { gs[0] != null ? gs[0].Invoke(context) as Function : null, gs[1] != null ? gs[1].Invoke(context) as Function : null };
                    prop.ValueType = JSObjectType.Property;
                }
                else
                    res.fields[this.fields[i]] = val.Clone() as JSObject;
            }
            return res;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] is ImmidateValueStatement) && ((values[i] as ImmidateValueStatement).value.ValueType == JSObjectType.Property))
                {
                    var gs = (values[i] as ImmidateValueStatement).value.oValue as Statement[];
                    Parser.Optimize(ref gs[0], 1, vars);
                    Parser.Optimize(ref gs[1], 1, vars);
                }
                else
                    Parser.Optimize(ref values[i], 2, vars);
            }
            return false;
        }

        public override string ToString()
        {
            string res = "{ ";
            for (int i = 0; i < fields.Length; i++)
            {
                if ((values[i] is ImmidateValueStatement) && ((values[i] as ImmidateValueStatement).value.ValueType == JSObjectType.Property))
                {
                    var gs = (values[i] as ImmidateValueStatement).value.oValue as Statement[];
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