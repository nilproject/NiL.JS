using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.Core.JIT;

namespace NiL.JS.Core
{
#if !NET35
#if !PORTABLE && !NETCORE
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

        private CodeNode[] _dynamicValues;
        private CodeNode _original;
        private Func<Context, CodeNode[], JSValue, JSValue> _compiledTree;
        private Expression _tree;

        public CodeNode Original { get { return _original; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return _original is Expressions.Expression && (_original as Expressions.Expression).ContextIndependent;
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
                if (!(_original is Expressions.Expression))
                    return PredictedType.Unknown;
                return (_original as Expressions.Expression).ResultType;
            }
        }

        public override int Length
        {
            get
            {
                return _original.Length;
            }
            internal set
            {
                _original.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return _original.Position;
            }
            internal set
            {
                _original.Position = value;
            }
        }

        public CompiledNode(CodeNode original, Expression tree, CodeNode[] dynamicValues)
            : base(original is Expressions.Expression ? (original as Expressions.Expression)._left : null, original is Expressions.Expression ? (original as Expressions.Expression)._right : null, (original is Expressions.Expression) && (original as Expressions.Expression)._tempContainer == null)
        {
            if (_tempContainer == null)
                _tempContainer = (original as Expressions.Expression)._tempContainer;
            this._original = original;
            this._tree = tree;
            this._dynamicValues = dynamicValues;
        }

        public CompiledNode(Expressions.Expression original, Expression tree, CodeNode[] dynamicValues)
            : base(original._left, original._right, original._tempContainer == null)
        {
            if (_tempContainer == null)
                _tempContainer = original._tempContainer;
            this._original = original;
            this._tree = tree;
            this._dynamicValues = dynamicValues;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return _original.Childs;
        }

        public override JSValue Evaluate(Context context)
        {
            if (_compiledTree == null)
            {
                Expression tree;
                this._tree = this._tree.Reduce();
                if (_original is Expressions.Expression)
                {
                    if (typeof(JSValue).IsAssignableFrom(this._tree.Type))
                        tree = this._tree;
                    else
                        tree = Expression.Call(wrapMethod.MakeGenericMethod(this._tree.Type), this._tree, wrapContainerParameter);
                }
                else
                {
                    tree = Expression.Block(this._tree, JITHelpers.UndefinedConstant);
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

                _compiledTree = Expression.Lambda<Func<Context, CodeNode[], JSValue, JSValue>>(tree, lambdaArgs).Compile();
            }
            var result = _compiledTree(context, _dynamicValues, _tempContainer);
            return result;
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            return _original.EvaluateForWrite(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return _original.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            _original.Optimize(ref _this, owner, message, opts, stats);
        }

        internal override Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            return _tree;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return _original.ToString();
        }
    }
#endif
}
