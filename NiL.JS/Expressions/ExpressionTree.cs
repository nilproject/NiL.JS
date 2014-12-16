using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    internal enum OperationTypeGroups : int
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

    [Serializable]
    internal enum OperationType : int
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

    [Serializable]
    internal sealed class ExpressionTree : Expression
    {
        private Expression fastImpl;

        private OperationType _type;
        internal OperationType Type
        {
            get
            {
                return _type;
            }
            private set
            {
                fastImpl = null;
                switch (value)
                {
                    case OperationType.Multiply:
                        {
                            fastImpl = new Expressions.Mul(first, second);
                            break;
                        }
                    case OperationType.None:
                        {
                            fastImpl = new Expressions.None(first, second);
                            break;
                        }
                    case OperationType.Assign:
                        {
                            fastImpl = new Expressions.Assign(first, second);
                            break;
                        }
                    case OperationType.Less:
                        {
                            fastImpl = new Expressions.Less(first, second);
                            break;
                        }
                    case OperationType.Incriment:
                        {
                            fastImpl = new Expressions.Incriment(first ?? second, first == null ? Expressions.Incriment.Type.Postincriment : Expressions.Incriment.Type.Preincriment);
                            break;
                        }
                    case OperationType.Call:
                        {
                            throw new InvalidOperationException("Call instance mast be created immediatly.");
                            //fastImpl = new Expressions.Call(first, second);
                            //break;
                        }
                    case OperationType.Decriment:
                        {
                            fastImpl = new Expressions.Decriment(first ?? second, first == null ? Expressions.Decriment.Type.Postdecriment : Expressions.Decriment.Type.Postdecriment);
                            break;
                        }
                    case OperationType.LessOrEqual:
                        {
                            fastImpl = new Expressions.LessOrEqual(first, second);
                            break;
                        }
                    case OperationType.Addition:
                        {
                            fastImpl = new Expressions.Addition(first, second);
                            break;
                        }
                    case OperationType.StrictNotEqual:
                        {
                            fastImpl = new Expressions.StrictNotEqual(first, second);
                            break;
                        }
                    case OperationType.More:
                        {
                            fastImpl = new Expressions.More(first, second);
                            break;
                        }
                    case OperationType.MoreOrEqual:
                        {
                            fastImpl = new Expressions.MoreOrEqual(first, second);
                            break;
                        }
                    case OperationType.Division:
                        {
                            fastImpl = new Expressions.Division(first, second);
                            break;
                        }
                    case OperationType.Equal:
                        {
                            fastImpl = new Expressions.Equal(first, second);
                            break;
                        }
                    case OperationType.Substract:
                        {
                            fastImpl = new Expressions.Substract(first, second);
                            break;
                        }
                    case OperationType.StrictEqual:
                        {
                            fastImpl = new Expressions.StrictEqual(first, second);
                            break;
                        }
                    case OperationType.LogicalOr:
                        {
                            fastImpl = new Expressions.LogicalOr(first, second);
                            break;
                        }
                    case OperationType.LogicalAnd:
                        {
                            fastImpl = new Expressions.LogicalAnd(first, second);
                            break;
                        }
                    case OperationType.NotEqual:
                        {
                            fastImpl = new Expressions.NotEqual(first, second);
                            break;
                        }
                    case OperationType.UnsignedShiftRight:
                        {
                            fastImpl = new Expressions.UnsignedShiftRight(first, second);
                            break;
                        }
                    case OperationType.SignedShiftLeft:
                        {
                            fastImpl = new Expressions.SignedShiftLeft(first, second);
                            break;
                        }
                    case OperationType.SignedShiftRight:
                        {
                            fastImpl = new Expressions.SignedShiftRight(first, second);
                            break;
                        }
                    case OperationType.Module:
                        {
                            fastImpl = new Expressions.Mod(first, second);
                            break;
                        }
                    case OperationType.LogicalNot:
                        {
                            fastImpl = new Expressions.LogicalNot(first);
                            break;
                        }
                    case OperationType.Not:
                        {
                            fastImpl = new Expressions.Not(first);
                            break;
                        }
                    case OperationType.Xor:
                        {
                            fastImpl = new Expressions.Xor(first, second);
                            break;
                        }
                    case OperationType.Or:
                        {
                            fastImpl = new Expressions.Or(first, second);
                            break;
                        }
                    case OperationType.And:
                        {
                            fastImpl = new Expressions.And(first, second);
                            break;
                        }
                    case OperationType.Ternary:
                        {
                            while ((second is ExpressionTree)
                                && (second as ExpressionTree)._type == OperationType.None
                                && (second as ExpressionTree).second == null)
                                second = (second as ExpressionTree).first;
                            fastImpl = new Expressions.Ternary(first, (Expression[])second.Evaluate(null).oValue);
                            break;
                        }
                    case OperationType.TypeOf:
                        {
                            fastImpl = new Expressions.TypeOf(first);
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
                            fastImpl = new Expressions.Delete(first);
                            break;
                        }
                    case OperationType.InstanceOf:
                        {
                            fastImpl = new Expressions.InstanceOf(first, second);
                            break;
                        }
                    case OperationType.In:
                        {
                            fastImpl = new Expressions.In(first, second);
                            break;
                        }
                    case OperationType.Yield:
                        {
                            fastImpl = new Expressions.Yield(first);
                            break;
                        }
                    default:
                        throw new ArgumentException("invalid operation type");
                }
                _type = value;
            }
        }

        public ExpressionTree()
        {
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

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, false, false, true, false, false);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, bool processComma)
        {
            return Parse(state, ref index, processComma, false, false, true, false, false);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary)
        {
            return Parse(state, ref index, processComma, forUnary, false, true, false, false);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary, bool forNew, bool root, bool forTernary, bool forEnumeration)
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
                        (Expression)ExpressionTree.Parse(state, ref i, true, false, false, false, false, forEnumeration).Statement,
                        null
                    };
                if (state.Code[i] != ':')
                    throw new ArgumentException("Invalid char in ternary operator");
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                first = new Constant(new JSObject() { valueType = JSObjectType.Object, oValue = threads }) { Position = position };
                threads[1] = (Expression)ExpressionTree.Parse(state, ref i, false, false, false, false, false, forEnumeration).Statement;
                first.Length = i - first.Position;
            }
            else if (Parser.ValidateName(state.Code, ref i, state.strict.Peek()) || Parser.Validate(state.Code, "this", ref i))
            {
                var name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (name == "undefined")
                    first = new Constant(JSObject.undefined) { Position = index, Length = i - index };
                else
                    first = new GetVariableExpression(name, state.functionsDepth) { Position = index, Length = i - index, functionDepth = state.functionsDepth };
            }
            else if (Parser.ValidateValue(state.Code, ref i))
            {
                string value = state.Code.Substring(s, i - s);
                if ((value[0] == '\'') || (value[0] == '"'))
                    first = new Constant(Tools.Unescape(value.Substring(1, value.Length - 2), state.strict.Peek())) { Position = index, Length = i - s };
                else
                {
                    bool b = false;
                    if (value == "null")
                        first = new Constant(JSObject.Null) { Position = s, Length = i - s };
                    else if (bool.TryParse(value, out b))
                        first = new Constant(b) { Position = index, Length = i - s };
                    else
                    {
                        int n = 0;
                        double d = 0;
                        if (Tools.ParseNumber(state.Code, ref s, out d, 0, Tools.ParseNumberOptions.Default | (state.strict.Peek() ? Tools.ParseNumberOptions.RaiseIfOctal : Tools.ParseNumberOptions.None)))
                        {
                            if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                first = new Constant(n) { Position = index, Length = i - index };
                            else
                                first = new Constant(d) { Position = index, Length = i - index };
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
                            }
                        }
                        else
                            throw new ArgumentException("Invalid process value (" + value + ")");
                    }
                }
            }
            else if ((state.Code[i] == '!')
                || (state.Code[i] == '~')
                || (state.Code[i] == '+')
                || (state.Code[i] == '-')
                || Parser.Validate(state.Code, "new", i)
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
                                    throw new JSException(new SyntaxError("Unexpected end of source."));
                                first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement;
                                if (((first as GetMemberExpression) as object ?? (first as GetVariableExpression)) == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                first = new Expressions.Incriment(first, Expressions.Incriment.Type.Preincriment);
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement;
                                first = new Expressions.ToNumber(f) { Position = index, Length = i - index };
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
                                    throw new JSException(new SyntaxError("Unexpected end of source."));
                                first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement;
                                if (((first as GetMemberExpression) as object ?? (first as GetVariableExpression)) == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                first = new Expressions.Decriment(first, Expressions.Decriment.Type.Predecriment) { Position = index, Length = i - index };
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement;
                                first = new Expressions.Neg(f) { Position = index, Length = i - index };
                            }
                            break;
                        }
                    case '!':
                        {
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = new Expressions.LogicalNot((Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case '~':
                        {
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, true, true, false, true, false, forEnumeration).Statement;
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.Not(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, false, true, false, true, false, forEnumeration).Statement;
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.TypeOf(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 'v':
                        {
                            i += 3;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = new Expressions.None((Expression)Parse(state, ref i, false, true, false, true, false, forEnumeration).Statement, new Constant(JSObject.undefined)) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, false, true, true, true, false, forEnumeration).Statement;
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            if (first is Call)
                                first = new New((first as Expression).FirstOperand, (first as Call).Arguments) { Position = index, Length = i - index };
                            else
                            {
                                if (state.message != null)
                                    state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, index, 0), "Missing brackets in a constructor invocation.");
                                first = new Expressions.New(first, new Expression[0]) { Position = index, Length = i - index };
                            }
                            break;
                        }
                    case 'd':
                        {
                            i += 5;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, false, true, false, true, false, forEnumeration).Statement;
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.Delete(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 'y':
                        {
                            if (!state.AllowYield.Peek())
                                throw new JSException(new SyntaxError("Invalid use of yield operator"));
                            i += 4;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = (Expression)Parse(state, ref i, false, true, false, true, false, forEnumeration).Statement;
                            if (first == null)
                            {
                                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.Yield(first) { Position = index, Length = i - index };
                            break;
                        }
                    default:
                        throw new NotImplementedException("Unary operator " + state.Code[i]);
                }
            }
            else if (state.Code[i] == '(')
            {
                while (state.Code[i] != ')')
                {
                    do i++; while (char.IsWhiteSpace(state.Code[i]));
                    var temp = (Expression)ExpressionTree.Parse(state, ref i, false).Statement;
                    if (first == null)
                        first = temp;
                    else
                        first = new None(first, temp);
                    while (char.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ')' && state.Code[i] != ',')
                        throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\"")));
                }
                i++;
                if ((state.InExpression > 0 && first is FunctionExpression)
                    || (forNew && first is Call))
                    first = new Expressions.None(first, null) { Position = index, Length = i - index };
            }
            else
            {
                if (forEnumeration)
                    return default(ParseResult);
                first = (Expression)Parser.Parse(state, ref i, 2);
            }
            if (first is EmptyStatement)
                throw new JSException((new Core.BaseTypes.SyntaxError("Invalid operator argument at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            bool canAsign = true && !forUnary; // на случай f() = x
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
                            //do
                            //    i++;
                            //while (char.IsWhiteSpace(state.Code[i]));
                            //position = i;
                            //var threads = new CodeNode[]
                            //    {
                            //        ExpressionStatement.Parse(state, ref i, true, false, false, true, false).Statement,
                            //        null
                            //    };
                            //if (state.Code[i] != ':')
                            //    throw new ArgumentException("Invalid char in ternary operator");
                            //do
                            //    i++;
                            //while (char.IsWhiteSpace(state.Code[i]));
                            //second = new Constant(new JSObject() { valueType = JSObjectType.Object, oValue = threads }) { Position = position };
                            //threads[1] = ExpressionStatement.Parse(state, ref i, false, false, false, true, false).Statement;
                            //second.Length = i - second.Position;
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
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                }
                                first = new Expressions.Incriment(first, Expressions.Incriment.Type.Postincriment) { Position = first.Position, Length = i + 2 - first.Position };
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
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableExpression)
                                        && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableExpression).Name + "\" in strict mode."));
                                }
                                first = new Expressions.Decriment(first, Expressions.Decriment.Type.Postdecriment) { Position = first.Position, Length = i + 2 - first.Position };
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
                            if (!Parser.ValidateName(state.Code, ref i, false, true, state.strict.Peek()))
                                throw new ArgumentException("code (" + i + ")");
                            string name = state.Code.Substring(s, i - s);
                            first = new GetMemberExpression(first, new Constant(name)
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
                            do i++; while (char.IsWhiteSpace(state.Code[i]));
                            int startPos = i;
                            mname = (Expression)ExpressionTree.Parse(state, ref i, true, false, false, true, false, false).Statement;
                            if (forEnumeration)
                                return new ParseResult();
                            if (mname == null)
                                throw new JSException((new Core.BaseTypes.SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            while (char.IsWhiteSpace(state.Code[i])) i++;
                            if (state.Code[i] != ']')
                                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \"]\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));                            
                            first = new GetMemberExpression(first, mname) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;

                            if (state.message != null)
                            {
                                startPos = 0;
                                var cname = mname as Constant;
                                if (cname != null
                                    && cname.value.valueType == JSObjectType.String
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
                                        throw new JSException(new SyntaxError("Empty argument of function"));
                                    do
                                        i++;
                                    while (char.IsWhiteSpace(state.Code[i]));
                                }
                                if (i + 1 == state.Code.Length)
                                    throw new JSException((new Core.BaseTypes.SyntaxError("Unexpected end of line")));
                                args.Add((Expression)ExpressionTree.Parse(state, ref i, false).Statement);
                                if (args[args.Count - 1] == null)
                                    throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            }
                            first = new Call(first, args.ToArray())
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
                            throw new JSException((new Core.BaseTypes.SyntaxError("Invalid operator '" + state.Code[i] + "' at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        }
                }
            } while (repeat);
            if (state.strict.Peek()
                && (first is GetVariableExpression) && ((first as GetVariableExpression).Name == "arguments" || (first as GetVariableExpression).Name == "eval"))
            {
                if (assign || type == OperationType.Assign)
                    throw new JSException((new SyntaxError("Assignment to eval or arguments is not allowed in strict mode")));
                //if (type == OperationType.Incriment || type == OperationType.Decriment)
                //    throw new JSException(new SyntaxError("Can not " + type.ToString().ToLower() + " \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
            }
            if ((!canAsign) && ((type == OperationType.Assign) || (assign)))
                throw new InvalidOperationException("invalid left-hand side in assignment");
            if (binary && !forUnary)
            {
                do
                    i++;
                while (state.Code.Length > i && char.IsWhiteSpace(state.Code[i]));
                if (state.Code.Length > i)
                    second = (Expression)ExpressionTree.Parse(state, ref i, processComma, false, false, false, type == OperationType.Ternary, forEnumeration).Statement;
            }
            CodeNode res = null;
            if (first == second && first == null)
                return new ParseResult();
            if (assign)
            {
                var opassigncache = new OpAssignCache(first);
                res = new ExpressionTree()
                {
                    first = opassigncache,
                    second = new None(deicstra(new ExpressionTree()
                    {
                        first = opassigncache,
                        second = second is ExpressionTree ? (second as ExpressionTree)._type == OperationType.None ? second : new None(deicstra(second as ExpressionTree), null) : second,
                        _type = type,
                        Position = index,
                        Length = i - index
                    }), null),
                    _type = OperationType.Assign,
                    Position = index,
                    Length = i - index
                };
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
            return new ParseResult()
            {
                Statement = res,
                IsParsed = true
            };
        }

        internal override JSObject Evaluate(Context context)
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message)
        {
            Type = Type;
            _this = fastImpl;
            fastImpl.Position = Position;
            fastImpl.Length = Length;
            return true;
        }
    }
}