using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed class MemberDescriptor
    {
        internal string _name;
        internal Expression _value;
        internal bool _static;

        public string Name { get { return _name; } }
        public Expression Value { get { return _value; } }
        public bool Static { get { return _static; } }

        public MemberDescriptor(string name, Expression value, bool @static)
        {
            _name = name;
            _value = value;
            _static = @static;
        }

        public override string ToString()
        {
            if (_static)
                return "static " + _value;
            return _value.ToString();
        }
    }

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

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        private ReadOnlyCollection<MemberDescriptor> members;
        private Expression baseType;
        private FunctionDefinition _constructor;

        public ICollection<MemberDescriptor> Members { get { return members; } }
        public Expression BaseClassExpression { get { return baseType; } }
        public FunctionDefinition Constructor { get { return _constructor; } }
        public override bool Hoist
        {
            get { return false; }
        }
        protected internal override bool NeedDecompose
        {
            get
            {
                if (_constructor.NeedDecompose)
                    return true;

                for (var i = 0; i < members.Count; i++)
                {
                    if (members[i]._value.NeedDecompose)
                        return true;
                }

                return false;
            }
        }

        private ClassDefinition(string name, Expression baseType, ICollection<MemberDescriptor> fields, FunctionDefinition ctor)
        {
            this.name = name;
            this.baseType = baseType;
            this._constructor = ctor;
            this.members = new List<MemberDescriptor>(fields).AsReadOnly();
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
                if (Parser.ValidateName(code, ref i, true))
                    name = code.Substring(n, i - n);

                while (char.IsWhiteSpace(code[i]))
                    i++;
            }
            if (Parser.Validate(code, "extends ", ref i))
            {
                var n = i;
                if (!Parser.ValidateName(code, ref i, true) && !Parser.Validate(code, "null", ref i))
                    ExceptionsHelper.ThrowSyntaxError("Invalid base class name", state.Code, i);
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
            var flds = new Dictionary<string, MemberDescriptor>();
            var oldCodeContext = state.CodeContext;
            while (code[i] != '}')
            {
                do
                    i++;
                while (char.IsWhiteSpace(code[i]) || code[i] == ';');
                if (state.Code[i] == '}')
                    break;

                bool @static = Parser.Validate(state.Code, "static ", ref i);

                int s = i;
                if (Parser.Validate(state.Code, "get ", ref i) || Parser.Validate(state.Code, "set ", ref i))
                {
                    i = s;
                    var mode = state.Code[i] == 's' ? FunctionType.Setter : FunctionType.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = (@static ? "static " : "") + propertyAccessor.name;
                    if (!flds.ContainsKey(accessorName))
                    {
                        var propertyPair = new GsPropertyPairExpression
                        (
                            mode == FunctionType.Getter ? propertyAccessor : null,
                            mode == FunctionType.Setter ? propertyAccessor : null
                        );
                        flds.Add(accessorName, new MemberDescriptor(propertyAccessor.name, propertyPair, @static));
                    }
                    else
                    {
                        var vle = flds[accessorName].Value as GsPropertyPairExpression;

                        if (vle == null)
                            ExceptionsHelper.Throw((new SyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));

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
                    string fieldName = null;
                    if (state.Code[i] == '*')
                    {
                        do
                            i++;
                        while (char.IsWhiteSpace(code[i]));
                    }

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
                        ExceptionsHelper.Throw((new SyntaxError("Invalid member name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                    if (fieldName == "constructor")
                    {
                        if (@static)
                        {
                            ExceptionsHelper.ThrowSyntaxError(Strings.ConstructorCannotBeStatic, state.Code, s);
                        }
                        if (ctor != null)
                        {
                            ExceptionsHelper.ThrowSyntaxError("Trying to redefinition constructor", state.Code, s);
                        }

                        state.CodeContext |= CodeContext.InClassConstructor;
                    }
                    else if (@static)
                    {
                        fieldName = "static " + fieldName;
                        state.CodeContext |= CodeContext.InStaticMember;
                    }
                    if (flds.ContainsKey(fieldName))
                        ExceptionsHelper.Throw(new SyntaxError("Trying to redefinition member \"" + fieldName + "\" at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s)));

                    state.CodeContext |= CodeContext.InClassDefenition;
                    state.CodeContext &= ~CodeContext.InGenerator;

                    i = s;
                    var method = FunctionDefinition.Parse(state, ref i, FunctionType.Method) as FunctionDefinition;

                    if (fieldName == "constructor")
                    {
                        ctor = method;

                        if (baseType != null && !(baseType is ConstantDefinition))
                        {
                            do
                            {
                                if (method.body.localVariables != null)
                                {
                                    var lvi = 0;
                                    for (; lvi < method.body.localVariables.Length; lvi++)
                                    {
                                        if (method.body.localVariables[lvi].name == "super")
                                        {
                                            break;
                                        }
                                    }
                                    if (lvi < method.body.localVariables.Length)
                                        break;
                                }

                                ExceptionsHelper.ThrowSyntaxError("constructor must contain call constructor of superclass", state.Code, s);
                            }
                            while (false);
                        }
                    }
                    else
                    {
                        flds[fieldName] = new MemberDescriptor(method.name, method, @static);
                    }
                    if (method == null)
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
            return new ClassDefinition(name, baseType, flds.Values, ctor as FunctionDefinition);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref _constructor, depth, variables, codeContext | CodeContext.InClassDefenition | CodeContext.InClassConstructor, message, statistic, opts);
            Parser.Build(ref baseType, depth, variables, codeContext, message, statistic, opts);

            for (var i = 0; i < members.Count; i++)
            {
                Parser.Build
                (
                    ref members[i]._value,
                    depth,
                    variables,
                    codeContext | CodeContext.InClassDefenition | (members[i]._static ? CodeContext.InStaticMember : 0),
                    message,
                    statistic,
                    opts
                );
            }

            return base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        public override JSValue Evaluate(Context context)
        {
            var ctor = this._constructor.Evaluate(context) as Function;
            ctor.RequireNewKeywordLevel = RequireNewKeywordLevel.WithNewOnly;

            JSValue protoCtor = TypeProxy.GlobalPrototype;
            if (this.baseType != null)
            {
                protoCtor = baseType.Evaluate(context).oValue as JSObject;
                if (protoCtor == null)
                {
                    ctor.prototype.__proto__ = null;
                }
                else
                {
                    ctor.prototype.__proto__ = Tools.InvokeGetter(protoCtor.GetProperty("prototype"), protoCtor).oValue as JSObject;
                }
                ctor.__proto__ = protoCtor as JSObject;
            }

            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var value = member.Value.Evaluate(context);
                JSValue target = null;
                if (member.Static)
                {
                    target = ctor;
                }
                else
                {
                    target = ctor.prototype;
                }

                target.SetProperty(member.Name, value, true);
            }

            if ((_codeContext & CodeContext.InExpression) == 0)
                context.DefineVariable(name).Assign(ctor);
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
            for (var i = 0; i < members.Count; i++)
            {
                var t = members[i].ToString().Replace(Environment.NewLine, Environment.NewLine + "  ");
                result.Append(t);
            }
            result.Append(Environment.NewLine).Append("}");
            return result.ToString();
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            for (var i = 0; i < members.Count; i++)
            {
                members[i]._value.Decompose(ref members[i]._value, result); // results will be empty at each iterations
#if DEBUG
                if (result.Count != 0)
                    System.Diagnostics.Debug.Fail("Decompose: results not empty");
#endif
            }
        }
    }
}