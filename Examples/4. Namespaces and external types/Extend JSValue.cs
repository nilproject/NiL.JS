using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace Examples._4_Namespaces_and_external_types
{
    public sealed class Extend_JSValue : ExamplesFramework.Example
    {
        private sealed class CustomJSValue : JSValue
        {
            private sealed class WriteAccessor : JSValue
            {
                private string _name;

                public WriteAccessor(string name)
                {
                    _name = name;
                }

                public override void Assign(JSValue value)
                {
                    Console.WriteLine("Write value \"" + value + "\" with key \"" + _name + "\"");
                }
            }

            public CustomJSValue()
            {
                ValueType = JSValueType.Object;
                Value = this; // it is necessary if ValueType is Object
            }

            public override void Assign(JSValue value)
            {
                throw new InvalidOperationException();
            }

            protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope)
            {
                if (forWrite)
                {
                    Console.WriteLine("Getting write accessor for \"" + key + "\"");
                    return new WriteAccessor(key.ToString());
                }
                else
                {
                    switch (key.ValueType)
                    {
                        case JSValueType.Integer:
                            {
                                return (int)key.Value;
                            }
                        case JSValueType.String:
                            {
                                var value = key.Value.ToString();
                                switch (value)
                                {
                                    case "0":
                                        {
                                            return "nil";
                                        }
                                    case "1":
                                        {
                                            return "one";
                                        }
                                    case "2":
                                        {
                                            return "two";
                                        }
                                }
                                goto default;
                            }
                        default:
                            {
                                return "Dummy for key \"" + key + "\" with type \"" + key.ValueType + "\"";
                            }
                    }
                }
            }
        }

        public override void Run()
        {
            var context = new Context();
            context.DefineVariable("myValue").Assign(new CustomJSValue());

            context.Eval("console.log(myValue[1]);"); // Console: 1
            context.Eval("console.log(myValue['1']);"); // Console: one
            context.Eval("console.log(myValue[true]);"); // Console: Dummy for key "true" with type "Boolean"
            context.Eval("console.log(myValue.true);"); // Console: Dummy for key "true" with type "String"

            context.Eval("myValue.key = 'value';"); // Console: Getting write container for "key"
                                                    // Console: Write value "value" with key "key"
        }
    }
}
