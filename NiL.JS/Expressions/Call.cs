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
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private CodeNode[] arguments;
        public CodeNode[] Arguments { get { return arguments; } }
        internal bool allowTCO;
        public bool AllowTCO { get { return allowTCO; } }

        internal Call(CodeNode first, CodeNode[] arguments)
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
            if (allowTCO
                && context.caller != null
                && func == context.caller.oValue
                && context.caller.oValue != Script.pseudoCaller)
            {
                context.abort = AbortType.TailRecursion;
                tail = true;
            }
            Arguments arguments = new Arguments();
            arguments.length = this.arguments.Length;
            for (int i = 0; i < arguments.length; i++)
                arguments[i] = prepareArg(context, this.arguments[i], tail);
            arguments.caller = context.strict && context.caller != null && context.caller.creator.body.strict ? Function.propertiesDummySM : context.caller;
            context.objectSource = null;
            if (tail)
            {
                for (var i = func.creator.body.localVariables.Length; i-- > 0; )
                {
                    if (func.creator.body.localVariables[i].Inititalizator == null)
                        func.creator.body.localVariables[i].cacheRes.Assign(JSObject.undefined);
                }
                func._arguments = arguments;
                return JSObject.undefined;
            }
            // Аргументы должны быть вычислены даже если функция не существует.
            if (func == null)
                throw new JSException((new NiL.JS.Core.BaseTypes.TypeError(first.ToString() + " is not callable")));
            func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);

            return func.Invoke(newThisBind, arguments);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (var i = 0; i < arguments.Length; i++)
                Parser.Build(ref arguments[i], depth + 1, vars, strict);
            base.Build(ref _this, depth, vars, strict);
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
                            if (func.body == null
                                || func.body.body == null
                                || func.body.body.Length == 0)
                            {
                                if (arguments.Length == 0)
                                    _this = new EmptyStatement();
                                else
                                {
                                    System.Array.Reverse(arguments, 0, arguments.Length);
                                    _this = new CodeBlock(arguments, strict);
                                }
                                    return true;
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