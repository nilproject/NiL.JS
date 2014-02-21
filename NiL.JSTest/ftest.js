var form = new System.Windows.Forms.Form();
form.Text = "Hello, world!";
var button = new System.Windows.Forms.Button();
button.Text = "Click Me!";
button.Click = function (sender) {
    sender.Text = "Tnx!";
    form.Text = "It's work!";
};
button.Parent = form;
form.ShowDialog();