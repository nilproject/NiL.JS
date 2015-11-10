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
    public sealed class ObjectDefinition : Expression
    {
        private string[] fields;
        private CodeNode[] values;

        public CodeNode[] Initializators { get { return values; } }
        public string[] Fields { get { return fields; } }

        public override bool ContextIndependent
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

        private ObjectDefinition(Dictionary<string, CodeNode> fields)
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
                if ((Parser.Validate(state.Code, "get ", ref i) || Parser.Validate(state.Code, "set ", ref i)) && state.Code[i] != ':')
                {
                    i = s;
                    var mode = state.Code[i] == 's' ? FunctionType.Setter : FunctionType.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = propertyAccessor.name;
                    if (!flds.ContainsKey(accessorName))
                    {
                        var propertyPair = new PropertyPairExpression
                        (
                            mode == FunctionType.Getter ? propertyAccessor : null,
                            mode == FunctionType.Setter ? propertyAccessor : null
                        );
                        flds.Add(accessorName, propertyPair);
                    }
                    else
                    {
                        var vle = flds[accessorName] as PropertyPairExpression;

                        if (vle == null)
                            ExceptionsHelper.ThrowSyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field", state.Code, s);

                        do
                        {
                            if (mode == FunctionType.Getter)
                            {
                                if (vle.Getter == null)
                                {
                                    vle.Getter = propertyAccessor;
                                    break;
                                }
                            }
                            else
                            {
                                if (vle.Setter == null)
                                {
                                    vle.Setter = propertyAccessor;
                                    break;
                                }
                            }

                            ExceptionsHelper.ThrowSyntaxError("Try to redefine " + mode.ToString().ToLowerInvariant() + " of " + propertyAccessor.Name, state.Code, s);
                        }
                        while (false);
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
                        if (state.strict ? (!(aei is ConstantDefinition) || (aei as ConstantDefinition).value != JSValue.undefined)
                                         : aei is PropertyPairExpression)
                            ExceptionsHelper.ThrowSyntaxError("Try to redefine field \"" + fieldName + "\"", state.Code, s, i - s);
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
            return new ObjectDefinition(flds)
                {
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = JSObject.CreateObject();
            if (fields.Length == 0)
                return res;
            res.fields = JSObject.getFieldsContainer();
            for (int i = 0; i < fields.Length; i++)
            {
                var val = values[i].Evaluate(context);
                val = val.CloneImpl(false);
                val.attributes = JSValueAttributesInternal.None;
                if (this.fields[i] == "__proto__")
                    res.__proto__ = val.oValue as JSObject;
                else
                    res.fields[this.fields[i]] = val;
            }
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            _codeContext = codeContext;

            for (int i = 0; i < values.Length; i++)
            {
                Parser.Build(ref values[i], 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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
                if ((values[i] is ConstantDefinition) && ((values[i] as ConstantDefinition).value.valueType == JSValueType.Property))
                {
                    var gs = (values[i] as ConstantDefinition).value.oValue as CodeNode[];
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