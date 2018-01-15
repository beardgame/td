using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.IO;
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

        public ConsoleScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, Logger logger)
            : base(parent, geometries)
        {
            this.logger = logger;

            bounds = new Bounds(new ScalingDimension(Screen.X), new FixedSizeDimension(Screen.Y, consoleHeight));
            consoleInput = new TextInput(Bounds.Within(bounds, consoleHeight - inputBoxHeight, padding, 0, padding));
            AddComponent(
                new ConsoleTextBox(Bounds.Within(bounds, padding, padding, padding + inputBoxHeight, padding), logger));
            AddComponent(consoleInput);
            consoleInput.Submitted += execute;
        }

        protected override bool DoHandleInput(InputContext input)
        {
            if (input.State.ForKey(Key.Tilde).Hit)
            {
                isConsoleEnabled = !isConsoleEnabled;
                if (isConsoleEnabled)
                    consoleInput.Focus();
                else
                    consoleInput.Unfocus();
            }

            if (!isConsoleEnabled) return true;

            base.DoHandleInput(input);
            
            if (input.State.ForKey(Key.Tab).Hit)
                consoleInput.Text = autoComplete(consoleInput.Text);
            if (input.State.ForKey(Key.Up).Hit && commandHistory.Count > 0 && commandHistoryIndex != 0)
            {
                if (commandHistoryIndex == -1)
                    setCommandHistoryIndex(commandHistory.Count - 1);
                else
                    setCommandHistoryIndex(commandHistoryIndex - 1);
            }
            if (input.State.ForKey(Key.Down).Hit && commandHistoryIndex != -1)
                setCommandHistoryIndex(commandHistoryIndex + 1);

            return false;
        }

        private void execute(string command)
        {
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
            var trimmed = incompleteCommand.TrimStart();

            if (incompleteCommand.Contains(" ")) return autoCompleteParameters(incompleteCommand);
            
            var extended = ConsoleCommands.Prefixes.ExtendPrefix(trimmed);

            if (extended == null)
            {
                logger.Info.Log($"> {trimmed}");
                logger.Warning.Log("No commands found.");
                return trimmed;
            }

            if (ConsoleCommands.Prefixes.Contains(extended))
            {
                var extendedWithSpace = extended + " ";
                if (trimmed != extended)
                    return extendedWithSpace;
                else
                    return autoCompleteParameters(extendedWithSpace);
            }

            if (extended == trimmed)
            {
                var availableCommands = ConsoleCommands.Prefixes.AllKeys(extended);
                logger.Info.Log("> {0}", trimmed);
                foreach (var command in availableCommands) logger.Info.Log(command);
            }

            return extended;
        }

        private static readonly char[] space = {' '};

        private string autoCompleteParameters(string incompleteCommand)
        {
            var splitBySpace = incompleteCommand.Split(space, StringSplitOptions.RemoveEmptyEntries);

            if (splitBySpace.Length == 0 || splitBySpace.Length > 2)
                return incompleteCommand;

            var command = splitBySpace[0];
            var parameterPrefixes = ConsoleCommands.ParameterPrefixesFor(command);

            if (parameterPrefixes == null)
            {
                logger.Info.Log($"> {incompleteCommand}");
                logger.Warning.Log("No parameters for command known.");
                return incompleteCommand;
            }

            var parameter = splitBySpace.Length > 1 ? splitBySpace[1] : "";
            var extended = parameterPrefixes.ExtendPrefix(parameter);

            if (extended == null)
            {
                logger.Info.Log($"> {incompleteCommand}");
                logger.Warning.Log("No matching parameters found.");
                return incompleteCommand;
            }

            if (parameterPrefixes.Contains(extended))
                return $"{command} {extended} ";

            if (extended != parameter)
                return $"{command} {extended}";
            
            var availableParameters = parameterPrefixes.AllKeys(extended);
            logger.Info.Log($"> {command} {extended}");
            foreach (var p in availableParameters) logger.Info.Log(p);

            return $"{command} {extended}";
        }

        public override void Draw()
        {
            if (!isConsoleEnabled) return;

            Geometries.ConsoleBackground.Color = Color.Black * 0.7f;
            Geometries.ConsoleBackground.DrawRectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height);

            base.Draw();
        }
    }
}
