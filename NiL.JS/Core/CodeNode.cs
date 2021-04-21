using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using System.Runtime.InteropServices;

#if !(PORTABLE || NETCORE)
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Core
{
    [Flags]
    public enum CodeContext
    {
        None = 0,
        Strict = 1,
        //ForAssign = 1 << 1,
        Conditional = 1 << 2,
        InLoop = 1 << 3,
        InWith = 1 << 4,
        InEval = 1 << 5,
        InExpression = 1 << 6,
        InClassDefinition = 1 << 7,
        InClassConstructor = 1 << 8,
        InStaticMember = 1 << 9,
        InGenerator = 1 << 10,
        InFunction = 1 << 11,
        InAsync = 1 << 12,
        InExport = 1 << 13,
        AllowDirectives = 1 << 14,
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyCodeNodeArray = new CodeNode[0];

#if !NET35 && !(PORTABLE || NETCORE)
        internal System.Linq.Expressions.Expression JitOverCall(bool forAssign)
        {
            return System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                GetType().GetMethod(forAssign ? "EvaluateForWrite" : "Evaluate", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
        }
#endif

        public virtual bool Eliminated { get; internal set; }
        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public int EndPosition { get { return Position + Length; } }

        private CodeNode[] children;
        public CodeNode[] Children { get { return children ?? (children = GetChildrenImpl() ?? emptyCodeNodeArray); } }

        protected internal virtual CodeNode[] GetChildrenImpl()
        {
            return new CodeNode[0];
        }

        protected internal virtual JSValue EvaluateForWrite(Context context)
        {
            ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
            return null;
        }

        public abstract JSValue Evaluate(Context context);

        public virtual bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public virtual void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {

        }
#if !PORTABLE
        internal virtual System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return null;
        }
#endif

        public abstract void Decompose(ref CodeNode self);

        public abstract void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias);

        public virtual T Visit<T>(Visitor<T> visitor)
        {
            return default(T);
        }
    }
}
