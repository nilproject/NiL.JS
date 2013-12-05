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
            this.varibleName = name;
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
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

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
