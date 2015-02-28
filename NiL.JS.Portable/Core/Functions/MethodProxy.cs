using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.Functions
{
    public sealed class MethodProxy : Function
    {
        private System.Reflection.ConstructorInfo constructorInfo;

        private MethodInfo methodInfo;
        private object p;
        internal ParameterInfo[] parameters;
        private MethodBase methodBase;
        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get { return parameters; }
        }

        public MethodProxy(System.Reflection.ConstructorInfo constructorInfo)
        {
            // TODO: Complete member initialization
            this.constructorInfo = constructorInfo;
        }

        public MethodProxy(MethodInfo methodInfo, object p)
        {
            // TODO: Complete member initialization
            this.methodInfo = methodInfo;
            this.p = p;
        }

        public MethodProxy(MethodBase methodBase)
        {
            // TODO: Complete member initialization
            this.methodBase = methodBase;
        }


        internal object[] ConvertArgs(Arguments args)
        {
            throw new NotImplementedException();
        }

        [Hidden]
        internal object InvokeImpl(JSObject thisBind, object[] args, Arguments argsSource)
        {
            throw new NotImplementedException();
        }
    }
}
