using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Call : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private Arguments cachedArgs;

        private CodeNode[] arguments;
        public CodeNode[] Arguments { get { return arguments; } }

        internal Call(CodeNode first, CodeNode[] arguments)
            : base(first, null, false)
        {
            this.arguments = arguments;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Evaluate(context);
            newThisBind = context.objectSource;

            Arguments arguments = null;
            bool cached = cachedArgs != null;
            if (cached)
            {
                arguments = cachedArgs;
                cachedArgs = null;
            }
            else
            {
                arguments = new Arguments()
                {
                    length = this.arguments.Length
                };
            }
            JSObject oldCaller = null;
            try
            {
                for (int i = 0; i < arguments.length; i++)
                {
                    context.objectSource = null;
                    var a = this.arguments[i].Evaluate(context);
                    if ((a.attributes & JSObjectAttributesInternal.Temporary) != 0)
                    {
                        a = a.CloneImpl();
                        a.attributes |= JSObjectAttributesInternal.Cloned;
                    }
#if DEBUG
                    if (a == null)
                        System.Diagnostics.Debugger.Break();
#endif
                    arguments[i] = a;
                }
                context.objectSource = null;

                // Аргументы должны быть вычислены даже если функция не существует.
                if (temp.valueType != JSObjectType.Function
                    //&& !(temp.valueType == JSObjectType.Object && temp.oValue is Function)
                    )
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
                func = temp.oValue as Function ?? (temp.oValue as TypeProxy).prototypeInstance as Function; // будем надеяться, что только в одном случае в oValue не будет лежать функция
                func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);

                oldCaller = func._caller;
                func._caller = context.caller.creator.body.strict ? Function.propertiesDummySM : context.caller;
                return func.Invoke(newThisBind, arguments);
            }
            finally
            {
                if (cached)
                    cachedArgs = arguments;
                if (oldCaller != null)
                    func._caller = oldCaller;
            }
        }

        private static bool isSimple(CodeNode expression)
        {
            if (expression == null
                || expression is ImmidateValueStatement
                || expression is GetVariableStatement)
                return true;
            if (expression is Call)
            {
                if ((expression as Call).first is VariableReference
                    && (expression as Call).first.ToString() == "eval")
                    return false;
                var args = (expression as Call).arguments;
                for (var i = 0; i < args.Length; i++)
                {
                    if (!isSimple(args[i]))
                        return false;
                }
                return true;
            }
            if (expression is Expression)
                return isSimple((expression as Expression).FirstOperand) && isSimple((expression as Expression).SecondOperand);
            return false;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (var i = 0; i < arguments.Length; i++)
                Parser.Optimize(ref arguments[i], depth + 1, vars, strict);
            base.Optimize(ref _this, depth, vars, strict);
            if (first is GetVariableStatement)
            {
                var name = first.ToString();
                VariableDescriptor f = null;
                if (vars.TryGetValue(name, out f))
                {
                    if (f.Inititalizator != null) // Defined function
                    {
                        var func = f.Inititalizator as FunctionStatement;
                        if (func != null)
                        {
                            if (func.body == null || func.body.body == null || func.body.body.Length == 0)
                            {
                                if (arguments.Length == 0)
                                    _this = new EmptyStatement();
                                else
                                {
                                    System.Array.Reverse(arguments, 0, arguments.Length);
                                    _this = new CodeBlock(arguments, strict);
                                }
                            }
                            /* // TODO
                            else if (func.body.body.Length == 1 && func.body.body[0] is ReturnStatement)
                            {
                                var ret = func.body.body[0] as ReturnStatement;
                                if (isSimple(ret.Body))
                                {
                                    var prms = func.Parameters;
                                    for (var i = 0; i < prms.Length; i++)
                                    {
                                    }
                                }
                            }
                            */
                            else
                            {
                                if (System.Array.Find(func.body.variables, x => x.Name == "arguments" || x.Name == "eval") == null)
                                {
                                    cachedArgs = new Arguments()
                                    {
                                        length = this.arguments.Length
                                    };
                                }
                            }
                        }
                    }
                }
            }
            return false;
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