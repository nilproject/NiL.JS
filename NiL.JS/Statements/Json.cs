using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class Json : Statement, IOptimizable
    {
        private string[] fields;
        private Statement[] values;

        public static ParseResult Parse(ParsingState state, ref int index)
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
                if (Parser.Validate(code, "set", i))
                {
                    var setter = FunctionStatement.Parse(state, ref i, FunctionStatement.FunctionParseMode.Setter).Statement as FunctionStatement;
                    if (flds.IndexOf(setter.Name) == -1)
                    {
                        flds.Add(setter.Name);
                        var vle = new ImmidateValueStatement(new Statement[2] { setter, null });
                        vle.Value.ValueType = JSObjectType.Property;
                        vls.Add(vle);
                    }
                    else
                    {
                        var vle = vls[flds.IndexOf(setter.Name)];
                        if (((vle as ImmidateValueStatement).Value.oValue as Statement[])[0] != null)
                            throw new ArgumentException("Try to redefine setter for " + setter.Name);
                        ((vle as ImmidateValueStatement).Value.oValue as Statement[])[0] = setter;
                    }
                }
                else if (Parser.Validate(code, "get", i))
                {
                    var getter = FunctionStatement.Parse(state, ref i, FunctionStatement.FunctionParseMode.Getter).Statement as FunctionStatement;
                    if (flds.IndexOf(getter.Name) == -1)
                    {
                        flds.Add(getter.Name);
                        var vle = new ImmidateValueStatement(new Statement[2] { null, getter });
                        vle.Value.ValueType = JSObjectType.Property;
                        vls.Add(vle);
                    }
                    else
                    {
                        var vle = vls[flds.IndexOf(getter.Name)];
                        if (((vle as ImmidateValueStatement).Value.oValue as Statement[])[1] != null)
                            throw new ArgumentException("Try to redefine getter for " + getter.Name);
                        ((vle as ImmidateValueStatement).Value.oValue as Statement[])[1] = getter;
                    }
                }
                else
                {
                    if (Parser.ValidateName(code, ref i, true))
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
                                flds.Add(d.ToString());
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
                    try
                    {
                        vls.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                    }
                    catch(ArgumentException e)
                    {
                        return new ParseResult() { Message = e.Message };
                    }
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

        public override JSObject Invoke(Context context)
        {
            var res = new JSObject(true);
            res.ValueType = JSObjectType.Object;
            res.oValue = new object();
            res.prototype = new JSObject(false);
            res.prototype.Assign(NiL.JS.Core.BaseTypes.BaseObject.Prototype);
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Invoke(context);
                if (val.ValueType == JSObjectType.Property)
                {
                    var gs = val.oValue as Statement[];
                    val.oValue = new Function[] { gs[0] != null ? gs[0].Invoke(context) as Function : null, gs[1] != null ? gs[1].Invoke(context) as Function : null };
                }
                res.GetField(fields[i], false, true).Assign(val);
            }
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] is ImmidateValueStatement) && ((values[i] as ImmidateValueStatement).Value.ValueType == JSObjectType.Property))
                {
                    var gs = (values[i] as ImmidateValueStatement).Value.oValue as Statement[];
                    Parser.Optimize(ref gs[0], 1, vars);
                    Parser.Optimize(ref gs[1], 1, vars);
                }
                else
                    Parser.Optimize(ref values[i], 2, vars);
            }
            return false;
        }
    }
}