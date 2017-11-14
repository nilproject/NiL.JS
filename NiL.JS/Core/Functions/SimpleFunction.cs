using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class SimpleFunction : Function
    {
        internal SimpleFunction(Context context, FunctionDefinition creator)
            : base(context, creator)
        {
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, bool withSpread, bool construct)
        {
            if (construct || withSpread)
                return base.InternalInvoke(targetObject, arguments, initiator, withSpread, construct);

            var body = _functionDefinition._body;
            var result = notExists;
            notExists._valueType = JSValueType.NotExists;

            if (_functionDefinition.parameters.Length == arguments.Length // из-за необходимости иметь возможность построить аргументы, если они потребуются
                && arguments.Length < 9)
            {
                return fastInvoke(targetObject, arguments, initiator);
            }

            return base.InternalInvoke(targetObject, arguments, initiator, withSpread, construct);
        }

        private JSValue fastInvoke(JSValue targetObject, Expression[] arguments, Context initiator)
        {
#if DEBUG && !(PORTABLE || NETCORE)
            if (_functionDefinition.trace)
                System.Console.WriteLine("DEBUG: Run \"" + _functionDefinition.Reference.Name + "\"");
#endif
            var body = _functionDefinition._body;
            targetObject = correctTargetObject(targetObject, body._strict);
            if (_functionDefinition.recursionDepth > _functionDefinition.parametersStored) // рекурсивный вызов.
            {
                storeParameters();
                _functionDefinition.parametersStored++;
            }

            JSValue res = null;
            Arguments args = null;
            bool tailCall = false;
            for (;;)
            {
                var internalContext = new Context(_initialContext, false, this);

                if (_functionDefinition.kind == FunctionKind.Arrow)
                    internalContext._thisBind = _initialContext._thisBind;
                else
                    internalContext._thisBind = targetObject;

                if (tailCall)
                    initParameters(args, internalContext);
                else
                    initParametersFast(arguments, initiator, internalContext);

                // Эта строка обязательно должна находиться после инициализации параметров
                _functionDefinition.recursionDepth++;

                if (this._functionDefinition.reference._descriptor != null && _functionDefinition.reference._descriptor.cacheRes == null)
                {
                    _functionDefinition.reference._descriptor.cacheContext = internalContext._parent;
                    _functionDefinition.reference._descriptor.cacheRes = this;
                }

                internalContext._strict |= body._strict;
                internalContext.Activate();

                try
                {
                    res = evaluateBody(internalContext);
                    if (internalContext._executionMode == ExecutionMode.TailRecursion)
                    {
                        tailCall = true;
                        args = internalContext._executionInfo as Arguments;
                    }
                    else
                        tailCall = false;
                }
                finally
                {
#if DEBUG && !(PORTABLE || NETCORE)
                    if (_functionDefinition.trace)
                        System.Console.WriteLine("DEBUG: Exit \"" + _functionDefinition.Reference.Name + "\"");
#endif
                    _functionDefinition.recursionDepth--;
                    if (_functionDefinition.parametersStored > _functionDefinition.recursionDepth)
                        _functionDefinition.parametersStored--;
                    exit(internalContext);
                }

                if (!tailCall)
                    break;

                targetObject = correctTargetObject(internalContext._objectSource, body._strict);
            }
            return res;
        }

        private void initParametersFast(Expression[] arguments, Core.Context initiator, Context internalContext)
        {
            JSValue a0 = null,
                    a1 = null,
                    a2 = null,
                    a3 = null,
                    a4 = null,
                    a5 = null,
                    a6 = null,
                    a7 = null; // Вместо кучи, выделяем память на стеке

            var argumentsCount = arguments.Length;
            if (_functionDefinition.parameters.Length != argumentsCount)
                throw new ArgumentException("Invalid arguments count");
            if (argumentsCount > 8)
                throw new ArgumentException("To many arguments");
            if (argumentsCount == 0)
                return;

            /*
             * Да, от этого кода можно вздрогнуть, но по ряду причин лучше сделать не получится.
             * Такая она цена оптимизации
             */

            /*
             * Эти два блока нельзя смешивать. Текущие значения параметров могут быть использованы для расчёта новых. 
             * Поэтому заменять значения можно только после полного расчёта новых значений
             */

            a0 = Tools.EvalExpressionSafe(initiator, arguments[0]);
            if (argumentsCount > 1)
            {
                a1 = Tools.EvalExpressionSafe(initiator, arguments[1]);
                if (argumentsCount > 2)
                {
                    a2 = Tools.EvalExpressionSafe(initiator, arguments[2]);
                    if (argumentsCount > 3)
                    {
                        a3 = Tools.EvalExpressionSafe(initiator, arguments[3]);
                        if (argumentsCount > 4)
                        {
                            a4 = Tools.EvalExpressionSafe(initiator, arguments[4]);
                            if (argumentsCount > 5)
                            {
                                a5 = Tools.EvalExpressionSafe(initiator, arguments[5]);
                                if (argumentsCount > 6)
                                {
                                    a6 = Tools.EvalExpressionSafe(initiator, arguments[6]);
                                    if (argumentsCount > 7)
                                    {
                                        a7 = Tools.EvalExpressionSafe(initiator, arguments[7]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            setParamValue(0, a0, internalContext);
            if (argumentsCount > 1)
            {
                setParamValue(1, a1, internalContext);
                if (argumentsCount > 2)
                {
                    setParamValue(2, a2, internalContext);
                    if (argumentsCount > 3)
                    {
                        setParamValue(3, a3, internalContext);
                        if (argumentsCount > 4)
                        {
                            setParamValue(4, a4, internalContext);
                            if (argumentsCount > 5)
                            {
                                setParamValue(5, a5, internalContext);
                                if (argumentsCount > 6)
                                {
                                    setParamValue(6, a6, internalContext);
                                    if (argumentsCount > 7)
                                    {
                                        setParamValue(7, a7, internalContext);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void setParamValue(int index, JSValue value, Context context)
        {
            if (_functionDefinition.parameters[index].assignments != null)
            {
                value = value.CloneImpl(false);
                value._attributes |= JSValueAttributesInternal.Argument;
            }
            else
                value._attributes &= ~JSValueAttributesInternal.Cloned;
            if (!value.Defined && _functionDefinition.parameters.Length > index && _functionDefinition.parameters[index].initializer != null)
                value.Assign(_functionDefinition.parameters[index].initializer.Evaluate(context));
            _functionDefinition.parameters[index].cacheRes = value;
            _functionDefinition.parameters[index].cacheContext = context;
            if (_functionDefinition.parameters[index].captured)
            {
                if (context._variables == null)
                    context._variables = getFieldsContainer();
                context._variables[_functionDefinition.parameters[index].name] = value;
            }
        }
    }
}
