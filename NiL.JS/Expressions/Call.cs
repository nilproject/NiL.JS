//#define USEARGPOOL

using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Call : Expression
    {
#if USEARGPOOL
        private static Arguments[] argumentsPool = new Arguments[2] { new PooledArguments(), new PooledArguments() };
        private static int poolIndex = 0;
        private static void resizePool(int newSize)
        {
            if (newSize <= argumentsPool.Length)
                return;
            var newPool = new Arguments[newSize];
            for (var i = argumentsPool.Length; i-- > 0; )
                newPool[i] = argumentsPool[i];
            argumentsPool = newPool;
        }
#endif
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private Expression[] arguments;
        public Expression[] Arguments { get { return arguments; } }
        internal bool allowTCO;
        public bool AllowTCO { get { return allowTCO; } }

        internal Call(Expression first, Expression[] arguments)
            : base(first, null, false)
        {
            this.arguments = arguments;
        }

        private static JSObject prepareArg(Context context, CodeNode source, bool tail)
        {
            context.objectSource = null;
            var a = source.Evaluate(context);
            if (tail || (a.attributes & JSObjectAttributesInternal.Temporary) != 0)
            {
                a = a.CloneImpl();
                a.attributes |= JSObjectAttributesInternal.Cloned;
            }
            return a;
        }

        internal override JSObject Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSObject newThisBind = context.objectSource;

            bool tail = false;
            Function func = temp.valueType == JSObjectType.Function ? temp.oValue as Function ?? (temp.oValue as TypeProxy).prototypeInstance as Function : null; // будем надеяться, что только в одном случае в oValue не будет лежать функция
#if USEARGPOOL
            bool pool = false;
#endif
            Arguments arguments = null;
            if (func == null)
            {
                for (int i = 0; i < this.arguments.Length; i++)
                {
                    context.objectSource = null;
                    this.arguments[i].Evaluate(context);
                }
                context.objectSource = null;
                // Аргументы должны быть вычислены даже если функция не существует.
                throw new JSException((new NiL.JS.Core.BaseTypes.TypeError(first.ToString() + " is not callable")));
            }
            else
            {
                if (allowTCO
                    && context.caller != null
                    && func == context.caller.oValue
                    && context.caller.oValue != Script.pseudoCaller)
                {
                    context.abort = AbortType.TailRecursion;
                    tail = true;
#if USEARGPOOL
                    if (func._arguments is PooledArguments)
                    {
                        if (poolIndex >= argumentsPool.Length)
                            resizePool(argumentsPool.Length * 2);
                        int pindex = poolIndex++;
                        arguments = argumentsPool[pindex];
                        if (arguments == null)
                            arguments = argumentsPool[pindex] = new PooledArguments();
                        pool = true;
                    }
                    else
#endif
                        arguments = new Arguments();
                }
                else
                {
#if USEARGPOOL
                    if (poolIndex >= argumentsPool.Length)
                        resizePool(argumentsPool.Length * 2);
                    int pindex = poolIndex++;
                    arguments = argumentsPool[pindex];
                    if (arguments == null)
                        arguments = argumentsPool[pindex] = new PooledArguments();
                    pool = true;
#else
                    arguments = new Arguments();
#endif
                }
#if USEARGPOOL
                try
                {
#endif
                    arguments.length = this.arguments.Length;
                    for (int i = 0; i < this.arguments.Length; i++)
                        arguments[i] = prepareArg(context, this.arguments[i], tail);
                    arguments.caller = context.strict && context.caller != null && context.caller.creator.body.strict ? Function.propertiesDummySM : context.caller;
                    context.objectSource = null;
                    if (tail)
                    {
                        arguments.callee = func;
                        for (var i = func.creator.body.localVariables.Length; i-- > 0; )
                        {
                            if (func.creator.body.localVariables[i].Inititalizator == null)
                                func.creator.body.localVariables[i].cacheRes.Assign(JSObject.undefined);
                        }
#if USEARGPOOL
                        if (pool)
                        {
                            (arguments as PooledArguments).CloneTo(func._arguments as Arguments);
                            argumentsPool[--poolIndex].Reset();
                        }
                        else
#endif
                        {
                            func._arguments = arguments;
                            if (context.fields != null && context.fields.ContainsKey("arguments"))
                                context.fields["arguments"] = arguments;
                        }
                        return JSObject.undefined;
                    }
#if USEARGPOOL
                }
                catch
                {
                    argumentsPool[--poolIndex].Reset();
                    throw;
                }
#endif
            }
            func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);
#if USEARGPOOL
            try
            {
                return func.Invoke(newThisBind, arguments);
            }
            finally
            {
                argumentsPool[--poolIndex].Reset();
            }
#else
            return func.Invoke(newThisBind, arguments);
#endif
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (var i = 0; i < arguments.Length; i++)
                Parser.Build(ref arguments[i], depth + 1, vars, strict);
            base.Build(ref _this, depth, vars, strict);
            if (first is GetVariableExpression)
            {
                var name = first.ToString();
                VariableDescriptor f = null;
                if (vars.TryGetValue(name, out f))
                {
                    if (f.Inititalizator != null) // Defined function
                    {
                        var func = f.Inititalizator as FunctionExpression;
                        if (func != null)
                        {
                            if (func.body == null
                                || func.body.lines == null
                                || func.body.lines.Length == 0)
                            {
                                if (arguments.Length == 0)
                                    _this = new Constant(JSObject.notExists);
                                else
                                {
                                    if (depth > 1)
                                    {
                                        var b = new CodeNode[arguments.Length + 1];
                                        for (int i = arguments.Length, j = 0; i > 0; i--, j++)
                                            b[i] = arguments[j];
                                        b[0] = new Constant(JSObject.notExists);
                                        _this = new CodeBlock(b, strict);
                                    }
                                    else
                                    {
                                        System.Array.Reverse(arguments, 0, arguments.Length);
                                        _this = new CodeBlock(arguments, strict);
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            base.Optimize(ref _this, owner);
            for (var i = arguments.Length; i-- > 0; )
            {
                var cn = arguments[i] as CodeNode;
                cn.Optimize(ref cn, owner);
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