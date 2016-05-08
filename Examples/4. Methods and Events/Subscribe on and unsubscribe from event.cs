using System;
using NiL.JS.Core;

namespace Examples.Methods_and_Events
{
    public sealed class Subscribe_on_and_unsubscribe_from_event : ExamplesFramework.Example
    {
        private sealed class TextEventArgs : EventArgs
        {
            public string Text { get; private set; }

            public TextEventArgs(string text)
            {
                Text = text;
            }
        }

        private sealed class ClassWithEvent
        {
            public event EventHandler<TextEventArgs> Event;

            public void FireEvent(string text)
            {
                var e = Event;
                if (e != null)
                    e(this, new TextEventArgs(text));
            }
        }

        public override void Run()
        {
            var objectWithEvent = new ClassWithEvent();
            var context = new Context();

            context.DefineVariable("objectWithEvent").Assign(JSValue.Marshal(objectWithEvent));
            context.Eval(@"
function eventHandler(sender, eventArgs) {
    console.log(eventArgs.Text);
}

objectWithEvent.Event = eventHandler;
objectWithEvent.FireEvent(""Hello, I'm event arg""); // Console: 'Hello, I'm event arg'

objectWithEvent.add_Event(eventHandler);
objectWithEvent.FireEvent(""Hello, I'm event arg""); // Console: 'Hello, I'm event arg' twice

objectWithEvent.remove_Event(eventHandler);
objectWithEvent.remove_Event(eventHandler);
objectWithEvent.FireEvent(""Hello, I'm event arg"");  // Console: <None>
");
        }
    }
}
