using System;
using NiL.JS.Core;

namespace Examples._1_Pass_values_into_JavaScript_environment
{
    public sealed class Via_Marshal : ExamplesFramework.Example
    {
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

            context.DefineVariable(_variableName).Assign(JSValue.Marshal(_value));

            context.Eval(string.Format("console.log({0});", _variableName)); // Console: Hi, I'm value!

            context.Eval(string.Format("console.log(typeof {0});", _variableName)); // Console: string
        }

        private void example2()
        {
            var context = new Context();

            context.DefineVariable(_variableName).Assign(JSValue.Marshal(new { NestedValue = _nestedValue }));

            context.Eval(string.Format("console.log({0});", _variableName)); // Console: [object <>f__AnonymousType0`1] (or similar)

            context.Eval(string.Format("console.log(typeof {0});", _variableName)); // Console: object

            context.Eval(string.Format("console.log({0}.NestedValue);", _variableName)); // Console: Hi, I'm nested value!
        }
    }
}
