using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Core
{
    [Flags]
    public enum BuildState
    {
        None = 0,
        Strict = 1,
        //ForAssign = 2,
        Conditional = 4,
        InLoop = 8,
        InWith = 16,
        InEval = 32,
        InExpression = 64
    }

#if !PORTABLE
    [Serializable]
#endif
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyCodeNodeArray = new CodeNode[0];
#if !PORTABLE
        internal static readonly MethodInfo EvaluateForAssignMethod = typeof(CodeNode).GetMethod("EvaluateForWrite", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);
        internal static readonly MethodInfo EvaluateMethod = typeof(CodeNode).GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);

#if !NET35
        internal System.Linq.Expressions.Expression JitOverCall(bool forAssign)
        {
            return System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                this.GetType().GetMethod(forAssign ? "EvaluateForWrite" : "Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
        }
#endif
#endif
        public virtual bool Eliminated { get; internal set; }
        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyCodeNodeArray); } }

        protected virtual CodeNode[] getChildsImpl()
        {
            return new CodeNode[0];
        }

        internal protected virtual JSValue EvaluateForWrite(NiL.JS.Core.Context context)
        {
            ExceptionsHelper.Throw(new ReferenceError("Invalid left-hand side in assignment."));
            return null;
        }

        internal protected abstract JSValue Evaluate(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое перед началом выполнения.
        /// </summary>
        /// <param name="self">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <returns>true если были внесены изменения и требуется повторный вызов функции</returns>
        internal protected virtual bool Build(ref CodeNode self, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal protected virtual void Optimize(ref CodeNode self, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {

        }
#if !PORTABLE
        internal virtual System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return null;
        }
#endif
        public virtual T Visit<T>(Visitor<T> visitor)
        {
            return default(T);
        }
    }
}
