using NiL.JS;
using NiL.JS.Core;

namespace Examples._4_Namespaces_and_external_types
{
    public sealed class Using_NamespaceProvider : ExamplesFramework.Example
    {
        public override void Run()
        {
            typeof(System.Windows.Forms.Form).ToString(); // This line causes load of assembly "System.Windows.Form.dll"

            var namespaceProvider = new NamespaceProvider("System.Windows.Forms");
            var context = new Context();

            context.DefineVariable("forms").Assign(namespaceProvider); // NamespaceProvider inherits JSValue. 
                                                                       // It is not necessary to wrap instance of this type

            // Form with title "Hello, World!"
            context.Eval(@"
var form = new forms.Form(); // new keyword is optional in this case
form.Text = 'Hello, World!';
form.ShowDialog();
");
        }
    }
}
