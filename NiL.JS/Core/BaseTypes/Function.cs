using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public class Function : EmbeddedType
    {
        [Hidden]
        internal protected Context context;
        [Hidden]
        internal protected JSObject protorypeField;
        [Hidden]
        public Context Context { get { return context; } }
        [Hidden]
        private string[] argumentsNames;
        [Hidden]
        private Statement body;
        [Hidden]
        public readonly string Name;

        #region Runtime
        [Hidden]
        private JSObject _arguments;
        [Hidden]
        public JSObject arguments
        {
            get
            {
                return _arguments;
            }
        }
        #endregion

        public Function()
        {
            context = Context.globalContext;
            body = new Statements.EmptyStatement();
            argumentsNames = new string[0];
            Name = "";
        }

        public Function(Context context, Statement body, string[] argumentsNames, string name)
        {
            this.context = context;
            this.argumentsNames = argumentsNames;
            this.body = body;
            Name = name;
            ValueType = JSObjectType.Function;
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject args)
        {
            var oldargs = _arguments;
            _arguments = args;
            try
            {
                Context internalContext = new Context(context);
                var @this = context.thisBind;
                if (@this != null && @this.ValueType < JSObjectType.Object)
                {
                    @this = new JSObject(false)
                    {
                        ValueType = JSObjectType.Object,
                        oValue = @this,
                        attributes = ObjectAttributes.DontEnum | ObjectAttributes.DontDelete | ObjectAttributes.Immutable
                    };
                }
                internalContext.thisBind = @this;
                context.thisBind = null;
                int i = 0;
                int min = System.Math.Min(args == null ? 0 : args.GetField("length", true, false).iValue, argumentsNames.Length);
                for (; i < min; i++)
                    internalContext.Define(argumentsNames[i]).Assign(args.GetField(i.ToString(), true, false));
                for (; i < argumentsNames.Length; i++)
                    internalContext.Define(argumentsNames[i]).Assign(null);

                internalContext.Assign("arguments", args);
                body.Invoke(internalContext);
                return internalContext.abortInfo;
            }
            finally
            {
                _arguments = oldargs;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(Context contextOverride, JSObject args)
        {
            if (contextOverride == null)
                return Invoke(args);
            var oldContext = context.thisBind;
            context.thisBind = contextOverride.thisBind;
            try
            {
                return Invoke(args);
            }
            finally
            {
                context.thisBind = oldContext;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            if (thisOverride == null)
                return Invoke(args);
            var oldContext = context.thisBind;
            context.thisBind = thisOverride;
            try
            {
                return Invoke(args);
            }
            finally
            {
                context.thisBind = oldContext;
            }
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            if (name == "prototype")
            {
                if (protorypeField == null)
                {
                    protorypeField = new JSObject()
                    {
                        oValue = new object(),
                        ValueType = JSObjectType.Object,
                        prototype = BaseObject.Prototype
                    };
                    protorypeField.GetField("constructor", false, true).Assign(this);
                }
                return protorypeField;
            }
            return DefaultFieldGetter(name, fast, own);
        }

        public override string ToString()
        {
            var res = "function " + Name + "(";
            for (int i = 0; i < argumentsNames.Length; )
                res += argumentsNames[i] + (++i < argumentsNames.Length ? "," : "");
            res += ")" + body.ToString();
            return res;
        }
    }
}
