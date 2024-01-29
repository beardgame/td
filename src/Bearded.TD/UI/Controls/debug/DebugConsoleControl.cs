using System.Collections.Frozen;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class DebugConsoleControl : ViewportClippingLayerControl
{
    private const int logHistoryLength = 100;
    private static readonly IReadOnlyDictionary<Logger.Severity, Color> colorBySeverity =
        new Dictionary<Logger.Severity, Color>()
        {

            {Logger.Severity.Fatal, Color.DeepPink},
            {Logger.Severity.Error, Color.Red},
            {Logger.Severity.Warning, Color.Yellow},
            {Logger.Severity.Info, Color.White},
            {Logger.Severity.Debug, Color.SpringGreen},
            {Logger.Severity.Trace, Color.SkyBlue},
        }.ToFrozenDictionary();

    private readonly DebugConsole debug;
    private readonly ListControl logBox;
    private readonly RotatingListItemSource<Logger.Entry> listItemSource;
    private readonly TextInput commandInput;

    public DebugConsoleControl(DebugConsole debug)
    {
        this.debug = debug;
        commandInput = new AutoCompletingTextInput(str => debug.AutoCompleteCommand(str, false)) { FontSize = 16 }
            .Anchor(a => a.Bottom(margin: 0, height: 20));
        logBox = new ListControl(new ViewportClippingLayerControl(), startStuckToBottom: true)
            .Anchor(a => a.Bottom(margin: 20));
        listItemSource = new RotatingListItemSource<Logger.Entry>(
            logBox, debug.GetLastLogEntries(logHistoryLength / 2), getControlForEntry, 20, logHistoryLength);
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
        logBox.Reload();
    }

    private void onDebugDisabled()
    {
        commandInput.Unfocus();
        IsVisible = false;
    }

    private void onDebugLogEntryAdded(Logger.Entry entry)
    {
        listItemSource.Push(entry);
    }

    private static Control getControlForEntry(Logger.Entry entry)
    {
        return new Label
        {
            Text = entry.Text,
            Color = colorBySeverity[entry.Severity],
            FontSize = 16,
            TextAnchor = .5 * Vector2d.UnitY,
            TextStyle = TextStyle.Monospace
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
            case Keys.Enter:
                debug.OnCommandExecuted(commandInput.Text);
                commandInput.Clear();
                break;
            case Keys.Tab:
                commandInput.Text = debug.AutoCompleteCommand(commandInput.Text, true);
                commandInput.MoveCursorToEnd();
                break;
            case Keys.Up:
                commandInput.Text = debug.GetPreviousCommandInHistory(commandInput.Text);
                commandInput.MoveCursorToEnd();
                break;
            case Keys.Down:
                commandInput.Text = debug.GetNextCommandInHistory(commandInput.Text);
                commandInput.MoveCursorToEnd();
                break;
            default:
                return false;
        }

        return true;
    }
}
