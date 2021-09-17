using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bearded.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Utilities.Console
{
    static class ConsoleCommands
    {
        [Command("help", "allAvailableCommands")]
        private static void help(Logger logger, CommandParameters p)
        {
            if (p.Args.Length > 0)
            {
                logger.Warning?.Log("Sorry, I can't tell you anything about specific commands yet. :(");
                return;
            }

            var allCommands = dictionary.Keys;
            logger.Info?.Log("Available commands:");
            foreach (var command in allCommands)
            {
                logger.Info?.Log(command);
            }
        }

        private sealed record Command(Action<Logger, CommandParameters> Run, CommandAttribute Attribute);

        private static readonly Dictionary<string, Command> dictionary = new();
        private static readonly Dictionary<string, PrefixTrie> parameterCompletion = new();

        private static PrefixTrie? prefixes;
        public static PrefixTrie Prefixes => prefixes!;

        public static bool TryRun(string command, Logger logger, CommandParameters parameters)
        {
            if (!dictionary.TryGetValue(command, out var c))
                return false;
            c.Run(logger, parameters);
            return true;
        }

        public static PrefixTrie? ParameterPrefixesFor(string command)
        {
            if (!dictionary.TryGetValue(command, out var c))
                return null;

            if (c.Attribute.ParameterCompletion == null)
                return null;

            lock (parameterCompletion)
            {
                return parameterCompletion.TryGetValue(c.Attribute.ParameterCompletion, out var prefixTrie)
                    ? prefixTrie
                    : null;
            }
        }

        public static void AddParameterCompletion(string parameterId, IEnumerable<string> prefixes)
        {
            lock(parameterCompletion)
                parameterCompletion.Add(parameterId, new PrefixTrie(prefixes));
        }

        public static void Initialize()
        {
            if (prefixes != null)
                throw new Exception("Do not initialise more than once!");

            initializeCommands();

            initializeParameterCompletions();
        }

        private static void initializeCommands()
        {
            var commands = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Any())
                .Select(m => (
                    Attribute: (CommandAttribute) m.GetCustomAttributes(typeof(CommandAttribute), false).First(),
                    Action: (Action<Logger, CommandParameters>) Delegate.CreateDelegate(
                        typeof(Action<Logger, CommandParameters>), m)
                ))
#if !DEBUG
                .Where(c => !(c.Attribute is DebugCommandAttribute))
#endif
                .ToList();

            dictionary.AddRange(commands.Select(
                c => new KeyValuePair<string, Command>(c.Attribute.Name, new Command(c.Action, c.Attribute))));

            prefixes = new PrefixTrie(commands.Select(c => c.Attribute.Name));

            AddParameterCompletion("allAvailableCommands", commands.Select(c => c.Attribute.Name));
        }

        private static void initializeParameterCompletions()
        {
            var parameterCompletions = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Where(m => m.GetCustomAttributes(typeof(CommandParameterCompletionAttribute), false).Any())
                .Select(m => (
                    ((CommandParameterCompletionAttribute)m.GetCustomAttributes(typeof(CommandParameterCompletionAttribute), false).First()).Name,
                    Parameters: (IEnumerable<string>)m.Invoke(null, null)
                    ));

            foreach (var (name, parameters) in parameterCompletions)
            {
                AddParameterCompletion(name, parameters);
            }
        }
    }
}
