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
    internal sealed class ExpressionStatement : CodeNode
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
                    case OperationType.UnsignedShiftLeft:
                        {
                            fastImpl = new Expressions.UnsignedShiftLeft(first, second);
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
                            fastImpl = new Expressions.Ternary(first, second.Evaluate(null).oValue as CodeNode[]);
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
                    default:
                        throw new ArgumentException("invalid operation type");
                }
                _type = value;
            }
        }
        private CodeNode first;
        private CodeNode second;

        public ExpressionStatement()
        {
        }

        private static CodeNode deicstra(ExpressionStatement statement)
        {
            if (statement == null)
                return null;
            ExpressionStatement cur = statement.second as ExpressionStatement;
            if (cur == null)
                return statement;
            Stack<CodeNode> stats = new Stack<CodeNode>();
            Stack<CodeNode> types = new Stack<CodeNode>();
            types.Push(statement);
            stats.Push(statement.first);
            while (cur != null)
            {
                stats.Push(cur.first);
                for (; types.Count > 0; )
                {
                    var topType = (int)(types.Peek() as ExpressionStatement)._type;
                    if (((topType & (int)OperationTypeGroups.Special) > ((int)cur._type & (int)OperationTypeGroups.Special))
                        || (((topType & (int)OperationTypeGroups.Special) == ((int)cur._type & (int)OperationTypeGroups.Special))
                            && (((int)cur._type & (int)OperationTypeGroups.Special) > 0x10)))
                    {
                        var stat = types.Pop() as ExpressionStatement;
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
                if (!(cur.second is ExpressionStatement))
                    stats.Push(cur.second);
                cur = cur.second as ExpressionStatement;
            }
            while (stats.Count > 1)
            {
                var stat = types.Pop() as ExpressionStatement;
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
            //string code = state.Code;
            int i = index;
            int position;
            OperationType type = OperationType.None;
            CodeNode first = null;
            CodeNode second = null;
            int s = i;
            state.InExpression++;
            if (Parser.ValidateName(state.Code, ref i, state.strict.Peek()) || Parser.Validate(state.Code, "this", ref i))
            {
                var name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (name == "undefined")
                    first = new ImmidateValueStatement(JSObject.undefined) { Position = index, Length = i - index };
                else
                    first = new GetVariableStatement(name, state.functionsDepth) { Position = index, Length = i - index, functionDepth = state.functionsDepth };
            }
            else if (Parser.ValidateValue(state.Code, ref i))
            {
                string value = state.Code.Substring(s, i - s);
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
                        if (Tools.ParseNumber(state.Code, ref s, out d, 0, Tools.ParseNumberOptions.Default | (state.strict.Peek() ? Tools.ParseNumberOptions.RaiseIfOctal : Tools.ParseNumberOptions.None)))
                        {
                            if ((n = (int)d) == d && !double.IsNegativeInfinity(1.0 / d))
                                first = new ImmidateValueStatement(n) { Position = index, Length = i - index };
                            else
                                first = new ImmidateValueStatement(d) { Position = index, Length = i - index };
                        }
                        else if (Parser.ValidateRegex(state.Code, ref s, true))
                        {
                            state.Code = state.Code = Tools.RemoveComments(state.SourceCode, i);
                            s = value.LastIndexOf('/') + 1;
                            string flags = value.Substring(s);
                            try
                            {
                                first = new RegExpStatement(value.Substring(1, s - 2), flags); // объекты должны быть каждый раз разные
                            }
                            catch (Exception e)
                            {
                                first = new ThrowStatement(e);
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
                //|| (state.Code[i] == 'n' && state.Code.Substring(i, 3) == "new")
                //|| (state.Code[i] == 'd' && state.Code.Substring(i, 6) == "delete")
                //|| (state.Code[i] == 't' && state.Code.Substring(i, 6) == "typeof")
                //|| (state.Code[i] == 'v' && state.Code.Substring(i, 4) == "void")
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
                                first = Parse(state, ref i, true, true, false, true).Statement;
                                if (((first as GetMemberStatement) as object ?? (first as GetVariableStatement)) == null)
                                {
                                    var cord = Tools.PositionToTextcord(state.Code, i);
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableStatement) && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                first = new Expressions.Incriment(first, Expressions.Incriment.Type.Preincriment);
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
                                first = new Expressions.Mul(new ImmidateValueStatement(1), f) { Position = index, Length = i - index };
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
                                first = Parse(state, ref i, true, true, false, true).Statement;
                                if (((first as GetMemberStatement) as object ?? (first as GetVariableStatement)) == null)
                                {
                                    var cord = Tools.PositionToTextcord(state.Code, i);
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                                }
                                if (state.strict.Peek()
                                    && (first is GetVariableStatement) && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                    throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                first = new Expressions.Decriment(first, Expressions.Decriment.Type.Predecriment) { Position = index, Length = i - index };
                            }
                            else
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                var f = Parse(state, ref i, true, true, false, true).Statement;
                                first = new Expressions.Neg(f) { Position = index, Length = i - index };
                            }
                            break;
                        }
                    case '!':
                        {
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = new Expressions.LogicalNot(Parse(state, ref i, true, true, false, true).Statement) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case '~':
                        {
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = Parse(state, ref i, true, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
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
                            first = Parse(state, ref i, false, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
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
                            first = new Expressions.None(Parse(state, ref i, false, true, false, true).Statement, new ImmidateValueStatement(JSObject.undefined)) { Position = index, Length = i - index };
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            break;
                        }
                    case 'n':
                        {
                            i += 2;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = Parse(state, ref i, false, true, true, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.New(first, new CodeNode[0]) { Position = index, Length = i - index };
                            break;
                        }
                    case 'd':
                        {
                            i += 5;
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            first = Parse(state, ref i, false, true, false, true).Statement;
                            if (first == null)
                            {
                                var cord = Tools.PositionToTextcord(state.Code, i);
                                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid prefix operation. " + cord)));
                            }
                            first = new Expressions.Delete(first) { Position = index, Length = i - index };
                            break;
                        }
                    default:
                        throw new NotImplementedException("Unary operator " + state.Code[i]);
                }
            }
            else if (state.Code[i] == '(')
            {
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                first = ExpressionStatement.Parse(state, ref i, true).Statement;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != ')')
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\"")));
                i++;
                if ((state.InExpression > 0 && first is FunctionStatement)
                    || (forNew && first is Call))
                    first = new Expressions.None(first, null) { Position = index, Length = i - index };
            }
            else
                first = Parser.Parse(state, ref i, 2);
            if (first is EmptyStatement)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid operator argument at " + Tools.PositionToTextcord(state.Code, i))));
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
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            position = i;
                            var threads = new CodeNode[]
                                {
                                    Parser.Parse(state, ref i, 1),
                                    null
                                };
                            if (state.Code[i] != ':')
                                throw new ArgumentException("Invalid char in ternary operator");
                            do
                                i++;
                            while (char.IsWhiteSpace(state.Code[i]));
                            second = new ImmidateValueStatement(new JSObject() { valueType = JSObjectType.Object, oValue = threads }) { Position = position };
                            threads[1] = Parser.Parse(state, ref i, 1);
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
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '+')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableStatement)
                                        && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not incriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                }
                                first = new Expressions.Incriment(first, Expressions.Incriment.Type.Postincriment) { Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Incriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
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
                            if (forUnary)
                            {
                                binary = false;
                                repeat = false;
                                i = rollbackPos;
                                break;
                            }
                            if (state.Code[i + 1] == '-')
                            {
                                if (rollbackPos != i)
                                    goto default;
                                if (state.strict.Peek())
                                {
                                    if ((first is GetVariableStatement)
                                        && ((first as GetVariableStatement).Name == "arguments" || (first as GetVariableStatement).Name == "eval"))
                                        throw new JSException(new SyntaxError("Can not decriment \"" + (first as GetVariableStatement).Name + "\" in strict mode."));
                                }
                                first = new Expressions.Decriment(first, Expressions.Decriment.Type.Postdecriment) { Position = first.Position, Length = i + 2 - first.Position };
                                //first = new OperatorStatement() { second = first, _type = OperationType.Decriment, Position = first.Position, Length = i + 2 - first.Position };
                                repeat = true;
                                i += 2;
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
                                if (state.Code[i + 1] == '<')
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
                            List<CodeNode> args = new List<CodeNode>();
                            i++;
                            int startPos = i;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                if (state.Code[i] == ']')
                                    break;
                                else if (state.Code[i] == ',')
                                    do
                                        i++;
                                    while (char.IsWhiteSpace(state.Code[i]));
                                args.Add(Parser.Parse(state, ref i, 1));
                                if (args[args.Count - 1] == null)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \"]\" at " + Tools.PositionToTextcord(state.Code, startPos))));
                                if ((args[args.Count - 1] is ExpressionStatement) && (args[args.Count - 1] as ExpressionStatement)._type == OperationType.None)
                                    args[args.Count - 1] = (args[args.Count - 1] as ExpressionStatement).first;
                            }
                            first = new GetMemberStatement(first, args[0]) { Position = first.Position, Length = i + 1 - first.Position };
                            i++;
                            repeat = true;
                            canAsign = true;
                            break;
                        }
                    case '(':
                        {
                            List<CodeNode> args = new List<CodeNode>();
                            i++;
                            int startPos = i;
                            for (; ; )
                            {
                                while (char.IsWhiteSpace(state.Code[i]))
                                    i++;
                                if (state.Code[i] == ')')
                                    break;
                                else if (state.Code[i] == ',')
                                    do
                                        i++;
                                    while (char.IsWhiteSpace(state.Code[i]));
                                if (i + 1 == state.Code.Length)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Unexpected end of line")));
                                args.Add(ExpressionStatement.Parse(state, ref i, false).Statement);
                                if (args[args.Count - 1] == null)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at " + Tools.PositionToTextcord(state.Code, startPos))));
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
                            throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid operator '" + state.Code[i] + "' at " + Tools.PositionToTextcord(state.Code, i))));
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
                do
                    i++;
                while (state.Code.Length > i && char.IsWhiteSpace(state.Code[i]));
                if (state.Code.Length > i)
                    second = ExpressionStatement.Parse(state, ref i, processComma, false, false, false).Statement;
            }
            CodeNode res = null;
            if (first == second && first == null)
                return new ParseResult();
            if (assign)
                res = new ExpressionStatement() { first = first, second = new ExpressionStatement() { first = first, second = second, _type = type, Position = index, Length = i - index }, _type = OperationType.Assign, Position = index, Length = i - index };
            else
            {
                if (!root || type != OperationType.None || second != null)
                {
                    if (forUnary && (type == OperationType.None) && (first is ExpressionStatement))
                        res = first as ExpressionStatement;
                    else
                        res = new ExpressionStatement() { first = first, second = second, _type = type, Position = index, Length = i - index };
                }
                else
                    res = first;
            }
            if (root)
                res = deicstra(res as ExpressionStatement) ?? res;
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

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            Type = Type;
            _this = fastImpl;
            fastImpl.Position = Position;
            fastImpl.Length = Length;
            return true;
        }
    }
}