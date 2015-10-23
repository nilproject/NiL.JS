using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ObjectNotation : Expression
    {
        private string[] fields;
        private CodeNode[] values;

        public CodeNode[] Initializators { get { return values; } }
        public string[] Fields { get { return fields; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        private ObjectNotation(Dictionary<string, CodeNode> fields)
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

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            if (state.Code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new Dictionary<string, CodeNode>();
            int i = index;
            while (state.Code[i] != '}')
            {
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                int s = i;
                if (state.Code[i] == '}')
                    break;
                if (Parser.Validate(state.Code, "set ", ref i) && state.Code[i] != ':')
                {
                    i = s;
                    var setter = FunctionNotation.Parse(state, ref i, FunctionType.Set) as FunctionNotation;
                    if (!flds.ContainsKey(setter.Name))
                    {
                        var vle = new ConstantNotation(new JSValue() { valueType = JSValueType.Object, oValue = new CodeNode[2] { setter, null } });
                        vle.value.valueType = JSValueType.Property;
                        flds.Add(setter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[setter.Name];
                        if (!(vle is ConstantNotation)
                            || (vle as ConstantNotation).value.valueType != JSValueType.Property)
                            ExceptionsHelper.Throw((new SyntaxError("Try to define setter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        if (((vle as ConstantNotation).value.oValue as CodeNode[])[0] != null)
                            ExceptionsHelper.Throw((new SyntaxError("Try to redefine setter " + setter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        ((vle as ConstantNotation).value.oValue as CodeNode[])[0] = setter;
                    }
                }
                else if (Parser.Validate(state.Code, "get ", ref i) && state.Code[i] != ':')
                {
                    i = s;
                    var getter = FunctionNotation.Parse(state, ref i, FunctionType.Get) as FunctionNotation;
                    if (!flds.ContainsKey(getter.Name))
                    {
                        var vle = new ConstantNotation(new JSValue() { valueType = JSValueType.Object, oValue = new CodeNode[2] { null, getter } });
                        vle.value.valueType = JSValueType.Property;
                        flds.Add(getter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[getter.Name];
                        if (!(vle is ConstantNotation)
                            || (vle as ConstantNotation).value.valueType != JSValueType.Property)
                            ExceptionsHelper.Throw((new SyntaxError("Try to define getter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        if (((vle as ConstantNotation).value.oValue as CodeNode[])[1] != null)
                            ExceptionsHelper.Throw((new SyntaxError("Try to redefine getter " + getter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        ((vle as ConstantNotation).value.oValue as CodeNode[])[1] = getter;
                    }
                }
                else
                {
                    i = s;
                    var fieldName = "";
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        if (state.Code[s] == '-')
                            ExceptionsHelper.Throw(new SyntaxError("Invalid char \"-\" at " + CodeCoordinates.FromTextPosition(state.Code, s, 1)));
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseNumber(state.Code, ref n, out d))
                            fieldName = Tools.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.strict);
                        else if (flds.Count != 0)
                            ExceptionsHelper.Throw((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                        else
                            return null;
                    }
                    else
                        return null;
                    while (char.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ':')
                        return null;
                    do
                        i++;
                    while (char.IsWhiteSpace(state.Code[i]));
                    var initializator = ExpressionTree.Parse(state, ref i, false, false);
                    CodeNode aei = null;
                    if (flds.TryGetValue(fieldName, out aei))
                    {
                        if (((state.strict && (!(aei is ConstantNotation) || (aei as ConstantNotation).value != JSValue.undefined))
                            || (aei is ConstantNotation && ((aei as ConstantNotation).value.valueType == JSValueType.Property))))
                            ExceptionsHelper.Throw(new SyntaxError("Try to redefine field \"" + fieldName + "\" at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s)));
                        if (state.message != null)
                            state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, initializator.Position, 0), "Duplicate key \"" + fieldName + "\"");
                    }
                    flds[fieldName] = initializator;
                }
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                if ((state.Code[i] != ',') && (state.Code[i] != '}'))
                    return null;
            }
            i++;
            var pos = index;
            index = i;
            return new ObjectNotation(flds)
                {
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = JSObject.CreateObject(false);
            if (fields.Length == 0)
                return res;
            res.fields = JSObject.createFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Evaluate(context);
                if (val.valueType == JSValueType.Property)
                {
                    var gs = val.oValue as CodeNode[];
                    var prop = res.fields[fields[i]] = new JSValue();
                    prop.oValue = new PropertyPair(gs[1] != null ? gs[1].Evaluate(context) as Function : null, gs[0] != null ? gs[0].Evaluate(context) as Function : null);
                    prop.valueType = JSValueType.Property;
                }
                else
                {
                    val = val.CloneImpl();
                    val.attributes = JSValueAttributesInternal.None;
                    if (this.fields[i] == "__proto__")
                        res.__proto__ = val.oValue as JSObject;
                    else
                        res.fields[this.fields[i]] = val;
                }
            }
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            for (int i = 0; i < values.Length; i++)
            {
                if ((values[i] is ConstantNotation) && ((values[i] as ConstantNotation).value.valueType == JSValueType.Property))
                {
                    var gs = (values[i] as ConstantNotation).value.oValue as CodeNode[];
                    Parser.Build(ref gs[0], 1, variables, state | BuildState.InExpression, message, statistic, opts);
                    Parser.Build(ref gs[1], 1, variables, state | BuildState.InExpression, message, statistic, opts);
                }
                else
                    Parser.Build(ref values[i], 2, variables, state | BuildState.InExpression, message, statistic, opts);
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            for (var i = Initializators.Length; i-- > 0; )
            {
                var cn = Initializators[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                Initializators[i] = cn as Expression;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            return values;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "{ ";
            for (int i = 0; i < fields.Length; i++)
            {
                if ((values[i] is ConstantNotation) && ((values[i] as ConstantNotation).value.valueType == JSValueType.Property))
                {
                    var gs = (values[i] as ConstantNotation).value.oValue as CodeNode[];
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