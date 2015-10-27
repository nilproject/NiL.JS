using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class CallOperator : Expression
    {
        private Expression[] arguments;
        internal bool withSpread;
        internal bool allowTCO;

        public override bool IsContextIndependent { get { return false; } }
        internal override bool ResultInTempContainer { get { return false; } }
        protected internal override PredictedType ResultType
        {
            get
            {

                if (first is VariableReference)
                {
                    var desc = (first as VariableReference).descriptor;
                    var fe = desc.initializer as FunctionDefinition;
                    if (fe != null)
                        return fe.statistic.ResultType; // для рекурсивных функций будет Unknown
                }

                return PredictedType.Unknown;
            }
        }
        public Expression[] Arguments { get { return arguments; } }
        public bool AllowTCO { get { return allowTCO; } }

        internal CallOperator(Expression first, Expression[] arguments)
            : base(first, null, false)
        {
            this.arguments = arguments;
        }

        internal static JSValue PrepareArg(Context context, CodeNode source)
        {
            var a = source.Evaluate(context);
            if (a.valueType != JSValueType.SpreadOperatorResult)
            {
                a = a.CloneImpl(false);
                a.attributes |= JSValueAttributesInternal.Cloned;
            }
            return a;
        }

        public override JSValue Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSValue newThisBind = context.objectSource;

            Function func = temp.valueType == JSValueType.Function ? temp.oValue as Function ?? (temp.oValue as TypeProxy).prototypeInstance as Function : null; // будем надеяться, что только в одном случае в oValue не будет лежать функция
            if (func == null)
            {
                for (int i = 0; i < this.arguments.Length; i++)
                {
                    context.objectSource = null;
                    this.arguments[i].Evaluate(context);
                }
                context.objectSource = null;
                // Аргументы должны быть вычислены даже если функция не существует.
                ExceptionsHelper.Throw(new TypeError(first.ToString() + " is not callable"));
            }
            else
            {
                if (allowTCO
                    && (func.Type != FunctionType.Generator)
                    && context.owner != null
                    && func == context.owner.oValue
                    && context.owner.oValue != Script.pseudoCaller)
                {
                    tailCall(context, func);
                    context.objectSource = newThisBind;
                    return JSValue.undefined;
                }
                else
                    context.objectSource = null;
            }
            func.attributes = (func.attributes & ~JSValueAttributesInternal.Eval) | (temp.attributes & JSValueAttributesInternal.Eval);

            checkStack();
            return func.InternalInvoke(newThisBind, this.arguments, context, withSpread);
        }

        private void tailCall(Context context, Function func)
        {
            context.abortType = AbortType.TailRecursion;

            var arguments = new Arguments(context)
            {
                length = this.arguments.Length
            };
            for (int i = 0; i < this.arguments.Length; i++)
                arguments[i] = PrepareArg(context, this.arguments[i]);
            context.objectSource = null;

            arguments.callee = func;
            context.abortInfo = arguments;
        }

        private static void checkStack()
        {
            try
            {
#if !PORTABLE && !NET35
                System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();
#endif
            }
            catch
            {
                ExceptionsHelper.Throw(new RangeError("Stack overflow."));
            }
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (statistic != null)
                statistic.UseCall = true;

            codeContext = state;

            for (var i = 0; i < arguments.Length; i++)
                Parser.Build(ref arguments[i], depth + 1, variables, state | CodeContext.InExpression, message, statistic, opts);
            base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if (first is GetVariableExpression)
            {
                var name = first.ToString();
                if (name == "eval" && statistic != null)
                    statistic.ContainsEval = true;
                VariableDescriptor f = null;
                if (variables.TryGetValue(name, out f))
                {
                    var func = f.initializer as FunctionDefinition;
                    if (func != null)
                    {
                        for (var i = 0; i < func.parameters.Length; i++)
                        {
                            if (i >= arguments.Length)
                                break;
                            if (func.parameters[i].lastPredictedType == PredictedType.Unknown)
                                func.parameters[i].lastPredictedType = arguments[i].ResultType;
                            else if (Tools.CompareWithMask(func.parameters[i].lastPredictedType, arguments[i].ResultType, PredictedType.Group) != 0)
                                func.parameters[i].lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            for (var i = arguments.Length; i-- > 0; )
            {
                var cn = arguments[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                arguments[i] = cn as Expression;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            var result = new CodeNode[arguments.Length + 1];
            result[0] = first;
            arguments.CopyTo(result, 1);
            return result;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = first + "(";
            for (int i = 0; i < arguments.Length; i++)
            {
                res += arguments[i];
                if (i + 1 < arguments.Length)
                    res += ", ";
            }
            return res + ")";
        }
    }
}