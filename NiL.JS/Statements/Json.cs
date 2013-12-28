using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class Json : Statement
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
                    var setter = Function.Parse(state, ref i, Function.FunctionParseMode.Setter).Statement as Function;
                    if (flds.IndexOf(setter.Name) == -1)
                    {
                        flds.Add(setter.Name);
                        var vle = new ImmidateValueStatement(new Statement[2] { setter, null });
                        vle.Value.ValueType = ObjectValueType.Property;
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
                    var getter = Function.Parse(state, ref i, Function.FunctionParseMode.Getter).Statement as Function;
                    if (flds.IndexOf(getter.Name) == -1)
                    {
                        flds.Add(getter.Name);
                        var vle = new ImmidateValueStatement(new Statement[2] { null, getter });
                        vle.Value.ValueType = ObjectValueType.Property;
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
                        flds.Add(code.Substring(s, i - s));
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

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            var res = new JSObject(false);
            res.ValueType = ObjectValueType.Object;
            res.oValue = new object();
            res.prototype = NiL.JS.Core.BaseTypes.BaseObject.Prototype;
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Invoke(context);
                if (val.ValueType == ObjectValueType.Property)
                {
                    var gs = val.oValue as Statement[];
                    val.oValue = new IContextStatement[] { gs[0] != null ? gs[0].Implement(context) : null, gs[1] != null ? gs[1].Implement(context) : null };
                }
                res.GetField(fields[i]).Assign(val);
            }
            return res;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}