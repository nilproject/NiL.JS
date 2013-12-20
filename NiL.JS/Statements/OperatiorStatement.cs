using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;
using System.Threading;
using System.Text.RegularExpressions;

namespace NiL.JS.Statements
{
    internal enum OperationTypeGroups : int
    {
        None = 0,
        Logic = 0x10,
        Bit = 0x20,
        Arithmetic0 = 0x30,
        Arithmetic1 = 0x40,
        Unary = 0x50,
        Special = 0xF0
    }

    internal enum OperationType : int
    {
        None = OperationTypeGroups.None + 0,
        Assign = OperationTypeGroups.None + 1,
        Ternary = OperationTypeGroups.None + 2,
        More = OperationTypeGroups.Logic + 0,
        Less = OperationTypeGroups.Logic + 1,
        MoreOrEqual = OperationTypeGroups.Logic + 2,
        LessOrEqual = OperationTypeGroups.Logic + 3,
        Equal = OperationTypeGroups.Logic + 4,
        NotEqual = OperationTypeGroups.Logic + 5,
        StrictEqual = OperationTypeGroups.Logic + 6,
        StrictNotEqual = OperationTypeGroups.Logic + 7,
        And = OperationTypeGroups.Logic + 8,
        Or = OperationTypeGroups.Logic + 9,
        Xor = OperationTypeGroups.Logic + 10,
        LogicalAnd = OperationTypeGroups.Logic + 11,
        LogicalOr = OperationTypeGroups.Logic + 12,
        InstanceOf = OperationTypeGroups.Logic + 13,
        In = OperationType.LogicalAnd + 14,
        SignedShiftLeft = OperationTypeGroups.Bit + 0,
        SignedShiftRight = OperationTypeGroups.Bit + 1,
        UnsignedShiftLeft = OperationTypeGroups.Bit + 2,
        UnsignedShiftRight = OperationTypeGroups.Bit + 3,
        Addition = OperationTypeGroups.Arithmetic0 + 0,
        Substract = OperationTypeGroups.Arithmetic0 + 1,
        Multiply = OperationTypeGroups.Arithmetic1 + 0,
        Module = OperationTypeGroups.Arithmetic1 + 1,
        Division = OperationTypeGroups.Arithmetic1 + 2,
        Incriment = OperationTypeGroups.Unary + 0,
        Decriment = OperationTypeGroups.Unary + 1,
        Negative = OperationTypeGroups.Unary + 2,
        Positive = OperationTypeGroups.Unary + 3,
        LogicalNot = OperationTypeGroups.Unary + 4,
        Not = OperationTypeGroups.Unary + 5,
        Call = OperationTypeGroups.Special + 0,
        TypeOf = OperationTypeGroups.Special + 1,
        New = OperationTypeGroups.Special + 2,
    }

    internal delegate JSObject OpDelegate(Context context);

    internal class OperatorStatement : Statement, IOptimizable
    {
        private static readonly JSObject tempResult = new JSObject();

        private Statement fastImpl;

        private OperationType _type;
        private OperationType type
        {
            get
            {
                return _type;
            }
            set
            {
                fastImpl = null;
                switch (value)
                {
                    case OperationType.Multiply:
                        {
                            fastImpl = new Operators.Mul(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.None:
                        {
                            fastImpl = new Operators.None(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Assign:
                        {
                            fastImpl = new Operators.Assign(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Less:
                        {
                            fastImpl = new Operators.Less(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Incriment:
                        {
                            fastImpl = new Operators.Incriment(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Call:
                        {
                            fastImpl = new Operators.Call(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Decriment:
                        {
                            del = OpDecriment;
                            break;
                        }
                    case OperationType.LessOrEqual:
                        {
                            del = OpLessOrEqual;
                            break;
                        }
                    case OperationType.Addition:
                        {
                            del = OpAddition;
                            break;
                        }
                    case OperationType.StrictNotEqual:
                        {
                            del = OpStrictNotEqual;
                            break;
                        }
                    case OperationType.More:
                        {
                            del = OpMore;
                            break;
                        }
                    case OperationType.MoreOrEqual:
                        {
                            del = OpMoreOrEqual;
                            break;
                        }
                    case OperationType.Division:
                        {
                            del = OpDivision;
                            break;
                        }
                    case OperationType.Equal:
                        {
                            del = OpEqual;
                            break;
                        }
                    case OperationType.Substract:
                        {
                            del = OpSubstract;
                            break;
                        }
                    case OperationType.StrictEqual:
                        {
                            fastImpl = new Operators.StrictEqual(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.LogicalOr:
                        {
                            del = OpLogicalOr;
                            break;
                        }
                    case OperationType.LogicalAnd:
                        {
                            del = OpLogicalAnd;
                            break;
                        }
                    case OperationType.NotEqual:
                        {
                            del = OpNotEqual;
                            break;
                        }
                    case OperationType.UnsignedShiftLeft:
                        {
                            del = OpUnsignedShiftLeft;
                            break;
                        }
                    case OperationType.UnsignedShiftRight:
                        {
                            del = OpUnsignedShiftRight;
                            break;
                        }
                    case OperationType.SignedShiftLeft:
                        {
                            del = OpSignedShiftLeft;
                            break;
                        }
                    case OperationType.SignedShiftRight:
                        {
                            del = OpSignedShiftRight;
                            break;
                        }
                    case OperationType.Module:
                        {
                            del = OpMod;
                            break;
                        }
                    case OperationType.LogicalNot:
                        {
                            del = OpLogicalNot;
                            break;
                        }
                    case OperationType.Not:
                        {
                            del = OpNot;
                            break;
                        }
                    case OperationType.Xor:
                        {
                            del = OpXor;
                            break;
                        }
                    case OperationType.Or:
                        {
                            del = OpOr;
                            break;
                        }
                    case OperationType.And:
                        {
                            del = OpAnd;
                            break;
                        }
                    case OperationType.Ternary:
                        {
                            del = OpTernary;
                            break;
                        }
                    case OperationType.TypeOf:
                        {
                            del = OpTypeOf;
                            break;
                        }
                    case OperationType.New:
                        {
                            del = OpNew;
                            break;
                        }
                    case OperationType.InstanceOf:
                        {
                            del = OpInstanceOf;
                            break;
                        }
                    case OperationType.In:
                        {
                            fastImpl = new Operators.In(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                _type = value;
            }
        }
        private Statement first;
        private Statement second;
        private OpDelegate del;

        public OperatorStatement()
        {
            //del = Opdel;
        }

        public static Statement ParseForUnary(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, true).Statement;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, false);
        }

        public static ParseResult Parse(ParsingState state, ref int index, bool processComma)
        {
            return Parse(state, ref index, processComma, false);
        }

        private static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary)
        {
            string code = state.Code;
            int i = index;
            OperationType type = OperationType.None;
            Statement first = null;
            Statement second = null;
            int s = i;
            if (Parser.ValidateName(code, ref i, true) || Parser.Validate(code, "this", ref i))
                first = new GetVaribleStatement(code.Substring(s, i - s));
            else if (Parser.ValidateValue(code, ref i, true))
            {
                string value = code.Substring(s, i - s);
                if ((value[0] == '\'') || (value[0] == '"'))
                    first = new ImmidateValueStatement(Parser.Unescape(value.Substring(1, value.Length - 2)));
                else
                {
                    bool b = false;
                    if (value == "null")
                        first = new ImmidateValueStatement(new JSObject() { ValueType = ObjectValueType.Object });
                    else if (bool.TryParse(value, out b))
                        first = new ImmidateValueStatement(b);
                    else
                    {
                        int n = 0;
                        double d = 0;
                        if (Parser.ParseNumber(code, ref s, true, out n))
                            first = new ImmidateValueStatement(n);
                        else if (Parser.ParseNumber(code, ref s, true, out d))
                            first = new ImmidateValueStatement(d);
                        else if (Parser.ValidateRegex(code, ref s, true))
                        {
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            first = new Operators.Call(new GetVaribleStatement("RegExp"), new ImmidateValueStatement(new IContextStatement[2]
                            {
                                new ImmidateValueStatement(value.Substring(1, s - 2)),
                                new ImmidateValueStatement(flags)
                            }));
                        }
                        else
                            throw new ArgumentException("Invalid process value (" + value + ")");
                    }
                }
            }
            else if ((code[i] == '!')
                || (code[i] == '~')
                || (code[i] == '+')
                || (code[i] == '-')
                || (code[i] == 'n' && code.Substring(i, 3) == "new")
                || (code[i] == 't' && code.Substring(i, 6) == "typeof")
                || (code[i] == 'v' && code.Substring(i, 4) == "void"))
            {
                switch (code[i])
                {
                    case '+':
                        {
                            i++;
                            if (code[i] == '+')
                            {
                                do i++; while (char.IsWhiteSpace(code[i]));
                                first = Parse(state, ref i, true, true).Statement;
                                (first as OperatorStatement).type = OperationType.Incriment;
                            }
                            else
                            {
                                var f = Parse(state, ref i, true, true).Statement;
                                index = i;
                                return new ParseResult()
                                {
                                    Statement = new Operators.Mul(new ImmidateValueStatement(1), f),
                                    Message = "",
                                    IsParsed = true
                                };
                            }
                            break;
                        }
                    case '-':
                        {
                            i++;
                            if (code[i] == '-')
                            {
                                do i++; while (char.IsWhiteSpace(code[i]));
                                first = Parse(state, ref i, true, true).Statement;
                                (first as OperatorStatement).type = OperationType.Decriment;
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                first = new OperatorStatement() { first = new ImmidateValueStatement(0), second = Parse(state, ref i, true, true).Statement, type = OperationType.Substract };
                            }
                            break;
                        }
                    case '!':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, true, true).Statement;
                            (first as OperatorStatement).type = OperationType.LogicalNot;
                            break;
                        }
                    case '~':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, true, true).Statement;
                            (first as OperatorStatement).type = OperationType.Not;
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true).Statement;
                            (first as OperatorStatement).type = OperationType.TypeOf;
                            break;
                        }
                    case 'v':
                        {
                            i += 3;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new OperatorStatement() { first = new ImmidateValueStatement(JSObject.undefined), second = Parse(state, ref i, false, true).Statement, type = OperationType.None };
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true).Statement;
                            (first as OperatorStatement).type = OperationType.New;
                            break;
                        }
                    default:
                        throw new NotImplementedException("Unary operator " + code[i]);
                }
            }
            else if (code[i] == '(')
            {
                i++;
                first = OperatorStatement.Parse(state, ref i, true).Statement;
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != ')')
                    throw new ArgumentException();
                i++;
            }
            else
                first = Parser.Parse(state, ref i, 2);
            if (first is EmptyStatement)
                throw new ArgumentException("Invalid operator argument");
            bool canAsign = true && !forUnary; // на случай f() = x
            bool assign = false; // на случай операторов 'x='
            bool binar = false;
            bool repeat; // лёгкая замена goto. Тот самый случай, когда он уместен.
            int rollbackPos;
            do
            {
                repeat = false;
                while (char.IsWhiteSpace(code[i]) && !Parser.isLineTerminator(code[i])) i++;
                rollbackPos = i;
                while (char.IsWhiteSpace(code[i])) i++;
                switch (code[i])
                {
                    case '\n':
                    case '\r':
                    case ';':
                    case ')':
                    case ']':
                    case '}':
                    case ':':
                        {
                            binar = false;
                            break;
                        }
                    case '!':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '=')
                            {
                                i++;
                                if (code[i + 1] == '=')
                                {
                                    i++;
                                    binar = true;
                                    type = OperationType.StrictNotEqual;
                                }
                                else
                                {
                                    binar = true;
                                    type = OperationType.NotEqual;
                                }
                            }
                            else throw new ArgumentException("Invalid operator '!'");
                            break;
                        }
                    case ',':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            i = rollbackPos;
                            goto case ';';
                        }
                    case '?':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            type = OperationType.Ternary;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            var sec = new Statement[]
                                {
                                    Parser.Parse(state, ref i, 1),
                                    null
                                };
                            if (code[i] != ':')
                                throw new ArgumentException("Invalid char in ternary operator");
                            do i++; while (char.IsWhiteSpace(code[i]));
                            sec[1] = Parser.Parse(state, ref i, 1);
                            second = new ImmidateValueStatement(sec);
                            binar = false;
                            repeat = false;
                            break;
                        }
                    case '=':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '=')
                            {
                                i++;
                                if (code[i + 1] == '=')
                                {
                                    i++;
                                    binar = true;
                                    type = OperationType.StrictEqual;
                                }
                                else
                                {
                                    binar = true;
                                    type = OperationType.Equal;
                                }
                            }
                            else
                            {
                                binar = true;
                                type = OperationType.Assign;
                            }
                            break;
                        }
                    case '+':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '+')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                first = new OperatorStatement() { second = first, type = OperationType.Incriment };
                                repeat = true;
                                i += 2;
                            }
                            else
                            {
                                binar = true;
                                type = OperationType.Addition;
                                if (code[i + 1] == '=')
                                {
                                    assign = true;
                                    i++;
                                }
                            }
                            break;
                        }
                    case '-':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '-')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                first = new OperatorStatement() { second = first, type = OperationType.Decriment };
                                repeat = true;
                                i += 2;
                            }
                            else
                            {
                                binar = true;
                                type = OperationType.Substract;
                                if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binar = true;
                            type = OperationType.Multiply;
                            if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '&')
                            {
                                i++;
                                binar = true;
                                assign = false;
                                type = OperationType.LogicalAnd;
                                break;
                            }
                            else
                            {
                                binar = true;
                                assign = false;
                                type = OperationType.And;
                                if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '|')
                            {
                                i++;
                                binar = true;
                                assign = false;
                                type = OperationType.LogicalOr;
                                break;
                            }
                            else
                            {
                                binar = true;
                                assign = false;
                                type = OperationType.Or;
                                if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binar = true;
                            type = OperationType.Xor;
                            if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binar = true;
                            type = OperationType.Division;
                            if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                break;
                            }
                            binar = true;
                            type = OperationType.Module;
                            if (code[i + 1] == '=')
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
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binar = true;
                            if (code[i + 1] == '<')
                            {
                                i++;
                                if (code[i + 1] == '<')
                                {
                                    type = OperationType.UnsignedShiftLeft;
                                    i++;
                                }
                                else
                                    type = OperationType.SignedShiftLeft;
                            }
                            else
                                type = OperationType.Less;
                            if (code[i + 1] == '=')
                            {
                                type = OperationType.LessOrEqual;
                                i++;
                            }
                            break;
                        }
                    case '>':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binar = true;
                            if (code[i + 1] == '>')
                            {
                                i++;
                                if (code[i + 1] == '>')
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
                            }
                            if (code[i + 1] == '=')
                            {
                                type = OperationType.MoreOrEqual;
                                i++;
                            }
                            break;
                        }
                    case '.':
                        {
                            binar = true;
                            i++;
                            while (char.IsWhiteSpace(code[i])) i++;
                            s = i;
                            if (!Parser.ValidateName(code, ref i, true, false) && !Parser.ValidateValue(code, ref i, true))
                                throw new ArgumentException("code (" + i + ")");
                            string name = code.Substring(s, i - s);
                            first = new GetFieldStatement(first, name);
                            repeat = true;
                            canAsign = true;
                            break;
                        }
                    case '[':
                        {
                            List<Statement> args = new List<Statement>();
                            i++;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                if (code[i] == ']')
                                    break;
                                else if (code[i] == ',')
                                    do i++; while (char.IsWhiteSpace(code[i]));
                                args.Add(Parser.Parse(state, ref i, 1));
                                if ((args[args.Count - 1] is OperatorStatement) && (args[args.Count - 1] as OperatorStatement).type == OperationType.None)
                                    args[args.Count - 1] = (args[args.Count - 1] as OperatorStatement).first;
                            }
                            first = new GetFieldStatement(first, args[0]);
                            i++;
                            repeat = true;
                            canAsign = true;
                            break;
                        }
                    case '(':
                        {
                            List<Statement> args = new List<Statement>();
                            i++;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                if (code[i] == ')')
                                    break;
                                else if (code[i] == ',')
                                    do i++; while (char.IsWhiteSpace(code[i]));
                                args.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                            }
                            first = new OperatorStatement()
                            {
                                first = first,
                                second = new ImmidateValueStatement(args.ToArray()),
                                type = OperationType.Call
                            };
                            i++;
                            repeat = !forUnary;
                            canAsign = false;
                            break;
                        }
                    case 'i':
                        {
                            if (forUnary)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (Parser.Validate(code, "instanceof", ref i))
                            {
                                type = OperationType.InstanceOf;
                                binar = true;
                                break;
                            }
                            else if (Parser.Validate(code, "in", ref i))
                            {
                                type = OperationType.In;
                                binar = true;
                                break;
                            }
                            goto default;
                        }
                    default:
                        {
                            if (Parser.isLineTerminator(code[i]))
                                goto case '\n';
                            if (i != rollbackPos)
                            {
                                i = rollbackPos;
                                goto case '\n';
                            }
                            throw new ArgumentException("Invalid operator '" + code[i] + "'");
                        }
                }
            } while (repeat);
            if ((!canAsign) && ((type == OperationType.Assign) || (assign)))
                throw new InvalidOperationException("invalid left-hand side in assignment");
            if (binar)
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                second = OperatorStatement.Parse(state, ref i, false).Statement;
                if (second is OperatorStatement)
                {
                    var seops = second as OperatorStatement;
                    if (seops.type != OperationType.None)
                    {
                        int op0 = (int)type & (int)OperationTypeGroups.Special;
                        int op1 = (int)seops.type & (int)OperationTypeGroups.Special;
                        if ((op0 > op1) || (op0 == op1 && type != OperationType.Assign))
                        {
                            var t0 = first;
                            var t1 = seops.first;
                            var t2 = seops.second;
                            var type0 = type;
                            var type1 = seops.type;

                            second = t2;
                            type = type1;
                            first = new OperatorStatement() { first = t0, second = t1, type = type0 };
                        }
                    }
                    else
                        second = ((OperatorStatement)second).first;
                }
            }
            if (processComma && (code[i] == ','))
            {
                first = new OperatorStatement() { first = first, second = second, type = type };
                type = OperationType.None;
                do i++; while (char.IsWhiteSpace(code[i]));
                second = OperatorStatement.Parse(state, ref i).Statement;
            }
            OperatorStatement res = null;
            if (assign)
                res = new OperatorStatement() { first = first, second = new OperatorStatement() { first = first, second = second, type = type }, type = OperationType.Assign };
            else
            {
                if (forUnary && (type == OperationType.None) && (first is OperatorStatement))
                    res = first as OperatorStatement;
                else
                    res = new OperatorStatement() { first = first, second = second, type = type };
            }
            index = i;
            return new ParseResult()
            {
                Statement = res,
                Message = "",
                IsParsed = true
            };
        }

        private JSObject OpAddition(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            double dr;
            string sr;
            switch (temp.ValueType)
            {
                case ObjectValueType.Int:
                    {
                        dr = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                        {
                            dr += temp.iValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.Double)
                        {
                            dr += temp.dValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.String)
                        {
                            sr = dr.ToString() + (string)temp.oValue;
                            tempResult.ValueType = ObjectValueType.String;
                            tempResult.oValue = sr;
                            return tempResult;
                        }
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dr = temp.dValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                        {
                            dr += temp.iValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.Double)
                        {
                            dr += temp.dValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        break;
                    }
                case ObjectValueType.String:
                    {
                        var val = temp.oValue as string;
                        temp = second.Invoke(context);
                        tempResult.ValueType = ObjectValueType.String;
                        if (temp.ValueType == ObjectValueType.Int)
                        {
                            val += temp.iValue;
                            tempResult.oValue = val;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.Double)
                        {
                            val += temp.dValue;
                            tempResult.oValue = val;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.String)
                        {
                            val += temp.oValue as string;
                            tempResult.oValue = val;
                            return tempResult;
                        }
                        break;
                    }
                case ObjectValueType.Date:
                    {
                        var val = temp.ToPrimitiveValue_String_Value();
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.String:
                                {
                                    tempResult.ValueType = ObjectValueType.String;
                                    tempResult.oValue = val.oValue as string + temp.oValue as string;
                                    return tempResult;
                                }
                        }
                        break;
                    }
                case ObjectValueType.NoExistInObject:
                case ObjectValueType.Undefined:
                    {
                        var val = "undefined";
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.String:
                                {
                                    tempResult.ValueType = ObjectValueType.String;
                                    tempResult.oValue = val as string + temp.oValue as string;
                                    return tempResult;
                                }
                        }
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String();
                        if (temp.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        else if (temp.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        else if (temp.ValueType == ObjectValueType.Object)
                        {
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = double.NaN;
                            return tempResult;
                        }
                        break;
                    }
                case ObjectValueType.NoExist:
                    throw new InvalidOperationException("Varible not defined");
            }
            throw new NotImplementedException();
        }

        private JSObject OpSubstract(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            double dr;
            ObjectValueType lvt = temp.ValueType;
            switch (lvt)
            {
                case ObjectValueType.Int:
                    {
                        dr = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                        {
                            dr -= temp.iValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == ObjectValueType.Double)
                        {
                            dr -= temp.dValue;
                            tempResult.ValueType = ObjectValueType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dr = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Int:
                                {
                                    dr -= temp.iValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Double:
                                {
                                    dr -= temp.dValue;
                                    tempResult.ValueType = ObjectValueType.Double;
                                    tempResult.dValue = dr;
                                    return tempResult;
                                }
                            case ObjectValueType.Object:
                            case ObjectValueType.Date:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == ObjectValueType.Int)
                                        goto case ObjectValueType.Int;
                                    else if (temp.ValueType == ObjectValueType.Double)
                                        goto case ObjectValueType.Double;
                                    break;
                                }
                        }
                        break;
                    }
                case ObjectValueType.String:
                    {
                        tempResult.ValueType = ObjectValueType.Double;
                        var tval = temp.oValue as string;
                        double val = double.NaN;
                        int i = 0;
                        if (!Parser.ParseNumber(tval, ref i, false, out val))
                        {
                            tempResult.dValue = val;
                            return tempResult;
                        }
                        else
                        {
                            temp = second.Invoke(context);
                            if (temp.ValueType == ObjectValueType.Int)
                            {
                                val -= temp.iValue;
                                tempResult.dValue = val;
                                return tempResult;
                            }
                            else if (temp.ValueType == ObjectValueType.Double)
                            {
                                val -= temp.dValue;
                                tempResult.dValue = val;
                                return tempResult;
                            }
                            else if (temp.ValueType == ObjectValueType.String)
                            {
                                i = 0;
                                if (!Parser.ParseNumber(tval, ref i, false, out tempResult.dValue))
                                {
                                    tempResult.dValue = val;
                                    return tempResult;
                                }
                                else
                                {
                                    tempResult.dValue = val - tempResult.dValue;
                                    return tempResult;
                                }
                            }
                        }
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String();
                        if (temp.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        else if (temp.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        break;
                    }
            }
            throw new NotImplementedException();
        }

        private JSObject OpAnd(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = (int)left & (int)temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left & (int)temp.dValue;
                else if (temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = (int)left & temp.iValue;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = (int)left & (int)temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left & (int)temp.dValue;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpDecriment(Context context)
        {
            var val = (first ?? second).Invoke(context);
            if (val.ValueType == ObjectValueType.NoExist)
                throw new InvalidOperationException("varible is undefined");
            if ((val.assignCallback != null) && (!val.assignCallback()))
                return double.NaN;

            JSObject o = null;
            if ((second != null) && (val.ValueType != ObjectValueType.Undefined))
            {
                o = tempResult;
                o.Assign(val);
            }
            else
                o = val;
            if (val.ValueType == ObjectValueType.Int)
                val.iValue--;
            else if (val.ValueType == ObjectValueType.Double)
                val.dValue = val.dValue--;
            else if (val.ValueType == ObjectValueType.Bool)
            {
                val.ValueType = ObjectValueType.Int;
                val.iValue--;
            }
            else if (val.ValueType == ObjectValueType.Undefined)
            {
                val.ValueType = ObjectValueType.Double;
                val.dValue = double.NaN;
            }
            else throw new NotImplementedException();
            return o;
        }

        private JSObject OpDivision(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            double dr;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                dr = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    dr /= temp.iValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    dr /= temp.dValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                dr = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    dr /= temp.iValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    dr /= temp.dValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

#if INLINE
        // Инлайнинг в OpNotEqual
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private JSObject OpEqual(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Bool;

            var lvt = temp.ValueType;
            switch (lvt)
            {
                case ObjectValueType.Bool:
                case ObjectValueType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    tempResult.iValue = left == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Bool:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = left == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        tempResult.iValue = 0;
                                    }
                                    else goto default;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case ObjectValueType.Int:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Double:
                                {
                                    tempResult.iValue = left == temp.dValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.Bool:
                                {
                                    tempResult.iValue = left == temp.iValue ? 1 : 0;
                                    break;
                                }
                            case ObjectValueType.String:
                                {
                                    double d = 0;
                                    int i = 0;
                                    string v = temp.oValue as string;
                                    if (Parser.ParseNumber(v, ref i, true, out d) && (i == v.Length))
                                        tempResult.iValue = left == d ? 1 : 0;
                                    else
                                        tempResult.iValue = 0;
                                    break;
                                }
                            case ObjectValueType.Object:
                                {
                                    temp = temp.ToPrimitiveValue_Value_String();
                                    if (temp.ValueType == ObjectValueType.Int)
                                    {
                                        goto case ObjectValueType.Int;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Double)
                                    {
                                        goto case ObjectValueType.Double;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Bool)
                                    {
                                        goto case ObjectValueType.Bool;
                                    }
                                    else if (temp.ValueType == ObjectValueType.String)
                                    {
                                        goto case ObjectValueType.String;
                                    }
                                    else if (temp.ValueType == ObjectValueType.Object)
                                    {
                                        tempResult.iValue = 0;
                                    }
                                    else goto default;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case ObjectValueType.String:
                    {
                        string left = temp.oValue as string;
                        temp = second.Invoke(context);
                        if (temp.ValueType == ObjectValueType.Int)
                        {
                            double d = 0;
                            int i = 0;
                            if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                tempResult.iValue = d == temp.iValue ? 1 : 0;
                            else
                                tempResult.iValue = 0;
                        }
                        else if (temp.ValueType == ObjectValueType.Double)
                        {
                            double d = 0;
                            int i = 0;
                            if (Parser.ParseNumber(left, ref i, true, out d) && (i == left.Length))
                                tempResult.iValue = d == temp.dValue ? 1 : 0;
                            else
                                tempResult.iValue = 0;
                        }
                        else if (temp.ValueType == ObjectValueType.String)
                        {
                            tempResult.iValue = left == temp.oValue as string ? 1 : 0;
                        }
                        else goto default;
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        var pv = temp.ToPrimitiveValue_Value_String();
                        if (pv.ValueType == ObjectValueType.Int)
                        {
                            temp = pv;
                            goto case ObjectValueType.Int;
                        }
                        else if (pv.ValueType == ObjectValueType.Double)
                        {
                            temp = pv;
                            goto case ObjectValueType.Double;
                        }
                        else if (pv.ValueType == ObjectValueType.Bool)
                        {
                            temp = pv;
                            goto case ObjectValueType.Bool;
                        }
                        else if (pv.ValueType == ObjectValueType.String)
                        {
                            temp = pv;
                            goto case ObjectValueType.String;
                        }
                        else
                        {
                            var left = temp;
                            temp = second.Invoke(context);
                            if (temp.ValueType == ObjectValueType.Object || temp.ValueType == ObjectValueType.Date)
                                tempResult.iValue = left.oValue == temp.oValue ? 1 : 0;
                            else if (temp.ValueType == ObjectValueType.Undefined && left.oValue == null)
                                tempResult.iValue = 1;
                            else
                                goto default;
                        }
                        break;
                    }
                case ObjectValueType.NoExistInObject:
                case ObjectValueType.Undefined:
                    {
                        var left = temp;
                        temp = second.Invoke(context);
                        tempResult.iValue = temp.ValueType == ObjectValueType.Undefined || temp.ValueType == ObjectValueType.NoExistInObject ? 1 : 0;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }

        private JSObject OpLogicalOr(Context context)
        {
            var left = first.Invoke(context);

            if (left)
                return left;
            else
                return second.Invoke(context);
        }

        private JSObject OpLogicalAnd(Context context)
        {
            var left = first.Invoke(context);
            if (!left)
                return left;
            else
                return second.Invoke(context);
        }

        private JSObject OpXor(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int || temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = (int)left ^ temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left ^ (int)temp.dValue;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = (int)left ^ (int)temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left ^ (int)temp.dValue;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpOr(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int || temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = (int)left | (int)temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left | (int)temp.dValue;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = (int)left | (int)temp.iValue;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = (int)left | (int)temp.dValue;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpUnsignedShiftLeft(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                int val = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((uint)(val) << temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((uint)(val) << (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                int val = (int)temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((uint)(val) << temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((uint)(val) << (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

        private JSObject OpUnsignedShiftRight(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                int val = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((uint)(val) >> temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((uint)(val) >> (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                int val = (int)temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((uint)(val) >> temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((uint)(val) >> (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

        private JSObject OpSignedShiftRight(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                int val = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((val) >> temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((val) >> (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                int val = (int)temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((val) >> temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((val) >> (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

        private JSObject OpSignedShiftLeft(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Int;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                int val = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((val) << temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((val) << (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                int val = (int)temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    val = (int)((val) << temp.iValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    val = (int)((val) << (int)temp.dValue);
                    tempResult.iValue = val;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

        private JSObject OpMod(Context context)
        {
            JSObject temp;
            temp = first.Invoke(context);

            double dr;
            ObjectValueType lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                dr = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    dr %= temp.iValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    dr %= temp.dValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
            }
            else if (lvt == ObjectValueType.Double)
            {
                dr = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                {
                    dr %= temp.iValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
                else if (temp.ValueType == ObjectValueType.Double)
                {
                    dr %= temp.dValue;
                    tempResult.ValueType = ObjectValueType.Double;
                    tempResult.dValue = dr;
                    return tempResult;
                }
            }
            throw new NotImplementedException();
        }

        private JSObject OpMore(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Bool;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int || temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = left > temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left > temp.dValue ? 1 : 0;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = left > temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left > temp.dValue ? 1 : 0;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpLessOrEqual(Context context)
        {
            var temp = first.Invoke(context);
            tempResult.ValueType = ObjectValueType.Bool;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int || temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = left <= temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left <= temp.dValue ? 1 : 0;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = left <= temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left <= temp.dValue ? 1 : 0;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpMoreOrEqual(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = ObjectValueType.Bool;

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int || lvt == ObjectValueType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int || temp.ValueType == ObjectValueType.Bool)
                    tempResult.iValue = left >= temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left >= temp.dValue ? 1 : 0;
            }
            else if (lvt == ObjectValueType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    tempResult.iValue = left >= temp.iValue ? 1 : 0;
                else if (temp.ValueType == ObjectValueType.Double)
                    tempResult.iValue = left >= temp.dValue ? 1 : 0;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpStrictNotEqual(Context context)
        {
            var temp = first.Invoke(context);

            var lvt = temp.ValueType;
            if (lvt == ObjectValueType.Int)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Double)
                    return l == temp.dValue;
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.iValue;
            }
            if (lvt == ObjectValueType.Double)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == ObjectValueType.Int)
                    return l != temp.iValue;
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.dValue;
            }
            if (lvt == ObjectValueType.Bool)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.iValue;
            }
            if (lvt == ObjectValueType.Statement)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.oValue;
            }
            if (lvt == ObjectValueType.Object)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.oValue;
            }
            if (lvt == ObjectValueType.String)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return !l.Equals(temp.oValue);
            }
            if (lvt == ObjectValueType.Undefined)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return false;
            }
            throw new NotImplementedException();
        }

        private JSObject OpNotEqual(Context context)
        {
            var t = OpEqual(context);
            t.iValue ^= 1;
            return t;
        }

        private JSObject OpTypeOf(Context context)
        {
            var val = first.Invoke(context);
            JSObject o = null;
            if (val.temporary)
                o = val;
            else
                o = tempResult;
            var vt = val.ValueType;
            o.ValueType = ObjectValueType.String;
            switch (vt)
            {
                case ObjectValueType.Int:
                case ObjectValueType.Double:
                    {
                        o.oValue = "number";
                        break;
                    }
                case ObjectValueType.NoExist:
                case ObjectValueType.NoExistInObject:
                case ObjectValueType.Undefined:
                    {
                        o.oValue = "undefined";
                        break;
                    }
                case ObjectValueType.String:
                    {
                        o.oValue = "string";
                        break;
                    }
                case ObjectValueType.Bool:
                    {
                        o.oValue = "boolean";
                        break;
                    }
                case ObjectValueType.Statement:
                    {
                        o.oValue = "function";
                        break;
                    }
                case ObjectValueType.Date:
                case ObjectValueType.Object:
                    {
                        o.oValue = "object";
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return o;
        }

        private JSObject OpInstanceOf(Context context)
        {
            var a = first.Invoke(context);
            var c = second.Invoke(context).GetField("prototype");
            JSObject o = tempResult;
            o.ValueType = ObjectValueType.Bool;
            o.iValue = 0;
            if (c.ValueType == ObjectValueType.Object)
                while (a.ValueType == ObjectValueType.Object)
                {
                    if (a.oValue == c.oValue)
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true);
                }
            return o;
        }

        private JSObject OpNew(Context context)
        {
            JSObject args = null;
            Statement[] sps = null;
            JSObject temp = first.Invoke(context);
            if (temp.ValueType <= ObjectValueType.NoExistInObject)
                throw new ArgumentException("varible is not defined");
            if (temp.ValueType != ObjectValueType.Statement)
                throw new ArgumentException(temp + " is not callable");
            var stat = temp.oValue;
            if (second != null)
            {
                args = second.Invoke(context);
                sps = args.oValue as Statement[];
            }
            JSObject _this = new JSObject();
            _this.Assign(temp.GetField("prototype", true));
            _this = new JSObject() { ValueType = ObjectValueType.Object, prototype = _this, oValue = _this.oValue };
            IContextStatement[] stmnts = null;
            if ((sps != null) && (sps.Length != 0))
            {
                stmnts = new ContextStatement[sps.Length];
                for (int i = 0; i < sps.Length; i++)
                    stmnts[i] = sps[i].Implement(context);
            }
            (stat as IContextStatement).Invoke(_this, stmnts);
            return _this;
        }

        private JSObject OpNot(Context context)
        {
            var val = first.Invoke(context);
            JSObject o = tempResult;
            var vt = val.ValueType;
            o.ValueType = ObjectValueType.Int;
            if (vt == ObjectValueType.Int || vt == ObjectValueType.Bool)
                o.iValue = val.iValue ^ -1;
            else if (vt == ObjectValueType.Double)
                o.iValue = (int)val.dValue ^ -1;
            else
                o.iValue = -1;
            return o;
        }

        private JSObject OpTernary(Context context)
        {
            var threads = ((second as ImmidateValueStatement).Value.oValue as Statement[]);
            if (first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        private JSObject OpLogicalNot(Context context)
        {
            var val = first.Invoke(context);
            JSObject o = tempResult;
            var vt = val.ValueType;
            o.ValueType = ObjectValueType.Bool;
            if (vt == ObjectValueType.Int || vt == ObjectValueType.Bool)
                o.iValue = val.iValue == 0 ? 1 : 0;
            else if (vt == ObjectValueType.Double)
                o.iValue = val.dValue == 0.0 ? 1 : 0;
            else if (vt == ObjectValueType.String)
                o.iValue = string.IsNullOrEmpty(val.oValue as string) ? 1 : 0;
            else if (vt == ObjectValueType.Object)
                o.iValue = val.oValue == null ? 1 : 0;
            else throw new NotImplementedException();
            return o;
        }

        public override JSObject Invoke(Context context)
        {
            return del(context);
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            if (fastImpl != null)
            {
                _this = fastImpl;
                return true;
            }
            else
            {
                if (first is IOptimizable)
                    (first as IOptimizable).Optimize(ref first, depth + 1, vars);
                if (second is IOptimizable)
                    (second as IOptimizable).Optimize(ref second, depth + 1, vars);
                if (type == OperationType.None && second == null && first is ImmidateValueStatement)
                {
                    _this = first;
                    return true;
                }
                if (((type == OperationType.Incriment) || (type == OperationType.Decriment)) && (depth != 0))
                {
                    if (second == null)
                        return false;
                    first = second;
                    second = null;
                }
                return false;
            }
        }
    }
}