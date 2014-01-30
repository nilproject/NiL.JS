var form = System.Windows.Forms.Form();
form.Text = "Hello, world!";
var button = System.Windows.Forms.Button();
button.Text = "Click Me!";
button.Click = function () { form.Text = "It's work!"; };
button.Parent = form;
form.ShowDialog();