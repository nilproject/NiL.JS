using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class CodeNode
    {
        internal static readonly MethodInfo EvaluateForAssignMethod = typeof(CodeNode).GetMethod("EvaluateForAssing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);
        internal static readonly MethodInfo EvaluateMethod = typeof(CodeNode).GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);

        internal static readonly CodeNode[] emptyCodeNodeArray = new CodeNode[0];

#if !NET35
        internal System.Linq.Expressions.Expression JitOverCall(bool forAssign)
        {
            return System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                this.GetType().GetMethod(forAssign ? "EvaluateForAssing" : "Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
        }
#endif

        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyCodeNodeArray); } }

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
        internal virtual bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
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

        public abstract T Visit<T>(Visitor<T> visitor);
    }
}
