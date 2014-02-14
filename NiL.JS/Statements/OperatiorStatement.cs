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
        None = 0x0,
        Assign = 0x10,
        Choice = 0x20,
        Logic0 = 0x30,
        Logic1 = 0x40,
        Logic2 = 0x50,
        Bit = 0x60,
        Arithmetic0 = 0x70,
        Arithmetic1 = 0x80,
        Unary = 0x90,
        Special = 0xF0
    }

    internal enum OperationType : int
    {
        None = OperationTypeGroups.None + 0,
        Assign = OperationTypeGroups.Assign + 0,
        Ternary = OperationTypeGroups.Choice + 0,
        And = OperationTypeGroups.Logic0 + 0,
        Or = OperationTypeGroups.Logic0 + 1,
        Xor = OperationTypeGroups.Logic0 + 2,
        LogicalAnd = OperationTypeGroups.Logic0 + 3,
        LogicalOr = OperationTypeGroups.Logic0 + 4,
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
        private Statement fastImpl;

        public Statement First { get { return first; } }
        public Statement Second { get { return second; } }

        private OperationType _type;
        public OperationType Type
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
                            fastImpl = new Operators.Mul(first, second);
                            break;
                        }
                    case OperationType.None:
                        {
                            fastImpl = new Operators.None(first, second);
                            break;
                        }
                    case OperationType.Assign:
                        {
                            fastImpl = new Operators.Assign(first, second);
                            break;
                        }
                    case OperationType.Less:
                        {
                            fastImpl = new Operators.Less(first, second);
                            break;
                        }
                    case OperationType.Incriment:
                        {
                            fastImpl = new Operators.Incriment(first, second);
                            break;
                        }
                    case OperationType.Call:
                        {
                            fastImpl = new Operators.Call(first, second);
                            break;
                        }
                    case OperationType.Decriment:
                        {
                            fastImpl = new Operators.Decriment(first, second);
                            break;
                        }
                    case OperationType.LessOrEqual:
                        {
                            fastImpl = new Operators.LessOrEqual(first, second);
                            break;
                        }
                    case OperationType.Addition:
                        {
                            fastImpl = new Operators.Addition(first, second);
                            break;
                        }
                    case OperationType.StrictNotEqual:
                        {
                            fastImpl = new Operators.StrictNotEqual(first, second);
                            break;
                        }
                    case OperationType.More:
                        {
                            fastImpl = new Operators.More(first, second);
                            break;
                        }
                    case OperationType.MoreOrEqual:
                        {
                            fastImpl = new Operators.MoreOrEqual(first, second);
                            break;
                        }
                    case OperationType.Division:
                        {
                            fastImpl = new Operators.Division(first, second);

                            break;
                        }
                    case OperationType.Equal:
                        {
                            fastImpl = new Operators.Equal(first, second);
                            break;
                        }
                    case OperationType.Substract:
                        {
                            fastImpl = new Operators.Substract(first, second);
                            break;
                        }
                    case OperationType.StrictEqual:
                        {
                            fastImpl = new Operators.StrictEqual(first, second);
                            break;
                        }
                    case OperationType.LogicalOr:
                        {
                            fastImpl = new Operators.LogicalOr(first, second);
                            break;
                        }
                    case OperationType.LogicalAnd:
                        {
                            fastImpl = new Operators.LogicalAnd(first, second);
                            break;
                        }
                    case OperationType.NotEqual:
                        {
                            fastImpl = new Operators.NotEqual(first, second);
                            break;
                        }
                    case OperationType.UnsignedShiftLeft:
                        {
                            fastImpl = new Operators.UnsignedShiftLeft(first, second);
                            break;
                        }
                    case OperationType.UnsignedShiftRight:
                        {
                            fastImpl = new Operators.UnsignedShiftRight(first, second);
                            break;
                        }
                    case OperationType.SignedShiftLeft:
                        {
                            fastImpl = new Operators.SignedShiftLeft(first, second);
                            break;
                        }
                    case OperationType.SignedShiftRight:
                        {
                            fastImpl = new Operators.SignedShiftRight(first, second);
                            break;
                        }
                    case OperationType.Module:
                        {
                            fastImpl = new Operators.Mod(first, second);
                            break;
                        }
                    case OperationType.LogicalNot:
                        {
                            fastImpl = new Operators.LogicalNot(first, second);
                            break;
                        }
                    case OperationType.Not:
                        {
                            fastImpl = new Operators.Not(first, second);
                            break;
                        }
                    case OperationType.Xor:
                        {
                            fastImpl = new Operators.Xor(first, second);
                            break;
                        }
                    case OperationType.Or:
                        {
                            fastImpl = new Operators.Or(first, second);
                            break;
                        }
                    case OperationType.And:
                        {
                            fastImpl = new Operators.And(first, second);
                            break;
                        }
                    case OperationType.Ternary:
                        {
                            fastImpl = new Operators.Ternary(first, second);
                            break;
                        }
                    case OperationType.TypeOf:
                        {
                            fastImpl = new Operators.TypeOf(first, second);
                            break;
                        }
                    case OperationType.New:
                        {
                            fastImpl = new Operators.New(first, second);
                            break;
                        }
                    case OperationType.Delete:
                        {
                            fastImpl = new Operators.Delete(first, second);
                            break;
                        }
                    case OperationType.InstanceOf:
                        {
                            fastImpl = new Operators.InstanceOf(first, second);
                            break;
                        }
                    case OperationType.In:
                        {
                            fastImpl = new Operators.In(first, second);
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

        public OperatorStatement()
        {
        }

        private static Statement deicstra(OperatorStatement statement)
        {
            Stack<Statement> stats = new Stack<Statement>();
            Stack<OperationType> types = new Stack<OperationType>();
            Stack<OpDelegate> delegates = new Stack<OpDelegate>();
            OperatorStatement cur = statement.second as OperatorStatement;
            if (cur == null)
                return statement;
            types.Push(statement._type);
            stats.Push(statement.first);
            while (cur != null)
            {
                stats.Push(cur.first);
                for (; types.Count > 0; )
                {
                    var topType = (int)types.Peek();
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._type & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._type & (int)OperationTypeGroups.Special))
                            && (((int)cur._type & (int)OperationTypeGroups.Special) > 0x10)))
                    {
                        stats.Push(new OperatorStatement()
                        {
                            _type = types.Pop(),
                            second = stats.Pop(),
                            first = stats.Pop()
                        });
                    }
                    else
                        break;
                }
                types.Push(cur._type);
                if (!(cur.second is OperatorStatement))
                    stats.Push(cur.second);
                cur = cur.second as OperatorStatement;
            }
            while (stats.Count > 1)
                stats.Push(new OperatorStatement()
                {
                    _type = types.Pop(),
                    second = stats.Pop(),
                    first = stats.Pop()
                });
            return stats.Peek();
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, true, false, false, true);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, bool processComma)
        {
            return Parse(state, ref index, processComma, false, false, true);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary)
        {
            return Parse(state, ref index, processComma, forUnary, false, true);
        }

        private static ParseResult Parse(ParsingState state, ref int index, bool processComma, bool forUnary, bool forNew, bool root)
        {
            string code = state.Code;
            int i = index;
            OperationType type = OperationType.None;
            Statement first = null;
            Statement second = null;
            int s = i;
            state.InExpression = true;
            if (Parser.ValidateName(code, ref i, true, state.strict.Peek()) || Parser.Validate(code, "this", ref i))
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
                            if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                first = new ImmidateValueStatement(n);
                            else
                                first = new ImmidateValueStatement(d);
                        }
                        else if (Parser.ValidateRegex(code, ref s, true, true))
                        {
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            first = new Operators.Call(new GetVaribleStatement("RegExp"), new ImmidateValueStatement(new Statement[2]
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
                                first = Parse(state, ref i, true, true, false, true).Statement;
                                if (first == null || (first as OperatorStatement)._type != OperationType.None)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation")));
                                (first as OperatorStatement)._type = OperationType.Incriment;
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
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
                                first = Parse(state, ref i, true, true, false, true).Statement;
                                if (first == null || (first as OperatorStatement)._type != OperationType.None)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation")));
                                (first as OperatorStatement)._type = OperationType.Decriment;
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
                                index = i;
                                first = new Operators.Mul(new ImmidateValueStatement(-1), f);
                            }
                            break;
                        }
                    case '!':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new OperatorStatement() { first = Parse(state, ref i, true, true, false, true).Statement, _type = OperationType.LogicalNot };
                            break;
                        }
                    case '~':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, true, true, false, true).Statement;
                            if ((first as OperatorStatement)._type == OperationType.None)
                                (first as OperatorStatement)._type = OperationType.Not;
                            else
                                first = new OperatorStatement() { first = first, _type = OperationType.Not };
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, false, true).Statement;
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
                            first = new Operators.None(Parse(state, ref i, false, true, false, true).Statement, new ImmidateValueStatement(JSObject.undefined));
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, true, true).Statement;
                            if ((first as OperatorStatement)._type == OperationType.None || (first as OperatorStatement)._type == OperationType.Call)
                                (first as OperatorStatement)._type = OperationType.New;
                            else
                                first = new Operators.New(first, second);
                            break;
                        }
                    case 'd':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, false, true).Statement;
                            if ((first as OperatorStatement)._type == OperationType.None)
                                (first as OperatorStatement)._type = OperationType.Delete;
                            else
                                first = new Operators.Delete(first, second);
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
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\"")));
                i++;
            }
            else
                first = Parser.Parse(state, ref i, 2);
            if (first is EmptyStatement)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid operator argument at " + Tools.PositionToTextcord(code, i))));
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
                            if (forUnary || !processComma)
                            {
                                binar = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            type = OperationType.None;
                            binar = true;
                            repeat = false;
                            break;
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
                            state.InExpression = true;
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
                            if (!Parser.ValidateName(code, ref i, true, false, true, state.strict.Peek()))
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
                            repeat = !forNew;
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
                            throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid operator '" + code[i] + "' at " + Tools.PositionToTextcord(code, i))));
                        }
                }
            } while (repeat);
            if ((!canAsign) && ((type == OperationType.Assign) || (assign)))
                throw new InvalidOperationException("invalid left-hand side in assignment");
            if (binar && !forUnary)
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                second = OperatorStatement.Parse(state, ref i, processComma, false, false, false).Statement;
            }
            OperatorStatement res = null;
            if (first == second && first == null)
                return new ParseResult();
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
            state.InExpression = !root;
            return new ParseResult()
            {
                Statement = res,
                Message = "",
                IsParsed = true
            };
        }

        public override JSObject Invoke(Context context)
        {
            throw new InvalidOperationException();
        }

        public bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            Type = Type;
            _this = fastImpl;
            return true;
        }
    }
}