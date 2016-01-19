using NiL.JS.Core;
using NiL.JS.Expressions;

namespace Examples._5_Syntax_extensions
{
    public sealed class Custom_operator : ExamplesFramework.Example
    {
        [CustomCodeFragment(CodeFragmentType.Expression, "keysof")]
        public sealed class KeysOfOperator : Expression
        {
            public KeysOfOperator(Expression source)
                : base(source, null, false)
            {

            }

            public static bool Validate(string code, int position)
            {
                return Parser.Validate(code, "keysof", position);
            }

            public static CodeNode Parse(ParseInfo state, ref int position)
            {
                if (!Parser.Validate(state.Code, "keysof", ref position))
                    return null;

                while (char.IsWhiteSpace(state.Code, position))
                    position++;

                int start = position;

                var source = ExpressionTree.Parse(state, ref position, true);

                return new KeysOfOperator(source);
            }

            public override JSValue Evaluate(Context context)
            {
                return JSObject.getOwnPropertyNames(new Arguments { FirstOperand.Evaluate(context) });
            }
        }

        public override void Run()
        {
            Parser.DefineCustomCodeFragment(typeof(KeysOfOperator));

            var context = new Context();

            context.Eval(@"
var keys = keysof { key0 : 0, key1: 1 };

console.log(keys); // Console: key0,key1
");
        }
    }
}
