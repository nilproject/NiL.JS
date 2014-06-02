using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetVaribleStatement : Statement
    {
        private Context cacheContext;
        private JSObject cacheRes;
        private string varibleName;
        private GetVaribleStatement unionGetter;
        
        public string VaribleName { get { return varibleName; } }

        internal GetVaribleStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, ref i, false, true, true, false))
                throw new ArgumentException("Invalid varible name");
            this.varibleName = name;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            context.objectSource = null;
            if (context == cacheContext)
                if (cacheRes == null)
                    return (cacheRes = context.GetField(varibleName));
                else
                    return cacheRes;
            cacheContext = context;
            return cacheRes = context.GetField(varibleName);
        }

        internal override JSObject Invoke(Context context)
        {
            if (context == cacheContext)
            {
                context.objectSource = null;
                return (cacheRes = context.GetField(varibleName));
            }
            lock (this)
            {
                if (context.GetType() == typeof(WithContext))
                {
                    cacheRes = context.GetField(varibleName);
                    if (cacheRes.ValueType == JSObjectType.Property)
                        cacheRes = (cacheRes.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context, null);
                    return cacheRes;
                }
                else
                {
                    cacheRes = context.GetField(varibleName);
                    cacheContext = context;
                    return cacheRes;
                }
            }
        }

        public override string ToString()
        {
            return varibleName;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
        {
            return false;
        }
    }
}