using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Assign : Expression
    {
        private JSObject setterArgs = new JSObject(true) { valueType = JSObjectType.Object, oValue = Arguments.Instance };
        private JSObject setterArg = new JSObject();

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Assign(CodeNode first, CodeNode second)
            : base(first, second, false)
        {
            setterArgs.fields["length"] = new JSObject()
            {
                iValue = 1,
                valueType = JSObjectType.Int,
                attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.ReadOnly
            };
            setterArgs.fields["0"] = setterArg;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                JSObject field = null;
                field = first.InvokeForAssing(context);
                if ((field.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && context.strict)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can not assign to readonly property \"" + first + "\"")));
                if (field.valueType == JSObjectType.Property)
                {
                    var fieldSource = context.objectSource;
                    setterArg.Assign(Tools.RaiseIfNotExist(second.Invoke(context)));
                    var setter = (field.oValue as NiL.JS.Core.BaseTypes.Function[])[0];
                    if (setter != null)
                        setter.Invoke(fieldSource, setterArgs);
                    else if (context.strict)
                        throw new JSException(TypeProxy.Proxy(new TypeError("Can not assign to readonly property \"" + first + "\"")));
                    return setterArg;
                }
                var t = second.Invoke(context);
                field.Assign(t);
                return t;
            }
        }

        public override string ToString()
        {
            string f = first.ToString();
            if (f[0] == '(')
                f = f.Substring(1, f.Length - 2);
            string t = second.ToString();
            if (t[0] == '(')
                t = t.Substring(1, t.Length - 2);
            return "(" + f + " = " + t + ")";
        }
    }
}