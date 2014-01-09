using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class GetVaribleStatement : Statement
    {
        private Context cacheContext;
        private JSObject cacheRes;
        private string varibleName;

        public GetVaribleStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, ref i, false, true))
                throw new ArgumentException("Invalid varible name");
            this.varibleName = name;
        }

        public override JSObject Invoke(Context context)
        {
            if (context == cacheContext)
                if (cacheRes == null)
                    return (cacheRes = context.GetField(varibleName));
                else
                    return cacheRes;
            cacheContext = context;
            return cacheRes = context.GetField(varibleName);
        }

        public override JSObject Invoke(Context context, JSObject[] args)
        {
            throw new NotImplementedException();
        }
    }
}