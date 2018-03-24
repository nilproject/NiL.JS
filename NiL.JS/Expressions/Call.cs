using System;
using System.Linq;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Statements;
using System.Runtime.CompilerServices;

namespace NiL.JS.Expressions
{
    public enum CallMode
    {
        Regular = 0,
        Construct,
        Super
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Call : Expression
    {
        private Expression[] _arguments;
        internal bool withSpread;
        internal bool allowTCO;
        internal CallMode _callMode;

        public CallMode CallMode { get { return _callMode; } }
        protected internal override bool ContextIndependent { get { return false; } }
        internal override bool ResultInTempContainer { get { return false; } }
        protected internal override PredictedType ResultType
        {
            get
            {

                if (_left is VariableReference)
                {
                    var desc = (_left as VariableReference)._descriptor;
                    var fe = desc.initializer as FunctionDefinition;
                    if (fe != null)
                        return fe._functionInfo.ResultType; // для рекурсивных функций будет Unknown
                }

                return PredictedType.Unknown;
            }
        }
        public Expression[] Arguments { get { return _arguments; } }
        public bool AllowTCO { get { return allowTCO && _callMode == 0; } }

        protected internal override bool NeedDecompose
        {
            get
            {
                if (_left.NeedDecompose)
                    return true;

                for (var i = 0; i < _arguments.Length; i++)
                {
                    if (_arguments[i].NeedDecompose)
                        return true;
                }

                return false;
            }
        }

        internal Call(Expression first, Expression[] arguments)
            : base(first, null, false)
        {
            this._arguments = arguments;
        }

        public override JSValue Evaluate(Context context)
        {
            var temp = _left.Evaluate(context);
            JSValue targetObject = context._objectSource;
            ICallable callable = null;
            Function func = null;

            if (temp._valueType >= JSValueType.Object)
            {
                if (temp._valueType == JSValueType.Function)
                {
                    func = temp._oValue as Function;
                    callable = func;
                }

                if (func == null)
                {
                    callable = temp._oValue as ICallable;
                    if (callable == null)
                        callable = temp.Value as ICallable;
                    if (callable == null)
                    {
                        var typeProxy = temp.Value as Proxy;
                        if (typeProxy != null)
                            callable = typeProxy.PrototypeInstance as ICallable;
                    }
                }
            }

            if (callable == null)
            {
                for (int i = 0; i < this._arguments.Length; i++)
                {
                    context._objectSource = null;
                    this._arguments[i].Evaluate(context);
                }

                context._objectSource = null;

                // Аргументы должны быть вычислены даже если функция не существует.
                ExceptionHelper.ThrowTypeError(_left.ToString() + " is not a function");

                return null;
            }
            else if (func == null)
            {
                switch (_callMode)
                {
                    case CallMode.Construct:
                        {
                            return callable.Construct(Tools.CreateArguments(_arguments, context));
                        }
                    case CallMode.Super:
                        {
                            return callable.Construct(targetObject, Tools.CreateArguments(_arguments, context));
                        }
                    default:
                        return callable.Call(targetObject, Tools.CreateArguments(_arguments, context));
                }
            }
            else
            {
                if (allowTCO
                    && _callMode == 0
                    && (func._functionDefinition.kind != FunctionKind.Generator)
                    && (func._functionDefinition.kind != FunctionKind.MethodGenerator)
                    && (func._functionDefinition.kind != FunctionKind.AnonymousGenerator)
                    && context._owner != null
                    && func == context._owner._oValue)
                {
                    tailCall(context, func);
                    context._objectSource = targetObject;
                    return JSValue.undefined;
                }
                else
                    context._objectSource = null;

                checkStack();
                if (_callMode == CallMode.Construct)
                    targetObject = null;

                if ((temp._attributes & JSValueAttributesInternal.Eval) != 0)
                    return callEval(context);

                return func.InternalInvoke(targetObject, _arguments, context, withSpread, _callMode != 0);
            }
        }

        private JSValue callEval(Context context)
        {
            if (_callMode != CallMode.Regular)
                ExceptionHelper.ThrowTypeError("function eval(...) cannot be called as a constructor");

            if (_arguments == null || _arguments.Length == 0)
                return JSValue.NotExists;

            var evalCode = _arguments[0].Evaluate(context);

            for (int i = 1; i < this._arguments.Length; i++)
            {
                context._objectSource = null;
                this._arguments[i].Evaluate(context);
            }

            if (evalCode._valueType != JSValueType.String)
                return evalCode;

            return context.Eval(evalCode.ToString(), false);
        }

        private void tailCall(Context context, Function func)
        {
            context._executionMode = ExecutionMode.TailRecursion;

            var arguments = new Arguments(context);

            for (int i = 0; i < this._arguments.Length; i++)
                arguments.Add(Tools.EvalExpressionSafe(context, _arguments[i]));
            context._objectSource = null;

            arguments.callee = func;
            context._executionInfo = arguments;
        }

        private static void checkStack()
        {
            try
            {
                checkStackInternal();
            }
            catch
            {
                throw new JSException(new RangeError("Stack overflow."));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void checkStackInternal()
        {
#pragma warning disable CS0168
            decimal f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10;
            decimal f11, f12, f13, f14, f15, f16, f17, f18, f19;
#pragma warning restore CS0168
#if !(PORTABLE || NETCORE) && !NET35
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseCall = true;

            this._codeContext = codeContext;

            var super = _left as Super;

            if (super != null)
            {
                super.ctorMode = true;
                _callMode = CallMode.Super;
            }

            for (var i = 0; i < _arguments.Length; i++)
            {
                Parser.Build(ref _arguments[i], expressionDepth + 1, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            }

            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (_left is Variable)
            {
                var name = _left.ToString();
                if (name == "eval" && stats != null)
                {
                    stats.ContainsEval = true;
                    foreach (var variable in variables)
                    {
                        variable.Value.captured = true;
                    }
                }
                VariableDescriptor f = null;
                if (variables.TryGetValue(name, out f))
                {
                    var func = f.initializer as FunctionDefinition;
                    if (func != null)
                    {
                        for (var i = 0; i < func.parameters.Length; i++)
                        {
                            if (i >= _arguments.Length)
                                break;
                            if (func.parameters[i].lastPredictedType == PredictedType.Unknown)
                                func.parameters[i].lastPredictedType = _arguments[i].ResultType;
                            else if (Tools.CompareWithMask(func.parameters[i].lastPredictedType, _arguments[i].ResultType, PredictedType.Group) != 0)
                                func.parameters[i].lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
            for (var i = _arguments.Length; i-- > 0;)
            {
                var cn = _arguments[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                _arguments[i] = cn as Expression;
            }
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var result = new CodeNode[_arguments.Length + 1];
            result[0] = _left;
            _arguments.CopyTo(result, 1);
            return result;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            _left.Decompose(ref _left, result);

            var lastDecomposeIndex = -1;
            for (var i = 0; i < _arguments.Length; i++)
            {
                _arguments[i].Decompose(ref _arguments[i], result);
                if (_arguments[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(_arguments[i] is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_arguments[i], false));
                    _arguments[i] = new ExtractStoredValue(_arguments[i]);
                }
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < _arguments.Length; i++)
                _arguments[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            string res = _left + "(";
            for (int i = 0; i < _arguments.Length; i++)
            {
                res += _arguments[i];
                if (i + 1 < _arguments.Length)
                    res += ", ";
            }
            res += ")";

            if (_callMode == CallMode.Construct)
                return "new " + res;
            return res;
        }
    }
}