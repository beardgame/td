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
        private readonly RotatingListItemSource listItemSource;

        public DebugConsoleControl(DebugConsole debug)
        {
            this.debug = debug;
            commandInput = new AutoCompletingTextInput(debug.AutoCompleteCommand) { FontSize = 16 }
                .Anchor(a => a.Bottom(margin: 0, height: 20));

            Add(new BackgroundBox());

            Add(commandInput);
            Add(new ListControl(listItemSource)
                .Anchor(a => a.Bottom(margin: 20)));

            debug.Enabled += onDebugEnabled;
            debug.Disabled += onDebugDisabled;
            if (debug.IsEnabled)
            {
                onDebugEnabled();
            }
            else
            {
                onDebugDisabled();
            }
        }

        private void onDebugEnabled()
        {
            commandInput.Focus();
            IsVisible = true;
        }

        private void onDebugDisabled()
        {
            commandInput.Unfocus();
            IsVisible = false;
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
                case Key.Up:
                    commandInput.Text = debug.GetPreviousCommandInHistory(commandInput.Text);
                    commandInput.MoveCursorToEnd();
                    break;
                case Key.Down:
                    commandInput.Text = debug.GetNextCommandInHistory(commandInput.Text);
                    commandInput.MoveCursorToEnd();
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
