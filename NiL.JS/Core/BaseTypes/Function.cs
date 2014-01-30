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
            context = Context.currentRootContext ?? Context.globalContext;
            body = new Statements.EmptyStatement();
            argumentsNames = new string[0];
            Name = "";
            ValueType = JSObjectType.Function;
            prototype = null;
        }

        public Function(JSObject args)
            : this()
        {
            var index = 0;
            int len = args.GetField("length", true, false).iValue - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args.GetField(i.ToString(), true, false) + (i + 1 < len ? "," : "");
            var fs = NiL.JS.Statements.FunctionStatement.Parse(new ParsingState("function(" + argn + "){" + args.GetField(len.ToString(), true, false) + "}"), ref index);
            if (fs.IsParsed)
            {
                Parser.Optimize(ref fs.Statement, new Dictionary<string, Statement>());
                var func = fs.Statement.Invoke(context) as Function;
                body = func.body;
                argumentsNames = func.argumentsNames;
            }
        }

        public Function(Context context, Statement body, string[] argumentsNames, string name)
        {
            this.context = context;
            this.argumentsNames = argumentsNames;
            this.body = body;
            Name = name;
            ValueType = JSObjectType.Function;
            prototype = null;
        }
        
        internal static readonly Number _length = new Number(0) { attributes = ObjectAttributes.ReadOnly | ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };

        public virtual JSObject length
        {
            get
            {
                _length.iValue = argumentsNames.Length;
                return _length;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject args)
        {
            return Invoke(null as JSObject, args);
        }

        [Hidden]
        public virtual JSObject Invoke(Context contextOverride, JSObject args)
        {
            return Invoke(args);
        }

        [Hidden]
        public virtual JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            return Invoke(args);
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            var oldargs = _arguments;
            _arguments = args;
            try
            {
                Context internalContext = new Context(context);
                var @this = thisOverride ?? context.thisBind;
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
                internalContext.Assign("arguments", args);
                if (Name != null && Name != "")
                    internalContext.Define(Name).Assign(this);
                int i = 0;
                int min = System.Math.Min(args == null ? 0 : args.GetField("length", true, false).iValue, argumentsNames.Length);
                for (; i < min; i++)
                    internalContext.Define(argumentsNames[i]).Assign(args.GetField(i.ToString(), true, false));
                for (; i < argumentsNames.Length; i++)
                    internalContext.Define(argumentsNames[i]).Assign(null);

                body.Invoke(internalContext);
                return internalContext.abortInfo;
            }
            finally
            {
                _arguments = oldargs;
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
            if (argumentsNames != null)
                for (int i = 0; i < argumentsNames.Length; )
                    res += argumentsNames[i] + (++i < argumentsNames.Length ? "," : "");
            res += ")" + (body is Statements.EmptyStatement ? "{ }" : ((object)body ?? "{ [native code] }").ToString());
            return res;
        }
    }
}
