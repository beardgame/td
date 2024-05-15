using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Bearded.Graphics.Color;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Console;
using static Bearded.Utilities.IO.Logger.Severity;

namespace Bearded.TD.UI.Controls;

sealed class DebugConsoleControl : ViewportClippingLayerControl
{
    private const int logHistoryLength = 100;

    private static readonly IReadOnlyDictionary<Logger.Severity, Color> colorBySeverity =
        new Dictionary<Logger.Severity, Color>
        {
            { Fatal, DeepPink },
            { Error, Red },
            { Warning, Yellow },
            { Info, White },
            { Debug, SpringGreen },
            { Trace, SkyBlue },
        }.AsReadOnly();

    private readonly DebugConsole debug;
    private readonly UIContext uiContext;
    private readonly ListControl logBox = new(new ViewportClippingLayerControl("Debug Console Log"), startStuckToBottom: true);
    private readonly RotatingListItemSource<Logger.Entry> listItemSource;
    private readonly TextInput commandInput;

    public DebugConsoleControl(DebugConsole debug, UIContext uiContext) : base("Debug Console")
    {
        this.debug = debug;
        this.uiContext = uiContext;

        listItemSource = new RotatingListItemSource<Logger.Entry>(
            logBox, debug.GetLastLogEntries(logHistoryLength / 2), getControlForEntry,
            LogEntryHeight,
            logHistoryLength);
        logBox.ItemSource = listItemSource;

        commandInput = new AutoCompletingTextInput(str => debug.AutoCompleteCommand(str, false))
            { FontSize = FontSize, TextStyle = Font };

        this.Add([
            new ComplexBox
            {
                Components = [
                    Fill.With(Colors.Get(BackgroundColor.Default) * 0.9f),
                    Glow.Outer(Menu.ShadowWidth, Menu.ShadowColor),
                ],
            }.Anchor(a => a.Bottom(Menu.ShadowWidth + 1)),
            logBox.Anchor(a => a.Bottom(margin: Menu.ShadowWidth + InputHeight)),
            commandInput.Anchor(a => a.Bottom(margin: Menu.ShadowWidth, height: InputHeight)),
        ]);

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

    private Control getControlForEntry(Logger.Entry entry)
    {
        var color = colorBySeverity[entry.Severity];

        var label = new Label
        {
            Text = entry.Text,
            Color = color,
            FontSize = FontSize,
            TextAnchor = .5 * Vector2d.UnitY,
            TextStyle = Font,
        };

        var timeSinceEntry = DateTime.Now - entry.Time;
        if (timeSinceEntry.TotalSeconds > NewEntryBackgroundAnimationDuration.NumericValue)
            return label;

        var background = new BackgroundBox();
        uiContext.Animations.Start(NewEntryBackgroundAnimation, (background, color));
        return new CompositeControl { background, label };
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
