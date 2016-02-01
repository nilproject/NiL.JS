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

#if !PORTABLE
    [Serializable]
#endif
    public sealed class Call : Expression
    {
        private Expression[] _arguments;
        internal bool withSpread;
        internal bool allowTCO;
        internal CallMode callMode;

        public CallMode CallMode { get { return callMode; } }
        protected internal override bool ContextIndependent { get { return false; } }
        internal override bool ResultInTempContainer { get { return false; } }
        protected internal override PredictedType ResultType
        {
            get
            {

                if (first is VariableReference)
                {
                    var desc = (first as VariableReference)._descriptor;
                    var fe = desc.initializer as FunctionDefinition;
                    if (fe != null)
                        return fe._functionInfo.ResultType; // для рекурсивных функций будет Unknown
                }

                return PredictedType.Unknown;
            }
        }
        public Expression[] Arguments { get { return _arguments; } }
        public bool AllowTCO { get { return allowTCO && callMode == 0; } }

        protected internal override bool NeedDecompose
        {
            get
            {
                if (first.NeedDecompose)
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
            var temp = first.Evaluate(context);
            JSValue targetObject = context.objectSource;

            Function func = temp.valueType != JSValueType.Function ? null :
                temp.oValue as Function
                ?? temp.Value as Function
                ?? (temp.oValue as TypeProxy).prototypeInstance as Function;

            if (func == null)
            {
                for (int i = 0; i < this._arguments.Length; i++)
                {
                    context.objectSource = null;
                    this._arguments[i].Evaluate(context);
                }
                context.objectSource = null;
                // Аргументы должны быть вычислены даже если функция не существует.
                ExceptionsHelper.ThrowTypeError(first.ToString() + " is not callable");
            }
            else
            {
                if (allowTCO
                    && callMode == 0
                    && (func.creator.kind != FunctionKind.Generator)
                    && (func.creator.kind != FunctionKind.MethodGenerator)
                    && (func.creator.kind != FunctionKind.AnonymousGenerator)
                    && context.owner != null
                    && func == context.owner.oValue)
                {
                    tailCall(context, func);
                    context.objectSource = targetObject;
                    return JSValue.undefined;
                }
                else
                    context.objectSource = null;
            }

            checkStack();
            if (callMode == CallMode.Construct)
                targetObject = null;

            func.attributes = (func.attributes & ~JSValueAttributesInternal.Eval) | (temp.attributes & JSValueAttributesInternal.Eval);
            return func.InternalInvoke(targetObject, this._arguments, context, callMode != 0 ? func : null, withSpread, callMode != 0);
        }

        private void tailCall(Context context, Function func)
        {
            context.abortReason = AbortReason.TailRecursion;

            var arguments = new Arguments(context)
            {
                length = this._arguments.Length
            };
            for (int i = 0; i < this._arguments.Length; i++)
                arguments[i] = Tools.PrepareArg(context, this._arguments[i]);
            context.objectSource = null;

            arguments.callee = func;
            context.abortInfo = arguments;
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
        private static void checkStackInternal(decimal a = 0, decimal b = 0, decimal c = 0, decimal d = 0)
        {
#if !PORTABLE && !NET35
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseCall = true;

            this._codeContext = codeContext;

            var super = first as Super;

            if (super != null)
            {
                super.ctorMode = true;
                callMode = CallMode.Super;
            }

            for (var i = 0; i < _arguments.Length; i++)
            {
                Parser.Build(ref _arguments[i], expressionDepth + 1, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            }

            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (first is GetVariable)
            {
                var name = first.ToString();
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

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
            for (var i = _arguments.Length; i-- > 0;)
            {
                var cn = _arguments[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                _arguments[i] = cn as Expression;
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var result = new CodeNode[_arguments.Length + 1];
            result[0] = first;
            _arguments.CopyTo(result, 1);
            return result;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            first.Decompose(ref first, result);

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
            {
                _arguments[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }

        public override string ToString()
        {
            string res = first + "(";
            for (int i = 0; i < _arguments.Length; i++)
            {
                res += _arguments[i];
                if (i + 1 < _arguments.Length)
                    res += ", ";
            }
            res += ")";

            if (callMode == CallMode.Construct)
                return "new " + res;
            return res;
        }
    }
}