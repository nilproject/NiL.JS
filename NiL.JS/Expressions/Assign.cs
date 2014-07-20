using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Assign : Expression
    {
        private Arguments setterArgs = new Arguments() { length = 1 };

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
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject temp;
            JSObject field = first.InvokeForAssing(context);
            if (field.valueType == JSObjectType.Property)
            {
                lock (this)
                {
                    var fieldSource = context.objectSource;
                    temp = second.Invoke(context);
                    setterArgs[0] = temp;
                    var setter = (field.oValue as NiL.JS.Core.BaseTypes.Function[])[0];
                    if (setter != null)
                        setter.Invoke(fieldSource, setterArgs);
                    else if (context.strict)
                        throw new JSException(new TypeError("Can not assign to readonly property \"" + first + "\""));
                    return temp;
                }
            }
            else
            {
                if ((field.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && context.strict)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + first + "\""));
            }
            temp = second.Invoke(context);
            field.Assign(temp);
            return temp;
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