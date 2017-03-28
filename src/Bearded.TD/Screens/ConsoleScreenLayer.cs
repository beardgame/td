using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class ConsoleScreenLayer : UIScreenLayer
    {
        private const float consoleHeight = 320;
        private const float inputBoxHeight = 20;
        private const float padding = 6;

        private readonly Logger logger;
        private bool isConsoleEnabled;

        private readonly Bounds bounds;
        private readonly TextInput consoleInput;

        private readonly List<string> commandHistory = new List<string>();
        private int commandHistoryIndex = -1;

        public ConsoleScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, Logger logger) : base(parent, geometries, 0, 1, true)
        {
            this.logger = logger;

            bounds = new Bounds(new ScalingDimension(Screen.X), new FixedSizeDimension(Screen.Y, consoleHeight));
            AddComponent(new ConsoleTextComponent(Bounds.Within(bounds, padding, padding, padding + inputBoxHeight, padding), logger));
            AddComponent(consoleInput = new TextInput(Bounds.Within(bounds, consoleHeight - inputBoxHeight, padding, 0, padding)));
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (InputManager.IsKeyHit(Key.Tilde))
                isConsoleEnabled = !isConsoleEnabled;

            if (!isConsoleEnabled) return true;

            base.HandleInput(args, inputState);

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
            Geometries.ConsoleBackground.DrawRectangle(bounds.XStart, bounds.YStart, bounds.Width, bounds.Height);

            base.Draw();
        }
    }
}
