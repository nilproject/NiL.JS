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

#if !(PORTABLE || NETCORE)
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

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal enum OperationType
    {
        None = OperationTypeGroups.None + 0,
        Assignment = OperationTypeGroups.Assign + 0,
        Conditional = OperationTypeGroups.Choice + 0,

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
        Modulo = OperationTypeGroups.Arithmetic1 + 1,
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

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ExpressionTree : Expression
    {
        private OperationType _operationKind;

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal OperationType Type
        {
            get
            {
                return _operationKind;
            }
        }

        private Expression getFastImpl()
        {
            switch (_operationKind)
            {
                case OperationType.Multiply:
                    {
                        return new Multiplication(_left, _right);
                    }
                case OperationType.None:
                    {
                        if (_right == null)
                            return _left;

                        return new Comma(_left, _right);
                    }
                case OperationType.Assignment:
                    {
                        return new Assignment(_left, _right);
                    }
                case OperationType.Less:
                    {
                        return new Less(_left, _right);
                    }
                case OperationType.Incriment:
                    {
                        return new Increment(_left ?? _right, _left == null ? IncrimentType.Postincriment : IncrimentType.Preincriment);
                    }
                case OperationType.Call:
                    {
                        throw new InvalidOperationException("Call instance mast be created immediatly.");
                    }
                case OperationType.Decriment:
                    {
                        return new Decrement(_left ?? _right, _left == null ? DecrimentType.Postdecriment : DecrimentType.Postdecriment);
                    }
                case OperationType.LessOrEqual:
                    {
                        return new LessOrEqual(_left, _right);
                    }
                case OperationType.Addition:
                    {
                        return new Addition(_left, _right);
                    }
                case OperationType.StrictNotEqual:
                    {
                        return new StrictNotEqual(_left, _right);
                    }
                case OperationType.More:
                    {
                        return new More(_left, _right);
                    }
                case OperationType.MoreOrEqual:
                    {
                        return new MoreOrEqual(_left, _right);
                    }
                case OperationType.Division:
                    {
                        return new Division(_left, _right);
                    }
                case OperationType.Equal:
                    {
                        return new Equal(_left, _right);
                    }
                case OperationType.Substract:
                    {
                        return new Substract(_left, _right);
                    }
                case OperationType.StrictEqual:
                    {
                        return new StrictEqual(_left, _right);
                    }
                case OperationType.LogicalOr:
                    {
                        return new LogicalDisjunction(_left, _right);
                    }
                case OperationType.LogicalAnd:
                    {
                        return new LogicalConjunction(_left, _right);
                    }
                case OperationType.NotEqual:
                    {
                        return new NotEqual(_left, _right);
                    }
                case OperationType.UnsignedShiftRight:
                    {
                        return new UnsignedShiftRight(_left, _right);
                    }
                case OperationType.SignedShiftLeft:
                    {
                        return new SignedShiftLeft(_left, _right);
                    }
                case OperationType.SignedShiftRight:
                    {
                        return new SignedShiftRight(_left, _right);
                    }
                case OperationType.Modulo:
                    {
                        return new Modulo(_left, _right);
                    }
                case OperationType.LogicalNot:
                    {
                        return new LogicalNegation(_left);
                    }
                case OperationType.Not:
                    {
                        return new BitwiseNegation(_left);
                    }
                case OperationType.Xor:
                    {
                        return new BitwiseExclusiveDisjunction(_left, _right);
                    }
                case OperationType.Or:
                    {
                        return new BitwiseDisjunction(_left, _right);
                    }
                case OperationType.And:
                    {
                        return new BitwiseConjunction(_left, _right);
                    }
                case OperationType.Conditional:
                    {
                        while ((_right is ExpressionTree)
                            && (_right as ExpressionTree)._operationKind == OperationType.None
                            && (_right as ExpressionTree)._right == null)
                            _right = (_right as ExpressionTree)._left;
                        return new Conditional(_left, (Expression[])_right.Evaluate(null)._oValue);
                    }
                case OperationType.TypeOf:
                    {
                        return new TypeOf(_left);
                    }
                case OperationType.New:
                    {
                        throw new InvalidOperationException("New instance mast be created immediatly.");
                    }
                case OperationType.Delete:
                    {
                        return new Delete(_left);
                    }
                case OperationType.InstanceOf:
                    {
                        return new InstanceOf(_left, _right);
                    }
                case OperationType.In:
                    {
                        return new In(_left, _right);
                    }
                default:
                    throw new ArgumentException("invalid operation type");
            }
        }

        private static Expression deicstra(ExpressionTree statement)
        {
            if (statement == null)
                return null;
            ExpressionTree cur = statement._right as ExpressionTree;
            if (cur == null)
                return statement;
            Stack<Expression> stats = new Stack<Expression>();
            Stack<Expression> types = new Stack<Expression>();
            types.Push(statement);
            stats.Push(statement._left);
            while (cur != null)
            {
                stats.Push(cur._left);
                for (; types.Count > 0;)
                {
                    var topType = (int)(types.Peek() as ExpressionTree)._operationKind;
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._operationKind & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._operationKind & (int)OperationTypeGroups.Special))
                            && (((int)cur._operationKind & (int)OperationTypeGroups.Special) > (int)OperationTypeGroups.Choice)))
                    {
                        var stat = types.Pop() as ExpressionTree;
                        stat._right = stats.Pop();
                        stat._left = stats.Pop();
                        stat.Position = (stat._left ?? stat).Position;
                        stat.Length = (stat._right ?? stat._left ?? stat).Length + (stat._right ?? stat._left ?? stat).Position - stat.Position;
                        stats.Push(stat);
                    }
                    else
                        break;
                }

                types.Push(cur);
                if (!(cur._right is ExpressionTree))
                    stats.Push(cur._right);
                cur = cur._right as ExpressionTree;
            }
            while (stats.Count > 1)
            {
                var stat = types.Pop() as Expression;
                stat._right = stats.Pop();
                stat._left = stats.Pop();
                stat.Position = (stat._left ?? stat).Position;
                stat.Length = (stat._right ?? stat._left ?? stat).Length + (stat._right ?? stat._left ?? stat).Position - stat.Position;
                stats.Push(stat);
            }
            return stats.Peek();
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, false, true, false, true, false);
        }

        public static Expression Parse(ParseInfo state, ref int index, bool forUnaryOperator)
        {
            return Parse(state, ref index, forUnaryOperator, true, false, true, false);
        }

        internal static Expression Parse(ParseInfo state, ref int index, bool forUnary = false, bool processComma = true, bool forNew = false, bool root = true, bool forForLoop = false)
        {
            int i = index;

            var first = parseOperand(state, ref i, forNew, forForLoop);
            if (first == null)
                return null;

            var res = parseContinuation(state, first, index, ref i, ref root, forUnary, processComma, forNew, forForLoop);

            if (root)
                res = deicstra(res as ExpressionTree) ?? res;

            if (!forForLoop && processComma && !forUnary && i < state.Code.Length && state.Code[i] == ';')
                i++;

            index = i;
            return res;
        }

        private static Expression parseContinuation(ParseInfo state, Expression first, int startIndex, ref int i, ref bool root, bool forUnary, bool processComma, bool forNew, bool forForLoop)
        {
            Expression second = null;
            OperationType kind = OperationType.None;
            bool canAsign = !forUnary; // на случай f() = x
            bool assign = false; // на случай операторов 'x='
            bool binary = false;
            bool repeat;
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
                                    kind = OperationType.StrictNotEqual;
                                }
                                else
                                {
                                    binary = true;
                                    kind = OperationType.NotEqual;
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
                            kind = OperationType.None;
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
                            binary = true;
                            repeat = false;
                            kind = OperationType.Conditional;
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
                                    kind = OperationType.StrictEqual;
                                }
                                else
                                    kind = OperationType.Equal;
                            }
                            else
                                kind = OperationType.Assignment;
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
                                    if ((first is Variable)
                                        && ((first as Variable).Name == "arguments" || (first as Variable).Name == "eval"))
                                        ExceptionHelper.ThrowSyntaxError("Cannot incriment \"" + (first as Variable).Name + "\" in strict mode.", state.Code, i);
                                }
                                first = new Increment(first, IncrimentType.Postincriment) { Position = first.Position, Length = i + 2 - first.Position };
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
                                kind = OperationType.Addition;
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
                                    if ((first is Variable)
                                        && ((first as Variable).Name == "arguments" || (first as Variable).Name == "eval"))
                                        ExceptionHelper.Throw(new SyntaxError("Can not decriment \"" + (first as Variable).Name + "\" in strict mode."));
                                }
                                first = new Decrement(first, DecrimentType.Postdecriment) { Position = first.Position, Length = i + 2 - first.Position };
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
                                kind = OperationType.Substract;
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
                            kind = OperationType.Multiply;
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
                                kind = OperationType.LogicalAnd;
                                break;
                            }
                            else
                            {
                                binary = true;
                                assign = false;
                                kind = OperationType.And;
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
                                kind = OperationType.LogicalOr;
                                break;
                            }
                            else
                            {
                                binary = true;
                                assign = false;
                                kind = OperationType.Or;
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
                            kind = OperationType.Xor;
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
                            state.Code = Tools.removeComments(state.SourceCode, i + 1);
                            binary = true;
                            kind = OperationType.Division;
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
                            kind = OperationType.Modulo;
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
                                kind = OperationType.SignedShiftLeft;
                            }
                            else
                            {
                                kind = OperationType.Less;
                                if (state.Code[i + 1] == '=')
                                {
                                    kind = OperationType.LessOrEqual;
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
                                    kind = OperationType.UnsignedShiftRight;
                                    i++;
                                }
                                else
                                {
                                    kind = OperationType.SignedShiftRight;
                                }
                            }
                            else
                            {
                                kind = OperationType.More;
                                if (state.Code[i + 1] == '=')
                                {
                                    kind = OperationType.MoreOrEqual;
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
                            Tools.SkipSpaces(state.Code, ref i);

                            var s = i;
                            if (!Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                                ExceptionHelper.Throw(new SyntaxError(string.Format(Strings.InvalidPropertyName, CodeCoordinates.FromTextPosition(state.Code, i, 0))));

                            string name = state.Code.Substring(s, i - s);

                            JSValue jsname = null;
                            if (!state.stringConstants.TryGetValue(name, out jsname))
                                state.stringConstants[name] = jsname = name;
                            else
                                name = jsname._oValue.ToString();

                            first = new Property(first, new Constant(name)
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
                            mname = (Expression)ExpressionTree.Parse(state, ref i, false, true, false, true, false);
                            if (mname == null)
                                ExceptionHelper.Throw((new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            if (state.Code[i] != ']')
                                ExceptionHelper.Throw((new SyntaxError("Expected \"]\" at " + CodeCoordinates.FromTextPosition(state.Code, startPos, 0))));
                            first = new Property(first, mname) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;

                            if (state.message != null)
                            {
                                startPos = 0;
                                var cname = mname as Constant;
                                if (cname != null
                                    && cname.value._valueType == JSValueType.String
                                    && Parser.ValidateName(cname.value._oValue.ToString(), ref startPos, false)
                                    && startPos == cname.value._oValue.ToString().Length)
                                    state.message(MessageLevel.Recomendation, mname.Position, mname.Length, "[\"" + cname.value._oValue + "\"] is better written in dot notation.");
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
                                Tools.SkipSpaces(state.Code, ref i);
                                Tools.CheckEndOfInput(state.Code, ref i);

                                if (state.Code[i] == ')')
                                {
                                    break;
                                }
                                else
                                {
                                    bool commaExists = args.Count == 0;
                                    for (;;)
                                    {
                                        if (state.Code[i] == ',')
                                        {
                                            if (commaExists)
                                                ExceptionHelper.ThrowSyntaxError("Missing argument of function call", state.Code, i);
                                            do
                                                i++;
                                            while (Tools.IsWhiteSpace(state.Code[i]));
                                            commaExists = true;
                                        }
                                        else
                                            break;
                                    }
                                    if (!commaExists)
                                        ExceptionHelper.ThrowSyntaxError("Expected ','", state.Code, i);
                                }
                                if (i + 1 == state.Code.Length)
                                    ExceptionHelper.ThrowSyntaxError("Unexpected end of source", state.Code, i);

                                var spread = Parser.Validate(state.Code, "...", ref i);
                                args.Add(Parse(state, ref i, false, false));
                                if (spread)
                                {
                                    args[args.Count - 1] = new Spread(args[args.Count - 1]);
                                    withSpread = true;
                                }
                                if (args[args.Count - 1] == null)
                                    ExceptionHelper.ThrowSyntaxError("Expected \")\"", state.Code, startPos);
                            }

                            first = new Call(first, args.ToArray())
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
                    case '`':
                        {
                            var template = TemplateString.Parse(state, ref i, TemplateStringMode.Tag);
                            first = new Call(first, new Expression[] { template })
                            {
                                Position = first.Position,
                                Length = i - first.Position + 1,
                                withSpread = true
                            };

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
                                kind = OperationType.InstanceOf;
                                binary = true;
                                break;
                            }
                            else if (Parser.Validate(state.Code, "in", ref i))
                            {
                                if (forForLoop)
                                {
                                    i = rollbackPos;
                                    goto case ';';
                                }
                                kind = OperationType.In;
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

                            ExceptionHelper.ThrowSyntaxError("Unexpected token '" + state.Code[i] + "'", state.Code, i);
                            break;
                        }
                }
            }
            while (repeat);

            if (state.strict
                && (first is Variable)
                && ((first as Variable).Name == "arguments" || (first as Variable).Name == "eval"))
            {
                if (assign || kind == OperationType.Assignment)
                    ExceptionHelper.ThrowSyntaxError("Assignment to eval or arguments is not allowed in strict mode", state.Code, i);
            }

            if ((kind == OperationType.Assignment) || assign)
            {
                var error = !canAsign || !canBeAssignee(first);

                if (kind == OperationType.Assignment)
                {
                    if (first is ObjectDefinition || first is ArrayDefinition)
                    {
                        try
                        {
                            first = new ObjectDesctructor(first);
                            error = false;
                        }
                        catch
                        {
                            // Exception will be handled in next line
                        }
                    }
                }

                if (error)
                    ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment, state.Code, first.Position, first.Length);
            }

            if (binary && !forUnary)
            {
                do
                    i++;
                while (state.Code.Length > i && Tools.IsWhiteSpace(state.Code[i]));
                if (state.Code.Length > i)
                {
                    if (kind == OperationType.Conditional)
                    {
                        bool proot = false;
                        second = parseContinuation(state, parseTernaryBranches(state, forForLoop, ref i), startIndex, ref i, ref proot, forUnary, processComma, false, forForLoop);
                    }
                    else
                        second = ExpressionTree.Parse(state, ref i, false, processComma, false, false, forForLoop);
                }
                else
                {
                    ExceptionHelper.ThrowSyntaxError("Expected second operand", state.Code, i);
                }
            }

            Expression res = null;
            if (assign)
            {
                root = false; // блокируем вызов дейкстры
                second = deicstra(second as ExpressionTree);
                var opassigncache = new AssignmentOperatorCache(first);
                if (second is ExpressionTree && (second as ExpressionTree)._operationKind == OperationType.None)
                {
                    second._left = new Assignment(opassigncache, new ExpressionTree()
                    {
                        _left = opassigncache,
                        _right = second._left,
                        _operationKind = kind,
                        Position = startIndex,
                        Length = i - startIndex
                    })
                    {
                        Position = startIndex,
                        Length = i - startIndex
                    };
                    res = second;
                }
                else
                {
                    res = new Assignment(opassigncache, new ExpressionTree()
                    {
                        _left = opassigncache,
                        _right = second,
                        _operationKind = kind,
                        Position = startIndex,
                        Length = i - startIndex
                    })
                    {
                        Position = startIndex,
                        Length = i - startIndex
                    };
                }
            }
            else
            {
                if (!root || kind != OperationType.None || second != null)
                {
                    if (forUnary && (kind == OperationType.None) && (first is ExpressionTree))
                        res = first as Expression;
                    else
                        res = new ExpressionTree() { _left = first, _right = second, _operationKind = kind, Position = startIndex, Length = i - startIndex };
                }
                else
                    res = first;
            }

            return res;
        }

        internal static bool canBeAssignee(Expression first)
        {
            return first is Variable
                || first is Property
                || ((first is Constant) && (first.Evaluate(null).ValueType <= JSValueType.Undefined));
        }

        private static Expression parseOperand(ParseInfo state, ref int i, bool forNew, bool forForLoop)
        {
            int start = i;
            Expression operand;
            if (Parser.Validate(state.Code, "this", ref i)
                || Parser.Validate(state.Code, "super", ref i)
                || Parser.Validate(state.Code, "new.target", ref i))
            {
                var name = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                switch (name)
                {
                    case "super":
                        {
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;

                            if ((state.CodeContext & CodeContext.InClassDefenition) == 0
                                || (state.Code[i] != '.'
                                    && state.Code[i] != '['
                                    && (state.Code[i] != '(' || (state.CodeContext & CodeContext.InClassConstructor) == 0)))
                                ExceptionHelper.ThrowSyntaxError("super keyword unexpected in this coontext", state.Code, i);

                            operand = new Super();
                            break;
                        }
                    case "new.target":
                        {
                            operand = new NewTarget();
                            break;
                        }
                    case "this":
                        {
                            operand = new This();
                            break;
                        }
                    default:
                        {
                            JSValue tempStr;
                            if (state.stringConstants.TryGetValue(name, out tempStr))
                                name = tempStr._oValue.ToString();
                            else
                                state.stringConstants[name] = name;

                            operand = new Variable(name, state.lexicalScopeLevel);
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
                    operand = (Expression)Parser.Parse(state, ref i, (CodeFragmentType)2, false);
                }
                finally
                {
                    state.CodeContext = oldCodeContext;
                }
            }

            if (operand == null)
            {
                if (Parser.ValidateName(state.Code, ref i, state.strict))
                {
                    var name = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                    if (name == "undefined")
                    {
                        operand = new Constant(JSValue.undefined);
                    }
                    else
                    {
                        JSValue jsName = null;
                        if (!state.stringConstants.TryGetValue(name, out jsName))
                            state.stringConstants[name] = jsName = name;
                        else
                            name = jsName._oValue.ToString();

                        operand = new Variable(name, state.lexicalScopeLevel);
                    }
                }
                else if (Parser.ValidateValue(state.Code, ref i))
                {
                    string value = state.Code.Substring(start, i - start);
                    if ((value[0] == '\'') || (value[0] == '"'))
                    {
                        value = Tools.Unescape(value.Substring(1, value.Length - 2), state.strict);
                        JSValue jsValue = null;
                        if (!state.stringConstants.TryGetValue(value, out jsValue))
                            state.stringConstants[value] = jsValue = value;

                        operand = new Constant(jsValue) { Position = start, Length = i - start };
                    }
                    else
                    {
                        bool b = false;
                        if (value == "null")
                            operand = new Constant(JSValue.@null) { Position = start, Length = i - start };
                        else if (bool.TryParse(value, out b))
                            operand = new Constant(b ? NiL.JS.BaseLibrary.Boolean.True : NiL.JS.BaseLibrary.Boolean.False) { Position = start, Length = i - start };
                        else
                        {
                            int n = 0;
                            double d = 0;
                            if (Tools.ParseNumber(state.Code, ref start, out d, 0, ParseNumberOptions.Default | (state.strict ? ParseNumberOptions.RaiseIfOctal : ParseNumberOptions.None)))
                            {
                                if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                {
                                    if (state.intConstants.ContainsKey(n))
                                        operand = new Constant(state.intConstants[n]) { Position = start, Length = i - start };
                                    else
                                        operand = new Constant(state.intConstants[n] = n) { Position = start, Length = i - start };
                                }
                                else
                                {
                                    if (state.doubleConstants.ContainsKey(d))
                                        operand = new Constant(state.doubleConstants[d]) { Position = start, Length = i - start };
                                    else
                                        operand = new Constant(state.doubleConstants[d] = d) { Position = start, Length = i - start };
                                }
                            }
                            else if (Parser.ValidateRegex(state.Code, ref start, true))
                                throw new InvalidOperationException("This case was moved");
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
                                        ExceptionHelper.Throw(new SyntaxError("Unexpected end of source."));

                                    operand = (Expression)Parse(state, ref i, true, true, false, true, forForLoop);

                                    if (((operand as Property) as object ?? (operand as Variable)) == null)
                                    {
                                        ExceptionHelper.ThrowSyntaxError("Invalid prefix operation. ", state.Code, i);
                                    }

                                    if (state.strict
                                        && (operand is Variable)
                                        && ((operand as Variable).Name == "arguments" || (operand as Variable).Name == "eval"))
                                    {
                                        ExceptionHelper.ThrowSyntaxError("Can not incriment \"" + (operand as Variable).Name + "\" in strict mode.", state.Code, i);
                                    }

                                    operand = new Increment(operand, IncrimentType.Preincriment);
                                }
                                else
                                {
                                    while (Tools.IsWhiteSpace(state.Code[i]))
                                        i++;
                                    var f = (Expression)Parse(state, ref i, true, true, false, true, forForLoop);
                                    operand = new ConvertToNumber(f);
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
                                        ExceptionHelper.Throw(new SyntaxError("Unexpected end of source."));

                                    operand = (Expression)Parse(state, ref i, true, true, false, true, forForLoop);

                                    if (((operand as Property) as object ?? (operand as Variable)) == null)
                                    {
                                        ExceptionHelper.ThrowSyntaxError("Invalid prefix operation.", state.Code, i);
                                    }

                                    if (state.strict
                                        && (operand is Variable)
                                        && ((operand as Variable).Name == "arguments" || (operand as Variable).Name == "eval"))
                                        ExceptionHelper.Throw(new SyntaxError("Can not decriment \"" + (operand as Variable).Name + "\" in strict mode."));

                                    operand = new Decrement(operand, DecrimentType.Predecriment);
                                }
                                else
                                {
                                    while (Tools.IsWhiteSpace(state.Code[i]))
                                        i++;
                                    var f = (Expression)Parse(state, ref i, true, true, false, true, forForLoop);
                                    operand = new Negation(f);
                                }
                                break;
                            }
                        case '!':
                            {
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                operand = new LogicalNegation((Expression)Parse(state, ref i, true, true, false, true, forForLoop));
                                if (operand == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                break;
                            }
                        case '~':
                            {
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                operand = (Expression)Parse(state, ref i, true, true, false, true, forForLoop);
                                if (operand == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                operand = new BitwiseNegation(operand);
                                break;
                            }
                        case 't':
                            {
                                i += 5;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                operand = (Expression)Parse(state, ref i, true, false, false, true, forForLoop);
                                if (operand == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                operand = new TypeOf(operand);
                                break;
                            }
                        case 'v':
                            {
                                i += 3;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));

                                operand = new Comma(
                                    (Expression)Parse(state, ref i, true, false, false, true, forForLoop),
                                    new Constant(JSValue.undefined));

                                if (operand == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                break;
                            }
                        case 'd':
                            {
                                i += 5;
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                                operand = (Expression)Parse(state, ref i, true, false, false, true, forForLoop);
                                if (operand == null)
                                {
                                    var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                                    ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                operand = new Expressions.Delete(operand);
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
                        var temp = Parse(state, ref i, false, false);
                        if (operand == null)
                            operand = temp;
                        else
                            operand = new Comma(operand, temp);
                        while (Tools.IsWhiteSpace(state.Code[i]))
                            i++;
                        if (state.Code[i] != ')' && state.Code[i] != ',')
                            ExceptionHelper.ThrowSyntaxError("Expected \")\"");
                    }

                    i++;
                    if (((state.CodeContext & (CodeContext.InExpression | CodeContext.InEval)) != 0 && operand is FunctionDefinition)
                        || (forNew && operand is Call))
                    {
                        operand = new Comma(operand, null);
                    }
                }
                else
                {
                    if (forForLoop)
                        return null;
                }
            }

            if (operand == null || operand is Empty)
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);

            if (operand.Position == 0)
            {
                operand.Position = start;
                operand.Length = i - start;
            }

            return operand;
        }

        private static Expression parseTernaryBranches(ParseInfo state, bool forEnumeration, ref int i)
        {
            Constant result = null;
            var oldCodeContext = state.CodeContext;
            state.CodeContext |= CodeContext.InExpression;
            try
            {
                var position = i;
                var threads = new[]
                    {
                        Parse(state, ref i, false, true, false, true, false),
                        null
                    };

                if (state.Code[i] != ':')
                    ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
                result = new Constant(new JSValue() { _valueType = JSValueType.Object, _oValue = threads }) { Position = position };
                threads[1] = Parse(state, ref i, false, false, false, true, forEnumeration);
                result.Length = i - position;
            }
            finally
            {
                state.CodeContext = oldCodeContext;
            }

            return result;
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            throw new InvalidOperationException();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _this = getFastImpl();
            _this.Position = Position;
            _this.Length = Length;
            return true;
        }

        public override string ToString()
        {
            return _operationKind + "(" + _left + (_right != null ? ", " + _right : "") + ")";
        }
    }
}