using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Call : Expression
    {
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

        internal static JSObject prepareArg(Context context, CodeNode source, bool tail, bool clone)
        {
            context.objectSource = null;
            var a = source.Evaluate(context);
            if ((a.attributes & (JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly)) == 0)
            {
                // Предполагается, что тут мы отдаём не контейнер, а сам объект. 
                // В частности, preventExtensions ожидает именно такое поведение
                if (a.valueType >= JSObjectType.Object)
                {
                    var intObj = a.oValue as JSObject;
                    if (intObj != null && intObj != a && intObj.valueType >= JSObjectType.Object)
                        return intObj;
                }
                if (clone)
                {
                    a = a.CloneImpl();
                    a.attributes |= JSObjectAttributesInternal.Cloned;
                }
            }
            return a;
        }

        internal override JSObject Evaluate(Context context)
        {
            var temp = first.Evaluate(context);
            JSObject newThisBind = context.objectSource;

            bool tail = false;
            Function func = temp.valueType == JSObjectType.Function ? temp.oValue as Function ?? (temp.oValue as TypeProxy).prototypeInstance as Function : null; // будем надеяться, что только в одном случае в oValue не будет лежать функция
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
                    && (func.Type == FunctionType.Function || func.Type == FunctionType.AnonymousFunction)
                    && func == context.caller.oValue
                    && context.caller.oValue != Script.pseudoCaller)
                {
                    context.abort = AbortType.TailRecursion;
                    tail = true;
                    //}
                    //if (tail || this.arguments.Length > 0)
                    //{
                    arguments = new Core.Arguments()
                    {
                        caller = context.strict && context.caller != null && context.caller.creator.body.strict ? Function.propertiesDummySM : context.caller,
                        length = this.arguments.Length
                    };
                    for (int i = 0; i < this.arguments.Length; i++)
                        arguments[i] = prepareArg(context, this.arguments[i], tail, this.arguments.Length > 1);
                    //}
                    context.objectSource = null;
                    //if (tail)
                    //{
                    arguments.callee = func;
                    for (var i = func.creator.body.localVariables.Length; i-- > 0; )
                    {
                        if (func.creator.body.localVariables[i].Inititalizator == null)
                            func.creator.body.localVariables[i].cacheRes.Assign(JSObject.undefined);
                    }
                    func._arguments = arguments;
                    if (context.fields != null && context.fields.ContainsKey("arguments"))
                        context.fields["arguments"] = arguments;
                    return JSObject.undefined;
                    //}
                }
                else
                    context.objectSource = null;
            }
            func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);

            checkStack();
            return func.InternalInvoke(newThisBind, this.arguments, context);
        }

        private static void checkStack()
        {
            try
            {
                System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();
            }
            catch
            {
                throw new JSException(new RangeError("Stack overflow."));
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            if (statistic != null)
                statistic.UseCall = true;
            for (var i = 0; i < arguments.Length; i++)
                Parser.Build(ref arguments[i], depth + 1, vars, strict, message, statistic, opts);
            base.Build(ref _this, depth, vars, strict, message, statistic, opts);
            if (first is GetVariableExpression)
            {
                var name = first.ToString();
                if (name == "eval" && statistic != null)
                    statistic.ContainsEval = true;
                /*VariableDescriptor f = null;
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
                }*/
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            base.Optimize(ref _this, owner, message);
            for (var i = arguments.Length; i-- > 0; )
            {
                var cn = arguments[i] as CodeNode;
                cn.Optimize(ref cn, owner, message);
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