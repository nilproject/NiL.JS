using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using NiL.JS.Statements;

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
        Assign = OperationTypeGroups.Assign + 0,
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
        private Expression _fastImpl;
        private OperationType _type;

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal OperationType Type
        {
            get
            {
                return _type;
            }
            private set
            {
                _fastImpl = null;
                switch (value)
                {
                    case OperationType.Multiply:
                        {
                            _fastImpl = new Expressions.MultiplicationOperator(first, second);
                            break;
                        }
                    case OperationType.None:
                        {
                            _fastImpl = new Expressions.CommaOperator(first, second);
                            break;
                        }
                    case OperationType.Assign:
                        {
                            _fastImpl = new Expressions.AssignmentOperator(first, second);
                            break;
                        }
                    case OperationType.Less:
                        {
                            _fastImpl = new Expressions.LessOperator(first, second);
                            break;
                        }
                    case OperationType.Incriment:
                        {
                            _fastImpl = new Expressions.IncrementOperator(first ?? second, first == null ? Expressions.IncrimentType.Postincriment : Expressions.IncrimentType.Preincriment);
                            break;
                        }
                    case OperationType.Call:
                        {
                            throw new InvalidOperationException("Call instance mast be created immediatly.");
                        }
                    case OperationType.Decriment:
                        {
                            _fastImpl = new Expressions.DecrementOperator(first ?? second, first == null ? Expressions.DecrimentType.Postdecriment : Expressions.DecrimentType.Postdecriment);
                            break;
                        }
                    case OperationType.LessOrEqual:
                        {
                            _fastImpl = new Expressions.LessOrEqualOperator(first, second);
                            break;
                        }
                    case OperationType.Addition:
                        {
                            _fastImpl = new Expressions.AdditionOperator(first, second);
                            break;
                        }
                    case OperationType.StrictNotEqual:
                        {
                            _fastImpl = new Expressions.StrictNotEqualOperator(first, second);
                            break;
                        }
                    case OperationType.More:
                        {
                            _fastImpl = new Expressions.MoreOperator(first, second);
                            break;
                        }
                    case OperationType.MoreOrEqual:
                        {
                            _fastImpl = new Expressions.MoreOrEqualOperator(first, second);
                            break;
                        }
                    case OperationType.Division:
                        {
                            _fastImpl = new Expressions.DivisionOperator(first, second);
                            break;
                        }
                    case OperationType.Equal:
                        {
                            _fastImpl = new Expressions.EqualOperator(first, second);
                            break;
                        }
                    case OperationType.Substract:
                        {
                            _fastImpl = new Expressions.SubstractOperator(first, second);
                            break;
                        }
                    case OperationType.StrictEqual:
                        {
                            _fastImpl = new Expressions.StrictEqualOperator(first, second);
                            break;
                        }
                    case OperationType.LogicalOr:
                        {
                            _fastImpl = new Expressions.LogicalDisjunctionOperator(first, second);
                            break;
                        }
                    case OperationType.LogicalAnd:
                        {
                            _fastImpl = new Expressions.LogicalConjunctionOperator(first, second);
                            break;
                        }
                    case OperationType.NotEqual:
                        {
                            _fastImpl = new Expressions.NotEqualOperator(first, second);
                            break;
                        }
                    case OperationType.UnsignedShiftRight:
                        {
                            _fastImpl = new Expressions.UnsignedShiftRightOperator(first, second);
                            break;
                        }
                    case OperationType.SignedShiftLeft:
                        {
                            _fastImpl = new Expressions.SignedShiftLeftOperator(first, second);
                            break;
                        }
                    case OperationType.SignedShiftRight:
                        {
                            _fastImpl = new Expressions.SignedShiftRightOperator(first, second);
                            break;
                        }
                    case OperationType.Module:
                        {
                            _fastImpl = new Expressions.ModuloOperator(first, second);
                            break;
                        }
                    case OperationType.LogicalNot:
                        {
                            _fastImpl = new Expressions.LogicalNegationOperator(first);
                            break;
                        }
                    case OperationType.Not:
                        {
                            _fastImpl = new Expressions.BitwiseNegationOperator(first);
                            break;
                        }
                    case OperationType.Xor:
                        {
                            _fastImpl = new Expressions.BitwiseExclusiveDisjunctionOperator(first, second);
                            break;
                        }
                    case OperationType.Or:
                        {
                            _fastImpl = new Expressions.BitwiseDisjunctionOperator(first, second);
                            break;
                        }
                    case OperationType.And:
                        {
                            _fastImpl = new Expressions.BitwiseConjunctionOperator(first, second);
                            break;
                        }
                    case OperationType.Ternary:
                        {
                            while ((second is ExpressionTree)
                                && (second as ExpressionTree)._type == OperationType.None
                                && (second as ExpressionTree).second == null)
                                second = (second as ExpressionTree).first;
                            _fastImpl = new Expressions.ConditionalOperator(first, (Expression[])second.Evaluate(null).oValue);
                            break;
                        }
                    case OperationType.TypeOf:
                        {
                            _fastImpl = new Expressions.TypeOfOperator(first);
                            break;
                        }
                    case OperationType.New:
                        {
                            throw new InvalidOperationException("New instance mast be created immediatly.");
                            //fastImpl = new Expressions.New(first, second);
                            //break;
                        }
                    case OperationType.Delete:
                        {
                            _fastImpl = new Expressions.DeleteOperator(first);
                            break;
                        }
                    case OperationType.InstanceOf:
                        {
                            _fastImpl = new Expressions.InstanceOfOperator(first, second);
                            break;
                        }
                    case OperationType.In:
                        {
                            _fastImpl = new Expressions.InOperator(first, second);
                            break;
                        }
#if !PORTABLE
                    case OperationType.Yield:
                        {
                            _fastImpl = new Expressions.YieldOperator(first);
                            break;
                        }
#endif
                    default:
                        throw new ArgumentException("invalid operation type");
                }
                _type = value;
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
                for (; types.Count > 0; )
                {
                    var topType = (int)(types.Peek() as ExpressionTree)._type;
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._type & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._type & (int)OperationTypeGroups.Special))
                            && (((int)cur._type & (int)OperationTypeGroups.Special) > (int)OperationTypeGroups.Choice)))
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

        public static CodeNode Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, false, true, false, true, false, false);
        }

        public static CodeNode Parse(ParsingState state, ref int index, bool forUnary)
        {
            return Parse(state, ref index, forUnary, true, false, true, false, false);
        }

        internal static CodeNode Parse(ParsingState state, ref int index, bool forUnary, bool processComma)
        {
            return Parse(state, ref index, forUnary, processComma, false, true, false, false);
        }

        internal static CodeNode Parse(ParsingState state, ref int index, bool forUnary, bool processComma, bool forNew, bool root, bool forTernary, bool forEnumeration)
        {
            int i = index;
            int position;
            OperationType type = OperationType.None;
            Expression first = null;
            Expression second = null;
            int s = i;
            state.InExpression++;
            if (forTernary)
            {
                position = i;
                var threads = new Expression[]
                    {
                        (Expression)ExpressionTree.Parse(state, ref i, false, true, false, true, false, false),
                        null
                    };
                if (state.Code[i] != ':')
                    ExceptionsHelper.Throw(new SyntaxError("Invalid char in ternary operator"));
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                first = new ConstantNotation(new JSValue() { valueType = JSValueType.Object, oValue = threads }) { Position = position };
                threads[1] = (Expression)ExpressionTree.Parse(state, ref i, false, false, false, true, false, forEnumeration);
                first.Length = i - first.Position;
            }
            else if (Parser.ValidateName(state.Code, ref i, state.strict) || Parser.Validate(state.Code, "this", ref i))
            {
                var name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                if (name == "undefined")
                    first = new ConstantNotation(JSValue.undefined) { Position = index, Length = i - index };
                else
                    first = new GetVariableExpression(name, state.functionsDepth) { Position = index, Length = i - index, defineDepth = state.functionsDepth };
            }
            else if (Parser.ValidateValue(state.Code, ref i))
            {
                string value = state.Code.Substring(s, i - s);
                if ((value[0] == '\'') || (value[0] == '"'))
                {
                    value = Tools.Unescape(value.Substring(1, value.Length - 2), state.strict);
                    if (state.stringConstants.ContainsKey(value))
                        first = new ConstantNotation(state.stringConstants[value]) { Position = index, Length = i - s };
                    else
                        first = new ConstantNotation(state.stringConstants[value] = value) { Position = index, Length = i - s };
                }
                else
                {
                    bool b = false;
                    if (value == "null")
                        first = new ConstantNotation(JSValue.Null) { Position = s, Length = i - s };
                    else if (bool.TryParse(value, out b))
                        first = new ConstantNotation(b ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False) { Position = index, Length = i - s };
                    else
                    {
                        int n = 0;
                        double d = 0;
                        if (Tools.ParseNumber(state.Code, ref s, out d, 0, Tools.ParseNumberOptions.Default | (state.strict ? Tools.ParseNumberOptions.RaiseIfOctal : Tools.ParseNumberOptions.None)))
                        {
                            if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                            {
                                if (state.intConstants.ContainsKey(n))
                                    first = new ConstantNotation(state.intConstants[n]) { Position = index, Length = i - index };
                                else
                                    first = new ConstantNotation(state.intConstants[n] = n) { Position = index, Length = i - index };
                            }
                            else
                            {
                                if (state.doubleConstants.ContainsKey(d))
                                    first = new ConstantNotation(state.doubleConstants[d]) { Position = index, Length = i - index };
                                else
                                    first = new ConstantNotation(state.doubleConstants[d] = d) { Position = index, Length = i - index };
                            }
                        }
                        else if (Parser.ValidateRegex(state.Code, ref s, true))
                        {
                            state.Code = Tools.RemoveComments(state.SourceCode, i);
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            try
                            {
                                first = new RegExpExpression(value.Substring(1, s - 2), flags); // объекты должны быть каждый раз разные
                            }
                            catch (Exception e)
                            {
                                first = new ExpressionWrapper(new ThrowStatement(e));
                                if (state.message != null)
                                    state.message(MessageLevel.Error, CodeCoordinates.FromTextPosition(state.Code, index, value.Length), string.Format(Strings.InvalidRegExp, value));
                            }
                        }
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
                || Parser.Validate(state.Code, "void", i)
                || Parser.Validate(state.Code, "yield", i)
                )
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
                                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                                if (i >= state.Code.Length)
                                    ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));
                                first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                if (((first as GetMemberOperator) as object ?? (first as GetVariableExpression)) == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict
                                    && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                    ExceptionsHelper.Throw(new SyntaxError("Can not incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                first = new Expressions.IncrementOperator(first, Expressions.IncrimentType.Preincriment) { Position = index, Length = i - index };
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                first = new Expressions.ToNumberOperator(f) { Position = index, Length = i - index };
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
                                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                                if (i >= state.Code.Length)
                                    ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));
                                first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                if (((first as GetMemberOperator) as object ?? (first as GetVariableExpression)) == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict
                                    && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                    ExceptionsHelper.Throw(new SyntaxError("Can not decriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                first = new Expressions.DecrementOperator(first, Expressions.DecrimentType.Predecriment) { Position = index, Length = i - index };
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                                first = new Expressions.NegationOperator(f) { Position = index, Length = i - index };
                            }
                            break;
                        }
                    case '!':
                        {
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = new Expressions.LogicalNegationOperator((Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration)) { Position = index, Length = i - index };
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
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration);
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.BitwiseNegationOperator(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration);
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.TypeOfOperator(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 'v':
                        {
                            i += 3;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = new Expressions.CommaOperator((Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration), new ConstantNotation(JSValue.undefined)) { Position = index, Length = i - index };
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
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, true, false, false, true, false, forEnumeration);
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.DeleteOperator(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 'y':
                        {
#if PORTABLE
                            throw new NotSupportedException("Do not supported in portable version");
#else
                            if (!state.AllowYield.Peek())
                                ExceptionsHelper.Throw(new SyntaxError("Invalid use of yield operator"));
                            i += 4;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, false, false, false, true, false, forEnumeration);
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.YieldOperator(first) { Position = index, Length = i - index };
                            break;
#endif
                        }
                    default:
                        ExceptionsHelper.ThrowUnknowToken(state.Code, i);
                        break;
                }
            }
            else if (state.Code[i] == '(')
            {
                while (state.Code[i] != ')')
                {
                    do
                        i++;
                    while (char.IsWhiteSpace(state.Code[i]));
                    var temp = (Expression)ExpressionTree.Parse(state, ref i, false, false);
                    if (first == null)
                        first = temp;
                    else
                        first = new CommaOperator(first, temp);
                    while (char.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ')' && state.Code[i] != ',')
                        ExceptionsHelper.Throw((new SyntaxError("Expected \")\"")));
                }
                i++;
                if ((state.InExpression > 0 && first is FunctionNotation)
                    || (forNew && first is CallOperator))
                    first = new Expressions.CommaOperator(first, null) { Position = index, Length = i - index };
            }
            else
            {
                if (forEnumeration)
                    return null;
                first = (Expression)Parser.Parse(state, ref i, (CodeFragmentType)2);
            }
            if (first is EmptyExpression)
                ExceptionsHelper.Throw((new SyntaxError("Invalid operator argument at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            bool canAsign = !forUnary; // на случай f() = x
            bool assign = false; // на случай операторов 'x='
            bool binary = false;
            bool repeat; // лёгкая замена goto. Тот самый случай, когда он уместен.
            int rollbackPos;
            do
            {
                repeat = false;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i]))
                    i++;
                if (state.Code.Length <= i)
                    break;
                rollbackPos = i;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
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
                                type = OperationType.Assign;
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
                                        ExceptionsHelper.Throw(new SyntaxError("Can not incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
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
                            binary = true;
                            i++;
                            while (char.IsWhiteSpace(state.Code[i]))
                                i++;
                            s = i;
                            if (!Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                                ExceptionsHelper.Throw(new SyntaxError(string.Format(Strings.InvalidPropertyName, CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            string name = state.Code.Substring(s, i - s);
                            JSValue jsname = null;
                            if (!state.stringConstants.TryGetValue(name, out jsname))
                                state.stringConstants[name] = jsname = name;
                            first = new GetMemberOperator(first, new ConstantNotation(name)
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
                            while (char.IsWhiteSpace(state.Code[i]));
                            int startPos = i;
                            mname = (Expression)ExpressionTree.Parse(state, ref i, false, true, false, true, false, false);
                            //if (forEnumeration) // why?! (o_O)
                            //    return null;
                            if (mname == null)
                                ExceptionsHelper.Throw((new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            while (char.IsWhiteSpace(state.Code[i]))
                                i++;
                            if (state.Code[i] != ']')
                                ExceptionsHelper.Throw((new SyntaxError("Expected \"]\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            first = new GetMemberOperator(first, mname) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;

                            if (state.message != null)
                            {
                                startPos = 0;
                                var cname = mname as ConstantNotation;
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
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                if (state.Code[i] == ')')
                                    break;
                                else if (state.Code[i] == ',')
                                {
                                    if (args.Count == 0)
                                        ExceptionsHelper.Throw(new SyntaxError("Empty argument of function"));
                                    do
                                        i++;
                                    while (char.IsWhiteSpace(state.Code[i]));
                                }
                                if (i + 1 == state.Code.Length)
                                    ExceptionsHelper.Throw(new SyntaxError("Unexpected end of line"));
                                args.Add((Expression)ExpressionTree.Parse(state, ref i, false, false));
                                if (args[args.Count - 1] == null)
                                    ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            }
                            first = new CallOperator(first, args.ToArray())
                            {
                                Position = first.Position,
                                Length = i - first.Position + 1
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
                            if (Tools.isLineTerminator(state.Code[i]))
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
                            ExceptionsHelper.Throw((new SyntaxError("Invalid operator '" + state.Code[i] + "' at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            break;
                        }
                }
            } while (repeat);
            if (state.strict
                && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
            {
                if (assign || type == OperationType.Assign)
                    ExceptionsHelper.Throw((new SyntaxError("Assignment to eval or arguments is not allowed in strict mode")));
                //if (type == OperationType.Incriment || type == OperationType.Decriment)
                //    ExceptionsHelper.Throw(new SyntaxError("Can not " + type.ToString().ToLower() + " \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
            }
            if ((!canAsign) && ((type == OperationType.Assign) || (assign)))
                ExceptionsHelper.Throw(new SyntaxError("invalid left-hand side in assignment"));
            if (binary && !forUnary)
            {
                do
                    i++;
                while (state.Code.Length > i && char.IsWhiteSpace(state.Code[i]));
                if (state.Code.Length > i)
                    second = (Expression)ExpressionTree.Parse(state, ref i, false, processComma, false, false, type == OperationType.Ternary, forEnumeration);
            }
            Expression res = null;
            if (first == second && first == null)
                return null;
            if (assign)
            {
                root = false; // блокируем вызов дейкстры
                second = deicstra(second as ExpressionTree);
                var opassigncache = new GetValueForAssignmentOperator(first);
                if (second is ExpressionTree
                    && (second as ExpressionTree)._type == OperationType.None)
                {
                    second.first = new AssignmentOperator(opassigncache, new ExpressionTree()
                    {
                        first = opassigncache,
                        second = second.first,
                        _type = type,
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
                        _type = type,
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
                        res = new ExpressionTree() { first = first, second = second, _type = type, Position = index, Length = i - index };
                }
                else
                    res = first;
            }
            if (root)
                res = deicstra(res as ExpressionTree) ?? res;
            index = i;
            state.InExpression--;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        protected override CodeNode[] getChildsImpl()
        {
            throw new InvalidOperationException();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Type = Type;
            _this = _fastImpl as T;
            _fastImpl.Position = Position;
            _fastImpl.Length = Length;
            return true;
        }
    }
}