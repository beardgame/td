using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class DebugConsoleControl : CompositeControl
    {
        private readonly DebugConsole debug;
        private readonly TextInput commandInput;

        public DebugConsoleControl(DebugConsole debug)
        {
            this.debug = debug;
            commandInput = new TextInput()
                .Anchor(a => a.Bottom(margin: 0, height: 20));

            Add(commandInput);
            Add(new SimpleControl()
                .Anchor(a => a.Bottom(margin: 20)));
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            base.KeyHit(eventArgs);

            if (eventArgs.Handled) return;

            eventArgs.Handled = tryHandleKeyHit(eventArgs);
        }

        private bool tryHandleKeyHit(KeyEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                case Key.Enter:
                    debug.OnCommandExecuted(commandInput.Text);
                    commandInput.Clear();
                    break;
                case Key.Tab:
                    commandInput.Text = debug.AutoCompleteCommand(commandInput.Text);
                    break;
                case Key.Up:
                    commandInput.Text = debug.GetPreviousCommandInHistory(commandInput.Text);
                    break;
                case Key.Down:
                    commandInput.Text = debug.GetNextCommandInHistory(commandInput.Text);
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
