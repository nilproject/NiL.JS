using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.JIT
{
    internal static class JITHelpers
    {
        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(Context), "context");
        public static readonly ConstantExpression UndefinedConstant = Expression.Constant(JSObject.undefined);
        public static readonly LabelTarget ReturnTarget = Expression.Label(typeof(JSObject), "ReturnLabel");
        public static readonly LabelExpression ReturnLabel = Expression.Label(ReturnTarget, UndefinedConstant);

        public static readonly MethodInfo JSObjectToBooleanMethod = typeof(JSObject).GetMethod("op_Explicit");

        public static Expression wrap(object obj)
        {
            return Expression.Constant(obj);
        }

        public static Func<Context, JSObject> compile(CodeNode node, bool allowReturn)
        {
            try
            {
                if (node.Length > 0)
                {
                    System.Linq.Expressions.Expression exp = System.Linq.Expressions.Expression.Block(node.BuildTree(new TreeBuildingState(allowReturn)), JITHelpers.ReturnLabel);
                    while (exp.CanReduce)
                        exp = exp.Reduce();
                    return (Func<Context, JSObject>)System.Linq.Expressions.Expression.Lambda(
                        exp,
                        JITHelpers.ContextParameter).Compile();
                }
            }
            catch
            {
#if DEBUG
                //System.Diagnostics.Debugger.Break();
#endif
            }
            return null;
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
    }
}
