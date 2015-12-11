using NiL.JS.Core;

namespace Examples._1_Pass_values_into_JavaScript_environment
{
    public sealed class Primitive_values_over_data_conversion : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.DefineVariable("boolValue").Assign(true);
            context.Eval("console.log(boolValue);"); // Console: true
            context.Eval("console.log(typeof boolValue);"); // Console: boolean

            context.DefineVariable("intValue").Assign(777);
            context.Eval("console.log(intValue);"); // Console: 777
            context.Eval("console.log(typeof intValue);"); // Console: number

            context.DefineVariable("doubleValue").Assign(3.141592653589D);
            context.Eval("console.log(doubleValue);"); // Console: 3.141592653589
            context.Eval("console.log(typeof doubleValue);"); // Console: number

            context.DefineVariable("stringValue").Assign("Hello!");
            context.Eval("console.log(stringValue);"); // Console: Hello!
            context.Eval("console.log(typeof stringValue);"); // Console: string
        }
    }
}
