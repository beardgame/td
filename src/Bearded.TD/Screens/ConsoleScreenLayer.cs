using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class ConsoleScreenLayer : UIScreenLayer
    {
        private const float consoleHeight = 320;
        private const float inputBoxHeight = 20;
        private const float padding = 6;

        private static readonly Dictionary<Logger.Severity, Color> colors = new Dictionary<Logger.Severity, Color>
        {
            { Logger.Severity.Fatal, Color.DeepPink },
            { Logger.Severity.Error, Color.Red },
            { Logger.Severity.Warning, Color.Yellow },
            { Logger.Severity.Info, Color.White },
            { Logger.Severity.Debug, Color.SpringGreen },
            { Logger.Severity.Trace, Color.SkyBlue },
        };

        private readonly Logger logger;
        private bool isConsoleEnabled;

        private readonly Canvas canvas;
        private readonly ConsoleTextComponent consoleText;
        private readonly TextInput consoleInput;

        private readonly List<string> commandHistory = new List<string>();
        private int commandHistoryIndex = -1;

        public ConsoleScreenLayer(Logger logger, GeometryManager geometries) : base(geometries, 0, 1, true)
        {
            this.logger = logger;

            canvas = new Canvas(new ScalingDimension(Screen.X), new FixedSizeDimension(Screen.Y, consoleHeight));
            consoleText = new ConsoleTextComponent(Canvas.Within(canvas, padding, padding, padding + inputBoxHeight, padding));
            consoleInput = new TextInput(Canvas.Within(canvas, consoleHeight - inputBoxHeight, padding, 0, padding));
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (InputManager.IsKeyHit(Key.Tilde))
                isConsoleEnabled = !isConsoleEnabled;

            if (!isConsoleEnabled) return true;

            consoleInput.HandleInput(inputState);

            if (InputManager.IsKeyHit(Key.Enter))
                execute();
            if (InputManager.IsKeyHit(Key.Tab))
                consoleInput.Text = autoComplete(consoleInput.Text);
            if (InputManager.IsKeyHit(Key.Up) && commandHistory.Count > 0 && commandHistoryIndex != 0)
            {
                if (commandHistoryIndex == -1)
                    setCommandHistoryIndex(commandHistory.Count - 1);
                else
                    setCommandHistoryIndex(commandHistoryIndex - 1);
            }
            if (InputManager.IsKeyHit(Key.Down) && commandHistoryIndex != -1)
                setCommandHistoryIndex(commandHistoryIndex + 1);

            return false;
        }

        public override void Update(UpdateEventArgs args) { }

        private void execute()
        {
            var command = consoleInput.Text.Trim();

            addToHistory(command);

            logger.Info.Log("> {0}", command);

            var split = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return;
            var args = split.Skip(1).ToArray();

            if (!ConsoleCommands.TryRun(split[0], logger, new CommandParameters(args)))
            {
                logger.Error.Log("Command not found.");
            }

            consoleInput.Text = "";
        }

        private void addToHistory(string command)
        {
            // Don't add double commands.
            if (commandHistory.Count > 0 && commandHistory[commandHistory.Count - 1] == command) return;
            commandHistory.Add(command);
            commandHistoryIndex = -1;

            if (commandHistory.Count >= 200)
                commandHistory.RemoveRange(0, 100);
        }

        private void setCommandHistoryIndex(int i)
        {
            if (commandHistoryIndex == -1)
                commandHistory.Add(consoleInput.Text);
                
            commandHistoryIndex = i;
            consoleInput.Text = commandHistory[i];

            if (commandHistoryIndex == commandHistory.Count - 1)
            {
                commandHistory.RemoveAt(commandHistoryIndex);
                commandHistoryIndex = -1;
            }
        }

        private string autoComplete(string incompleteCommand)
        {
            var trimmed = incompleteCommand.Trim();
            if (trimmed.Contains(" ")) return autoCompleteParameters(incompleteCommand);

            var extended = ConsoleCommands.Prefixes.ExtendPrefix(trimmed);

            if (extended == null)
            {
                logger.Info.Log("> {0}", trimmed);
                logger.Warning.Log("No commands found.");
                return trimmed;
            }

            if (ConsoleCommands.Prefixes.Contains(extended))
            {
                if (trimmed != extended)
                    return extended + " ";
                else
                    return autoCompleteParameters(incompleteCommand);
            }

            if (extended == trimmed)
            {
                var availableCommands = ConsoleCommands.Prefixes.AllKeys(extended);
                logger.Info.Log("> {0}", trimmed);
                foreach (var command in availableCommands) logger.Info.Log(command);
            }

            return extended;
        }

        private string autoCompleteParameters(string incompleteCommand)
        {
            logger.Warning.Log("Autocompletion of parameters has not been implemented yet.");
            return incompleteCommand;
        }

        public override void Draw()
        {
            if (!isConsoleEnabled) return;

            Geometries.ConsoleBackground.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            Geometries.ConsoleBackground.DrawRectangle(canvas.XStart, canvas.YStart, canvas.Width, canvas.Height);

            consoleText.Draw(Geometries, logger.GetSafeRecentEntries());
            consoleInput.Draw(Geometries);
        }

        private class ConsoleTextComponent
        {
            private const float fontSize = 14;
            private const float lineHeight = 16;
            
#if DEBUG
            private static readonly HashSet<Logger.Severity> visibleSeverities = new HashSet<Logger.Severity>
            {
                Logger.Severity.Fatal, Logger.Severity.Error, Logger.Severity.Warning,
                Logger.Severity.Info, Logger.Severity.Debug, Logger.Severity.Trace
            };
#else
            private static readonly HashSet<Logger.Severity> visibleSeverities = new HashSet<Logger.Severity>
            {
                Logger.Severity.Fatal, Logger.Severity.Error, Logger.Severity.Warning, Logger.Severity.Info
            };
#endif

            private readonly Canvas canvas;

            public ConsoleTextComponent(Canvas canvas)
            {
                this.canvas = canvas;
            }

            public void Draw(GeometryManager geometries, IReadOnlyList<Logger.Entry> logEntries)
            {
                geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
                geometries.ConsoleFont.Height = fontSize;

                var y = canvas.YEnd - lineHeight;
                var i = logEntries.Count;

                while (y >= -lineHeight && i > 0)
                {
                    var entry = logEntries[--i];
                    if (!visibleSeverities.Contains(entry.Severity)) continue;
                    geometries.ConsoleFont.Color = colors[logEntries[i].Severity];
                    geometries.ConsoleFont.DrawString(new Vector2(padding, y), logEntries[i].Text);
                    y -= lineHeight;
                }
            }
        }
    }
}
