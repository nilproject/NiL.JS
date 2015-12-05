using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    /*
     * Ты входишь в самый тёмный переулок всего проекта. Ходят слухи о страшных делах, происходящих в этом месте.
     * Изредка отсюда доносятся людские стоны, полные боли и отчаяния. Наберись терпения и мужества, ибо те ужасы,
     * что ты можешь тут увидеть, испытают твой нервы.
     * Если желание твоё посетить сие место всё ещё живо и не угасло... и да хранит тебя Б-г. 
     */

#if !PORTABLE
    [Serializable]
#endif
    internal enum OperationTypeGroups
    {
        None = 0x0,
        Assign = 0x10,
        Choice = 0x20,
        LOr = 0x30,
        LAnd = 0x40,
        Or = 0x50,
        Xor = 0x60,
        And = 0x70,
        Logic1 = 0x80,
        Logic2 = 0x90,
        Bit = 0xa0,
        Arithmetic0 = 0xb0,
        Arithmetic1 = 0xc0,
        Unary0 = 0xd0,
        Unary1 = 0xe0,
        Special = 0xF0
    }

#if !PORTABLE
    [Serializable]
#endif
    internal enum OperationType
    {
        None = OperationTypeGroups.None + 0,
        Assignment = OperationTypeGroups.Assign + 0,
        Ternary = OperationTypeGroups.Choice + 0,

        LogicalOr = OperationTypeGroups.LOr,
        LogicalAnd = OperationTypeGroups.LAnd,
        Or = OperationTypeGroups.Or,
        Xor = OperationTypeGroups.Xor,
        And = OperationTypeGroups.And,

        Equal = OperationTypeGroups.Logic1 + 0,
        NotEqual = OperationTypeGroups.Logic1 + 1,
        StrictEqual = OperationTypeGroups.Logic1 + 2,
        StrictNotEqual = OperationTypeGroups.Logic1 + 3,

        InstanceOf = OperationTypeGroups.Logic2 + 0,
        In = OperationTypeGroups.Logic2 + 1,
        More = OperationTypeGroups.Logic2 + 2,
        Less = OperationTypeGroups.Logic2 + 3,
        MoreOrEqual = OperationTypeGroups.Logic2 + 4,
        LessOrEqual = OperationTypeGroups.Logic2 + 5,

        SignedShiftLeft = OperationTypeGroups.Bit + 0,
        SignedShiftRight = OperationTypeGroups.Bit + 1,
        UnsignedShiftRight = OperationTypeGroups.Bit + 2,

        Addition = OperationTypeGroups.Arithmetic0 + 0,
        Substract = OperationTypeGroups.Arithmetic0 + 1,
        Multiply = OperationTypeGroups.Arithmetic1 + 0,
        Module = OperationTypeGroups.Arithmetic1 + 1,
        Division = OperationTypeGroups.Arithmetic1 + 2,

        Negative = OperationTypeGroups.Unary0 + 0,
        Positive = OperationTypeGroups.Unary0 + 1,
        LogicalNot = OperationTypeGroups.Unary0 + 2,
        Not = OperationTypeGroups.Unary0 + 3,
        TypeOf = OperationTypeGroups.Unary0 + 4,
        Delete = OperationTypeGroups.Unary0 + 5,

        Incriment = OperationTypeGroups.Unary1 + 0,
        Decriment = OperationTypeGroups.Unary1 + 1,

        Call = OperationTypeGroups.Special + 0,
        New = OperationTypeGroups.Special + 2,
        Yield = OperationTypeGroups.Special + 4
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class ExpressionTree : Expression
    {
        private OperationType _operationType;

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal OperationType Type
        {
            get
            {
                return _operationType;
            }
        }

        private Expression getFastImpl()
        {
            switch (_operationType)
            {
                case OperationType.Multiply:
                    {
                        return new MultiplicationOperator(first, second);
                    }
                case OperationType.None:
                    {
                        return new CommaOperator(first, second);
                    }
                case OperationType.Assignment:
                    {
                        return new AssignmentOperator(first, second);
                    }
                case OperationType.Less:
                    {
                        return new LessOperator(first, second);
                    }
                case OperationType.Incriment:
                    {
                        return new IncrementOperator(first ?? second, first == null ? IncrimentType.Postincriment : IncrimentType.Preincriment);
                    }
                case OperationType.Call:
                    {
                        throw new InvalidOperationException("Call instance mast be created immediatly.");
                    }
                case OperationType.Decriment:
                    {
                        return new DecrementOperator(first ?? second, first == null ? DecrimentType.Postdecriment : DecrimentType.Postdecriment);
                    }
                case OperationType.LessOrEqual:
                    {
                        return new LessOrEqualOperator(first, second);
                    }
                case OperationType.Addition:
                    {
                        return new AdditionOperator(first, second);
                    }
                case OperationType.StrictNotEqual:
                    {
                        return new StrictNotEqualOperator(first, second);
                    }
                case OperationType.More:
                    {
                        return new MoreOperator(first, second);
                    }
                case OperationType.MoreOrEqual:
                    {
                        return new MoreOrEqualOperator(first, second);
                    }
                case OperationType.Division:
                    {
                        return new DivisionOperator(first, second);
                    }
                case OperationType.Equal:
                    {
                        return new EqualOperator(first, second);
                    }
                case OperationType.Substract:
                    {
                        return new SubstractOperator(first, second);
                    }
                case OperationType.StrictEqual:
                    {
                        return new StrictEqualOperator(first, second);
                    }
                case OperationType.LogicalOr:
                    {
                        return new LogicalDisjunctionOperator(first, second);
                    }
                case OperationType.LogicalAnd:
                    {
                        return new LogicalConjunctionOperator(first, second);
                    }
                case OperationType.NotEqual:
                    {
                        return new NotEqualOperator(first, second);
                    }
                case OperationType.UnsignedShiftRight:
                    {
                        return new UnsignedShiftRightOperator(first, second);
                    }
                case OperationType.SignedShiftLeft:
                    {
                        return new SignedShiftLeftOperator(first, second);
                    }
                case OperationType.SignedShiftRight:
                    {
                        return new SignedShiftRightOperator(first, second);
                    }
                case OperationType.Module:
                    {
                        return new ModuloOperator(first, second);
                    }
                case OperationType.LogicalNot:
                    {
                        return new LogicalNegationOperator(first);
                    }
                case OperationType.Not:
                    {
                        return new BitwiseNegationOperator(first);
                    }
                case OperationType.Xor:
                    {
                        return new BitwiseExclusiveDisjunctionOperator(first, second);
                    }
                case OperationType.Or:
                    {
                        return new BitwiseDisjunctionOperator(first, second);
                    }
                case OperationType.And:
                    {
                        return new BitwiseConjunctionOperator(first, second);
                    }
                case OperationType.Ternary:
                    {
                        while ((second is ExpressionTree)
                            && (second as ExpressionTree)._operationType == OperationType.None
                            && (second as ExpressionTree).second == null)
                            second = (second as ExpressionTree).first;
                        return new ConditionalOperator(first, (Expression[])second.Evaluate(null).oValue);
                    }
                case OperationType.TypeOf:
                    {
                        return new TypeOfOperator(first);
                    }
                case OperationType.New:
                    {
                        throw new InvalidOperationException("New instance mast be created immediatly.");
                    }
                case OperationType.Delete:
                    {
                        return new DeleteOperator(first);
                    }
                case OperationType.InstanceOf:
                    {
                        return new InstanceOfOperator(first, second);
                    }
                case OperationType.In:
                    {
                        return new InOperator(first, second);
                    }
                default:
                    throw new ArgumentException("invalid operation type");
            }
        }

        private static Expression deicstra(ExpressionTree statement)
        {
            if (statement == null)
                return null;
            ExpressionTree cur = statement.second as ExpressionTree;
            if (cur == null)
                return statement;
            Stack<Expression> stats = new Stack<Expression>();
            Stack<Expression> types = new Stack<Expression>();
            types.Push(statement);
            stats.Push(statement.first);
            while (cur != null)
            {
                stats.Push(cur.first);
                for (; types.Count > 0;)
                {
                    var topType = (int)(types.Peek() as ExpressionTree)._operationType;
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._operationType & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._operationType & (int)OperationTypeGroups.Special))
                            && (((int)cur._operationType & (int)OperationTypeGroups.Special) > (int)OperationTypeGroups.Choice)))
                    {
                        var stat = types.Pop() as ExpressionTree;
                        stat.second = stats.Pop();
                        stat.first = stats.Pop();
                        stat.Position = (stat.first ?? stat).Position;
                        stat.Length = (stat.second ?? stat.first ?? stat).Length + (stat.second ?? stat.first ?? stat).Position - stat.Position;
                        stats.Push(stat);
                    }
                    else
                        break;
                }
                types.Push(cur);
                if (!(cur.second is ExpressionTree))
                    stats.Push(cur.second);
                cur = cur.second as ExpressionTree;
            }
            while (stats.Count > 1)
            {
                var stat = types.Pop() as Expression;
                stat.second = stats.Pop();
                stat.first = stats.Pop();
                stat.Position = (stat.first ?? stat).Position;
                stat.Length = (stat.second ?? stat.first ?? stat).Length + (stat.second ?? stat.first ?? stat).Position - stat.Position;
                stats.Push(stat);
            }
            return stats.Peek();
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, false, true, false, true, false, false);
        }

        public static Expression Parse(ParseInfo state, ref int index, bool forUnaryOperator)
        {
            return Parse(state, ref index, forUnaryOperator, true, false, true, false, false);
        }

        internal static Expression Parse(ParseInfo state, ref int index, bool forUnary = false, bool processComma = true, bool forNew = false, bool root = true, bool forTernary = false, bool forEnumeration = false)
        {
            int i = index;
            int position;
            OperationType type = OperationType.None;
            Expression first = null;
            Expression second = null;
            int s = i;

            if (Parser.Validate(state.Code, "this", ref i)
                || Parser.Validate(state.Code, "super", ref i)
                || Parser.Validate(state.Code, "new.target", ref i))
            {
                var name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                switch (name)
                {
                    case "super":
                        {
                            /*
                             * Это ключевое слово. Не переменная.
                             * Оно может быть использовано только для получения свойств
                             * или вызова конструктора. При том, вызов конструктора допускается 
                             * только внутри конструктора класса-потомка.
                             */

                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;

                            if ((state.CodeContext & CodeContext.InClassDefenition) == 0
                                || (state.Code[i] != '.' // получение свойства
                                    && state.Code[i] != '['
                                    && (state.Code[i] != '(' || (state.CodeContext & CodeContext.InClassConstructor) == 0))) // вызов конструктора
                                ExceptionsHelper.ThrowSyntaxError("super keyword unexpected in this coontext", state.Code, i);

                            first = new GetSuper();
                            break;
                        }
                    case "new.target":
                        {
                            first = new GetNewTarget();
                            break;
                        }
                    case "this":
                        {
                            first = new GetThis();
                            break;
                        }
                    default:
                        {
                            first = new GetVariableExpression(name, state.lexicalScopeLevel);
                            break;
                        }
                }
            }
            else
            {
                var oldCodeContext = state.CodeContext;
                state.CodeContext |= CodeContext.InExpression;
                try
                {
                    if (forTernary)
                    {
                        position = i;
                        var threads = new[]
                            {
                                Parse(state, ref i, false, true, false, true, false, false),
                                null
                            };

                        if (state.Code[i] != ':')
                            ExceptionsHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);
                        do
                            i++;
                        while (Tools.IsWhiteSpace(state.Code[i]));
                        first = new ConstantDefinition(new JSValue() { valueType = JSValueType.Object, oValue = threads }) { Position = position };
                        threads[1] = Parse(state, ref i, false, false, false, true, false, forEnumeration);
                        first.Length = i - first.Position;
                    }
                    else
                        first = (Expression)Parser.Parse(state, ref i, (CodeFragmentType)2, false);
                }
                finally
                {
                    state.CodeContext = oldCodeContext;
                }
            }

            if (first == null)
            {
                if (Parser.ValidateName(state.Code, ref i, state.strict))
                {
                    var name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                    if (name == "undefined")
                    {
                        first = new ConstantDefinition(JSValue.undefined);
                    }
                    else
                    {
                        first = new GetVariableExpression(name, state.lexicalScopeLevel);
                    }
                }
                else if (Parser.ValidateValue(state.Code, ref i))
                {
                    string value = state.Code.Substring(s, i - s);
                    if ((value[0] == '\'') || (value[0] == '"'))
                    {
                        value = Tools.Unescape(value.Substring(1, value.Length - 2), state.strict);
                        if (state.stringConstants.ContainsKey(value))
                            first = new ConstantDefinition(state.stringConstants[value]) { Position = index, Length = i - s };
                        else
                            first = new ConstantDefinition(state.stringConstants[value] = value) { Position = index, Length = i - s };
                    }
                    else
                    {
                        bool b = false;
                        if (value == "null")
                            first = new ConstantDefinition(JSValue.@null) { Position = s, Length = i - s };
                        else if (bool.TryParse(value, out b))
                            first = new ConstantDefinition(b ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False) { Position = index, Length = i - s };
                        else
                        {
                            int n = 0;
                            double d = 0;
                            if (Tools.ParseNumber(state.Code, ref s, out d, 0, Tools.ParseNumberOptions.Default | (state.strict ? Tools.ParseNumberOptions.RaiseIfOctal : Tools.ParseNumberOptions.None)))
                            {
                                if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                {
                                    if (state.intConstants.ContainsKey(n))
                                        first = new ConstantDefinition(state.intConstants[n]) { Position = index, Length = i - index };
                                    else
                                        first = new ConstantDefinition(state.intConstants[n] = n) { Position = index, Length = i - index };
                                }
                                else
                                {
                                    if (state.doubleConstants.ContainsKey(d))
                                        first = new ConstantDefinition(state.doubleConstants[d]) { Position = index, Length = i - index };
                                    else
                                        first = new ConstantDefinition(state.doubleConstants[d] = d) { Position = index, Length = i - index };
                                }
                            }
                            else if (Parser.ValidateRegex(state.Code, ref s, true))
                                throw new ApplicationException("This case was moved");
                            else
                                throw new ArgumentException("Unable to process value (" + value + ")");
                        }
                    }
                }
                else if ((state.Code[i] == '!')
                    || (state.Code[i] == '~')
                    || (state.Code[i] == '+')
                    || (state.Code[i] == '-')
                    || Parser.Validate(state.Code, "delete", i)
                    || Parser.Validate(state.Code, "typeof", i)
                    || Parser.Validate(state.Code, "void", i))
                {
                    switch (state.Code[i])
                    {
                        case '+':
                            {
                                i++;
                                if (state.Code[i] == '+')
                                {
                                    do
                                        i++;
                                    while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]));
                                    if (i >= state.Code.Length)
                                        ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));

                                    first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);

                                    if (((first as GetPropertyOperator) as object ?? (first as GetVariableExpression)) == null)
                                    {
                                        ExceptionsHelper.ThrowSyntaxError("Invalid prefix operation. ", state.Code, i);
                                    }

                                    if (state.strict
                                        && (first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                    {
                                        ExceptionsHelper.ThrowSyntaxError("Can not incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode.", state.Code, i);
                                    }

                                    first = new IncrementOperator(first, IncrimentType.Preincriment);
                                }
                                else
                                {
                                    while (Tools.IsWhiteSpace(state.Code[i]))
                                        i++;
                                    var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                    first = new ToNumberOperator(f);
                                }
                                break;
                            }
                        case '-':
                            {
                                i++;
                                if (state.Code[i] == '-')
                                {
                                    do
                                        i++;
                                    while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]));
                                    if (i >= state.Code.Length)
                                        ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));

                                    first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);

                                    if (((first as GetPropertyOperator) as object ?? (first as GetVariableExpression)) == null)
                                    {
                                        ExceptionsHelper.ThrowSyntaxError("Invalid prefix operation.", state.Code, i);
                                    }

                                    if (state.strict
                                        && (first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                        ExceptionsHelper.Throw(new SyntaxError("Can not decriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));

                                    first = new DecrementOperator(first, DecrimentType.Predecriment);
                                }
                                else
                                {
                                    while (Tools.IsWhiteSpace(state.Code[i]))
                                        i++;
                                    var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                    first = new NegationOperator(f);
                                }
                                break;
                            }
                        case '!':
                            {
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                first = new LogicalNegationOperator((Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration));
                                if (first == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                break;
                            }
                        case '~':
                            {
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                if (first == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                first = new BitwiseNegationOperator(first);
                                break;
                            }
                        case 't':
                            {
                                i += 5;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                first = (Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration);
                                if (first == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                first = new TypeOfOperator(first);
                                break;
                            }
                        case 'v':
                            {
                                i += 3;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));

                                first = new CommaOperator(
                                    (Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration),
                                    new ConstantDefinition(JSValue.undefined));

                                if (first == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                break;
                            }
                        case 'd':
                            {
                                i += 5;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                first = (Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration);
                                if (first == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                first = new Expressions.DeleteOperator(first);
                                break;
                            }
                    }
                }
                else if (state.Code[i] == '(')
                {
                    while (state.Code[i] != ')')
                    {
                        do
                            i++;
                        while (Tools.IsWhiteSpace(state.Code[i]));
                        var temp = (Expression)ExpressionTree.Parse(state, ref i, false, false);
                        if (first == null)
                            first = temp;
                        else
                            first = new CommaOperator(first, temp);
                        while (Tools.IsWhiteSpace(state.Code[i]))
                            i++;
                        if (state.Code[i] != ')' && state.Code[i] != ',')
                            ExceptionsHelper.Throw((new SyntaxError("Expected \")\"")));
                    }
                    i++;
                    if (((state.CodeContext & CodeContext.InExpression) != 0 && first is FunctionDefinition)
                        || (forNew && first is CallOperator))
                    {
                        first = new Expressions.CommaOperator(first, null);
                    }
                }
                else
                {
                    if (forEnumeration)
                        return null;
                }
            }

            if (first == null || first is EmptyExpression)
                ExceptionsHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);

            first.Position = index;
            first.Length = i - index;

            bool canAsign = !forUnary; // на случай f() = x
            bool assign = false; // на случай операторов 'x='
            bool binary = false;
            bool repeat; // лёгкая замена goto. Тот самый случай, когда он уместен.
            int rollbackPos;
            do
            {
                repeat = false;
                while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]) && !Tools.IsLineTerminator(state.Code[i]))
                    i++;
                if (state.Code.Length <= i)
                    break;
                rollbackPos = i;
                while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code.Length <= i)
                {
                    i = rollbackPos;
                    break;
                }
                switch (state.Code[i])
                {
                    case '\v':
                    case '\n':
                    case '\r':
                    case ';':
                    case ')':
                    case ']':
                    case '}':
                    case ':':
                        {
                            binary = false;
                            break;
                        }
                    case '!':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '=')
                            {
                                i++;
                                if (state.Code[i + 1] == '=')
                                {
                                    i++;
                                    binary = true;
                                    type = OperationType.StrictNotEqual;
                                }
                                else
                                {
                                    binary = true;
                                    type = OperationType.NotEqual;
                                }
                            }
                            else
                                throw new ArgumentException("Invalid operator '!'");
                            break;
                        }
                    case ',':
                        {
                            if (forUnary || !processComma)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            type = OperationType.None;
                            binary = true;
                            repeat = false;
                            break;
                        }
                    case '?':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            type = OperationType.Ternary;
                            binary = true;
                            repeat = false;
                            break;
                        }
                    case '=':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '=')
                            {
                                i++;
                                if (state.Code[i + 1] == '=')
                                {
                                    i++;
                                    type = OperationType.StrictEqual;
                                }
                                else
                                    type = OperationType.Equal;
                            }
                            else
                                type = OperationType.Assignment;
                            binary = true;
                            break;
                        }
                    case '+':
                        {

                            if (state.Code[i + 1] == '+')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict)
                                {
                                    if ((first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                        ExceptionsHelper.ThrowSyntaxError("Cannot incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode.", state.Code, i);
                                }
                                first = new Expressions.IncrementOperator(first, Expressions.IncrimentType.Postincriment) { Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Incriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
                            }
                            else if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            else
                            {
                                binary = true;
                                type = OperationType.Addition;
                                if (state.Code[i + 1] == '=')
                                {
                                    assign = true;
                                    i++;
                                }
                            }
                            break;
                        }
                    case '-':
                        {

                            if (state.Code[i + 1] == '-')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict)
                                {
                                    if ((first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                        ExceptionsHelper.Throw(new SyntaxError("Can not decriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                }
                                first = new Expressions.DecrementOperator(first, Expressions.DecrimentType.Postdecriment) { Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Decriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
                            }
                            else if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            else
                            {
                                binary = true;
                                type = OperationType.Substract;
                                if (state.Code[i + 1] == '=')
                                {
                                    assign = true;
                                    i++;
                                }
                            }
                            break;
                        }
                    case '*':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
                            type = OperationType.Multiply;
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '&':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '&')
                            {
                                i++;
                                binary = true;
                                assign = false;
                                type = OperationType.LogicalAnd;
                                break;
                            }
                            else
                            {
                                binary = true;
                                assign = false;
                                type = OperationType.And;
                                if (state.Code[i + 1] == '=')
                                {
                                    assign = true;
                                    i++;
                                }
                                break;
                            }
                        }
                    case '|':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '|')
                            {
                                i++;
                                binary = true;
                                assign = false;
                                type = OperationType.LogicalOr;
                                break;
                            }
                            else
                            {
                                binary = true;
                                assign = false;
                                type = OperationType.Or;
                                if (state.Code[i + 1] == '=')
                                {
                                    assign = true;
                                    i++;
                                }
                                break;
                            }
                        }
                    case '^':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
                            type = OperationType.Xor;
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '/':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            state.Code = Tools.RemoveComments(state.SourceCode, i + 1);
                            binary = true;
                            type = OperationType.Division;
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '%':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                break;
                            }
                            binary = true;
                            type = OperationType.Module;
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '<':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
                            if (state.Code[i + 1] == '<')
                            {
                                i++;
                                type = OperationType.SignedShiftLeft;
                            }
                            else
                            {
                                type = OperationType.Less;
                                if (state.Code[i + 1] == '=')
                                {
                                    type = OperationType.LessOrEqual;
                                    i++;
                                }
                            }
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '>':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
                            if (state.Code[i + 1] == '>')
                            {
                                i++;
                                if (state.Code[i + 1] == '>')
                                {
                                    type = OperationType.UnsignedShiftRight;
                                    i++;
                                }
                                else
                                {
                                    type = OperationType.SignedShiftRight;
                                }
                            }
                            else
                            {
                                type = OperationType.More;
                                if (state.Code[i + 1] == '=')
                                {
                                    type = OperationType.MoreOrEqual;
                                    i++;
                                }
                            }
                            if (state.Code[i + 1] == '=')
                            {
                                assign = true;
                                i++;
                            }
                            break;
                        }
                    case '.':
                        {
                            i++;
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            s = i;
                            if (!Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                                ExceptionsHelper.Throw(new SyntaxError(string.Format(Strings.InvalidPropertyName, CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            string name = state.Code.Substring(s, i - s);
                            JSValue jsname = null;
                            if (!state.stringConstants.TryGetValue(name, out jsname))
                                state.stringConstants[name] = jsname = name;
                            first = new GetPropertyOperator(first, new ConstantDefinition(name)
                            {
                                Position = s,
                                Length = i - s
                            })
                            {
                                Position = first.Position,
                                Length = i - first.Position
                            };
                            repeat = true;
                            canAsign = true;
                            break;
                        }
                    case '[':
                        {
                            Expression mname = null;
                            do
                                i++;
                            while (Tools.IsWhiteSpace(state.Code[i]));
                            int startPos = i;
                            mname = (Expression)ExpressionTree.Parse(state, ref i, false, true, false, true, false, false);
                            //if (forEnumeration) // why?! (o_O)
                            //    return null;
                            if (mname == null)
                                ExceptionsHelper.Throw((new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            if (state.Code[i] != ']')
                                ExceptionsHelper.Throw((new SyntaxError("Expected \"]\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            first = new GetPropertyOperator(first, mname) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;

                            if (state.message != null)
                            {
                                startPos = 0;
                                var cname = mname as ConstantDefinition;
                                if (cname != null
                                    && cname.value.valueType == JSValueType.String
                                    && Parser.ValidateName(cname.value.oValue.ToString(), ref startPos, false)
                                    && startPos == cname.value.oValue.ToString().Length)
                                    state.message(MessageLevel.Recomendation, CodeCoordinates.FromTextPosition(state.Code, mname.Position, mname.Length), "[\"" + cname.value.oValue + "\"] is better written in dot notation.");
                            }
                            break;
                        }
                    case '(':
                        {
                            var args = new List<Expression>();
                            i++;
                            int startPos = i;
                            bool withSpread = false;
                            for (;;)
                            {
                                while (Tools.IsWhiteSpace(state.Code[i]))
                                    i++;
                                if (state.Code[i] == ')')
                                    break;
                                else if (state.Code[i] == ',')
                                {
                                    if (args.Count == 0)
                                        ExceptionsHelper.ThrowSyntaxError("Empty argument of function call", state.Code, i);
                                    do
                                        i++;
                                    while (Tools.IsWhiteSpace(state.Code[i]));
                                }
                                if (i + 1 == state.Code.Length)
                                    ExceptionsHelper.ThrowSyntaxError("Unexpected end of source", state.Code, i);
                                var spread = Parser.Validate(state.Code, "...", ref i);
                                args.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                                if (spread)
                                {
                                    args[args.Count - 1] = new SpreadOperator(args[args.Count - 1]);
                                    withSpread = true;
                                }
                                if (args[args.Count - 1] == null)
                                    ExceptionsHelper.ThrowSyntaxError("Expected \")\"", state.Code, startPos);
                            }
                            first = new CallOperator(first, args.ToArray())
                            {
                                Position = first.Position,
                                Length = i - first.Position + 1,
                                withSpread = withSpread
                            };
                            i++;
                            repeat = !forNew;
                            canAsign = false;
                            binary = false;
                            break;
                        }
                    case 'i':
                        {
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (Parser.Validate(state.Code, "instanceof", ref i))
                            {
                                type = OperationType.InstanceOf;
                                binary = true;
                                break;
                            }
                            else if (Parser.Validate(state.Code, "in", ref i))
                            {
                                if (forEnumeration)
                                {
                                    i = rollbackPos;
                                    goto case ';';
                                }
                                type = OperationType.In;
                                binary = true;
                                break;
                            }
                            goto default;
                        }
                    default:
                        {
                            if (Tools.IsLineTerminator(state.Code[i]))
                                goto case '\n';
                            if (i != rollbackPos)
                            {
                                i = rollbackPos;
                                goto case '\n';
                            }
                            if (state.Code[i] == 'o' && state.Code[i + 1] == 'f')
                            {
                                i = rollbackPos;
                                goto case ';';
                            }
                            ExceptionsHelper.ThrowSyntaxError("Unexpected token '" + state.Code[i] + "'", state.Code, i);
                            break;
                        }
                }
            } while (repeat);
            if (state.strict
                && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
            {
                if (assign || type == OperationType.Assignment)
                    ExceptionsHelper.ThrowSyntaxError("Assignment to eval or arguments is not allowed in strict mode", state.Code, i);
                //if (type == OperationType.Incriment || type == OperationType.Decriment)
                //    ExceptionsHelper.Throw(new SyntaxError("Can not " + type.ToString().ToLower() + " \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
            }
            if ((!canAsign) && ((type == OperationType.Assignment) || (assign)))
                ExceptionsHelper.ThrowSyntaxError("Invalid left-hand side in assignment", state.Code, i);
            if (binary && !forUnary)
            {
                do
                    i++;
                while (state.Code.Length > i && Tools.IsWhiteSpace(state.Code[i]));
                if (state.Code.Length > i)
                    second = (Expression)ExpressionTree.Parse(state, ref i, false, processComma, false, false, type == OperationType.Ternary, forEnumeration);
                else
                    ExceptionsHelper.ThrowSyntaxError("Expected second operand", state.Code, i);
            }
            Expression res = null;
            if (first == second && first == null)
                return null;
            if (assign)
            {
                root = false; // блокируем вызов дейкстры
                second = deicstra(second as ExpressionTree);
                var opassigncache = new AssignmentOperatorCache(first);
                if (second is ExpressionTree
                    && (second as ExpressionTree)._operationType == OperationType.None)
                {
                    second.first = new AssignmentOperator(opassigncache, new ExpressionTree()
                    {
                        first = opassigncache,
                        second = second.first,
                        _operationType = type,
                        Position = index,
                        Length = i - index
                    })
                    {
                        Position = index,
                        Length = i - index
                    };
                    res = second;
                }
                else
                {
                    res = new AssignmentOperator(opassigncache, new ExpressionTree()
                    {
                        first = opassigncache,
                        second = second,
                        _operationType = type,
                        Position = index,
                        Length = i - index
                    })
                    {
                        Position = index,
                        Length = i - index
                    };
                }
            }
            else
            {
                if (!root || type != OperationType.None || second != null)
                {
                    if (forUnary && (type == OperationType.None) && (first is ExpressionTree))
                        res = first as Expression;
                    else
                        res = new ExpressionTree() { first = first, second = second, _operationType = type, Position = index, Length = i - index };
                }
                else
                    res = first;
            }
            if (root)
                res = deicstra(res as ExpressionTree) ?? res;
            index = i;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            throw new InvalidOperationException();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _this = getFastImpl();
            _this.Position = Position;
            _this.Length = Length;
            return true;
        }
    }
}