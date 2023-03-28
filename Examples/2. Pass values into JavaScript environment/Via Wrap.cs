using System;
using ExamplesFramework;
using NiL.JS.Core;

namespace Examples.Pass_values_into_JavaScript_environment
{
    [Level(2)]
    public sealed class Via_Wrap : ExamplesFramework.Example
    {
        private sealed class ClassWithStringValue
        {
            public string NestedValue { get; set; }
        }

        private const string _nestedValue = "Hi, I'm nested value!";
        private readonly string _value = "Hi, I'm value!";
        private readonly string _variableName = "valueFromDotNet";

        public override void Run()
        {
            example1();

            Console.WriteLine();

            example2();
        }

        private void example1()
        {
            var context = new Context();

            context.DefineVariable(_variableName).Assign(context.GlobalContext.WrapValue(_value));

            context.Eval(string.Format("console.log({0});", _variableName)); // Console: [object String]

            context.Eval(string.Format("console.log(typeof {0});", _variableName)); // Console: object

            context.Eval(string.Format("console.log({0}.ToString());", _variableName)); // Console: Hi, I'm value!
        }

        private void example2()
        {
            var context = new Context();

            context.DefineVariable(_variableName).Assign(context.GlobalContext.WrapValue(new ClassWithStringValue { NestedValue = _nestedValue }));

            context.Eval(string.Format("console.log({0});", _variableName)); // Console: [object ClassWithStringValue]

            context.Eval(string.Format("console.log(typeof {0});", _variableName)); // Console: object

            context.Eval(string.Format("console.log({0}.NestedValue);", _variableName)); // Console: Hi, I'm nested value!
        }
    }
}
