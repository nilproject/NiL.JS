using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace Examples._4_Namespaces_and_external_types
{
    public sealed class Using_of_attributes : ExamplesFramework.Example
    {
        private sealed class TestClass
        {
            public string RegularProperty { get; } = "Regular property";

            [DoNotEnumerate]
            public string NonEnumerableProperty { get; } = "Non-enumerable property";

            [NotConfigurable]
            public string NonConfigurableProperty { get; } = "Non-configerable property";

            [DoNotDelete]
            [NotConfigurable]
            public string NonConfigurableAndNonDeletableProperty { get; } = "Non-configerable and non-deletable property";

            [ReadOnly]
            public string ReadOnlyField = "Read only field";

            public string RegularField = "Regular field";

            [Hidden]
            public string HiddenProperty { get; set; } = "Hidden property";

            [DoNotDelete]
            public string NonDeletableProperty { get; } = "Non-deletable property";

            public IEnumerable<int> PropertyWithCompositeType {[return: MyValueConverter] get; } = new[] { 1, 2, 3, 4, 5 };

            public void MethodWithConverter([MyValueConverter] string[] parts)
            {
                Console.WriteLine(parts.Aggregate((x, result) => result + x));
            }
        }

        private sealed class MyValueConverter : ConvertValueAttribute
        {
            public override object From(object source)
            {
                var enumerable = source as IEnumerable<int>;
                if (enumerable != null)
                {
                    return enumerable.Aggregate((x, sum) => sum + x);
                }

                return null;
            }

            public override object To(object source)
            {
                var @string = source as string;
                if (@string != null)
                {
                    return @string.Select(x => x.ToString()).ToArray();
                }

                return null;
            }
        }

        public override void Run()
        {
            var context = new Context();
            var instance = new TestClass();
            context.DefineVariable("instance").Assign(JSValue.Wrap(instance));

            example1(context);

            example2(context);

            example3(context);

            example4(context);

            example5(context, instance);

            example6(context, instance);

            example7(context, instance);

            example8(context, instance);

            example9(context);
        }

        private static void header([CallerMemberName] string exampleName = "")
        {
            if (Console.CursorTop > 1)
                Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(exampleName + ":");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void example1(Context context)
        {
            header();
            context.Eval(@"
let result = false;
for (let property in instance)
{
    if (property === 'NonEnumerableProperty')
        result = true;
}

console.log(result); // Console: false
");
        }

        private static void example2(Context context)
        {
            header();
            context.Eval(@"
try
{
    Object.defineProperty(instance.__proto__, 'NonConfigurableProperty', { configurable: true });
}
catch (e)
{
    // TypeError: Cannot set configurable attribute to true.
}

let result = Object.getOwnPropertyDescriptor(instance.__proto__, 'NonConfigurableProperty').configurable;

console.log(result); // Console: true
");
        }

        private static void example3(Context context)
        {
            header();
            context.Eval(@"
try
{
    Object.defineProperty(instance.__proto__, 'NonConfigurableAndNonDeletableProperty', { configurable: true });
}
catch (e)
{
    // TypeError: Cannot set configurable attribute to true.
}

let result = Object.getOwnPropertyDescriptor(instance.__proto__, 'NonConfigurableAndNonDeletableProperty').configurable;

console.log(result); // Console: false
");
        }

        private static void example4(Context context)
        {
            header();
            context.Eval(@"
instance.ReadOnlyField = 'my value';
let result = instance.ReadOnlyField === 'my value';

console.log(result); // Console: false
");
        }

        private static void example5(Context context, TestClass instance)
        {
            header();
            context.Eval(@"
instance.RegularField = 'my value';
let result = instance.RegularField === 'my value';

console.log(result); // Console: true
");
            Console.WriteLine(instance.RegularField); // Console: my value
        }

        private static void example6(Context context, TestClass instance)
        {
            header();
            context.Eval(@"
let result = instance.HiddenProperty === undefined;

console.log(result); // Console: true
");
            context.Eval(@"
instance.HiddenProperty = 'my value';
let result = instance.HiddenProperty === 'my value';

console.log(result); // Console: true
");
            Console.WriteLine(instance.HiddenProperty); // Console: Hidden property
        }

        private void example7(Context context, TestClass instance)
        {
            header();
            context.Eval(@"
let result = delete instance.__proto__.NonDeletableProperty;

console.log(result); // Console: false

result = Object.getOwnPropertyDescriptor(instance.__proto__, 'NonDeletableProperty').configurable;

console.log(result); // true;
");
        }

        private static void example8(Context context, TestClass instance)
        {
            header();
            context.Eval(@"
let result = instance.PropertyWithCompositeType === 15;

console.log(result); // Console: true
");
            Console.WriteLine(instance.PropertyWithCompositeType); // Console: System.Int32[]
        }

        private static void example9(Context context)
        {
            header();
            context.Eval(@"
instance.MethodWithConverter('54321'); // Console: 12345
");
        }
    }
}
