using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Assign : Operator
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

        public Assign(Statement first, Statement second)
            : base(first, second, false)
        {
            setterArgs.fields["length"] = new JSObject()
            {
                iValue = 1,
                valueType = JSObjectType.Int,
                attributes = JSObjectAttributes.DoNotEnum | JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly
            };
            setterArgs.fields["0"] = setterArg;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                JSObject field = null;
                field = first.InvokeForAssing(context);
                if (field.valueType == JSObjectType.Property)
                {
                    var fieldSource = context.objectSource;
                    setterArg.Assign(Tools.RaiseIfNotExist(second.Invoke(context)));
                    var setter = (field.oValue as NiL.JS.Core.BaseTypes.Function[])[0];
                    var otb = context.thisBind;
                    context.thisBind = fieldSource;
                    try
                    {
                        if (setter != null)
                            setter.Invoke(context, setterArgs);
                        return setterArg;
                    }
                    finally
                    {
                        context.thisBind = otb;
                        context.objectSource = null;
                    }
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