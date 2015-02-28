using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.Functions
{
    public sealed class MethodProxy : Function
    {
        private Func<object, object[], Arguments, object> implementation;
        private bool raw;

        private object hardTarget;
        internal ParameterInfo[] parameters;
        private MethodBase methodBase;
        private ConvertValueAttribute cva;
        private ConvertValueAttribute[] convertValueAttribute;
        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get { return parameters; }
        }

        public override string name
        {
            get
            {
                return methodBase.Name;
            }
        }

        public MethodProxy(MethodBase methodBase, object hardTarget)
        {
            // TODO: Complete member initialization
            this.methodBase = methodBase;
            this.hardTarget = hardTarget;

            parameters = methodBase.GetParameters();

            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (methodBase is MethodInfo)
            {
                var methodInfo = methodBase as MethodInfo;

                if (parameters.Length == 0)
                {
                    raw = true;
                    if (methodInfo.ReturnType == typeof(void))
                        tree = Expression.Block(methodInfo.IsStatic ? Expression.Call(methodInfo) : Expression.Call(target, methodInfo), Expression.Constant(null));
                    else
                        tree = methodInfo.IsStatic ? Expression.Call(methodInfo) : Expression.Call(target, methodInfo);
                }
                else
                {
                    prms = new Expression[parameters.Length];
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                    {
                        raw = true;
                        if (methodInfo.ReturnType == typeof(void))
                            tree = Expression.Block(Expression.Call(target, methodInfo, argsSource), Expression.Constant(null));
                        else
                            tree = Expression.Call(target, methodInfo, argsSource);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            else if (methodBase is ConstructorInfo)
            {
                var constructorInfo = methodBase as ConstructorInfo;

                if (parameters.Length == 0)
                {
                    raw = true;
                    tree = Expression.New(constructorInfo.DeclaringType);
                }
                else
                {
                    prms = new Expression[parameters.Length];
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                    {
                        raw = true;
                        tree = Expression.New(constructorInfo, argsSource);
                    }
                    else
                    {
                        for (var i = 0; i < prms.Length; i++)
                            prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
                        tree = Expression.New(constructorInfo, prms);
                    }
                }
            }
            else
                throw new NotImplementedException();
            try
            {
                implementation = Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
            _length = parameters.Length;
        }

        public MethodProxy(MethodBase methodBase)
            : this(methodBase, null)
        {
        }

        [Hidden]
        internal object[] ConvertArgs(Arguments args)
        {
            throw new NotImplementedException();
        }

        [Hidden]
        internal object InvokeImpl(JSObject thisBind, object[] args, Arguments argsSource)
        {
            return TypeProxing.TypeProxy.Proxy(
                implementation(
                marshal(thisBind, methodBase.DeclaringType),
                raw ? null : args ?? ConvertArgs(argsSource), 
                argsSource));
        }

        protected internal override NiL.JS.Core.JSObject InternalInvoke(NiL.JS.Core.JSObject self, NiL.JS.Expressions.Expression[] arguments, NiL.JS.Core.Context initiator)
        {
            throw new NotImplementedException();
        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.Arguments args)
        {
            throw new NotImplementedException();
        }

        private static object[] convertArray(NiL.JS.Core.BaseTypes.Array array)
        {
            var arg = new object[array.data.Count];
            for (var j = arg.Length; j-- > 0; )
            {
                var temp = (array.data[j] ?? undefined).Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
            }
            return arg;
        }

        internal static object[] argumentsToArray(Arguments source)
        {
            var len = source.length;
            var res = new object[len];
            for (int i = 0; i < len; i++)
                res[i] = source[i] as object;
            return res;
        }

        private static object marshal(JSObject obj, Type targetType)
        {
            if (obj == null)
                return null;
            var v = Tools.convertJStoObj(obj, targetType);
            if (v != null)
                return v;
            v = obj.Value;
            if (v is Core.BaseTypes.Array)
                return convertArray(v as Core.BaseTypes.Array);
            else if (v is ProxyConstructor)
                return (v as ProxyConstructor).proxy.hostedType;
            else if (v is Function && targetType.IsSubclassOf(typeof(Delegate)))
                return (v as Function).MakeDelegate(targetType);
            else if (targetType.IsArray)
            {
                var eltype = targetType.GetElementType();
                if (eltype.GetTypeInfo().IsPrimitive)
                {
                    if (eltype == typeof(byte) && v is ArrayBuffer)
                        return (v as ArrayBuffer).Data;
                    var ta = v as TypedArray;
                    if (ta != null && ta.ElementType == eltype)
                        return ta.ToNativeArray();
                }
            }
            return v;
        }
    }
}
