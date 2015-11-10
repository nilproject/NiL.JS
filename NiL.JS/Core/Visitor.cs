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

        internal protected virtual T Visit(AdditionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(BitwiseConjunctionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ArrayDefinition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(AssignmentOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(CallOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ClassDefinition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ConstantDefinition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(DecrementOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(DeleteOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(DeleteMemberExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(DivisionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(EqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(Expression node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(FunctionDefinition node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(GetMemberOperator node)
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

        internal protected virtual T Visit(InOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(IncrementOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(InstanceOfOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ObjectDefinition  node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LessOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LessOrEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalConjunctionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalNegationOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(LogicalDisjunctionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ModuloOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(MoreOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(MoreOrEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(MultiplicationOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NegationOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NewOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(CommaOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(BitwiseNegationOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NotEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberAdditionOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberLessOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberLessOrEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberMoreOpeartor node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(NumberMoreOrEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(BitwiseDisjunctionOperator node)
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

        internal protected virtual T Visit(SignedShiftLeftOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(SignedShiftRightOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StrictEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StrictNotEqualOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(StringConcatenationExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(SubstractOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ConditionalOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToBooleanOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToIntegerOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToNumberOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToStringExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(ToUnsignedIntegerExpression node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(TypeOfOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(UnsignedShiftRightOperator node)
        {
            return Visit(node as Expression);
        }

        internal protected virtual T Visit(BitwiseExclusiveDisjunctionOperator node)
        {
            return Visit(node as Expression);
        }
#if !PORTABLE
        internal protected virtual T Visit(YieldOperator node)
        {
            return Visit(node as Expression);
        }
#endif
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

        internal protected virtual T Visit(DebuggerStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(DoWhileStatement node)
        {
            return Visit(node as CodeNode);
        }

        internal protected virtual T Visit(EmptyExpression node)
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

        internal protected virtual T Visit(InfinityLoopStatement node)
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
