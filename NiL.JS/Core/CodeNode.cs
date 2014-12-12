using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyArray = new CodeNode[0];

#if !NET35

        internal virtual System.Linq.Expressions.Expression CompileToIL(TreeBuildingState state)
        {
#if DEBUG
            System.Diagnostics.Debug.Print("JIT for " + this.GetType() + " not implemented");
#endif
            return defaultJit(state);
        }

        internal System.Linq.Expressions.Expression JitOverCall(bool forAssign)
        {
            return System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                this.GetType().GetMethod(forAssign ? "EvaluateForAssing" : "Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
        }

        internal System.Linq.Expressions.Expression defaultJit(TreeBuildingState state)
        {
            var wraper = JitOverCall(false);
            return System.Linq.Expressions.Expression.Block(
                    wraper,
                    System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Equal(JITHelpers.@const(AbortType.Return), System.Linq.Expressions.Expression.Field(JITHelpers.ContextParameter, "abort")),
                                        System.Linq.Expressions.Expression.Return(state.ReturnTarget, System.Linq.Expressions.Expression.Field(JITHelpers.ContextParameter, "abortInfo"))),
                    JITHelpers.UndefinedConstant
                   );
        }

#endif

        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyArray); } }

        protected abstract CodeNode[] getChildsImpl();

        internal virtual JSObject EvaluateForAssing(NiL.JS.Core.Context context)
        {
            return raiseInvalidAssignment();
        }

        protected static JSObject raiseInvalidAssignment()
        {
            throw new JSException(new ReferenceError("Invalid left-hand side in assignment."));
        }

        internal abstract JSObject Evaluate(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое перед началом выполнения.
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <param name="functionDepth">Глубина погружения в функции. Увеличивается при входе в функцию и уменьшается при выходе из нее</param>
        /// <returns>true если были внесены изменения и требуется повторный вызов функции</returns>
        internal virtual bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            return false;
        }

        internal virtual void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {

        }

        internal virtual System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return null;
        }
    }
}
