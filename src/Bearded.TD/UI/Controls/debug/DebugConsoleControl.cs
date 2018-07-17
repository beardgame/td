using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities.IO;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class DebugConsoleControl : CompositeControl
    {
        private const int logHistoryLength = 100;
        private static readonly Dictionary<Logger.Severity, Color> colorBySeverity =
            new Dictionary<Logger.Severity, Color>
            {

                {Logger.Severity.Fatal, Color.DeepPink},
                {Logger.Severity.Error, Color.Red},
                {Logger.Severity.Warning, Color.Yellow},
                {Logger.Severity.Info, Color.White},
                {Logger.Severity.Debug, Color.SpringGreen},
                {Logger.Severity.Trace, Color.SkyBlue},
            };

        private readonly DebugConsole debug;
        private readonly RotatingListItemSource listItemSource;
        private readonly TextInput commandInput;

        public DebugConsoleControl(DebugConsole debug)
        {
            this.debug = debug;
            commandInput = new AutoCompletingTextInput(str => debug.AutoCompleteCommand(str, false)) { FontSize = 16 }
                .Anchor(a => a.Bottom(margin: 0, height: 20));
            var logBox = new ListControl(startStuckToBottom: true)
                .Anchor(a => a.Bottom(margin: 20));
            listItemSource = new RotatingListItemSource(
                logBox, debug.GetLastLogEntries(logHistoryLength / 2).Select(getControlForEntry), logHistoryLength, 20);
            logBox.ItemSource = listItemSource;

            Add(new BackgroundBox());
            Add(logBox);
            Add(commandInput);

            debug.Enabled += onDebugEnabled;
            debug.Disabled += onDebugDisabled;
            debug.LogEntryAdded += onDebugLogEntryAdded;

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

        private void onDebugLogEntryAdded(Logger.Entry entry)
        {
            listItemSource.Push(getControlForEntry(entry));
        }

        private static Control getControlForEntry(Logger.Entry entry)
        {
            return new Label
            {
                Text = entry.Text,
                Color = colorBySeverity[entry.Severity],
                FontSize = 16,
                TextAnchor = .5 * Vector2d.UnitY
            };
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
                    commandInput.Text = debug.AutoCompleteCommand(commandInput.Text, true);
                    commandInput.MoveCursorToEnd();
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
