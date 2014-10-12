using System;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.Statements;

namespace NiL.JS.Core.JIT
{
#if !NET35

    internal static class JITHelpers
    {
        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(Context), "context");
        public static readonly ConstantExpression UndefinedConstant = Expression.Constant(JSObject.undefined);
        public static readonly ConstantExpression NotExistsConstant = Expression.Constant(JSObject.notExists);

        public static readonly MethodInfo JSObjectToBooleanMethod = typeof(JSObject).GetMethod("op_Explicit");

        public static Expression wrap(object obj)
        {
            return Expression.Constant(obj);
        }

        public static Func<Context, JSObject> compile(CodeBlock node, bool defaultReturn)
        {
            Expression exp = null;
            try
            {
                var state = new TreeBuildingState();
                if (defaultReturn)
                    state.ReturnTarget = Expression.Label(typeof(JSObject), "@NJS@Return.Label");
                exp = node.CompileToIL(state);
                if (defaultReturn)
                    exp = Expression.Block(exp, state.ReturnLabel);
                else if (exp.Type == typeof(void))
                    exp = Expression.Block(exp, UndefinedConstant);
                while (exp.CanReduce)
                    exp = exp.Reduce();
                return (Func<Context, JSObject>)System.Linq.Expressions.Expression.Lambda(
                    exp,
                    JITHelpers.ContextParameter).Compile();
            }
            catch
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw;
            }
        }

        internal static MethodInfo methodof(Func<JSObject, JSObject, bool, JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<object, bool> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, CodeNode[], JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, Exception, Exception> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<Context, Exception> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<JSObject, object> func)
        {
            return func.Method;
        }

        internal static MethodInfo methodof(Func<Context, JSObject, Context> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<string, Exception, Context> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<Exception, Context> method)
        {
            return method.Method;
        }
    }
#endif
}