using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using System.Threading.Tasks;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Import : Expression
    {
        protected internal override PredictedType ResultType => PredictedType.Object;

        internal override bool ResultInTempContainer => false;

        protected internal override bool ContextIndependent => false;

        public Import(Expression importPath)
            : base(importPath, null, false)
        { }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            var i = index;

            if (!Parser.Validate(state.Code, "import", ref i))
                throw new InvalidOperationException("\"import\" expected");

            Tools.SkipSpaces(state.Code, ref i);

            if (!Parser.Validate(state.Code, "(", ref i))
            {
                if ((state.CodeContext & CodeContext.InExpression) == 0)
                    return null;

                ExceptionHelper.ThrowSyntaxError("\"(\" expected", state.Code, i);
            }

            Tools.SkipSpaces(state.Code, ref i);

            var path = (Expression)ExpressionTree.Parse(state, ref i);

            Tools.SkipSpaces(state.Code, ref i);

            if (!Parser.Validate(state.Code, ")", ref i))
                ExceptionHelper.ThrowSyntaxError("\")\" expected", state.Code, i);

            index = i;

            return new Import(path);
        }

        public override JSValue Evaluate(Context context)
        {
            var task = new Task<JSValue>(() =>
            {
                Module module = context._module.Import(LeftOperand.Evaluate(context).ToString());

                var m = JSObject.CreateObject();
                foreach (var item in module.Exports)
                {
                    var key = item.Key;
                    if (key == string.Empty)
                    {
                        key = "default";
                    }

                    m[key] = item.Value;
                }

                return m;
            });

            task.Start();

            var result = new Promise(task);

            return context.GlobalContext.ProxyValue(result);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Import(" + LeftOperand + ")";
        }
    }
}
