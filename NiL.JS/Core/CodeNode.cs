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
    public enum CodeContext
    {
        None = 0,
        Strict = 1,
        //ForAssign = 2,
        Conditional = 4,
        InLoop = 8,
        InWith = 16,
        InEval = 32,
        InExpression = 64,
        InClassDefenition = 128,
        InClassConstructor = 256
    }

#if !PORTABLE
    [Serializable]
#endif
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyCodeNodeArray = new CodeNode[0];
#if !PORTABLE
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
            ExceptionsHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
            return null;
        }

        public abstract JSValue Evaluate(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое перед началом выполнения.
        /// </summary>
        /// <param name="self">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <returns>true если были внесены изменения и требуется повторный вызов функции</returns>
        internal protected virtual bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal protected virtual void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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
