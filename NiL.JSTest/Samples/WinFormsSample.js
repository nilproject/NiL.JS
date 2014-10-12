var form = new forms.Form();
form.Text = "Hello, world!";
var button = new forms.Button();
button.Text = "Click Me!";
button.Click = function (sender) {
    sender.Text = "Tnx!";
    form.Text = "It's work!";
};
button.Parent = form;
form.ShowDialog();