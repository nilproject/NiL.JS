using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.Core.JIT;

namespace NiL.JS.Core
{
#if !NET35
#if !PORTABLE
    [Serializable]
#endif
    public sealed class CompiledNode : Expressions.Expression
    {
        private static readonly MethodInfo wrapMethod = typeof(JITHelpers).GetMethod("wrap", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ParameterExpression wrapContainerParameter = Expression.Parameter(typeof(JSValue), "wrapContainer");

        private static readonly ParameterExpression[] lambdaArgs = new[] 
                { 
                    JITHelpers.ContextParameter,
                    JITHelpers.DynamicValuesParameter,
                    wrapContainerParameter
                };

        private CodeNode[] dynamicValues;
        private CodeNode original;
        private Func<Context, CodeNode[], JSValue, JSValue> compiledTree;
        private Expression tree;

        public CodeNode Original { get { return original; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return original is Expressions.Expression && (original as Expressions.Expression).ContextIndependent;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                if (!(original is Expressions.Expression))
                    return PredictedType.Unknown;
                return (original as Expressions.Expression).ResultType;
            }
        }

        public override int Length
        {
            get
            {
                return original.Length;
            }
            internal set
            {
                original.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return original.Position;
            }
            internal set
            {
                original.Position = value;
            }
        }

        public CompiledNode(CodeNode original, Expression tree, CodeNode[] dynamicValues)
            : base(original is Expressions.Expression ? (original as Expressions.Expression).first : null, original is Expressions.Expression ? (original as Expressions.Expression).second : null, (original is Expressions.Expression) && (original as Expressions.Expression).tempContainer == null)
        {
            if (tempContainer == null)
                tempContainer = (original as Expressions.Expression).tempContainer;
            this.original = original;
            this.tree = tree;
            this.dynamicValues = dynamicValues;
        }

        public CompiledNode(Expressions.Expression original, Expression tree, CodeNode[] dynamicValues)
            : base(original.first, original.second, original.tempContainer == null)
        {
            if (tempContainer == null)
                tempContainer = original.tempContainer;
            this.original = original;
            this.tree = tree;
            this.dynamicValues = dynamicValues;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return original.Childs;
        }

        public override JSValue Evaluate(Context context)
        {
            if (compiledTree == null)
            {
                Expression tree;
                this.tree = this.tree.Reduce();
                if (original is Expressions.Expression)
                {
                    if (typeof(JSValue).IsAssignableFrom(this.tree.Type))
                        tree = this.tree;
                    else
                        tree = Expression.Call(wrapMethod.MakeGenericMethod(this.tree.Type), this.tree, wrapContainerParameter);
                }
                else
                {
                    tree = Expression.Block(this.tree, JITHelpers.UndefinedConstant);
                }
                //var ps = new PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
                //ps.Assert();
                //ps.AddPermission(new System.Security.Permissions.ZoneIdentityPermission(SecurityZone.MyComputer));
                //var assm = AppDomain.CurrentDomain.DefineDynamicAssembly(
                //    new AssemblyName("DynamicAssm" + Environment.TickCount),
                //    AssemblyBuilderAccess.RunAndCollect,
                //    ps,
                //    null,
                //    null);
                //var module = assm.DefineDynamicModule("DynamicModule");
                //var type = module.DefineType("DynamicType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);
                //var method = type.DefineMethod(
                //    "DynamicMethod",
                //    MethodAttributes.Public | MethodAttributes.Static,
                //    typeof(JSObject),
                //    new[] { typeof(Context), typeof(CodeNode[]), typeof(JSObject) });

                //Expression.Lambda(tree, lambdaArgs).CompileToMethod(method);
                //compiledTree = (Func<Context, CodeNode[], JSObject, JSObject>)type.CreateType().GetMethods()[0].CreateDelegate(typeof(Func<Context, CodeNode[], JSObject, JSObject>));

                compiledTree = Expression.Lambda<Func<Context, CodeNode[], JSValue, JSValue>>(tree, lambdaArgs).Compile();
            }
            var result = compiledTree(context, dynamicValues, tempContainer);
            return result;
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            return original.EvaluateForWrite(context);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return original.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        internal protected override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            original.Optimize(ref _this, owner, message, opts, statistic);
        }

        internal override Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return tree;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return original.ToString();
        }
    }
#endif
}
