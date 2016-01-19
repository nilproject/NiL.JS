using System;
using NiL.JS;
using NiL.JS.Core.Interop;

namespace Examples._4_Namespaces_and_external_types
{
    public sealed class Define_constructor_of_external_type_in_JavaScript_environment : ExamplesFramework.Example
    {
        public sealed class ClassWithoutLimitation
        {
            public string Text { get; private set; }

            public ClassWithoutLimitation()
            {
                Text = "I'm born!";
            }
        }

        [RequireNewKeyword]
        public sealed class ClassWithNewOnlyLimitation
        {
            public string Text { get; private set; }

            public ClassWithNewOnlyLimitation()
            {
                Text = "I'm born!";
            }
        }

        [DisallowNewKeyword]
        public sealed class ClassWithoutNewOnlyLimitation
        {
            public string Text { get; private set; }

            public ClassWithoutNewOnlyLimitation()
            {
                Text = "I'm born!";
            }
        }

        public override void Run()
        {
            example1();

            Console.WriteLine();

            example2();

            Console.WriteLine();

            example3();
        }

        private void example1()
        {
            var module = new Module(@"
var instance0 = new ClassWithoutLimitation();
var instance1 = ClassWithoutLimitation();

console.log(instance0 === instance1); // Console: false
console.log(instance0.Text); // Console: I'm born!
console.log(instance1.Text); // Console: I'm born!
");
            module.Context.DefineConstructor(typeof(ClassWithoutLimitation));
            module.Run();
        }

        private void example2()
        {
            var module = new Module(@"
var instance0 = new ClassWithNewOnlyLimitation();
try
{
    var instance1 = ClassWithNewOnlyLimitation();
}
catch(e)
{
    console.log(e); // TypeError
}

console.log(instance0 === instance1); // Console: false
console.log(instance0.Text); // Console: I'm born!
console.log(instance1 === undefined); // Console: true
");
            module.Context.DefineConstructor(typeof(ClassWithNewOnlyLimitation));
            module.Run();
        }

        private void example3()
        {
            var module = new Module(@"
try
{
    var instance0 = new ClassWithoutNewOnlyLimitation();
}
catch(e)
{
    console.log(e); // TypeError
}

var instance1 = ClassWithoutNewOnlyLimitation();

console.log(instance0 === instance1); // Console: false
console.log(instance0 === undefined); // Console: true
console.log(instance1.Text); // Console: I'm born!
");
            module.Context.DefineConstructor(typeof(ClassWithoutNewOnlyLimitation));
            module.Run();
        }
    }
}
