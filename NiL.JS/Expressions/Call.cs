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
        internal bool _allowTCO;
        internal bool _withSpread;
        internal CallMode _callMode;

        public CallMode CallMode => _callMode;
        protected internal override bool ContextIndependent => false;
        internal override bool ResultInTempContainer => false;
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

        public bool AllowTCO { get { return _allowTCO && _callMode == 0; } }

        public bool OptionalChaining { get; }

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

        public Call(Expression first, Expression[] arguments)
            : this(first, arguments, false)
        { }

        public Call(Expression first, Expression[] arguments, bool optionalChaining)
            : base(first, null, false)
        {
            _arguments = arguments;
            OptionalChaining = optionalChaining;
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._callDepth >= 700)
                ExceptionHelper.Throw(new RangeError("Stack overflow."), this, context);

            var function = _left.Evaluate(context);

            JSValue targetObject = context._objectSource;

            Function func = null;
            if (function._valueType == JSValueType.Function)
                func = function._oValue as Function;

            if (func == null)
            {
                return callCallable(context, targetObject, function);
            }

            if (_allowTCO
                && _callMode == 0
                && (func._functionDefinition._kind != FunctionKind.Generator)
                && (func._functionDefinition._kind != FunctionKind.MethodGenerator)
                && (func._functionDefinition._kind != FunctionKind.AnonymousGenerator)
                && context._owner != null
                && func == context._owner._oValue)
            {
                tailCall(context, func);
                context._objectSource = targetObject;
                return JSValue.undefined;
            }
            else
                context._objectSource = null;

            if (_callMode == CallMode.Construct)
                targetObject = null;

            JSValue result;
            if ((function._attributes & JSValueAttributesInternal.Eval) != 0)
                result = callEval(context);
            else
                result = func.InternalInvoke(targetObject, _arguments, context, _withSpread, _callMode != 0);

            return result;
        }

        private void throwNaF(Context context)
        {
            for (int i = 0; i < _arguments.Length; i++)
            {
                context._objectSource = null;
                _arguments[i].Evaluate(context);
            }

            context._objectSource = null;

            // Аргументы должны быть вычислены даже если функция не существует.
            ExceptionHelper.ThrowTypeError(_left.ToString() + " is not a function", this, context);
        }

        private JSValue callCallable(Context context, JSValue targetObject, JSValue function)
        {
            var callable = function._oValue as ICallable;
            if (callable == null)
                callable = function.Value as ICallable;

            if (callable == null)
            {
                var typeProxy = function.Value as Proxy;
                if (typeProxy != null)
                    callable = typeProxy.PrototypeInstance as ICallable;
            }

            if (callable == null)
            {
                if (OptionalChaining)
                    return JSValue.undefined;

                throwNaF(context);

                return null;
            }

            switch (_callMode)
            {
                case CallMode.Construct:
                    return callable.Construct(Tools.CreateArguments(_arguments, context));

                case CallMode.Super:
                    return callable.Construct(targetObject, Tools.CreateArguments(_arguments, context));

                default:
                    return callable.Call(targetObject, Tools.CreateArguments(_arguments, context));
            }
        }

        private JSValue callEval(Context context)
        {
            if (_callMode != CallMode.Regular)
                ExceptionHelper.ThrowTypeError("function eval(...) cannot be called as a constructor");

            if (_arguments == null || _arguments.Length == 0)
                return JSValue.NotExists;

            var evalCode = _arguments[0].Evaluate(context);

            for (int i = 1; i < _arguments.Length; i++)
            {
                context._objectSource = null;
                _arguments[i].Evaluate(context);
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

            arguments._callee = func;
            context._executionInfo = arguments;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseCall = true;

            _codeContext = codeContext;

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
                        for (var i = 0; i < func._parameters.Length; i++)
                        {
                            if (i >= _arguments.Length)
                                break;
                            if (func._parameters[i].lastPredictedType == PredictedType.Unknown)
                                func._parameters[i].lastPredictedType = _arguments[i].ResultType;
                            else if (Tools.CompareWithMask(func._parameters[i].lastPredictedType, _arguments[i].ResultType, PredictedType.Group) != 0)
                                func._parameters[i].lastPredictedType = PredictedType.Ambiguous;
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

        protected internal override CodeNode[] GetChildrenImpl()
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