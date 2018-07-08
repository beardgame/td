using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class DebugConsole : NavigationNode<Void>
    {
        private static readonly char[] space = {' '};

        private readonly List<string> commandHistory = new List<string>();
        private int commandHistoryIndex = -1;

        private bool isEnabled;
        private Logger logger;

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            logger = dependencies.Resolve<Logger>();
        }

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        public void Toggle()
        {
            isEnabled = !isEnabled;
        }

        public void OnCommandExecuted(string command)
        {
            addToHistory(command);
            
            logger.Info.Log("> {0}", command);

            var split = command.Split(space, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0) return;
            var args = split.Skip(1).ToArray();

            if (!ConsoleCommands.TryRun(split[0], logger, new CommandParameters(args)))
            {
                logger.Error.Log("Command not found.");
            }
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

        public string GetPreviousCommandInHistory(string currentCommand)
        {
            if (commandHistory.Count == 0 || commandHistoryIndex == 0) return currentCommand;
            
            if (commandHistoryIndex == -1)
            {
                commandHistory.Add(currentCommand);
                commandHistoryIndex = commandHistory.Count - 2;
            }
            else
            {
                commandHistoryIndex--;
            }

            return commandHistory[commandHistoryIndex];
        }

        public string GetNextCommandInHistory(string currentCommand)
        {
            if (commandHistoryIndex == -1) return currentCommand;

            commandHistoryIndex++;
            var cmd = commandHistory[commandHistoryIndex];

            if (commandHistoryIndex == commandHistoryIndex - 1)
            {
                commandHistory.RemoveAt(commandHistoryIndex);
                commandHistoryIndex = -1;
            }

            return cmd;
        }

        public string AutoCompleteCommand(string incompleteCommand)
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
                return trimmed != extended
                    ? extendedWithSpace
                    : autoCompleteParameters(extendedWithSpace);
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
            var splitBySpace = incompleteCommand.Split(space, StringSplitOptions.RemoveEmptyEntries);

            if (splitBySpace.Length == 0 || splitBySpace.Length > 2)
            {
                return incompleteCommand;
            }

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
            {
                return $"{command} {extended} ";
            }

            if (extended != parameter)
            {
                return $"{command} {extended}";
            }

            var availableParameters = parameterPrefixes.AllKeys(extended);
            logger.Info.Log($"> {command} {extended}");
            foreach (var p in availableParameters) logger.Info.Log(p);

            return $"{command} {extended}";
        }
    }
}
