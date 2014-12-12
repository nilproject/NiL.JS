using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.JIT;

namespace NiL.JS.Core
{
    internal sealed class CompiledNode : Expressions.Expression
    {
        private static readonly MethodInfo wrapMethod = typeof(JITHelpers).GetMethod("wrap", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ParameterExpression wrapContainerParameter = Expression.Parameter(typeof(JSObject), "wrapContainer");

        private static readonly ParameterExpression[] lambdaArgs = new[] 
                { 
                    JITHelpers.ContextParameter,
                    JITHelpers.DynamicValuesParameter,
                    wrapContainerParameter
                };

        private CodeNode[] dynamicValues;
        private CodeNode original;
        private Func<Context, CodeNode[], JSObject, JSObject> compiledTree;
        private Expression tree;

        public CodeNode Original { get { return original; } }

        public override bool IsContextIndependent
        {
            get
            {
                return original is Expressions.Expression && (original as Expressions.Expression).IsContextIndependent;
            }
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

        public override int EndPosition
        {
            get
            {
                return original.EndPosition;
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

        protected override CodeNode[] getChildsImpl()
        {
            return original.Childs;
        }

        internal override JSObject Evaluate(Context context)
        {
            if (compiledTree == null)
            {
                Expression tree;
                if (original is Expressions.Expression)
                {
                    if (typeof(JSObject).IsAssignableFrom(this.tree.Type))
                        tree = this.tree;
                    else
                        tree = Expression.Call(wrapMethod.MakeGenericMethod(this.tree.Type), this.tree, wrapContainerParameter);
                }
                else
                {
                    tree = Expression.Block(this.tree, JITHelpers.UndefinedConstant);
                }
                //var evidence = new System.Security.Policy.Evidence();
                //evidence.
                //evidence.AddHostEvidence(new System.Security.Policy.Zone(SecurityZone.MyComputer));
                //var ps = new PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
                //ps.Assert();
                //ps.AddPermission(new System.Security.Permissions.ZoneIdentityPermission(SecurityZone.MyComputer));
                //evidence.AddHostEvidence(new System.Security.Policy.Zone(SecurityZone.Trusted));
                //var assm = AppDomain.CurrentDomain.DefineDynamicAssembly(
                //    new AssemblyName("<>DynamicAssm" + Environment.TickCount),
                //    AssemblyBuilderAccess.RunAndCollect,
                //    evidence,
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

                //compiledTree = method.CreateDelegate(typeof(Func<Context, CodeNode[], JSObject, JSObject>)) as Func<Context, CodeNode[], JSObject, JSObject>;

                compiledTree = Expression.Lambda<Func<Context, CodeNode[], JSObject, JSObject>>(tree, lambdaArgs).Compile();
            }
            var result = compiledTree(context, dynamicValues, tempContainer);
            return result;
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            return original.EvaluateForAssing(context);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            return original.Build(ref _this, depth, variables, strict, message);
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message)
        {
            original.Optimize(ref _this, owner, message);
        }

        internal override Expression CompileToIL(JIT.TreeBuildingState state)
        {
            return tree;
        }

        internal override Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return tree;
        }

        public override string ToString()
        {
            return original.ToString();
        }
    }
}
