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

        internal override JSObject Evaluate(Context context)
        {
            JSObject temp;
            JSObject field = first.EvaluateForAssing(context);
            if (field.valueType == JSObjectType.Property)
            {
                lock (this)
                {
                    var fieldSource = context.objectSource;
                    temp = second.Evaluate(context);
                    setterArgs.Reset();
                    setterArgs.length = 1;
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
            temp = second.Evaluate(context);
            field.Assign(temp);
            return temp;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var r = base.Build(ref _this, depth, vars, strict);
            if (first is VariableReference)
                ((first as VariableReference).Descriptor.assignations ??
                    ((first as VariableReference).Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
#if DEBUG
            if (r)
                System.Diagnostics.Debugger.Break();
#endif
            return r;
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