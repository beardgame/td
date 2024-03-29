﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bearded.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Newtonsoft.Json;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls;

sealed class DebugConsole : UpdateableNavigationNode<Void>
{
    private static readonly char[] space = [' '];

    private List<string> commandHistory = [];
    private readonly ConcurrentQueue<Logger.Entry> loggerEntriesAdded = new();
    private int commandHistoryIndex = -1;

    private Logger logger = null!;

    public bool IsEnabled { get; private set; }

    public VoidEventHandler? Enabled;
    public VoidEventHandler? Disabled;
    public GenericEventHandler<Logger.Entry>? LogEntryAdded;

    protected override void Initialize(DependencyResolver dependencies, Void parameters)
    {
        base.Initialize(dependencies, parameters);

        logger = dependencies.Resolve<Logger>();
        IsEnabled = false;

        logger.Logged += fireLoggerEntryEvent;

        tryLoadCommandHistory();
    }

    public IEnumerable<Logger.Entry> GetLastLogEntries(int maxNumOfEntries)
    {
        var entries = logger.GetSafeRecentEntries();
        return entries.Count <= maxNumOfEntries ? entries : entries.Skip(entries.Count - maxNumOfEntries);
    }

    public override void Terminate()
    {
        base.Terminate();
        logger.Logged -= fireLoggerEntryEvent;
    }

    private static readonly char[] newLineCharacters = ['\r', '\n'];

    public override void Update(UpdateEventArgs args)
    {
        while (loggerEntriesAdded.TryDequeue(out var entry))
        {
            var lines = entry.Text.Split(newLineCharacters, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                LogEntryAdded?.Invoke(new Logger.Entry(line, entry.Severity, entry.Time));
            }
        }
    }

    public void Enable()
    {
        IsEnabled = true;
        Enabled?.Invoke();
    }

    public void Disable()
    {
        IsEnabled = false;
        Disabled?.Invoke();
    }

    public void Toggle()
    {
        if (IsEnabled)
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }

    private void printInfo(string message)
    {
        fireLoggerEntryEvent(Logger.Severity.Info, message);
    }

    private void printWarning(string message)
    {
        fireLoggerEntryEvent(Logger.Severity.Warning, message);
    }

    private void printError(string message)
    {
        fireLoggerEntryEvent(Logger.Severity.Error, message);
    }

    private void fireLoggerEntryEvent(Logger.Severity severity, string message)
    {
        fireLoggerEntryEvent(new Logger.Entry(message, severity));
    }

    private void fireLoggerEntryEvent(Logger.Entry loggerEntry)
    {
        if (loggerEntry.Severity == Logger.Severity.Trace && !UserSettings.Instance.Debug.ShowTraceMessages)
        {
            return;
        }

        loggerEntriesAdded.Enqueue(loggerEntry);
    }

    public void OnCommandExecuted(string command)
    {
        addToHistory(command);

        printInfo($"> {command}");

        var split = command.Split(space, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0) return;
        var args = split.Skip(1).ToImmutableArray();

        if (!ConsoleCommands.TryRun(split[0], logger, new CommandParameters(args)))
        {
            printError("Command not found.");
        }
    }

    private void addToHistory(string command)
    {
        // Don't add double commands.
        if (commandHistory.Count > 0 && commandHistory[^1] == command) return;
        commandHistory.Add(command);
        commandHistoryIndex = -1;

        if (commandHistory.Count >= 200)
            commandHistory.RemoveRange(0, 100);

        trySaveCommandHistory();
    }

    private void trySaveCommandHistory()
    {
        var itemsInHistory = commandHistory.Count;
        var itemsToSave = Math.Min(itemsInHistory, 100);
        var defensiveCopy = commandHistory.GetRange(commandHistory.Count - itemsToSave, itemsToSave);
        Task.Run(() =>
        {
            try
            {
                var json = JsonConvert.SerializeObject(defensiveCopy);
                File.WriteAllText(Constants.Paths.CommandHistoryFile, json);
            }
            catch (Exception e)
            {
                logger.Warning?.Log($"Failed to save command history: {e.Message}");
            }
        });
    }

    private void tryLoadCommandHistory()
    {
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists(Constants.Paths.CommandHistoryFile))
                    return;
                var json = File.ReadAllText(Constants.Paths.CommandHistoryFile);
                var history = JsonConvert.DeserializeObject<List<string>>(json);
                if (history != null)
                    commandHistory = history;
            }
            catch (Exception e)
            {
                logger.Warning?.Log($"Failed to load command history: {e.Message}");
            }
        });
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

    public string AutoCompleteCommand(string incompleteCommand, bool printAlternatives)
    {
        var trimmed = incompleteCommand.TrimStart();

        if (incompleteCommand.Contains(" ")) return autoCompleteParameters(incompleteCommand, printAlternatives);

        var extended = ConsoleCommands.Prefixes.ExtendPrefix(trimmed);

        if (extended == null)
        {
            if (printAlternatives)
            {
                printInfo($"> {trimmed}");
                printWarning("No commands found.");
            }
            return trimmed;
        }

        if (ConsoleCommands.Prefixes.Contains(extended))
        {
            var extendedWithSpace = extended + " ";
            return trimmed != extended
                ? extendedWithSpace
                : autoCompleteParameters(extendedWithSpace, printAlternatives);
        }

        if (extended == trimmed && printAlternatives)
        {
            var availableCommands = ConsoleCommands.Prefixes.AllKeys(extended);
            printInfo($"> {trimmed}");
            foreach (var command in availableCommands) printInfo(command);
        }

        return extended;
    }

    private string autoCompleteParameters(string incompleteCommand, bool printAlternatives)
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
            if (printAlternatives)
            {
                printInfo($"> {incompleteCommand}");
                printWarning("No parameters for command known.");
            }
            return incompleteCommand;
        }

        var parameter = splitBySpace.Length > 1 ? splitBySpace[1] : "";
        var extended = parameterPrefixes.ExtendPrefix(parameter);

        if (extended == null)
        {
            if (printAlternatives)
            {
                printInfo($"> {incompleteCommand}");
                printWarning("No matching parameters found.");
            }
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

        if (printAlternatives)
        {
            var availableParameters = parameterPrefixes.AllKeys(extended);
            printInfo($"> {command} {extended}");
            foreach (var p in availableParameters) printInfo(p);
        }

        return $"{command} {extended}";
    }
}
