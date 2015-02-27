using System;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    /// <summary>
    /// AST nodes visitor.
    /// </summary>
    /// <typeparam name="T">Type of return value</typeparam>
#if !PORTABLE
    [Serializable]
#endif
    public abstract class Visitor<T>
    {
        internal protected abstract T Visit(CodeNode node);

        internal protected virtual T Visit(Addition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(And node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ArrayExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Assign node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Call node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Constant node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Decriment node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Delete node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(DeleteMemberExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Division node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Equal node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Expression node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(FunctionExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(GetMemberExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(GetVariableExpression node)
        {
            return Visit(node as VariableReference);
        }

        internal protected virtual T Visit(VariableReference node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(In node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Incriment node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(InstanceOf node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Json node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Less node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LessOrEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalAnd node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalNot node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalOr node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Mod node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(More node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(MoreOrEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Mul node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Neg node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(New node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(None node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Not node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NotEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberAddition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberLess node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberLessOrEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberMore node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberMoreOrEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Or node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(RegExpExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(SetMemberExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(SignedShiftLeft node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(SignedShiftRight node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StrictEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StrictNotEqual node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StringConcat node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Substract node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Ternary node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToBool node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToInt node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToNumber node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToStr node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToUInt node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(TypeOf node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(UnsignedShiftRight node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Xor node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Yield node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(BreakStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(CodeBlock node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ContinueStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(DebuggerOperator node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(DoWhileStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(EmptyStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ForInStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ForOfStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ForStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(IfElseStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(IfStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(InfinityLoop node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(LabeledStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ReturnStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(SwitchStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(ThrowStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(TryCatchStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(VariableDefineStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(WhileStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(WithStatement node)
        {
            return Visit(node as CodeNode);
        }
    }
}
