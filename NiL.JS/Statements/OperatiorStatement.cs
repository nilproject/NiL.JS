using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;
using System.Threading;
using System.Text.RegularExpressions;

namespace NiL.JS.Statements
{
    [Serializable]
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

    [Serializable]
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

    [Serializable]
    internal sealed class OperatorStatement : Statement
    {
        private Statement fastImpl;

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
                            fastImpl = new Operators.Incriment(first ?? second, first == null ? Operators.Incriment.Type.Postincriment : Operators.Incriment.Type.Preincriment);
                            break;
                        }
                    case OperationType.Call:
                        {
                            fastImpl = new Operators.Call(first, second);
                            break;
                        }
                    case OperationType.Decriment:
                        {
                            fastImpl = new Operators.Decriment(first ?? second, first == null ? Operators.Decriment.Type.Postdecriment : Operators.Decriment.Type.Postdecriment);
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
                            fastImpl = new Operators.LogicalNot(first);
                            break;
                        }
                    case OperationType.Not:
                        {
                            fastImpl = new Operators.Not(first);
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
                            fastImpl = new Operators.TypeOf(first);
                            break;
                        }
                    case OperationType.New:
                        {
                            fastImpl = new Operators.New(first, second);
                            break;
                        }
                    case OperationType.Delete:
                        {
                            fastImpl = new Operators.Delete(first);
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
                        throw new ArgumentException("invalid operation type");
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
            if (statement == null)
                return null;
            OperatorStatement cur = statement.second as OperatorStatement;
            if (cur == null)
                return statement;
            Stack<Statement> stats = new Stack<Statement>();
            Stack<Statement> types = new Stack<Statement>();
            types.Push(statement);
            stats.Push(statement.first);
            while (cur != null)
            {
                stats.Push(cur.first);
                for (; types.Count > 0; )
                {
                    var topType = (int)(types.Peek() as OperatorStatement)._type;
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._type & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._type & (int)OperationTypeGroups.Special))
                            && (((int)cur._type & (int)OperationTypeGroups.Special) > 0x10)))
                    {
                        var stat = types.Pop() as OperatorStatement;
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
                if (!(cur.second is OperatorStatement))
                    stats.Push(cur.second);
                cur = cur.second as OperatorStatement;
            }
            while (stats.Count > 1)
            {
                var stat = types.Pop() as OperatorStatement;
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
            int position;
            OperationType type = OperationType.None;
            Statement first = null;
            Statement second = null;
            int s = i;
            state.InExpression = true;
            if (Parser.ValidateName(code, ref i, state.strict.Peek()) || Parser.Validate(code, "this", ref i))
            {
                var name = Tools.Unescape(code.Substring(s, i - s), state.strict.Peek());
                if (name == "undefined")
                    first = new ImmidateValueStatement(JSObject.undefined) { Position = index, Length = i - index };
                else
                    first = new GetVariableStatement(name) { Position = index, Length = i - index };
            }
            else if (Parser.ValidateValue(code, ref i))
            {
                string value = code.Substring(s, i - s);
                if ((value[0] == '\'') || (value[0] == '"'))
                    first = new ImmidateValueStatement(Tools.Unescape(value.Substring(1, value.Length - 2), state.strict.Peek())) { Position = index, Length = i - s };
                else
                {
                    bool b = false;
                    if (value == "null")
                        first = new ImmidateValueStatement(JSObject.Null);
                    else if (bool.TryParse(value, out b))
                        first = new ImmidateValueStatement(b) { Position = index, Length = i - s };
                    else
                    {
                        int n = 0;
                        double d = 0;
                        if (Tools.ParseNumber(code, ref s, out d, 0, !state.strict.Peek()))
                        {
                            if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                first = new ImmidateValueStatement(n) { Position = index, Length = i - index };
                            else
                                first = new ImmidateValueStatement(d) { Position = index, Length = i - index };
                        }
                        else if (Parser.ValidateRegex(code, ref s, true))
                        {
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            first = new Operators.Call(new GetVariableStatement("RegExp") { Position = i }, new ImmidateValueStatement(new JSObject()
                            {
                                valueType = JSObjectType.Object,
                                oValue = new Statement[2]
								{
									new ImmidateValueStatement(value.Substring(1, s - 2)) { Position = i , Length = s - 2 },
									new ImmidateValueStatement(flags) { Position = s  }
								}
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
                                if (((first as GetMemberStatement) as object ?? (first as GetVariableStatement)) == null)
                                {
                                    var cord = Tools.PositionToTextcord(code, i);
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableStatement) && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                first = new Operators.Incriment(first, Operators.Incriment.Type.Preincriment);
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
                                first = new Operators.Mul(new ImmidateValueStatement(1), f) { Position = index, Length = i - index };
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
                                if (((first as GetMemberStatement) as object ?? (first as GetVariableStatement)) == null)
                                {
                                    var cord = Tools.PositionToTextcord(code, i);
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableStatement) && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                first = new Operators.Decriment(first, Operators.Decriment.Type.Predecriment) { Position = index, Length = i - index };
                            }
                            else
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
                                first = new Operators.Mul(new ImmidateValueStatement(-1), f) { Position = index, Length = i - index };
                            }
                            break;
                        }
                    case '!':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new Operators.LogicalNot(Parse(state, ref i, true, true, false, true).Statement) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case '~':
                        {
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, true, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Operators.Not(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 't':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Operators.TypeOf(first) { Position = index, Length = i - index };
                            break;
                        }
                    case 'v':
                        {
                            i += 3;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = new Operators.None(Parse(state, ref i, false, true, false, true).Statement, new ImmidateValueStatement(JSObject.undefined)) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, true, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            if (first is OperatorStatement && ((first as OperatorStatement)._type == OperationType.None || (first as OperatorStatement)._type == OperationType.Call))
                                (first as OperatorStatement)._type = OperationType.New;
                            else
                                first = new Operators.New(first, second) { Position = index, Length = i - index };
                            break;
                        }
                    case 'd':
                        {
                            i += 5;
                            do i++; while (char.IsWhiteSpace(code[i]));
                            first = Parse(state, ref i, false, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Operators.Delete(first) { Position = index, Length = i - index };
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
            bool binary = false;
            bool repeat; // лёгкая замена goto. Тот самый случай, когда он уместен.
            int rollbackPos;
            do
            {
                repeat = false;
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if (code.Length <= i)
                    break;
                rollbackPos = i;
                while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
                if (code.Length <= i)
                {
                    i = rollbackPos;
                    break;
                }
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
                            if (code[i + 1] == '=')
                            {
                                i++;
                                if (code[i + 1] == '=')
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
                            else throw new ArgumentException("Invalid operator '!'");
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
                            do i++; while (char.IsWhiteSpace(code[i]));
                            position = i;
                            var sec = new Statement[]
                                {
                                    Parser.Parse(state, ref i, 1),
                                    null
                                };
                            if (code[i] != ':')
                                throw new ArgumentException("Invalid char in ternary operator");
                            do i++; while (char.IsWhiteSpace(code[i]));
                            state.InExpression = true;
                            second = new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = sec }) { Position = position };
                            sec[1] = Parser.Parse(state, ref i, 1);
                            second.Length = i - second.Position;
                            binary = false;
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
                            if (code[i + 1] == '=')
                            {
                                i++;
                                if (code[i + 1] == '=')
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
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '+')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableStatement) 
                                        && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                }
                                first = new Operators.Incriment(first, Operators.Incriment.Type.Postincriment) { Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Incriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
                            }
                            else
                            {
                                binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '-')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableStatement)
                                        && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                }
                                first = new Operators.Decriment(first, Operators.Decriment.Type.Postdecriment ){ Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Decriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
                            }
                            else
                            {
                                binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '&')
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (code[i + 1] == '|')
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
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
                                binary = false;
                                repeat = false;
                                break;
                            }
                            binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
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
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            binary = true;
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
                            binary = true;
                            i++;
                            while (char.IsWhiteSpace(code[i])) i++;
                            s = i;
                            if (!Parser.ValidateName(code, ref i, false, true, state.strict.Peek()))
                                throw new ArgumentException("code (" + i + ")");
                            string name = code.Substring(s, i - s);
                            first = new GetMemberStatement(first, new ImmidateValueStatement(name)
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
                            List<Statement> args = new List<Statement>();
                            i++;
                            int startPos = i;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                if (code[i] == ']')
                                    break;
                                else if (code[i] == ',')
                                    do i++; while (char.IsWhiteSpace(code[i]));
                                args.Add(Parser.Parse(state, ref i, 1));
                                if (args[args.Count - 1] == null)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \"]\" at " + Tools.PositionToTextcord(code, startPos))));
                                if ((args[args.Count - 1] is OperatorStatement) && (args[args.Count - 1] as OperatorStatement)._type == OperationType.None)
                                    args[args.Count - 1] = (args[args.Count - 1] as OperatorStatement).first;
                            }
                            first = new GetMemberStatement(first, args[0]) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;
                            break;
                        }
                    case '(':
                        {
                            List<Statement> args = new List<Statement>();
                            i++;
                            int startPos = i;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(code[i])) i++;
                                if (code[i] == ')')
                                    break;
                                else if (code[i] == ',')
                                    do i++; while (char.IsWhiteSpace(code[i]));
                                if (i + 1 == code.Length)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Unexpected end of line")));
                                args.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                                if (args[args.Count - 1] == null)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at " + Tools.PositionToTextcord(code, startPos))));
                            }
                            first = new OperatorStatement()
                            {
                                first = first,
                                second = new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = args.ToArray() }) { Position = startPos, Length = i - startPos },
                                _type = OperationType.Call,
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
                            if (Parser.Validate(code, "instanceof", ref i))
                            {
                                type = OperationType.InstanceOf;
                                binary = true;
                                break;
                            }
                            else if (Parser.Validate(code, "in", ref i))
                            {
                                type = OperationType.In;
                                binary = true;
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
            if (state.strict.Peek()
                && (first is GetVariableStatement) && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
            {
                if (assign || type == OperationType.Assign)
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Assignment to eval or arguments is not allowed in strict mode")));
                //if (type == OperationType.Incriment || type == OperationType.Decriment)
                //    throw new JSException(new SyntaxError("Can not " + type.ToString().ToLower() + " \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
            }
            if ((!canAsign) && ((type == OperationType.Assign) || (assign)))
                throw new InvalidOperationException("invalid left-hand side in assignment");
            if (binary && !forUnary)
            {
                do i++; while (code.Length > i && char.IsWhiteSpace(code[i]));
                if (code.Length > i)
                    second = OperatorStatement.Parse(state, ref i, processComma, false, false, false).Statement;
            }
            Statement res = null;
            if (first == second && first == null)
                return new ParseResult();
            if (assign)
                res = new OperatorStatement() { first = first, second = new OperatorStatement() { first = first, second = second, _type = type, Position = index, Length = i - index }, _type = OperationType.Assign, Position = index, Length = i - index };
            else
            {
                if (!root || type != OperationType.None || second != null)
                {
                    if (forUnary && (type == OperationType.None) && (first is OperatorStatement))
                        res = first as OperatorStatement;
                    else
                        res = new OperatorStatement() { first = first, second = second, _type = type, Position = index, Length = i - index };
                }
                else
                    res = first;
            }
            if (root)
                res = deicstra(res as OperatorStatement) ?? res;
            index = i;
            state.InExpression = !root;
            return new ParseResult()
            {
                Statement = res,
                IsParsed = true
            };
        }

        internal override JSObject Invoke(Context context)
        {
            throw new InvalidOperationException();
        }

        protected override Statement[] getChildsImpl()
        {
            throw new InvalidOperationException();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            Type = Type;
            _this = fastImpl;
            fastImpl.Position = Position;
            fastImpl.Length = Length;
            return true;
        }
    }
}