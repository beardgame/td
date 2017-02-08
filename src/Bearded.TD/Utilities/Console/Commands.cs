using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Console
{
    internal class Commands
    {
        [Command("help", "allAvailableCommands")]
        private static void help(Logger logger, CommandParameters p)
        {
            if (p.Args.Length > 0)
            {
                logger.Warning.Log("Sorry, I can't tell you anything about specific commands yet. :(");
                return;
            }

            var allCommands = dictionary.Keys;
            logger.Info.Log("Available commands:");
            foreach (var command in allCommands)
            {
                logger.Info.Log(command);
            }
        }

        private class Command
        {
            private readonly Action<Logger, CommandParameters> action;

            public CommandAttribute Attribute { get; }

            public Command(Action<Logger, CommandParameters> action, CommandAttribute attribute)
            {
                this.action = action;
                Attribute = attribute;
            }

            public void Run(Logger logger, CommandParameters args)
            {
                action(logger, args);
            }
        }

        private static Dictionary<string, Command> dictionary;

        private static readonly Dictionary<string, PrefixTrie> parameterCompletion = new Dictionary<string, PrefixTrie>();

        public static PrefixTrie Prefixes { get; private set; }

        public static bool TryRun(string command, Logger logger, CommandParameters parameters)
        {
            Command c;
            if (!dictionary.TryGetValue(command, out c)) return false;
            c.Run(logger, parameters);
            return true;
        }

        public static PrefixTrie ParameterPrefixesFor(string command)
        {
            Command c;
            if (!dictionary.TryGetValue(command, out c))
                return null;

            if (c.Attribute.ParameterCompletion == null)
                return null;

            PrefixTrie prefixes;
            return parameterCompletion.TryGetValue(c.Attribute.ParameterCompletion, out prefixes)
                ? prefixes
                : null;
        }

        public static void AddParameterCompletion(string parameterId, IEnumerable<string> prefixes)
        {
            lock(parameterCompletion)
                parameterCompletion.Add(parameterId, new PrefixTrie(prefixes));
        }

        public static void Initialise()
        {
            if (Prefixes != null)
                throw new Exception("Do not initialise more than once!");

            var commands = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Where(m => m.GetCustomAttributes(typeof (CommandAttribute), false).Any())
                .Select(m => new
                {
                    Attribute = (CommandAttribute)m.GetCustomAttributes(typeof (CommandAttribute), false).First(),
                    Action = (Action<Logger, CommandParameters>)Delegate.CreateDelegate(typeof(Action<Logger, CommandParameters>), m)
                })
#if !DEBUG
                .Where(c => !(c.Attribute is DebugCommandAttribute))
#endif
                .ToList();

            dictionary = new Dictionary<string, Command>(commands.Count);
            dictionary.AddRange(commands.Select(
                c => new KeyValuePair<string, Command>(c.Attribute.Name, new Command(c.Action, c.Attribute))));

            Prefixes = new PrefixTrie(commands.Select(c => c.Attribute.Name));

            AddParameterCompletion("allAvailableCommands", commands.Select(c => c.Attribute.Name));
        }

    }
}
