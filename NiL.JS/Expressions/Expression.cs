using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class Expression : CodeNode
    {
        /*
         * Правила именования:
         *      Один оператор в выражении:                  <смысл выражения>Operator. Конструкция "!!" рассматривается как один унарный оператор
         *      Два или более оператора в выражении:        <смысл выражения>Expression. StringConcatenationExpression может содержать более одного оператора "+", а ToIntExpression состоит из операторов "|0" и "()"
         *      Операторов нет, но есть описание сущности:  <смысл выражения>Notation.
         *      Исключения: 
         *          GetVariableExpression - оператора получения переменной нет, сущность не описывается
         *          
         * Наследниками Expression являются только те конструкции, которые могут возвращать значени (r-value).
         */

        internal JSValue tempContainer;

        internal protected virtual PredictedType ResultType
        {
            get
            {
                return PredictedType.Unknown;
            }
        }
        internal virtual bool ResultInTempContainer
        {
            get { return false; }
        }
        internal CodeContext _codeContext;

        internal Expression first;
        internal Expression second;

        public Expression FirstOperand { get { return first; } }
        public Expression SecondOperand { get { return second; } }

        public override bool Eliminated
        {
            get
            {
                return base.Eliminated;
            }
            internal set
            {
                if (first != null)
                    first.Eliminated = true;
                if (second != null)
                    second.Eliminated = true;
                base.Eliminated = value;
            }
        }

        protected internal virtual bool ContextIndependent
        {
            get
            {
                return (first == null || first.ContextIndependent)
                    && (second == null || second.ContextIndependent);
            }
        }

        protected internal virtual bool NeedDecompose
        {
            get
            {
                return (first != null && first.NeedDecompose)
                    || (second != null && second.NeedDecompose);
            }
        }

        protected Expression()
        {

        }

        protected Expression(Expression first, Expression second, bool createTempContainer)
        {
            if (createTempContainer)
                tempContainer = new JSValue() { attributes = JSValueAttributesInternal.Temporary };
            this.first = first;
            this.second = second;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            _codeContext = codeContext;
            codeContext = codeContext | CodeContext.InExpression;

            Parser.Build(ref first, depth + 1, variables, codeContext, message, statistic, opts);
            Parser.Build(ref second, depth + 1, variables, codeContext, message, statistic, opts);
            if (this.ContextIndependent)
            {
                if (message != null && !(this is RegExpExpression))
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Constant expression. Maybe, it's a mistake.");
                try
                {
                    var res = this.Evaluate(null);
                    if (res.valueType == JSValueType.Double
                        && !double.IsNegativeInfinity(1.0 / res.dValue)
                        && res.dValue == (double)(int)res.dValue)
                    {
                        res.iValue = (int)res.dValue;
                        res.valueType = JSValueType.Int;
                    }
                    _this = new ConstantDefinition(res) as CodeNode;
                    return true;
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(new ConstantDefinition(e.Avatar)));
                    expressionWillThrow(message);
                    return true;
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(e));
                    expressionWillThrow(message);
                    return true;
                }
            }
            return false;
        }

        internal void Optimize(ref Expression self, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            CodeNode cn = self;
            Optimize(ref cn, owner, message, opts, statistic);
            self = (Expression)cn;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            baseOptimize(ref _this, owner, message, opts, statistic);
        }

        internal void baseOptimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var f = first as CodeNode;
            var s = second as CodeNode;
            if (f != null)
            {
                f.Optimize(ref f, owner, message, opts, statistic);
                first = f as Expression;
            }
            if (s != null)
            {
                s.Optimize(ref s, owner, message, opts, statistic);
                second = s as Expression;
            }
            if (ContextIndependent && !(this is ConstantDefinition))
            {
                try
                {
                    _this = new ConstantDefinition(Evaluate(null));
                }
                catch (JSException e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(new ConstantDefinition(e.Avatar)));
                    expressionWillThrow(message);
                }
                catch (Exception e)
                {
                    _this = new ExpressionWrapper(new ThrowStatement(e));
                    expressionWillThrow(message);
                }
            }
        }

        private void expressionWillThrow(CompilerMessageCallback message)
        {
            if (message != null && !(this is RegExpExpression))
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Expression will throw an exception");
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            if (first != null && second != null)
                return new CodeNode[]{
                    first,
                    second
                };
            if (first != null)
                return new CodeNode[]{
                    first
                };
            if (second != null)
                return new CodeNode[]{
                    second
                };
            return null;
        }

        protected internal virtual void Decompose(ref Expression self, IList<CodeNode> result)
        {
            if (first != null)
            {
                first.Decompose(ref first, result);
            }
            
            if (second != null)
            {
                if (second.NeedDecompose && !(first is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(first));
                    first = new ExtractStoredValueExpression(first);
                }

                second.Decompose(ref second, result);
            }
        }
    }
}