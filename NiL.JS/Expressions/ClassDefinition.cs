using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ClassDefinition : EntityDefinition
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Function;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        private string[] fields;
        private CodeNode[] values;
        private Expression baseType;
        private FunctionDefinition _ctor;

        public CodeNode[] Initializators { get { return values; } }
        public string[] Fields { get { return fields; } }
        public Expression BaseClassExpression { get { return baseType; } }
        public FunctionDefinition Constructor { get { return _ctor; } }
        public override bool Hoist
        {
            get { return false; }
        }

        public ClassDefinition(string name, Expression baseType, Dictionary<string, CodeNode> fields, FunctionDefinition ctor)
        {
            this.name = name;
            this.baseType = baseType;
            this.fields = new string[fields.Count];
            this.values = new CodeNode[fields.Count];
            this._ctor = ctor;
            int i = 0;
            foreach (var f in fields)
            {
                this.fields[i] = f.Key;
                this.values[i++] = f.Value;
            }
        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "class", ref i))
                return null;
            while (char.IsWhiteSpace(code[i]))
                i++;
            string name = null;
            Expression baseType = null;
            if (!Parser.Validate(code, "extends ", i))
            {
                var n = i;
                if (!Parser.ValidateName(code, ref i, true))
                    ExceptionsHelper.Throw(new SyntaxError("Invalid class name"));
                name = code.Substring(n, i - n);
                do
                    i++;
                while (char.IsWhiteSpace(code[i]));
            }
            if (Parser.Validate(code, "extends ", ref i))
            {
                var n = i;
                if (!Parser.ValidateName(code, ref i, true) && !Parser.Validate(code, "null", ref i))
                    ExceptionsHelper.Throw(new SyntaxError("Invalid base class name"));
                var baseClassName = code.Substring(n, i - n);
                if (baseClassName == "null")
                    baseType = new ConstantDefinition(JSValue.Null) { Position = n, Length = 4 };
                else
                    baseType = new GetVariableExpression(baseClassName, state.functionsDepth);
                while (char.IsWhiteSpace(code[i]))
                    i++;
            }
            if (code[i] != '{')
                ExceptionsHelper.ThrowSyntaxError(Strings.UnexpectedToken, code, i);

            CodeNode ctor = null;
            var oldStrict = state.strict;
            state.strict = true;
            var flds = new Dictionary<string, CodeNode>();
            var oldCodeContext = state.CodeContext;
            state.CodeContext |= CodeContext.InClassDefenition;
            while (code[i] != '}')
            {
                do
                    i++;
                while (char.IsWhiteSpace(code[i]) || code[i] == ';');
                int s = i;
                if (state.Code[i] == '}')
                    break;
                if (Parser.Validate(state.Code, "set ", ref i) && state.Code[i] != ':')
                {
                    i = s;
                    var setter = FunctionDefinition.Parse(state, ref i, FunctionType.Set) as FunctionDefinition;
                    if (!flds.ContainsKey(setter.Name))
                    {
                        var vle = new ConstantDefinition(new JSValue() { valueType = JSValueType.Object, oValue = new CodeNode[2] { setter, null } });
                        vle.value.valueType = JSValueType.Property;
                        flds.Add(setter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[setter.Name];
                        if (!(vle is ConstantDefinition)
                            || (vle as ConstantDefinition).value.valueType != JSValueType.Property)
                            ExceptionsHelper.Throw((new SyntaxError("Try to define setter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        if (((vle as ConstantDefinition).value.oValue as CodeNode[])[0] != null)
                            ExceptionsHelper.Throw((new SyntaxError("Try to redefine setter " + setter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        ((vle as ConstantDefinition).value.oValue as CodeNode[])[0] = setter;
                    }
                }
                else if (Parser.Validate(state.Code, "get ", ref i) && state.Code[i] != ':')
                {
                    i = s;
                    var getter = FunctionDefinition.Parse(state, ref i, FunctionType.Get) as FunctionDefinition;
                    if (!flds.ContainsKey(getter.Name))
                    {
                        var vle = new ConstantDefinition(new JSValue() { valueType = JSValueType.Object, oValue = new CodeNode[2] { null, getter } });
                        vle.value.valueType = JSValueType.Property;
                        flds.Add(getter.Name, vle);
                    }
                    else
                    {
                        var vle = flds[getter.Name];
                        if (!(vle is ConstantDefinition)
                            || (vle as ConstantDefinition).value.valueType != JSValueType.Property)
                            ExceptionsHelper.Throw((new SyntaxError("Try to define getter for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        if (((vle as ConstantDefinition).value.oValue as CodeNode[])[1] != null)
                            ExceptionsHelper.Throw((new SyntaxError("Try to redefine getter " + getter.Name + " at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));
                        ((vle as ConstantDefinition).value.oValue as CodeNode[])[1] = getter;
                    }
                }
                else
                {
                    i = s;
                    string fieldName = null;
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseNumber(state.Code, ref n, out d))
                            fieldName = Tools.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.strict);
                    }
                    if (fieldName == null)
                        ExceptionsHelper.Throw((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                    if (flds.ContainsKey(fieldName))
                        ExceptionsHelper.Throw(new SyntaxError("Trying to redefinition field \"" + fieldName + "\" at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s)));
                    if (fieldName == "constructor")
                    {
                        if (ctor != null)
                            ExceptionsHelper.ThrowSyntaxError("Trying to redefinition constructor", state.Code, i);
                        state.CodeContext |= CodeContext.InClassConstructor;
                    }
                    i = s;
                    var initializator = FunctionDefinition.Parse(state, ref i, FunctionType.Method) as FunctionDefinition;
                    if (ctor != null)
                        state.CodeContext = oldCodeContext | CodeContext.InClassDefenition;
                    else
                        flds[fieldName] = initializator;
                    if (initializator == null)
                        ExceptionsHelper.Throw(new SyntaxError());
                }
            }
            if (ctor == null)
            {
                string ctorCode;
                int ctorIndex = 0;
                if (baseType != null && !(baseType is ConstantDefinition))
                    ctorCode = "constructor(...args) { super(...args); }";
                else
                    ctorCode = "constructor(...args) { }";
                ctor = FunctionDefinition.Parse(new ParsingState(ctorCode, ctorCode, null)
                {
                    strict = true,
                    CodeContext = CodeContext.InClassConstructor | CodeContext.InClassDefenition
                }, ref ctorIndex, FunctionType.Method);
            }
            state.CodeContext = oldCodeContext;
            state.strict = oldStrict;
            index = i + 1;
            return new ClassDefinition(name, baseType, flds, ctor as FunctionDefinition);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref _ctor, depth, variables, codeContext, message, statistic, opts);
            Parser.Build(ref baseType, depth, variables, codeContext, message, statistic, opts);

            return base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        public override JSValue Evaluate(Context context)
        {
            var proto = TypeProxy.GlobalPrototype;
            if (this.baseType != null)
                proto = baseType.Evaluate(context).GetMember("prototype") as JSObject;
            var ctor = this._ctor.Evaluate(context) as Function;
            context.DefineVariable(name).Assign(ctor);
            ctor.prototype.__proto__ = proto;

            for (var i = 0; i < fields.Length; i++)
            {
                ctor.prototype[fields[i]] = values[i].Evaluate(context);
            }

            return ctor;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("class ").Append(name);
            if (baseType != null)
                result.Append(" extends ").Append(baseType);
            result.Append(" {").Append(Environment.NewLine);
            for (var i = 0; i < fields.Length; i++)
            {
                var t = values[i].ToString().Replace(Environment.NewLine, Environment.NewLine + "  ");
                result.Append(t);
            }
            result.Append(Environment.NewLine).Append("}");
            return result.ToString();
        }
    }
}