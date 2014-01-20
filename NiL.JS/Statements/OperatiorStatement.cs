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
        Logic0 = 0x10,
        Logic1 = 0x20,
        Logic2 = 0x30,
        Bit = 0x40,
        Arithmetic0 = 0x50,
        Arithmetic1 = 0x60,
        Unary = 0x70,
        Special = 0xF0
    }

    internal enum OperationType : int
    {
        None = OperationTypeGroups.None + 0,
        Assign = OperationTypeGroups.None + 1,
        Ternary = OperationTypeGroups.None + 2,
        And = OperationTypeGroups.Logic0 + 0,
        Or = OperationTypeGroups.Logic0 + 1,
        Xor = OperationTypeGroups.Logic0 + 2,
        LogicalAnd = OperationTypeGroups.Logic0 + 3,
        LogicalOr = OperationTypeGroups.Logic0 + 4,
        Equal = OperationTypeGroups.Logic1 + 0,
        NotEqual = OperationTypeGroups.Logic1 + 1,
        StrictEqual = OperationTypeGroups.Logic1 + 2,
        StrictNotEqual = OperationTypeGroups.Logic1 + 3,
        InstanceOf = OperationTypeGroups.Logic1 + 4,
        In = OperationTypeGroups.Logic1 + 5,
        More = OperationTypeGroups.Logic2 + 0,
        Less = OperationTypeGroups.Logic2 + 1,
        MoreOrEqual = OperationTypeGroups.Logic2 + 2,
        LessOrEqual = OperationTypeGroups.Logic2 + 3,
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
        Delete = OperationTypeGroups.Special + 3
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
                            fastImpl = new Operators.StrictNotEqual(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.More:
                        {
                            del = OpMore;
                            break;
                        }
                    case OperationType.MoreOrEqual:
                        {
                            fastImpl = new Operators.MoreOrEqual(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Division:
                        {
                            fastImpl = new Operators.Division(first, second);
                            del = fastImpl.Invoke;
                            break;
                        }
                    case OperationType.Equal:
                        {
                            fastImpl = new Operators.Equal(first, second);
                            del = fastImpl.Invoke;
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
                            fastImpl = new Operators.NotEqual(first, second);
                            del = fastImpl.Invoke;
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
                            fastImpl = new Operators.LogicalNot(first, second);
                            del = fastImpl.Invoke;
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
                            del = (fastImpl = new Operators.TypeOf(first, second)).Invoke;
                            break;
                        }
                    case OperationType.New:
                        {
                            del = OpNew;
                            break;
                        }
                    case OperationType.Delete:
                        {
                            del = (fastImpl = new Operators.Delete(first, second)).Invoke;
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

        private static Statement deicstra(OperatorStatement statement)
        {
            Stack<Statement> stats = new Stack<Statement>();
            Stack<OperationType> types = new Stack<OperationType>();
            Stack<OpDelegate> delegates = new Stack<OpDelegate>();
            OperatorStatement cur = statement.second as OperatorStatement;
            if (cur == null || cur._type == OperationType.None)
                return statement;
            types.Push(statement._type);
            delegates.Push(statement.del);
            stats.Push(statement.first);
            while (cur != null)
            {
                stats.Push(cur.first);
                if ((((int)types.Peek() & (int)OperationTypeGroups.Special) > ((int)cur._type & (int)OperationTypeGroups.Special))
                    || ((((int)types.Peek() & (int)OperationTypeGroups.Special) == ((int)cur._type & (int)OperationTypeGroups.Special)) && (((int)cur._type & (int)OperationTypeGroups.Special) != 0)))
                {
                    stats.Push(new OperatorStatement()
                    {
                        _type = types.Pop(),
                        del = delegates.Pop(),
                        second = stats.Pop(),
                        first = stats.Pop()
                    });
                }
                if (cur._type == OperationType.None)
                    break;
                types.Push(cur._type);
                delegates.Push(cur.del);
                if (!(cur.second is OperatorStatement))
                    stats.Push(cur.second);
                cur = cur.second as OperatorStatement;
            }
            while (stats.Count > 1)
                stats.Push(new OperatorStatement()
                {
                    _type = types.Pop(),
                    del = delegates.Pop(),
                    second = stats.Pop(),
                    first = stats.Pop()
                });
            return stats.Peek();
        }

        public static Statement ParseForUnary(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, true, true).Statement;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, false, true);
        }

        public static ParseResult Parse(ParsingState state, ref int index, bool processComma)
        {
            return Parse(state, ref index, processComma, false, true);
        }

        private static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary, bool root)
        {
            string code = state.Code;
            int i = index;
            OperationType type = OperationType.None;
            Statement first = null;
            Statement second = null;
            int s = i;
            if (Parser.ValidateName(code, ref i, true) || Parser.Validate(code, "this", ref i))
                first = new GetVaribleStatement(Tools.Unescape(code.Substring(s, i - s)));
            else if (Parser.ValidateValue(code, ref i, true))
            {
                string value = code.Substring(s, i - s);
                if ((value[0] == '\'') || (value[0] == '"'))
                    first = new ImmidateValueStatement(Tools.Unescape(value.Substring(1, value.Length - 2)));
                else
                {
                    bool b = false;
                    if (value == "null")
                        first = new ImmidateValueStatement(JSObject.Null);
                    else if (bool.TryParse(value, out b))
                        first = new ImmidateValueStatement(b);
                    else
                    {
                        int n = 0;
                        double d = 0;
                        if (Tools.ParseNumber(code, ref s, true, out d))
                        {
                            if ((n = (int)d) == d && d != -0)
                                first = new ImmidateValueStatement(n);
                            else
                                first = new ImmidateValueStatement(d);
                        }
                        else if (Parser.ValidateRegex(code, ref s, true, true))
                        {
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            first = new Operators.Call(new GetVaribleStatement("RegExp"), new ImmidateValueStatement(new JSObject[2]
                            {
                                value.Substring(1, s - 2),
                                flags
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
                || (code[i] == 'd' && code.Substring(i, 6) == "delete")
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
                                first = Parse(state, ref i, true, true, true).Statement;
                                if ((first as OperatorStatement)._type != OperationType.None)
                                    throw new InvalidOperationException("Invalid prefix operation");
                                (first as OperatorStatement)._type = OperationType.Incriment;
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, true).Statement;
                                index = i;
                                first = new Operators.Mul(new ImmidateValueStatement(1), f);
                            }
                            break;
                        }
                    case '-':
                        {
                            i++;
                            if (code[i] == '-')
                            {
                                do i++; while (char.IsWhiteSpace(code[i]));
                                first = Parse(state, ref i, true, true, true).Statement;
                                if ((first as OperatorStatement)._type != OperationType.None)
                                    throw new InvalidOperationException("Invalid prefix operation");
                                (first as OperatorStatement)._type = OperationType.Decriment;
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, true).Statement;
                                index = i;
                                first = new Operators.Mul(new ImmidateValueStatement(-1), f);
                            }
                            break;
                        }
                    case '!':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new OperatorStatement() { first = Parse(state, ref i, true, true, true).Statement, _type = OperationType.LogicalNot };
                            break;
                        }
                    case '~':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, true, true, true).Statement;
                            (first as OperatorStatement)._type = OperationType.Not;
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, true).Statement;
                            if ((first as OperatorStatement)._type == OperationType.None)
                                (first as OperatorStatement)._type = OperationType.TypeOf;
                            else
                                first = new Operators.TypeOf(first, second);
                            break;
                        }
                    case 'v':
                        {
                            i += 3;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new OperatorStatement() { first = Parse(state, ref i, false, true, true).Statement, second = new ImmidateValueStatement(JSObject.undefined), _type = OperationType.None };
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, true).Statement;
                            (first as OperatorStatement)._type = OperationType.New;
                            break;
                        }
                    case 'd':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, true).Statement;
                            (first as OperatorStatement)._type = OperationType.Delete;
                            break;
                        }
                    default:
                        throw new NotImplementedException("Unary operator " + code[i]);
                }
            }
            else if (code[i] == '(')
            {
                do i++; while (char.IsWhiteSpace(code[i]));
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
                while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                rollbackPos = i;
                while (char.IsWhiteSpace(code[i])) i++;
                switch (code[i])
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
                                first = new OperatorStatement() { second = first, _type = OperationType.Incriment };
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
                                first = new OperatorStatement() { second = first, _type = OperationType.Decriment };
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
                            {
                                type = OperationType.Less;
                                if (code[i + 1] == '=')
                                {
                                    type = OperationType.LessOrEqual;
                                    i++;
                                }
                            }
                            if (code[i + 1] == '=')
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
                                if (code[i + 1] == '=')
                                {
                                    type = OperationType.MoreOrEqual;
                                    i++;
                                }
                            }
                            if (code[i + 1] == '=')
                            {
                                assign = true;
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
                            if (!Parser.ValidateName(code, ref i, true, false))
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
                                if ((args[args.Count - 1] is OperatorStatement) && (args[args.Count - 1] as OperatorStatement)._type == OperationType.None)
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
                                _type = OperationType.Call
                            };
                            i++;
                            repeat = !forUnary;
                            canAsign = false;
                            binar = false;
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
                            if (Tools.isLineTerminator(code[i]))
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
            if (binar && !forUnary)
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                second = OperatorStatement.Parse(state, ref i, false, false, false).Statement;
            }
            if (processComma && (code[i] == ','))
            {
                first = deicstra(new OperatorStatement() { first = first, second = second, _type = type });
                type = OperationType.None;
                do i++; while (char.IsWhiteSpace(code[i]));
                second = OperatorStatement.Parse(state, ref i).Statement;
            }
            OperatorStatement res = null;
            if (assign)
                res = new OperatorStatement() { first = first, second = new OperatorStatement() { first = first, second = second, _type = type }, _type = OperationType.Assign };
            else
            {
                if (forUnary && (type == OperationType.None) && (first is OperatorStatement))
                    res = first as OperatorStatement;
                else
                    res = new OperatorStatement() { first = first, second = second, _type = type };
            }
            index = i;
            if (root)
                res = deicstra(res) as OperatorStatement;
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
            switch (temp.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        var type = temp.ValueType;
                        dr = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType >= JSObjectType.Object)
                            temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                        {
                            dr += temp.iValue;
                            tempResult.ValueType = JSObjectType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.Double)
                        {
                            dr += temp.dValue;
                            tempResult.ValueType = JSObjectType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.String)
                        {
                            tempResult.oValue = (type == JSObjectType.Bool ? (dr != 0 ? "true" : "false") : dr.ToString()) + (string)temp.oValue;
                            tempResult.ValueType = JSObjectType.String;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject)
                        {
                            tempResult.dValue = double.NaN;
                            tempResult.ValueType = JSObjectType.Double;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.Object) // x+null
                        {
                            tempResult.dValue = dr;
                            tempResult.ValueType = JSObjectType.Double;
                            return tempResult;
                        }
                        break;
                    }
                case JSObjectType.Double:
                    {
                        dr = temp.dValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType >= JSObjectType.Object)
                            temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                        {
                            dr += temp.iValue;
                            tempResult.ValueType = JSObjectType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.Double)
                        {
                            dr += temp.dValue;
                            tempResult.ValueType = JSObjectType.Double;
                            tempResult.dValue = dr;
                            return tempResult;
                        }
                        else if (temp.ValueType == JSObjectType.String)
                        {
                            tempResult.oValue = dr.ToString() + (string)temp.oValue;
                            tempResult.ValueType = JSObjectType.String;
                            return tempResult;
                        }
                        break;
                    }
                case JSObjectType.String:
                    {
                        var val = temp.oValue as string;
                        temp = second.Invoke(context);
                        if (temp.ValueType >= JSObjectType.Object)
                            temp = temp.ToPrimitiveValue_Value_String(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.Int:
                                {
                                    val += temp.iValue;
                                    break;
                                }
                            case JSObjectType.Double:
                                {
                                    val += temp.dValue;
                                    break;
                                }
                            case JSObjectType.Bool:
                                {
                                    val += temp.iValue != 0 ? "true" : "false";
                                    break;
                                }
                            case JSObjectType.String:
                                {
                                    val += temp.oValue;
                                    break;
                                }
                            case JSObjectType.Undefined:
                            case JSObjectType.NotExistInObject:
                                {
                                    val += "undefined";
                                    break;
                                }
                            case JSObjectType.Object:
                            case JSObjectType.Proxy:
                            case JSObjectType.Function:
                            case JSObjectType.Date:
                                {
                                    val += "null";
                                    break;
                                }
                        }
                        tempResult.oValue = val;
                        tempResult.ValueType = JSObjectType.String;
                        return tempResult;
                    }
                case JSObjectType.Date:
                    {
                        var val = temp.ToPrimitiveValue_String_Value(context);
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.String:
                                {
                                    tempResult.ValueType = JSObjectType.String;
                                    tempResult.oValue = val.oValue as string + temp.oValue as string;
                                    return tempResult;
                                }
                            case JSObjectType.Int:
                                {
                                    tempResult.ValueType = JSObjectType.String;
                                    tempResult.oValue = val.oValue as string + tempResult.iValue;
                                    return tempResult;
                                }
                            case JSObjectType.Bool:
                                {
                                    tempResult.ValueType = JSObjectType.String;
                                    tempResult.oValue = val.oValue as string + (tempResult.iValue != 0);
                                    return tempResult;
                                }
                            case JSObjectType.Double:
                                {
                                    tempResult.ValueType = JSObjectType.String;
                                    tempResult.oValue = val.oValue as string + tempResult.dValue;
                                    return tempResult;
                                }
                        }
                        break;
                    }
                case JSObjectType.NotExistInObject:
                case JSObjectType.Undefined:
                    {
                        var val = "undefined";
                        temp = second.Invoke(context);
                        if (temp.ValueType >= JSObjectType.Object)
                            temp = temp.ToPrimitiveValue_Value_String(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.String:
                                {
                                    tempResult.ValueType = JSObjectType.String;
                                    tempResult.oValue = val as string + temp.oValue as string;
                                    return tempResult;
                                }
                            case JSObjectType.Double:
                            case JSObjectType.Bool:
                            case JSObjectType.Int:
                                {
                                    tempResult.ValueType = JSObjectType.Double;
                                    tempResult.dValue = double.NaN;
                                    return tempResult;
                                }
                            case JSObjectType.Object: // undefined+null
                            case JSObjectType.NotExistInObject:
                            case JSObjectType.Undefined:
                                {
                                    tempResult.ValueType = JSObjectType.Double;
                                    tempResult.dValue = double.NaN;
                                    return tempResult;
                                }
                        }
                        break;
                    }
                case JSObjectType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                            goto case JSObjectType.Int;
                        else if (temp.ValueType == JSObjectType.Object)
                        {
                            temp = second.Invoke(context);
                            if (temp.ValueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String(context);
                            if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                            {
                                tempResult.ValueType = JSObjectType.Int;
                                tempResult.iValue = temp.iValue;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.Double)
                            {
                                tempResult.ValueType = JSObjectType.Double;
                                tempResult.dValue = temp.dValue;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.String)
                            {
                                tempResult.oValue = "null" + (string)temp.oValue;
                                tempResult.ValueType = JSObjectType.String;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.Undefined || temp.ValueType == JSObjectType.NotExistInObject)
                            {
                                tempResult.dValue = double.NaN;
                                tempResult.ValueType = JSObjectType.Double;
                                return tempResult;
                            }
                            else if (temp.ValueType == JSObjectType.Object) // null+null
                            {
                                tempResult.iValue = 0;
                                tempResult.ValueType = JSObjectType.Int;
                                return tempResult;
                            }
                        }
                        else if (temp.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (temp.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined");
            }
            throw new NotImplementedException();
        }

        private JSObject OpSubstract(Context context)
        {
            double left = Tools.JSObjectToDouble(first.Invoke(context));
            tempResult.dValue = left - Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        private JSObject OpAnd(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = left & Tools.JSObjectToInt(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpDecriment(Context context)
        {
            var val = (first ?? second).Invoke(context);
            if (val.ValueType == JSObjectType.NotExist)
                throw new InvalidOperationException("varible is undefined");
            if ((val.assignCallback != null) && (!val.assignCallback()))
                return double.NaN;

            JSObject o = null;
            if ((second != null) && (val.ValueType != JSObjectType.Undefined) && (val.ValueType != JSObjectType.NotExistInObject))
            {
                o = tempResult;
                o.Assign(val);
            }
            else
                o = val;
            if (val.ValueType == JSObjectType.Int)
                val.iValue--;
            else if (val.ValueType == JSObjectType.Double)
                val.dValue = val.dValue--;
            else if (val.ValueType == JSObjectType.Bool)
            {
                val.ValueType = JSObjectType.Int;
                val.iValue--;
            }
            else if (val.ValueType == JSObjectType.String)
            {
                double resd;
                int i = 0;
                if (!Tools.ParseNumber(val.oValue as string, ref i, false, out resd))
                    resd = double.NaN;
                resd++;
                val.ValueType = JSObjectType.Double;
                val.dValue = resd;
            }
            else if (val.ValueType == JSObjectType.Undefined || val.ValueType == JSObjectType.NotExistInObject)
            {
                val.ValueType = JSObjectType.Double;
                val.dValue = double.NaN;
            }
            else throw new NotImplementedException();
            return o;
        }

        private JSObject OpLogicalOr(Context context)
        {
            var left = first.Invoke(context);

            if ((bool)left)
                return left;
            else
                return second.Invoke(context);
        }

        private JSObject OpLogicalAnd(Context context)
        {
            var left = first.Invoke(context);
            if (!(bool)left)
                return left;
            else
                return second.Invoke(context);
        }

        private JSObject OpXor(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = left ^ Tools.JSObjectToInt(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpOr(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = left | Tools.JSObjectToInt(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpUnsignedShiftLeft(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = (int)((uint)left << Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpUnsignedShiftRight(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.dValue = (double)((uint)left >> Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        private JSObject OpSignedShiftRight(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = (int)(left >> Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpSignedShiftLeft(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = (int)(left << Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpMod(Context context)
        {
            double left = Tools.JSObjectToDouble(first.Invoke(context));
            tempResult.dValue = left % Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        private JSObject OpMore(Context context)
        {
            var temp = first.Invoke(context);

            tempResult.ValueType = JSObjectType.Bool;
            var lvt = temp.ValueType;
            switch (lvt)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        int left = temp.iValue;
                        temp = second.Invoke(context);
                        if (temp.ValueType == JSObjectType.Int)
                            tempResult.iValue = left > temp.iValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Double)
                            tempResult.iValue = left > temp.dValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Bool)
                            tempResult.iValue = left > temp.iValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                case JSObjectType.Double:
                    {
                        double left = temp.dValue;
                        temp = second.Invoke(context);
                        if (double.IsNaN(left))
                            tempResult.iValue = 0;
                        else if (temp.ValueType == JSObjectType.Int)
                            tempResult.iValue = left > temp.iValue ? 1 : 0;
                        else if (temp.ValueType == JSObjectType.Double)
                            tempResult.iValue = left > temp.dValue ? 1 : 0;
                        else throw new NotImplementedException();
                        break;
                    }
                case JSObjectType.String:
                    {
                        string left = temp.oValue as string;
                        temp = second.Invoke(context);
                        switch (temp.ValueType)
                        {
                            case JSObjectType.String:
                                {
                                    tempResult.iValue = string.Compare(left, temp.oValue as string) > 0 ? 1 : 0;
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        temp = temp.ToPrimitiveValue_Value_String(context);
                        if (temp.ValueType == JSObjectType.Int)
                            goto case JSObjectType.Int;
                        else if (temp.ValueType == JSObjectType.Double)
                            goto case JSObjectType.Double;
                        else if (temp.ValueType == JSObjectType.String)
                            goto case JSObjectType.String;
                        break;
                    }
                default: throw new NotImplementedException();
            }
            return tempResult;
        }

        private JSObject OpLessOrEqual(Context context)
        {
            var temp = first.Invoke(context);
            tempResult.ValueType = JSObjectType.Bool;

            var lvt = temp.ValueType;
            if (lvt == JSObjectType.Int || lvt == JSObjectType.Bool)
            {
                int left = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.Int || temp.ValueType == JSObjectType.Bool)
                    tempResult.iValue = left <= temp.iValue ? 1 : 0;
                else if (temp.ValueType == JSObjectType.Double)
                    tempResult.iValue = left <= temp.dValue ? 1 : 0;
            }
            else if (lvt == JSObjectType.Double)
            {
                double left = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.Int)
                    tempResult.iValue = left <= temp.iValue ? 1 : 0;
                else if (temp.ValueType == JSObjectType.Double)
                    tempResult.iValue = left <= temp.dValue ? 1 : 0;
            }
            else throw new NotImplementedException();
            return tempResult;
        }

        private JSObject OpStrictNotEqual(Context context)
        {
            var temp = first.Invoke(context);

            var lvt = temp.ValueType;
            if (lvt == JSObjectType.Int)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.Double)
                    return l == temp.dValue;
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.iValue;
            }
            if (lvt == JSObjectType.Double)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (temp.ValueType == JSObjectType.Int)
                    return l != temp.iValue;
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.dValue;
            }
            if (lvt == JSObjectType.Bool)
            {
                var l = temp.iValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.iValue;
            }
            if (lvt == JSObjectType.Function)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.oValue;
            }
            if (lvt == JSObjectType.Object)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return l != temp.oValue;
            }
            if (lvt == JSObjectType.String)
            {
                var l = temp.oValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return !l.Equals(temp.oValue);
            }
            if (lvt == JSObjectType.Undefined)
            {
                var l = temp.dValue;
                temp = second.Invoke(context);
                if (lvt != temp.ValueType)
                    return true;
                return false;
            }
            throw new NotImplementedException();
        }

        private JSObject OpInstanceOf(Context context)
        {
            var a = first.Invoke(context);
            var c = second.Invoke(context).GetField("prototype", true, false);
            JSObject o = tempResult;
            o.ValueType = JSObjectType.Bool;
            o.iValue = 0;
            if (c.ValueType >= JSObjectType.Object && c.oValue != null)
                while (a.ValueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == c.oValue || (c.oValue is Type && a.oValue.GetType() as object == c.oValue))
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true, false);
                }
            return o;
        }

        private JSObject OpNew(Context context)
        {
            JSObject temp = first.Invoke(context);
            if (temp.ValueType <= JSObjectType.NotExistInObject)
                throw new ArgumentException("varible is not defined");
            if (temp.ValueType != JSObjectType.Function)
                throw new ArgumentException(temp + " is not callable");
            
            var call = Operators.Call.Instance;
            (call.First as ImmidateValueStatement).Value = temp;
            if (second != null)
                (call.Second as ImmidateValueStatement).Value = second.Invoke(context);
            else
                (call.Second as ImmidateValueStatement).Value = new JSObject[0];
            JSObject _this = new JSObject()
            {
                ValueType = JSObjectType.Object,
                oValue = temp is TypeProxy ? null : new object()
            };
            if (temp is TypeProxy)
                _this.prototype = temp.GetField("prototype", true, false);
            else
                (_this.prototype = new JSObject()).Assign(temp.GetField("prototype", true, false));
            var otb = context.thisBind;
            context.thisBind = _this;
            try
            {
                call.Invoke(context);
            }
            finally
            {
                context.thisBind = otb;
            }
            return _this;
        }

        private JSObject OpNot(Context context)
        {
            tempResult.iValue = Tools.JSObjectToInt(first.Invoke(context)) ^ -1;
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        private JSObject OpTernary(Context context)
        {
            var threads = ((second as ImmidateValueStatement).Value.oValue as Statement[]);
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        public override JSObject Invoke(Context context)
        {
            return del(context);
        }

        public bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            type = type;
            if (fastImpl != null)
            {
                _this = fastImpl;
                return true;
            }
            else
            {
                if (first is IOptimizable)
                    Parser.Optimize(ref first, depth + 1, vars);
                if (second is IOptimizable)
                    Parser.Optimize(ref second, depth + 1, vars);
                if (_type == OperationType.None && second == null && first is ImmidateValueStatement)
                {
                    _this = first;
                    return true;
                }
                if (((_type == OperationType.Incriment) || (_type == OperationType.Decriment)) && (depth != 0))
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